using System;
using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;
using UnityEngine.UI;

namespace IronVeil
{
    public enum ScreenId
    {
        None = 0,
        MainMenu = 1,
        CharacterSelect = 2,
        Options = 3,
        Controls = 4,
        Pause = 5,
        Savepoint = 6,
        LevelUp = 7,
        QuestLog = 8,
        Gallery = 9,
        Codex = 10,
        GameOver = 11,
        ChapterComplete = 12,
        Ending = 13,
        Message = 14,
        BossIntro = 15,
        Dialogue = 16,
        Choice = 17,
        Hud = 18
    }

    /// <summary>
    /// Base class for every menu. Widgets are focused spatially, so one navigation
    /// implementation serves vertical lists, tab rows and grids alike.
    /// </summary>
    public abstract class ScreenBase
    {
        protected readonly List<UiWidget> Widgets = new List<UiWidget>();

        private int _focusIndex;

        public ScreenId Id { get; protected set; }
        public RectTransform Root { get; protected set; }
        public RectTransform Container { get; protected set; }
        public bool Visible => Root != null && Root.gameObject.activeSelf;
        public virtual bool AllowsBack => true;
        public virtual bool BlocksPause => true;

        public void Create(Transform parent)
        {
            Root = UiKit.Empty(GetType().Name, parent);
            UiKit.Stretch(Root);
            Container = Root;
            Build();
            Root.gameObject.SetActive(false);
        }

        protected abstract void Build();

        public virtual void OnShow() { }

        public virtual void OnHide() { }

        public virtual void Tick(float deltaTime) { }

        /// <summary>Back button. Return false to keep the screen open.</summary>
        public virtual bool OnCancel() => true;

        public virtual void Refresh() { }

        protected T Add<T>(T widget) where T : UiWidget
        {
            if (widget != null) Widgets.Add(widget);
            return widget;
        }

        protected void ClearWidgets()
        {
            for (int i = 0; i < Widgets.Count; i++)
            {
                if (Widgets[i] != null) UnityEngine.Object.Destroy(Widgets[i].gameObject);
            }
            Widgets.Clear();
            _focusIndex = 0;
        }

        public UiWidget Focused => Widgets.Count == 0 ? null : Widgets[Mathf.Clamp(_focusIndex, 0, Widgets.Count - 1)];

        public void FocusWidget(UiWidget widget)
        {
            int index = Widgets.IndexOf(widget);
            if (index >= 0) SetFocus(index);
        }

        private void SetFocus(int index)
        {
            _focusIndex = Mathf.Clamp(index, 0, Mathf.Max(0, Widgets.Count - 1));
            for (int i = 0; i < Widgets.Count; i++)
            {
                Widgets[i]?.SetHighlighted(i == _focusIndex && Widgets[i].Interactable);
            }
        }

        public void MoveFocus(Vector2 direction)
        {
            if (Widgets.Count == 0 || direction.sqrMagnitude < 0.04f) return;

            UiWidget current = Focused;
            if (current == null || !current.Interactable)
            {
                FocusFirst();
                return;
            }

            Vector2 from = current.CenterScreenPoint();
            Vector2 wanted = direction.normalized;
            float bestScore = float.MaxValue;
            int bestIndex = -1;

            for (int i = 0; i < Widgets.Count; i++)
            {
                UiWidget candidate = Widgets[i];
                if (candidate == null || candidate == current || !candidate.Interactable) continue;

                Vector2 offset = candidate.CenterScreenPoint() - from;
                if (offset.sqrMagnitude < 1f) continue;

                float forward = Vector2.Dot(offset.normalized, wanted);
                if (forward < 0.5f) continue;

                float score = offset.magnitude * (2f - forward);
                if (IsAxisAligned(wanted, offset)) score *= 0.7f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            if (bestIndex >= 0) SetFocus(bestIndex);
        }

        private static bool IsAxisAligned(Vector2 direction, Vector2 offset)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) return Mathf.Abs(offset.y) < Mathf.Abs(offset.x) * 0.5f;
            return Mathf.Abs(offset.x) < Mathf.Abs(offset.y) * 0.5f;
        }

        public void FocusFirst()
        {
            for (int i = 0; i < Widgets.Count; i++)
            {
                if (Widgets[i] != null && Widgets[i].Interactable)
                {
                    SetFocus(i);
                    return;
                }
            }
            SetFocus(0);
        }

        public void SubmitFocused()
        {
            UiWidget focused = Focused;
            if (focused == null || !focused.Interactable) return;
            Services.Get<AudioService>()?.PlaySfx("sfx_ui_click");
            focused.OnSubmit();
        }

        public void AdjustFocused(int delta)
        {
            UiWidget focused = Focused;
            if (focused == null || !focused.Interactable) return;
            focused.OnAdjust(delta);
        }

        public void HandlePointer(Vector2 screenPoint, bool moved)
        {
            if (!moved) return;

            for (int i = 0; i < Widgets.Count; i++)
            {
                UiWidget widget = Widgets[i];
                if (widget == null || !widget.Interactable || !widget.gameObject.activeInHierarchy) continue;
                if (!widget.ContainsScreenPoint(screenPoint)) continue;
                if (Focused != widget)
                {
                    SetFocus(i);
                    Services.Get<AudioService>()?.PlaySfx("sfx_ui_hover");
                }
                return;
            }
        }

        public bool TryClick(Vector2 screenPoint)
        {
            for (int i = 0; i < Widgets.Count; i++)
            {
                UiWidget widget = Widgets[i];
                if (widget == null || !widget.Interactable || !widget.gameObject.activeInHierarchy) continue;
                if (!widget.ContainsScreenPoint(screenPoint)) continue;
                SetFocus(i);
                SubmitFocused();
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Owns every screen, the input routing between them and the HUD. Screens are
    /// stacked: only the top one takes input. Messages and boss cards are overlays
    /// that draw above the top screen without capturing input.
    /// </summary>
    public sealed class ScreenManager : MonoBehaviour
    {
        private const float MessageSeconds = 4f;

        private readonly Dictionary<ScreenId, ScreenBase> _screens = new Dictionary<ScreenId, ScreenBase>();
        private readonly List<ScreenId> _stack = new List<ScreenId>();
        private readonly HashSet<ScreenId> _overlays = new HashSet<ScreenId>();

        private RectTransform _canvasRoot;
        private RectTransform _screenLayer;
        private RectTransform _overlayLayer;
        private RectTransform _hudLayer;
        private HudScreen _hud;
        private ComicPlayer _comic;

        private Vector2 _lastPointer;
        private bool _pointerSeen;
        private float _messageTimer;
        private float _bossCardTimer;
        private ScreenId _optionsReturn = ScreenId.MainMenu;
        private ScreenId _controlsReturn = ScreenId.MainMenu;
        private ScreenId _galleryReturn = ScreenId.MainMenu;
        private ScreenId _codexReturn = ScreenId.MainMenu;

        public ComicPlayer Comic => _comic;
        public bool HidesGameHud { get; set; }

        public bool IsCapturingInput
        {
            get
            {
                if (_comic != null && _comic.IsPlaying) return true;
                for (int i = _stack.Count - 1; i >= 0; i--)
                {
                    if (_screens.TryGetValue(_stack[i], out ScreenBase screen) && screen.Visible) return screen.BlocksPause;
                }
                return false;
            }
        }

        public void Initialize()
        {
            _canvasRoot = UiKit.CreateCanvasObject("IronVeil.Canvas", transform, 100, out _);

            _hudLayer = UiKit.Empty("Hud", _canvasRoot);
            UiKit.Stretch(_hudLayer);

            _screenLayer = UiKit.Empty("Screens", _canvasRoot);
            UiKit.Stretch(_screenLayer);

            _overlayLayer = UiKit.Empty("Overlays", _canvasRoot);
            UiKit.Stretch(_overlayLayer);

            _comic = new ComicPlayer();
            _comic.Initialize(_canvasRoot.transform);

            Register(new HudScreen(), _hudLayer);
            Register(new MainMenuScreen(), _screenLayer);
            Register(new CharacterSelectScreen(), _screenLayer);
            Register(new OptionsScreen(), _screenLayer);
            Register(new ControlsScreen(), _screenLayer);
            Register(new PauseScreen(), _screenLayer);
            Register(new SavepointScreen(), _screenLayer);
            Register(new LevelUpScreen(), _screenLayer);
            Register(new QuestLogScreen(), _screenLayer);
            Register(new GalleryScreen(), _screenLayer);
            Register(new CodexScreen(), _screenLayer);
            Register(new GameOverScreen(), _screenLayer);
            Register(new ChapterCompleteScreen(), _screenLayer);
            Register(new EndingScreen(), _screenLayer);
            Register(new MessageScreen(), _overlayLayer);
            Register(new BossIntroScreen(), _overlayLayer);
            Register(new DialogueScreen(), _screenLayer);
            Register(new ChoiceScreen(), _screenLayer);

            _hud = Get<HudScreen>(ScreenId.Hud);

            SaveService saves = Services.Get<SaveService>();
            if (saves != null) saves.AutoSaved += HandleAutoSaved;

            Show(ScreenId.MainMenu);
        }

        private void HandleAutoSaved(string slot)
        {
            _hud?.FlashSaved();
        }

        private void OnDestroy()
        {
            SaveService saves = Services.Get<SaveService>();
            if (saves != null) saves.AutoSaved -= HandleAutoSaved;
        }

        private void Register(ScreenBase screen, RectTransform layer)
        {
            screen.Create(layer);
            _screens[screen.Id] = screen;
        }

        public T Get<T>(ScreenId id) where T : ScreenBase
        {
            return _screens.TryGetValue(id, out ScreenBase screen) ? screen as T : null;
        }

        private ScreenBase Top()
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (_screens.TryGetValue(_stack[i], out ScreenBase screen) && screen.Visible) return screen;
            }
            return null;
        }

        // ------------------------------------------------------------- visibility

        public void Show(ScreenId id)
        {
            if (!_screens.TryGetValue(id, out ScreenBase screen)) return;
            if (!screen.Visible)
            {
                screen.Root.gameObject.SetActive(true);
                screen.OnShow();
                screen.Refresh();
                screen.FocusFirst();
            }
            screen.Root.SetAsLastSibling();
            if (!_stack.Contains(id)) _stack.Add(id);
        }

        /// <summary>Shows a screen above the stack without taking input.</summary>
        public void ShowOverlay(ScreenId id)
        {
            if (!_screens.TryGetValue(id, out ScreenBase screen)) return;
            screen.Root.gameObject.SetActive(true);
            screen.Root.SetAsLastSibling();
            _overlays.Add(id);
        }

        public void HideOverlay(ScreenId id)
        {
            if (!_overlays.Contains(id)) return;
            _overlays.Remove(id);
            if (_screens.TryGetValue(id, out ScreenBase screen)) screen.Root.gameObject.SetActive(false);
        }

        public void Hide(ScreenId id)
        {
            if (!_screens.TryGetValue(id, out ScreenBase screen)) return;
            screen.OnHide();
            screen.Root.gameObject.SetActive(false);
            _stack.Remove(id);
            _overlays.Remove(id);
        }

        public void HideAll()
        {
            List<ScreenId> copy = new List<ScreenId>(_stack);
            for (int i = 0; i < copy.Count; i++) Hide(copy[i]);
            List<ScreenId> overlays = new List<ScreenId>(_overlays);
            for (int i = 0; i < overlays.Count; i++) HideOverlay(overlays[i]);
            _stack.Clear();
            _overlays.Clear();
        }

        public void ShowHud()
        {
            if (_hud != null) _hud.Root.gameObject.SetActive(true);
        }

        public void HideHud()
        {
            if (_hud != null) _hud.Root.gameObject.SetActive(false);
        }

        /// <summary>Closes the topmost screen, or the playing comic.</summary>
        public void Back()
        {
            if (_comic != null && _comic.IsPlaying)
            {
                _comic.SkipAll();
                return;
            }

            // Messages dismiss first so they never trap input.
            if (_overlays.Contains(ScreenId.Message))
            {
                HideOverlay(ScreenId.Message);
                _messageTimer = 0f;
            }

            ScreenBase top = Top();
            if (top == null) return;
            if (!top.OnCancel()) return;

            Services.Get<AudioService>()?.PlaySfx("sfx_ui_back");
            Hide(top.Id);
            ReturnFrom(top.Id);
        }

        private void ReturnFrom(ScreenId closed)
        {
            GameRoot root = GameRoot.Instance;

            switch (closed)
            {
                case ScreenId.Pause:
                    root?.ToState(GameFlowState.Playing);
                    break;
                case ScreenId.Options:
                    ReturnToOrigin(_optionsReturn);
                    break;
                case ScreenId.Controls:
                    ReturnToOrigin(_controlsReturn);
                    break;
                case ScreenId.Gallery:
                    ReturnToOrigin(_galleryReturn);
                    break;
                case ScreenId.Codex:
                    ReturnToOrigin(_codexReturn);
                    break;
                case ScreenId.Savepoint:
                case ScreenId.LevelUp:
                case ScreenId.QuestLog:
                    root?.ToState(GameFlowState.Playing);
                    break;
            }
        }

        private void ReturnToOrigin(ScreenId origin)
        {
            GameRoot root = GameRoot.Instance;
            Show(origin);
            switch (origin)
            {
                case ScreenId.MainMenu: root?.ToState(GameFlowState.MainMenu); break;
                case ScreenId.CharacterSelect: root?.ToState(GameFlowState.CharacterSelect); break;
                case ScreenId.Gallery: root?.ToState(GameFlowState.Gallery); break;
                case ScreenId.Codex: root?.ToState(GameFlowState.Codex); break;
                case ScreenId.Pause: root?.ToState(GameFlowState.Paused); break;
                case ScreenId.Savepoint: root?.ToState(GameFlowState.Savepoint); break;
                case ScreenId.LevelUp: root?.ToState(GameFlowState.LevelUp); break;
                case ScreenId.QuestLog: root?.ToState(GameFlowState.QuestLog); break;
                default: root?.ToState(GameFlowState.Playing); break;
            }
        }

        public bool CanCloseCurrent() => Top() != null && Top().AllowsBack;

        // ------------------------------------------------------------- entry points

        public void ShowOptions(ScreenId from)
        {
            _optionsReturn = from;
            Get<OptionsScreen>(ScreenId.Options)?.SetOrigin(from);
            Show(ScreenId.Options);
        }

        public void ShowControls(ScreenId from)
        {
            _controlsReturn = from;
            Get<ControlsScreen>(ScreenId.Controls)?.SetOrigin(from);
            Show(ScreenId.Controls);
        }

        public void ShowGallery(ScreenId from)
        {
            _galleryReturn = from;
            Show(ScreenId.Gallery);
        }

        public void ShowCodex(ScreenId from)
        {
            _codexReturn = from;
            Show(ScreenId.Codex);
        }

        public void ShowMessage(string title, string body)
        {
            MessageScreen message = Get<MessageScreen>(ScreenId.Message);
            if (message == null) return;
            message.Set(title, body);
            ShowOverlay(ScreenId.Message);
            _messageTimer = MessageSeconds;
        }

        public void ShowBossIntro(BossDefinition boss)
        {
            BossIntroScreen intro = Get<BossIntroScreen>(ScreenId.BossIntro);
            if (intro == null) return;
            intro.Set(boss);
            ShowOverlay(ScreenId.BossIntro);
            _bossCardTimer = 2.8f;
        }

        public void ShowComic(ComicPageDefinition page, Action onFinished)
        {
            HideHud();
            _comic?.Play(page, () =>
            {
                ShowHud();
                GameRoot.Instance?.ToState(GameFlowState.Playing);
                onFinished?.Invoke();
            });
        }

        public void ShowDialogue(string title, List<string> lines, Action onFinished)
        {
            DialogueScreen dialogue = Get<DialogueScreen>(ScreenId.Dialogue);
            if (dialogue == null)
            {
                onFinished?.Invoke();
                return;
            }
            dialogue.Set(title, lines, () =>
            {
                Hide(ScreenId.Dialogue);
                onFinished?.Invoke();
            });
            Show(ScreenId.Dialogue);
        }

        public void ShowChoice(string title, string prompt, string[] options, Action<int> onChosen)
        {
            ChoiceScreen choice = Get<ChoiceScreen>(ScreenId.Choice);
            if (choice == null)
            {
                onChosen?.Invoke(0);
                return;
            }
            choice.Set(title, prompt, options, index =>
            {
                Hide(ScreenId.Choice);
                onChosen?.Invoke(index);
            });
            Show(ScreenId.Choice);
        }

        public void ShowGameOver(string cause)
        {
            Get<GameOverScreen>(ScreenId.GameOver)?.Set(cause);
            Show(ScreenId.GameOver);
        }

        public void ShowChapterComplete(ChapterDefinition chapter, string summary)
        {
            Get<ChapterCompleteScreen>(ScreenId.ChapterComplete)?.Set(chapter, summary);
            Show(ScreenId.ChapterComplete);
        }

        public void ShowEnding() => Show(ScreenId.Ending);

        public void ShowLevelUp(LevelUpResult result)
        {
            Get<LevelUpScreen>(ScreenId.LevelUp)?.Set(result);
            Show(ScreenId.LevelUp);
        }

        public void ShowSavepoint(SavepointDefinition savepoint)
        {
            Get<SavepointScreen>(ScreenId.Savepoint)?.Set(savepoint);
            Show(ScreenId.Savepoint);
        }

        public void SetObjective(string title, string objective) => _hud?.SetObjective(title, objective);
        public void SetInteractPrompt(string prompt) => _hud?.SetInteractPrompt(prompt);

        // ------------------------------------------------------------- tick

        public void Tick(float deltaTime)
        {
            InputService input = Services.Get<InputService>();

            if (_comic != null && _comic.IsPlaying)
            {
                _comic.Tick(deltaTime);
                if (input != null)
                {
                    if (input.SubmitPressed || input.ClickPressed) _comic.Advance();
                    else if (input.CancelPressed) _comic.SkipAll();
                }
                return;
            }

            if (_messageTimer > 0f)
            {
                _messageTimer -= deltaTime;
                if (_messageTimer <= 0f) HideOverlay(ScreenId.Message);
            }

            if (_bossCardTimer > 0f)
            {
                _bossCardTimer -= deltaTime;
                if (_bossCardTimer <= 0f) HideOverlay(ScreenId.BossIntro);
            }

            ScreenBase top = Top();
            if (top != null && input != null)
            {
                HandleScreenInput(top, input, deltaTime);
            }

            top?.Tick(deltaTime);
            _hud?.Tick(deltaTime);
        }

        private void HandleScreenInput(ScreenBase top, InputService input, float deltaTime)
        {
            Vector2 navigate = input.NavigateRepeat();
            if (navigate.sqrMagnitude > 0.2f)
            {
                bool horizontal = Mathf.Abs(navigate.x) > Mathf.Abs(navigate.y);
                UiWidget focused = top.Focused;
                bool adjusts = focused is UiOptionRow || focused is UiSlider || focused is UiToggleRow;

                if (horizontal && adjusts) top.AdjustFocused(navigate.x > 0f ? 1 : -1);
                else top.MoveFocus(horizontal ? new Vector2(navigate.x, 0f) : new Vector2(0f, navigate.y));
            }

            if (input.SubmitPressed) top.SubmitFocused();
            if (input.PausePressed && top.Id != ScreenId.Pause) Back();
            if (input.CancelPressed) Back();

            if (input.UsesCursorAim)
            {
                Vector2 pointer = input.PointPosition;
                bool moved = !_pointerSeen || (pointer - _lastPointer).sqrMagnitude > 16f;
                _lastPointer = pointer;
                _pointerSeen = true;
                top.HandlePointer(pointer, moved);
                if (input.ClickPressed) top.TryClick(pointer);
            }
        }

        public void Refresh()
        {
            Top()?.Refresh();
            _hud?.Refresh();
        }
    }
}
