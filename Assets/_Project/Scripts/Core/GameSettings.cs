using System;
using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    public enum AimSource
    {
        Auto = 0,
        MouseCursor = 1,
        RightStick = 2
    }

    public enum HudScaleMode
    {
        Compact = 0,
        Normal = 1,
        Large = 2
    }

    /// <summary>
    /// Everything the options menu can change, in four groups: display, audio,
    /// gameplay and accessibility. Stored in its own file next to the saves so
    /// deleting a save never resets the player's setup.
    /// </summary>
    [Serializable]
    public sealed class GameSettings
    {
        public const int CurrentVersion = 1;

        // -------- display --------
        public int ResolutionWidth;
        public int ResolutionHeight;
        public int FullScreenMode = 1;              // UnityEngine.FullScreenMode
        public int MonitorIndex;
        public bool VSync = true;
        public int FrameRateCap = 144;              // 0 = uncapped
        public int QualityLevel = -1;               // -1 = pick the highest available
        public float Brightness = 1f;               // drives the gamma overlay

        // -------- audio --------
        public float MasterVolume = 0.8f;
        public float MusicVolume = 0.7f;
        public float SfxVolume = 0.9f;
        public float VoiceVolume = 1f;
        public bool Muted;
        public bool MonoOutput;
        public float AmbienceVolume = 0.6f;

        // -------- gameplay --------
        public int DefaultDifficulty = 1;           // Normal
        public bool AutoConfirmDifficulty = false;
        public bool ScreenShake = true;
        public float ScreenShakeScale = 1f;
        public bool HitStop = true;
        public bool DamageNumbers = true;
        public bool AimGuide = true;
        public int AimSource = 0;                   // Auto
        public float AimAssistStrength = 0.25f;     // 0 = off, 1 = strong
        public bool InvertY;
        public float StickDeadzone = 0.18f;
        public bool ToggleAim = false;              // hold vs toggle for lock-on
        public bool AutoSaveOnCheckpoint = true;
        public bool AutoSaveOnSavepoint = true;
        public bool PauseOnFocusLost = true;
        public bool ShowEnemyHealthBars = true;
        public float ComicAutoAdvanceSeconds = 0f; // 0 = manual

        // -------- accessibility --------
        public bool ReducedFlashing;
        public bool HighContrastUi;
        public bool ColorblindSafePalette;
        public bool LargeText;
        public int HudScale = 1;                    // HudScaleMode
        public bool Subtitles = true;
        public bool SubtitleSpeakerNames = true;
        public float SubtitleTextScale = 1f;
        public float DialogueSpeed = 1f;
        public int ExtraLives = 0;                  // 0 = none
        public bool HonoursDamageReduction = false; // "forgiving damage" assist
        public bool SlowDownBosses = false;         // accessibility: boss pattern speed
        public bool AimSnapToTargets = false;
        public bool ComicSkipHint = true;
        public bool AutoAdvanceComicPanels = false;

        public void Sanitize()
        {
            MasterVolume = Mathf.Clamp01(MasterVolume);
            MusicVolume = Mathf.Clamp01(MusicVolume);
            SfxVolume = Mathf.Clamp01(SfxVolume);
            VoiceVolume = Mathf.Clamp01(VoiceVolume);
            AmbienceVolume = Mathf.Clamp01(AmbienceVolume);
            Brightness = Mathf.Clamp(Brightness, 0.5f, 2f);
            ScreenShakeScale = Mathf.Clamp(ScreenShakeScale, 0f, 2f);
            AimAssistStrength = Mathf.Clamp01(AimAssistStrength);
            StickDeadzone = Mathf.Clamp(StickDeadzone, 0.05f, 0.6f);
            DialogueSpeed = Mathf.Clamp(DialogueSpeed, 0.5f, 2f);
            SubtitleTextScale = Mathf.Clamp(SubtitleTextScale, 0.8f, 1.8f);
            ComicAutoAdvanceSeconds = Mathf.Clamp(ComicAutoAdvanceSeconds, 0f, 30f);
            DefaultDifficulty = MathLite.Clamp(DefaultDifficulty, 0, 2);
            FullScreenMode = MathLite.Clamp(FullScreenMode, 0, 3);
            HudScale = MathLite.Clamp(HudScale, 0, 2);
            ExtraLives = MathLite.Clamp(ExtraLives, 0, 3);
            MonitorIndex = MathLite.Max(0, MonitorIndex);
            FrameRateCap = FrameRateCap < 0 ? 0 : FrameRateCap;
            QualityLevel = MathLite.Clamp(QualityLevel, -1, 9);

            if (ResolutionWidth <= 0 || ResolutionHeight <= 0)
            {
                ResolutionWidth = Screen.currentResolution.width > 0 ? Screen.currentResolution.width : 1920;
                ResolutionHeight = Screen.currentResolution.height > 0 ? Screen.currentResolution.height : 1080;
            }
        }

        public Difficulty Difficulty => (Difficulty)MathLite.Clamp(DefaultDifficulty, 0, 2);

        public GameSettings Clone()
        {
            return (GameSettings)MemberwiseClone();
        }

        // ---------------- serialisation ----------------

        public JsonValue ToJson()
        {
            JsonValue root = JsonValue.NewObject();
            root.Add("version", CurrentVersion);

            JsonValue display = JsonValue.NewObject();
            display.Add("w", ResolutionWidth);
            display.Add("h", ResolutionHeight);
            display.Add("fullscreen", FullScreenMode);
            display.Add("monitor", MonitorIndex);
            display.Add("vsync", VSync);
            display.Add("fpsCap", FrameRateCap);
            display.Add("quality", QualityLevel);
            display.Add("brightness", Brightness);
            root.Add("display", display);

            JsonValue audio = JsonValue.NewObject();
            audio.Add("master", MasterVolume);
            audio.Add("music", MusicVolume);
            audio.Add("sfx", SfxVolume);
            audio.Add("voice", VoiceVolume);
            audio.Add("ambience", AmbienceVolume);
            audio.Add("muted", Muted);
            audio.Add("mono", MonoOutput);
            root.Add("audio", audio);

            JsonValue gameplay = JsonValue.NewObject();
            gameplay.Add("difficulty", DefaultDifficulty);
            gameplay.Add("autoConfirmDifficulty", AutoConfirmDifficulty);
            gameplay.Add("screenShake", ScreenShake);
            gameplay.Add("screenShakeScale", ScreenShakeScale);
            gameplay.Add("hitStop", HitStop);
            gameplay.Add("damageNumbers", DamageNumbers);
            gameplay.Add("aimGuide", AimGuide);
            gameplay.Add("aimSource", AimSource);
            gameplay.Add("aimAssist", AimAssistStrength);
            gameplay.Add("invertY", InvertY);
            gameplay.Add("deadzone", StickDeadzone);
            gameplay.Add("toggleAim", ToggleAim);
            gameplay.Add("autoSaveCheckpoint", AutoSaveOnCheckpoint);
            gameplay.Add("autoSaveSavepoint", AutoSaveOnSavepoint);
            gameplay.Add("pauseOnFocusLost", PauseOnFocusLost);
            gameplay.Add("enemyHealthBars", ShowEnemyHealthBars);
            gameplay.Add("comicAutoAdvance", ComicAutoAdvanceSeconds);
            root.Add("gameplay", gameplay);

            JsonValue access = JsonValue.NewObject();
            access.Add("reducedFlashing", ReducedFlashing);
            access.Add("highContrast", HighContrastUi);
            access.Add("colorblind", ColorblindSafePalette);
            access.Add("largeText", LargeText);
            access.Add("hudScale", HudScale);
            access.Add("subtitles", Subtitles);
            access.Add("speakerNames", SubtitleSpeakerNames);
            access.Add("subtitleScale", SubtitleTextScale);
            access.Add("dialogueSpeed", DialogueSpeed);
            access.Add("extraLives", ExtraLives);
            access.Add("forgivingDamage", HonoursDamageReduction);
            access.Add("slowBosses", SlowDownBosses);
            access.Add("aimSnap", AimSnapToTargets);
            access.Add("comicSkipHint", ComicSkipHint);
            access.Add("autoAdvancePanels", AutoAdvanceComicPanels);
            root.Add("accessibility", access);

            return root;
        }

        public static GameSettings FromJson(JsonValue root)
        {
            GameSettings settings = new GameSettings();
            if (root == null || root.Kind != JsonKind.Object) return settings;

            JsonValue display = root["display"];
            settings.ResolutionWidth = display["w"].AsInt(settings.ResolutionWidth);
            settings.ResolutionHeight = display["h"].AsInt(settings.ResolutionHeight);
            settings.FullScreenMode = display["fullscreen"].AsInt(settings.FullScreenMode);
            settings.MonitorIndex = display["monitor"].AsInt(settings.MonitorIndex);
            settings.VSync = display["vsync"].AsBool(settings.VSync);
            settings.FrameRateCap = display["fpsCap"].AsInt(settings.FrameRateCap);
            settings.QualityLevel = display["quality"].AsInt(settings.QualityLevel);
            settings.Brightness = display["brightness"].AsFloat(settings.Brightness);

            JsonValue audio = root["audio"];
            settings.MasterVolume = audio["master"].AsFloat(settings.MasterVolume);
            settings.MusicVolume = audio["music"].AsFloat(settings.MusicVolume);
            settings.SfxVolume = audio["sfx"].AsFloat(settings.SfxVolume);
            settings.VoiceVolume = audio["voice"].AsFloat(settings.VoiceVolume);
            settings.AmbienceVolume = audio["ambience"].AsFloat(settings.AmbienceVolume);
            settings.Muted = audio["muted"].AsBool(settings.Muted);
            settings.MonoOutput = audio["mono"].AsBool(settings.MonoOutput);

            JsonValue gameplay = root["gameplay"];
            settings.DefaultDifficulty = gameplay["difficulty"].AsInt(settings.DefaultDifficulty);
            settings.AutoConfirmDifficulty = gameplay["autoConfirmDifficulty"].AsBool(settings.AutoConfirmDifficulty);
            settings.ScreenShake = gameplay["screenShake"].AsBool(settings.ScreenShake);
            settings.ScreenShakeScale = gameplay["screenShakeScale"].AsFloat(settings.ScreenShakeScale);
            settings.HitStop = gameplay["hitStop"].AsBool(settings.HitStop);
            settings.DamageNumbers = gameplay["damageNumbers"].AsBool(settings.DamageNumbers);
            settings.AimGuide = gameplay["aimGuide"].AsBool(settings.AimGuide);
            settings.AimSource = gameplay["aimSource"].AsInt(settings.AimSource);
            settings.AimAssistStrength = gameplay["aimAssist"].AsFloat(settings.AimAssistStrength);
            settings.InvertY = gameplay["invertY"].AsBool(settings.InvertY);
            settings.StickDeadzone = gameplay["deadzone"].AsFloat(settings.StickDeadzone);
            settings.ToggleAim = gameplay["toggleAim"].AsBool(settings.ToggleAim);
            settings.AutoSaveOnCheckpoint = gameplay["autoSaveCheckpoint"].AsBool(settings.AutoSaveOnCheckpoint);
            settings.AutoSaveOnSavepoint = gameplay["autoSaveSavepoint"].AsBool(settings.AutoSaveOnSavepoint);
            settings.PauseOnFocusLost = gameplay["pauseOnFocusLost"].AsBool(settings.PauseOnFocusLost);
            settings.ShowEnemyHealthBars = gameplay["enemyHealthBars"].AsBool(settings.ShowEnemyHealthBars);
            settings.ComicAutoAdvanceSeconds = gameplay["comicAutoAdvance"].AsFloat(settings.ComicAutoAdvanceSeconds);

            JsonValue access = root["accessibility"];
            settings.ReducedFlashing = access["reducedFlashing"].AsBool(settings.ReducedFlashing);
            settings.HighContrastUi = access["highContrast"].AsBool(settings.HighContrastUi);
            settings.ColorblindSafePalette = access["colorblind"].AsBool(settings.ColorblindSafePalette);
            settings.LargeText = access["largeText"].AsBool(settings.LargeText);
            settings.HudScale = access["hudScale"].AsInt(settings.HudScale);
            settings.Subtitles = access["subtitles"].AsBool(settings.Subtitles);
            settings.SubtitleSpeakerNames = access["speakerNames"].AsBool(settings.SubtitleSpeakerNames);
            settings.SubtitleTextScale = access["subtitleScale"].AsFloat(settings.SubtitleTextScale);
            settings.DialogueSpeed = access["dialogueSpeed"].AsFloat(settings.DialogueSpeed);
            settings.ExtraLives = access["extraLives"].AsInt(settings.ExtraLives);
            settings.HonoursDamageReduction = access["forgivingDamage"].AsBool(settings.HonoursDamageReduction);
            settings.SlowDownBosses = access["slowBosses"].AsBool(settings.SlowDownBosses);
            settings.AimSnapToTargets = access["aimSnap"].AsBool(settings.AimSnapToTargets);
            settings.ComicSkipHint = access["comicSkipHint"].AsBool(settings.ComicSkipHint);
            settings.AutoAdvanceComicPanels = access["autoAdvancePanels"].AsBool(settings.AutoAdvanceComicPanels);

            settings.Sanitize();
            return settings;
        }
    }

    /// <summary>Loads and saves settings, and applies them to the engine.</summary>
    public sealed class SettingsService
    {
        public const string FileName = "settings.json";

        private GameSettings _settings = new GameSettings();
        private List<Resolution> _resolutions;

        public GameSettings Settings => _settings;

        public event Action Changed;

        public void Initialize()
        {
            Load();
            Apply();
        }

        public string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public void Load()
        {
            try
            {
                if (System.IO.File.Exists(Path))
                {
                    string text = System.IO.File.ReadAllText(Path);
                    if (JsonValue.TryParse(text, out JsonValue root, out string error))
                    {
                        _settings = GameSettings.FromJson(root);
                    }
                    else
                    {
                        Debug.LogWarning("[IronVeil] Settings file unreadable (" + error + "), using defaults.");
                        _settings = new GameSettings();
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[IronVeil] Could not read settings: " + exception.Message);
                _settings = new GameSettings();
            }

            _settings.Sanitize();
        }

        public void Save()
        {
            try
            {
                System.IO.File.WriteAllText(Path, _settings.ToJson().ToJson(true));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[IronVeil] Could not write settings: " + exception.Message);
            }
        }

        /// <summary>Call after mutating Settings so the change is stored and applied.</summary>
        public void Commit()
        {
            _settings.Sanitize();
            Apply();
            Save();
            Changed?.Invoke();
        }

        /// <summary>
        /// Applies display, refresh and quality settings. Resolution is deliberately
        /// only applied when the requested size is one the current monitor reports,
        /// so a stale settings file cannot push the game off screen.
        /// </summary>
        public void Apply()
        {
            GameSettings settings = _settings;

            if (settings.QualityLevel >= 0 && settings.QualityLevel < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(settings.QualityLevel, true);
            }
            else
            {
                string[] names = QualitySettings.names;
                if (names.Length > 1)
                {
                    QualitySettings.SetQualityLevel(names.Length - 1, true);
                    settings.QualityLevel = names.Length - 1;
                }
            }

            QualitySettings.vSyncCount = settings.VSync ? 1 : 0;
            Application.targetFrameRate = settings.VSync || settings.FrameRateCap <= 0 ? -1 : settings.FrameRateCap;

            FullScreenMode mode = (FullScreenMode)settings.FullScreenMode;
            bool resolutionIsSupported = IsSupported(settings.ResolutionWidth, settings.ResolutionHeight);
            if (resolutionIsSupported && (Screen.width != settings.ResolutionWidth || Screen.height != settings.ResolutionHeight || Screen.fullScreenMode != mode))
            {
                Screen.SetResolution(settings.ResolutionWidth, settings.ResolutionHeight, mode);
            }
            else if (Screen.fullScreenMode != mode)
            {
                Screen.fullScreenMode = mode;
            }

            AudioService audio = Services.Get<AudioService>();
            if (audio != null)
            {
                audio.SetVolume(AudioChannel.Master, settings.MasterVolume);
                audio.SetVolume(AudioChannel.Music, settings.MusicVolume);
                audio.SetVolume(AudioChannel.Sfx, settings.SfxVolume);
                audio.SetVolume(AudioChannel.Voice, settings.VoiceVolume);
                audio.SetMuted(settings.Muted);
                audio.SetMonoOutput(settings.MonoOutput);
            }
        }

        private bool IsSupported(int width, int height)
        {
            if (width <= 0 || height <= 0) return false;
            List<Resolution> resolutions = Resolutions();
            for (int i = 0; i < resolutions.Count; i++)
            {
                if (resolutions[i].width == width && resolutions[i].height == height) return true;
            }
            // Windowed mode can use sizes the monitor does not advertise.
            return (FullScreenMode)_settings.FullScreenMode == UnityEngine.FullScreenMode.Windowed;
        }

        /// <summary>Distinct resolutions reported by the current monitor, largest first.</summary>
        public List<Resolution> Resolutions()
        {
            if (_resolutions != null) return _resolutions;

            _resolutions = new List<Resolution>();
            HashSet<string> seen = new HashSet<string>();
            Resolution[] reported = Screen.resolutions;
            for (int i = reported.Length - 1; i >= 0; i--)
            {
                string key = reported[i].width + "x" + reported[i].height;
                if (seen.Add(key)) _resolutions.Add(reported[i]);
            }
            if (_resolutions.Count == 0)
            {
                _resolutions.Add(new Resolution { width = 1280, height = 720 });
                _resolutions.Add(new Resolution { width = 1920, height = 1080 });
                _resolutions.Add(new Resolution { width = 2560, height = 1440 });
            }
            return _resolutions;
        }

        public string ResolutionLabel(Resolution resolution)
        {
            return resolution.width + " x " + resolution.height;
        }

        /// <summary>Picks the resolution closest to the current window, for the menu default.</summary>
        public Resolution CurrentResolution()
        {
            List<Resolution> resolutions = Resolutions();
            if (_settings.ResolutionWidth > 0)
            {
                for (int i = 0; i < resolutions.Count; i++)
                {
                    if (resolutions[i].width == _settings.ResolutionWidth && resolutions[i].height == _settings.ResolutionHeight)
                    {
                        return resolutions[i];
                    }
                }
            }
            for (int i = 0; i < resolutions.Count; i++)
            {
                if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height) return resolutions[i];
            }
            return resolutions[0];
        }

        public static string FullScreenLabel(int mode)
        {
            switch ((FullScreenMode)mode)
            {
                case UnityEngine.FullScreenMode.ExclusiveFullScreen: return "Exclusive Fullscreen";
                case UnityEngine.FullScreenMode.FullScreenWindow: return "Fullscreen Window";
                case UnityEngine.FullScreenMode.MaximizedWindow: return "Maximized Window";
                default: return "Windowed";
            }
        }

        public void ResetAudioDefaults()
        {
            _settings.MasterVolume = 0.8f;
            _settings.MusicVolume = 0.7f;
            _settings.SfxVolume = 0.9f;
            _settings.VoiceVolume = 1f;
            _settings.AmbienceVolume = 0.6f;
            _settings.Muted = false;
            Commit();
        }

        public void ResetAccessibilityDefaults()
        {
            GameSettings defaults = new GameSettings();
            _settings.ReducedFlashing = defaults.ReducedFlashing;
            _settings.HighContrastUi = defaults.HighContrastUi;
            _settings.ColorblindSafePalette = defaults.ColorblindSafePalette;
            _settings.LargeText = defaults.LargeText;
            _settings.HudScale = defaults.HudScale;
            _settings.Subtitles = defaults.Subtitles;
            _settings.SubtitleSpeakerNames = defaults.SubtitleSpeakerNames;
            _settings.SubtitleTextScale = defaults.SubtitleTextScale;
            _settings.DialogueSpeed = defaults.DialogueSpeed;
            _settings.HonoursDamageReduction = defaults.HonoursDamageReduction;
            _settings.SlowDownBosses = defaults.SlowDownBosses;
            _settings.AimSnapToTargets = defaults.AimSnapToTargets;
            _settings.ExtraLives = defaults.ExtraLives;
            Commit();
        }

        public void ResetAllToDefaults()
        {
            GameSettings defaults = new GameSettings();
            defaults.ResolutionWidth = _settings.ResolutionWidth;
            defaults.ResolutionHeight = _settings.ResolutionHeight;
            defaults.FullScreenMode = _settings.FullScreenMode;
            _settings = defaults;
            Commit();
        }
    }
}
