using System;
using System.Collections.Generic;

namespace IronVeil.Rules
{
    public enum BeatKind
    {
        Comic = 0,
        Dialogue = 1,
        Exploration = 2,
        Arena = 3,
        MiniBossFight = 4,
        BossFight = 5,
        Puzzle = 6,
        Savepoint = 7,
        Shop = 8,
        Choice = 9,
        SideQuestHook = 10,
        ChapterComplete = 11
    }

    public enum CheckpointTrigger
    {
        ChapterStart = 0,
        EnterArena = 1,
        AfterCutscene = 2,
        PreBoss = 3,
        ChapterComplete = 4,
        Manual = 5,
        MidChapter = 6
    }

    public enum ComicShotType
    {
        Establish = 0,
        Wide = 1,
        Medium = 2,
        CloseUp = 3,
        ExtremeCloseUp = 4,
        Insert = 5,
        OverShoulder = 6,
        Birdseye = 7
    }

    [Serializable]
    public sealed class ComicActorPlacement
    {
        public string CharacterId = string.Empty;
        public string PoseId = "idle";
        public string ExpressionId = "neutral";
        public float X = 0.5f;
        public float Y = 0f;
        public float Scale = 1f;
        public bool FlipX = false;
        public bool IsSilhouette = false;
    }

    /// <summary>
    /// One comic panel. Cutscenes are authored as data and drawn procedurally, so
    /// there is no video pipeline and text is localisable.
    /// </summary>
    [Serializable]
    public sealed class ComicPanelDefinition
    {
        public string Id = string.Empty;
        public ComicShotType Shot = ComicShotType.Medium;
        public string BackgroundPaletteId = "ruins_day";
        public List<ComicActorPlacement> Actors = new List<ComicActorPlacement>();

        public string Speaker = string.Empty;
        public string Speech = string.Empty;
        public string Caption = string.Empty;
        public string SfxText = string.Empty;

        /// <summary>Presentation effect key: "speedlines", "impact", "flash", "shake", "zoom".</summary>
        public string EffectKey = string.Empty;

        /// <summary>Minimum seconds the panel holds before auto-advance is allowed.</summary>
        public float HoldSeconds = 1.6f;

        public bool AutoAdvance = false;

        public ComicActorPlacement AddActor(
            string characterId,
            float x,
            float y,
            string pose = "idle",
            string expression = "neutral",
            float scale = 1f,
            bool flipX = false,
            bool silhouette = false)
        {
            ComicActorPlacement actor = new ComicActorPlacement
            {
                CharacterId = characterId,
                PoseId = pose,
                ExpressionId = expression,
                X = x,
                Y = y,
                Scale = scale,
                FlipX = flipX,
                IsSilhouette = silhouette
            };
            Actors.Add(actor);
            return actor;
        }
    }

    [Serializable]
    public sealed class ComicPageDefinition
    {
        public string Id = string.Empty;
        public int ChapterIndex = 1;
        public int PageNumber = 1;
        public string BeatId = string.Empty;
        public string Title = string.Empty;
        public List<ComicPanelDefinition> Panels = new List<ComicPanelDefinition>();

        /// <summary>True when the page is added to the gallery once seen.</summary>
        public bool UnlocksInGallery = true;

        /// <summary>Gallery tab: "story", "boss", "bestiary", "concept".</summary>
        public string GalleryCategory = "story";

        public string MusicId = string.Empty;
        public bool IsSkippable = true;
    }

    [Serializable]
    public sealed class ChapterBeat
    {
        public string Id = string.Empty;
        public BeatKind Kind = BeatKind.Exploration;
        public string Title = string.Empty;
        public string Objective = string.Empty;

        /// <summary>Comic page id, arena id, boss id or quest id depending on Kind.</summary>
        public string ReferenceId = string.Empty;

        public int Order = 0;

        /// <summary>Autosave fires when this beat completes.</summary>
        public bool AutosaveOnComplete = true;

        public bool IsOptional = false;
    }

    [Serializable]
    public sealed class SavepointDefinition
    {
        public string Id = string.Empty;
        public int ChapterIndex = 1;
        public string BeatId = string.Empty;
        public string Label = string.Empty;
        public string ArenaId = string.Empty;

        /// <summary>Savepoints before boss fights are guaranteed and restore full resources.</summary>
        public bool IsPreBoss = false;

        public bool AllowsShop = true;
        public bool AllowsRespec = true;
        public bool AllowsWeaponUpgrade = true;
        public bool AllowsTravel = false;
    }

    [Serializable]
    public sealed class CheckpointDefinition
    {
        public string Id = string.Empty;
        public int ChapterIndex = 1;
        public string BeatId = string.Empty;
        public string Label = string.Empty;
        public CheckpointTrigger Trigger = CheckpointTrigger.MidChapter;

        /// <summary>Autosave (true) versus respawn-only marker (false).</summary>
        public bool WritesAutosave = true;

        public string ArenaId = string.Empty;
    }

    [Serializable]
    public sealed class ArenaDefinition
    {
        public string Id = string.Empty;
        public int ChapterIndex = 1;
        public string DisplayName = string.Empty;
        public float Width = 48f;
        public float Height = 18f;
        public bool IsBossArena = false;
        public bool CameraLocked = false;
        public string BackdropPaletteId = "ruins_day";
        public int ParallaxLayers = 3;
        public List<string> HazardIds = new List<string>();
        public List<string> SpawnPointTags = new List<string>();
        public List<string> PropIds = new List<string>();
        public string MusicId = string.Empty;
    }

    /// <summary>One chapter of the campaign. Twelve chapters, roughly two hours each.</summary>
    [Serializable]
    public sealed class ChapterDefinition
    {
        public int Index = 1;
        public string Title = string.Empty;
        public string Subtitle = string.Empty;
        public string ActTitle = string.Empty;
        public string Synopsis = string.Empty;
        public string LocationName = string.Empty;
        public string LocationId = string.Empty;

        public List<ChapterBeat> Beats = new List<ChapterBeat>();
        public List<ComicPageDefinition> ComicPages = new List<ComicPageDefinition>();
        public List<ArenaDefinition> Arenas = new List<ArenaDefinition>();
        public List<SavepointDefinition> Savepoints = new List<SavepointDefinition>();
        public List<CheckpointDefinition> Checkpoints = new List<CheckpointDefinition>();

        public string BossId = string.Empty;
        public List<string> MiniBossIds = new List<string>();
        public List<string> SideQuestIds = new List<string>();
        public List<string> NewEnemyIds = new List<string>();

        /// <summary>Upgrades (weapon or gear ids) unlocked by finishing the chapter.</summary>
        public List<string> ChapterRewardIds = new List<string>();

        public string UnlockFlagOnComplete = string.Empty;
        public int EstimatedMinutes = 120;
        public int RecommendedLevel = 1;
        public int MinimumLevel = 1;
        public string EndingFlag = string.Empty;

        public ChapterBeat Beat(string id)
        {
            if (Beats == null) return null;
            for (int i = 0; i < Beats.Count; i++)
            {
                if (Beats[i] != null && Beats[i].Id == id) return Beats[i];
            }
            return null;
        }

        public SavepointDefinition Savepoint(string id)
        {
            if (Savepoints == null) return null;
            for (int i = 0; i < Savepoints.Count; i++)
            {
                if (Savepoints[i] != null && Savepoints[i].Id == id) return Savepoints[i];
            }
            return null;
        }

        public bool HasPreBossSavepoint()
        {
            if (Savepoints == null) return false;
            for (int i = 0; i < Savepoints.Count; i++)
            {
                if (Savepoints[i] != null && Savepoints[i].IsPreBoss) return true;
            }
            return false;
        }

        public int TotalComicPages => ComicPages == null ? 0 : ComicPages.Count;

        public int TotalPanels()
        {
            int total = 0;
            if (ComicPages == null) return 0;
            for (int i = 0; i < ComicPages.Count; i++)
            {
                if (ComicPages[i] != null && ComicPages[i].Panels != null) total += ComicPages[i].Panels.Count;
            }
            return total;
        }
    }

    /// <summary>Chapter pacing, level guidance and scaling curves.</summary>
    public static class ChapterRules
    {
        public const int ChapterCount = 12;

        /// <summary>Minutes per chapter, summing to 25 hours of main-path play.</summary>
        private static readonly int[] Minutes =
        {
            105, 110, 115, 120, 120, 125, 125, 130, 130, 135, 140, 145
        };

        private static readonly int[] RecommendedLevels =
        {
            1, 6, 12, 18, 24, 30, 35, 40, 45, 50, 55, 58
        };

        public static int EstimatedMinutes(int chapterIndex)
        {
            int index = MathLite.Clamp(chapterIndex, 1, ChapterCount) - 1;
            return Minutes[index];
        }

        public static int TotalMinutes()
        {
            int total = 0;
            for (int i = 0; i < Minutes.Length; i++) total += Minutes[i];
            return total;
        }

        /// <summary>Title of the act a chapter belongs to (1-4, 5-8, 9-12).</summary>
        public static string ActTitle(int chapterIndex)
        {
            int chapter = MathLite.Clamp(chapterIndex, 1, ChapterCount);
            if (chapter <= 4) return "Act I - Breach and Burn";
            if (chapter <= 8) return "Act II - The Hollow Coast";
            return "Act III - Iron Veil";
        }

        public static int RecommendedLevel(int chapterIndex)
        {
            int index = MathLite.Clamp(chapterIndex, 1, ChapterCount) - 1;
            return RecommendedLevels[index];
        }

        public static int MinimumLevel(int chapterIndex)
        {
            // A little slack below the recommendation so players are never hard-walled,
            // but far enough under that skipping content is punished.
            return MathLite.Max(1, RecommendedLevel(chapterIndex) - 4);
        }

        public static float XpMultiplier(int chapterIndex)
        {
            return 1f + 0.5f * MathLite.Max(0, chapterIndex - 1);
        }

        public static float EnemyHealthMultiplier(int chapterIndex)
        {
            return 1f + 0.85f * MathLite.Max(0, chapterIndex - 1);
        }

        public static float EnemyDamageMultiplier(int chapterIndex)
        {
            return 1f + 0.5f * MathLite.Max(0, chapterIndex - 1);
        }

        /// <summary>Total XP the main path grants, used to sanity check the curve.</summary>
        public static int TotalMainPathXp(int enemiesPerChapter, DifficultyProfile difficulty)
        {
            int total = 0;
            DifficultyProfile profile = difficulty ?? DifficultyProfile.For(Difficulty.Normal);
            for (int chapter = 1; chapter <= ChapterCount; chapter++)
            {
                total += enemiesPerChapter * XpCurve.KillReward(1, chapter, profile);
                total += XpCurve.BossReward(chapter, profile);
                total += 2 * XpCurve.MiniBossReward(chapter, profile);
            }
            return total;
        }
    }
}
