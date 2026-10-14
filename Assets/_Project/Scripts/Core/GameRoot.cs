using System;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    public enum GameFlowState
    {
        Booting = 0,
        MainMenu = 1,
        CharacterSelect = 2,
        Options = 3,
        Controls = 4,
        Playing = 5,
        Paused = 6,
        Comic = 7,
        Savepoint = 8,
        LevelUp = 9,
        QuestLog = 10,
        Gallery = 11,
        Codex = 12,
        Dying = 13,
        GameOver = 14,
        ChapterComplete = 15
    }

    /// <summary>
    /// Single source of truth for what the game is doing right now. Input routing,
    /// pause behaviour and the UI all read from here so they can never disagree.
    /// </summary>
    public sealed class GameStateService
    {
        private GameFlowState _state = GameFlowState.Booting;
        private GameFlowState _previous = GameFlowState.Booting;

        public GameFlowState State => _state;

        /// <summary>The state to return to when a menu closes over gameplay.</summary>
        public GameFlowState Previous => _previous;

        public event Action<GameFlowState> Changed;

        public bool IsGameplayActive => _state == GameFlowState.Playing || _state == GameFlowState.Dying;

        /// <summary>True whenever the player can be hurt but the world keeps running.</summary>
        public bool IsWorldRunning =>
            _state == GameFlowState.Playing ||
            _state == GameFlowState.Dying ||
            _state == GameFlowState.Comic;

        public bool IsUiVisible =>
            _state == GameFlowState.MainMenu ||
            _state == GameFlowState.CharacterSelect ||
            _state == GameFlowState.Options ||
            _state == GameFlowState.Controls ||
            _state == GameFlowState.Paused ||
            _state == GameFlowState.Savepoint ||
            _state == GameFlowState.LevelUp ||
            _state == GameFlowState.QuestLog ||
            _state == GameFlowState.Gallery ||
            _state == GameFlowState.Codex ||
            _state == GameFlowState.GameOver ||
            _state == GameFlowState.ChapterComplete;

        /// <summary>Pause-like: the world stops but gameplay input is still routed to UI.</summary>
        public bool IsTimeStopped =>
            _state == GameFlowState.Comic ||
            _state == GameFlowState.Paused ||
            _state == GameFlowState.Savepoint ||
            _state == GameFlowState.LevelUp ||
            _state == GameFlowState.QuestLog ||
            _state == GameFlowState.Gallery ||
            _state == GameFlowState.Codex ||
            _state == GameFlowState.GameOver ||
            _state == GameFlowState.ChapterComplete ||
            _state == GameFlowState.Options ||
            _state == GameFlowState.Controls ||
            _state == GameFlowState.CharacterSelect;

        public void Set(GameFlowState state)
        {
            if (_state == state) return;
            _previous = _state;
            _state = state;
            Changed?.Invoke(state);
        }

        public void RestorePrevious(GameFlowState fallback = GameFlowState.Playing)
        {
            GameFlowState target = _previous;
            if (target == GameFlowState.Booting) target = fallback;
            Set(target);
        }
    }

    /// <summary>
    /// Boots the whole game. Created automatically before the first scene loads, so
    /// the project is playable from any scene without scene wiring.
    /// </summary>
    public sealed class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance { get; private set; }

        private GameStateService _state;
        private SettingsService _settings;
        private InputService _input;
        private InputRebinder _rebinder;
        private SaveService _saves;
        private ProgressionService _progression;
        private QuestService _quests;
        private StoryService _story;
        private AudioService _audio;
        private ContentDatabase _content;
        private ScreenManager _screens;
        private ChapterRuntime _chapter;
        private Camera _camera;

        public GameStateService State => _state;
        public SettingsService Settings => _settings;
        public InputService Input => _input;
        public InputRebinder Rebinder => _rebinder;
        public SaveService Saves => _saves;
        public ProgressionService Progression => _progression;
        public QuestService Quests => _quests;
        public StoryService Story => _story;
        public AudioService Audio => _audio;
        public ContentDatabase Content => _content;
        public ScreenManager Screens => _screens;
        public ChapterRuntime Chapter => _chapter;

        /// <summary>No scene setup required: the root builds itself before the first scene.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoBoot()
        {
            if (Instance != null) return;
            GameObject root = new GameObject("IronVeil.GameRoot");
            root.AddComponent<GameRoot>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Services.Register(this);

            Application.targetFrameRate = -1;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _content = new ContentDatabase();
            Services.Register(_content);

            _state = new GameStateService();
            Services.Register(_state);

            _audio = new AudioService();
            _audio.Initialize();
            Services.Register(_audio);

            _settings = new SettingsService();
            Services.Register(_settings);
            _settings.Initialize();

            _input = new InputService();
            Services.Register(_input);
            _input.Initialize();

            _rebinder = new InputRebinder(_input);
            Services.Register(_rebinder);

            _saves = new SaveService(_content);
            Services.Register(_saves);
            _saves.Initialize();

            _progression = new ProgressionService(_content, _saves);
            Services.Register(_progression);

            _quests = new QuestService(_content, _saves);
            Services.Register(_quests);

            _story = new StoryService(_content, _saves);
            Services.Register(_story);

            CreateCamera();

            GameObject uiRoot = new GameObject("IronVeil.UI");
            uiRoot.transform.SetParent(transform, false);
            _screens = uiRoot.AddComponent<ScreenManager>();
            Services.Register(_screens);
            _screens.Initialize();

            GameObject chapterRoot = new GameObject("IronVeil.Chapter");
            chapterRoot.transform.SetParent(transform, false);
            _chapter = chapterRoot.AddComponent<ChapterRuntime>();
            Services.Register(_chapter);
            _chapter.Initialize(_camera);

            _state.Changed += HandleStateChanged;
            _state.Set(GameFlowState.MainMenu);

            _audio.PlayMusic("mx_menu");
            Debug.Log("[IronVeil] " + _content.Content.Title + " booted. " +
                      _content.Content.Chapters.Count + " chapters, " +
                      _content.Content.Bosses.Count + " bosses, " +
                      _content.Content.Quests.Count + " quests, " +
                      _content.Content.TotalComicPanels() + " comic panels.");
        }

        private void CreateCamera()
        {
            GameObject cameraObject = new GameObject("IronVeil.Camera");
            cameraObject.transform.SetParent(transform, false);
            _camera = cameraObject.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 6.2f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = Palette.Background;
            _camera.transform.position = new Vector3(0f, 2f, -10f);
            cameraObject.tag = "MainCamera";
        }

        public Camera GameCamera => _camera;

        private void Update()
        {
            if (_input != null) _input.Poll();

            if (_state.IsTimeStopped) Time.timeScale = 0f;
            else Time.timeScale = 1f;

            switch (_state.State)
            {
                case GameFlowState.Playing:
                case GameFlowState.Dying:
                    _saves.TickPlayTime(Time.deltaTime);
                    _chapter.Tick(Time.deltaTime);
                    break;
                case GameFlowState.Comic:
                    _chapter.TickComic(Time.unscaledDeltaTime);
                    break;
                default:
                    _chapter.TickIdle(Time.unscaledDeltaTime);
                    break;
            }

            // Menus handle their own pause/cancel input; this only opens the pause menu
            // from live gameplay.
            if (_input != null && _state.IsWorldRunning && !_screens.IsCapturingInput && _input.PausePressed)
            {
                TogglePause();
            }

            if (_screens != null) _screens.Tick(Time.unscaledDeltaTime);
        }

        private void HandleStateChanged(GameFlowState state)
        {
            bool gameplayInput = state == GameFlowState.Playing || state == GameFlowState.Dying;
            _input?.SetGameplayAndUiEnabled(gameplayInput, _state.IsUiVisible || gameflowNeedsUi());
            if (state == GameFlowState.Paused || state == GameFlowState.Playing)
            {
                _screens?.Refresh();
            }
        }

        private bool gameflowNeedsUi()
        {
            return _state.IsUiVisible || _state.State == GameFlowState.Comic || _state.State == GameFlowState.Playing;
        }

        /// <summary>Moves the game to a flow state. Used by screens and the runtime.</summary>
        public void ToState(GameFlowState state) => _state.Set(state);

        public void OpenCharacterSelect()
        {
            _screens.Show(ScreenId.CharacterSelect);
            _state.Set(GameFlowState.CharacterSelect);
        }

        public void OpenLevelUpScreen()
        {
            LevelUpResult result = new LevelUpResult
            {
                Level = _progression.Level,
                XpIntoLevel = _progression.XpIntoLevel,
                LevelsGained = 0,
                PointsGained = _progression.AvailablePoints
            };
            _screens.ShowLevelUp(result);
            _state.Set(GameFlowState.LevelUp);
        }

        // ------------------------------------------------------------- flow

        public void StartNewGame(CharacterId character, Difficulty difficulty)
        {
            _screens.HideAll();
            SaveGame save = SaveCodec.NewGame(character, difficulty, _content.Content);
            _saves.SetCurrent(save);
            _saves.SaveAutosave("chapter_start");
            _progression.Rebuild();
            _quests.Rebuild();
            _story.Rebuild();
            _chapter.LoadChapter(1, save.BeatId, true);
            _audio.PlayMusic("mx_ch01");
            _screens.ShowHud();
            _state.Set(GameFlowState.Playing);
        }

        public void ContinueGame(string slotId)
        {
            SaveGame save = _saves.Load(slotId, out string error);
            if (save == null)
            {
                Debug.LogWarning("[IronVeil] Continue failed: " + error);
                _screens.ShowMessage("Could not load that save", error);
                return;
            }

            _screens.HideAll();

            _saves.SetCurrent(save);
            _progression.Rebuild();
            _quests.Rebuild();
            _story.Rebuild();
            _chapter.LoadChapter(save.ChapterIndex, save.BeatId, false);
            _audio.PlayMusic(_chapter.CurrentMusicId);
            _screens.ShowHud();
            _state.Set(GameFlowState.Playing);
        }

        public void TogglePause()
        {
            if (_state.State == GameFlowState.Paused)
            {
                ClosePause();
                return;
            }
            if (!_state.IsWorldRunning) return;
            _screens.Show(ScreenId.Pause);
            _state.Set(GameFlowState.Paused);
        }

        public void ClosePause()
        {
            _screens.Hide(ScreenId.Pause);
            _state.Set(GameFlowState.Playing);
        }

        public void ReturnToMainMenu()
        {
            _saves.SaveCurrent(true);
            _chapter.Unload();
            _screens.HideAll();
            _screens.Show(ScreenId.MainMenu);
            _audio.PlayMusic("mx_menu");
            _state.Set(GameFlowState.MainMenu);
        }

        public void OpenOptions(ScreenId from)
        {
            _screens.ShowOptions(from);
            _state.Set(GameFlowState.Options);
        }

        public void OpenControls(ScreenId from)
        {
            _screens.ShowControls(from);
            _state.Set(GameFlowState.Controls);
        }

        public void OpenQuestLog()
        {
            if (!_state.IsWorldRunning) return;
            _screens.Show(ScreenId.QuestLog);
            _state.Set(GameFlowState.QuestLog);
        }

        public void CloseQuestLog()
        {
            _screens.Hide(ScreenId.QuestLog);
            _state.Set(GameFlowState.Playing);
        }

        public void OpenGallery()
        {
            if (_state.State != GameFlowState.MainMenu && !_state.IsWorldRunning) return;
            _screens.Show(ScreenId.Gallery);
            _state.Set(GameFlowState.Gallery);
        }

        public void CloseGallery()
        {
            _screens.Hide(ScreenId.Gallery);
            if (_saves.HasCurrent && _chapter.IsLoaded) _state.Set(GameFlowState.Playing);
            else _state.Set(GameFlowState.MainMenu);
        }

        public void OpenCodex()
        {
            if (_state.State != GameFlowState.MainMenu && !_state.IsWorldRunning) return;
            _screens.Show(ScreenId.Codex);
            _state.Set(GameFlowState.Codex);
        }

        public void CloseCodex()
        {
            _screens.Hide(ScreenId.Codex);
            if (_saves.HasCurrent && _chapter.IsLoaded) _state.Set(GameFlowState.Playing);
            else _state.Set(GameFlowState.MainMenu);
        }

        /// <summary>Called by the chapter runtime when the player dies.</summary>
        public void HandlePlayerDeath()
        {
            _state.Set(GameFlowState.Dying);
        }

        /// <summary>Called once the death animation has played out.</summary>
        public void ShowGameOver(string cause)
        {
            _screens.ShowGameOver(cause);
            _state.Set(GameFlowState.GameOver);
        }

        /// <summary>Respawns at the last checkpoint, applying the death penalty.</summary>
        public void RespawnAtCheckpoint()
        {
            _saves.RegisterDeath();
            _chapter.RespawnAtCheckpoint();
            _screens.Hide(ScreenId.GameOver);
            _state.Set(GameFlowState.Playing);
        }

        public void RequestChapterComplete()
        {
            _screens.ShowChapterComplete(_chapter.Chapter, _chapter.ChapterSummary());
            _state.Set(GameFlowState.ChapterComplete);
        }

        public void AdvanceToNextChapter()
        {
            ChapterDefinition current = _chapter.Chapter;
            if (current == null)
            {
                ReturnToMainMenu();
                return;
            }

            SaveGame save = _saves.Current;
            save.CompleteChapter("ch" + current.Index.ToString("00"));
            if (!string.IsNullOrEmpty(current.UnlockFlagOnComplete)) save.GrantFlag(current.UnlockFlagOnComplete);

            int next = current.Index + 1;
            if (next > ChapterRules.ChapterCount)
            {
                _screens.ShowEnding();
                _saves.SaveCurrent(true);
                return;
            }

            _saves.SaveAutosave("chapter_complete");
            SaveGame saved = _saves.Current;
            saved.ChapterIndex = next;
            ChapterDefinition nextChapter = _content.Chapter(next);
            saved.BeatId = nextChapter != null && nextChapter.Beats.Count > 0 ? nextChapter.Beats[0].Id : string.Empty;
            saved.CheckpointId = string.Empty;
            _saves.SaveAutosave("chapter_start");
            _chronologicalChapterEntry(next);
        }

        private void _chronologicalChapterEntry(int next)
        {
            _screens.Hide(ScreenId.ChapterComplete);
            _chapter.LoadChapter(next, _saves.Current.BeatId, true);
            _audio.PlayMusic(_chapter.CurrentMusicId);
            _state.Set(GameFlowState.Playing);
        }

        public void OpenSavepoint(SavepointDefinition savepoint)
        {
            _screens.ShowSavepoint(savepoint);
            _state.Set(GameFlowState.Savepoint);
        }

        public void CloseSavepoint()
        {
            _screens.Hide(ScreenId.Savepoint);
            _state.Set(GameFlowState.Playing);
        }

        public void ShowLevelUp(LevelUpResult result)
        {
            _screens.ShowLevelUp(result);
            _state.Set(GameFlowState.LevelUp);
        }

        public void CloseLevelUp()
        {
            _screens.Hide(ScreenId.LevelUp);
            _state.Set(GameFlowState.Playing);
        }

        // ------------------------------------------------------------- lifecycle

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && _settings != null && _settings.Settings.PauseOnFocusLost &&
                _state != null && _state.State == GameFlowState.Playing)
            {
                TogglePause();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                _saves?.SaveCurrent(true);
                _settings?.Save();
            }
        }

        private void OnApplicationQuit()
        {
            _settings?.Save();
            _saves?.SaveCurrent(true);
            _input?.SaveBindingOverrides();
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            _rebinder?.Dispose();
            _input?.Dispose();
            Instance = null;
            Services.Clear();
            AimAssist.Clear();
        }
    }
}
