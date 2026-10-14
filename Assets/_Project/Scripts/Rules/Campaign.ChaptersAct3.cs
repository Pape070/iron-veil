namespace IronVeil.Rules
{
    public static partial class Campaign
    {
        private static void BuildChaptersActThree(CampaignContent content)
        {
            /* ============================================================
               CHAPTER 9 - RUNOFF CATHEDRAL  (130 min, level 45)
               ============================================================ */
            ChapterDefinition c9 = new ChapterDefinition
            {
                Index = 9,
                Title = "Runoff Cathedral",
                Subtitle = "Down the drain of the Veil",
                ActTitle = ChapterRules.ActTitle(9),
                LocationName = "Runoff Cathedral",
                LocationId = "loc_runoff",
                Synopsis =
                    "The Veil flushes what it cannot archive into cisterns under the coast. The runoff has " +
                    "grown a shape of its own, and Sump sits at the bottom of the vat while the Directorate " +
                    "drills through the ceiling above it.",
                BossId = "boss_sump",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(9),
                RecommendedLevel = ChapterRules.RecommendedLevel(9),
                MinimumLevel = ChapterRules.MinimumLevel(9),
                UnlockFlagOnComplete = "flag_ch09_complete"
            };
            c9.MiniBossIds.AddRange(new[] { "mb_drillmaster", "mb_hound_mother" });
            c9.SideQuestIds.AddRange(new[] { "q09_clean_vat", "q09_bore_no_more", "q09_purge" });
            c9.NewEnemyIds.AddRange(new[] { "e_echo_wisp" });
            c9.ChapterRewardIds.AddRange(new[] { "g_trinket_kinetic" });

            c9.Arenas.Add(Arena("arena_ch09_stair", 9, "Cistern Stair", 46f, 18f, false, "runoff_green",
                new[] { "hz_runoff_pool", "hz_steam_vent" }, StandardSpawnTags(),
                new[] { "prop_valve", "prop_pipe", "prop_barrel" }, "mx_ch09_stair"));
            c9.Arenas.Add(Arena("arena_ch09_cistern", 9, "Lower Cistern", 54f, 19f, false, "runoff_green",
                new[] { "hz_runoff_pool", "hz_rising_water", "hz_steam_vent" }, StandardSpawnTags(),
                new[] { "prop_valve", "prop_pipe", "prop_grate" }, "mx_ch09_cistern"));
            c9.Arenas.Add(Arena("arena_ch09_mb1", 9, "Bore Shaft", 38f, 17f, false, "runoff_green",
                new[] { "hz_steam_vent", "hz_collapsing_walkway" }, StandardSpawnTags(),
                new[] { "prop_pipe", "prop_grate" }, "mx_miniboss"));
            c9.Arenas.Add(Arena("arena_ch09_mb2", 9, "Den Floor", 36f, 16f, false, "runoff_green",
                new[] { "hz_runoff_pool", "hz_bone_grinder" }, StandardSpawnTags(),
                new[] { "prop_bone_pile", "prop_grate" }, "mx_miniboss"));
            c9.Arenas.Add(Arena("arena_ch09_boss", 9, "The Vat", 58f, 20f, true, "runoff_green",
                new[] { "hz_runoff_pool", "hz_rising_water", "hz_steam_vent", "hz_geyser" }, StandardSpawnTags(),
                new[] { "prop_valve", "prop_pipe", "prop_grate", "prop_vat_wall" }, "mx_boss_sump"));

            c9.Savepoints.Add(Savepoint("sp09_stair", 9, "ch09_beat_save_a", "Cistern stair - pump room", "arena_ch09_stair", false));
            c9.Savepoints.Add(Savepoint("sp09_preboss", 9, "ch09_beat_save_b", "Vat lip - drainage platform", "arena_ch09_boss", true));

            c9.Checkpoints.Add(Checkpoint("chk09_01", 9, "ch09_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c9.Checkpoints.Add(Checkpoint("chk09_02", 9, "ch09_beat_cistern", "Lower cistern cleared", CheckpointTrigger.EnterArena));
            c9.Checkpoints.Add(Checkpoint("chk09_03", 9, "ch09_beat_save_b", "Before Sump", CheckpointTrigger.PreBoss));
            c9.Checkpoints.Add(Checkpoint("chk09_04", 9, "ch09_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c9.Beats.Add(Beat(1, BeatKind.Comic, "ch09_beat_comic_intro", "Down", "Watch the opening page", "cp09_intro"));
            c9.Beats.Add(Beat(2, BeatKind.Exploration, "ch09_beat_stair", "Cistern stair", "Descend the stair", "arena_ch09_stair"));
            c9.Beats.Add(Beat(3, BeatKind.Arena, "ch09_beat_enc_stair", "Stair crawlers", "Clear the stair", "enc_ch09_stair"));
            c9.Beats.Add(Beat(4, BeatKind.Savepoint, "ch09_beat_save_a", "Pump room", "Save at the pump room", "sp09_stair"));
            c9.Beats.Add(Beat(5, BeatKind.MiniBossFight, "ch09_beat_mb1", "Bore shaft", "Deal with the Drillmaster", "mb_drillmaster"));
            c9.Beats.Add(Beat(6, BeatKind.Comic, "ch09_beat_comic_story", "What it flushes", "Watch the mid-chapter page", "cp09_story"));
            c9.Beats.Add(Beat(7, BeatKind.Arena, "ch09_beat_cistern", "Lower cistern", "Clear the lower cistern", "enc_ch09_cistern"));
            c9.Beats.Add(Beat(8, BeatKind.MiniBossFight, "ch09_beat_mb2", "Den floor", "Deal with the Hound Mother", "mb_hound_mother", optional: true));
            c9.Beats.Add(Beat(9, BeatKind.Savepoint, "ch09_beat_save_b", "Drainage platform", "Save before the vat", "sp09_preboss"));
            c9.Beats.Add(Beat(10, BeatKind.Comic, "ch09_beat_comic_boss", "The vat", "Watch the boss intro page", "cp09_boss"));
            c9.Beats.Add(Beat(11, BeatKind.BossFight, "ch09_beat_boss", "Sump", "Defeat Sump", "boss_sump"));
            c9.Beats.Add(Beat(12, BeatKind.Comic, "ch09_beat_comic_outro", "Overflow", "Watch the closing page", "cp09_outro"));
            c9.Beats.Add(Beat(13, BeatKind.ChapterComplete, "ch09_beat_complete", "Chapter complete", "Autosave and continue", "flag_ch09_complete"));

            c9.ComicPages.Add(Page("cp09_intro", 9, 1, "ch09_beat_comic_intro", "Down",
                new[]
                {
                    Narration("cp09_intro_p1", ComicShotType.Establish, "runoff_green",
                        "Below the Bone Yards, the Veil has been flushing for eleven months. It is very full.",
                        null, "zoom"),
                    Speech("cp09_intro_p2", ComicShotType.Medium, "runoff_green", "Ivo",
                        "I am not going down there. Everything down there came out of something that used to be a person.",
                        new[] { A("ivo", 0.5f, "idle", "queasy", 1.05f) }),
                    Speech("cp09_intro_p3", ComicShotType.OverShoulder, "runoff_green", "Vera",
                        "Everything up here did too. At least the runoff is honest about it.",
                        new[] { A("vera", 0.62f, "idle", "flat", 1f, flip: true), A("ivo", 0.32f, "idle", "queasy", 0.95f) }),
                    Speech("cp09_intro_p4", ComicShotType.CloseUp, "runoff_green", "Kade",
                        "Undercity is below the vat. That is the road. Hand me the lamp.",
                        new[] { A("kade", 0.5f, "idle", "set", 1.2f) }),
                    Narration("cp09_intro_p5", ComicShotType.Wide, "runoff_green",
                        "Something at the bottom of the stair takes a breath it should not be able to take.",
                        null, "speedlines")
                }, "mx_ch09_stair"));

            c9.ComicPages.Add(Page("cp09_story", 9, 2, "ch09_beat_comic_story", "What It Flushes",
                new[]
                {
                    Narration("cp09_story_p1", ComicShotType.Insert, "runoff_green",
                        "The runoff is not waste. It is the part of the archive the Veil could not index.",
                        null, "flash"),
                    Speech("cp09_story_p2", ComicShotType.Medium, "runoff_green", "Vera",
                        "This is memory that failed quality control. Facts it could not file, so it threw them out.",
                        new[] { A("vera", 0.5f, "idle", "fascinated", 1.1f) }),
                    Speech("cp09_story_p3", ComicShotType.CloseUp, "runoff_green", "Kade",
                        "Then somewhere down here is the version of Meridian that the Veil could not file. I want to hear it.",
                        new[] { A("kade", 0.5f, "idle", "grim", 1.25f) }),
                    Speech("cp09_story_p4", ComicShotType.OverShoulder, "runoff_green", "Vera",
                        "You are going to get us killed for a memory. Keep moving and we can both survive it.",
                        new[] { A("vera", 0.66f, "idle", "hard", 1.05f, flip: true), A("kade", 0.33f, "idle", "grim", 1f) }),
                    Narration("cp09_story_p5", ComicShotType.Wide, "runoff_green",
                        "A wave of runoff turns in the dark and starts walking against the current.", null, "shake")
                }));

            c9.ComicPages.Add(Page("cp09_boss", 9, 3, "ch09_beat_comic_boss", "The Vat",
                new[]
                {
                    Narration("cp09_boss_p1", ComicShotType.Establish, "runoff_green",
                        "The vat is eighty metres across and the floor is only dry in the middle.",
                        null, "impact"),
                    Speech("cp09_boss_p2", ComicShotType.Wide, "runoff_green", "Sump",
                        "YOU DROPPED ME HERE.",
                        new[] { A("sump", 0.5f, "rise", "veil", 1.7f) }, effect: "shake"),
                    Speech("cp09_boss_p3", ComicShotType.Medium, "runoff_green", "Kade",
                        "We did not drop you. We just never came to get you.",
                        new[] { A("kade", 0.35f, "aim", "flat", 1.1f) }),
                    Speech("cp09_boss_p4", ComicShotType.Medium, "runoff_green", "Vera",
                        "Tendrils. Cut the tendrils, or Sump pulls itself back together from the floor.",
                        new[] { A("vera", 0.5f, "point", "focused", 1.1f) }),
                    Narration("cp09_boss_p5", ComicShotType.Insert, "runoff_green",
                        "The runoff rises. Every pool in the room is now part of the thing in the middle.",
                        null, "speedlines", "BURBLE")
                }, "mx_boss_sump", "boss"));

            c9.ComicPages.Add(Page("cp09_outro", 9, 4, "ch09_beat_comic_outro", "Overflow",
                new[]
                {
                    Narration("cp09_outro_p1", ComicShotType.Wide, "runoff_green",
                        "Sump collapses into the cistern and the runoff finally drains towards the sea.",
                        null, "impact"),
                    Speech("cp09_outro_p2", ComicShotType.Medium, "runoff_green", "Ivo",
                        "Lines are clear. Whatever is under us, we can walk to it now.",
                        new[] { A("ivo", 0.5f, "idle", "relieved", 1.05f) }),
                    Speech("cp09_outro_p3", ComicShotType.OverShoulder, "runoff_green", "Vera",
                        "Ossuary next. The Veilwardens keep the Directorate's honoured dead, and the Conductor is behind them.",
                        new[] { A("vera", 0.6f, "idle", "focused", 1f, flip: true), A("ivo", 0.32f, "idle", "relieved", 1f) }),
                    Speech("cp09_outro_p4", ComicShotType.CloseUp, "runoff_green", "Kade",
                        "Then we end the honourable part of the Directorate first.",
                        new[] { A("kade", 0.5f, "idle", "cold", 1.2f) }),
                    Narration("cp09_outro_p5", ComicShotType.Establish, "runoff_green",
                        "Above them, through metres of rock, the Veil's signal changes pitch.", null, "zoom")
                }));
            content.Chapters.Add(c9);

            /* ============================================================
               CHAPTER 10 - OSSUARY          (135 min, level 50)
               ============================================================ */
            ChapterDefinition c10 = new ChapterDefinition
            {
                Index = 10,
                Title = "Ossuary",
                Subtitle = "The Directorate's honoured dead",
                ActTitle = ChapterRules.ActTitle(10),
                LocationName = "Veil Ossuary",
                LocationId = "loc_ossuary",
                Synopsis =
                    "The Directorate's first Veilwardens were the people who agreed to be poured into the " +
                    "machine. They still guard the way to the Conductor, and the ones who broke their oath " +
                    "will help you past the ones who did not.",
                BossId = "boss_veilwarden",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(10),
                RecommendedLevel = ChapterRules.RecommendedLevel(10),
                MinimumLevel = ChapterRules.MinimumLevel(10),
                UnlockFlagOnComplete = "flag_ch10_complete"
            };
            c10.MiniBossIds.AddRange(new[] { "mb_tollkeeper", "mb_penitent" });
            c10.SideQuestIds.AddRange(new[] { "q10_honour_guard", "q10_reflect_nothing", "q10_bridge_toll" });
            c10.NewEnemyIds.AddRange(new[] { "e_veilguard", "e_veil_engine" });
            c10.ChapterRewardIds.AddRange(new[] { "perk_kade_demolition", "perk_vera_overdraw" });

            c10.Arenas.Add(Arena("arena_ch10_gallery", 10, "Bone Gallery", 50f, 19f, false, "ossuary_bone",
                new[] { "hz_laser_grid", "hz_plasma_vent" }, StandardSpawnTags(),
                new[] { "prop_sarcophagus", "prop_bone_arch", "prop_censer" }, "mx_ch10_gallery"));
            c10.Arenas.Add(Arena("arena_ch10_bridge", 10, "Veil Bridge", 56f, 17f, false, "ossuary_bone",
                new[] { "hz_open_air", "hz_laser_grid" }, StandardSpawnTags(),
                new[] { "prop_veil_pylon", "prop_bone_arch" }, "mx_ch10_bridge"));
            c10.Arenas.Add(Arena("arena_ch10_mb1", 10, "Toll Crossing", 40f, 16f, false, "ossuary_bone",
                new[] { "hz_laser_grid" }, StandardSpawnTags(),
                new[] { "prop_veil_pylon", "prop_barrier" }, "mx_miniboss"));
            c10.Arenas.Add(Arena("arena_ch10_mb2", 10, "Penitent Cell", 38f, 17f, false, "ossuary_bone",
                new[] { "hz_plasma_vent" }, StandardSpawnTags(),
                new[] { "prop_sarcophagus", "prop_censer" }, "mx_miniboss"));
            c10.Arenas.Add(Arena("arena_ch10_boss", 10, "Ossuary Core", 60f, 21f, true, "ossuary_bone",
                new[] { "hz_laser_grid", "hz_plasma_vent", "hz_veil_pulse" }, StandardSpawnTags(),
                new[] { "prop_sarcophagus", "prop_veil_pylon", "prop_bone_arch" }, "mx_boss_warden"));

            c10.Savepoints.Add(Savepoint("sp10_gallery", 10, "ch10_beat_save_a", "Bone gallery - censer alcove", "arena_ch10_gallery", false));
            c10.Savepoints.Add(Savepoint("sp10_preboss", 10, "ch10_beat_save_b", "Ossuary core - sealed gate", "arena_ch10_boss", true));

            c10.Checkpoints.Add(Checkpoint("chk10_01", 10, "ch10_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c10.Checkpoints.Add(Checkpoint("chk10_02", 10, "ch10_beat_bridge", "Veil bridge crossed", CheckpointTrigger.EnterArena));
            c10.Checkpoints.Add(Checkpoint("chk10_03", 10, "ch10_beat_save_b", "Before the Veilwarden", CheckpointTrigger.PreBoss));
            c10.Checkpoints.Add(Checkpoint("chk10_04", 10, "ch10_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c10.Beats.Add(Beat(1, BeatKind.Comic, "ch10_beat_comic_intro", "Hall of honour", "Watch the opening page", "cp10_intro"));
            c10.Beats.Add(Beat(2, BeatKind.Exploration, "ch10_beat_gallery", "Bone gallery", "Cross the gallery", "arena_ch10_gallery"));
            c10.Beats.Add(Beat(3, BeatKind.Arena, "ch10_beat_enc_gallery", "Gallery guard", "Clear the gallery", "enc_ch10_gallery"));
            c10.Beats.Add(Beat(4, BeatKind.Savepoint, "ch10_beat_save_a", "Censer alcove", "Save at the alcove", "sp10_gallery"));
            c10.Beats.Add(Beat(5, BeatKind.MiniBossFight, "ch10_beat_mb1", "Toll crossing", "Deal with the Tollkeeper", "mb_tollkeeper"));
            c10.Beats.Add(Beat(6, BeatKind.Comic, "ch10_beat_comic_story", "The oath", "Watch the mid-chapter page", "cp10_story"));
            c10.Beats.Add(Beat(7, BeatKind.Arena, "ch10_beat_bridge", "Veil bridge", "Cross the veil bridge", "enc_ch10_bridge"));
            c10.Beats.Add(Beat(8, BeatKind.MiniBossFight, "ch10_beat_mb2", "Penitent cell", "Deal with the Penitent", "mb_penitent", optional: true));
            c10.Beats.Add(Beat(9, BeatKind.Savepoint, "ch10_beat_save_b", "Sealed gate", "Save before the core", "sp10_preboss"));
            c10.Beats.Add(Beat(10, BeatKind.Comic, "ch10_beat_comic_boss", "First of the honoured", "Watch the boss intro page", "cp10_boss"));
            c10.Beats.Add(Beat(11, BeatKind.BossFight, "ch10_beat_boss", "Veilwarden Ossuary", "Defeat the Veilwarden", "boss_veilwarden"));
            c10.Beats.Add(Beat(12, BeatKind.Comic, "ch10_beat_comic_outro", "Helmet off", "Watch the closing page", "cp10_outro"));
            c10.Beats.Add(Beat(13, BeatKind.ChapterComplete, "ch10_beat_complete", "Chapter complete", "Autosave and continue", "flag_ch10_complete"));

            c10.ComicPages.Add(Page("cp10_intro", 10, 1, "ch10_beat_comic_intro", "Hall of Honour",
                new[]
                {
                    Narration("cp10_comic_p1", ComicShotType.Establish, "ossuary_bone",
                        "The Directorate's honoured dead are not buried here. They are posted here.",
                        null, "zoom"),
                    Speech("cp10_comic_p2", ComicShotType.Medium, "ossuary_bone", "Archivist",
                        "Eight Veilwardens between you and the Conductor. Each one volunteered. Each one regrets it.",
                        new[] { A("archivist", 0.5f, "idle", "still", 1.05f) }),
                    Speech("cp10_comic_p3", ComicShotType.OverShoulder, "ossuary_bone", "Vera",
                        "Volunteered for what, exactly?",
                        new[] { A("vera", 0.35f, "idle", "focused", 1f), A("archivist", 0.68f, "idle", "still", 1f, flip: true) }),
                    Speech("cp10_comic_p4", ComicShotType.CloseUp, "ossuary_bone", "Archivist",
                        "For the honour of being the Veil's armour. It is the only promotion the Directorate still offers.",
                        new[] { A("archivist", 0.5f, "idle", "hollow", 1.2f) }),
                    Narration("cp10_comic_p5", ComicShotType.Wide, "ossuary_bone",
                        "In the gallery ahead, eight sets of armour stand up at once and salute.",
                        null, "impact", "CLANK")
                }, "mx_ch10_gallery"));

            c10.ComicPages.Add(Page("cp10_story", 10, 2, "ch10_beat_comic_story", "The Oath",
                new[]
                {
                    Narration("cp10_story_p1", ComicShotType.Insert, "ossuary_bone",
                        "The oath is carved into every sarcophagus, in the same hand, in the same words.",
                        null, "flash"),
                    Speech("cp10_story_p2", ComicShotType.Medium, "ossuary_bone", "Vera",
                        "It is a promise to protect the coast. They were told the Veil was the coast.",
                        new[] { A("vera", 0.5f, "idle", "sad", 1.1f) }),
                    Speech("cp10_story_p3", ComicShotType.CloseUp, "ossuary_bone", "Kade",
                        "They were told. We were told. The difference is whether we keep walking after we know better.",
                        new[] { A("kade", 0.5f, "idle", "grim", 1.25f) }),
                    Speech("cp10_story_p4", ComicShotType.OverShoulder, "ossuary_bone", "Archivist",
                        "One of them broke the oath. It sits in a cell below and it will help you, if you can stand to look at it.",
                        new[] { A("archivist", 0.62f, "idle", "still", 1.05f, flip: true), A("kade", 0.33f, "idle", "grim", 1f) }),
                    Narration("cp10_story_p5", ComicShotType.Wide, "ossuary_bone",
                        "Down the corridor, something heavy and armoured is crying, very quietly.", null, "shake")
                }));

            c10.ComicPages.Add(Page("cp10_boss", 10, 3, "ch10_beat_comic_boss", "First of the Honoured",
                new[]
                {
                    Narration("cp10_boss_p1", ComicShotType.Establish, "ossuary_bone",
                        "Ossuary core. The first Veilwarden has stood here since the Veil went up.",
                        null, "impact"),
                    Speech("cp10_boss_p2", ComicShotType.Wide, "ossuary_bone", "Veilwarden",
                        "THE HONOUR IS ETERNAL. THE HONOUR IS LOUD.",
                        new[] { A("veilwarden", 0.5f, "guard", "machine", 1.6f) }, effect: "shake"),
                    Speech("cp10_boss_p3", ComicShotType.Medium, "ossuary_bone", "Kade",
                        "Your honour is a filing cabinet with a rifle.",
                        new[] { A("kade", 0.35f, "aim", "cold", 1.1f) }),
                    Speech("cp10_boss_p4", ComicShotType.Medium, "ossuary_bone", "Vera",
                        "Front plate reflects everything. Flank it, or hit the seams.",
                        new[] { A("vera", 0.5f, "point", "focused", 1.1f) }),
                    Narration("cp10_boss_p5", ComicShotType.Insert, "ossuary_bone",
                        "It salutes with the rifle still loaded. The salute takes one second. That is the only warning.",
                        null, "speedlines")
                }, "mx_boss_warden", "boss"));

            c10.ComicPages.Add(Page("cp10_outro", 10, 4, "ch10_beat_comic_outro", "Helmet Off",
                new[]
                {
                    Narration("cp10_outro_p1", ComicShotType.Wide, "ossuary_bone",
                        "The Veilwarden kneels, and the Veil light drains out of its plating one panel at a time.",
                        null, "impact"),
                    Speech("cp10_outro_p2", ComicShotType.Medium, "ossuary_bone", "Veilwarden",
                        "The Conductor is beneath the undercity. He has been waiting for a soldier to come and argue with him.",
                        new[] { A("veilwarden", 0.5f, "kneel", "spent", 1.2f) }),
                    Speech("cp10_outro_p3", ComicShotType.OverShoulder, "ossuary_bone", "Kade",
                        "I am not a soldier. I am the man who opened Meridian.",
                        new[] { A("kade", 0.33f, "idle", "hard", 1f), A("veilwarden", 0.68f, "kneel", "spent", 1f) }),
                    Speech("cp10_outro_p4", ComicShotType.CloseUp, "ossuary_bone", "Veilwarden",
                        "Then he will be glad to see you. He has been rehearsing.",
                        new[] { A("veilwarden", 0.5f, "kneel", "sad", 1.25f) }),
                    Narration("cp10_outro_p5", ComicShotType.Establish, "ossuary_bone",
                        "Beyond the core, a shaft drops into the undercity. Warm air comes up, smelling of copper and paper.",
                        null, "zoom")
                }));
            content.Chapters.Add(c10);

            /* ============================================================
               CHAPTER 11 - UNDERCITY        (140 min, level 55)
               ============================================================ */
            ChapterDefinition c11 = new ChapterDefinition
            {
                Index = 11,
                Title = "Undercity",
                Subtitle = "The office that never closed",
                ActTitle = ChapterRules.ActTitle(11),
                LocationName = "Veil Undercity",
                LocationId = "loc_undercity",
                Synopsis =
                    "Under the coast, the Directorate still runs. The Conductor signs orders to units that no " +
                    "longer exist and keeps the Veil's service shaft closed. The Gutterking holds the only other " +
                    "way in, and he does not give it away.",
                BossId = "boss_conductor",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(11),
                RecommendedLevel = ChapterRules.RecommendedLevel(11),
                MinimumLevel = ChapterRules.MinimumLevel(11),
                UnlockFlagOnComplete = "flag_ch11_complete"
            };
            c11.MiniBossIds.AddRange(new[] { "mb_gutterking", "mb_signal_adjunct" });
            c11.SideQuestIds.AddRange(new[] { "q11_crown_of_gutters", "q11_memo_war", "q11_requisition" });
            c11.NewEnemyIds.AddRange(new[] { "e_signal_conductor" });
            c11.ChapterRewardIds.AddRange(new[] { "g_plating_mk4", "g_weave_mk4" });

            c11.Arenas.Add(Arena("arena_ch11_plaza", 11, "Drowned Plaza", 52f, 19f, false, "undercity_black",
                new[] { "hz_runoff_pool", "hz_plasma_vent" }, StandardSpawnTags(),
                new[] { "prop_pillar", "prop_relay_stack", "prop_barrier" }, "mx_ch11_plaza"));
            c11.Arenas.Add(Arena("arena_ch11_exchange", 11, "Relay Exchange", 54f, 20f, false, "undercity_black",
                new[] { "hz_laser_grid", "hz_plasma_vent", "hz_resonance_field" }, StandardSpawnTags(),
                new[] { "prop_relay_stack", "prop_pillar", "prop_hardcase" }, "mx_ch11_exchange"));
            c11.Arenas.Add(Arena("arena_ch11_mb1", 11, "Throne of Gutters", 42f, 18f, false, "undercity_black",
                new[] { "hz_runoff_pool" }, StandardSpawnTags(),
                new[] { "prop_relay_stack", "prop_barrier" }, "mx_miniboss"));
            c11.Arenas.Add(Arena("arena_ch11_mb2", 11, "Adjunct Office", 40f, 17f, false, "undercity_black",
                new[] { "hz_laser_grid" }, StandardSpawnTags(),
                new[] { "prop_hardcase", "prop_pillar" }, "mx_miniboss"));
            c11.Arenas.Add(Arena("arena_ch11_boss", 11, "Directorate Chamber", 62f, 21f, true, "undercity_black",
                new[] { "hz_laser_grid", "hz_plasma_vent", "hz_resonance_field", "hz_veil_pulse" }, StandardSpawnTags(),
                new[] { "prop_relay_stack", "prop_pillar", "prop_directorate_seal" }, "mx_boss_conductor"));

            c11.Savepoints.Add(Savepoint("sp11_plaza", 11, "ch11_beat_save_a", "Drowned plaza - Tace's mark", "arena_ch11_plaza", false));
            c11.Savepoints.Add(Savepoint("sp11_preboss", 11, "ch11_beat_save_b", "Chamber door - last checkpoint", "arena_ch11_boss", true));

            c11.Checkpoints.Add(Checkpoint("chk11_01", 11, "ch11_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c11.Checkpoints.Add(Checkpoint("chk11_02", 11, "ch11_beat_exchange", "Exchange cleared", CheckpointTrigger.EnterArena));
            c11.Checkpoints.Add(Checkpoint("chk11_03", 11, "ch11_beat_save_b", "Before the Conductor", CheckpointTrigger.PreBoss));
            c11.Checkpoints.Add(Checkpoint("chk11_04", 11, "ch11_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c11.Beats.Add(Beat(1, BeatKind.Comic, "ch11_beat_comic_intro", "Still open", "Watch the opening page", "cp11_intro"));
            c11.Beats.Add(Beat(2, BeatKind.Exploration, "ch11_beat_plaza", "Drowned plaza", "Cross the plaza", "arena_ch11_plaza"));
            c11.Beats.Add(Beat(3, BeatKind.Arena, "ch11_beat_enc_plaza", "Plaza garrison", "Clear the plaza", "enc_ch11_plaza"));
            c11.Beats.Add(Beat(4, BeatKind.Savepoint, "ch11_beat_save_a", "Tace's mark", "Save at the plaza", "sp11_plaza"));
            c11.Beats.Add(Beat(5, BeatKind.MiniBossFight, "ch11_beat_mb1", "Throne of gutters", "Deal with the Gutterking", "mb_gutterking"));
            c11.Beats.Add(Beat(6, BeatKind.Comic, "ch11_beat_comic_story", "The office", "Watch the mid-chapter page", "cp11_story"));
            c11.Beats.Add(Beat(7, BeatKind.Arena, "ch11_beat_exchange", "Relay exchange", "Clear the exchange", "enc_ch11_exchange"));
            c11.Beats.Add(Beat(8, BeatKind.MiniBossFight, "ch11_beat_mb2", "Adjunct office", "Deal with the Signal Adjunct", "mb_signal_adjunct", optional: true));
            c11.Beats.Add(Beat(9, BeatKind.Savepoint, "ch11_beat_save_b", "Chamber door", "Save before the chamber", "sp11_preboss"));
            c11.Beats.Add(Beat(10, BeatKind.Comic, "ch11_beat_comic_boss", "The Conductor", "Watch the boss intro page", "cp11_boss"));
            c11.Beats.Add(Beat(11, BeatKind.BossFight, "ch11_beat_boss", "The Conductor", "Defeat the Conductor", "boss_conductor"));
            c11.Beats.Add(Beat(12, BeatKind.Comic, "ch11_beat_comic_outro", "Office closed", "Watch the closing page", "cp11_outro"));
            c11.Beats.Add(Beat(13, BeatKind.ChapterComplete, "ch11_beat_complete", "Chapter complete", "Autosave and continue", "flag_ch11_complete"));

            c11.ComicPages.Add(Page("cp11_intro", 11, 1, "ch11_beat_comic_intro", "Still Open",
                new[]
                {
                    Narration("cp11_intro_p1", ComicShotType.Establish, "undercity_black",
                        "The undercity runs on Veil current and paperwork. Both are still flowing.",
                        null, "zoom"),
                    Speech("cp11_intro_p2", ComicShotType.Medium, "undercity_black", "Tace",
                        "I have been mapping this for a year. Every door needs a Directorate seal, and every seal needs a person.",
                        new[] { A("tace", 0.5f, "idle", "wry", 1.05f) }),
                    Speech("cp11_intro_p3", ComicShotType.OverShoulder, "undercity_black", "Vera",
                        "Then we bring a seal. The Gutterking has one, and he is not using it for anything but a crown.",
                        new[] { A("vera", 0.62f, "idle", "cold", 1f, flip: true), A("tace", 0.32f, "idle", "wry", 0.95f) }),
                    Speech("cp11_intro_p4", ComicShotType.CloseUp, "undercity_black", "Kade",
                        "Then we take the crown off him and walk through the front door.",
                        new[] { A("kade", 0.5f, "idle", "set", 1.2f) }),
                    Narration("cp11_intro_p5", ComicShotType.Wide, "undercity_black",
                        "Somewhere below, an office light switches on. It has been on for eleven months.",
                        null, "speedlines")
                }, "mx_ch11_plaza"));

            c11.ComicPages.Add(Page("cp11_story", 11, 2, "ch11_beat_comic_story", "The Office",
                new[]
                {
                    Narration("cp11_story_p1", ComicShotType.Insert, "undercity_black",
                        "The orders are still being written. Same paper, same ink, same stamped seal.",
                        null, "flash"),
                    Speech("cp11_story_p2", ComicShotType.Medium, "undercity_black", "Vera",
                        "These are dated this morning. Someone down here is still commanding an army that does not exist.",
                        new[] { A("vera", 0.5f, "idle", "disgusted", 1.1f) }),
                    Speech("cp11_story_p3", ComicShotType.CloseUp, "undercity_black", "Kade",
                        "Not someone. The office. The Conductor is a chair with a person welded into it.",
                        new[] { A("kade", 0.5f, "idle", "cold", 1.25f) }),
                    Speech("cp11_story_p4", ComicShotType.OverShoulder, "undercity_black", "Vera",
                        "Then we will have to argue with a chair. Take the signature with us when we leave.",
                        new[] { A("vera", 0.66f, "idle", "hard", 1.05f, flip: true), A("kade", 0.33f, "idle", "cold", 1f) }),
                    Narration("cp11_story_p5", ComicShotType.Wide, "undercity_black",
                        "Every speaker in the undercity clicks on at once and reads a name. It reads Kade's.",
                        null, "shake")
                }));

            c11.ComicPages.Add(Page("cp11_boss", 11, 3, "ch11_beat_comic_boss", "The Conductor",
                new[]
                {
                    Narration("cp11_boss_p1", ComicShotType.Establish, "undercity_black",
                        "The chamber is an office the size of a church, and the desk has a person fused into it.",
                        null, "impact"),
                    Speech("cp11_boss_p2", ComicShotType.Wide, "undercity_black", "Conductor",
                        "Kade Rourke. Meridian. Nine hundred and forty metres. You are in my file, son.",
                        new[] { A("conductor", 0.5f, "sit", "veil", 1.5f) }),
                    Speech("cp11_boss_p3", ComicShotType.CloseUp, "undercity_black", "Kade",
                        "You signed the order. I pulled the trigger. One of us gets to walk out.",
                        new[] { A("kade", 0.5f, "aim", "furious", 1.25f) }),
                    Speech("cp11_boss_p4", ComicShotType.Medium, "undercity_black", "Conductor",
                        "Neither of us is leaving. The office does not close.",
                        new[] { A("conductor", 0.5f, "sit", "veil", 1.5f) }, "shake"),
                    Narration("cp11_boss_p5", ComicShotType.Wide, "undercity_black",
                        "Hardlight orders fill the room and start enforcing themselves.",
                        null, "speedlines", "STAMP")
                }, "mx_boss_conductor", "boss"));

            c11.ComicPages.Add(Page("cp11_outro", 11, 4, "ch11_beat_comic_outro", "Office Closed",
                new[]
                {
                    Narration("cp11_outro_p1", ComicShotType.Wide, "undercity_black",
                        "The Conductor's desk goes dark. For the first time since the Veil went up, no orders are issued.",
                        null, "impact"),
                    Speech("cp11_outro_p2", ComicShotType.Medium, "undercity_black", "Vera",
                        "He signed Meridian and then he kept signing. Same pen. I took both.",
                        new[] { A("vera", 0.5f, "idle", "tired", 1.05f) }),
                    Speech("cp11_outro_p3", ComicShotType.OverShoulder, "undercity_black", "Kade",
                        "One floor left. The Veil itself, with the curator still inside it.",
                        new[] { A("kade", 0.33f, "idle", "grim", 1f), A("vera", 0.68f, "idle", "tired", 1f, flip: true) }),
                    Speech("cp11_outro_p4", ComicShotType.CloseUp, "undercity_black", "Tace",
                        "Shaft is open. Whatever is up there has been waiting for a visitor since the day it started.",
                        new[] { A("tace", 0.5f, "idle", "serious", 1.15f) }),
                    Narration("cp11_outro_p5", ComicShotType.Establish, "undercity_black",
                        "The service shaft rises into white light. The Veil is listening for footsteps.",
                        null, "zoom")
                }));
            content.Chapters.Add(c11);

            /* ============================================================
               CHAPTER 12 - THE IRON VEIL    (145 min, level 58)
               ============================================================ */
            ChapterDefinition c12 = new ChapterDefinition
            {
                Index = 12,
                Title = "The Iron Veil",
                Subtitle = "Break the last machine on the coast",
                ActTitle = ChapterRules.ActTitle(12),
                LocationName = "The Iron Veil",
                LocationId = "loc_iron_veil",
                Synopsis =
                    "The Veil is an archiving engine the size of a cathedral, and Curator Sel has been keeping " +
                    "it awake since the coast went dark. She built it to save what she could not save, and she " +
                    "will not let it go quiet while Meridian is still in it.",
                BossId = "boss_veil_engine",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(12),
                RecommendedLevel = ChapterRules.RecommendedLevel(12),
                MinimumLevel = ChapterRules.MinimumLevel(12),
                UnlockFlagOnComplete = "flag_ch12_complete",
                EndingFlag = "flag_ending_canon"
            };
            c12.MiniBossIds.AddRange(new[] { "mb_veil_hand", "mb_last_archivist" });
            c12.SideQuestIds.AddRange(new[] { "q12_last_names", "q12_veil_hand", "q12_archive_erased" });

            c12.Arenas.Add(Arena("arena_ch12_spine", 12, "Archive Spine", 54f, 20f, false, "veil_core",
                new[] { "hz_archive_beam", "hz_veil_pulse" }, StandardSpawnTags(),
                new[] { "prop_archive_spine", "prop_memory_glass", "prop_node" }, "mx_ch12_spine"));
            c12.Arenas.Add(Arena("arena_ch12_meridian", 12, "Meridian Annex", 50f, 19f, false, "veil_core",
                new[] { "hz_archive_beam", "hz_memory_shards" }, StandardSpawnTags(),
                new[] { "prop_memory_glass", "prop_node", "prop_memorial" }, "mx_ch12_meridian"));
            c12.Arenas.Add(Arena("arena_ch12_mb1", 12, "Curator's Reach", 42f, 19f, false, "veil_core",
                new[] { "hz_archive_beam" }, StandardSpawnTags(),
                new[] { "prop_archive_spine", "prop_node" }, "mx_miniboss"));
            c12.Arenas.Add(Arena("arena_ch12_mb2", 12, "Index Hall", 44f, 19f, false, "veil_core",
                new[] { "hz_veil_pulse", "hz_laser_grid" }, StandardSpawnTags(),
                new[] { "prop_node", "prop_memory_glass" }, "mx_miniboss"));
            c12.Arenas.Add(Arena("arena_ch12_boss", 12, "The Veil Core", 64f, 22f, true, "veil_core",
                new[] { "hz_archive_beam", "hz_veil_pulse", "hz_laser_grid", "hz_harvest_square" }, StandardSpawnTags(),
                new[] { "prop_archive_spine", "prop_node", "prop_memory_glass", "prop_curator_dais" }, "mx_boss_veil"));

            c12.Savepoints.Add(Savepoint("sp12_spine", 12, "ch12_beat_save_a", "Archive spine - service lift", "arena_ch12_spine", false));
            c12.Savepoints.Add(Savepoint("sp12_preboss", 12, "ch12_beat_save_b", "Core dais approach - final savepoint", "arena_ch12_boss", true, travel: true));

            c12.Checkpoints.Add(Checkpoint("chk12_01", 12, "ch12_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c12.Checkpoints.Add(Checkpoint("chk12_02", 12, "ch12_beat_meridian", "Meridian annex read", CheckpointTrigger.AfterCutscene));
            c12.Checkpoints.Add(Checkpoint("chk12_03", 12, "ch12_beat_save_b", "Before the Veil", CheckpointTrigger.PreBoss));
            c12.Checkpoints.Add(Checkpoint("chk12_04", 12, "ch12_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c12.Beats.Add(Beat(1, BeatKind.Comic, "ch12_beat_comic_intro", "Service shaft", "Watch the opening page", "cp12_intro"));
            c12.Beats.Add(Beat(2, BeatKind.Exploration, "ch12_beat_spine", "Archive spine", "Climb the archive spine", "arena_ch12_spine"));
            c12.Beats.Add(Beat(3, BeatKind.Arena, "ch12_beat_enc_spine", "Spine guard", "Clear the spine", "enc_ch12_spine"));
            c12.Beats.Add(Beat(4, BeatKind.Savepoint, "ch12_beat_save_a", "Service lift", "Save at the service lift", "sp12_spine"));
            c12.Beats.Add(Beat(5, BeatKind.MiniBossFight, "ch12_beat_mb1", "Curator's reach", "Sever the Veil Hand", "mb_veil_hand"));
            c12.Beats.Add(Beat(6, BeatKind.Comic, "ch12_beat_comic_story", "Meridian annex", "Watch the mid-chapter page", "cp12_story"));
            c12.Beats.Add(Beat(7, BeatKind.Arena, "ch12_beat_meridian", "Meridian annex", "Clear the annex", "enc_ch12_meridian"));
            c12.Beats.Add(Beat(8, BeatKind.MiniBossFight, "ch12_beat_mb2", "Index hall", "Deal with the Last Archivist", "mb_last_archivist", optional: true));
            c12.Beats.Add(Beat(9, BeatKind.Savepoint, "ch12_beat_save_b", "Core dais approach", "Final save before the Veil", "sp12_preboss"));
            c12.Beats.Add(Beat(10, BeatKind.Comic, "ch12_beat_comic_boss", "The Veil recognises you", "Watch the boss intro page", "cp12_boss"));
            c12.Beats.Add(Beat(11, BeatKind.BossFight, "ch12_beat_boss", "The Iron Veil", "Defeat the Iron Veil and Curator Sel", "boss_veil_engine"));
            c12.Beats.Add(Beat(12, BeatKind.Comic, "ch12_beat_comic_outro", "Last signal", "Watch the ending", "cp12_outro"));
            c12.Beats.Add(Beat(13, BeatKind.Choice, "ch12_beat_choice", "What the record says", "Decide what happens to the archive", "choice_archive"));
            c12.Beats.Add(Beat(14, BeatKind.ChapterComplete, "ch12_beat_complete", "Campaign complete", "Roll credits and unlock gallery", "flag_ending_canon"));

            c12.ComicPages.Add(Page("cp12_intro", 12, 1, "ch12_beat_comic_intro", "Service Shaft",
                new[]
                {
                    Narration("cp12_comic_p1", ComicShotType.Establish, "veil_core",
                        "The Veil is a cathedral built out of every record the coast ever kept.",
                        null, "zoom"),
                    Speech("cp12_comic_p2", ComicShotType.Medium, "veil_core", "Vera",
                        "It is beautiful. I hate that it is beautiful.",
                        new[] { A("vera", 0.5f, "idle", "conflicted", 1.05f) }),
                    Speech("cp12_comic_p3", ComicShotType.OverShoulder, "veil_core", "Kade",
                        "It is a filing cabinet with stained glass. Do not let it talk to you.",
                        new[] { A("kade", 0.33f, "idle", "flat", 1f), A("vera", 0.68f, "idle", "conflicted", 1f, flip: true) }),
                    Speech("cp12_comic_p4", ComicShotType.CloseUp, "veil_core", "Vera",
                        "It already is. It has been reading my maps back to me since the lift started moving.",
                        new[] { A("vera", 0.5f, "idle", "afraid", 1.2f) }),
                    Narration("cp12_comic_p5", ComicShotType.Wide, "veil_core",
                        "At the top of the spine, a woman made of light and archive glass is waiting with her hands folded.",
                        new[] { A("curatorsel", 0.5f, "idle", "calm", 1.4f, silhouette: true) }, "impact")
                }, "mx_ch12_spine"));

            c12.ComicPages.Add(Page("cp12_story", 12, 2, "ch12_beat_comic_story", "Meridian Annex",
                new[]
                {
                    Narration("cp12_story_p1", ComicShotType.Insert, "veil_core",
                        "The annex holds the Meridian shelter in full: every voice, every minute, on loop.",
                        null, "flash"),
                    Speech("cp12_story_p2", ComicShotType.Medium, "veil_core", "Kade",
                        "They kept them. All of them. Every person I let the Directorate seal in.",
                        new[] { A("kade", 0.5f, "kneel", "grieving", 1.15f) }),
                    Speech("cp12_story_p3", ComicShotType.CloseUp, "veil_core", "Vera",
                        "Not kept. Indexed. This is a library, Kade, and we are standing in the shelf.",
                        new[] { A("vera", 0.5f, "idle", "hard", 1.2f) }),
                    Speech("cp12_story_p4", ComicShotType.Wide, "veil_core", "Curator Sel",
                        "You are wrong. It is not a shelf. It is the only place they still exist. Break it and you kill them twice.",
                        new[] { A("curatorsel", 0.5f, "idle", "calm", 1.4f, silhouette: true) }),
                    Narration("cp12_story_p5", ComicShotType.Wide, "veil_core",
                        "Kade stands in the annex with his hand flat on the glass, and does not answer her.",
                        new[] { A("kade", 0.4f, "idle", "broken", 1.1f) }, "shake")
                }));

            c12.ComicPages.Add(Page("cp12_boss", 12, 3, "ch12_beat_comic_boss", "The Veil Recognises You",
                new[]
                {
                    Narration("cp12_boss_p1", ComicShotType.Establish, "veil_core",
                        "The core is every archiving arm the Veil has, and it has been growing them for eleven months.",
                        null, "impact"),
                    Speech("cp12_boss_p2", ComicShotType.Wide, "veil_core", "Curator Sel",
                        "I built this to keep them. You built the door that killed them. Which of us should stop?",
                        new[] { A("curatorsel", 0.5f, "idle", "calm", 1.5f) }),
                    Speech("cp12_boss_p3", ComicShotType.CloseUp, "veil_core", "Kade",
                        "Neither. We finish it, and then we build something that does not need their screams to run.",
                        new[] { A("kade", 0.5f, "aim", "resolute", 1.3f) }),
                    Speech("cp12_boss_p4", ComicShotType.Medium, "veil_core", "Vera",
                        "Two stages. The machine first, then the person inside it. Do not get them mixed up.",
                        new[] { A("vera", 0.5f, "point", "focused", 1.15f) }),
                    Narration("cp12_boss_p5", ComicShotType.Wide, "veil_core",
                        "The Veil unfolds every arm it has, and the room fills with the sound of a library screaming.",
                        null, "speedlines", "ARCHIVE")
                }, "mx_boss_veil", "boss"));

            c12.ComicPages.Add(Page("cp12_outro", 12, 4, "ch12_beat_comic_outro", "Last Signal",
                new[]
                {
                    Narration("cp12_outro_p1", ComicShotType.Wide, "veil_core",
                        "The Veil goes dark from the outside in, district by district, and the sky over the coast is only sky again.",
                        null, "zoom"),
                    Speech("cp12_outro_p2", ComicShotType.Medium, "veil_core", "Curator Sel",
                        "You will forget them. All of them. There will be nobody left who remembers Meridian.",
                        new[] { A("curatorsel", 0.5f, "sit", "broken", 1.2f) }),
                    Speech("cp12_outro_p3", ComicShotType.CloseUp, "veil_core", "Kade",
                        "No. We will carry it. Badly, and out loud, and every day.",
                        new[] { A("kade", 0.5f, "sit", "spent", 1.25f) }),
                    Speech("cp12_outro_p4", ComicShotType.OverShoulder, "veil_core", "Vera",
                        "I drew the maps that brought the Directorate here. I will be the one who writes down what happened.",
                        new[] { A("vera", 0.62f, "idle", "resolute", 1.1f, flip: true), A("kade", 0.32f, "sit", "spent", 1f) }),
                    Narration("cp12_outro_p5", ComicShotType.Establish, "veil_core",
                        "Above them, for the first time in eleven months, the coast is lit by something that is not the Veil.",
                        new[] { A("kade", 0.25f, "walk", "spent", 0.85f), A("vera", 0.72f, "walk", "soft", 0.85f, flip: true) }, "zoom"),
                    Narration("cp12_outro_p6", ComicShotType.Insert, "veil_core",
                        "Two survivors walk out of a broken archive carrying a box of names. The road ahead is only road.",
                        new[] { A("kade", 0.35f, "walk", "spent", 0.8f), A("vera", 0.62f, "walk", "soft", 0.8f, flip: true) })
                }));
            content.Chapters.Add(c12);
        }
    }
}
