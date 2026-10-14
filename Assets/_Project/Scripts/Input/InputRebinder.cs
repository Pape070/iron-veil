using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IronVeil
{
    /// <summary>One rebindable row in the controls panel.</summary>
    public sealed class RebindEntry
    {
        public string MapName;
        public string ActionName;
        public string ActionLabel;
        public int BindingIndex;
        public string SlotLabel;    // "Primary", "Secondary", "WASD Up" ...
        public string Group;        // KeyboardMouse / Gamepad / empty
        public string CurrentPath;
        public string CurrentLabel;
        public bool IsOverridden;
    }

    /// <summary>
    /// Drives the Input System's interactive rebinding, filters illegal controls,
    /// detects conflicts with other actions and persists the overrides as JSON.
    /// </summary>
    public sealed class InputRebinder : IDisposable
    {
        private readonly InputService _input;
        private InputActionRebindingExtensions.RebindingOperation _operation;

        public bool IsRebinding => _operation != null;
        public RebindEntry Active { get; private set; }

        /// <summary>Raised when a rebind ends: success, the new label, or a failure reason.</summary>
        public event Action<bool, string> RebindFinished;

        /// <summary>Raised when a rebind begins so the UI can show "press a key".</summary>
        public event Action<RebindEntry> RebindStarted;

        public InputRebinder(InputService input)
        {
            _input = input;
        }

        public void Dispose()
        {
            Cancel();
        }

        // ------------------------------------------------- listing

        /// <summary>
        /// Every rebindable binding for a control scheme. Composite parts are listed
        /// individually ("Move — Up") so a player can remap WASD to the arrow keys.
        /// Mouse position and scroll are excluded: they are pointer inputs, not keys.
        /// </summary>
        public List<RebindEntry> Entries(string mapName, InputDeviceKind scheme)
        {
            List<RebindEntry> entries = new List<RebindEntry>();
            InputActionAsset asset = _input?.Asset;
            if (asset == null) return entries;

            InputActionMap map = asset.FindActionMap(mapName, false);
            if (map == null) return entries;

            string group = scheme == InputDeviceKind.Gamepad ? "Gamepad" : "KeyboardMouse";

            for (int a = 0; a < map.actions.Count; a++)
            {
                InputAction action = map.actions[a];
                if (IsNonRebindable(action)) continue;

                int shownForAction = 0;
                for (int b = 0; b < action.bindings.Count; b++)
                {
                    InputBinding binding = action.bindings[b];
                    if (binding.isComposite) continue;
                    if (!GroupMatches(binding.groups, group)) continue;
                    if (IsPointerBinding(binding.effectivePath)) continue;

                    string slot;
                    if (binding.isPartOfComposite)
                    {
                        slot = CompositeName(action, b) + " " + Capitalise(binding.name);
                    }
                    else
                    {
                        shownForAction++;
                        slot = shownForAction == 1 ? "Primary" : "Secondary " + shownForAction;
                    }

                    entries.Add(new RebindEntry
                    {
                        MapName = mapName,
                        ActionName = action.name,
                        ActionLabel = PrettyName(action.name),
                        BindingIndex = b,
                        SlotLabel = slot.Trim(),
                        Group = binding.groups,
                        CurrentPath = binding.effectivePath,
                        CurrentLabel = InputService.DisplayPath(binding.effectivePath),
                        IsOverridden = !string.IsNullOrEmpty(binding.overridePath)
                    });
                }
            }

            return entries;
        }

        private static bool IsNonRebindable(InputAction action)
        {
            // Leave the UI map's pointer actions alone; everything else is fair game.
            return false;
        }

        private static string CompositeName(InputAction action, int bindingIndex)
        {
            for (int i = bindingIndex; i >= 0; i--)
            {
                if (action.bindings[i].isComposite) return action.bindings[i].name;
            }
            return string.Empty;
        }

        private static bool IsPointerBinding(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return path.Contains("<Mouse>/position") || path.Contains("<Mouse>/delta") ||
                   path.Contains("<Pointer>/position") || path.Contains("<Mouse>/scroll");
        }

        private static bool GroupMatches(string groups, string group)
        {
            if (string.IsNullOrEmpty(groups)) return false;
            string[] split = groups.Split('&', ';');
            for (int i = 0; i < split.Length; i++)
            {
                if (string.Equals(split[i].Trim(), group, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        /// <summary>"AbilityPrimary" -> "Ability Primary".</summary>
        public static string PrettyName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            StringBuilder builder = new StringBuilder(name.Length + 6);
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (i > 0 && char.IsUpper(c) && !char.IsUpper(name[i - 1])) builder.Append(' ');
                builder.Append(c);
            }
            return builder.ToString();
        }

        private static string Capitalise(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        // ------------------------------------------------- rebinding

        /// <summary>
        /// Starts an interactive rebind. Escape cancels and the result is reported
        /// through RebindFinished. Gameplay input is disabled for the duration so a
        /// press cannot leak into the game.
        /// </summary>
        public bool Begin(RebindEntry entry)
        {
            if (entry == null || _input?.Asset == null) return false;
            if (IsRebinding) Cancel();

            InputAction action = _input.Asset.FindActionMap(entry.MapName, false)?.FindAction(entry.ActionName, false);
            if (action == null)
            {
                RebindFinished?.Invoke(false, "That action no longer exists.");
                return false;
            }

            Active = entry;
            RebindStarted?.Invoke(entry);

            _input.SetGameplayAndUiEnabled(false, false);

            try
            {
                _operation = action.PerformInteractiveRebinding(entry.BindingIndex)
                    .WithControlsExcluding("<Mouse>/position")
                    .WithControlsExcluding("<Mouse>/delta")
                    .WithControlsExcluding("<Mouse>/scroll")
                    .WithControlsExcluding("<Pointer>/position")
                    .WithCancelingThrough("<Keyboard>/escape")
                    .OnMatchWaitForAnotherPress()
                    .OnCancel(operation => Finish(false, "Rebinding cancelled.", false))
                    .OnComplete(operation => Finish(true, null, true))
                    .Start();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[IronVeil] Rebinding failed to start: " + exception.Message);
                Finish(false, "This control cannot be rebound.", false);
                return false;
            }

            return true;
        }

        public void Cancel()
        {
            if (_operation == null) return;
            try
            {
                _operation.Cancel();
            }
            catch (Exception)
            {
                // Ignore: the operation is being torn down anyway.
            }
        }

        private void Finish(bool success, string message, bool apply)
        {
            InputActionRebindingExtensions.RebindingOperation operation = _operation;
            _operation = null;

            if (apply && Active != null && success)
            {
                InputAction action = _input.Asset.FindActionMap(Active.MapName, false)?.FindAction(Active.ActionName, false);
                if (action != null)
                {
                    string newPath = action.bindings[Active.BindingIndex].effectivePath;
                    string label = InputService.DisplayPath(newPath);

                    List<string> conflicts = FindConflicts(Active, newPath);
                    if (conflicts.Count > 0)
                    {
                        // Keep the binding but tell the player what it now clashes with.
                        message = "Bound to " + label + " (also used by " + string.Join(", ", conflicts.ToArray()) + ").";
                    }
                    else
                    {
                        message = "Bound to " + label + ".";
                    }

                    Active.CurrentPath = newPath;
                    Active.CurrentLabel = label;
                    Active.IsOverridden = !string.IsNullOrEmpty(action.bindings[Active.BindingIndex].overridePath);
                }
            }

            operation?.Dispose();

            _input.SaveBindingOverrides();
            GameStateService state = Services.Get<GameStateService>();
            if (state == null) _input.SetGameplayAndUiEnabled(true, true);
            else _input.SetGameplayAndUiEnabled(state.IsGameplayActive, state.IsUiVisible);
            RebindFinished?.Invoke(success, message);
            Active = null;
        }

        // ------------------------------------------------- conflicts

        /// <summary>
        /// Lists other bindings that already use this control. Matching is done on
        /// the resolved control path, ignoring device prefixes, so "leftCtrl" on any
        /// keyboard collides with "leftCtrl" on another keyboard.
        /// </summary>
        public List<string> FindConflicts(RebindEntry entry, string effectivePath)
        {
            List<string> conflicts = new List<string>();
            InputActionAsset asset = _input?.Asset;
            if (asset == null || string.IsNullOrEmpty(effectivePath)) return conflicts;

            string needle = Normalise(effectivePath);
            for (int m = 0; m < asset.actionMaps.Count; m++)
            {
                InputActionMap map = asset.actionMaps[m];
                for (int a = 0; a < map.actions.Count; a++)
                {
                    InputAction action = map.actions[a];
                    for (int b = 0; b < action.bindings.Count; b++)
                    {
                        if (map.name == entry.MapName && action.name == entry.ActionName && b == entry.BindingIndex) continue;
                        string path = action.bindings[b].effectivePath;
                        if (string.IsNullOrEmpty(path)) continue;
                        if (Normalise(path) != needle) continue;

                        string description = PrettyName(action.name);
                        if (!conflicts.Contains(description)) conflicts.Add(description);
                    }
                }
            }
            return conflicts;
        }

        private static string Normalise(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            int slash = path.IndexOf('/');
            string withoutDevice = slash >= 0 ? path.Substring(slash) : path;
            return withoutDevice.ToLowerInvariant().Replace("/press", string.Empty);
        }

        /// <summary>Number of bindings the player has changed, for the controls header.</summary>
        public int OverrideCount()
        {
            InputActionAsset asset = _input?.Asset;
            if (asset == null) return 0;

            int count = 0;
            for (int m = 0; m < asset.actionMaps.Count; m++)
            {
                InputActionMap map = asset.actionMaps[m];
                for (int a = 0; a < map.actions.Count; a++)
                {
                    InputAction action = map.actions[a];
                    for (int b = 0; b < action.bindings.Count; b++)
                    {
                        if (!string.IsNullOrEmpty(action.bindings[b].overridePath)) count++;
                    }
                }
            }
            return count;
        }

        public void ResetAction(RebindEntry entry)
        {
            if (entry == null) return;
            _input.ResetActionBindings(entry.MapName, entry.ActionName);
            RebindFinished?.Invoke(true, PrettyName(entry.ActionName) + " restored to defaults.");
        }

        public void ResetAll()
        {
            _input.ResetAllBindings();
            RebindFinished?.Invoke(true, "All controls restored to defaults.");
        }
    }
}
