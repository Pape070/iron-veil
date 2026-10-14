using System;
using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;
using UnityEngine.UI;

namespace IronVeil
{
    /// <summary>Small helpers for laying screens out against the 1920x1080 reference canvas.</summary>
    public static class UiLayout
    {
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;

        public static RectTransform Column(Transform parent, string name, float width, float anchorX, float top)
        {
            RectTransform column = UiKit.Empty(name, parent);
            column.anchorMin = new Vector2(anchorX, 1f);
            column.anchorMax = new Vector2(anchorX, 1f);
            column.pivot = new Vector2(0.5f, 1f);
            column.sizeDelta = new Vector2(width, ReferenceHeight);
            column.anchoredPosition = new Vector2(0f, -top);
            return column;
        }

        public static RectTransform Place(RectTransform rect, float y, float height, float inset = 0f)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(inset, 0f);
            rect.offsetMax = new Vector2(-inset, 0f);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            rect.anchoredPosition = new Vector2(0f, -y);
            return rect;
        }

        public static RectTransform Inset(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        public static RectTransform Wrap(Component component)
        {
            RectTransform rect = (RectTransform)component.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            return rect;
        }

        /// <summary>Standard screen frame: dark backdrop plus a titled card.</summary>
        public static RectTransform Frame(ScreenBase screen, RectTransform parent, string title, string subtitle, out Text titleText)
        {
            Image backdrop = UiKit.Solid("Backdrop", parent, new Color(0.03f, 0.04f, 0.06f, 0.96f));
            UiKit.Stretch((RectTransform)backdrop.transform);

            RectTransform card = UiKit.Empty("Card", parent);
            UiKit.Anchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1560f, 900f));

            Image cardImage = UiKit.Panel("CardPanel", card, Palette.Panel, Palette.Border, 14);
            UiKit.Stretch((RectTransform)cardImage.transform);

            titleText = UiKit.Label("Title", card, title, 54, Palette.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Anchor((RectTransform)titleText.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -30f), new Vector2(1200f, 70f));

            Text sub = UiKit.Label("Subtitle", card, subtitle, 26, Palette.TextDim, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)sub.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -102f), new Vector2(1200f, 40f));

            Image divider = UiKit.Solid("Divider", card, new Color(1f, 1f, 1f, 0.08f));
            UiKit.Anchor((RectTransform)divider.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1464f, 2f));

            return card;
        }
    }

    public sealed class MainMenuScreen : ScreenBase
    {
        private UiButton _continue;
        private Text _footer;

        public MainMenuScreen()
        {
            Id = ScreenId.MainMenu;
        }

        protected override void Build()
        {
            GameRoot root = GameRoot.Instance;
            ContentDatabase content = root?.Content;
            string subtitle = content?.Content == null
                ? string.Empty
                : content.Content.Subtitle + "   |   " + ChapterRules.ChapterCount + " chapters, " +
                  ChapterRules.TotalMinutes() / 60 + " hours";

            RectTransform card = UiLayout.Frame(this, Container, "IRON VEIL", subtitle, out _);
            RectTransform column = UiLayout.Column(card, "Menu", 560f, 0f, 210f);
            UiLayout.Inset(column, 0f, 0f, 0f, 0f);
            column.anchorMin = new Vector2(0.5f, 1f);
            column.anchorMax = new Vector2(0.5f, 1f);
            column.anchoredPosition = new Vector2(0f, -210f);

            float y = 0f;
            const float height = 74f;
            const float gap = 16f;

            _continue = MakeButton(column, "Continue", y, height, () =>
            {
                SaveService saves = Services.Get<SaveService>();
                string slot = saves?.NewestSlotId();
                if (string.IsNullOrEmpty(slot))
                {
                    Services.Get<ScreenManager>()?.ShowMessage("No save found", "Start a new campaign first.");
                    return;
                }
                GameRoot.Instance?.ContinueGame(slot);
            });
            y += height + gap;

            MakeButton(column, "New Campaign", y, height, () =>
            {
                GameRoot.Instance?.OpenCharacterSelect();
            });
            y += height + gap;

            MakeButton(column, "Options", y, height, () => GameRoot.Instance?.OpenOptions(ScreenId.MainMenu));
            y += height + gap;

            MakeButton(column, "Controls", y, height, () => GameRoot.Instance?.OpenControls(ScreenId.MainMenu));
            y += height + gap;

            MakeButton(column, "Gallery", y, height, () => GameRoot.Instance?.OpenGallery());
            y += height + gap;

            MakeButton(column, "Codex", y, height, () => GameRoot.Instance?.OpenCodex());
            y += height + gap;

            MakeButton(column, "Quit", y, height, Quit);

            _footer = UiKit.Label("Footer", card, string.Empty, 22, Palette.TextDim, TextAnchor.LowerLeft);
            UiKit.Anchor((RectTransform)_footer.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(48f, 32f), new Vector2(1400f, 120f));

            Text hints = UiKit.Label("Hints", card,
                "Move " + Prompt("Move") + "   |   Aim " + Prompt("Aim") + "   |   Fire " + Prompt("Fire") +
                "   |   Abilities " + Prompt("AbilityPrimary") + " " + Prompt("AbilitySecondary") +
                "   |   Pause " + Prompt("Pause"),
                24, Palette.TextDim, TextAnchor.LowerRight);
            UiKit.Anchor((RectTransform)hints.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-48f, 32f), new Vector2(1100f, 80f));
        }

        private string Prompt(string action)
        {
            InputService input = Services.Get<InputService>();
            return input == null ? action : input.PromptGameplay(action);
        }

        private UiButton MakeButton(RectTransform parent, string label, float y, float height, Action onClick)
        {
            UiButton button = UiKit.Button("Btn_" + label, parent, label, onClick);
            UiLayout.Place(button.Rect, y, height);
            return Add(button);
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public override void OnShow()
        {
            SaveService saves = Services.Get<SaveService>();
            bool hasSave = !string.IsNullOrEmpty(saves?.NewestSlotId());
            if (_continue != null)
            {
                _continue.Enabled = hasSave;
                _continue.Label.color = hasSave ? Palette.Text : Palette.TextDim;
            }

            StoryService story = Services.Get<StoryService>();
            _footer.text = story != null && saves != null && saves.HasCurrent
                ? story.ProgressSummary()
                : "Press " + Prompt("Interact") + " to select.  All progress is saved automatically at checkpoints.";
            FocusFirst();
        }
    }

    public sealed class CharacterSelectScreen : ScreenBase
    {
        private CharacterId _selected = CharacterId.Kade;
        private int _difficulty = 1;
        private Text _details;
        private UiOptionRow _difficultyRow;
        private readonly List<UiButton> _characterButtons = new List<UiButton>();

        public CharacterSelectScreen()
        {
            Id = ScreenId.CharacterSelect;
        }

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "New Campaign",
                "Choose your operative and how hard Helion hits back.", out _);

            RectTransform column = UiLayout.Column(card, "Body", 1360f, 0.5f, 190f);
            column.anchorMin = new Vector2(0.5f, 1f);
            column.anchorMax = new Vector2(0.5f, 1f);
            column.anchoredPosition = new Vector2(0f, -190f);

            float y = 0f;
            MakeCharacterButton(column, CharacterId.Kade, y);
            y += 96f;
            MakeCharacterButton(column, CharacterId.Vera, y);
            y += 116f;

            _details = UiKit.Label("Details", column, string.Empty, 24, Palette.Text, TextAnchor.UpperLeft);
            UiLayout.Place((RectTransform)_details.transform, y, 260f, 20f);
            y += 280f;

            _difficultyRow = UiKit.OptionRow("Difficulty", column, "Difficulty", new[] { "Easy", "Normal", "Hard" }, _difficulty, index =>
            {
                _difficulty = index;
                UpdateDetails();
            });
            UiLayout.Place(_difficultyRow.Rect, y, 68f);
            Add(_difficultyRow);
            y += 90f;

            UiButton start = UiKit.Button("Start", column, "Deploy", () =>
            {
                GameRoot.Instance?.StartNewGame(_selected, (Difficulty)Mathf.Clamp(_difficulty, 0, 2));
            }, true);
            UiLayout.Place(start.Rect, y, 78f);
            Add(start);
            y += 96f;

            UiButton back = UiKit.Button("Back", column, "Back", () => Services.Get<ScreenManager>()?.Back());
            UiLayout.Place(back.Rect, y, 66f);
            Add(back);
        }

        private void MakeCharacterButton(RectTransform parent, CharacterId id, float y)
        {
            ContentDatabase content = GameRoot.Instance?.Content;
            CharacterDefinition character = content?.Character(id);
            string label = character == null
                ? id.ToString()
                : character.Name + "  \"" + character.Callsign + "\"  -  " + character.Role;

            UiButton button = UiKit.Button("Char_" + id, parent, label, () =>
            {
                _selected = id;
                UpdateDetails();
            }, id == _selected);
            UiLayout.Place(button.Rect, y, 80f);
            _characterButtons.Add(button);
            Add(button);
        }

        public override void OnShow()
        {
            _difficulty = Services.Get<SettingsService>()?.Settings.DefaultDifficulty ?? 1;
            if (_difficultyRow != null)
            {
                _difficultyRow.Index = Mathf.Clamp(_difficulty, 0, 2);
                _difficultyRow.Refresh();
            }
            UpdateDetails();
            FocusFirst();
        }

        private void UpdateDetails()
        {
            ContentDatabase content = GameRoot.Instance?.Content;
            CharacterDefinition character = content?.Character(_selected);

            // Keep the two character buttons showing which one is selected.
            for (int i = 0; i < _characterButtons.Count; i++)
            {
                bool isSelected = (i == 0 && _selected == CharacterId.Kade) || (i == 1 && _selected == CharacterId.Vera);
                _characterButtons[i].IsPrimary = isSelected;
                _characterButtons[i].SetHighlighted(false);
            }

            if (character == null || _details == null) return;

            StatBlock stats = character.BaseStats;
            DifficultyProfile profile = DifficultyProfile.For((Difficulty)Mathf.Clamp(_difficulty, 0, 2));

            _difficultiesApplied(profile, character);
        }

        private void _difficultiesApplied(DifficultyProfile profile, CharacterDefinition character)
        {
            WeaponDefinition signature = GameRoot.Instance?.Content?.Weapon(character.SignatureWeaponId);
            AbilityDefinition primary = character.PrimaryAbility;
            AbilityDefinition secondary = character.SecondaryAbility;
            PassiveDefinition passive = character.Passive;

            _details.text =
                character.Bio + "\n\n" +
                "<b>Signature weapon:</b> " + (signature == null ? "none" : signature.DisplayName + " - " + signature.Description) + "\n" +
                "<b>Primary:</b> " + (primary == null ? "none" : primary.DisplayName + " - " + primary.Description) + "\n" +
                "<b>Secondary (unlocks chapter " + character.SecondAbilityUnlockChapter + "):</b> " +
                (secondary == null ? "none" : secondary.DisplayName + " - " + secondary.Description) + "\n" +
                "<b>Passive:</b> " + (passive == null ? "none" : passive.DisplayName + " - " + passive.Description) + "\n\n" +
                "<b>Base stats:</b> Health " + Mathf.RoundToInt(stats.MaxHealth) + "   Shield " + Mathf.RoundToInt(stats.Shield) +
                "   Power " + Mathf.RoundToInt(stats.Power) + "   Defense " + Mathf.RoundToInt(stats.Defense) +
                "   Speed " + stats.MoveSpeed.ToString("0.0") + "\n" +
                "<b>" + profile.DisplayName + ":</b> " + profile.Summary;
        }
    }

    public sealed class OptionsScreen : ScreenBase
    {
        private enum Tab
        {
            Display = 0,
            Audio = 1,
            Gameplay = 2,
            Accessibility = 3
        }

        private Tab _tab = Tab.Display;
        private ScreenId _origin = ScreenId.MainMenu;
        private readonly List<UiWidget> _rows = new List<UiWidget>();
        private Text _note;
        private UiButton _back;
        private RectTransform _body;

        public OptionsScreen()
        {
            Id = ScreenId.Options;
        }

        public void SetOrigin(ScreenId origin) => _origin = origin;

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "Options", "Changes apply immediately and are saved to disk.", out _);

            RectTransform tabs = UiLayout.Column(card, "Tabs", 1360f, 0.5f, 170f);
            tabs.anchorMin = new Vector2(0.5f, 1f);
            tabs.anchorMax = new Vector2(0.5f, 1f);
            tabs.anchoredPosition = new Vector2(0f, -170f);

            string[] names = { "Display", "Audio", "Gameplay", "Accessibility" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                UiButton tab = UiKit.Button("Tab_" + names[i], tabs, names[i], () =>
                {
                    _tab = (Tab)index;
                    Rebuild();
                });
                RectTransform rect = tab.Rect;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(320f, 62f);
                rect.anchoredPosition = new Vector2(i * 336f, 0f);
                Add(tab);
            }

            _body = UiLayout.Column(card, "Body", 1360f, 0.5f, 260f);
            _body.anchorMin = new Vector2(0.5f, 1f);
            _body.anchorMax = new Vector2(0.5f, 1f);
            _body.anchoredPosition = new Vector2(0f, -260f);

            _note = UiKit.Label("Note", card, string.Empty, 22, Palette.TextDim, TextAnchor.LowerLeft);
            UiKit.Anchor((RectTransform)_note.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(48f, 104f), new Vector2(1000f, 60f));

            _back = UiKit.Button("Back", card, "Back", () => Services.Get<ScreenManager>()?.Back());
            UiKit.Anchor(_back.Rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-48f, 40f), new Vector2(260f, 64f));
            Add(_back);
        }

        public override void OnShow() => Rebuild();

        public override void Refresh() => Rebuild();

        private void Rebuild()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null) Destroy(_rows[i].gameObject);
            }
            _rows.Clear();

            SettingsService service = Services.Get<SettingsService>();
            GameSettings settings = service?.Settings;
            if (settings == null) return;

            float y = 0f;
            const float rowHeight = 62f;
            const float gap = 10f;

            switch (_tab)
            {
                case Tab.Display:
                {
                    List<Resolution> resolutions = service.Resolutions();
                    string[] labels = new string[resolutions.Count];
                    int current = 0;
                    Resolution active = service.CurrentResolution();
                    for (int i = 0; i < resolutions.Count; i++)
                    {
                        labels[i] = service.ResolutionLabel(resolutions[i]);
                        if (resolutions[i].width == active.width && resolutions[i].height == active.height) current = i;
                    }

                    y = AddRow(UiKit.OptionRow("Resolution", _body, "Resolution", labels, current, index =>
                    {
                        Resolution picked = resolutions[Mathf.Clamp(index, 0, resolutions.Count - 1)];
                        settings.ResolutionWidth = picked.width;
                        settings.ResolutionHeight = picked.height;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.OptionRow("Fullscreen", _body, "Display mode",
                        new[] { SettingsService.FullScreenLabel(0), SettingsService.FullScreenLabel(1), SettingsService.FullScreenLabel(2), SettingsService.FullScreenLabel(3) },
                        settings.FullScreenMode, index =>
                        {
                            settings.FullScreenMode = index;
                            service.Commit();
                        }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("Vsync", _body, "V-Sync", settings.VSync, value =>
                    {
                        settings.VSync = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.OptionRow("FrameCap", _body, "Frame rate cap",
                        new[] { "30", "60", "90", "120", "144", "165", "Unlimited" }, FrameCapIndex(settings.FrameRateCap), index =>
                        {
                            int[] caps = { 30, 60, 90, 120, 144, 165, 0 };
                            settings.FrameRateCap = caps[Mathf.Clamp(index, 0, caps.Length - 1)];
                            service.Commit();
                        }), y, rowHeight, gap);

                    string[] quality = QualitySettings.names;
                    if (quality.Length > 0)
                    {
                        y = AddRow(UiKit.OptionRow("Quality", _body, "Quality level", quality,
                            settings.QualityLevel < 0 ? quality.Length - 1 : Mathf.Clamp(settings.QualityLevel, 0, quality.Length - 1),
                            index =>
                            {
                                settings.QualityLevel = index;
                                service.Commit();
                            }), y, rowHeight, gap);
                    }

                    y = AddRow(UiKit.Slider("Brightness", _body, "Brightness", (settings.Brightness - 0.5f) / 1.5f, 0.05f,
                        value =>
                        {
                            settings.Brightness = 0.5f + value * 1.5f;
                            service.Commit();
                        },
                        value => (0.5f + value * 1.5f).ToString("0.00") + "x"), y, rowHeight, gap);

                    _note.text = "Resolution " + active.width + " x " + active.height + " at " + Screen.width + " x " + Screen.height + " window.";
                    break;
                }

                case Tab.Audio:
                    y = AddRow(UiKit.Slider("Master", _body, "Master volume", settings.MasterVolume, 0.05f, value =>
                    {
                        settings.MasterVolume = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Slider("Music", _body, "Music volume", settings.MusicVolume, 0.05f, value =>
                    {
                        settings.MusicVolume = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Slider("Sfx", _body, "Effects volume", settings.SfxVolume, 0.05f, value =>
                    {
                        settings.SfxVolume = value;
                        service.Commit();
                        Services.Get<AudioService>()?.PlaySfx("sfx_ui_click");
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Slider("Voice", _body, "Voice volume", settings.VoiceVolume, 0.05f, value =>
                    {
                        settings.VoiceVolume = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Slider("Ambience", _body, "Ambience volume", settings.AmbienceVolume, 0.05f, value =>
                    {
                        settings.AmbienceVolume = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("Mute", _body, "Mute all audio", settings.Muted, value =>
                    {
                        settings.Muted = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("Mono", _body, "Mono output (single channel)", settings.MonoOutput, value =>
                    {
                        settings.MonoOutput = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Button("AudioReset", _body, "Reset audio defaults", () => service.ResetAudioDefaults()), y, rowHeight, gap);
                    _note.text = "Mono output downmixes everything to one channel for single-speaker setups and headphone users who prefer it.";
                    break;

                case Tab.Gameplay:
                    y = AddRow(UiKit.OptionRow("Difficulty", _body, "Default difficulty", new[] { "Easy", "Normal", "Hard" },
                        settings.DefaultDifficulty, index =>
                        {
                            settings.DefaultDifficulty = index;
                            service.Commit();
                        }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("Shake", _body, "Screen shake", settings.ScreenShake, value =>
                    {
                        settings.ScreenShake = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Slider("ShakeScale", _body, "Screen shake strength", settings.ScreenShakeScale / 2f, 0.1f, value =>
                    {
                        settings.ScreenShakeScale = value * 2f;
                        service.Commit();
                    }, value => (value * 2f).ToString("0.0") + "x"), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("HitStop", _body, "Hit stop on impact", settings.HitStop, value =>
                    {
                        settings.HitStop = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("DamageNumbers", _body, "Damage numbers", settings.DamageNumbers, value =>
                    {
                        settings.DamageNumbers = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("AimGuide", _body, "Aim guide line", settings.AimGuide, value =>
                    {
                        settings.AimGuide = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.OptionRow("AimSource", _body, "Aim source",
                        new[] { "Auto", "Mouse cursor", "Right stick" }, settings.AimSource, index =>
                        {
                            settings.AimSource = index;
                            service.Commit();
                        }), y, rowHeight, gap);

                    y = AddRow(UiKit.Slider("AimAssist", _body, "Aim assist", settings.AimAssistStrength, 0.1f, value =>
                    {
                        settings.AimAssistStrength = value;
                        service.Commit();
                    }, value => Mathf.RoundToInt(value * 100f) + "%"), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("InvertY", _body, "Invert vertical aim", settings.InvertY, value =>
                    {
                        settings.InvertY = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Slider("Deadzone", _body, "Stick dead zone", (settings.StickDeadzone - 0.05f) / 0.55f, 0.05f, value =>
                    {
                        settings.StickDeadzone = 0.05f + value * 0.55f;
                        service.Commit();
                    }, value => (0.05f + value * 0.55f).ToString("0.00")), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("AutoSaveCheckpoint", _body, "Autosave at checkpoints", settings.AutoSaveOnCheckpoint, value =>
                    {
                        settings.AutoSaveOnCheckpoint = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("AutoSaveSavepoint", _body, "Autosave at savepoints", settings.AutoSaveOnSavepoint, value =>
                    {
                        settings.AutoSaveOnSavepoint = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("PauseFocus", _body, "Pause when the window loses focus", settings.PauseOnFocusLost, value =>
                    {
                        settings.PauseOnFocusLost = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("EnemyBars", _body, "Enemy health bars", settings.ShowEnemyHealthBars, value =>
                    {
                        settings.ShowEnemyHealthBars = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    _note.text = "Difficulty can be lowered at any savepoint but never raised mid-run, so records stay honest.";
                    break;

                default:
                    y = AddRow(UiKit.Toggle("ReducedFlash", _body, "Reduce flashing effects", settings.ReducedFlashing, value =>
                    {
                        settings.ReducedFlashing = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("HighContrast", _body, "High contrast UI", settings.HighContrastUi, value =>
                    {
                        settings.HighContrastUi = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("Colorblind", _body, "Colour-blind safe palette", settings.ColorblindSafePalette, value =>
                    {
                        settings.ColorblindSafePalette = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("LargeText", _body, "Large text", settings.LargeText, value =>
                    {
                        settings.LargeText = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.OptionRow("HudScale", _body, "HUD size", new[] { "Compact", "Normal", "Large" }, settings.HudScale, index =>
                    {
                        settings.HudScale = index;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Slider("SubtitleScale", _body, "Subtitle text size", (settings.SubtitleTextScale - 0.8f) / 1f, 0.1f, value =>
                    {
                        settings.SubtitleTextScale = 0.8f + value;
                        service.Commit();
                    }, value => (0.8f + value).ToString("0.0") + "x"), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("Subtitles", _body, "Subtitles", settings.Subtitles, value =>
                    {
                        settings.Subtitles = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Slider("DialogueSpeed", _body, "Dialogue speed", (settings.DialogueSpeed - 0.5f) / 1.5f, 0.1f, value =>
                    {
                        settings.DialogueSpeed = 0.5f + value * 1.5f;
                        service.Commit();
                    }, value => (0.5f + value * 1.5f).ToString("0.0") + "x"), y, rowHeight, gap);

                    y = AddRow(UiKit.OptionRow("ExtraLives", _body, "Extra lives", new[] { "None", "1", "2", "3" }, settings.ExtraLives, index =>
                    {
                        settings.ExtraLives = index;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("Forgiving", _body, "Reduced incoming damage", settings.HonoursDamageReduction, value =>
                    {
                        settings.HonoursDamageReduction = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("SlowBosses", _body, "Slower boss patterns", settings.SlowDownBosses, value =>
                    {
                        settings.SlowDownBosses = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("AimSnap", _body, "Aim snaps to nearby targets", settings.AimSnapToTargets, value =>
                    {
                        settings.AimSnapToTargets = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Toggle("AutoComic", _body, "Auto-advance comic panels", settings.AutoAdvanceComicPanels, value =>
                    {
                        settings.AutoAdvanceComicPanels = value;
                        service.Commit();
                    }), y, rowHeight, gap);

                    y = AddRow(UiKit.Button("AccessReset", _body, "Reset accessibility defaults", () => service.ResetAccessibilityDefaults()), y, rowHeight, gap);
                    _note.text = "Accessibility options never disable achievements or change story content.";
                    break;
            }

            FocusFirst();
        }

        private static int FrameCapIndex(int cap)
        {
            int[] caps = { 30, 60, 90, 120, 144, 165, 0 };
            for (int i = 0; i < caps.Length; i++)
            {
                if (caps[i] == cap) return i;
            }
            return 6;
        }

        private float AddRow(UiWidget widget, float y, float height, float gap)
        {
            UiLayout.Place(widget.Rect, y, height);
            _rows.Add(widget);
            Add(widget);
            return y + height + gap;
        }
    }

    public sealed class ControlsScreen : ScreenBase
    {
        private readonly List<UiWidget> _rows = new List<UiWidget>();
        private RectTransform _body;
        private Text _status;
        private UiOptionRow _deviceRow;
        private InputDeviceKind _scheme = InputDeviceKind.KeyboardMouse;
        private ScreenId _origin = ScreenId.MainMenu;
        private int _scrollOffset;
        private int _maxVisible = 11;
        private string _activeMap = ScreenManagerMap.Gameplay;

        private static class ScreenManagerMap
        {
            public const string Gameplay = "Gameplay";
            public const string Ui = "UI";
        }

        public ControlsScreen()
        {
            Id = ScreenId.Controls;
        }

        public void SetOrigin(ScreenId origin) => _origin = origin;

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "Controls",
                "Every action is rebindable on both keyboard/mouse and gamepad.", out _);

            _deviceRow = UiKit.OptionRow("Scheme", card, "Binding set", new[] { "Keyboard + Mouse", "Gamepad" }, 0, index =>
            {
                _scheme = index == 0 ? InputDeviceKind.KeyboardMouse : InputDeviceKind.Gamepad;
                _scrollOffset = 0;
                Rebuild();
            });
            UiKit.Anchor(_deviceRow.Rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(1360f, 60f));
            Add(_deviceRow);

            _body = UiLayout.Column(card, "Body", 1360f, 0.5f, 250f);
            _body.anchorMin = new Vector2(0.5f, 1f);
            _body.anchorMax = new Vector2(0.5f, 1f);
            _body.anchoredPosition = new Vector2(0f, -250f);

            UiButton gameplay = UiKit.Button("MapGameplay", card, "Gameplay map", () =>
            {
                _activeMap = ScreenManagerMap.Gameplay;
                _scrollOffset = 0;
                Rebuild();
            });
            UiKit.Anchor(gameplay.Rect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(48f, 40f), new Vector2(240f, 58f));
            Add(gameplay);

            UiButton ui = UiKit.Button("MapUi", card, "UI map", () =>
            {
                _activeMap = ScreenManagerMap.Ui;
                _scrollOffset = 0;
                Rebuild();
            });
            UiKit.Anchor(ui.Rect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(300f, 40f), new Vector2(240f, 58f));
            Add(ui);

            UiButton resetAll = UiKit.Button("ResetAll", card, "Reset all bindings", () =>
            {
                Services.Get<InputRebinder>()?.ResetAll();
                Services.Get<ScreenManager>()?.ShowMessage("Controls reset", "All bindings are back to their defaults.");
                Rebuild();
            });
            UiKit.Anchor(resetAll.Rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-48f, 40f), new Vector2(300f, 58f));
            Add(resetAll);

            UiButton back = UiKit.Button("Back", card, "Back", () => Services.Get<ScreenManager>()?.Back());
            UiKit.Anchor(back.Rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-368f, 40f), new Vector2(240f, 58f));
            Add(back);

            _status = UiKit.Label("Status", card, string.Empty, 22, Palette.Accent, TextAnchor.LowerCenter);
            UiKit.Anchor((RectTransform)_status.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 106f), new Vector2(1000f, 40f));
        }

        public override void OnShow()
        {
            InputRebinder rebinder = Services.Get<InputRebinder>();
            if (rebinder != null)
            {
                rebinder.RebindFinished -= HandleRebindFinished;
                rebinder.RebindFinished += HandleRebindFinished;
            }
            Rebuild();
        }

        public override void OnHide()
        {
            Services.Get<InputRebinder>()?.Cancel();
        }

        private void HandleRebindFinished(bool success, string message)
        {
            if (_status != null) _status.text = message ?? string.Empty;
            Rebuild();
        }

        private void Rebuild()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null) Destroy(_rows[i].gameObject);
            }
            _rows.Clear();

            InputRebinder rebinder = Services.Get<InputRebinder>();
            List<RebindEntry> entries = rebinder?.Entries(_activeMap, _scheme) ?? new List<RebindEntry>();

            if (_status != null)
            {
                _status.text = entries.Count + " bindings   |   " + (rebinder?.OverrideCount() ?? 0) + " customised";
            }

            int count = Mathf.Min(_maxVisible, entries.Count);
            for (int i = 0; i < count; i++)
            {
                int index = _scrollOffset + i;
                if (index >= entries.Count) break;

                RebindEntry entry = entries[index];
                int captured = index;
                UiButton row = UiKit.Button("Bind_" + entry.ActionName + "_" + entry.BindingIndex,
                    _body,
                    entry.ActionLabel + "  [" + entry.SlotLabel + "]",
                    () => StartRebind(captured, entries));
                UiLayout.Place(row.Rect, i * 62f, 54f);
                _rows.Add(row);
                Add(row);

                Text value = UiKit.Label("Value", row.transform, entry.CurrentLabel + (entry.IsOverridden ? " *" : string.Empty),
                    24, entry.IsOverridden ? Palette.Accent : Palette.TextDim, TextAnchor.MiddleRight);
                UiKit.Anchor((RectTransform)value.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(420f, 40f));
            }

            FocusFirst();
        }

        private void StartRebind(int entryIndex, List<RebindEntry> entries)
        {
            if (entryIndex < 0 || entryIndex >= entries.Count) return;
            InputRebinder rebinder = Services.Get<InputRebinder>();
            if (rebinder == null) return;

            if (_status != null) _status.text = "Listening for input... press a key or button, " + Prompt("Cancel") + " to cancel.";

            bool started = rebinder.Begin(entries[entryIndex]);
            if (!started) Rebuild();
        }

        private string Prompt(string action)
        {
            InputService input = Services.Get<InputService>();
            return input == null ? action : input.PromptUi(action);
        }

        public override bool OnCancel()
        {
            InputRebinder rebinder = Services.Get<InputRebinder>();
            if (rebinder != null && rebinder.IsRebinding)
            {
                rebinder.Cancel();
                return false;
            }
            return true;
        }
    }

    public sealed class PauseScreen : ScreenBase
    {
        private Text _summary;

        public PauseScreen()
        {
            Id = ScreenId.Pause;
        }

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "Paused", "Helion does not wait, but it will.", out _);
            RectTransform column = UiLayout.Column(card, "Menu", 560f, 0.5f, 210f);
            column.anchorMin = new Vector2(0.5f, 1f);
            column.anchorMax = new Vector2(0.5f, 1f);
            column.anchoredPosition = new Vector2(0f, -210f);

            float y = 0f;
            const float height = 70f;
            const float gap = 14f;

            y = AddButton(column, "Resume", y, height, gap, () => Services.Get<ScreenManager>()?.Back());
            y = AddButton(column, "Save now", y, height, gap, () =>
            {
                Services.Get<SaveService>()?.SaveAutosave("manual");
                Services.Get<ScreenManager>()?.ShowMessage("Saved", "Progress written to the autosave slot.");
            });
            y = AddButton(column, "Quest log", y, height, gap, () => GameRoot.Instance?.OpenQuestLog());
            y = AddButton(column, "Options", y, height, gap, () => GameRoot.Instance?.OpenOptions(ScreenId.Pause));
            y = AddButton(column, "Controls", y, height, gap, () => GameRoot.Instance?.OpenControls(ScreenId.Pause));
            y = AddButton(column, "Gallery", y, height, gap, () => GameRoot.Instance?.OpenGallery());
            y = AddButton(column, "Main menu", y, height, gap, () =>
            {
                Services.Get<ScreenManager>()?.HideAll();
                GameRoot.Instance?.ReturnToMainMenu();
            });

            _summary = UiKit.Label("Summary", card, string.Empty, 24, Palette.TextDim, TextAnchor.LowerLeft);
            UiKit.Anchor((RectTransform)_summary.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(48f, 32f), new Vector2(1300f, 60f));
        }

        private float AddButton(RectTransform parent, string label, float y, float height, float gap, Action onClick)
        {
            UiButton button = UiKit.Button("Btn_" + label, parent, label, onClick);
            UiLayout.Place(button.Rect, y, height);
            Add(button);
            return y + height + gap;
        }

        public override void OnShow() => Refresh();

        public override void Refresh()
        {
            SaveService saves = Services.Get<SaveService>();
            ProgressionService progression = Services.Get<ProgressionService>();
            if (_summary != null && saves?.Current != null)
            {
                _summary.text = saves.Current.Summary(Services.Get<ContentDatabase>()?.Content) +
                                "   |   " + progression?.LevelSummary();
            }
        }
    }

    public sealed class SavepointScreen : ScreenBase
    {
        private SavepointDefinition _savepoint;
        private readonly List<UiWidget> _rows = new List<UiWidget>();
        private RectTransform _body;
        private Text _header;

        public SavepointScreen()
        {
            Id = ScreenId.Savepoint;
        }

        public void Set(SavepointDefinition savepoint) => _savepoint = savepoint;

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "Savepoint",
                "Rest, spend points, tune your loadout. Lowering difficulty here is permanent for this save.", out _);

            _header = UiKit.Label("Header", card, string.Empty, 24, Palette.Accent, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_header.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -170f), new Vector2(1300f, 40f));

            _body = UiLayout.Column(card, "Body", 1360f, 0.5f, 220f);
            _body.anchorMin = new Vector2(0.5f, 1f);
            _body.anchorMax = new Vector2(0.5f, 1f);
            _body.anchoredPosition = new Vector2(0f, -220f);

            UiButton continueButton = UiKit.Button("Continue", card, "Continue", () => Services.Get<ScreenManager>()?.Back(), true);
            UiKit.Anchor(continueButton.Rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-48f, 40f), new Vector2(280f, 64f));
            Add(continueButton);
        }

        public override void OnShow() => Rebuild();

        private void Rebuild()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null) Destroy(_rows[i].gameObject);
            }
            _rows.Clear();

            SaveService saves = Services.Get<SaveService>();
            ProgressionService progression = Services.Get<ProgressionService>();
            SaveGame save = saves?.Current;

            if (_header != null && save != null)
            {
                _header.text = "Savepoint: " + (_savepoint == null ? "field station" : _savepoint.Label) +
                               "   |   Chapter " + save.ChapterIndex + "   |   Level " + save.Level +
                               "   |   Salvage " + save.Salvage;
            }

            float y = 0f;
            const float height = 62f;
            const float gap = 10f;

            y = AddRow(UiKit.Button("Rest", _body, "Rest - heal, refill flasks and save",
                () =>
                {
                    SavePointRest();
                }, true), y, height + 8f, gap);

            y = AddRow(UiKit.Button("LevelUp", _body, "Level up - spend " + (progression?.AvailablePoints ?? 0) + " points",
                () => GameRoot.Instance?.OpenLevelUpScreen()), y, height, gap);

            // Weapon upgrades.
            List<WeaponDefinition> weapons = progression?.OwnedWeapons() ?? new List<WeaponDefinition>();
            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponDefinition weapon = weapons[i];
                int level = progression.WeaponUpgradeLevel(weapon.Id);
                bool maxed = level >= weapon.MaxUpgradeLevel;
                int cost = maxed ? 0 : WeaponMath.NextUpgradeCost(weapon, level);
                string label = maxed
                    ? weapon.DisplayName + "  +" + level + "  (max)"
                    : weapon.DisplayName + "  +" + level + "  ->  +" + (level + 1) + "   (" + cost + " salvage)";

                y = AddRow(UiKit.Button("Upgrade_" + weapon.Id, _body, label, () =>
                {
                    if (progression.TryUpgradeWeapon(weapon.Id, out string message))
                    {
                        Services.Get<ScreenManager>()?.ShowMessage("Weapon upgraded", message);
                    }
                    else
                    {
                        Services.Get<ScreenManager>()?.ShowMessage("Cannot upgrade", message);
                    }
                    Rebuild();
                }), y, height - 6f, gap);
            }

            if (progression != null && progression.AvailablePoints < StatComputation.PointsForLevel(progression.Level))
            {
                y = AddRow(UiKit.Button("Respec", _body,
                    "Respec all stat points (" + ProgressionService.RespecCost + " salvage)",
                    () =>
                    {
                        bool ok = progression.Respec(out string message);
                        Services.Get<ScreenManager>()?.ShowMessage(ok ? "Respec complete" : "Respec failed", message);
                        Rebuild();
                    }), y, height, gap);
            }

            y = AddRow(UiKit.Button("Difficulty", _body, "Lower difficulty (permanent for this save)", () =>
            {
                SaveGame current = saves?.Current;
                if (current == null) return;
                if (current.Difficulty <= IronVeil.Rules.Difficulty.Easy)
                {
                    Services.Get<ScreenManager>()?.ShowMessage("Difficulty", "Already at the lowest difficulty.");
                    return;
                }
                current.Difficulty -= 1;
                saves.SaveAutosave("difficulty_lowered");
                Services.Get<ScreenManager>()?.ShowMessage("Difficulty lowered",
                    "Now " + DifficultyProfile.For(current.Difficulty).DisplayName + ". It cannot be raised again on this save.");
                Rebuild();
            }), y, height, gap);

            FocusFirst();
        }

        private void SavePointRest()
        {
            SaveService saves = Services.Get<SaveService>();
            ChapterRuntime chapter = GameRoot.Instance?.Chapter;
            chapter?.Player?.RefillFlasks(3);
            chapter?.HealPlayer();
            saves?.SaveAutosave("savepoint");
            Services.Get<ScreenManager>()?.ShowMessage("Rested", "Health restored, flasks refilled, progress saved.");
        }

        private float AddRow(UiWidget widget, float y, float height, float gap)
        {
            UiLayout.Place(widget.Rect, y, height);
            _rows.Add(widget);
            Add(widget);
            return y + height + gap;
        }
    }

    /// <summary>One stat in the level-up list: left refunds a point, right invests one.</summary>
    public sealed class UiStatRow : UiWidget
    {
        public Text Label;
        public Text Value;
        public Image Background;
        public StatId Stat;
        public ProgressionService Progression;
        public Action OnChanged;

        public override void OnAdjust(int delta)
        {
            if (Progression == null || delta == 0) return;
            bool changed = delta > 0 ? Progression.SpendPoint(Stat) : Progression.RefundPoint(Stat);
            if (!changed) return;
            Refresh();
            OnChanged?.Invoke();
        }

        public override void OnSubmit() => OnAdjust(1);

        public void Refresh()
        {
            if (Progression == null || Label == null) return;
            StatBlock stats = Progression.Stats;
            int invested = Progression.SpentIn(Stat);
            float current = StatCatalog.Get(stats, Stat);
            string formatted = StatCatalog.IsPercent(Stat)
                ? (current * 100f).ToString("0.#") + "%"
                : current.ToString("0.#");
            Label.text = StatCatalog.DisplayName(Stat) + "   " + formatted;
            Label.color = invested > 0 ? Palette.Text : Palette.Text;

            if (Value != null)
            {
                bool canAdd = Progression.CanInvest(Stat);
                bool canRemove = invested > 0;
                Value.text = "[ " + (canRemove ? "-" : " ") + " " + invested + " " + (canAdd ? "+" : " ") + " ]";
                Value.color = canAdd ? Palette.Accent : Palette.TextDim;
            }
        }

        public override void SetHighlighted(bool highlighted)
        {
            if (Background != null)
            {
                Background.color = highlighted ? new Color(0.28f, 0.34f, 0.46f, 0.98f) : new Color(0.13f, 0.15f, 0.2f, 0.9f);
            }
            if (Label != null && highlighted) Label.color = Palette.Accent;
        }
    }

    public sealed class LevelUpScreen : ScreenBase
    {
        private LevelUpResult _result;
        private readonly List<UiWidget> _rows = new List<UiWidget>();
        private RectTransform _body;
        private Text _header;
        private Text _perks;

        public LevelUpScreen()
        {
            Id = ScreenId.LevelUp;
        }

        public void Set(LevelUpResult result) => _result = result;

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "Level Up",
                "Spend points on the stats that suit your run. Points can be refunded at any savepoint.", out _);

            _header = UiKit.Label("Header", card, string.Empty, 30, Palette.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_header.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -172f), new Vector2(1300f, 44f));

            _body = UiLayout.Column(card, "Body", 900f, 0f, 230f);
            _body.anchorMin = new Vector2(0f, 1f);
            _body.anchorMax = new Vector2(0f, 1f);
            _body.pivot = new Vector2(0f, 1f);
            _body.anchoredPosition = new Vector2(48f, -230f);

            _perks = UiKit.Label("Perks", card, string.Empty, 24, Palette.Text, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_perks.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-48f, -230f), new Vector2(560f, 620f));

            UiButton done = UiKit.Button("Done", card, "Back to the fight", () => Services.Get<ScreenManager>()?.Back(), true);
            UiKit.Anchor(done.Rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-48f, 40f), new Vector2(340f, 64f));
            Add(done);
        }

        public override void OnShow() => Rebuild();

        public override void Refresh() => Rebuild();

        private void Rebuild()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null) Destroy(_rows[i].gameObject);
            }
            _rows.Clear();

            ProgressionService progression = Services.Get<ProgressionService>();
            if (progression == null) return;

            if (_header != null)
            {
                _header.text = "Level " + progression.Level +
                               (_result.LevelsGained > 1 ? "  (+" + _result.LevelsGained + " levels!)" : string.Empty) +
                               "   |   Points available: " + progression.AvailablePoints +
                               "   |   XP to next: " + progression.XpRemaining;
            }

            float y = 0f;
            const float height = 54f;
            const float gap = 6f;

            StatBlock stats = progression.Stats;
            for (int i = 0; i < StatCatalog.All.Length; i++)
            {
                StatId id = StatCatalog.All[i];
                int invested = progression.SpentIn(id);
                string value = StatCatalog.IsPercent(id)
                    ? (StatCatalog.Get(stats, id) * 100f).ToString("0.#") + "%"
                    : StatCatalog.Get(stats, id).ToString("0.#");

                UiStatRow row = StatRow(_body, id, StatCatalog.DisplayName(id) + "  (" + value + ")", invested);
                UiLayout.Place(row.Rect, y, height);
                _rows.Add(row);
                Add(row);

                y += height + gap;
            }

            UiTextBlock spendHint = UiKit.TextBlock("HowTo", _body,
                "Left/right on a stat refunds or invests a point. Points can also be refunded at any savepoint for salvage.",
                22, Palette.TextDim, TextAnchor.UpperLeft);
            UiLayout.Place((RectTransform)spendHint.transform, y + 6f, 60f);
            _rows.Add(spendHint);
            Add(spendHint);

            if (_perks != null)
            {
                List<PerkDefinition> perks = progression.AllPerksForCharacter();
                List<string> lines = new List<string> { "<b>Perks</b>" };
                for (int i = 0; i < perks.Count; i++)
                {
                    PerkDefinition perk = perks[i];
                    bool unlocked = progression.Level >= perk.UnlockLevel;
                    lines.Add((unlocked ? "<color=#6FD98A>" : "<color=#8891A6>") + perk.DisplayName +
                              " (Lv " + perk.UnlockLevel + ") - " + perk.Description + "</color>");
                }
                _perks.text = string.Join("\n", lines.ToArray());
            }
        }

        private UiStatRow StatRow(RectTransform parent, StatId id, string label, int invested)
        {
            GameObject go = new GameObject("Stat_" + id);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = PlaceholderArt.Panel("ui_row", new Color(0.13f, 0.15f, 0.2f, 0.9f), new Color(0.22f, 0.26f, 0.34f, 0.9f), 5, 48, 48, 16f);
            image.type = Image.Type.Sliced;

            UiStatRow row = go.AddComponent<UiStatRow>();
            row.Background = image;
            row.Stat = id;
            row.Progression = Services.Get<ProgressionService>();
            row.OnChanged = Rebuild;
            row.Label = UiKit.Label("Label", go.transform, label, 26, Palette.Text);
            UiKit.Anchor((RectTransform)row.Label.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(620f, 44f));
            row.Value = UiKit.Label("Value", go.transform, string.Empty, 26, Palette.Accent, TextAnchor.MiddleRight);
            UiKit.Anchor((RectTransform)row.Value.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(300f, 40f));
            row.Refresh();
            return row;
        }

        private string Prompt(string action)
        {
            InputService input = Services.Get<InputService>();
            return input == null ? action : input.PromptUi(action);
        }
    }

    public sealed class QuestLogScreen : ScreenBase
    {
        private readonly List<UiWidget> _rows = new List<UiWidget>();
        private RectTransform _list;
        private Text _details;
        private Text _header;
        private int _selected;

        public QuestLogScreen()
        {
            Id = ScreenId.QuestLog;
        }

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "Quest Log",
                "Side quests, bounties and collections available in this chapter.", out _);

            _header = UiKit.Label("Header", card, string.Empty, 24, Palette.TextDim, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_header.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -170f), new Vector2(1300f, 40f));

            _list = UiLayout.Column(card, "List", 760f, 0f, 220f);
            _list.anchorMin = new Vector2(0f, 1f);
            _list.anchorMax = new Vector2(0f, 1f);
            _list.pivot = new Vector2(0f, 1f);
            _list.anchoredPosition = new Vector2(48f, -220f);

            _details = UiKit.Label("Details", card, string.Empty, 24, Palette.Text, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_details.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-48f, -220f), new Vector2(640f, 600f));

            UiButton close = UiKit.Button("Close", card, "Back", () => Services.Get<ScreenManager>()?.Back(), true);
            UiKit.Anchor(close.Rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-48f, 40f), new Vector2(280f, 62f));
            Add(close);
        }

        public override void OnShow() => Rebuild();

        public override void Refresh() => Rebuild();

        private void Rebuild()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null) Destroy(_rows[i].gameObject);
            }
            _rows.Clear();

            QuestService quests = Services.Get<QuestService>();
            ContentDatabase content = Services.Get<ContentDatabase>();
            GameRoot root = GameRoot.Instance;
            int chapterIndex = root?.Chapter?.ChapterIndex ?? 1;

            List<QuestDefinition> all = new List<QuestDefinition>();
            all.AddRange(quests?.ActiveQuests() ?? new List<QuestDefinition>());
            List<QuestDefinition> inChapter = quests?.QuestsInChapter(chapterIndex) ?? new List<QuestDefinition>();
            for (int i = 0; i < inChapter.Count; i++)
            {
                if (!all.Contains(inChapter[i])) all.Add(inChapter[i]);
            }

            if (_header != null)
            {
                _header.text = all.Count + " quests tracked   |   " + (Services.Get<StoryService>()?.ProgressSummary() ?? string.Empty);
            }

            for (int i = 0; i < all.Count; i++)
            {
                QuestDefinition quest = all[i];
                QuestState state = quests?.StateOf(quest.Id) ?? QuestState.Locked;
                float progress = quests?.CompletionPercent(quest) ?? 0f;
                string label = quest.Title + "  [" + state + "]  " + Mathf.RoundToInt(progress * 100f) + "%";

                int captured = i;
                UiButton row = UiKit.Button("Quest_" + quest.Id, _list, label, () =>
                {
                    _selected = captured;
                    ShowDetails(all);
                });
                UiLayout.Place(row.Rect, i * 66f, 56f);
                _rows.Add(row);
                Add(row);
            }

            ShowDetails(all);
        }

        private void ShowDetails(List<QuestDefinition> quests)
        {
            QuestService service = Services.Get<QuestService>();
            if (_details == null || quests == null || quests.Count == 0)
            {
                if (_details != null) _details.text = "No quests are available in this chapter yet. Advance the story to unlock more.";
                return;
            }

            QuestDefinition quest = quests[Mathf.Clamp(_selected, 0, quests.Count - 1)];
            QuestState state = service?.StateOf(quest.Id) ?? QuestState.Locked;

            List<string> lines = new List<string>
            {
                "<b>" + quest.Title + "</b>",
                quest.Kind + "  -  Chapter " + quest.ChapterIndex + "  -  " + state,
                string.Empty,
                quest.Summary,
                string.Empty,
                "<b>Giver:</b> " + quest.GiverName + "   <b>Where:</b> " + quest.LocationHint,
                "<b>Rewards:</b> " + QuestRules.RewardSummary(quest),
                "<b>Est. time:</b> " + quest.EstimatedMinutes + " minutes",
                string.Empty
            };

            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                QuestObjective objective = quest.Objectives[i];
                if (objective == null) continue;
                QuestProgress progress = service?.Progress(quest.Id);
                int current = progress == null ? 0 : progress.CountAt(i);
                lines.Add((objective.Optional ? "[optional] " : string.Empty) + objective.ProgressText(current));
            }

            if (state == QuestState.ReadyToTurnIn)
            {
                lines.Add(string.Empty);
                lines.Add("<color=#6FD98A>Ready to turn in</color>");
            }

            _details.text = string.Join("\n", lines.ToArray());
        }
    }
}
