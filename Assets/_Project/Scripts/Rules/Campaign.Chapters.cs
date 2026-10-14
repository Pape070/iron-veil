namespace IronVeil.Rules
{
    public static partial class Campaign
    {
        private static void BuildChapters(CampaignContent content)
        {
            BuildChaptersActOne(content);   // 1-4   Breach and Burn
            BuildChaptersActTwo(content);   // 5-8   The Hollow Coast
            BuildChaptersActThree(content); // 9-12  Iron Veil
        }

        // ------------------------------------------------------------------
        // Chapter authoring helpers
        // ------------------------------------------------------------------

        internal static ArenaDefinition Arena(
            string id,
            int chapter,
            string name,
            float width,
            float height,
            bool bossArena,
            string palette,
            string[] hazards,
            string[] spawnTags,
            string[] props,
            string music,
            bool cameraLocked = false,
            int parallax = 3)
        {
            ArenaDefinition arena = new ArenaDefinition
            {
                Id = id,
                ChapterIndex = chapter,
                DisplayName = name,
                Width = width,
                Height = height,
                IsBossArena = bossArena,
                CameraLocked = cameraLocked || bossArena,
                BackdropPaletteId = palette,
                ParallaxLayers = parallax,
                MusicId = music
            };
            if (hazards != null) arena.HazardIds.AddRange(hazards);
            if (spawnTags != null) arena.SpawnPointTags.AddRange(spawnTags);
            if (props != null) arena.PropIds.AddRange(props);
            return arena;
        }

        internal static SavepointDefinition Savepoint(
            string id,
            int chapter,
            string beatId,
            string label,
            string arenaId,
            bool preBoss,
            bool shop = true,
            bool respec = true,
            bool upgrade = true,
            bool travel = false)
        {
            return new SavepointDefinition
            {
                Id = id,
                ChapterIndex = chapter,
                BeatId = beatId,
                Label = label,
                ArenaId = arenaId ?? string.Empty,
                IsPreBoss = preBoss,
                AllowsShop = shop,
                AllowsRespec = respec,
                AllowsWeaponUpgrade = upgrade,
                AllowsTravel = travel
            };
        }

        internal static CheckpointDefinition Checkpoint(
            string id,
            int chapter,
            string beatId,
            string label,
            CheckpointTrigger trigger,
            bool autosave = true,
            string arenaId = null)
        {
            return new CheckpointDefinition
            {
                Id = id,
                ChapterIndex = chapter,
                BeatId = beatId,
                Label = label,
                Trigger = trigger,
                WritesAutosave = autosave,
                ArenaId = arenaId ?? string.Empty
            };
        }

        internal static ChapterBeat Beat(
            int order,
            BeatKind kind,
            string id,
            string title,
            string objective,
            string referenceId,
            bool autosave = true,
            bool optional = false)
        {
            return new ChapterBeat
            {
                Id = id,
                Kind = kind,
                Title = title,
                Objective = objective,
                ReferenceId = referenceId ?? string.Empty,
                Order = order,
                AutosaveOnComplete = autosave,
                IsOptional = optional
            };
        }

        internal static ComicPageDefinition Page(
            string id,
            int chapter,
            int number,
            string beatId,
            string title,
            ComicPanelDefinition[] panels,
            string music = null,
            string category = "story")
        {
            ComicPageDefinition page = new ComicPageDefinition
            {
                Id = id,
                ChapterIndex = chapter,
                PageNumber = number,
                BeatId = beatId,
                Title = title,
                MusicId = music ?? string.Empty,
                GalleryCategory = category,
                UnlocksInGallery = true
            };
            if (panels != null) page.Panels.AddRange(panels);
            return page;
        }

        internal static ComicPanelDefinition Narration(
            string id,
            ComicShotType shot,
            string palette,
            string caption,
            ComicActorPlacement[] actors = null,
            string effect = "",
            string sfx = "",
            float hold = 1.8f)
        {
            ComicPanelDefinition panel = new ComicPanelDefinition
            {
                Id = id,
                Shot = shot,
                BackgroundPaletteId = palette,
                Caption = caption,
                EffectKey = effect,
                SfxText = sfx,
                HoldSeconds = hold
            };
            if (actors != null) panel.Actors.AddRange(actors);
            return panel;
        }

        internal static ComicPanelDefinition Speech(
            string id,
            ComicShotType shot,
            string palette,
            string speaker,
            string speech,
            ComicActorPlacement[] actors = null,
            string effect = "",
            string sfx = "",
            float hold = 2.2f,
            string caption = "")
        {
            ComicPanelDefinition panel = new ComicPanelDefinition
            {
                Id = id,
                Shot = shot,
                BackgroundPaletteId = palette,
                Speaker = speaker,
                Speech = speech,
                Caption = caption,
                EffectKey = effect,
                SfxText = sfx,
                HoldSeconds = hold
            };
            if (actors != null) panel.Actors.AddRange(actors);
            return panel;
        }

        internal static ComicActorPlacement A(
            string characterId,
            float x,
            string pose = "idle",
            string expression = "neutral",
            float scale = 1f,
            bool flip = false,
            bool silhouette = false,
            float y = 0f)
        {
            return new ComicActorPlacement
            {
                CharacterId = characterId,
                PoseId = pose,
                ExpressionId = expression,
                X = x,
                Y = y,
                Scale = scale,
                FlipX = flip,
                IsSilhouette = silhouette
            };
        }

        internal static EncounterDefinition Encounter(
            string id,
            int chapter,
            string name,
            string arenaId,
            int seconds,
            EncounterWave[] waves,
            string objective = null,
            string rewardTable = null,
            bool isBossArena = false,
            string music = null)
        {
            EncounterDefinition encounter = new EncounterDefinition
            {
                Id = id,
                DisplayName = name,
                ChapterIndex = chapter,
                ArenaId = arenaId,
                IsArena = true,
                IsBossArena = isBossArena,
                TargetSeconds = seconds,
                Objective = objective ?? "Clear the area",
                RewardTableId = rewardTable ?? string.Empty,
                MusicId = music ?? string.Empty
            };
            if (waves != null) encounter.Waves.AddRange(waves);
            return encounter;
        }

        internal static EncounterWave Wave(string name, float delay, bool requiresClear, SpawnEntry[] spawns)
        {
            EncounterWave wave = new EncounterWave
            {
                Name = name,
                Delay = delay,
                RequiresWaveClear = requiresClear
            };
            if (spawns != null) wave.Spawns.AddRange(spawns);
            return wave;
        }

        internal static SpawnEntry S(string archetypeId, int count, string tag = "ground", float interval = 0.6f, bool elite = false)
        {
            return new SpawnEntry
            {
                ArchetypeId = archetypeId,
                Count = count,
                SpawnPointTag = tag,
                SpawnInterval = interval,
                ForceElite = elite
            };
        }

        /// <summary>Standard spawn tags every arena supports unless it overrides them.</summary>
        private static readonly string[] GroundTags = { "ground", "ledge", "air", "spawn_left", "spawn_right" };

        internal static string[] StandardSpawnTags()
        {
            return (string[])GroundTags.Clone();
        }
    }
}
