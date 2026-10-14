using System;
using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;
using UnityEngine.UI;

namespace IronVeil
{
    /// <summary>
    /// The in-game heads-up display: health, shield, flasks, weapon and ammo,
    /// abilities, XP, objective, quest tracker, interact prompt and the boss bar.
    /// Values are only pushed to the text components when they change.
    /// </summary>
    public sealed class HudScreen : ScreenBase
    {
        private const float LowHealthThreshold = 0.35f;

        private Image _healthFill;
        private Image _shieldFill;
        private Image _xpFill;
        private Image _abilityPrimaryFill;
        private Image _abilitySecondaryFill;
        private Image _flaskPips;
        private Image _vignette;
        private Image _autosaveFlash;
        private Image _bossBarFill;
        private GameObject _bossBarRoot;

        private Text _healthText;
        private Text _flaskText;
        private Text _weaponText;
        private Text _ammoText;
        private Text _levelText;
        private Text _objective;
        private Text _objectiveDetail;
        private Text _interactPrompt;
        private Text _questTracker;
        private Text _bossName;
        private Text _momentumText;

        private float _autosaveTimer;
        private string _lastObjective;
        private string _lastPrompt;
        private int _lastHealth = -1;
        private int _lastAmmo = -1;
        private string _lastWeapon = string.Empty;
        private RectTransform _scaled;

        public HudScreen()
        {
            Id = ScreenId.Hud;
        }

        protected override void Build()
        {
            Image root = UiKit.Solid("HudRoot", Container, new Color(0f, 0f, 0f, 0f));
            UiKit.Stretch((RectTransform)root.transform);
            root.raycastTarget = false;

            _scaled = UiKit.Empty("Scaled", Container);
            UiKit.Stretch(_scaled);

            // --- bottom left: vitals ---
            Image vitalsPanel = UiKit.Panel("Vitals", _scaled, new Color(0.06f, 0.07f, 0.1f, 0.72f), new Color(0.2f, 0.24f, 0.32f, 0.7f), 8);
            UiKit.Anchor((RectTransform)vitalsPanel.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 40f), new Vector2(520f, 150f));

            Image healthBack = UiKit.Solid("HealthBack", vitalsPanel.transform, new Color(0.02f, 0.02f, 0.03f, 0.9f));
            UiKit.Anchor((RectTransform)healthBack.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -22f), new Vector2(480f, 34f));

            _healthFill = UiKit.Solid("HealthFill", healthBack.transform, new Color(0.86f, 0.28f, 0.30f));
            RectTransform healthFillRect = (RectTransform)_healthFill.transform;
            healthFillRect.anchorMin = Vector2.zero;
            healthFillRect.anchorMax = new Vector2(1f, 1f);
            healthFillRect.offsetMin = new Vector2(2f, 2f);
            healthFillRect.offsetMax = new Vector2(-2f, -2f);
            _healthFill.type = Image.Type.Filled;
            _healthFill.fillMethod = Image.FillMethod.Horizontal;

            _healthText = UiKit.Label("HealthText", vitalsPanel.transform, "100 / 100", 24, Palette.Text, TextAnchor.MiddleCenter);
            UiKit.Anchor((RectTransform)_healthText.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -22f), new Vector2(480f, 34f));

            Image shieldBack = UiKit.Solid("ShieldBack", vitalsPanel.transform, new Color(0.02f, 0.02f, 0.03f, 0.9f));
            UiKit.Anchor((RectTransform)shieldBack.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -62f), new Vector2(480f, 18f));

            _shieldFill = UiKit.Solid("ShieldFill", shieldBack.transform, new Color(0.38f, 0.74f, 0.95f));
            RectTransform shieldRect = (RectTransform)_shieldFill.transform;
            shieldRect.anchorMin = Vector2.zero;
            shieldRect.anchorMax = new Vector2(1f, 1f);
            shieldRect.offsetMin = new Vector2(2f, 2f);
            shieldRect.offsetMax = new Vector2(-2f, -2f);
            _shieldFill.type = Image.Type.Filled;
            _shieldFill.fillMethod = Image.FillMethod.Horizontal;

            _flaskPips = UiKit.Solid("Flasks", vitalsPanel.transform, new Color(0.55f, 0.9f, 0.62f, 0.9f));
            UiKit.Anchor((RectTransform)_flaskPips.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 20f), new Vector2(200f, 22f));

            _flaskText = UiKit.Label("FlaskText", vitalsPanel.transform, "Flasks 3", 22, Palette.TextDim, TextAnchor.MiddleLeft);
            UiKit.Anchor((RectTransform)_flaskText.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(232f, 18f), new Vector2(260f, 26f));

            _momentumText = UiKit.Label("Momentum", vitalsPanel.transform, string.Empty, 22, Palette.Accent, TextAnchor.LowerRight);
            UiKit.Anchor((RectTransform)_momentumText.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 16f), new Vector2(300f, 26f));

            // --- bottom right: weapon ---
            Image weaponPanel = UiKit.Panel("Weapon", _scaled, new Color(0.06f, 0.07f, 0.1f, 0.72f), new Color(0.2f, 0.24f, 0.32f, 0.7f), 8);
            UiKit.Anchor((RectTransform)weaponPanel.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 40f), new Vector2(520f, 150f));

            _weaponText = UiKit.Label("WeaponName", weaponPanel.transform, "-", 28, Palette.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_weaponText.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -18f), new Vector2(480f, 40f));

            _ammoText = UiKit.Label("Ammo", weaponPanel.transform, "-", 34, Palette.Accent, TextAnchor.MiddleLeft);
            UiKit.Anchor((RectTransform)_ammoText.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -62f), new Vector2(480f, 44f));

            _abilityPrimaryFill = MakeAbilityPip(weaponPanel.transform, new Vector2(20f, 20f), out Text primaryLabel);
            _abilitySecondaryFill = MakeAbilityPip(weaponPanel.transform, new Vector2(270f, 20f), out Text secondaryLabel);

            // --- top left: level and xp ---
            Image levelPanel = UiKit.Panel("Level", _scaled, new Color(0.06f, 0.07f, 0.1f, 0.7f), new Color(0.2f, 0.24f, 0.32f, 0.7f), 8);
            UiKit.Anchor((RectTransform)levelPanel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(360f, 92f));

            _levelText = UiKit.Label("LevelText", levelPanel.transform, "Level 1", 26, Palette.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_levelText.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -12f), new Vector2(320f, 34f));

            Image xpBack = UiKit.Solid("XpBack", levelPanel.transform, new Color(0.02f, 0.02f, 0.03f, 0.9f));
            UiKit.Anchor((RectTransform)xpBack.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(18f, 16f), new Vector2(324f, 16f));

            _xpFill = UiKit.Solid("XpFill", xpBack.transform, Palette.Accent);
            RectTransform xpRect = (RectTransform)_xpFill.transform;
            xpRect.anchorMin = Vector2.zero;
            xpRect.anchorMax = new Vector2(1f, 1f);
            xpRect.offsetMin = new Vector2(2f, 2f);
            xpRect.offsetMax = new Vector2(-2f, -2f);
            _xpFill.type = Image.Type.Filled;
            _xpFill.fillMethod = Image.FillMethod.Horizontal;

            // --- top centre: objective and boss bar ---
            _objective = UiKit.Label("Objective", _scaled, string.Empty, 30, Palette.Text, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_objective.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1100f, 40f));

            _objectiveDetail = UiKit.Label("ObjectiveDetail", _scaled, string.Empty, 24, Palette.TextDim, TextAnchor.UpperCenter);
            UiKit.Anchor((RectTransform)_objectiveDetail.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -68f), new Vector2(1100f, 36f));

            _bossBarRoot = new GameObject("BossBar");
            _bossBarRoot.transform.SetParent(_scaled, false);
            RectTransform bossRect = (RectTransform)_bossBarRoot.transform;
            bossRect.anchorMin = new Vector2(0.5f, 1f);
            bossRect.anchorMax = new Vector2(0.5f, 1f);
            bossRect.pivot = new Vector2(0.5f, 1f);
            bossRect.anchoredPosition = new Vector2(0f, -112f);
            bossRect.sizeDelta = new Vector2(1100f, 54f);

            Image bossBack = UiKit.Solid("BossBack", _bossBarRoot.transform, new Color(0.02f, 0.02f, 0.03f, 0.9f));
            UiKit.Stretch((RectTransform)bossBack.transform, 0f, 0f, 26f, 0f);

            _bossBarFill = UiKit.Solid("BossFill", bossBack.transform, new Color(0.88f, 0.26f, 0.28f));
            RectTransform bossFillRect = (RectTransform)_bossBarFill.transform;
            bossFillRect.anchorMin = Vector2.zero;
            bossFillRect.anchorMax = new Vector2(1f, 1f);
            bossFillRect.offsetMin = new Vector2(2f, 2f);
            bossFillRect.offsetMax = new Vector2(-2f, -2f);
            _bossBarFill.type = Image.Type.Filled;
            _bossBarFill.fillMethod = Image.FillMethod.Horizontal;

            _bossName = UiKit.Label("BossName", _bossBarRoot.transform, string.Empty, 26, Palette.Text, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_bossName.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(1100f, 32f));
            _bossBarRoot.SetActive(false);

            // --- top right: quest tracker ---
            _questTracker = UiKit.Label("QuestTracker", _scaled, string.Empty, 22, Palette.TextDim, TextAnchor.UpperRight);
            UiKit.Anchor((RectTransform)_questTracker.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(560f, 320f));

            // --- bottom centre: interact prompt ---
            _interactPrompt = UiKit.Label("Prompt", _scaled, string.Empty, 30, Palette.Accent, TextAnchor.LowerCenter, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_interactPrompt.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 210f), new Vector2(900f, 44f));

            // --- feedback layers ---
            _vignette = UiKit.Solid("Vignette", _scaled, new Color(0.7f, 0.05f, 0.08f, 0f));
            UiKit.Stretch((RectTransform)_vignette.transform);
            _vignette.raycastTarget = false;

            _autosaveFlash = UiKit.Label("Autosave", _scaled, string.Empty, 22, Palette.TextDim, TextAnchor.LowerRight);
            UiKit.Anchor((RectTransform)_autosaveFlash.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 200f), new Vector2(400f, 32f));
        }

        private Image MakeAbilityPip(Transform parent, Vector2 position, out Text label)
        {
            Image back = UiKit.Solid("AbilityBack_" + position.x, parent, new Color(0.02f, 0.02f, 0.03f, 0.9f));
            UiKit.Anchor((RectTransform)back.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), position, new Vector2(230f, 20f));

            Image fill = UiKit.Solid("AbilityFill_" + position.x, back.transform, Palette.AccentCool);
            RectTransform rect = (RectTransform)fill.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(2f, 2f);
            rect.offsetMax = new Vector2(-2f, -2f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;

            label = UiKit.Label("AbilityLabel_" + position.x, parent, string.Empty, 20, Palette.TextDim, TextAnchor.LowerLeft);
            UiKit.Anchor((RectTransform)label.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), position + new Vector2(0f, 22f), new Vector2(230f, 26f));
            return fill;
        }

        public void SetObjective(string title, string objective)
        {
            if (_objective == null) return;
            _lastObjective = title;
            _objective.text = title;
            _objectiveDetail.text = objective;
        }

        public void SetInteractPrompt(string prompt)
        {
            if (_interactPrompt == null) return;
            _lastPrompt = prompt;
            _interactPrompt.text = string.IsNullOrEmpty(prompt) ? string.Empty : prompt + "   [" + Prompt("Interact") + "]";
        }

        private string Prompt(string action)
        {
            InputService input = Services.Get<InputService>();
            return input == null ? action : input.PromptGameplay(action);
        }

        public override void OnShow()
        {
            GameSettings settings = Services.Get<SettingsService>()?.Settings;
            float scale = settings == null ? 1f : settings.HudScale == 0 ? 0.85f : settings.HudScale == 2 ? 1.15f : 1f;
            if (_scaled != null) _scaled.localScale = Vector3.one * scale;
        }

        public override void Tick(float deltaTime)
        {
            GameRoot root = GameRoot.Instance;
            ChapterRuntime chapter = root?.Chapter;
            PlayerActor player = chapter?.Player;
            ProgressionService progression = Services.Get<ProgressionService>();

            if (player == null || progression == null) return;

            int health = Mathf.CeilToInt(player.Health);
            if (health != _lastHealth)
            {
                _lastHealth = health;
                float ratio = player.MaxHealth <= 0f ? 0f : Mathf.Clamp01(player.Health / player.MaxHealth);
                _healthFill.fillAmount = ratio;
                _healthText.text = health + " / " + Mathf.RoundToInt(player.MaxHealth);
                _healthFill.color = ratio < LowHealthThreshold
                    ? Color.Lerp(new Color(0.95f, 0.15f, 0.2f), new Color(0.86f, 0.28f, 0.30f), 1f - Mathf.Abs(Mathf.Sin(Time.unscaledTime * 6f)))
                    : new Color(0.86f, 0.28f, 0.30f);
                _vignette.color = new Color(0.7f, 0.05f, 0.08f, Mathf.Clamp01((LowHealthThreshold - ratio) / LowHealthThreshold) * 0.35f);
            }

            _shieldFill.fillAmount = player.MaxShield <= 0f ? 0f : Mathf.Clamp01(player.Shield / player.MaxShield);
            _shieldFill.enabled = player.MaxShield > 0.01f;

            _flaskText.text = "Flasks " + player.Flasks;
            _flaskPips.rectTransform.sizeDelta = new Vector2(Mathf.Clamp(player.Flasks, 0, 9) * 22f, 22f);

            WeaponRuntime weapon = player.Weapon;
            string weaponName = weapon?.Definition == null ? "Unarmed" : weapon.Definition.DisplayName;
            if (weaponName != _lastWeapon)
            {
                _lastWeapon = weaponName;
                _weaponText.text = weaponName + (weapon != null && weapon.UpgradeLevel > 1 ? "  +" + weapon.UpgradeLevel : string.Empty);
            }

            int ammo = weapon == null ? 0 : weapon.Magazine;
            if (ammo != _lastAmmo)
            {
                _lastAmmo = ammo;
                _ammoText.text = weapon == null
                    ? "-"
                    : weapon.Magazine + " / " + weapon.Reserve + (weapon.Reloading ? "   reloading" : string.Empty);
            }
            else if (weapon != null && weapon.Reloading)
            {
                _ammoText.text = weapon.Magazine + " / " + weapon.Reserve + "   reloading";
            }

            _abilityPrimaryFill.fillAmount = 1f - player.PrimaryCooldownRatio;
            _abilitySecondaryFill.fillAmount = 1f - player.SecondaryCooldownRatio;
            _abilityPrimaryFill.color = player.PrimaryCooldownRatio <= 0f ? Palette.Accent : Palette.AccentCool * 0.6f;
            _abilitySecondaryFill.color = player.SecondaryCooldownRatio <= 0f ? Palette.Accent : Palette.AccentCool * 0.6f;

            _levelText.text = "Level " + progression.Level + "   " + Mathf.RoundToInt(progression.XpProgress * 100f) + "%";
            _xpFill.fillAmount = progression.XpProgress;

            if (player.MomentumStacks > 0)
            {
                _momentumText.text = "Momentum x" + player.MomentumStacks;
            }
            else if (player.HasBarrier)
            {
                _momentumText.text = "Bulwark up";
            }
            else if (player.IsOverclocked)
            {
                _momentumText.text = "Overclocked";
            }
            else
            {
                _momentumText.text = string.Empty;
            }

            // Boss bar.
            BossActor boss = chapter.Boss;
            bool showBoss = boss != null && boss.IsAlive;
            if (_bossBarRoot.activeSelf != showBoss) _bossBarRoot.SetActive(showBoss);
            if (showBoss)
            {
                _bossBarFill.fillAmount = boss.HealthRatio;
                BossDefinition definition = boss.Definition;
                string phaseName = boss.Runner.CurrentPhase != null ? boss.Runner.CurrentPhase.DisplayName : string.Empty;
                _bossName.text = definition.DisplayName + (string.IsNullOrEmpty(phaseName) ? string.Empty : "   -   " + phaseName) +
                                 (boss.Runner.IsEnraged ? "   ENRAGED" : string.Empty);
            }

            // Quest tracker.
            QuestService quests = Services.Get<QuestService>();
            List<QuestDefinition> active = quests?.ActiveQuests();
            if (active != null)
            {
                if (active.Count == 0)
                {
                    _questTracker.text = string.Empty;
                }
                else
                {
                    List<string> lines = new List<string>();
                    for (int i = 0; i < active.Count && i < 4; i++)
                    {
                        lines.Add("<b>" + active[i].Title + "</b>\n" + quests.ObjectiveText(active[i]));
                    }
                    _questTracker.text = string.Join("\n", lines.ToArray());
                }
            }

            // Autosave indicator.
            if (_autosaveTimer > 0f)
            {
                _autosaveTimer -= deltaTime;
                _autosaveFlash.text = _autosaveTimer > 0f ? "Progress saved" : string.Empty;
            }
        }

        public override void Refresh()
        {
            GameSettings settings = Services.Get<SettingsService>()?.Settings;
            float scale = settings == null ? 1f : settings.HudScale == 0 ? 0.85f : settings.HudScale == 2 ? 1.15f : 1f;
            if (_scaled != null) _scaled.localScale = Vector3.one * scale;
        }

        /// <summary>Called by the runtime when an autosave lands, so the HUD can confirm it.</summary>
        public void FlashSaved()
        {
            _autosaveTimer = 2.5f;
            if (_autosaveFlash != null) _autosaveFlash.text = "Progress saved";
        }
    }

    public sealed class GalleryScreen : ScreenBase
    {
        private readonly List<UiWidget> _rows = new List<UiWidget>();
        private RectTransform _list;
        private Text _header;
        private Text _details;
        private int _pageIndex;

        public GalleryScreen()
        {
            Id = ScreenId.Gallery;
        }

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "Gallery",
                "Every comic page you have seen, plus the bestiary and boss records.", out _);

            _header = UiKit.Label("Header", card, string.Empty, 24, Palette.TextDim, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_header.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -170f), new Vector2(1300f, 40f));

            _list = UiLayout.Column(card, "List", 900f, 0f, 220f);
            _list.anchorMin = new Vector2(0f, 1f);
            _list.anchorMax = new Vector2(0f, 1f);
            _list.pivot = new Vector2(0f, 1f);
            _list.anchoredPosition = new Vector2(48f, -220f);

            _details = UiKit.Label("Details", card, string.Empty, 22, Palette.Text, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_details.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-48f, -220f), new Vector2(560f, 620f));

            UiButton replay = UiKit.Button("Replay", card, "Replay page", () =>
            {
                List<ComicPageDefinition> pages = Services.Get<StoryService>()?.UnlockedPages() ?? new List<ComicPageDefinition>();
                if (pages.Count == 0) return;
                ComicPageDefinition page = pages[Mathf.Clamp(_pageIndex, 0, pages.Count - 1)];
                Services.Get<ScreenManager>()?.ShowComic(page, null);
            }, true);
            UiKit.Anchor(replay.Rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-48f, 40f), new Vector2(300f, 60f));
            Add(replay);

            UiButton back = UiKit.Button("Back", card, "Back", () => Services.Get<ScreenManager>()?.Back());
            UiKit.Anchor(back.Rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-368f, 40f), new Vector2(240f, 60f));
            Add(back);
        }

        public override void OnShow() => Rebuild();

        private void Rebuild()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null) Destroy(_rows[i].gameObject);
            }
            _rows.Clear();

            StoryService story = Services.Get<StoryService>();
            List<ComicPageDefinition> pages = story?.UnlockedPages() ?? new List<ComicPageDefinition>();
            List<BossDefinition> bosses = story?.DefeatedBosses(false) ?? new List<BossDefinition>();
            List<BossDefinition> miniBosses = story?.DefeatedBosses(true) ?? new List<BossDefinition>();

            if (_header != null)
            {
                _header.text = pages.Count + " comic pages unlocked   |   " + bosses.Count + " bosses defeated   |   " +
                               miniBosses.Count + " mini-bosses defeated";
            }

            for (int i = 0; i < pages.Count; i++)
            {
                ComicPageDefinition page = pages[i];
                int captured = i;
                UiButton row = UiKit.Button("Page_" + page.Id, _list,
                    "Ch " + page.ChapterIndex + "  -  Page " + page.PageNumber + "  -  " + page.Title,
                    () =>
                    {
                        _pageIndex = captured;
                        ShowDetails(pages);
                    });
                UiLayout.Place(row.Rect, i * 60f, 52f);
                _rows.Add(row);
                Add(row);
            }

            if (pages.Count == 0 && _details != null)
            {
                _details.text = "No pages unlocked yet. Comic panels unlock as you reach them in the story.";
            }
            else
            {
                ShowDetails(pages);
            }
        }

        private void ShowDetails(List<ComicPageDefinition> pages)
        {
            if (_details == null || pages.Count == 0) return;
            ComicPageDefinition page = pages[Mathf.Clamp(_pageIndex, 0, pages.Count - 1)];
            List<string> lines = new List<string>
            {
                "<b>" + page.Title + "</b>",
                "Chapter " + page.ChapterIndex + "  -  page " + page.PageNumber + "  -  " + page.Panels.Count + " panels"
            };
            for (int i = 0; i < page.Panels.Count && i < 8; i++)
            {
                ComicPanelDefinition panel = page.Panels[i];
                string who = string.IsNullOrEmpty(panel.Speaker) ? panel.Caption : panel.Speaker;
                string what = string.IsNullOrEmpty(panel.Speech) ? panel.SfxText : panel.Speech;
                lines.Add((i + 1) + ". " + panel.Shot + " - " + who + (string.IsNullOrEmpty(what) ? string.Empty : ": " + what));
            }
            _details.text = string.Join("\n", lines.ToArray());
        }
    }

    public sealed class CodexScreen : ScreenBase
    {
        private enum Tab
        {
            Characters = 0,
            Weapons = 1,
            Gear = 2,
            Enemies = 3,
            Bosses = 4,
            Chapters = 5
        }

        private Tab _tab = Tab.Characters;
        private readonly List<UiWidget> _rows = new List<UiWidget>();
        private RectTransform _list;
        private Text _details;
        private Text _header;
        private int _selected;

        public CodexScreen()
        {
            Id = ScreenId.Codex;
        }

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "Codex",
                "Characters, arsenal, bestiary and the campaign structure.", out _);

            string[] tabs = { "Characters", "Weapons", "Gear", "Bestiary", "Bosses", "Chapters" };
            for (int i = 0; i < tabs.Length; i++)
            {
                int index = i;
                UiButton tab = UiKit.Button("Tab_" + tabs[i], card, tabs[i], () =>
                {
                    _tab = (Tab)index;
                    _selected = 0;
                    Rebuild();
                });
                UiKit.Anchor(tab.Rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f + i * 250f, -172f), new Vector2(236f, 54f));
                Add(tab);
            }

            _header = UiKit.Label("Header", card, string.Empty, 22, Palette.TextDim, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_header.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -238f), new Vector2(1300f, 34f));

            _list = UiLayout.Column(card, "List", 700f, 0f, 286f);
            _list.anchorMin = new Vector2(0f, 1f);
            _list.anchorMax = new Vector2(0f, 1f);
            _list.pivot = new Vector2(0f, 1f);
            _list.anchoredPosition = new Vector2(48f, -286f);

            _details = UiKit.Label("Details", card, string.Empty, 22, Palette.Text, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_details.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-48f, -286f), new Vector2(700f, 540f));

            UiButton back = UiKit.Button("Back", card, "Back", () => Services.Get<ScreenManager>()?.Back());
            UiKit.Anchor(back.Rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-48f, 40f), new Vector2(240f, 60f));
            Add(back);
        }

        public override void OnShow() => Rebuild();

        private void Rebuild()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null) Destroy(_rows[i].gameObject);
            }
            _rows.Clear();

            CampaignContent content = Services.Get<ContentDatabase>()?.Content;
            if (content == null) return;

            List<string> labels = new List<string>();
            switch (_tab)
            {
                case Tab.Characters:
                    for (int i = 0; i < content.Characters.Count; i++) labels.Add(content.Characters[i].Name + "  \"" + content.Characters[i].Callsign + "\"");
                    break;
                case Tab.Weapons:
                    for (int i = 0; i < content.Weapons.Count; i++) labels.Add(content.Weapons[i].DisplayName + "  (" + content.Weapons[i].Kind + ")");
                    break;
                case Tab.Gear:
                    for (int i = 0; i < content.GearItems.Count; i++) labels.Add(content.GearItems[i].DisplayName + "  (" + content.GearItems[i].Rarity + ")");
                    break;
                case Tab.Enemies:
                    for (int i = 0; i < content.Enemies.Count; i++) labels.Add(content.Enemies[i].DisplayName + "  -  " + content.Enemies[i].Class);
                    break;
                case Tab.Bosses:
                    for (int i = 0; i < content.Bosses.Count; i++)
                    {
                        BossDefinition boss = content.Bosses[i];
                        labels.Add("Ch " + boss.ChapterIndex + "  " + boss.DisplayName + (boss.IsMiniBoss ? "  (mini-boss)" : string.Empty));
                    }
                    break;
                default:
                    for (int i = 0; i < content.Chapters.Count; i++) labels.Add("Ch " + content.Chapters[i].Index + "  " + content.Chapters[i].Title);
                    break;
            }

            if (_header != null) _header.text = labels.Count + " entries";

            int maxRows = 12;
            for (int i = 0; i < Mathf.Min(maxRows, labels.Count); i++)
            {
                int captured = i;
                UiButton row = UiKit.Button("Codex_" + i, _list, labels[i], () =>
                {
                    _selected = captured;
                    ShowDetails(content, labels);
                });
                UiLayout.Place(row.Rect, i * 58f, 50f);
                _rows.Add(row);
                Add(row);
            }

            ShowDetails(content, labels);
        }

        private void ShowDetails(CampaignContent content, List<string> labels)
        {
            if (_details == null) return;
            _selected = Mathf.Clamp(_selected, 0, Mathf.Max(0, labels.Count - 1));

            switch (_tab)
            {
                case Tab.Characters:
                {
                    CharacterDefinition character = content.Characters[_selected];
                    StatBlock stats = character.BaseStats;
                    _details.text = "<b>" + character.Name + " \"" + character.Callsign + "\"</b>\n" + character.Role + "\n\n" +
                                    character.Bio + "\n\nHealth " + Mathf.RoundToInt(stats.MaxHealth) + "   Shield " + Mathf.RoundToInt(stats.Shield) +
                                    "   Power " + Mathf.RoundToInt(stats.Power) + "   Defense " + Mathf.RoundToInt(stats.Defense) +
                                    "   Speed " + stats.MoveSpeed.ToString("0.0") + "\n\n" +
                                    "<b>Primary</b> " + character.PrimaryAbility.DisplayName + " - " + character.PrimaryAbility.Description + "\n" +
                                    "<b>Secondary</b> " + character.SecondaryAbility.DisplayName + " - " + character.SecondaryAbility.Description + "\n" +
                                    "<b>Passive</b> " + character.Passive.DisplayName + " - " + character.Passive.Description;
                    break;
                }
                case Tab.Weapons:
                {
                    WeaponDefinition weapon = content.Weapons[_selected];
                    _details.text = "<b>" + weapon.DisplayName + "</b>\n" + weapon.Kind + "  -  " + weapon.Mode + "  -  tier " + weapon.Tier + "\n\n" +
                                    weapon.Description + "\n\nDamage " + weapon.DamagePerShot.ToString("0.#") + "   Rate " + weapon.RoundsPerSecond.ToString("0.#") +
                                    "/s   Magazine " + weapon.MagazineSize + "   Reload " + weapon.ReloadTime.ToString("0.00") + "s\n" +
                                    "Pellets " + weapon.PelletsPerShot + "   Spread " + weapon.SpreadDegrees.ToString("0.#") + " deg   Range " + weapon.Range.ToString("0.#") + "m\n" +
                                    "Upgrades to +" + weapon.MaxUpgradeLevel + " (first costs " + weapon.UpgradeSalvageCost + " salvage)\n" +
                                    "Unlock: " + weapon.UnlockSource;
                    break;
                }
                case Tab.Gear:
                {
                    GearDefinition gear = content.GearItems[_selected];
                    StatBlock bonus = gear.Bonus;
                    _details.text = "<b>" + gear.DisplayName + "</b>\n" + gear.Slot + "  -  " + gear.Rarity + "  -  tier " + gear.Tier + "\n\n" +
                                    gear.Description + "\n\nBonus: Power " + bonus.Power.ToString("0.#") + "   Defense " + bonus.Defense.ToString("0.#") +
                                    "   Health " + bonus.MaxHealth.ToString("0.#") + "   Shield " + bonus.Shield.ToString("0.#") +
                                    "   Speed " + bonus.MoveSpeed.ToString("0.#") + "\n" +
                                    "Dropped by: " + gear.DroppedBy;
                    break;
                }
                case Tab.Enemies:
                {
                    EnemyArchetype enemy = content.Enemies[_selected];
                    _details.text = "<b>" + enemy.DisplayName + "</b>\n" + enemy.Class + "  -  tier " + enemy.Tier + "  -  from chapter " + enemy.FirstChapter + "\n\n" +
                                    enemy.Bestiary + "\n\nHealth " + enemy.BaseHealth.ToString("0") + "   Damage " + enemy.BaseDamage.ToString("0.#") +
                                    "   Defense " + enemy.BaseDefense.ToString("0") + "   Speed " + enemy.MoveSpeed.ToString("0.0") + "\n" +
                                    "Attack range " + enemy.AttackRange.ToString("0.#") + "m   Cooldown " + enemy.AttackCooldown.ToString("0.00") + "s   Telegraph " + enemy.Telegraph.ToString("0.00") + "s\n" +
                                    "Patterns: " + enemy.Attacks.Count + "   Weak point multiplier x" + enemy.WeakPointMultiplier.ToString("0.00");
                    break;
                }
                case Tab.Bosses:
                {
                    BossDefinition boss = content.Bosses[_selected];
                    _details.text = "<b>" + boss.DisplayName + "</b>\n" + boss.Title + "  -  " + boss.Epithet + "\n\n" +
                                    boss.BestiaryEntry + "\n\nHealth " + boss.BaseHealth.ToString("0") + "   Damage " + boss.BaseDamage.ToString("0.#") +
                                    "   Defense " + boss.BaseDefense.ToString("0") + "   Phases " + boss.PhaseCount +
                                    "   Weak points " + boss.WeakPointCount + "\n" +
                                    "Enrage after " + (boss.EnrageAfterSeconds > 0f ? boss.EnrageAfterSeconds.ToString("0") + "s" : "never") +
                                    "   Patterns at current difficulty: " + boss.TotalPatterns(DifficultyProfile.For(Services.Get<ProgressionService>()?.Difficulty.Difficulty ?? Difficulty.Normal)) + "\n" +
                                    "Drops: " + string.Join(", ", boss.UpgradeDrops.ToArray());
                    break;
                }
                default:
                {
                    ChapterDefinition chapter = content.Chapters[_selected];
                    _details.text = "<b>Chapter " + chapter.Index + " - " + chapter.Title + "</b>\n" + chapter.ActTitle + "   |   " + chapter.Subtitle + "\n\n" +
                                    chapter.Synopsis + "\n\nLocation: " + chapter.LocationName + "\n" +
                                    "Beats: " + chapter.Beats.Count + "   Comic pages: " + chapter.ComicPages.Count +
                                    "   Savepoints: " + chapter.Savepoints.Count + "   Checkpoints: " + chapter.Checkpoints.Count + "\n" +
                                    "Boss: " + chapter.BossId + "   Mini-bosses: " + chapter.MiniBossIds.Count + "   Side quests: " + chapter.SideQuestIds.Count + "\n" +
                                    "Estimated " + chapter.EstimatedMinutes + " minutes   Recommended level " + chapter.RecommendedLevel +
                                    "   Minimum level " + chapter.MinimumLevel;
                    break;
                }
            }
        }
    }

    public sealed class GameOverScreen : ScreenBase
    {
        private Text _cause;
        private Text _stats;

        public GameOverScreen()
        {
            Id = ScreenId.GameOver;
        }

        public void Set(string cause)
        {
            if (_cause != null) _cause.text = string.IsNullOrEmpty(cause) ? "You went down." : cause;
            Refresh();
        }

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "Down", "Death is a checkpoint, not an ending.", out _);
            RectTransform column = UiLayout.Column(card, "Menu", 700f, 0.5f, 220f);
            column.anchorMin = new Vector2(0.5f, 1f);
            column.anchorMax = new Vector2(0.5f, 1f);
            column.anchoredPosition = new Vector2(0f, -220f);

            _cause = UiKit.Label("Cause", column, string.Empty, 28, Palette.Danger, TextAnchor.UpperLeft);
            UiLayout.Place((RectTransform)_cause.transform, 0f, 60f);
            _stats = UiKit.Label("Stats", column, string.Empty, 24, Palette.TextDim, TextAnchor.UpperLeft);
            UiLayout.Place((RectTransform)_stats.transform, 70f, 220f);

            float y = 300f;
            UiButton retry = UiKit.Button("Retry", column, "Retry from checkpoint", () =>
            {
                GameRoot.Instance?.RespawnAtCheckpoint();
            }, true);
            UiLayout.Place(retry.Rect, y, 70f);
            Add(retry);
            y += 86f;

            UiButton options = UiKit.Button("Options", column, "Options", () => GameRoot.Instance?.OpenOptions(ScreenId.GameOver));
            UiLayout.Place(options.Rect, y, 66f);
            Add(options);
            y += 82f;

            UiButton menu = UiKit.Button("Menu", column, "Main menu", () =>
            {
                Services.Get<ScreenManager>()?.HideAll();
                GameRoot.Instance?.ReturnToMainMenu();
            });
            UiLayout.Place(menu.Rect, y, 66f);
            Add(menu);
        }

        public override void OnShow() => Refresh();

        public override void Refresh()
        {
            SaveService saves = Services.Get<SaveService>();
            ProgressionService progression = Services.Get<ProgressionService>();
            if (_stats != null && saves?.Current != null)
            {
                _stats.text = "Deaths " + saves.Current.Deaths + "   |   " + progression?.LevelSummary() +
                              "\nLast checkpoint: " + (string.IsNullOrEmpty(saves.Current.CheckpointId) ? "chapter start" : saves.Current.CheckpointId);
            }
        }
    }

    public sealed class ChapterCompleteScreen : ScreenBase
    {
        private ChapterDefinition _chapter;
        private Text _title;
        private Text _body;

        public ChapterCompleteScreen()
        {
            Id = ScreenId.ChapterComplete;
        }

        public void Set(ChapterDefinition chapter, string summary)
        {
            _chapter = chapter;
            if (_title != null)
            {
                _title.text = chapter == null ? "Chapter complete" : "Chapter " + chapter.Index + " complete - " + chapter.Title;
            }
            if (_body != null)
            {
                _body.text = (chapter == null ? string.Empty : chapter.ActTitle + "\n\n" + chapter.Synopsis + "\n\n") + summary;
            }
        }

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "Chapter Complete", "The next chapter is already unlocked at a savepoint.", out _);

            _title = UiKit.Label("ChapterTitle", card, string.Empty, 34, Palette.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_title.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -180f), new Vector2(1400f, 46f));

            _body = UiKit.Label("Body", card, string.Empty, 24, Palette.Text, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_body.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -240f), new Vector2(1400f, 440f));

            UiButton next = UiKit.Button("Next", card, "Continue to the next chapter", () => GameRoot.Instance?.AdvanceToNextChapter(), true);
            UiKit.Anchor(next.Rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-48f, 44f), new Vector2(520f, 68f));
            Add(next);

            UiButton menu = UiKit.Button("Menu", card, "Main menu", () =>
            {
                Services.Get<ScreenManager>()?.HideAll();
                GameRoot.Instance?.ReturnToMainMenu();
            });
            UiKit.Anchor(menu.Rect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(48f, 44f), new Vector2(280f, 62f));
            Add(menu);
        }
    }

    public sealed class EndingScreen : ScreenBase
    {
        private Text _body;

        public EndingScreen()
        {
            Id = ScreenId.Ending;
        }

        protected override void Build()
        {
            RectTransform card = UiLayout.Frame(this, Container, "Ending", "Iron Veil - the veil is thinner than they told you.", out _);

            _body = UiKit.Label("Body", card, string.Empty, 26, Palette.Text, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_body.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -180f), new Vector2(1400f, 520f));

            UiButton menu = UiKit.Button("Menu", card, "Return to the main menu", () =>
            {
                Services.Get<ScreenManager>()?.HideAll();
                GameRoot.Instance?.ReturnToMainMenu();
            }, true);
            UiKit.Anchor(menu.Rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-48f, 44f), new Vector2(480f, 66f));
            Add(menu);
        }

        public override void OnShow()
        {
            ContentDatabase content = Services.Get<ContentDatabase>();
            StoryService story = Services.Get<StoryService>();
            string flags = string.Empty;
            SaveGame save = Services.Get<SaveService>()?.Current;
            if (save != null && save.StoryFlags.Count > 0)
            {
                flags = "\n\nDecisions recorded: " + save.StoryFlags.Count;
            }

            _body.text = (content?.Content?.PlotSummary ?? string.Empty) +
                         "\n\nHelion's chain of command is broken, the Veil is sealed and both operatives walk out alive." +
                         "\n\n" + (story?.ProgressSummary() ?? string.Empty) + flags;
        }
    }

    public sealed class MessageScreen : ScreenBase
    {
        private Text _title;
        private Text _body;

        public MessageScreen()
        {
            Id = ScreenId.Message;
        }

        public void Set(string title, string body)
        {
            if (_title != null) _title.text = title;
            if (_body != null) _body.text = body;
        }

        protected override void Build()
        {
            Image panel = UiKit.Panel("MessagePanel", Container, new Color(0.06f, 0.07f, 0.1f, 0.96f), Palette.Border, 10);
            UiKit.Anchor((RectTransform)panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(900f, 150f));

            _title = UiKit.Label("Title", panel.transform, string.Empty, 26, Palette.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_title.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -14f), new Vector2(860f, 34f));

            _body = UiKit.Label("Body", panel.transform, string.Empty, 22, Palette.Text, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_body.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -52f), new Vector2(860f, 90f));
        }
    }

    public sealed class BossIntroScreen : ScreenBase
    {
        private Text _name;
        private Text _title;
        private Text _bestiary;

        public BossIntroScreen()
        {
            Id = ScreenId.BossIntro;
        }

        public void Set(BossDefinition boss)
        {
            if (boss == null) return;
            if (_name != null) _name.text = boss.DisplayName;
            if (_title != null) _title.text = boss.Title + "   -   " + boss.Epithet;
            if (_bestiary != null) _bestiary.text = boss.BestiaryEntry;
        }

        protected override void Build()
        {
            UiKit.Solid("Band", Container, new Color(0.02f, 0.02f, 0.03f, 0.72f));

            Image band = UiKit.Panel("BossCard", Container, new Color(0.08f, 0.06f, 0.08f, 0.92f), new Color(0.6f, 0.25f, 0.25f, 0.9f), 12);
            UiKit.Anchor((RectTransform)band.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(1200f, 190f));

            _name = UiKit.Label("Name", band.transform, string.Empty, 54, Palette.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_name.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -18f), new Vector2(1136f, 66f));

            _title = UiKit.Label("Title", band.transform, string.Empty, 26, Palette.Danger, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_title.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -88f), new Vector2(1136f, 36f));

            _bestiary = UiKit.Label("Bestiary", band.transform, string.Empty, 22, Palette.TextDim, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_bestiary.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -126f), new Vector2(1136f, 56f));
        }
    }

    public sealed class DialogueScreen : ScreenBase
    {
        private Text _speaker;
        private Text _line;
        private Text _hint;
        private List<string> _lines = new List<string>();
        private int _index;
        private Action _onFinished;

        public DialogueScreen()
        {
            Id = ScreenId.Dialogue;
        }

        public void Set(string speaker, List<string> lines, Action onFinished)
        {
            _lines = lines ?? new List<string>();
            _index = 0;
            _onFinished = onFinished;
            if (_speaker != null) _speaker.text = speaker;
            ShowLine();
        }

        protected override void Build()
        {
            Image panel = UiKit.Panel("DialoguePanel", Container, new Color(0.05f, 0.06f, 0.08f, 0.94f), Palette.Border, 10);
            UiKit.Anchor((RectTransform)panel.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(1500f, 220f));

            _speaker = UiKit.Label("Speaker", panel.transform, string.Empty, 26, Palette.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_speaker.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -18f), new Vector2(1436f, 34f));

            _line = UiKit.Label("Line", panel.transform, string.Empty, 30, Palette.Text, TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_line.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -62f), new Vector2(1436f, 110f));

            _hint = UiKit.Label("Hint", panel.transform, string.Empty, 22, Palette.TextDim, TextAnchor.LowerRight);
            UiKit.Anchor((RectTransform)_hint.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-32f, 14f), new Vector2(700f, 30f));

            UiButton next = UiKit.Button("Next", Container, "Next", Advance, true);
            UiKit.Anchor(next.Rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(320f, 58f));
            Add(next);
        }

        public override void OnShow()
        {
            GameSettings settings = Services.Get<SettingsService>()?.Settings;
            bool subtitles = settings == null || settings.Subtitles;
            if (_line != null) _line.gameObject.SetActive(subtitles);
            if (_hint != null)
            {
                InputService input = Services.Get<InputService>();
                _hint.text = "Advance: " + (input == null ? "Enter" : input.PromptUi("Submit"));
            }
            float scale = settings == null ? 1f : settings.SubtitleTextScale;
            if (_line != null) _line.transform.localScale = Vector3.one * scale;
            ShowLine();
            FocusFirst();
        }

        private void ShowLine()
        {
            if (_line != null)
            {
                _line.text = _lines.Count == 0 ? "..." : _lines[Mathf.Clamp(_index, 0, _lines.Count - 1)];
            }
        }

        private void Advance()
        {
            _index++;
            if (_index >= _lines.Count)
            {
                Action callback = _onFinished;
                _onFinished = null;
                callback?.Invoke();
                return;
            }
            ShowLine();
        }

        public override bool OnCancel()
        {
            Advance();
            return false;
        }
    }

    public sealed class ChoiceScreen : ScreenBase
    {
        private Text _title;
        private Text _prompt;
        private Action<int> _onChosen;
        private readonly List<UiWidget> _options = new List<UiWidget>();

        public ChoiceScreen()
        {
            Id = ScreenId.Choice;
        }

        public void Set(string title, string prompt, string[] options, Action<int> onChosen)
        {
            _onChosen = onChosen;
            if (_title != null) _title.text = title;
            if (_prompt != null) _prompt.text = prompt;

            for (int i = 0; i < _options.Count; i++)
            {
                if (_options[i] != null) Destroy(_options[i].gameObject);
            }
            _options.Clear();

            options = options ?? new string[0];
            for (int i = 0; i < options.Length; i++)
            {
                int captured = i;
                UiButton button = UiKit.Button("Choice_" + i, Container, options[i], () => Chosen(captured), i == 0);
                UiKit.Anchor(button.Rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 40f - i * 90f), new Vector2(900f, 74f));
                _options.Add(button);
                Add(button);
            }
        }

        private void Chosen(int index)
        {
            Action<int> callback = _onChosen;
            _onChosen = null;
            callback?.Invoke(index);
        }

        protected override void Build()
        {
            UiKit.Solid("Backdrop", Container, new Color(0.02f, 0.02f, 0.03f, 0.75f));

            _title = UiKit.Label("Title", Container, string.Empty, 40, Palette.Text, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_title.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 170f), new Vector2(1400f, 60f));

            _prompt = UiKit.Label("Prompt", Container, string.Empty, 26, Palette.TextDim, TextAnchor.MiddleCenter);
            UiKit.Anchor((RectTransform)_prompt.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(1400f, 60f));
        }

        public override bool OnCancel() => false;
    }
}
