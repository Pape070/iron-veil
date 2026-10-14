namespace IronVeil.Rules
{
    public static partial class Campaign
    {
        private static void BuildChaptersActOne(CampaignContent content)
        {
            /* ============================================================
               CHAPTER 1 - THE RUST GATE     (105 min, level 1)
               ============================================================ */
            ChapterDefinition c1 = new ChapterDefinition
            {
                Index = 1,
                Title = "The Rust Gate",
                Subtitle = "Breach the outer checkpoint",
                ActTitle = ChapterRules.ActTitle(1),
                LocationName = "Rust Gate Checkpoint",
                LocationId = "loc_rust_gate",
                Synopsis =
                    "The Iron Veil has sealed the coastal states behind checkpoint walls. Kade Rourke and " +
                    "Vera Voss hit the outermost gate, where Warden Skarn has been keeping the Directorate's " +
                    "exit closed for eleven months. Break the gate, and the road into the coast opens.",
                BossId = "boss_skarn",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(1),
                RecommendedLevel = ChapterRules.RecommendedLevel(1),
                MinimumLevel = ChapterRules.MinimumLevel(1),
                UnlockFlagOnComplete = "flag_ch01_complete"
            };
            c1.MiniBossIds.AddRange(new[] { "mb_rustpan", "mb_turnkey" });
            c1.SideQuestIds.AddRange(new[] { "q01_scrap_run", "q01_lost_cartography", "q01_wall_of_names" });
            c1.NewEnemyIds.AddRange(new[] { "e_rust_drone", "e_line_trooper", "e_breach_hound" });

            c1.Arenas.Add(Arena("arena_ch01_gate", 1, "Gate Approach", 46f, 16f, false, "ruins_day",
                new[] { "hz_barbed_wire", "hz_floodlight" }, StandardSpawnTags(),
                new[] { "prop_barrier", "prop_crate", "prop_watchtower" }, "mx_ch01_approach"));
            c1.Arenas.Add(Arena("arena_ch01_yard", 1, "Scrap Yard", 50f, 18f, false, "ruins_day",
                new[] { "hz_crusher", "hz_collapsing_walkway" }, StandardSpawnTags(),
                new[] { "prop_scrap_pile", "prop_conveyor", "prop_crane" }, "mx_ch01_yard"));
            c1.Arenas.Add(Arena("arena_ch01_mb1", 1, "Junction Pit", 34f, 14f, false, "ruins_day",
                new[] { "hz_barbed_wire" }, StandardSpawnTags(),
                new[] { "prop_scrap_pile", "prop_crate" }, "mx_miniboss"));
            c1.Arenas.Add(Arena("arena_ch01_mb2", 1, "Warden's Booth", 30f, 13f, false, "ruins_day",
                new[] { "hz_floodlight" }, StandardSpawnTags(),
                new[] { "prop_watchtower", "prop_barrier" }, "mx_miniboss"));
            c1.Arenas.Add(Arena("arena_ch01_boss", 1, "Gate Control", 52f, 18f, true, "ruins_day",
                new[] { "hz_gate_piston", "hz_barbed_wire" }, StandardSpawnTags(),
                new[] { "prop_gate_winch", "prop_barrier", "prop_crate" }, "mx_boss_iron"));

            c1.Savepoints.Add(Savepoint("sp01_gate", 1, "ch01_beat_save_a", "Gate approach - supply cache", "arena_ch01_gate", false));
            c1.Savepoints.Add(Savepoint("sp01_preboss", 1, "ch01_beat_save_b", "Gate control - breach line", "arena_ch01_boss", true));

            c1.Checkpoints.Add(Checkpoint("chk01_01", 1, "ch01_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c1.Checkpoints.Add(Checkpoint("chk01_02", 1, "ch01_beat_yard", "Scrap yard cleared", CheckpointTrigger.EnterArena));
            c1.Checkpoints.Add(Checkpoint("chk01_03", 1, "ch01_beat_save_b", "Before Warden Skarn", CheckpointTrigger.PreBoss));
            c1.Checkpoints.Add(Checkpoint("chk01_04", 1, "ch01_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c1.Beats.Add(Beat(1, BeatKind.Comic, "ch01_beat_comic_intro", "Cold open", "Watch the opening page", "cp01_intro"));
            c1.Beats.Add(Beat(2, BeatKind.Exploration, "ch01_beat_gate", "Approach the gate", "Cross the checkpoint approach", "arena_ch01_gate"));
            c1.Beats.Add(Beat(3, BeatKind.Arena, "ch01_beat_enc_gate", "Picket line", "Clear the Directorate picket", "enc_ch01_gate"));
            c1.Beats.Add(Beat(4, BeatKind.Savepoint, "ch01_beat_save_a", "Supply cache", "Save at the cache", "sp01_gate"));
            c1.Beats.Add(Beat(5, BeatKind.MiniBossFight, "ch01_beat_mb1", "Scrap yard bully", "Deal with Rustpan", "mb_rustpan"));
            c1.Beats.Add(Beat(6, BeatKind.Comic, "ch01_beat_comic_story", "What the gate is for", "Watch the mid-chapter page", "cp01_story"));
            c1.Beats.Add(Beat(7, BeatKind.Arena, "ch01_beat_yard", "Scrap yard", "Clear the scrap yard", "enc_ch01_yard"));
            c1.Beats.Add(Beat(8, BeatKind.MiniBossFight, "ch01_beat_mb2", "Gate operator", "Deal with Turnkey", "mb_turnkey", optional: true));
            c1.Beats.Add(Beat(9, BeatKind.Savepoint, "ch01_beat_save_b", "Breach line", "Save before the warden", "sp01_preboss"));
            c1.Beats.Add(Beat(10, BeatKind.Comic, "ch01_beat_comic_boss", "The warden speaks", "Watch the boss intro page", "cp01_boss"));
            c1.Beats.Add(Beat(11, BeatKind.BossFight, "ch01_beat_boss", "Warden Skarn", "Defeat Warden Skarn", "boss_skarn"));
            c1.Beats.Add(Beat(12, BeatKind.Comic, "ch01_beat_comic_outro", "Through the gate", "Watch the closing page", "cp01_outro"));
            c1.Beats.Add(Beat(13, BeatKind.ChapterComplete, "ch01_beat_complete", "Chapter complete", "Autosave and continue", "flag_ch01_complete"));

            c1.ComicPages.Add(Page("cp01_intro", 1, 1, "ch01_beat_comic_intro", "Eleven Months Later",
                new[]
                {
                    Narration("cp01_intro_p1", ComicShotType.Establish, "ruins_day",
                        "Eleven months ago, the Directorate sealed the sky over the coast and called it the Iron Veil.",
                        new[] { A("kade", 0.18f, "walk", "grim", 0.7f, silhouette: true), A("vera", 0.34f, "walk", "grim", 0.7f, silhouette: true) }),
                    Speech("cp01_intro_p2", ComicShotType.Medium, "ruins_day", "Kade",
                        "Gate's still standing.",
                        new[] { A("kade", 0.35f, "idle", "grim", 1f) }),
                    Speech("cp01_intro_p3", ComicShotType.CloseUp, "ruins_day", "Vera",
                        "Gate's still watched. Two squads, a warden, and a turret on the far tower.",
                        new[] { A("vera", 0.62f, "crouch", "focused", 1f, flip: true) }),
                    Speech("cp01_intro_p4", ComicShotType.OverShoulder, "ruins_day", "Kade",
                        "I brought charges.",
                        new[] { A("kade", 0.3f, "idle", "flat", 1f), A("vera", 0.62f, "idle", "skeptical", 1f, flip: true) }),
                    Speech("cp01_intro_p5", ComicShotType.Wide, "ruins_day", "Vera",
                        "Then put them back. We need the gate to open, not to fall over. The road is inside the gate.",
                        new[] { A("kade", 0.25f, "idle", "flat", 0.95f), A("vera", 0.7f, "point", "focused", 0.95f, flip: true), A("helion_trooper", 0.9f, "patrol", "helmet", 0.8f, silhouette: true) }),
                    Narration("cp01_intro_p6", ComicShotType.Insert, "ruins_day",
                        "Behind them, Ivo's crew waits with a truck of scrap and no plan.",
                        new[] { A("ivo", 0.5f, "idle", "worried", 1f) }, "speedlines")
                }));

            c1.ComicPages.Add(Page("cp01_story", 1, 2, "ch01_beat_comic_story", "The Wall of Names",
                new[]
                {
                    Narration("cp01_story_p1", ComicShotType.Wide, "ruins_day",
                        "Inside the gate, the concrete is covered in names scratched by people who never got out.",
                        new[] { A("kade", 0.4f, "idle", "still", 1f, silhouette: true) }),
                    Speech("cp01_story_p2", ComicShotType.Medium, "ruins_day", "Kade",
                        "That one is Meridian. I put the charge on that shelter door.",
                        new[] { A("kade", 0.45f, "idle", "grim", 1.1f) }),
                    Speech("cp01_story_p3", ComicShotType.OverShoulder, "ruins_day", "Vera",
                        "You were following an order. The order was the crime, not your hands.",
                        new[] { A("vera", 0.6f, "idle", "soft", 1f, flip: true), A("kade", 0.3f, "idle", "grim", 1f) }),
                    Speech("cp01_story_p4", ComicShotType.CloseUp, "ruins_day", "Kade",
                        "The order had a signature on it. I am going to find it and burn it.",
                        new[] { A("kade", 0.5f, "idle", "hard", 1.25f) }),
                    Speech("cp01_story_p5", ComicShotType.Wide, "ruins_day", "Vera",
                        "Then we agree. The Veil keeps the signature archived. Twelve chapters of walking, and we take it apart.",
                        new[] { A("vera", 0.68f, "walk", "resolute", 1f, flip: true), A("kade", 0.3f, "walk", "grim", 1f) })
                }));

            c1.ComicPages.Add(Page("cp01_boss", 1, 3, "ch01_beat_comic_boss", "Keeper of the Rust Gate",
                new[]
                {
                    Narration("cp01_boss_p1", ComicShotType.Establish, "ruins_day",
                        "Gate control. Two hundred tonnes of counterweight, and the man who keeps it shut.",
                        null, "impact"),
                    Speech("cp01_boss_p2", ComicShotType.Wide, "ruins_day", "Skarn",
                        "You are standing inside a door. I am the lock.",
                        new[] { A("skarn", 0.5f, "guard", "cold", 1.35f, silhouette: true) }),
                    Speech("cp01_boss_p3", ComicShotType.Medium, "ruins_day", "Kade",
                        "Locks break. That is the whole job.",
                        new[] { A("kade", 0.3f, "aim", "hard", 1.1f) }),
                    Speech("cp01_boss_p4", ComicShotType.CloseUp, "ruins_day", "Skarn",
                        "Then you will have to do it the loud way. I have been looking forward to the loud way.",
                        new[] { A("skarn", 0.5f, "guard", "grin", 1.4f) }),
                    Narration("cp01_boss_p5", ComicShotType.Insert, "ruins_day",
                        "The gate starts to close. The fight starts before it finishes.",
                        null, "shake", "CLANG")
                }, "mx_boss_iron", "boss"));

            c1.ComicPages.Add(Page("cp01_outro", 1, 4, "ch01_beat_comic_outro", "Through the Gate",
                new[]
                {
                    Narration("cp01_outro_p1", ComicShotType.Wide, "ruins_day",
                        "The counterweight comes down. The gate opens onto a coast road nobody has driven in a year.",
                        new[] { A("kade", 0.3f, "idle", "exhausted", 1f), A("vera", 0.7f, "idle", "exhausted", 1f, flip: true) }),
                    Speech("cp01_outro_p2", ComicShotType.Medium, "ruins_day", "Ivo",
                        "Road's clear for maybe a day. After that, the Directorate sends somebody who matters.",
                        new[] { A("ivo", 0.5f, "idle", "worried", 1.05f) }),
                    Speech("cp01_outro_p3", ComicShotType.OverShoulder, "ruins_day", "Vera",
                        "Somebody who matters is exactly who I drew maps against. Keep the truck ready.",
                        new[] { A("vera", 0.6f, "idle", "focused", 1f, flip: true), A("ivo", 0.3f, "idle", "worried", 0.9f) }),
                    Speech("cp01_outro_p4", ComicShotType.CloseUp, "ruins_day", "Kade",
                        "Next stop is the docks. The Veil loses its shipping, it loses its teeth.",
                        new[] { A("kade", 0.5f, "idle", "set", 1.2f) }),
                    Narration("cp01_outro_p5", ComicShotType.Establish, "ruins_day",
                        "Beyond the gate, the coast is quiet in the wrong way. The Veil is listening.",
                        new[] { A("kade", 0.25f, "walk", "grim", 0.8f), A("vera", 0.7f, "walk", "grim", 0.8f, flip: true) }, "zoom")
                }));
            content.Chapters.Add(c1);

            /* ============================================================
               CHAPTER 2 - SUNKEN DOCKS     (110 min, level 6)
               ============================================================ */
            ChapterDefinition c2 = new ChapterDefinition
            {
                Index = 2,
                Title = "Sunken Docks",
                Subtitle = "Cut the coast's shipping",
                ActTitle = ChapterRules.ActTitle(2),
                LocationName = "Sunken Docks",
                LocationId = "loc_sunken_docks",
                Synopsis =
                    "The Directorate moves supplies through the flooded harbour. Dockmaster Ren keeps the last " +
                    "working cranes alive while a siege frame called the Tidewalker wades through the container " +
                    "rows. Kill the shipping, and the coast starts to starve.",
                BossId = "boss_tidewalker",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(2),
                RecommendedLevel = ChapterRules.RecommendedLevel(2),
                MinimumLevel = ChapterRules.MinimumLevel(2),
                UnlockFlagOnComplete = "flag_ch02_complete"
            };
            c2.MiniBossIds.AddRange(new[] { "mb_wreck_diver", "mb_salt_tongue" });
            c2.SideQuestIds.AddRange(new[] { "q02_dock_rights", "q02_deep_salvage", "q02_tide_charts" });
            c2.NewEnemyIds.AddRange(new[] { "e_sentinel_turret", "e_skiff" });
            c2.ChapterRewardIds.AddRange(new[] { "g_trinket_salvager" });

            c2.Arenas.Add(Arena("arena_ch02_pier", 2, "South Pier", 48f, 17f, false, "docks_dusk",
                new[] { "hz_flooded_gap", "hz_crane_swing" }, StandardSpawnTags(),
                new[] { "prop_container", "prop_crane", "prop_buoy" }, "mx_ch02_pier"));
            c2.Arenas.Add(Arena("arena_ch02_hold", 2, "Flooded Hold", 44f, 16f, false, "docks_dusk",
                new[] { "hz_flooded_gap", "hz_steam_vent" }, StandardSpawnTags(),
                new[] { "prop_container", "prop_barrel", "prop_pump" }, "mx_ch02_hold"));
            c2.Arenas.Add(Arena("arena_ch02_mb1", 2, "Wreck Site", 36f, 15f, false, "docks_dusk",
                new[] { "hz_flooded_gap" }, StandardSpawnTags(),
                new[] { "prop_wreck", "prop_buoy" }, "mx_miniboss"));
            c2.Arenas.Add(Arena("arena_ch02_mb2", 2, "Dockmaster's Shed", 32f, 14f, false, "docks_dusk",
                new[] { "hz_steam_vent" }, StandardSpawnTags(),
                new[] { "prop_barrel", "prop_crate" }, "mx_miniboss"));
            c2.Arenas.Add(Arena("arena_ch02_boss", 2, "Harbour Basin", 56f, 20f, true, "docks_dusk",
                new[] { "hz_flooded_gap", "hz_rising_water", "hz_steam_vent" }, StandardSpawnTags(),
                new[] { "prop_crane", "prop_container", "prop_buoy" }, "mx_boss_tide"));

            c2.Savepoints.Add(Savepoint("sp02_pier", 2, "ch02_beat_save_a", "South pier - Ren's shed", "arena_ch02_pier", false));
            c2.Savepoints.Add(Savepoint("sp02_preboss", 2, "ch02_beat_save_b", "Harbour basin - dry crate", "arena_ch02_boss", true));

            c2.Checkpoints.Add(Checkpoint("chk02_01", 2, "ch02_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c2.Checkpoints.Add(Checkpoint("chk02_02", 2, "ch02_beat_hold", "Hold flooded", CheckpointTrigger.EnterArena));
            c2.Checkpoints.Add(Checkpoint("chk02_03", 2, "ch02_beat_save_b", "Before the Tidewalker", CheckpointTrigger.PreBoss));
            c2.Checkpoints.Add(Checkpoint("chk02_04", 2, "ch02_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c2.Beats.Add(Beat(1, BeatKind.Comic, "ch02_beat_comic_intro", "Low tide", "Watch the opening page", "cp02_intro"));
            c2.Beats.Add(Beat(2, BeatKind.Exploration, "ch02_beat_pier", "South pier", "Work along the pier", "arena_ch02_pier"));
            c2.Beats.Add(Beat(3, BeatKind.Arena, "ch02_beat_enc_pier", "Turret line", "Clear the pier picket", "enc_ch02_pier"));
            c2.Beats.Add(Beat(4, BeatKind.Savepoint, "ch02_beat_save_a", "Ren's shed", "Save at Ren's shed", "sp02_pier"));
            c2.Beats.Add(Beat(5, BeatKind.MiniBossFight, "ch02_beat_mb1", "Wreck site", "Deal with the Wreck Diver", "mb_wreck_diver"));
            c2.Beats.Add(Beat(6, BeatKind.Comic, "ch02_beat_comic_story", "Who flooded the city", "Watch the mid-chapter page", "cp02_story"));
            c2.Beats.Add(Beat(7, BeatKind.Arena, "ch02_beat_hold", "Flooded hold", "Clear the flooded hold", "enc_ch02_hold"));
            c2.Beats.Add(Beat(8, BeatKind.MiniBossFight, "ch02_beat_mb2", "Dock enforcer", "Deal with Salt Tongue", "mb_salt_tongue", optional: true));
            c2.Beats.Add(Beat(9, BeatKind.Savepoint, "ch02_beat_save_b", "Dry crate", "Save before the basin", "sp02_preboss"));
            c2.Beats.Add(Beat(10, BeatKind.Comic, "ch02_beat_comic_boss", "Tidewalker wades in", "Watch the boss intro page", "cp02_boss"));
            c2.Beats.Add(Beat(11, BeatKind.BossFight, "ch02_beat_boss", "Tidewalker", "Defeat the Tidewalker", "boss_tidewalker"));
            c2.Beats.Add(Beat(12, BeatKind.Comic, "ch02_beat_comic_outro", "Shipping stopped", "Watch the closing page", "cp02_outro"));
            c2.Beats.Add(Beat(13, BeatKind.ChapterComplete, "ch02_beat_complete", "Chapter complete", "Autosave and continue", "flag_ch02_complete"));

            c2.ComicPages.Add(Page("cp02_intro", 2, 1, "ch02_beat_comic_intro", "Low Tide",
                new[]
                {
                    Narration("cp02_intro_p1", ComicShotType.Establish, "docks_dusk",
                        "The docks were the coast's whole supply line. Now they are a lake with cranes in it.",
                        new[] { A("kade", 0.2f, "walk", "grim", 0.75f, silhouette: true), A("vera", 0.75f, "walk", "grim", 0.75f, flip: true, silhouette: true) }),
                    Speech("cp02_intro_p2", ComicShotType.Medium, "docks_dusk", "Ren",
                        "You are the ones who opened the Rust Gate. Good. Now open my harbour.",
                        new[] { A("ren", 0.5f, "idle", "tired", 1.1f) }),
                    Speech("cp02_intro_p3", ComicShotType.OverShoulder, "docks_dusk", "Vera",
                        "What is in the water that eats your crews?",
                        new[] { A("vera", 0.4f, "idle", "focused", 1f), A("ren", 0.7f, "idle", "tired", 1f, flip: true) }),
                    Speech("cp02_intro_p4", ComicShotType.Wide, "docks_dusk", "Ren",
                        "A siege frame. It walks the basin like it still has a schedule to keep.",
                        new[] { A("ren", 0.5f, "point", "grim", 1f), A("tidewalker", 0.85f, "walk", "machine", 1.1f, silhouette: true) }),
                    Narration("cp02_intro_p5", ComicShotType.Insert, "docks_dusk",
                        "Somewhere under the water, the Tidewalker's legs find the bottom and keep going.", null, "zoom")
                }, "mx_ch02_pier"));

            c2.ComicPages.Add(Page("cp02_story", 2, 2, "ch02_beat_comic_story", "Who Flooded the City",
                new[]
                {
                    Narration("cp02_story_p1", ComicShotType.Wide, "docks_dusk",
                        "The lock gates were opened from the inside. Someone wanted this harbour drowned.", null, "flash"),
                    Speech("cp02_story_p2", ComicShotType.Medium, "docks_dusk", "Vera",
                        "These are Directorate lock codes. Same signature block as the Meridian order.",
                        new[] { A("vera", 0.55f, "kneel", "focused", 1.05f) }),
                    Speech("cp02_story_p3", ComicShotType.CloseUp, "docks_dusk", "Kade",
                        "Halden. He signs everything. He signs it and then he watches it happen from a roof.",
                        new[] { A("kade", 0.5f, "idle", "hard", 1.2f) }),
                    Speech("cp02_story_p4", ComicShotType.OverShoulder, "docks_dusk", "Vera",
                        "Then the rooftops are next. Keep the codes. They are evidence.",
                        new[] { A("vera", 0.65f, "idle", "resolute", 1f, flip: true), A("kade", 0.32f, "idle", "grim", 1f) }),
                    Narration("cp02_story_p5", ComicShotType.Establish, "docks_dusk",
                        "The water rises a centimetre. The Tidewalker is closer than the last page.",
                        null, "speedlines")
                }));

            c2.ComicPages.Add(Page("cp02_boss", 2, 3, "ch02_beat_comic_boss", "Harbour Basin",
                new[]
                {
                    Narration("cp02_boss_p1", ComicShotType.Establish, "docks_dusk",
                        "The basin is open water and dry crates. Only one of those is a safe place to stand.",
                        null, "impact"),
                    Speech("cp02_boss_p2", ComicShotType.Wide, "docks_dusk", "Vera",
                        "Its legs are the weak point. The armour stops at the hip joints.",
                        new[] { A("vera", 0.3f, "point", "focused", 1.05f) }),
                    Speech("cp02_boss_p3", ComicShotType.Medium, "docks_dusk", "Kade",
                        "Then I will keep it looking at me. You keep the legs honest.",
                        new[] { A("kade", 0.55f, "aim", "set", 1.1f) }),
                    Narration("cp02_boss_p4", ComicShotType.Wide, "docks_dusk",
                        "The Tidewalker surfaces, sheds a wall of harbour water, and opens every valve it has.",
                        new[] { A("tidewalker", 0.5f, "roar", "machine", 1.6f) }, "shake", "GROAN"),
                    Narration("cp02_boss_p5", ComicShotType.Insert, "docks_dusk",
                        "The water is coming up. The fight has a clock on it now.", null, "speedlines")
                }, "mx_boss_tide", "boss"));

            c2.ComicPages.Add(Page("cp02_outro", 2, 4, "ch02_beat_comic_outro", "Shipping Stopped",
                new[]
                {
                    Narration("cp02_outro_p1", ComicShotType.Wide, "docks_dusk",
                        "The Tidewalker goes down in the basin it drowned. The harbour goes quiet.",
                        null, "impact"),
                    Speech("cp02_outro_p2", ComicShotType.Medium, "docks_dusk", "Ren",
                        "Nothing moves through here now. Not us, not them. You cost them the coast and you cost me my work.",
                        new[] { A("ren", 0.5f, "idle", "bitter", 1.05f) }),
                    Speech("cp02_outro_p3", ComicShotType.OverShoulder, "docks_dusk", "Kade",
                        "You get your harbour back when the Veil comes down. That was always the deal.",
                        new[] { A("kade", 0.35f, "idle", "flat", 1f), A("ren", 0.68f, "idle", "bitter", 1f, flip: true) }),
                    Speech("cp02_outro_p4", ComicShotType.CloseUp, "docks_dusk", "Vera",
                        "Halden signs his work. He signed this harbour and he signed Meridian. Next stop is his rooftops.",
                        new[] { A("vera", 0.5f, "idle", "cold", 1.2f) }),
                    Narration("cp02_outro_p5", ComicShotType.Establish, "docks_dusk",
                        "The road turns inland and up. On the last roof of the district, a scope glints.", null, "zoom")
                }));
            content.Chapters.Add(c2);

            /* ============================================================
               CHAPTER 3 - ROOFTOP WATCH     (115 min, level 12)
               ============================================================ */
            ChapterDefinition c3 = new ChapterDefinition
            {
                Index = 3,
                Title = "Rooftop Watch",
                Subtitle = "Climb into Halden's sightlines",
                ActTitle = ChapterRules.ActTitle(3),
                LocationName = "Watch District Rooftops",
                LocationId = "loc_rooftop_watch",
                Synopsis =
                    "Overseer Halden has held the district's rooftops since the Veil went up, shooting anything " +
                    "that moves between the towers. The climb is exposed, the snipers are patient, and Halden " +
                    "keeps a paper ledger of every shot he has ever taken.",
                BossId = "boss_halden",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(3),
                RecommendedLevel = ChapterRules.RecommendedLevel(3),
                MinimumLevel = ChapterRules.MinimumLevel(3),
                UnlockFlagOnComplete = "flag_ch03_complete"
            };
            c3.MiniBossIds.AddRange(new[] { "mb_signal_thief", "mb_dust_devil" });
            c3.SideQuestIds.AddRange(new[] { "q03_long_watch_down", "q03_signal_echoes", "q03_snipers_ledger" });
            c3.NewEnemyIds.AddRange(new[] { "e_longwatch", "e_bulwark_trooper" });
            c3.ChapterRewardIds.AddRange(new[] { "g_trinket_sprinter" });

            c3.Arenas.Add(Arena("arena_ch03_street", 3, "Watch Street", 46f, 16f, false, "rooftops_night",
                new[] { "hz_barbed_wire", "hz_collapsing_walkway" }, StandardSpawnTags(),
                new[] { "prop_barrier", "prop_antenna", "prop_crate" }, "mx_ch03_street"));
            c3.Arenas.Add(Arena("arena_ch03_roofs", 3, "Lower Rooftops", 52f, 19f, false, "rooftops_night",
                new[] { "hz_open_air", "hz_antenna_array" }, StandardSpawnTags(),
                new[] { "prop_antenna", "prop_vent", "prop_water_tank" }, "mx_ch03_roofs"));
            c3.Arenas.Add(Arena("arena_ch03_mb1", 3, "Relay Tower", 34f, 15f, false, "rooftops_night",
                new[] { "hz_antenna_array" }, StandardSpawnTags(),
                new[] { "prop_antenna", "prop_crate" }, "mx_miniboss"));
            c3.Arenas.Add(Arena("arena_ch03_mb2", 3, "Gap Crossing", 30f, 13f, false, "rooftops_night",
                new[] { "hz_open_air" }, StandardSpawnTags(),
                new[] { "prop_vent", "prop_water_tank" }, "mx_miniboss"));
            c3.Arenas.Add(Arena("arena_ch03_boss", 3, "Watch Command Roof", 50f, 18f, true, "rooftops_night",
                new[] { "hz_open_air", "hz_antenna_array", "hz_floodlight" }, StandardSpawnTags(),
                new[] { "prop_antenna", "prop_vent", "prop_hardcase" }, "mx_boss_watch"));

            c3.Savepoints.Add(Savepoint("sp03_street", 3, "ch03_beat_save_a", "Watch street - safe house", "arena_ch03_street", false));
            c3.Savepoints.Add(Savepoint("sp03_preboss", 3, "ch03_beat_save_b", "Stairwell to command roof", "arena_ch03_boss", true));

            c3.Checkpoints.Add(Checkpoint("chk03_01", 3, "ch03_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c3.Checkpoints.Add(Checkpoint("chk03_02", 3, "ch03_beat_roofs", "Rooftops reached", CheckpointTrigger.EnterArena));
            c3.Checkpoints.Add(Checkpoint("chk03_03", 3, "ch03_beat_save_b", "Before Halden", CheckpointTrigger.PreBoss));
            c3.Checkpoints.Add(Checkpoint("chk03_04", 3, "ch03_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c3.Beats.Add(Beat(1, BeatKind.Comic, "ch03_beat_comic_intro", "Sightlines", "Watch the opening page", "cp03_intro"));
            c3.Beats.Add(Beat(2, BeatKind.Exploration, "ch03_beat_street", "Watch street", "Cross the open street", "arena_ch03_street"));
            c3.Beats.Add(Beat(3, BeatKind.Arena, "ch03_beat_enc_street", "Street patrol", "Clear the street patrol", "enc_ch03_street"));
            c3.Beats.Add(Beat(4, BeatKind.Savepoint, "ch03_beat_save_a", "Safe house", "Save at the safe house", "sp03_street"));
            c3.Beats.Add(Beat(5, BeatKind.MiniBossFight, "ch03_beat_mb1", "Signal thief", "Deal with the Signal Thief", "mb_signal_thief"));
            c3.Beats.Add(Beat(6, BeatKind.Comic, "ch03_beat_comic_story", "The ledger", "Watch the mid-chapter page", "cp03_story"));
            c3.Beats.Add(Beat(7, BeatKind.Arena, "ch03_beat_roofs", "Lower rooftops", "Cross the rooftops", "enc_ch03_roofs"));
            c3.Beats.Add(Beat(8, BeatKind.MiniBossFight, "ch03_beat_mb2", "Gap crossing", "Deal with Dust Devil", "mb_dust_devil", optional: true));
            c3.Beats.Add(Beat(9, BeatKind.Savepoint, "ch03_beat_save_b", "Stairwell", "Save before the climb", "sp03_preboss"));
            c3.Beats.Add(Beat(10, BeatKind.Comic, "ch03_beat_comic_boss", "Long watch", "Watch the boss intro page", "cp03_boss"));
            c3.Beats.Add(Beat(11, BeatKind.BossFight, "ch03_beat_boss", "Overseer Halden", "Defeat Overseer Halden", "boss_halden"));
            c3.Beats.Add(Beat(12, BeatKind.Comic, "ch03_beat_comic_outro", "The last round", "Watch the closing page", "cp03_outro"));
            c3.Beats.Add(Beat(13, BeatKind.ChapterComplete, "ch03_beat_complete", "Chapter complete", "Autosave and continue", "flag_ch03_complete"));

            c3.ComicPages.Add(Page("cp03_intro", 3, 1, "ch03_beat_comic_intro", "Sightlines",
                new[]
                {
                    Narration("cp03_intro_p1", ComicShotType.Establish, "rooftops_night",
                        "The Watch district was built tall so the Directorate could see everything. Halden never left.",
                        null, "zoom"),
                    Speech("cp03_intro_p2", ComicShotType.Medium, "rooftops_night", "Vera",
                        "Assume we are already in his scope. Everything from here is a sprint between cover.",
                        new[] { A("vera", 0.5f, "crouch", "focused", 1.05f) }),
                    Speech("cp03_intro_p3", ComicShotType.CloseUp, "rooftops_night", "Kade",
                        "Assume nothing. He has been shooting this district for a year. He knows the gaps better than we do.",
                        new[] { A("kade", 0.5f, "idle", "grim", 1.2f) }),
                    Speech("cp03_intro_p4", ComicShotType.OverShoulder, "rooftops_night", "Bek",
                        "He took four of mine off that roof. You bring him down, I will carry the wounded anywhere you need.",
                        new[] { A("bek", 0.55f, "idle", "hard", 1.05f) }),
                    Speech("cp03_intro_p5", ComicShotType.Wide, "rooftops_night", "Vera",
                        "Then keep your people indoors. We are going up.",
                        new[] { A("vera", 0.35f, "walk", "resolute", 0.95f), A("kade", 0.7f, "walk", "grim", 0.95f) }),
                    Narration("cp03_intro_p6", ComicShotType.Insert, "rooftops_night",
                        "On the highest roof, a man writes a single line in a ledger and closes it.", null, "speedlines")
                }, "mx_ch03_street"));

            c3.ComicPages.Add(Page("cp03_story", 3, 2, "ch03_beat_comic_story", "The Ledger",
                new[]
                {
                    Narration("cp03_story_p1", ComicShotType.CloseUp, "rooftops_night",
                        "The ledger is a list of names, distances, wind speeds and dates. Meridian is on page one.",
                        null, "impact"),
                    Speech("cp03_story_p2", ComicShotType.Medium, "rooftops_night", "Kade",
                        "Meridian, nine hundred and forty metres. He wrote it down like a weather report.",
                        new[] { A("kade", 0.5f, "kneel", "furious", 1.15f) }),
                    Speech("cp03_story_p3", ComicShotType.OverShoulder, "rooftops_night", "Vera",
                        "He signed the order and then he shot the survivors. Take the pages. Both are evidence.",
                        new[] { A("vera", 0.65f, "idle", "cold", 1f, flip: true), A("kade", 0.33f, "kneel", "furious", 1f) }),
                    Speech("cp03_story_p4", ComicShotType.CloseUp, "rooftops_night", "Kade",
                        "I do not need evidence. I need him.",
                        new[] { A("kade", 0.5f, "idle", "hard", 1.25f) }),
                    Speech("cp03_story_p5", ComicShotType.Wide, "rooftops_night", "Vera",
                        "You need both. Burn the Veil with his name still in the record. Otherwise somebody rebuilds it.",
                        new[] { A("vera", 0.5f, "idle", "resolute", 1.15f) })
                }));

            c3.ComicPages.Add(Page("cp03_boss", 3, 3, "ch03_beat_comic_boss", "Long Watch",
                new[]
                {
                    Narration("cp03_boss_p1", ComicShotType.Establish, "rooftops_night",
                        "Watch command roof. A hardcase of ammunition, a rail rifle, and a man who has not missed since Meridian.",
                        null, "impact"),
                    Speech("cp03_boss_p2", ComicShotType.Wide, "rooftops_night", "Halden",
                        "I know your name. I have a line on you already. Distance, wind, elevation.",
                        new[] { A("halden", 0.5f, "aim", "cold", 1.4f, silhouette: true) }),
                    Speech("cp03_boss_p3", ComicShotType.Medium, "rooftops_night", "Kade",
                        "Then write down today.",
                        new[] { A("kade", 0.3f, "aim", "hard", 1.1f) }),
                    Speech("cp03_boss_p4", ComicShotType.CloseUp, "rooftops_night", "Halden",
                        "I always do. It is the only honest record left on this coast.",
                        new[] { A("halden", 0.5f, "aim", "calm", 1.45f) }),
                    Narration("cp03_boss_p5", ComicShotType.Insert, "rooftops_night",
                        "The rifle bolt cycles. The fight starts at nine hundred metres and closes fast.", null, "shake", "CRACK")
                }, "mx_boss_watch", "boss"));

            c3.ComicPages.Add(Page("cp03_outro", 3, 4, "ch03_beat_comic_outro", "The Last Round",
                new[]
                {
                    Narration("cp03_outro_p1", ComicShotType.Wide, "rooftops_night",
                        "Halden sits against the parapet with the ledger in his lap and nothing left in the rifle.",
                        new[] { A("halden", 0.5f, "sit", "broken", 1.15f) }, "impact"),
                    Speech("cp03_outro_p2", ComicShotType.Medium, "rooftops_night", "Halden",
                        "Foundry. The Veil's real work is in the Foundry, and the order you want is in the machine there.",
                        new[] { A("halden", 0.5f, "sit", "broken", 1.1f) }),
                    Speech("cp03_outro_p3", ComicShotType.OverShoulder, "rooftops_night", "Kade",
                        "Why would you tell me?",
                        new[] { A("kade", 0.33f, "idle", "flat", 1f), A("halden", 0.68f, "sit", "broken", 1f) }),
                    Speech("cp03_outro_p4", ComicShotType.CloseUp, "rooftops_night", "Halden",
                        "Because I wrote Meridian down, and I would like the record to stop somewhere.",
                        new[] { A("halden", 0.5f, "sit", "soft", 1.3f) }),
                    Narration("cp03_outro_p5", ComicShotType.Establish, "rooftops_night",
                        "Dawn comes up orange over the Foundry stacks. Somewhere in there, the Veil is being written.",
                        null, "zoom")
                }));
            content.Chapters.Add(c3);

            /* ============================================================
               CHAPTER 4 - THE FOUNDRY      (120 min, level 18)
               ============================================================ */
            ChapterDefinition c4 = new ChapterDefinition
            {
                Index = 4,
                Title = "The Foundry",
                Subtitle = "Where the Veil is cast",
                ActTitle = ChapterRules.ActTitle(4),
                LocationName = "Helion Foundry Prime",
                LocationId = "loc_foundry",
                Synopsis =
                    "Behind the coast, the Foundry melts memory into Veil alloy and bolts it onto soldiers. " +
                    "Foundry Prime is the line that never stops, and the workers still welded into it are the " +
                    "first people Kade has to look at without looking away.",
                BossId = "boss_foundry",
                EstimatedMinutes = ChapterRules.EstimatedMinutes(4),
                RecommendedLevel = ChapterRules.RecommendedLevel(4),
                MinimumLevel = ChapterRules.MinimumLevel(4),
                UnlockFlagOnComplete = "flag_ch04_complete"
            };
            c4.MiniBossIds.AddRange(new[] { "mb_rangefinder", "mb_lockjaw" });
            c4.SideQuestIds.AddRange(new[] { "q04_line_halt", "q04_molten_haul", "q04_foundry_whistle" });
            c4.NewEnemyIds.AddRange(new[] { "e_toxin_crawler", "e_ironjaw_brute" });
            c4.ChapterRewardIds.AddRange(new[] { "g_core_reactive" });

            c4.Arenas.Add(Arena("arena_ch04_yard", 4, "Foundry Yard", 48f, 17f, false, "foundry_glow",
                new[] { "hz_molten_pour", "hz_crusher" }, StandardSpawnTags(),
                new[] { "prop_slag_pile", "prop_press", "prop_container" }, "mx_ch04_yard"));
            c4.Arenas.Add(Arena("arena_ch04_floor", 4, "Casting Floor", 54f, 19f, false, "foundry_glow",
                new[] { "hz_molten_pour", "hz_steam_vent", "hz_crusher" }, StandardSpawnTags(),
                new[] { "prop_press", "prop_conveyor", "prop_slag_pile" }, "mx_ch04_floor"));
            c4.Arenas.Add(Arena("arena_ch04_mb1", 4, "Sighting Gallery", 36f, 15f, false, "foundry_glow",
                new[] { "hz_molten_pour" }, StandardSpawnTags(),
                new[] { "prop_press", "prop_container" }, "mx_miniboss"));
            c4.Arenas.Add(Arena("arena_ch04_mb2", 4, "Press Hall", 34f, 15f, false, "foundry_glow",
                new[] { "hz_crusher", "hz_steam_vent" }, StandardSpawnTags(),
                new[] { "prop_press", "prop_slag_pile" }, "mx_miniboss"));
            c4.Arenas.Add(Arena("arena_ch04_boss", 4, "Assembly Line One", 58f, 20f, true, "foundry_glow",
                new[] { "hz_molten_pour", "hz_crusher", "hz_steam_vent", "hz_laser_grid" }, StandardSpawnTags(),
                new[] { "prop_press", "prop_conveyor", "prop_crane", "prop_slag_pile" }, "mx_boss_foundry"));

            c4.Savepoints.Add(Savepoint("sp04_yard", 4, "ch04_beat_save_a", "Foundry yard - worker lockers", "arena_ch04_yard", false));
            c4.Savepoints.Add(Savepoint("sp04_preboss", 4, "ch04_beat_save_b", "Line one - control booth", "arena_ch04_boss", true));

            c4.Checkpoints.Add(Checkpoint("chk04_01", 4, "ch04_beat_comic_intro", "Chapter start", CheckpointTrigger.ChapterStart));
            c4.Checkpoints.Add(Checkpoint("chk04_02", 4, "ch04_beat_floor", "Casting floor cleared", CheckpointTrigger.EnterArena));
            c4.Checkpoints.Add(Checkpoint("chk04_03", 4, "ch04_beat_save_b", "Before Foundry Prime", CheckpointTrigger.PreBoss));
            c4.Checkpoints.Add(Checkpoint("chk04_04", 4, "ch04_beat_complete", "Chapter complete", CheckpointTrigger.ChapterComplete));

            c4.Beats.Add(Beat(1, BeatKind.Comic, "ch04_beat_comic_intro", "Shift change", "Watch the opening page", "cp04_intro"));
            c4.Beats.Add(Beat(2, BeatKind.Exploration, "ch04_beat_yard", "Foundry yard", "Cross the yard", "arena_ch04_yard"));
            c4.Beats.Add(Beat(3, BeatKind.Arena, "ch04_beat_enc_yard", "Yard security", "Clear yard security", "enc_ch04_yard"));
            c4.Beats.Add(Beat(4, BeatKind.Savepoint, "ch04_beat_save_a", "Worker lockers", "Save at the lockers", "sp04_yard"));
            c4.Beats.Add(Beat(5, BeatKind.MiniBossFight, "ch04_beat_mb1", "Sighting gallery", "Deal with the Rangefinder", "mb_rangefinder"));
            c4.Beats.Add(Beat(6, BeatKind.Comic, "ch04_beat_comic_story", "The line", "Watch the mid-chapter page", "cp04_story"));
            c4.Beats.Add(Beat(7, BeatKind.Arena, "ch04_beat_floor", "Casting floor", "Clear the casting floor", "enc_ch04_floor"));
            c4.Beats.Add(Beat(8, BeatKind.MiniBossFight, "ch04_beat_mb2", "Press hall", "Deal with Lockjaw", "mb_lockjaw", optional: true));
            c4.Beats.Add(Beat(9, BeatKind.Savepoint, "ch04_beat_save_b", "Control booth", "Save before the line", "sp04_preboss"));
            c4.Beats.Add(Beat(10, BeatKind.Comic, "ch04_beat_comic_boss", "Assembly line one", "Watch the boss intro page", "cp04_boss"));
            c4.Beats.Add(Beat(11, BeatKind.BossFight, "ch04_beat_boss", "Foundry Prime", "Defeat Foundry Prime", "boss_foundry"));
            c4.Beats.Add(Beat(12, BeatKind.Comic, "ch04_beat_comic_outro", "Line stopped", "Watch the closing page", "cp04_outro"));
            c4.Beats.Add(Beat(13, BeatKind.ChapterComplete, "ch04_beat_complete", "Chapter complete", "Autosave and continue", "flag_ch04_complete"));

            c4.ComicPages.Add(Page("cp04_intro", 4, 1, "ch04_beat_comic_intro", "Shift Change",
                new[]
                {
                    Narration("cp04_intro_p1", ComicShotType.Establish, "foundry_glow",
                        "The Foundry runs three shifts and the third shift has not gone home in a year.",
                        null, "zoom"),
                    Speech("cp04_intro_p2", ComicShotType.Medium, "foundry_glow", "Ulla",
                        "My crew is in that line. Not near it. In it. Wired into the presses they used to run.",
                        new[] { A("ulla", 0.5f, "idle", "grieving", 1.1f) }),
                    Speech("cp04_intro_p3", ComicShotType.OverShoulder, "foundry_glow", "Kade",
                        "Then we break the line.",
                        new[] { A("kade", 0.32f, "idle", "grim", 1f), A("ulla", 0.65f, "idle", "grieving", 1f, flip: true) }),
                    Speech("cp04_intro_p4", ComicShotType.CloseUp, "foundry_glow", "Ulla",
                        "Break it gently. Some of them are still awake in there.",
                        new[] { A("ulla", 0.5f, "idle", "pleading", 1.25f) }),
                    Narration("cp04_intro_p5", ComicShotType.Wide, "foundry_glow",
                        "Foundry Prime stands over the line like a foreman who never sleeps, because it never has.",
                        new[] { A("foundry", 0.5f, "idle", "machine", 1.5f, silhouette: true) }, "impact")
                }, "mx_ch04_yard"));

            c4.ComicPages.Add(Page("cp04_story", 4, 2, "ch04_beat_comic_story", "The Line",
                new[]
                {
                    Narration("cp04_story_p1", ComicShotType.Insert, "foundry_glow",
                        "Veil alloy is poured into a mould the shape of a person. The mould is a person.",
                        null, "flash"),
                    Speech("cp04_story_p2", ComicShotType.Medium, "foundry_glow", "Vera",
                        "This is not armour. This is a replacement. They are not plating soldiers over, they are pouring them in.",
                        new[] { A("vera", 0.5f, "idle", "horrified", 1.15f) }),
                    Speech("cp04_story_p3", ComicShotType.CloseUp, "foundry_glow", "Kade",
                        "Meridian. This is what they did with the names. They built the Veil out of them.",
                        new[] { A("kade", 0.5f, "idle", "furious", 1.2f) }),
                    Speech("cp04_story_p4", ComicShotType.OverShoulder, "foundry_glow", "Vera",
                        "One more reason to take the machine down instead of the people. Both are on our side of this.",
                        new[] { A("vera", 0.66f, "idle", "resolute", 1f, flip: true), A("kade", 0.33f, "idle", "furious", 1f) }),
                    Narration("cp04_story_p5", ComicShotType.Establish, "foundry_glow",
                        "The line restarts behind them. It has already replaced the crew they just carried out.", null, "shake")
                }));

            c4.ComicPages.Add(Page("cp04_boss", 4, 3, "ch04_beat_comic_boss", "Assembly Line One",
                new[]
                {
                    Narration("cp04_boss_p1", ComicShotType.Establish, "foundry_glow",
                        "Line one is six hundred metres of press, conveyor and memory furnace. It is also the boss.",
                        null, "impact"),
                    Speech("cp04_boss_p2", ComicShotType.Wide, "foundry_glow", "Foundry Prime",
                        "PRODUCTION MUST CONTINUE.",
                        new[] { A("foundry", 0.5f, "idle", "machine", 1.6f) }, effect: "shake"),
                    Speech("cp04_boss_p3", ComicShotType.Medium, "foundry_glow", "Kade",
                        "Not today it does not.",
                        new[] { A("kade", 0.33f, "aim", "hard", 1.1f) }),
                    Speech("cp04_boss_p4", ComicShotType.Insert, "foundry_glow", "Vera",
                        "Three couplings under the gantry. Hit those and the line loses its mind.",
                        new[] { A("vera", 0.5f, "point", "focused", 1.05f) }),
                    Narration("cp04_boss_p5", ComicShotType.Wide, "foundry_glow",
                        "The pistons come down. The line is trying to schedule them into the process.",
                        null, "speedlines", "WHAM")
                }, "mx_boss_foundry", "boss"));

            c4.ComicPages.Add(Page("cp04_outro", 4, 4, "ch04_beat_comic_outro", "Line Stopped",
                new[]
                {
                    Narration("cp04_outro_p1", ComicShotType.Wide, "foundry_glow",
                        "Foundry Prime settles into its own slag. For the first time in a year, the shift bell rings for nobody.",
                        null, "impact"),
                    Speech("cp04_outro_p2", ComicShotType.Medium, "foundry_glow", "Ulla",
                        "Thank you. I mean it. Now get out of my foundry before the Veil notices.",
                        new[] { A("ulla", 0.5f, "idle", "tired", 1.05f) }),
                    Speech("cp04_outro_p3", ComicShotType.OverShoulder, "foundry_glow", "Vera",
                        "It noticed the moment we cut the first coupling. There is an opera house on the map with no roads to it.",
                        new[] { A("vera", 0.6f, "idle", "focused", 1f, flip: true), A("kade", 0.32f, "idle", "grim", 0.95f) }),
                    Speech("cp04_outro_p4", ComicShotType.CloseUp, "foundry_glow", "Kade",
                        "Then we walk. Every Veil we break makes the next one louder.",
                        new[] { A("kade", 0.5f, "idle", "set", 1.2f) }),
                    Narration("cp04_outro_p5", ComicShotType.Establish, "foundry_glow",
                        "Outside, the coast road is dead quiet. Somewhere ahead, something is singing.", null, "zoom")
                }));
            content.Chapters.Add(c4);
        }
    }
}
