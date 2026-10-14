using System;
using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace IronVeil
{
    public enum InputDeviceKind
    {
        KeyboardMouse = 0,
        Gamepad = 1
    }

    /// <summary>
    /// The single place gameplay and UI read input from. It owns the actions asset,
    /// tracks which device the player is currently using, and resolves aim for both
    /// control schemes (mouse cursor on keyboard, right stick on a pad).
    /// </summary>
    public sealed class InputService : IDisposable
    {
        public const string GameplayMap = "Gameplay";
        public const string UiMap = "UI";
        public const string AssetResourcePath = "GameInput";

        private InputActionAsset _asset;
        private InputActionMap _gameplay;
        private InputActionMap _ui;

        private InputAction _move;
        private InputAction _aim;
        private InputAction _aimPoint;
        private InputAction _fire;
        private InputAction _reload;
        private InputAction _abilityPrimary;
        private InputAction _abilitySecondary;
        private InputAction _swapWeapon;
        private InputAction _nextWeapon;
        private InputAction _previousWeapon;
        private InputAction _useFlask;
        private InputAction _brace;
        private InputAction _interact;
        private InputAction _pause;

        private InputAction _navigate;
        private InputAction _submit;
        private InputAction _cancel;
        private InputAction _point;
        private InputAction _click;
        private InputAction _scrollWheel;
        private InputAction _tabLeft;
        private InputAction _tabRight;

        private float _uiRepeatTimer;
        private Vector2 _uiRepeatVector;

        public InputActionAsset Asset => _asset;
        public InputDeviceKind CurrentDevice { get; private set; } = InputDeviceKind.KeyboardMouse;

        /// <summary>Raised when the player switches between pad and keyboard/mouse.</summary>
        public event Action<InputDeviceKind> DeviceChanged;

        /// <summary>Raised whenever any bound control is used, for idle detection.</summary>
        public event Action AnyInput;

        public bool IsReady => _asset != null;

        public float LastGamepadActivity { get; private set; }
        public float LastKeyboardMouseActivity { get; private set; }

        public void Initialize()
        {
            _asset = Resources.Load<InputActionAsset>(AssetResourcePath);
            if (_asset == null)
            {
                Debug.LogError("[IronVeil] Resources/GameInput.inputactions is missing. " +
                               "Run tools/inputmap/generate_inputactions.py and reimport the project.");
                return;
            }

            _gameplay = _asset.FindActionMap(GameplayMap, false);
            _ui = _asset.FindActionMap(UiMap, false);
            if (_gameplay == null || _ui == null)
            {
                Debug.LogError("[IronVeil] Input asset is missing its Gameplay or UI action map.");
                return;
            }

            _move = Require(_gameplay, "Move");
            _aim = Require(_gameplay, "Aim");
            _aimPoint = Require(_gameplay, "AimPoint");
            _fire = Require(_gameplay, "Fire");
            _reload = Require(_gameplay, "Reload");
            _abilityPrimary = Require(_gameplay, "AbilityPrimary");
            _abilitySecondary = Require(_gameplay, "AbilitySecondary");
            _swapWeapon = Require(_gameplay, "SwapWeapon");
            _nextWeapon = Require(_gameplay, "NextWeapon");
            _previousWeapon = Require(_gameplay, "PreviousWeapon");
            _useFlask = Require(_gameplay, "UseFlask");
            _brace = Require(_gameplay, "Brace");
            _interact = Require(_gameplay, "Interact");
            _pause = Require(_gameplay, "Pause");

            _navigate = Require(_ui, "Navigate");
            _submit = Require(_ui, "Submit");
            _cancel = Require(_ui, "Cancel");
            _point = Require(_ui, "Point");
            _click = Require(_ui, "Click");
            _scrollWheel = Require(_ui, "ScrollWheel");
            _tabLeft = Require(_ui, "TabLeft");
            _tabRight = Require(_ui, "TabRight");

            LoadBindingOverrides();

            _asset.Enable();
            InputSystem.onActionChange += HandleActionChange;

            SetGameplayEnabled(false);
            SetUiEnabled(true);
        }

        private static InputAction Require(InputActionMap map, string name)
        {
            InputAction action = map.FindAction(name, false);
            if (action == null)
            {
                Debug.LogError("[IronVeil] Input asset action map '" + map.name + "' has no action '" + name + "'.");
            }
            return action;
        }

        public void Dispose()
        {
            InputSystem.onActionChange -= HandleActionChange;
            if (_asset != null) _asset.Disable();
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (_gameplay == null) return;
            if (enabled) _gameplay.Enable();
            else _gameplay.Disable();
        }

        public void SetUiEnabled(bool enabled)
        {
            if (_ui == null) return;
            if (enabled) _ui.Enable();
            else _ui.Disable();
        }

        /// <summary>Both maps active: used by the pause menu over live gameplay.</summary>
        public void SetGameplayAndUiEnabled(bool gameplay, bool ui)
        {
            SetGameplayEnabled(gameplay);
            SetUiEnabled(ui);
        }

        // ------------------------------------------------- device tracking

        private void HandleActionChange(object target, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed && change != InputActionChange.ActionStarted) return;
            if (!(target is InputAction action)) return;

            InputDevice device = action.activeControl?.device;
            if (device == null) return;

            MarkDevice(device);
            AnyInput?.Invoke();
        }

        private void MarkDevice(InputDevice device)
        {
            InputDeviceKind kind = device is Gamepad ? InputDeviceKind.Gamepad : InputDeviceKind.KeyboardMouse;
            float now = Time.unscaledTime;
            if (kind == InputDeviceKind.Gamepad) LastGamepadActivity = now;
            else LastKeyboardMouseActivity = now;

            if (kind == CurrentDevice) return;
            CurrentDevice = kind;
            DeviceChanged?.Invoke(kind);
        }

        /// <summary>
        /// Catches stick movement that never produces an action event (for example a
        /// small nudge while a menu is open) so the on-screen prompts stay correct.
        /// </summary>
        public void Poll()
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                bool active =
                    gamepad.leftStick.ReadValue().sqrMagnitude > 0.04f ||
                    gamepad.rightStick.ReadValue().sqrMagnitude > 0.04f ||
                    gamepad.leftTrigger.ReadValue() > 0.2f ||
                    gamepad.rightTrigger.ReadValue() > 0.2f ||
                    gamepad.dpad.ReadValue().sqrMagnitude > 0.04f ||
                    gamepad.buttonSouth.isPressed || gamepad.buttonEast.isPressed ||
                    gamepad.buttonNorth.isPressed || gamepad.buttonWest.isPressed ||
                    gamepad.startButton.isPressed || gamepad.selectButton.isPressed;

                if (active)
                {
                    MarkDevice(gamepad);
                    AnyInput?.Invoke();
                }
            }

            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;
            if (mouse != null && mouse.delta.ReadValue().sqrMagnitude > 9f) MarkDevice(mouse);
            if (keyboard != null && keyboard.anyKey.isPressed) MarkDevice(keyboard);
        }

        // ------------------------------------------------- gameplay values

        public Vector2 Move => _move?.ReadValue<Vector2>() ?? Vector2.zero;

        /// <summary>Raw right-stick value. Zero on keyboard, which aims by cursor.</summary>
        public Vector2 AimVector => _aim?.ReadValue<Vector2>() ?? Vector2.zero;

        public bool HasAimVector
        {
            get
            {
                if (_aim == null) return false;
                Vector2 value = _aim.ReadValue<Vector2>();
                float deadzone = Services.Get<SettingsService>()?.Settings.StickDeadzone ?? 0.18f;
                return value.sqrMagnitude >= deadzone * deadzone;
            }
        }

        public bool FireHeld => _fire != null && _fire.IsPressed();
        public bool FirePressed => _fire != null && _fire.WasPressedThisFrame();
        public bool FireReleased => _fire != null && _fire.WasReleasedThisFrame();
        public bool ReloadPressed => _reload != null && _reload.WasPressedThisFrame();
        public bool AbilityPrimaryPressed => _abilityPrimary != null && _abilityPrimary.WasPressedThisFrame();
        public bool AbilityPrimaryHeld => _abilityPrimary != null && _abilityPrimary.IsPressed();
        public bool AbilitySecondaryPressed => _abilitySecondary != null && _abilitySecondary.WasPressedThisFrame();
        public bool AbilitySecondaryHeld => _abilitySecondary != null && _abilitySecondary.IsPressed();
        public bool SwapWeaponPressed => _swapWeapon != null && _swapWeapon.WasPressedThisFrame();
        public bool NextWeaponPressed => _nextWeapon != null && _nextWeapon.WasPressedThisFrame();
        public bool PreviousWeaponPressed => _previousWeapon != null && _previousWeapon.WasPressedThisFrame();
        public bool UseFlaskPressed => _useFlask != null && _useFlask.WasPressedThisFrame();
        public bool BraceHeld => _brace != null && _brace.IsPressed();
        public bool InteractPressed => _interact != null && _interact.WasPressedThisFrame();
        public bool PausePressed => _pause != null && _pause.WasPressedThisFrame();

        // ------------------------------------------------- UI values

        public Vector2 Navigate => _navigate?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool SubmitPressed => _submit != null && _submit.WasPressedThisFrame();
        public bool CancelPressed => _cancel != null && _cancel.WasPressedThisFrame();
        public bool TabLeftPressed => _tabLeft != null && _tabLeft.WasPressedThisFrame();
        public bool TabRightPressed => _tabRight != null && _tabRight.WasPressedThisFrame();
        public Vector2 ScrollWheel => _scrollWheel?.ReadValue<Vector2>() ?? Vector2.zero;
        public Vector2 PointPosition => _point?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool ClickPressed => _click != null && _click.WasPressedThisFrame();

        /// <summary>
        /// Menu navigation with a repeat delay, so held sticks do not fly through a list.
        /// </summary>
        public Vector2 NavigateRepeat()
        {
            Vector2 value = Navigate;
            if (value.sqrMagnitude < 0.36f)
            {
                _uiRepeatTimer = 0f;
                _uiRepeatVector = Vector2.zero;
                return Vector2.zero;
            }

            bool sameDirection = Vector2.Dot(value.normalized, _uiRepeatVector) > 0.7f;
            if (!sameDirection)
            {
                _uiRepeatTimer = 0.28f;
                _uiRepeatVector = value.normalized;
                return value;
            }

            _uiRepeatTimer -= Time.unscaledDeltaTime;
            if (_uiRepeatTimer > 0f) return Vector2.zero;
            _uiRepeatTimer = 0.12f;
            return value;
        }

        public void ConsumeNavigateRepeat()
        {
            _uiRepeatTimer = 0.28f;
            _uiRepeatVector = Vector2.zero;
        }

        // ------------------------------------------------- aim resolution

        /// <summary>
        /// Resolves a world-space aim direction for the actor standing at originWorld.
        /// Mouse cursor wins on keyboard, right stick wins on a pad; Auto picks based
        /// on the device the player last touched.
        /// </summary>
        public Vector2 ResolveAimDirection(Vector3 originWorld, Camera camera, out bool valid)
        {
            GameSettings settings = Services.Get<SettingsService>()?.Settings;
            AimSource source = settings == null ? AimSource.Auto : (AimSource)settings.AimSource;

            bool useStick;
            switch (source)
            {
                case AimSource.RightStick: useStick = true; break;
                case AimSource.MouseCursor: useStick = false; break;
                default: useStick = CurrentDevice == InputDeviceKind.Gamepad && HasAimVector; break;
            }

            Vector2 direction = Vector2.zero;
            valid = false;

            if (useStick && HasAimVector)
            {
                direction = AimVector.normalized;
                valid = true;
            }
            else if (!useStick && camera != null && _aimPoint != null)
            {
                Vector2 screenPoint = _aimPoint.ReadValue<Vector2>();
                Vector3 world = camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, Mathf.Abs(camera.transform.position.z)));
                Vector3 delta = world - originWorld;
                delta.z = 0f;
                if (delta.sqrMagnitude > 0.0004f)
                {
                    direction = new Vector2(delta.x, delta.y).normalized;
                    valid = true;
                }
            }

            if (!valid)
            {
                // Fall back to the other scheme so aim is never completely lost.
                if (HasAimVector)
                {
                    direction = AimVector.normalized;
                    valid = true;
                }
                else
                {
                    direction = Vector2.right;
                    valid = false;
                }
            }
            else
            {
                float assist = settings == null ? 0f : settings.AimAssistStrength;
                bool snap = settings != null && settings.AimSnapToTargets;
                if (assist > 0.001f || snap)
                {
                    direction = AimAssist.Snap(direction, originWorld, assist, snap);
                }
            }

            return direction;
        }

        /// <summary>Screen-space cursor position, used for the aim reticle.</summary>
        public Vector2 CursorPosition()
        {
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
            return _point?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        public bool UsesCursorAim
        {
            get
            {
                GameSettings settings = Services.Get<SettingsService>()?.Settings;
                AimSource source = settings == null ? AimSource.Auto : (AimSource)settings.AimSource;
                if (source == AimSource.RightStick) return false;
                if (source == AimSource.MouseCursor) return true;
                return CurrentDevice == InputDeviceKind.KeyboardMouse;
            }
        }

        // ------------------------------------------------- prompts

        /// <summary>
        /// Human readable prompt for an action on the device the player is holding,
        /// e.g. "RT" on a pad and "Mouse Left" on keyboard. Used by every hint in the
        /// HUD and menus so the UI always teaches the right controls.
        /// </summary>
        public string Prompt(string mapName, string actionName, int bindingIndex = -1)
        {
            if (_asset == null) return actionName;
            InputActionMap map = _asset.FindActionMap(mapName, false);
            InputAction action = map?.FindAction(actionName, false);
            if (action == null) return actionName;

            int index = bindingIndex;
            if (index < 0)
            {
                index = PreferredBindingIndex(action);
                if (index < 0) return actionName;
            }

            return DescribeBinding(action, index);
        }

        public string PromptGameplay(string actionName, int bindingIndex = -1) => Prompt(GameplayMap, actionName, bindingIndex);

        public string PromptUi(string actionName, int bindingIndex = -1) => Prompt(UiMap, actionName, bindingIndex);

        private int PreferredBindingIndex(InputAction action)
        {
            bool wantGamepad = CurrentDevice == InputDeviceKind.Gamepad;
            string group = wantGamepad ? "Gamepad" : "KeyboardMouse";

            int fallback = -1;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                if (binding.isComposite)
                {
                    if (fallback < 0) fallback = -1;
                    continue;
                }
                if (binding.isPartOfComposite)
                {
                    // Composites read better as a group ("WASD" / "Left Stick").
                    if (fallback < 0 && GroupMatches(binding.groups, group)) fallback = i;
                    continue;
                }
                if (GroupMatches(binding.groups, group)) return i;
                if (fallback < 0) fallback = i;
            }
            return fallback >= 0 ? fallback : (action.bindings.Count > 0 ? 0 : -1);
        }

        private static bool GroupMatches(string groups, string group)
        {
            if (string.IsNullOrEmpty(groups)) return false;
            if (string.IsNullOrEmpty(group)) return true;
            string[] split = groups.Split('&', ';');
            for (int i = 0; i < split.Length; i++)
            {
                if (string.Equals(split[i].Trim(), group, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private string DescribeBinding(InputAction action, int index)
        {
            // For a composite, describe the composite as a whole ("WASD", "Left Stick").
            if (action.bindings[index].isPartOfComposite)
            {
                for (int i = index; i >= 0; i--)
                {
                    if (action.bindings[i].isComposite)
                    {
                        index = i;
                        break;
                    }
                }
            }

            InputBinding binding = action.bindings[index];
            if (binding.isComposite)
            {
                string group = CurrentDevice == InputDeviceKind.Gamepad ? "Gamepad" : "KeyboardMouse";

                // Prefer the composite whose own name matches the active scheme.
                int bestIndex = -1;
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    if (!action.bindings[i].isComposite) continue;
                    if (!GroupMatches(action.bindings[i].groups, group)) continue;
                    bestIndex = i;
                    if (string.Equals(action.bindings[i].name, binding.name, StringComparison.Ordinal)) break;
                }

                if (bestIndex >= 0)
                {
                    string compositeName = action.bindings[bestIndex].name;
                    if (!string.IsNullOrEmpty(compositeName)) return compositeName;
                }
            }

            return DisplayPath(binding.effectivePath);
        }

        /// <summary>Turns a control path into a short label such as "Left Mouse" or "RT".</summary>
        public static string DisplayPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "-";
            if (path == "2DVector") return "Composite";

            try
            {
                string readable = InputControlPath.ToHumanReadableString(
                    path,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
                if (!string.IsNullOrEmpty(readable)) return Compact(readable);
            }
            catch (Exception)
            {
                // Fall through to the manual conversion below.
            }

            int slash = path.LastIndexOf('/');
            return slash >= 0 && slash + 1 < path.Length ? path.Substring(slash + 1) : path;
        }

        private static string Compact(string readable)
        {
            switch (readable)
            {
                case "Left Mouse Button": return "LMB";
                case "Right Mouse Button": return "RMB";
                case "Middle Mouse Button": return "MMB";
                case "Position": return "Mouse";
                case "Left Ctrl": return "L-Ctrl";
                case "Right Ctrl": return "R-Ctrl";
                case "Left Shift": return "L-Shift";
                case "Right Shift": return "R-Shift";
                case "Left Alt": return "L-Alt";
                case "Right Alt": return "R-Alt";
                case "Left Shoulder": return "LB";
                case "Right Shoulder": return "RB";
                case "Left Trigger": return "LT";
                case "Right Trigger": return "RT";
                case "Left Stick": return "LS";
                case "Right Stick": return "RS";
                case "Left Stick Up": return "LS Up";
                case "Left Stick Down": return "LS Down";
                case "Left Stick Left": return "LS Left";
                case "Left Stick Right": return "LS Right";
                case "D-Pad Up": return "D-Up";
                case "D-Pad Down": return "D-Down";
                case "D-Pad Left": return "D-Left";
                case "D-Pad Right": return "D-Right";
                case "Button South": return "A";
                case "Button East": return "B";
                case "Button North": return "Y";
                case "Button West": return "X";
                case "Start Button": return "Start";
                case "Select Button": return "Back";
                default: return readable;
            }
        }

        // ------------------------------------------------- binding overrides

        public const string BindingsFileName = "bindings.json";

        public static string BindingsPath => System.IO.Path.Combine(Application.persistentDataPath, BindingsFileName);

        public bool HasOverrides => _asset != null && _asset.actionMaps.Count > 0 && HasAnyOverride();

        private bool HasAnyOverride()
        {
            for (int m = 0; m < _asset.actionMaps.Count; m++)
            {
                InputActionMap map = _asset.actionMaps[m];
                for (int a = 0; a < map.actions.Count; a++)
                {
                    InputAction action = map.actions[a];
                    for (int b = 0; b < action.bindings.Count; b++)
                    {
                        if (!string.IsNullOrEmpty(action.bindings[b].overridePath)) return true;
                    }
                }
            }
            return false;
        }

        public void SaveBindingOverrides()
        {
            if (_asset == null) return;
            try
            {
                string json = _asset.SaveBindingOverridesAsJson();
                System.IO.File.WriteAllText(BindingsPath, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[IronVeil] Could not save input bindings: " + exception.Message);
            }
        }

        public void LoadBindingOverrides()
        {
            if (_asset == null) return;
            try
            {
                if (!System.IO.File.Exists(BindingsPath)) return;
                string json = System.IO.File.ReadAllText(BindingsPath);
                if (string.IsNullOrEmpty(json)) return;
                _asset.LoadBindingOverridesFromJson(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[IronVeil] Could not load input bindings: " + exception.Message);
            }
        }

        public void ResetAllBindings()
        {
            if (_asset == null) return;
            _asset.RemoveAllBindingOverrides();
            SaveBindingOverrides();
        }

        public void ResetActionBindings(string mapName, string actionName)
        {
            InputAction action = _asset?.FindActionMap(mapName, false)?.FindAction(actionName, false);
            if (action == null) return;
            action.RemoveAllBindingOverrides();
            SaveBindingOverrides();
        }
    }

    /// <summary>
    /// Registry of things the player can aim at. Enemies and weak points register
    /// themselves, which lets aim assist and the aim-snap accessibility option work
    /// without any per-level authoring.
    /// </summary>
    public static class AimAssist
    {
        private static readonly List<AimTarget> Targets = new List<AimTarget>();

        public struct AimTarget
        {
            public Transform Transform;
            public float Priority;
        }

        public static void Register(Transform target, float priority = 1f)
        {
            if (target == null) return;
            for (int i = 0; i < Targets.Count; i++)
            {
                if (Targets[i].Transform == target) return;
            }
            Targets.Add(new AimTarget { Transform = target, Priority = priority });
        }

        public static void Unregister(Transform target)
        {
            for (int i = Targets.Count - 1; i >= 0; i--)
            {
                if (Targets[i].Transform == null || Targets[i].Transform == target) Targets.RemoveAt(i);
            }
        }

        public static void Clear() => Targets.Clear();

        public static int Count => Targets.Count;

        /// <summary>
        /// Bends a firing direction towards a nearby target. Strength is 0..1, where
        /// 1 locks on within a wide cone. Snap mode ignores the cone entirely and
        /// takes the closest target.
        /// </summary>
        public static Vector2 Snap(Vector2 desired, Vector3 origin, float strength, bool snap)
        {
            if (Targets.Count == 0) return desired;

            float coneDegrees = snap ? 180f : Mathf.Lerp(4f, 26f, Mathf.Clamp01(strength));
            float cosCone = Mathf.Cos(coneDegrees * Mathf.Deg2Rad * 0.5f);

            Transform best = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < Targets.Count; i++)
            {
                Transform target = Targets[i].Transform;
                if (target == null) continue;

                Vector3 delta = target.position - origin;
                delta.z = 0f;
                float sqrDistance = delta.sqrMagnitude;
                if (sqrDistance < 0.01f) continue;

                Vector2 toTarget = new Vector2(delta.x, delta.y).normalized;
                float dot = Vector2.Dot(toTarget, desired);
                if (dot < cosCone) continue;

                // Closest wins, weighted by how well it lines up with the current aim.
                float score = sqrDistance * Mathf.Lerp(1f, 0.35f, Mathf.InverseLerp(cosCone, 1f, dot)) / Mathf.Max(0.1f, Targets[i].Priority);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = target;
                }
            }

            if (best == null) return desired;

            Vector3 targetDelta = best.position - origin;
            targetDelta.z = 0f;
            if (targetDelta.sqrMagnitude < 0.01f) return desired;
            Vector2 targetDirection = new Vector2(targetDelta.x, targetDelta.y).normalized;

            if (snap) return targetDirection;

            float blend = Mathf.Clamp01(strength) * 0.6f;
            return Vector2.Lerp(desired, targetDirection, blend).normalized;
        }

        /// <summary>Nearest registered target to a point, used by the reticle highlight.</summary>
        public static Transform Nearest(Vector3 origin, float maxDistance = float.MaxValue)
        {
            Transform best = null;
            float bestDistance = maxDistance * maxDistance;
            for (int i = 0; i < Targets.Count; i++)
            {
                Transform target = Targets[i].Transform;
                if (target == null) continue;
                Vector3 delta = target.position - origin;
                delta.z = 0f;
                float sqr = delta.sqrMagnitude;
                if (sqr < bestDistance)
                {
                    bestDistance = sqr;
                    best = target;
                }
            }
            return best;
        }
    }
}
