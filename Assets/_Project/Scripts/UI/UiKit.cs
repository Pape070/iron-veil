using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IronVeil
{
    /// <summary>
    /// Unity 6 ships a single built-in font ("LegacyRuntime.ttf"); older names are
    /// tried as fallbacks, and finally an OS font, so text always renders.
    /// </summary>
    public static class FontProvider
    {
        private static Font _font;

        public static Font Get()
        {
            if (_font != null) return _font;

            string[] candidates = { "LegacyRuntime.ttf", "Arial.ttf" };
            for (int i = 0; i < candidates.Length; i++)
            {
                try
                {
                    _font = Resources.GetBuiltinResource<Font>(candidates[i]);
                }
                catch (Exception)
                {
                    _font = null;
                }
                if (_font != null) return _font;
            }

            try
            {
                _font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[IronVeil] No usable font found: " + exception.Message);
            }
            return _font;
        }
    }

    /// <summary>Where a widget lives so focus navigation can reason about it.</summary>
    public abstract class UiWidget : MonoBehaviour
    {
        public bool Enabled = true;

        public RectTransform Rect => (RectTransform)transform;

        public virtual bool Interactable => Enabled && gameObject.activeInHierarchy;

        public virtual void OnSubmit() { }

        /// <summary>Left/right (or up/down) adjustment for option rows and sliders.</summary>
        public virtual void OnAdjust(int delta) { }

        public virtual void SetHighlighted(bool highlighted) { }

        public bool ContainsScreenPoint(Vector2 screenPoint)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(Rect, screenPoint);
        }

        public Vector2 CenterScreenPoint()
        {
            Vector3 world = Rect.TransformPoint(Rect.rect.center);
            return RectTransformUtility.WorldToScreenPoint(null, world);
        }
    }

    public sealed class UiButton : UiWidget
    {
        public Text Label;
        public Image Background;
        public Action OnClick;
        public bool IsPrimary;

        public override void OnSubmit() => OnClick?.Invoke();

        public override void SetHighlighted(bool highlighted)
        {
            if (Background == null) return;
            Background.color = highlighted
                ? (IsPrimary ? new Color(0.98f, 0.71f, 0.24f, 0.95f) : new Color(0.28f, 0.34f, 0.46f, 0.98f))
                : (IsPrimary ? new Color(0.80f, 0.56f, 0.18f, 0.9f) : new Color(0.16f, 0.19f, 0.26f, 0.92f));
            if (Label != null) Label.color = highlighted && IsPrimary ? new Color(0.08f, 0.08f, 0.1f) : Palette.Text;
        }
    }

    /// <summary>A row that cycles through discrete choices with left/right.</summary>
    public sealed class UiOptionRow : UiWidget
    {
        public Text Label;
        public Text Value;
        public Image Background;
        public string[] Options = Array.Empty<string>();
        public int Index;
        public Action<int> OnChanged;
        public bool Wrap = true;

        public string Current => Options == null || Options.Length == 0 ? string.Empty : Options[Mathf.Clamp(Index, 0, Options.Length - 1)];

        public override void OnSubmit() => OnAdjust(1);

        public override void OnAdjust(int delta)
        {
            if (Options == null || Options.Length == 0) return;
            int next = Index + delta;
            if (Wrap)
            {
                next = (next % Options.Length + Options.Length) % Options.Length;
            }
            else
            {
                next = Mathf.Clamp(next, 0, Options.Length - 1);
            }
            if (next == Index) return;
            Index = next;
            Refresh();
            OnChanged?.Invoke(Index);
        }

        public void Refresh()
        {
            if (Value != null) Value.text = Current;
        }

        public override void SetHighlighted(bool highlighted)
        {
            if (Background != null)
            {
                Background.color = highlighted ? new Color(0.28f, 0.34f, 0.46f, 0.98f) : new Color(0.13f, 0.15f, 0.2f, 0.9f);
            }
            if (Value != null) Value.color = highlighted ? Palette.Accent : Palette.Text;
        }
    }

    /// <summary>Continuous 0..1 slider with a step size and a formatted readout.</summary>
    public sealed class UiSlider : UiWidget
    {
        public Text Label;
        public Text Value;
        public Image Fill;
        public Image Background;
        public float Value01;
        public float Step = 0.05f;
        public Action<float> OnChanged;
        public Func<float, string> Format;

        public override void OnAdjust(int delta)
        {
            float next = Mathf.Clamp01(Value01 + Step * delta);
            if (Mathf.Abs(next - Value01) < 0.0001f) return;
            Value01 = next;
            Refresh();
            OnChanged?.Invoke(Value01);
        }

        public void SetValue(float value, bool notify)
        {
            Value01 = Mathf.Clamp01(value);
            Refresh();
            if (notify) OnChanged?.Invoke(Value01);
        }

        public void Refresh()
        {
            if (Fill != null)
            {
                Fill.fillAmount = Value01;
            }
            if (Value != null)
            {
                Value.text = Format != null ? Format(Value01) : Mathf.RoundToInt(Value01 * 100f) + "%";
            }
        }

        public override void SetHighlighted(bool highlighted)
        {
            if (Background != null)
            {
                Background.color = highlighted ? new Color(0.28f, 0.34f, 0.46f, 0.98f) : new Color(0.13f, 0.15f, 0.2f, 0.9f);
            }
            if (Value != null) Value.color = highlighted ? Palette.Accent : Palette.Text;
        }
    }

    public sealed class UiToggleRow : UiWidget
    {
        public Text Label;
        public Text Value;
        public Image Background;
        public bool Value_ = true;
        public Action<bool> OnChanged;

        public override void OnSubmit() => OnAdjust(1);

        public override void OnAdjust(int delta)
        {
            if (delta == 0) return;
            Value_ = !Value_;
            Refresh();
            OnChanged?.Invoke(Value_);
        }

        public void SetValue(bool value, bool notify)
        {
            Value_ = value;
            Refresh();
            if (notify) OnChanged?.Invoke(Value_);
        }

        public void Refresh()
        {
            if (Value != null)
            {
                Value.text = Value_ ? "On" : "Off";
                Value.color = Value_ ? Palette.Success : Palette.TextDim;
            }
        }

        public override void SetHighlighted(bool highlighted)
        {
            if (Background != null)
            {
                Background.color = highlighted ? new Color(0.28f, 0.34f, 0.46f, 0.98f) : new Color(0.13f, 0.15f, 0.2f, 0.9f);
            }
            if (Label != null) Label.color = highlighted ? Palette.Accent : Palette.Text;
        }
    }

    /// <summary>Static or dynamic text block that can still be focused for scrolling.</summary>
    public sealed class UiTextBlock : UiWidget
    {
        public Text Text;
        public override bool Interactable => false;
    }

    public static class UiKit
    {
        public static RectTransform CreateCanvasObject(string name, Transform parent, int sortingOrder, out Canvas canvas)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return (RectTransform)go.transform;
        }

        public static RectTransform Empty(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            return rect;
        }

        public static Image Panel(string name, Transform parent, Color fill, Color border, int corner = 8)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            string key = "ui_panel_" + ColorKey(fill) + "_" + ColorKey(border) + "_" + corner;
            image.sprite = PlaceholderArt.Panel(key, fill, border, corner, 64, 64, 16f);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            return image;
        }

        public static Image Solid(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = PlaceholderArt.Rect("ui_solid_" + ColorKey(color), color, 8, 8, 16f);
            image.color = Color.white;
            return image;
        }

        public static Text Label(string name, Transform parent, string text, int fontSize, Color color,
            TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal, bool wrap = true)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text label = go.AddComponent<Text>();
            label.font = FontProvider.Get();
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = color;
            label.text = text;
            label.alignment = anchor;
            label.supportRichText = true;
            label.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        private static string ColorKey(Color color)
        {
            return Mathf.RoundToInt(color.r * 255f) + "_" + Mathf.RoundToInt(color.g * 255f) + "_" +
                   Mathf.RoundToInt(color.b * 255f) + "_" + Mathf.RoundToInt(color.a * 255f);
        }

        public static void Stretch(RectTransform rect, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static void Anchor(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        /// <summary>Row container used by list-style screens.</summary>
        public static RectTransform Row(string name, Transform parent, float height, float y)
        {
            RectTransform row = Empty(name, parent);
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.offsetMin = new Vector2(0f, 0f);
            row.offsetMax = new Vector2(0f, 0f);
            row.sizeDelta = new Vector2(0f, height);
            row.anchoredPosition = new Vector2(0f, -y);
            return row;
        }

        // ------------------------------------------------------------- widgets

        public static UiButton Button(string name, Transform parent, string text, Action onClick, bool primary = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = PlaceholderArt.Panel("ui_button_" + (primary ? "primary" : "normal"),
                primary ? new Color(0.80f, 0.56f, 0.18f, 0.9f) : new Color(0.16f, 0.19f, 0.26f, 0.92f),
                Palette.Border, 6, 48, 48, 16f);
            image.type = Image.Type.Sliced;

            UiButton button = go.AddComponent<UiButton>();
            button.Background = image;
            button.OnClick = onClick;
            button.IsPrimary = primary;
            button.Label = Label(name + "_Label", go.transform, text, primary ? 30 : 28,
                primary ? new Color(0.08f, 0.08f, 0.1f) : Palette.Text, TextAnchor.MiddleCenter);
            Stretch((RectTransform)button.Label.transform, 12f, 12f, 4f, 4f);
            return button;
        }

        public static UiOptionRow OptionRow(string name, Transform parent, string label, string[] options, int index, Action<int> onChanged)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = PlaceholderArt.Panel("ui_row", new Color(0.13f, 0.15f, 0.2f, 0.9f), new Color(0.22f, 0.26f, 0.34f, 0.9f), 5, 48, 48, 16f);
            image.type = Image.Type.Sliced;

            UiOptionRow row = go.AddComponent<UiOptionRow>();
            row.Background = image;
            row.Options = options ?? Array.Empty<string>();
            row.Index = Mathf.Clamp(index, 0, Mathf.Max(0, row.Options.Length - 1));
            row.OnChanged = onChanged;
            row.Label = Label(name + "_Label", go.transform, label, 26, Palette.Text);
            Anchor((RectTransform)row.Label.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(560f, 44f));

            row.Value = Label(name + "_Value", go.transform, row.Current, 26, Palette.Accent, TextAnchor.MiddleRight);
            Anchor((RectTransform)row.Value.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-26f, 0f), new Vector2(520f, 44f));
            return row;
        }

        public static UiSlider Slider(string name, Transform parent, string label, float value01, float step,
            Action<float> onChanged, Func<float, string> format = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = PlaceholderArt.Panel("ui_row", new Color(0.13f, 0.15f, 0.2f, 0.9f), new Color(0.22f, 0.26f, 0.34f, 0.9f), 5, 48, 48, 16f);
            image.type = Image.Type.Sliced;

            UiSlider slider = go.AddComponent<UiSlider>();
            slider.Background = image;
            slider.Value01 = Mathf.Clamp01(value01);
            slider.Step = step;
            slider.OnChanged = onChanged;
            slider.Format = format;
            slider.Label = Label(name + "_Label", go.transform, label, 26, Palette.Text);
            Anchor((RectTransform)slider.Label.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(560f, 44f));

            GameObject track = new GameObject(name + "_Track");
            track.transform.SetParent(go.transform, false);
            Image trackImage = track.AddComponent<Image>();
            trackImage.sprite = PlaceholderArt.Rect("ui_track", new Color(0.08f, 0.09f, 0.12f, 0.9f), 8, 8, 16f);
            RectTransform trackRect = (RectTransform)track.transform;
            Anchor(trackRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-260f, 0f), new Vector2(300f, 18f));

            GameObject fill = new GameObject(name + "_Fill");
            fill.transform.SetParent(track.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.sprite = PlaceholderArt.Rect("ui_fill", Palette.Accent, 8, 8, 16f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            slider.Fill = fillImage;
            Stretch((RectTransform)fill.transform);

            slider.Value = Label(name + "_Value", go.transform, string.Empty, 24, Palette.TextDim, TextAnchor.MiddleRight);
            Anchor((RectTransform)slider.Value.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-26f, 34f), new Vector2(300f, 40f));
            slider.Refresh();
            return slider;
        }

        public static UiToggleRow Toggle(string name, Transform parent, string label, bool value, Action<bool> onChanged)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = PlaceholderArt.Panel("ui_row", new Color(0.13f, 0.15f, 0.2f, 0.9f), new Color(0.22f, 0.26f, 0.34f, 0.9f), 5, 48, 48, 16f);
            image.type = Image.Type.Sliced;

            UiToggleRow toggle = go.AddComponent<UiToggleRow>();
            toggle.Background = image;
            toggle.OnChanged = onChanged;
            toggle.Label = Label(name + "_Label", go.transform, label, 26, Palette.Text);
            Anchor((RectTransform)toggle.Label.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(760f, 44f));

            toggle.Value = Label(name + "_Value", go.transform, string.Empty, 26, Palette.Success, TextAnchor.MiddleRight);
            Anchor((RectTransform)toggle.Value.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-26f, 0f), new Vector2(260f, 40f));
            toggle.SetValue(value, false);
            return toggle;
        }

        public static UiTextBlock TextBlock(string name, Transform parent, string text, int fontSize, Color color, TextAnchor anchor = TextAnchor.UpperLeft)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            UiTextBlock block = go.AddComponent<UiTextBlock>();
            block.Text = Label(name + "_Text", go.transform, text, fontSize, color, anchor);
            Stretch((RectTransform)block.Text.transform, 4f, 4f, 2f, 2f);
            return block;
        }

        /// <summary>Says whether a widget is left of / right of / above / below another.</summary>
        public static Vector2 OffsetBetween(UiWidget from, UiWidget to)
        {
            Vector2 a = from.CenterScreenPoint();
            Vector2 b = to.CenterScreenPoint();
            return b - a;
        }
    }
}
