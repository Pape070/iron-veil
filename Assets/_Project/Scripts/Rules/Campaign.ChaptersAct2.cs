namespace IronVeil.Rules
{
    public static partial class Campaign
    {
        private static void BuildChaptersActTwo(CampaignContent content)
        {
            /* ============================================================
               CHAPTER 5 - THE SIREN'S STAGE   (120 min, level 24)
               ============================================================ */
            ChapterDefinition c5 = new ChapterDefinition
            {
                Index = 5,
                Title = "The Siren's Stage",
                Subtitle = "Where the Veil learned to sing",
                ActTitle = ChapterRules.ActTitle(5),
                LocationName = "Coast Opera House",
                LocationId = "loc_opera",
                Synopsis =
                    "The Veil does not just archive memory, it performs it. In the old opera house a cantor " +
                    "called the Siren replays the coast's last night on loop, and everyone who listens for " +
                    "too long starts singing along.",
                BossId = "boss_siren",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(5),
                RecommendedLevel = ChapterRules.RecommendedLevel(5),
                MinimumLevel = ChapterRules.MinimumLevel(5),
                UnlockFlagOnComplete = "flag_ch05_complete"
            };
            c5.MiniBossIds.AddRange(new[] { "mb_siren", "mb_cinder" });
            c5.SideQuestIds.AddRange(new[] { "q05_silence_choir", "q05_stage_fright", "q05_encore" });
            c5.NewEnemyIds.AddRange(new[] { "e_mortar_team", "e_field_chaplain" });
            c5.ChapterRewardIds.AddRange(new[] { "perk_kade_anchor", "perk_vera_slipstream" });

            c5.Arenas.Add(Arena("arena_ch05_foyer", 5, "Opera Foyer", 46f, 17f, false, "opera_red",
                new[] { "hz_collapsing_walkway", "hz_chandelier" }, StandardSpawnTags(),
                new[] { "prop_chandelier", "prop_curtain", "prop_barrier" }, "mx_ch05_foyer"));
            c5.Arenas.Add(Arena("arena_ch05_stalls", 5, "Stalls and Pit", 52f, 19f, false, "opera_red",
                new[] { "hz_chandelier", "hz_rising_water" }, StandardSpawnTags(),
                new[] { "prop_curtain", "prop_orchestra_chair", "prop_chandelier" }, "mx_ch05_stalls"));
            c5.Arenas.Add(Arena("arena_ch05_mb1", 5, "Upper Balcony", 36f, 15f, false, "opera_red",
                new[] { "hz_collapsing_walkway" }, StandardSpawnTags(),
                new[] { "prop_curtain", "prop_barrier" }, "mx_miniboss"));
            c5.Arenas.Add(Arena("arena_ch05_mb2", 5, "Fly Tower", 34f, 16f, false, "opera_red",
                new[] { "hz_chandelier", "hz_steam_vent" }, StandardSpawnTags(),
                new[] { "prop_curtain", "prop_rigging" }, "mx_miniboss"));
            c5.Arenas.Add(Arena("arena_ch05_boss", 5, "Main Stage", 56f, 19f, true, "opera_red",
                new[] { "hz_chandelier", "hz_collapsing_walkway", "hz_resonance_field" }, StandardSpawnTags(),
                new[] { "prop_curtain", "prop_chandelier", "prop_orchestra_chair" }, "mx_boss_siren"));

            c5.Savepoints.Add(Savepoint("sp05_foyer", 5, "ch05_beat_save_a", "Foyer - box office", "arena_ch05_foyer", false));
            c5.Savepoints.Add(Savepoint("sp05_preboss", 5, "ch05_beat_save_b", "Wings - stage door", "arena_ch05_boss", true));

            c5.Checkpoints.Add(Checkpoint("chk05_01", 5, "ch05_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c5.Checkpoints.Add(Checkpoint("chk05_02", 5, "ch05_beat_stalls", "Stalls cleared", CheckpointTrigger.EnterArena));
            c5.Checkpoints.Add(Checkpoint("chk05_03", 5, "ch05_beat_save_b", "Before the Siren", CheckpointTrigger.PreBoss));
            c5.Checkpoints.Add(Checkpoint("chk05_04", 5, "ch05_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c5.Beats.Add(Beat(1, BeatKind.Comic, "ch05_beat_comic_intro", "Doors open", "Watch the opening page", "cp05_intro"));
            c5.Beats.Add(Beat(2, BeatKind.Exploration, "ch05_beat_foyer", "Foyer", "Cross the foyer", "arena_ch05_foyer"));
            c5.Beats.Add(Beat(3, BeatKind.Arena, "ch05_beat_enc_foyer", "Foyer picket", "Clear the foyer", "enc_ch05_foyer"));
            c5.Beats.Add(Beat(4, BeatKind.Savepoint, "ch05_beat_save_a", "Box office", "Save at the box office", "sp05_foyer"));
            c5.Beats.Add(Beat(5, BeatKind.MiniBossFight, "ch05_beat_mb1", "Upper balcony", "Deal with the Lesser Siren", "mb_siren"));
            c5.Beats.Add(Beat(6, BeatKind.Comic, "ch05_beat_comic_story", "The last night", "Watch the mid-chapter page", "cp05_story"));
            c5.Beats.Add(Beat(7, BeatKind.Arena, "ch05_beat_stalls", "Stalls and pit", "Clear the stalls", "enc_ch05_stalls"));
            c5.Beats.Add(Beat(8, BeatKind.MiniBossFight, "ch05_beat_mb2", "Fly tower", "Deal with Cinder", "mb_cinder", optional: true));
            c5.Beats.Add(Beat(9, BeatKind.Savepoint, "ch05_beat_save_b", "Stage door", "Save before the stage", "sp05_preboss"));
            c5.Beats.Add(Beat(10, BeatKind.Comic, "ch05_beat_comic_boss", "Curtain up", "Watch the boss intro page", "cp05_boss"));
            c5.Beats.Add(Beat(11, BeatKind.BossFight, "ch05_beat_boss", "The Siren", "Defeat the Siren", "boss_siren"));
            c5.Beats.Add(Beat(12, BeatKind.Comic, "ch05_beat_comic_outro", "Curtain down", "Watch the closing page", "cp05_outro"));
            c5.Beats.Add(Beat(13, BeatKind.ChapterComplete, "ch05_beat_complete", "Chapter complete", "Autosave and continue", "flag_ch05_complete"));

            c5.ComicPages.Add(Page("cp05_intro", 5, 1, "ch05_beat_comic_intro", "Doors Open",
                new[]
                {
                    Narration("cp05_intro_p1", ComicShotType.Establish, "opera_red",
                        "The opera house has no power and no audience, but the doors open on time every night.",
                        null, "zoom"),
                    Speech("cp05_intro_p2", ComicShotType.Medium, "opera_red", "Ord",
                        "Do not hum. Whatever you do, do not hum. It gets in through the humming.",
                        new[] { A("ord", 0.5f, "idle", "afraid", 1.1f) }),
                    Speech("cp05_intro_p3", ComicShotType.OverShoulder, "opera_red", "Vera",
                        "What is it playing?",
                        new[] { A("vera", 0.35f, "idle", "focused", 1f), A("ord", 0.68f, "idle", "afraid", 1f, flip: true) }),
                    Speech("cp05_intro_p4", ComicShotType.CloseUp, "opera_red", "Ord",
                        "The night the coast went dark. Every scream, arranged. It has been performing it for eleven months.",
                        new[] { A("ord", 0.5f, "idle", "hollow", 1.25f) }),
                    Narration("cp05_intro_p5", ComicShotType.Wide, "opera_red",
                        "From the stage, a voice that is not one voice begins the overture.",
                        new[] { A("siren", 0.5f, "sing", "veil", 1.4f, silhouette: true) }, "speedlines")
                }, "mx_ch05_foyer"));

            c5.ComicPages.Add(Page("cp05_story", 5, 2, "ch05_beat_comic_story", "The Last Night",
                new[]
                {
                    Narration("cp05_story_p1", ComicShotType.Insert, "opera_red",
                        "The score on the understage is handwritten. The last page is dated eleven months ago.",
                        null, "flash"),
                    Speech("cp05_story_p2", ComicShotType.Medium, "opera_red", "Vera",
                        "This is a Directorate arrangement. Someone took the archive and set it to music.",
                        new[] { A("vera", 0.5f, "kneel", "sickened", 1.1f) }),
                    Speech("cp05_story_p3", ComicShotType.CloseUp, "opera_red", "Kade",
                        "Meridian is in the second movement. I can hear my own charge going off.",
                        new[] { A("kade", 0.5f, "idle", "grim", 1.2f) }),
                    Speech("cp05_story_p4", ComicShotType.OverShoulder, "opera_red", "Vera",
                        "Then we take the score and we take the singer. The Veil does not get to keep performing this.",
                        new[] { A("vera", 0.66f, "idle", "hard", 1.05f, flip: true), A("kade", 0.33f, "idle", "grim", 1f) }),
                    Narration("cp05_story_p5", ComicShotType.Wide, "opera_red",
                        "The orchestra pit fills with something that is not water and starts keeping time.", null, "shake")
                }));

            c5.ComicPages.Add(Page("cp05_boss", 5, 3, "ch05_beat_comic_boss", "Curtain Up",
                new[]
                {
                    Narration("cp05_boss_p1", ComicShotType.Establish, "opera_red",
                        "Main stage. Eleven months of performance and one performer who never stopped.",
                        null, "impact"),
                    Speech("cp05_boss_p2", ComicShotType.Wide, "opera_red", "Siren",
                        "You are late. We opened without you.",
                        new[] { A("siren", 0.5f, "sing", "veil", 1.45f) }),
                    Speech("cp05_boss_p3", ComicShotType.Medium, "opera_red", "Vera",
                        "Nobody came. It was only ever you and the recording.",
                        new[] { A("vera", 0.35f, "idle", "cold", 1.1f) }),
                    Speech("cp05_boss_p4", ComicShotType.CloseUp, "opera_red", "Siren",
                        "Then let us be loud enough for the whole coast.",
                        new[] { A("siren", 0.5f, "sing", "veil", 1.5f) }, "shake", "CRESCENDO"),
                    Narration("cp05_boss_p5", ComicShotType.Insert, "opera_red",
                        "The chandeliers swing. Sound becomes something with weight.",
                        new[] { A("kade", 0.3f, "aim", "hard", 1f), A("vera", 0.7f, "aim", "focused", 1f, flip: true) }, "speedlines")
                }, "mx_boss_siren", "boss"));

            c5.ComicPages.Add(Page("cp05_outro", 5, 4, "ch05_beat_comic_outro", "Curtain Down",
                new[]
                {
                    Narration("cp05_outro_p1", ComicShotType.Wide, "opera_red",
                        "The Siren stops mid-phrase. The house lights come up on an empty room.",
                        null, "impact"),
                    Speech("cp05_outro_p2", ComicShotType.Medium, "opera_red", "Ord",
                        "It is quiet. I have not had quiet in my head since the coast went dark.",
                        new[] { A("ord", 0.5f, "idle", "tearful", 1.05f) }),
                    Speech("cp05_outro_p3", ComicShotType.OverShoulder, "opera_red", "Kade",
                        "We are not done. There is a cathedral on the coast road that the Directorate built before the Veil.",
                        new[] { A("kade", 0.33f, "idle", "set", 1f), A("ord", 0.68f, "idle", "quiet", 1f, flip: true) }),
                    Speech("cp05_outro_p4", ComicShotType.CloseUp, "opera_red", "Vera",
                        "The Hollow Cathedral. The Veil's archive of the dead. That is where the Meridian order is stored.",
                        new[] { A("vera", 0.5f, "idle", "cold", 1.2f) }),
                    Narration("cp05_outro_p5", ComicShotType.Establish, "opera_red",
                        "They leave the score on the stage. Somebody should find it and know what happened here.", null, "zoom")
                }));
            content.Chapters.Add(c5);

            /* ============================================================
               CHAPTER 6 - HOLLOW CATHEDRAL   (125 min, level 30)
               ============================================================ */
            ChapterDefinition c6 = new ChapterDefinition
            {
                Index = 6,
                Title = "Hollow Cathedral",
                Subtitle = "The archive of the dead",
                ActTitle = ChapterRules.ActTitle(6),
                LocationName = "Hollow Cathedral",
                LocationId = "loc_cathedral",
                Synopsis =
                    "The Directorate built the cathedral to store what the Veil harvested. Now the archive has " +
                    "learned to sing on its own, and its heart is a swarm engine that calls itself the Choir. " +
                    "Somewhere in the crypts is the Meridian order, signed.",
                BossId = "boss_choir_engine",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(6),
                RecommendedLevel = ChapterRules.RecommendedLevel(6),
                MinimumLevel = ChapterRules.MinimumLevel(6),
                UnlockFlagOnComplete = "flag_ch06_complete"
            };
            c6.MiniBossIds.AddRange(new[] { "mb_brood_keeper", "mb_static_priest" });
            c6.SideQuestIds.AddRange(new[] { "q06_catalogue", "q06_den_mother", "q06_sanctuary" });
            c6.NewEnemyIds.AddRange(new[] { "e_memory_mites" });
            c6.ChapterRewardIds.AddRange(new[] { "g_trinket_marksman" });

            c6.Arenas.Add(Arena("arena_ch06_nave", 6, "Cathedral Nave", 54f, 20f, false, "cathedral_hollow",
                new[] { "hz_memory_shards", "hz_collapsing_walkway" }, StandardSpawnTags(),
                new[] { "prop_pew", "prop_archive_rack", "prop_pillar" }, "mx_ch06_nave"));
            c6.Arenas.Add(Arena("arena_ch06_crypt", 6, "Crypt Stair", 44f, 16f, false, "cathedral_hollow",
                new[] { "hz_memory_shards", "hz_flooded_gap" }, StandardSpawnTags(),
                new[] { "prop_archive_rack", "prop_coffin", "prop_pillar" }, "mx_ch06_crypt"));
            c6.Arenas.Add(Arena("arena_ch06_mb1", 6, "North Transept", 36f, 16f, false, "cathedral_hollow",
                new[] { "hz_memory_shards" }, StandardSpawnTags(),
                new[] { "prop_pew", "prop_archive_rack" }, "mx_miniboss"));
            c6.Arenas.Add(Arena("arena_ch06_mb2", 6, "Chapel of Static", 34f, 15f, false, "cathedral_hollow",
                new[] { "hz_resonance_field" }, StandardSpawnTags(),
                new[] { "prop_pillar", "prop_archive_rack" }, "mx_miniboss"));
            c6.Arenas.Add(Arena("arena_ch06_boss", 6, "Choir Loft", 58f, 21f, true, "cathedral_hollow",
                new[] { "hz_memory_shards", "hz_resonance_field", "hz_collapsing_walkway" }, StandardSpawnTags(),
                new[] { "prop_pillar", "prop_archive_rack", "prop_choir_stall" }, "mx_boss_choir"));

            c6.Savepoints.Add(Savepoint("sp06_nave", 6, "ch06_beat_save_a", "Nave - archive desk", "arena_ch06_nave", false));
            c6.Savepoints.Add(Savepoint("sp06_preboss", 6, "ch06_beat_save_b", "Crypt door - choir stair", "arena_ch06_boss", true));

            c6.Checkpoints.Add(Checkpoint("chk06_01", 6, "ch06_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c6.Checkpoints.Add(Checkpoint("chk06_02", 6, "ch06_beat_crypt", "Crypt reached", CheckpointTrigger.EnterArena));
            c6.Checkpoints.Add(Checkpoint("chk06_03", 6, "ch06_beat_save_b", "Before the Choir", CheckpointTrigger.PreBoss));
            c6.Checkpoints.Add(Checkpoint("chk06_04", 6, "ch06_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c6.Beats.Add(Beat(1, BeatKind.Comic, "ch06_beat_comic_intro", "Doors ajar", "Watch the opening page", "cp06_intro"));
            c6.Beats.Add(Beat(2, BeatKind.Exploration, "ch06_beat_nave", "Nave", "Cross the nave", "arena_ch06_nave"));
            c6.Beats.Add(Beat(3, BeatKind.Arena, "ch06_beat_enc_nave", "Nave swarm", "Clear the nave", "enc_ch06_nave"));
            c6.Beats.Add(Beat(4, BeatKind.Savepoint, "ch06_beat_save_a", "Archive desk", "Save at the archive desk", "sp06_nave"));
            c6.Beats.Add(Beat(5, BeatKind.SideQuestHook, "ch06_beat_hook_archive", "A voice in the racks", "Talk to the archive", "q06_catalogue"));
            c6.Beats.Add(Beat(6, BeatKind.MiniBossFight, "ch06_beat_mb1", "North transept", "Deal with the Brood Keeper", "mb_brood_keeper"));
            c6.Beats.Add(Beat(7, BeatKind.Comic, "ch06_beat_comic_story", "The order, signed", "Watch the mid-chapter page", "cp06_story"));
            c6.Beats.Add(Beat(8, BeatKind.Arena, "ch06_beat_crypt", "Crypt stair", "Descend the crypt", "enc_ch06_crypt"));
            c6.Beats.Add(Beat(9, BeatKind.MiniBossFight, "ch06_beat_mb2", "Chapel of static", "Deal with the Static Priest", "mb_static_priest", optional: true));
            c6.Beats.Add(Beat(10, BeatKind.Savepoint, "ch06_beat_save_b", "Choir stair", "Save before the loft", "sp06_preboss"));
            c6.Beats.Add(Beat(11, BeatKind.Comic, "ch06_beat_comic_boss", "The Choir inhales", "Watch the boss intro page", "cp06_boss"));
            c6.Beats.Add(Beat(12, BeatKind.BossFight, "ch06_beat_boss", "The Hollow Choir", "Defeat the Choir engine", "boss_choir_engine"));
            c6.Beats.Add(Beat(13, BeatKind.Comic, "ch06_beat_comic_outro", "Archive quiet", "Watch the closing page", "cp06_outro"));
            c6.Beats.Add(Beat(14, BeatKind.ChapterComplete, "ch06_beat_complete", "Chapter complete", "Autosave and continue", "flag_ch06_complete"));

            c6.ComicPages.Add(Page("cp06_intro", 6, 1, "ch06_beat_comic_intro", "Doors Ajar",
                new[]
                {
                    Narration("cp06_intro_p1", ComicShotType.Establish, "cathedral_hollow",
                        "The cathedral was built to hold the Veil's archive. Its doors have been ajar for a year.",
                        null, "zoom"),
                    Speech("cp06_intro_p2", ComicShotType.Medium, "cathedral_hollow", "Archivist",
                        "You are loud. Everyone here is very quiet. Please continue to be loud; I am recording it.",
                        new[] { A("archivist", 0.5f, "idle", "curious", 1.05f) }),
                    Speech("cp06_intro_p3", ComicShotType.OverShoulder, "cathedral_hollow", "Vera",
                        "Where is the Meridian order?",
                        new[] { A("vera", 0.35f, "idle", "focused", 1f), A("archivist", 0.68f, "idle", "curious", 1f, flip: true) }),
                    Speech("cp06_intro_p4", ComicShotType.CloseUp, "cathedral_hollow", "Archivist",
                        "Below. The Choir keeps the loud pages. It is very fond of Meridian.",
                        new[] { A("archivist", 0.5f, "idle", "still", 1.2f) }),
                    Narration("cp06_intro_p5", ComicShotType.Wide, "cathedral_hollow",
                        "The nave is full of dust that moves against the draught, like it is being read.",
                        null, "speedlines")
                }, "mx_ch06_nave"));

            c6.ComicPages.Add(Page("cp06_story", 6, 2, "ch06_beat_comic_story", "The Order, Signed",
                new[]
                {
                    Narration("cp06_story_p1", ComicShotType.Insert, "cathedral_hollow",
                        "The order is one page. It authorises the clearing of Meridian shelter, in the name of the Veil.",
                        null, "flash"),
                    Speech("cp06_story_p2", ComicShotType.CloseUp, "cathedral_hollow", "Kade",
                        "Conductor. Not Halden. The Conductor signed it, and Halden shot the survivors.",
                        new[] { A("kade", 0.5f, "idle", "dark", 1.25f) }),
                    Speech("cp06_story_p3", ComicShotType.Medium, "cathedral_hollow", "Vera",
                        "Conductor is a Directorate office, not a name. Whoever holds it is still giving orders.",
                        new[] { A("vera", 0.5f, "idle", "focused", 1.1f) }),
                    Speech("cp06_story_p4", ComicShotType.OverShoulder, "cathedral_hollow", "Kade",
                        "Then we take the office apart with whoever is inside it. Copy the page. Two copies.",
                        new[] { A("kade", 0.35f, "idle", "hard", 1f), A("vera", 0.68f, "idle", "resolute", 1f, flip: true) }),
                    Narration("cp06_story_p5", ComicShotType.Wide, "cathedral_hollow",
                        "Something in the crypt below starts singing the page back at them, word for word.", null, "shake")
                }, category: "story"));

            c6.ComicPages.Add(Page("cp06_boss", 6, 3, "ch06_beat_comic_boss", "The Choir Inhales",
                new[]
                {
                    Narration("cp06_boss_p1", ComicShotType.Establish, "cathedral_hollow",
                        "The choir loft is a machine the shape of an organ, and every pipe is a person's last minute.",
                        null, "impact"),
                    Speech("cp06_boss_p2", ComicShotType.Wide, "cathedral_hollow", "Choir",
                        "WE REMEMBER MERIDIAN. WE REMEMBER YOU STANDING IN THE DOOR.",
                        new[] { A("choir", 0.5f, "sing", "veil", 1.5f) }, effect: "shake"),
                    Speech("cp06_boss_p3", ComicShotType.CloseUp, "cathedral_hollow", "Kade",
                        "Good. Remember this too.",
                        new[] { A("kade", 0.5f, "aim", "hard", 1.25f) }),
                    Speech("cp06_boss_p4", ComicShotType.Medium, "cathedral_hollow", "Vera",
                        "Cut the pipes. Every pipe you break is one less voice it has to throw at us.",
                        new[] { A("vera", 0.5f, "point", "focused", 1.1f) }),
                    Narration("cp06_boss_p5", ComicShotType.Insert, "cathedral_hollow",
                        "Every pipe opens at once and the loft fills with sound and teeth.",
                        null, "speedlines", "CHORUS")
                }, "mx_boss_choir", "boss"));

            c6.ComicPages.Add(Page("cp06_outro", 6, 4, "ch06_beat_comic_outro", "Archive Quiet",
                new[]
                {
                    Narration("cp06_outro_p1", ComicShotType.Wide, "cathedral_hollow",
                        "The pipes go silent one at a time, like a room emptying. The archive is quiet for the first time.",
                        null, "impact"),
                    Speech("cp06_outro_p2", ComicShotType.Medium, "cathedral_hollow", "Archivist",
                        "I have the page. Both copies. I will keep them where the Veil does not index.",
                        new[] { A("archivist", 0.5f, "idle", "still", 1.05f) }),
                    Speech("cp06_outro_p3", ComicShotType.OverShoulder, "cathedral_hollow", "Vera",
                        "There is a district north of here made entirely of glass. The Veil keeps its faces there.",
                        new[] { A("vera", 0.6f, "idle", "focused", 1f, flip: true), A("archivist", 0.32f, "idle", "still", 1f) }),
                    Speech("cp06_outro_p4", ComicShotType.CloseUp, "cathedral_hollow", "Kade",
                        "Then let us go and take its face off.",
                        new[] { A("kade", 0.5f, "idle", "cold", 1.2f) }),
                    Narration("cp06_outro_p5", ComicShotType.Establish, "cathedral_hollow",
                        "Behind them, one pipe hums. Then stops. Then hums again, slightly out of tune.", null, "zoom")
                }));
            content.Chapters.Add(c6);

            /* ============================================================
               CHAPTER 7 - GLASS DISTRICT    (125 min, level 35)
               ============================================================ */
            ChapterDefinition c7 = new ChapterDefinition
            {
                Index = 7,
                Title = "Glass District",
                Subtitle = "Take its face off",
                ActTitle = ChapterRules.ActTitle(7),
                LocationName = "Glass District",
                LocationId = "loc_glass",
                Synopsis =
                    "The Veil wears the faces of people it archived. In the mirrored halls of the glass district " +
                    "one of them has learned to give orders, and it is using Kade's outline to do it.",
                BossId = "boss_nightglass",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(7),
                RecommendedLevel = ChapterRules.RecommendedLevel(7),
                MinimumLevel = ChapterRules.MinimumLevel(7),
                UnlockFlagOnComplete = "flag_ch07_complete"
            };
            c7.MiniBossIds.AddRange(new[] { "mb_nightglass", "mb_hooks" });
            c7.SideQuestIds.AddRange(new[] { "q07_shatter_twins", "q07_glass_run", "q07_reflections" });
            c7.NewEnemyIds.AddRange(new[] { "e_veil_stalker" });
            c7.ChapterRewardIds.AddRange(new[] { "perk_kade_concussive", "perk_vera_perfect_line" });

            c7.Arenas.Add(Arena("arena_ch07_atrium", 7, "Mirror Atrium", 50f, 19f, false, "glass_white",
                new[] { "hz_glass_floor", "hz_mirror_trap" }, StandardSpawnTags(),
                new[] { "prop_mirror_wall", "prop_glass_pillar", "prop_chandelier" }, "mx_ch07_atrium"));
            c7.Arenas.Add(Arena("arena_ch07_hall", 7, "Mirror Hall", 54f, 18f, false, "glass_white",
                new[] { "hz_glass_floor", "hz_mirror_trap", "hz_open_air" }, StandardSpawnTags(),
                new[] { "prop_mirror_wall", "prop_glass_pillar" }, "mx_ch07_hall"));
            c7.Arenas.Add(Arena("arena_ch07_mb1", 7, "Reflection Gallery", 38f, 16f, false, "glass_white",
                new[] { "hz_mirror_trap" }, StandardSpawnTags(),
                new[] { "prop_mirror_wall", "prop_glass_pillar" }, "mx_miniboss"));
            c7.Arenas.Add(Arena("arena_ch07_mb2", 7, "Hook Yard", 36f, 17f, false, "glass_white",
                new[] { "hz_hook_line", "hz_glass_floor" }, StandardSpawnTags(),
                new[] { "prop_hook_chain", "prop_crate" }, "mx_miniboss"));
            c7.Arenas.Add(Arena("arena_ch07_boss", 7, "Hall of Faces", 58f, 20f, true, "glass_white",
                new[] { "hz_glass_floor", "hz_mirror_trap", "hz_hook_line" }, StandardSpawnTags(),
                new[] { "prop_mirror_wall", "prop_glass_pillar", "prop_face_display" }, "mx_boss_glass"));

            c7.Savepoints.Add(Savepoint("sp07_atrium", 7, "ch07_beat_save_a", "Atrium - message drop", "arena_ch07_atrium", false));
            c7.Savepoints.Add(Savepoint("sp07_preboss", 7, "ch07_beat_save_b", "Hall of faces - entry", "arena_ch07_boss", true));

            c7.Checkpoints.Add(Checkpoint("chk07_01", 7, "ch07_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c7.Checkpoints.Add(Checkpoint("chk07_02", 7, "ch07_beat_hall", "Mirror hall crossed", CheckpointTrigger.EnterArena));
            c7.Checkpoints.Add(Checkpoint("chk07_03", 7, "ch07_beat_save_b", "Before Nightglass", CheckpointTrigger.PreBoss));
            c7.Checkpoints.Add(Checkpoint("chk07_04", 7, "ch07_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c7.Beats.Add(Beat(1, BeatKind.Comic, "ch07_beat_comic_intro", "Bad reflection", "Watch the opening page", "cp07_intro"));
            c7.Beats.Add(Beat(2, BeatKind.Exploration, "ch07_beat_atrium", "Mirror atrium", "Cross the atrium", "arena_ch07_atrium"));
            c7.Beats.Add(Beat(3, BeatKind.Arena, "ch07_beat_enc_atrium", "Atrium ambush", "Clear the atrium", "enc_ch07_atrium"));
            c7.Beats.Add(Beat(4, BeatKind.Savepoint, "ch07_beat_save_a", "Message drop", "Save at the drop", "sp07_atrium"));
            c7.Beats.Add(Beat(5, BeatKind.MiniBossFight, "ch07_beat_mb1", "Reflection gallery", "Deal with the Glass Twin", "mb_nightglass"));
            c7.Beats.Add(Beat(6, BeatKind.Comic, "ch07_beat_comic_story", "Wearing your face", "Watch the mid-chapter page", "cp07_story"));
            c7.Beats.Add(Beat(7, BeatKind.Arena, "ch07_beat_hall", "Mirror hall", "Cross the mirror hall", "enc_ch07_hall"));
            c7.Beats.Add(Beat(8, BeatKind.MiniBossFight, "ch07_beat_mb2", "Hook yard", "Deal with Hooks", "mb_hooks", optional: true));
            c7.Beats.Add(Beat(9, BeatKind.Savepoint, "ch07_beat_save_b", "Hall entry", "Save before the hall of faces", "sp07_preboss"));
            c7.Beats.Add(Beat(10, BeatKind.Comic, "ch07_beat_comic_boss", "Hall of faces", "Watch the boss intro page", "cp07_boss"));
            c7.Beats.Add(Beat(11, BeatKind.BossFight, "ch07_beat_boss", "Nightglass", "Defeat Nightglass", "boss_nightglass"));
            c7.Beats.Add(Beat(12, BeatKind.Comic, "ch07_beat_comic_outro", "Face off", "Watch the closing page", "cp07_outro"));
            c7.Beats.Add(Beat(13, BeatKind.ChapterComplete, "ch07_beat_complete", "Chapter complete", "Autosave and continue", "flag_ch07_complete"));

            c7.ComicPages.Add(Page("cp07_intro", 7, 1, "ch07_beat_comic_intro", "Bad Reflection",
                new[]
                {
                    Narration("cp07_intro_p1", ComicShotType.Establish, "glass_white",
                        "The glass district was the Directorate's archive of faces. Every window still shows someone.",
                        null, "zoom"),
                    Speech("cp07_intro_p2", ComicShotType.OverShoulder, "glass_white", "Vera",
                        "Kade. There are two of you in that window. One of them is giving orders to the street.",
                        new[] { A("vera", 0.6f, "idle", "alarmed", 1.05f, flip: true), A("kade", 0.3f, "idle", "grim", 1f) }),
                    Speech("cp07_intro_p3", ComicShotType.CloseUp, "glass_white", "Kade",
                        "Then it is using my voice to move troops. That has to stop today.",
                        new[] { A("kade", 0.5f, "idle", "hard", 1.25f) }),
                    Speech("cp07_intro_p4", ComicShotType.Medium, "glass_white", "Tace",
                        "Stalkers in the halls. They wear outlines and they know where you are before you do.",
                        new[] { A("tace", 0.5f, "idle", "wary", 1.05f) }),
                    Narration("cp07_intro_p5", ComicShotType.Wide, "glass_white",
                        "In every pane, a hundred archived people turn to look at the two real ones.",
                        null, "speedlines")
                }, "mx_ch07_atrium"));

            c7.ComicPages.Add(Page("cp07_story", 7, 2, "ch07_beat_comic_story", "Wearing Your Face",
                new[]
                {
                    Narration("cp07_story_p1", ComicShotType.Insert, "glass_white",
                        "A mirror plate holds the last minute of a Directorate officer. He is signing something.",
                        null, "flash"),
                    Speech("cp07_story_p2", ComicShotType.Medium, "glass_white", "Vera",
                        "The plate answers to the Conductor. It is not a person, it is a desk with an archive attached.",
                        new[] { A("vera", 0.5f, "idle", "focused", 1.1f) }),
                    Speech("cp07_story_p3", ComicShotType.CloseUp, "glass_white", "Kade",
                        "Then we take the face it is wearing and use it to walk into the undercity.",
                        new[] { A("kade", 0.5f, "idle", "calculating", 1.2f) }),
                    Speech("cp07_story_p4", ComicShotType.OverShoulder, "glass_white", "Vera",
                        "You do not use a stolen face. You break it, in front of the people it has been ordering around.",
                        new[] { A("vera", 0.66f, "idle", "hard", 1.05f, flip: true), A("kade", 0.33f, "idle", "flat", 1f) }),
                    Narration("cp07_story_p5", ComicShotType.Wide, "glass_white",
                        "Something in the next window copies Kade's posture exactly, one beat late.", null, "shake")
                }));

            c7.ComicPages.Add(Page("cp07_boss", 7, 3, "ch07_beat_comic_boss", "Hall of Faces",
                new[]
                {
                    Narration("cp07_boss_p1", ComicShotType.Establish, "glass_white",
                        "Hall of faces: two hundred panes, two hundred archivists, and one thing that has learned to move between them.",
                        null, "impact"),
                    Speech("cp07_boss_p2", ComicShotType.Wide, "glass_white", "Nightglass",
                        "I am the man who opened Meridian. I am the better version of it, too.",
                        new[] { A("nightglass", 0.5f, "idle", "smug", 1.45f) }),
                    Speech("cp07_boss_p3", ComicShotType.CloseUp, "glass_white", "Kade",
                        "You are a recording with ambition.",
                        new[] { A("kade", 0.5f, "aim", "cold", 1.25f) }),
                    Speech("cp07_boss_p4", ComicShotType.Medium, "glass_white", "Nightglass",
                        "Then hit me. I will be the pane behind you when you do.",
                        new[] { A("nightglass", 0.5f, "blink", "smug", 1.4f) }, "speedlines"),
                    Narration("cp07_boss_p5", ComicShotType.Insert, "glass_white",
                        "Two hundred reflections step forward. Six of them have weapons.",
                        null, "shake", "SHATTER")
                }, "mx_boss_glass", "boss"));

            c7.ComicPages.Add(Page("cp07_outro", 7, 4, "ch07_beat_comic_outro", "Face Off",
                new[]
                {
                    Narration("cp07_outro_p1", ComicShotType.Wide, "glass_white",
                        "Nightglass comes apart into panes, and each pane goes dark as it lands.",
                        null, "impact"),
                    Speech("cp07_outro_p2", ComicShotType.Medium, "glass_white", "Vera",
                        "It was never Kade. It was the Conductor's desk, wearing whichever face was useful.",
                        new[] { A("vera", 0.5f, "idle", "tired", 1.05f) }),
                    Speech("cp07_outro_p3", ComicShotType.OverShoulder, "glass_white", "Kade",
                        "Then the undercity is next, and this time the orders come from a person.",
                        new[] { A("kade", 0.33f, "idle", "set", 1f), A("vera", 0.68f, "idle", "tired", 1f, flip: true) }),
                    Speech("cp07_outro_p4", ComicShotType.CloseUp, "glass_white", "Tace",
                        "Undercity is mine to map. Bring rope, and bring a lot of ammunition.",
                        new[] { A("tace", 0.5f, "idle", "wry", 1.15f) }),
                    Narration("cp07_outro_p5", ComicShotType.Establish, "glass_white",
                        "Above the district, the Veil's light flickers like a signal being retuned.", null, "zoom")
                }));
            content.Chapters.Add(c7);

            /* ============================================================
               CHAPTER 8 - BONE YARDS        (130 min, level 40)
               ============================================================ */
            ChapterDefinition c8 = new ChapterDefinition
            {
                Index = 8,
                Title = "Bone Yards",
                Subtitle = "Everything the Veil threw away",
                ActTitle = ChapterRules.ActTitle(8),
                LocationName = "Bone Yards",
                LocationId = "loc_bone_yards",
                Synopsis =
                    "Everything the Veil harvested and could not use was dumped in the yards north of the " +
                    "glass district. Something ate it all and grew. The pack has an alpha, and the alpha is " +
                    "between the crew and the undercity road.",
                BossId = "boss_ironmaw",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(8),
                RecommendedLevel = ChapterRules.RecommendedLevel(8),
                MinimumLevel = ChapterRules.MinimumLevel(8),
                UnlockFlagOnComplete = "flag_ch08_complete"
            };
            c8.MiniBossIds.AddRange(new[] { "mb_ironjaw", "mb_hollow_choir" });
            c8.SideQuestIds.AddRange(new[] { "q08_yard_cull", "q08_alpha_down", "q08_feeding_time" });
            c8.NewEnemyIds.AddRange(new[] { "e_hollow_hound" });
            c8.ChapterRewardIds.AddRange(new[] { "g_core_bastion" });

            c8.Arenas.Add(Arena("arena_ch08_container", 8, "Container Maze", 52f, 18f, false, "yards_blood",
                new[] { "hz_crushing_container", "hz_hook_line" }, StandardSpawnTags(),
                new[] { "prop_container", "prop_bone_pile", "prop_gantry" }, "mx_ch08_maze"));
            c8.Arenas.Add(Arena("arena_ch08_gantry", 8, "North Gantry", 48f, 20f, false, "yards_blood",
                new[] { "hz_hook_line", "hz_open_air", "hz_bone_grinder" }, StandardSpawnTags(),
                new[] { "prop_gantry", "prop_container", "prop_bone_pile" }, "mx_ch08_gantry"));
            c8.Arenas.Add(Arena("arena_ch08_mb1", 8, "Yard Throne", 40f, 17f, false, "yards_blood",
                new[] { "hz_bone_grinder" }, StandardSpawnTags(),
                new[] { "prop_bone_pile", "prop_container" }, "mx_miniboss"));
            c8.Arenas.Add(Arena("arena_ch08_mb2", 8, "Cantors' Yard", 38f, 17f, false, "yards_blood",
                new[] { "hz_resonance_field" }, StandardSpawnTags(),
                new[] { "prop_bone_pile", "prop_gantry" }, "mx_miniboss"));
            c8.Arenas.Add(Arena("arena_ch08_boss", 8, "Feeding Floor", 60f, 20f, true, "yards_blood",
                new[] { "hz_bone_grinder", "hz_crushing_container", "hz_hook_line" }, StandardSpawnTags(),
                new[] { "prop_bone_pile", "prop_container", "prop_gantry" }, "mx_boss_beast"));

            c8.Savepoints.Add(Savepoint("sp08_maze", 8, "ch08_beat_save_a", "Container maze - crew camp", "arena_ch08_container", false));
            c8.Savepoints.Add(Savepoint("sp08_preboss", 8, "ch08_beat_save_b", "Feeding floor - barricade", "arena_ch08_boss", true));

            c8.Checkpoints.Add(Checkpoint("chk08_01", 8, "ch08_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c8.Checkpoints.Add(Checkpoint("chk08_02", 8, "ch08_beat_gantry", "Gantry cleared", CheckpointTrigger.EnterArena));
            c8.Checkpoints.Add(Checkpoint("chk08_03", 8, "ch08_beat_save_b", "Before Ironmaw", CheckpointTrigger.PreBoss));
            c8.Checkpoints.Add(Checkpoint("chk08_04", 8, "ch08_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c8.Beats.Add(Beat(1, BeatKind.Comic, "ch08_beat_comic_intro", "The smell", "Watch the opening page", "cp08_intro"));
            c8.Beats.Add(Beat(2, BeatKind.Exploration, "ch08_beat_maze", "Container maze", "Cross the maze", "arena_ch08_container"));
            c8.Beats.Add(Beat(3, BeatKind.Arena, "ch08_beat_enc_maze", "Pack in the maze", "Clear the maze", "enc_ch08_maze"));
            c8.Beats.Add(Beat(4, BeatKind.Savepoint, "ch08_beat_save_a", "Crew camp", "Save at the crew camp", "sp08_maze"));
            c8.Beats.Add(Beat(5, BeatKind.SideQuestHook, "ch08_beat_hook_ren", "Ren's yard crew", "Talk to Ren", "q08_alpha_down"));
            c8.Beats.Add(Beat(6, BeatKind.MiniBossFight, "ch08_beat_mb1", "Yard throne", "Deal with Ironjaw", "mb_ironjaw"));
            c8.Beats.Add(Beat(7, BeatKind.Comic, "ch08_beat_comic_story", "What the Veil throws away", "Watch the mid-chapter page", "cp08_story"));
            c8.Beats.Add(Beat(8, BeatKind.Arena, "ch08_beat_gantry", "North gantry", "Clear the gantry", "enc_ch08_gantry"));
            c8.Beats.Add(Beat(9, BeatKind.MiniBossFight, "ch08_beat_mb2", "Cantors' yard", "Deal with the Hollow Cantor", "mb_hollow_choir", optional: true));
            c8.Beats.Add(Beat(10, BeatKind.Savepoint, "ch08_beat_save_b", "Barricade", "Save before the feeding floor", "sp08_preboss"));
            c8.Beats.Add(Beat(11, BeatKind.Comic, "ch08_beat_comic_boss", "Feeding floor", "Watch the boss intro page", "cp08_boss"));
            c8.Beats.Add(Beat(12, BeatKind.BossFight, "ch08_beat_boss", "Ironmaw", "Defeat Ironmaw", "boss_ironmaw"));
            c8.Beats.Add(Beat(13, BeatKind.Comic, "ch08_beat_comic_outro", "Road open", "Watch the closing page", "cp08_outro"));
            c8.Beats.Add(Beat(14, BeatKind.ChapterComplete, "ch08_beat_complete", "Chapter complete", "Autosave and continue", "flag_ch08_complete"));

            c8.ComicPages.Add(Page("cp08_intro", 8, 1, "ch08_beat_comic_intro", "The Smell",
                new[]
                {
                    Narration("cp08_intro_p1", ComicShotType.Establish, "yards_blood",
                        "The Bone Yards were a landfill for the Veil's leftovers. Then something moved in.",
                        null, "zoom"),
                    Speech("cp08_intro_p2", ComicShotType.Medium, "yards_blood", "Ren",
                        "It ate the trucks first. Then it ate the drivers. Now it walks the maze like it owns the place.",
                        new[] { A("ren", 0.5f, "idle", "grim", 1.05f) }),
                    Speech("cp08_intro_p3", ComicShotType.OverShoulder, "yards_blood", "Vera",
                        "It does own the place. Nothing else would live here.",
                        new[] { A("vera", 0.62f, "idle", "flat", 1f, flip: true), A("ren", 0.32f, "idle", "grim", 0.95f) }),
                    Speech("cp08_intro_p4", ComicShotType.CloseUp, "yards_blood", "Kade",
                        "The undercity road runs through this yard. So we go through it, or over it.",
                        new[] { A("kade", 0.5f, "idle", "set", 1.2f) }),
                    Narration("cp08_intro_p5", ComicShotType.Wide, "yards_blood",
                        "Somewhere in the maze, a sound like a hydraulic door closing, repeated and hunting.",
                        null, "speedlines")
                }, "mx_ch08_maze"));

            c8.ComicPages.Add(Page("cp08_story", 8, 2, "ch08_beat_comic_story", "What the Veil Throws Away",
                new[]
                {
                    Narration("cp08_story_p1", ComicShotType.Insert, "yards_blood",
                        "Bone, plating, memory shards, uniforms with names still stitched into them.",
                        null, "flash"),
                    Speech("cp08_story_p2", ComicShotType.Medium, "yards_blood", "Vera",
                        "These are Directorate issue. The Veil dumps its own dead here as well as ours.",
                        new[] { A("vera", 0.5f, "kneel", "grim", 1.1f) }),
                    Speech("cp08_story_p3", ComicShotType.CloseUp, "yards_blood", "Kade",
                        "Then it is not a monster. It is a scavenger, and this is its pantry.",
                        new[] { A("kade", 0.5f, "idle", "flat", 1.2f) }),
                    Speech("cp08_story_p4", ComicShotType.OverShoulder, "yards_blood", "Vera",
                        "Do not get sentimental about it. It will eat Pada while we talk.",
                        new[] { A("vera", 0.66f, "idle", "hard", 1.05f, flip: true), A("kade", 0.33f, "idle", "grim", 1f) }),
                    Narration("cp08_story_p5", ComicShotType.Wide, "yards_blood",
                        "Two dozen sets of eyes open along the container line, all facing the same way.", null, "shake")
                }));

            c8.ComicPages.Add(Page("cp08_boss", 8, 3, "ch08_beat_comic_boss", "Feeding Floor",
                new[]
                {
                    Narration("cp08_boss_p1", ComicShotType.Establish, "yards_blood",
                        "The feeding floor is where the yard tips its refuse. Something has been waiting at the bottom of it.",
                        null, "impact"),
                    Speech("cp08_boss_p2", ComicShotType.Wide, "yards_blood", "Ironmaw",
                        "GROOOOOAAAR.",
                        new[] { A("ironmaw", 0.5f, "roar", "beast", 1.7f) }, effect: "shake"),
                    Speech("cp08_boss_p3", ComicShotType.Medium, "yards_blood", "Kade",
                        "It is not a fight. It is a cull.",
                        new[] { A("kade", 0.35f, "aim", "cold", 1.1f) }),
                    Speech("cp08_boss_p4", ComicShotType.Medium, "yards_blood", "Vera",
                        "Watch the head before it charges. That is the whole fight, right there.",
                        new[] { A("vera", 0.5f, "aim", "focused", 1.1f) }),
                    Narration("cp08_boss_p5", ComicShotType.Insert, "yards_blood",
                        "It lowers its head, and the yard is suddenly very small.",
                        null, "speedlines", "SLAM")
                }, "mx_boss_beast", "boss"));

            c8.ComicPages.Add(Page("cp08_outro", 8, 4, "ch08_beat_comic_outro", "Road Open",
                new[]
                {
                    Narration("cp08_outro_p1", ComicShotType.Wide, "yards_blood",
                        "Ironmaw folds into the refuse it grew from. The hounds scatter into the empty yards.",
                        null, "impact"),
                    Speech("cp08_outro_p2", ComicShotType.Medium, "yards_blood", "Pada",
                        "Gantry is running. I can have the crews through in an hour.",
                        new[] { A("pada", 0.5f, "idle", "relieved", 1.05f) }),
                    Speech("cp08_outro_p3", ComicShotType.OverShoulder, "yards_blood", "Vera",
                        "Undercity is under our feet. The Veil built its waste cathedrals down there.",
                        new[] { A("vera", 0.6f, "idle", "focused", 1f, flip: true), A("pada", 0.32f, "idle", "relieved", 1f) }),
                    Speech("cp08_outro_p4", ComicShotType.CloseUp, "yards_blood", "Kade",
                        "Then we go down. Every floor closer to the Conductor is a floor the Veil loses.",
                        new[] { A("kade", 0.5f, "idle", "set", 1.2f) }),
                    Narration("cp08_outro_p5", ComicShotType.Establish, "yards_blood",
                        "Below the yards, something enormous shifts its weight and settles. The Veil is waking up.",
                        null, "zoom")
                }));
            content.Chapters.Add(c8);
        }
    }
}
