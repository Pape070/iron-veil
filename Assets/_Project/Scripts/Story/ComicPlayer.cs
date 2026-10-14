using System;
using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;
using UnityEngine.UI;

namespace IronVeil
{
    /// <summary>
    /// Plays a comic page: one panel at a time, actors placed by the content data,
    /// speech bubbles, captions, onomatopoeia and panel effects. Skippable, and it
    /// respects the accessibility settings (reduced flashing, text scale, speed).
    /// </summary>
    public sealed class ComicPlayer
    {
        private const float PanelWidth = 1380f;
        private const float PanelHeight = 700f;

        private RectTransform _root;
        private Image _backdrop;
        private Image _frame;
        private Image _flash;
        private Image _effectBand;
        private Image _panelIndexBack;
        private Text _panelIndex;
        private Text _title;
        private Text _caption;
        private Text _sfx;
        private Text _speech;
        private Text _speaker;
        private Text _hint;
        private RectTransform _actorLayer;
        private RectTransform _bubble;

        private readonly List<Image> _actorImages = new List<Image>();
        private ComicPageDefinition _page;
        private int _panelIndex;
        private Action _onFinished;
        private float _panelTimer;
        private float _typed;
        private float _flashTimer;
        private float _shakeTimer;
        private Vector2 _frameBase;
        private bool _playing;
        private float _textScale = 1f;
        private float _speed = 1f;

        public bool IsPlaying => _playing;
        public bool CanAdvance => _playing;
        public int PanelIndex => _panelIndex;
        public int PanelCount => _page == null ? 0 : _page.Panels.Count;

        public void Initialize(Transform parent)
        {
            _root = UiKit.Empty("ComicPlayer", parent);
            UiKit.Stretch(_root);
            _root.gameObject.SetActive(false);

            _backdrop = UiKit.Solid("Backdrop", _root, new Color(0.03f, 0.03f, 0.05f, 0.98f));
            UiKit.Stretch((RectTransform)_backdrop.transform);

            Image panel = UiKit.Panel("Panel", _root, new Color(0.1f, 0.12f, 0.16f, 1f), Palette.Border, 10);
            RectTransform panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = new Vector2(0f, 40f);
            panelRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            _frame = panel;
            _frameBase = panelRect.anchoredPosition;

            _effectBand = UiKit.Solid("EffectBand", panel.transform, new Color(1f, 1f, 1f, 0f));
            UiKit.Stretch((RectTransform)_effectBand.transform);

            _actorLayer = UiKit.Empty("Actors", panel.transform);
            UiKit.Stretch(_actorLayer);

            _flash = UiKit.Solid("Flash", _root, new Color(1f, 1f, 1f, 0f));
            UiKit.Stretch((RectTransform)_flash.transform);

            _bubble = UiKit.Empty("Bubble", panel.transform);
            UiKit.Stretch(_bubble);

            Image bubblePanel = UiKit.Panel("BubblePanel", _bubble, new Color(0.96f, 0.96f, 0.98f, 0.96f), new Color(0.2f, 0.2f, 0.24f), 10);
            RectTransform bubbleRect = (RectTransform)bubblePanel.transform;
            bubbleRect.anchorMin = new Vector2(0.5f, 1f);
            bubbleRect.anchorMax = new Vector2(0.5f, 1f);
            bubbleRect.pivot = new Vector2(0.5f, 1f);
            bubbleRect.anchoredPosition = new Vector2(0f, -18f);

            _speaker = UiKit.Label("Speaker", bubblePanel.transform, string.Empty, 26, new Color(0.2f, 0.2f, 0.26f), TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Anchor((RectTransform)_speaker.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -10f), new Vector2(PanelWidth - 160f, 34f));

            _speech = UiKit.Label("Speech", bubblePanel.transform, string.Empty, 30, new Color(0.08f, 0.08f, 0.1f), TextAnchor.UpperLeft);
            UiKit.Anchor((RectTransform)_speech.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -46f), new Vector2(PanelWidth - 180f, 120f));

            bubbleRect.sizeDelta = new Vector2(PanelWidth - 120f, 190f);

            _caption = UiKit.Label("Caption", _root, string.Empty, 30, Palette.Text, TextAnchor.MiddleCenter);
            RectTransform captionRect = (RectTransform)_caption.transform;
            captionRect.anchorMin = new Vector2(0.5f, 0f);
            captionRect.anchorMax = new Vector2(0.5f, 0f);
            captionRect.pivot = new Vector2(0.5f, 0f);
            captionRect.anchoredPosition = new Vector2(0f, 150f);
            captionRect.sizeDelta = new Vector2(PanelWidth, 90f);

            _sfx = UiKit.Label("Sfx", panel.transform, string.Empty, 64, Palette.Accent, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform sfxRect = (RectTransform)_sfx.transform;
            sfxRect.anchorMin = new Vector2(1f, 1f);
            sfxRect.anchorMax = new Vector2(1f, 1f);
            sfxRect.pivot = new Vector2(1f, 1f);
            sfxRect.anchoredPosition = new Vector2(-30f, -20f);
            sfxRect.sizeDelta = new Vector2(500f, 120f);

            _title = UiKit.Label("Title", _root, string.Empty, 26, Palette.TextDim, TextAnchor.UpperCenter);
            RectTransform titleRect = (RectTransform)_title.transform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -30f);
            titleRect.sizeDelta = new Vector2(1200f, 40f);

            _panelIndex = UiKit.Label("PanelIndex", _root, string.Empty, 24, Palette.TextDim, TextAnchor.LowerRight);
            RectTransform indexRect = (RectTransform)_panelIndex.transform;
            indexRect.anchorMin = new Vector2(1f, 0f);
            indexRect.anchorMax = new Vector2(1f, 0f);
            indexRect.pivot = new Vector2(1f, 0f);
            indexRect.anchoredPosition = new Vector2(-60f, 40f);
            indexRect.sizeDelta = new Vector2(300f, 40f);

            _hint = UiKit.Label("Hint", _root, string.Empty, 24, Palette.TextDim, TextAnchor.LowerLeft);
            RectTransform hintRect = (RectTransform)_hint.transform;
            hintRect.anchorMin = new Vector2(0f, 0f);
            hintRect.anchorMax = new Vector2(0f, 0f);
            hintRect.pivot = new Vector2(0f, 0f);
            hintRect.anchoredPosition = new Vector2(60f, 40f);
            hintRect.sizeDelta = new Vector2(900f, 40f);
        }

        public void Play(ComicPageDefinition page, Action onFinished)
        {
            if (page == null)
            {
                onFinished?.Invoke();
                return;
            }

            GameSettings settings = Services.Get<SettingsService>()?.Settings;
            _textScale = settings != null ? settings.SubtitleTextScale : 1f;
            _speed = settings != null ? settings.DialogueSpeed : 1f;

            _page = page;
            _panelIndex = 0;
            _onFinished = onFinished;
            _playing = true;
            _root.gameObject.SetActive(true);
            _title.text = string.IsNullOrEmpty(page.Title) ? "Chapter " + page.ChapterIndex + " - Page " + page.PageNumber : page.Title;

            _hint.text = settings != null && settings.ComicSkipHint
                ? "Advance: " + Prompt("UI", "Submit") + " / " + Prompt("UI", "Click") + "     Skip page: " + Prompt("UI", "Cancel")
                : string.Empty;

            ShowPanel(0);
        }

        private string Prompt(string map, string action)
        {
            InputService input = Services.Get<InputService>();
            return input == null ? action : input.Prompt(map, action);
        }

        private void ShowPanel(int index)
        {
            if (_page == null || _page.Panels == null || _page.Panels.Count == 0)
            {
                Finish();
                return;
            }

            _panelIndex = Mathf.Clamp(index, 0, _page.Panels.Count - 1);
            ComicPanelDefinition panel = _page.Panels[_panelIndex];
            _panelTimer = 0f;
            _typed = 0f;

            Palette.ComicPalette(panel.BackgroundPaletteId, out Color top, out Color bottom, out Color ink);
            _frame.color = Color.white;
            _frame.sprite = PlaceholderArt.Panel("comic_panel", bottom, Palette.Border, 10, 64, 64, 16f);
            _backdrop.sprite = PlaceholderArt.Gradient("comic_backdrop_" + panel.BackgroundPaletteId, top, bottom, 96, 96, 16f);

            BuildActors(panel, ink);

            _caption.text = panel.Caption;
            _caption.color = PanelInkForCaption(panel);
            _caption.transform.localScale = Vector3.one * _textScale;

            _speaker.text = panel.Speaker;
            _speech.text = panel.Speech;
            _speech.transform.localScale = Vector3.one * _textScale;
            _bubble.gameObject.SetActive(!string.IsNullOrEmpty(panel.Speech));

            _sfx.text = panel.SfxText;
            _sfx.color = ink;

            _panelIndex.text = "Panel " + (_panelIndex + 1) + " / " + _page.Panels.Count;

            bool reducedFlash = Services.Get<SettingsService>()?.Settings.ReducedFlashing ?? false;
            if (!string.IsNullOrEmpty(panel.EffectKey) && !reducedFlash)
            {
                ApplyPanelEffect(panel.EffectKey);
            }

            Services.Get<AudioService>()?.PlaySfx("sfx_comic_page", 1f + _panelIndex * 0.05f);
        }

        private Color PanelInkForCaption(ComicPanelDefinition panel)
        {
            Palette.ComicPalette(panel.BackgroundPaletteId, out _, out _, out Color ink);
            return ink;
        }

        private void BuildActors(ComicPanelDefinition panel, Color ink)
        {
            for (int i = 0; i < _actorImages.Count; i++)
            {
                if (_actorImages[i] != null) UnityEngine.Object.Destroy(_actorImages[i].gameObject);
            }
            _actorImages.Clear();

            if (panel.Actors == null) return;

            for (int i = 0; i < panel.Actors.Count; i++)
            {
                ComicActorPlacement actor = panel.Actors[i];
                if (actor == null) continue;

                CharacterDefinition character = FindCharacter(actor.CharacterId);
                Color primary = character != null && character.Id == CharacterId.Vera ? Palette.VeraPrimary : Palette.KadePrimary;
                Color accent = character != null && character.Id == CharacterId.Vera ? Palette.VeraAccent : Palette.KadeAccent;
                if (actor.IsSilhouette)
                {
                    primary = new Color(0.04f, 0.04f, 0.06f, 0.92f);
                    accent = new Color(0.08f, 0.08f, 0.12f, 0.95f);
                }
                if (actor.CharacterId == "crowd") primary = ink;
                if (actor.CharacterId == "env") primary = new Color(ink.r * 0.6f, ink.g * 0.6f, ink.b * 0.6f, 0.75f);

                GameObject go = new GameObject("Actor_" + actor.CharacterId);
                go.transform.SetParent(_actorLayer, false);
                Image image = go.AddComponent<Image>();
                string key = "comic_actor_" + actor.CharacterId + "_" + actor.PoseId;
                image.sprite = actor.IsSilhouette || actor.CharacterId == "env"
                    ? PlaceholderArt.Silhouette(key, primary, 72, 96)
                    : PlaceholderArt.Actor(key, primary, accent, 64, 128);
                image.raycastTarget = false;

                RectTransform rect = (RectTransform)go.transform;
                rect.anchorMin = new Vector2(actor.X, 0.5f);
                rect.anchorMax = new Vector2(actor.X, 0.5f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, actor.Y * PanelHeight * 0.5f - 60f);
                rect.sizeDelta = new Vector2(190f * actor.Scale, 380f * actor.Scale);
                float scaleX = actor.FlipX ? -Mathf.Abs(actor.Scale) : Mathf.Abs(actor.Scale);
                rect.localScale = new Vector3(scaleX, actor.Scale, 1f);

                _actorImages.Add(image);
            }
        }

        private CharacterDefinition FindCharacter(string characterId)
        {
            ContentDatabase content = Services.Get<ContentDatabase>();
            if (content == null) return null;
            if (string.Equals(characterId, "kade", StringComparison.OrdinalIgnoreCase)) return content.Character(CharacterId.Kade);
            if (string.Equals(characterId, "vera", StringComparison.OrdinalIgnoreCase)) return content.Character(CharacterId.Vera);
            return null;
        }

        private void ApplyPanelEffect(string effectKey)
        {
            switch (effectKey)
            {
                case "flash":
                case "impact":
                    _flashTimer = 0.35f;
                    break;
                case "shake":
                    _shakeTimer = 0.5f;
                    break;
                default:
                    _flashTimer = 0.2f;
                    break;
            }
        }

        public void Tick(float deltaTime)
        {
            if (!_playing) return;

            GameSettings settings = Services.Get<SettingsService>()?.Settings;

            // Typewriter reveal, sped up or slowed by the dialogue speed setting.
            ComicPanelDefinition panel = CurrentPanel();
            if (panel != null && !string.IsNullOrEmpty(panel.Speech))
            {
                float characters = panel.Speech.Length;
                _typed += deltaTime * (28f * _speed);
                int shown = Mathf.Clamp(Mathf.RoundToInt(_typed), 0, panel.Speech.Length);
                _speech.text = panel.Speech.Substring(0, shown);
                if (shown >= characters) _speech.text = panel.Speech;
            }

            _panelTimer += deltaTime;

            // Auto advance when configured.
            if (settings != null && settings.ComicAutoAdvancePanels)
            {
                float hold = (panel != null ? Mathf.Max(0.8f, panel.HoldSeconds) : 1.4f) + settings.ComicAutoAdvanceSeconds;
                if (_panelTimer >= hold) Advance();
            }

            if (_flashTimer > 0f)
            {
                _flashTimer = Mathf.Max(0f, _flashTimer - deltaTime);
                float alpha = Mathf.Clamp01(_flashTimer / 0.35f) * 0.75f;
                _flash.color = new Color(1f, 1f, 1f, alpha);
            }
            else if (_flash.color.a > 0f)
            {
                _flash.color = new Color(1f, 1f, 1f, 0f);
            }

            if (_shakeTimer > 0f)
            {
                _shakeTimer = Mathf.Max(0f, _shakeTimer - deltaTime);
                float amount = _shakeTimer * 24f;
                _frame.rectTransform.anchoredPosition = _frameBase + new Vector2(
                    Mathf.Sin(Time.unscaledTime * 60f) * amount,
                    Mathf.Cos(Time.unscaledTime * 71f) * amount);
            }
            else
            {
                _frame.rectTransform.anchoredPosition = _frameBase;
            }

        }

        private ComicPanelDefinition CurrentPanel()
        {
            if (_page == null || _page.Panels == null || _panelIndex >= _page.Panels.Count) return null;
            return _page.Panels[_panelIndex];
        }

        /// <summary>Next panel, or finish the page when the last panel is showing.</summary>
        public void Advance()
        {
            if (!_playing) return;

            // First press completes the typewriter so fast readers are not blocked.
            ComicPanelDefinition panel = CurrentPanel();
            if (panel != null && !string.IsNullOrEmpty(panel.Speech) && _speech.text != panel.Speech)
            {
                _speech.text = panel.Speech;
                _typed = panel.Speech.Length;
                return;
            }

            if (_page != null && _panelIndex + 1 < _page.Panels.Count) ShowPanel(_panelIndex + 1);
            else Finish();
        }

        public void SkipAll()
        {
            if (!_playing) return;
            Services.Get<AudioService>()?.PlaySfx("sfx_ui_back");
            Finish();
        }

        private void Finish()
        {
            if (!_playing) return;
            _playing = false;
            _root.gameObject.SetActive(false);
            Action callback = _onFinished;
            _onFinished = null;
            callback?.Invoke();
        }

        public void Close() => Finish();
    }
}
