using System;
using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    /// <summary>Minimal service registry. GameRoot owns the lifetime of everything here.</summary>
    public static class Services
    {
        private static readonly Dictionary<Type, object> Registry = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            if (service == null) return;
            Registry[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            return Registry.TryGetValue(typeof(T), out object service) ? service as T : null;
        }

        public static bool Has<T>() where T : class => Registry.ContainsKey(typeof(T));

        public static void Clear() => Registry.Clear();
    }

    /// <summary>
    /// Holds the campaign data. The content is authored in code (IronVeil.Rules
    /// Campaign.Build) and optionally overridden by baked ScriptableObject assets
    /// created by the editor tooling, so designers can tweak numbers without
    /// touching code.
    /// </summary>
    public sealed class ContentDatabase
    {
        private CampaignContent _content;
        private ValidationReport _report;

        public CampaignContent Content
        {
            get
            {
                if (_content == null) Load();
                return _content;
            }
        }

        public ValidationReport Report
        {
            get
            {
                if (_content == null) Load();
                return _report;
            }
        }

        private void Load()
        {
            _content = Campaign.Build();
            _report = ContentValidator.Validate(_content);
            if (!_report.IsValid)
            {
                Debug.LogError("[IronVeil] Campaign content failed validation:\n" + _report.Summary());
            }
            else if (_report.Warnings.Count > 0)
            {
                Debug.LogWarning("[IronVeil] Campaign content warnings:\n" + _report.Summary());
            }
        }

        public CharacterDefinition Character(CharacterId id) => Content.Character(id);
        public WeaponDefinition Weapon(string id) => Content.Weapon(id);
        public GearDefinition Gear(string id) => Content.Gear(id);
        public EnemyArchetype Enemy(string id) => Content.Enemy(id);
        public BossDefinition Boss(string id) => Content.Boss(id);
        public QuestDefinition Quest(string id) => Content.Quest(id);
        public ChapterDefinition Chapter(int index) => Content.Chapter(index);
        public ComicPageDefinition ComicPage(string id) => Content.FindComicPage(id);
        public EncounterDefinition Encounter(string id) => Content.Encounter(id);
        public DifficultyProfile Difficulty(Difficulty difficulty) => DifficultyProfile.For(difficulty);
    }

    /// <summary>
    /// Generates every sprite at runtime. The game ships playable with no art
    /// assets: characters, enemies, bosses and comic panels are drawn from code so
    /// the whole campaign can be played and tested before a single texture exists.
    /// Replace keys with real sprites later by assigning them in the palette.
    /// </summary>
    public static class PlaceholderArt
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        public static void Clear()
        {
            foreach (KeyValuePair<string, Sprite> pair in Cache)
            {
                if (pair.Value != null) UnityEngine.Object.Destroy(pair.Value);
            }
            foreach (KeyValuePair<string, Texture2D> pair in Textures)
            {
                if (pair.Value != null) UnityEngine.Object.Destroy(pair.Value);
            }
            Cache.Clear();
            Textures.Clear();
        }

        /// <summary>Flat coloured rectangle, pixels per unit 16 by default.</summary>
        public static Sprite Rect(string key, Color color, int width = 32, int height = 32, float pixelsPerUnit = 16f)
        {
            string cacheKey = "rect:" + key;
            if (Cache.TryGetValue(cacheKey, out Sprite cached) && cached != null) return cached;

            Texture2D texture = NewTexture(cacheKey, width, height);
            Color32[] pixels = new Color32[width * height];
            Color32 fill = color;
            for (int i = 0; i < pixels.Length; i++) pixels[i] = fill;
            texture.SetPixels32(pixels);
            texture.Apply();
            return Commit(cacheKey, texture, pixelsPerUnit);
        }

        /// <summary>Vertical gradient, useful for backdrops and comic panels.</summary>
        public static Sprite Gradient(string key, Color top, Color bottom, int width = 64, int height = 64, float pixelsPerUnit = 16f)
        {
            string cacheKey = "gradient:" + key;
            if (Cache.TryGetValue(cacheKey, out Sprite cached) && cached != null) return cached;

            Texture2D texture = NewTexture(cacheKey, width, height);
            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                Color row = Color.Lerp(bottom, top, y / (float)Mathf.Max(1, height - 1));
                Color32 fill = row;
                for (int x = 0; x < width; x++) pixels[y * width + x] = fill;
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Commit(cacheKey, texture, pixelsPerUnit);
        }

        public static Sprite Circle(string key, Color color, int diameter = 32, float pixelsPerUnit = 16f)
        {
            string cacheKey = "circle:" + key;
            if (Cache.TryGetValue(cacheKey, out Sprite cached) && cached != null) return cached;

            Texture2D texture = NewTexture(cacheKey, diameter, diameter);
            Color32[] pixels = new Color32[diameter * diameter];
            Color32 fill = color;
            Color32 clear = new Color(0f, 0f, 0f, 0f);
            float radius = diameter * 0.5f;
            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x + 0.5f - radius;
                    float dy = y + 0.5f - radius;
                    pixels[y * diameter + x] = dx * dx + dy * dy <= radius * radius ? fill : clear;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Commit(cacheKey, texture, pixelsPerUnit);
        }

        /// <summary>Rounded panel with a border, used for every UI surface.</summary>
        public static Sprite Panel(string key, Color fill, Color border, int radius = 6, int width = 64, int height = 64, float pixelsPerUnit = 16f)
        {
            string cacheKey = "panel:" + key;
            if (Cache.TryGetValue(cacheKey, out Sprite cached) && cached != null) return cached;

            Texture2D texture = NewTexture(cacheKey, width, height);
            Color32[] pixels = new Color32[width * height];
            Color32 fillColor = fill;
            Color32 borderColor = border;
            Color32 clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float edge = RoundedDistance(x, y, width, height, radius);
                    if (edge > 1f) pixels[y * width + x] = clear;
                    else if (edge > 0f) pixels[y * width + x] = borderColor;
                    else pixels[y * width + x] = fillColor;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Commit(cacheKey, texture, pixelsPerUnit);
        }

        /// <summary>
        /// A simple readable actor: body, head and a facing highlight. Used for the
        /// player and for every enemy until real art lands.
        /// </summary>
        public static Sprite Actor(string key, Color body, Color accent, int width = 24, int height = 36)
        {
            string cacheKey = "actor:" + key;
            if (Cache.TryGetValue(cacheKey, out Sprite cached) && cached != null) return cached;

            Texture2D texture = NewTexture(cacheKey, width, height);
            Color32[] pixels = new Color32[width * height];
            Color32 bodyColor = body;
            Color32 accentColor = accent;
            Color32 clear = new Color(0f, 0f, 0f, 0f);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            int headHeight = Mathf.Max(6, height / 4);
            int headRadius = Mathf.Max(4, width / 3);
            int headCenterY = height - headHeight / 2 - 2;
            int headCenterX = width / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inHead = (x - headCenterX) * (x - headCenterX) + (y - headCenterY) * (y - headCenterY) <= headRadius * headRadius;
                    bool inBody = y < height - headHeight && x >= width / 2 - 5 && x <= width / 2 + 5;
                    bool isAccent = inBody && (y % 8 == 0 || x == width / 2 + 5);

                    if (inHead) pixels[y * width + x] = accentColor;
                    else if (inBody) pixels[y * width + x] = isAccent ? accentColor : bodyColor;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Commit(cacheKey, texture, 16f);
        }

        /// <summary>Wide silhouette used for bosses and comic establishing shots.</summary>
        public static Sprite Silhouette(string key, Color color, int width = 64, int height = 48)
        {
            string cacheKey = "silhouette:" + key;
            if (Cache.TryGetValue(cacheKey, out Sprite cached) && cached != null) return cached;

            Texture2D texture = NewTexture(cacheKey, width, height);
            Color32[] pixels = new Color32[width * height];
            Color32 fill = color;
            Color32 clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < height; y++)
            {
                float t = y / (float)Mathf.Max(1, height - 1);
                int halfWidth = Mathf.RoundToInt(width * 0.5f * Mathf.Sin(t * Mathf.PI) + width * 0.16f);
                for (int x = 0; x < width; x++)
                {
                    bool inside = Mathf.Abs(x - width / 2) <= halfWidth;
                    pixels[y * width + x] = inside ? fill : clear;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Commit(cacheKey, texture, 16f);
        }

        private static float RoundedDistance(int x, int y, int width, int height, int radius)
        {
            float r = Mathf.Min(radius, Mathf.Min(width, height) / 2f);
            float cx = Mathf.Clamp(x + 0.5f, r, width - r);
            float cy = Mathf.Clamp(y + 0.5f, r, height - r);
            float dx = x + 0.5f - cx;
            float dy = y + 0.5f - cy;
            float distance = Mathf.Sqrt(dx * dx + dy * dy) - r;
            if (distance <= -1f) return -1f;   // inside
            if (distance >= 0f) return 1f;     // outside
            return 0f;                          // border band
        }

        private static Texture2D NewTexture(string key, int width, int height)
        {
            Texture2D texture = new Texture2D(Mathf.Max(1, width), Mathf.Max(1, height), TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "IronVeil_" + key
            };
            Textures[key] = texture;
            return texture;
        }

        private static Sprite Commit(string key, Texture2D texture, float pixelsPerUnit)
        {
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            sprite.name = "IronVeil_" + key;
            Cache[key] = sprite;
            return sprite;
        }
    }

    /// <summary>Palette shared by gameplay and UI so the game reads as one product.</summary>
    public static class Palette
    {
        public static readonly Color Background = new Color(0.05f, 0.06f, 0.09f, 1f);
        public static readonly Color Panel = new Color(0.10f, 0.12f, 0.17f, 0.96f);
        public static readonly Color PanelLight = new Color(0.16f, 0.19f, 0.26f, 1f);
        public static readonly Color Border = new Color(0.28f, 0.34f, 0.45f, 1f);
        public static readonly Color Text = new Color(0.92f, 0.94f, 0.98f, 1f);
        public static readonly Color TextDim = new Color(0.62f, 0.67f, 0.76f, 1f);
        public static readonly Color Accent = new Color(0.98f, 0.71f, 0.24f, 1f);
        public static readonly Color AccentCool = new Color(0.36f, 0.78f, 0.98f, 1f);
        public static readonly Color Danger = new Color(0.92f, 0.32f, 0.31f, 1f);
        public static readonly Color Success = new Color(0.44f, 0.85f, 0.52f, 1f);
        public static readonly Color KadePrimary = new Color(0.86f, 0.48f, 0.24f, 1f);
        public static readonly Color KadeAccent = new Color(0.55f, 0.58f, 0.64f, 1f);
        public static readonly Color VeraPrimary = new Color(0.38f, 0.68f, 0.92f, 1f);
        public static readonly Color VeraAccent = new Color(0.85f, 0.90f, 0.98f, 1f);

        public static Color CharacterPrimary(CharacterId id) => id == CharacterId.Vera ? VeraPrimary : KadePrimary;

        public static Color CharacterAccent(CharacterId id) => id == CharacterId.Vera ? VeraAccent : KadeAccent;

        /// <summary>Backdrop colours for the comic panel palettes referenced by the content.</summary>
        public static void ComicPalette(string paletteId, out Color top, out Color bottom, out Color ink)
        {
            switch (paletteId)
            {
                case "docks_dusk": top = new Color(0.16f, 0.24f, 0.34f); bottom = new Color(0.08f, 0.12f, 0.20f); ink = new Color(0.85f, 0.88f, 0.94f); break;
                case "rooftops_night": top = new Color(0.10f, 0.14f, 0.26f); bottom = new Color(0.04f, 0.05f, 0.12f); ink = new Color(0.80f, 0.86f, 0.98f); break;
                case "foundry_glow": top = new Color(0.34f, 0.18f, 0.10f); bottom = new Color(0.12f, 0.06f, 0.05f); ink = new Color(0.99f, 0.80f, 0.55f); break;
                case "opera_red": top = new Color(0.30f, 0.09f, 0.14f); bottom = new Color(0.09f, 0.03f, 0.06f); ink = new Color(0.98f, 0.82f, 0.82f); break;
                case "cathedral_hollow": top = new Color(0.18f, 0.18f, 0.24f); bottom = new Color(0.05f, 0.05f, 0.09f); ink = new Color(0.88f, 0.90f, 0.96f); break;
                case "glass_white": top = new Color(0.72f, 0.80f, 0.88f); bottom = new Color(0.28f, 0.36f, 0.48f); ink = new Color(0.08f, 0.10f, 0.16f); break;
                case "yards_blood": top = new Color(0.28f, 0.14f, 0.12f); bottom = new Color(0.10f, 0.05f, 0.05f); ink = new Color(0.95f, 0.86f, 0.80f); break;
                case "runoff_green": top = new Color(0.12f, 0.26f, 0.18f); bottom = new Color(0.03f, 0.09f, 0.07f); ink = new Color(0.80f, 0.98f, 0.84f); break;
                case "ossuary_bone": top = new Color(0.42f, 0.40f, 0.34f); bottom = new Color(0.13f, 0.13f, 0.12f); ink = new Color(0.98f, 0.96f, 0.90f); break;
                case "undercity_black": top = new Color(0.09f, 0.10f, 0.14f); bottom = new Color(0.02f, 0.02f, 0.04f); ink = new Color(0.86f, 0.92f, 0.99f); break;
                case "veil_core": top = new Color(0.62f, 0.68f, 0.98f); bottom = new Color(0.14f, 0.16f, 0.34f); ink = new Color(0.05f, 0.06f, 0.12f); break;
                default: top = new Color(0.20f, 0.22f, 0.28f); bottom = new Color(0.07f, 0.08f, 0.11f); ink = new Color(0.90f, 0.92f, 0.96f); break;
            }
        }
    }

    public enum AudioChannel
    {
        Master = 0,
        Music = 1,
        Sfx = 2,
        Voice = 3
    }

    /// <summary>
    /// Volume control, mono/stereo output and generated placeholder sounds. Music
    /// and effects are synthesised at runtime so the game has audio without any
    /// imported clips; swap in real clips by assigning them in the clip table.
    /// </summary>
    public sealed class AudioService
    {
        private const string MusicSourceName = "IronVeil_Music";
        private const string SfxSourceName = "IronVeil_Sfx";

        private readonly Dictionary<AudioChannel, float> _volumes = new Dictionary<AudioChannel, float>
        {
            { AudioChannel.Master, 0.8f },
            { AudioChannel.Music, 0.7f },
            { AudioChannel.Sfx, 0.9f },
            { AudioChannel.Voice, 1f }
        };

        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private AudioSource _music;
        private AudioSource _sfx;
        private GameObject _root;
        private bool _muted;

        /// <summary>Mono downmixes everything, standard stereo keeps channels separate.</summary>
        public bool MonoOutput { get; private set; }

        public void Initialize()
        {
            if (_root != null) return;
            _root = new GameObject("IronVeil_Audio");
            UnityEngine.Object.DontDestroyOnLoad(_root);

            _music = CreateSource(MusicSourceName, true);
            _sfx = CreateSource(SfxSourceName, false);
        }

        private AudioSource CreateSource(string name, bool loop)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_root.transform, false);
            AudioSource source = go.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 1f;
            return source;
        }

        public float GetVolume(AudioChannel channel)
        {
            return _volumes.TryGetValue(channel, out float value) ? value : 1f;
        }

        public void SetVolume(AudioChannel channel, float value)
        {
            _volumes[channel] = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        public void SetMuted(bool muted)
        {
            _muted = muted;
            ApplyVolumes();
        }

        public bool IsMuted => _muted;

        private void ApplyVolumes()
        {
            float master = _muted ? 0f : GetVolume(AudioChannel.Master);
            if (_music != null) _music.volume = master * GetVolume(AudioChannel.Music);
            if (_sfx != null) _sfx.volume = master * GetVolume(AudioChannel.Sfx);
        }

        /// <summary>
        /// Switches the output configuration between stereo and mono. Unity only
        /// applies this when the audio device is recreated, and it can fail on some
        /// platforms, so the result is reported back to the caller.
        /// </summary>
        public bool SetMonoOutput(bool mono)
        {
            MonoOutput = mono;

            AudioConfiguration configuration = AudioSettings.GetConfiguration();
            AudioSpeakerMode target = mono ? AudioSpeakerMode.Mono : AudioSpeakerMode.Stereo;
            if (configuration.speakerMode == target) return true;

            configuration.speakerMode = target;
            if (configuration.dspBufferSize <= 0) configuration.dspBufferSize = 1024;
            if (configuration.sampleRate <= 0) configuration.sampleRate = 48000;

            try
            {
                bool applied = AudioSettings.Reset(configuration);
                if (!applied) Debug.LogWarning("[IronVeil] Audio output mode could not be changed at runtime.");
                return applied;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[IronVeil] Audio output mode change failed: " + exception.Message);
                return false;
            }
        }

        public void PlayMusic(string clipId)
        {
            if (_music == null) return;
            AudioClip clip = Clip(clipId, 8f);
            if (_music.clip == clip && _music.isPlaying) return;
            _music.clip = clip;
            _music.Play();
        }

        public void StopMusic()
        {
            if (_music != null) _music.Stop();
        }

        public void PlaySfx(string clipId, float pitch = 1f)
        {
            if (_sfx == null) return;
            AudioClip clip = Clip(clipId, 0.18f);
            _sfx.pitch = Mathf.Clamp(pitch, 0.4f, 2f);
            _sfx.PlayOneShot(clip, GetVolume(AudioChannel.Sfx));
        }

        /// <summary>
        /// Generates a short synthesised clip. Keeps the project asset free while
        /// still giving every action audible feedback.
        /// </summary>
        private AudioClip Clip(string clipId, float duration)
        {
            if (string.IsNullOrEmpty(clipId)) clipId = "default";
            if (_clips.TryGetValue(clipId, out AudioClip existing) && existing != null) return existing;

            int sampleRate = 44100;
            int samples = Mathf.Max(256, Mathf.RoundToInt(sampleRate * duration));
            float[] data = new float[samples];
            System.Random random = new System.Random(StableHash(clipId));

            float baseFrequency = BaseFrequency(clipId);
            bool noise = clipId.StartsWith("sfx_hit", StringComparison.Ordinal) ||
                         clipId.StartsWith("sfx_explosion", StringComparison.Ordinal);

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-t * (duration > 1f ? 0.6f : 6f));
                float value = noise
                    ? (float)(random.NextDouble() * 2.0 - 1.0)
                    : Mathf.Sin(2f * Mathf.PI * baseFrequency * t) * 0.6f +
                      Mathf.Sin(2f * Mathf.PI * baseFrequency * 2f * t) * 0.2f;
                data[i] = value * envelope * 0.35f;
            }

            AudioClip clip = AudioClip.Create("IronVeil_" + clipId, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            _clips[clipId] = clip;
            return clip;
        }

        private static float BaseFrequency(string clipId)
        {
            if (clipId.StartsWith("mx_", StringComparison.Ordinal)) return 110f;
            if (clipId.StartsWith("sfx_hit", StringComparison.Ordinal)) return 180f;
            if (clipId.StartsWith("sfx_shoot", StringComparison.Ordinal)) return 420f;
            if (clipId.StartsWith("sfx_explosion", StringComparison.Ordinal)) return 90f;
            if (clipId.StartsWith("sfx_ui", StringComparison.Ordinal)) return 660f;
            return 330f;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash == 0 ? 1 : hash;
            }
        }
    }
}
