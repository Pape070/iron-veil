namespace IronVeil.Rules
{
    public static partial class Campaign
    {
        private static void BuildEncounters(CampaignContent content)
        {
            // ---- Chapter 1 ----
            content.Encounters.Add(Encounter("enc_ch01_gate", 1, "Checkpoint Picket", "arena_ch01_gate", 70,
                new[]
                {
                    Wave("Advance", 0f, true, new[] { S("e_line_trooper", 3), S("e_rust_drone", 4, "air") }),
                    Wave("Second line", 6f, true, new[] { S("e_line_trooper", 4), S("e_breach_hound", 3, "spawn_right") })
                },
                "Clear the checkpoint picket", "dt_elite_early", music: "mx_combat_iron"));
            content.Encounters.Add(Encounter("enc_ch01_yard", 1, "Scrap Yard Sweep", "arena_ch01_yard", 90,
                new[]
                {
                    Wave("Yard crew", 0f, true, new[] { S("e_line_trooper", 4), S("e_rust_drone", 5, "air", 0.4f) }),
                    Wave("Hounds", 8f, true, new[] { S("e_breach_hound", 5, "spawn_left", 0.5f) }),
                    Wave("Hold the yard", 12f, true, new[] { S("e_line_trooper", 5), S("e_breach_hound", 3, "spawn_right") })
                },
                "Hold the scrap yard", "dt_elite_early", music: "mx_combat_iron"));
            content.Encounters.Add(Encounter("enc_ch01_patrol", 1, "Road Patrol", "arena_ch01_gate", 55,
                new[]
                {
                    Wave("Patrol", 0f, true, new[] { S("e_line_trooper", 3), S("e_breach_hound", 2) })
                },
                "Clear the road patrol", "dt_elite_early"));

            // ---- Chapter 2 ----
            content.Encounters.Add(Encounter("enc_ch02_pier", 2, "Pier Turret Line", "arena_ch02_pier", 85,
                new[]
                {
                    Wave("Turret line", 0f, true, new[] { S("e_sentinel_turret", 2, "ledge"), S("e_line_trooper", 4) }),
                    Wave("Skiffs", 8f, true, new[] { S("e_skiff", 3, "air", 1.2f), S("e_line_trooper", 3, "ledge") })
                },
                "Break the pier defences", "dt_elite_early", music: "mx_combat_tide"));
            content.Encounters.Add(Encounter("enc_ch02_hold", 2, "Flooded Hold", "arena_ch02_hold", 95,
                new[]
                {
                    Wave("Hold crew", 0f, true, new[] { S("e_line_trooper", 4, "ground"), S("e_sentinel_turret", 2, "ledge") }),
                    Wave("Divers", 10f, true, new[] { S("e_skiff", 4, "air", 1f) }),
                    Wave("Last of the crew", 10f, true, new[] { S("e_line_trooper", 5) })
                },
                "Clear the flooded hold", "dt_arena_chest", music: "mx_combat_tide"));
            content.Encounters.Add(Encounter("enc_ch02_basin", 2, "Basin Sweep", "arena_ch02_pier", 60,
                new[]
                {
                    Wave("Basin patrol", 0f, true, new[] { S("e_sentinel_turret", 3, "ledge"), S("e_skiff", 2, "air") })
                },
                "Clear the basin", "dt_elite_early"));

            // ---- Chapter 3 ----
            content.Encounters.Add(Encounter("enc_ch03_street", 3, "Watch Street Ambush", "arena_ch03_street", 80,
                new[]
                {
                    Wave("Street patrol", 0f, true, new[] { S("e_bulwark_trooper", 3), S("e_line_trooper", 3, "ledge") }),
                    Wave("Marksmen", 8f, true, new[] { S("e_longwatch", 2, "ledge", 1.5f), S("e_breach_hound", 3, "spawn_left") })
                },
                "Cross the street alive", "dt_elite_mid", music: "mx_combat_night"));
            content.Encounters.Add(Encounter("enc_ch03_roofs", 3, "Rooftop Crossing", "arena_ch03_roofs", 100,
                new[]
                {
                    Wave("Roof guard", 0f, true, new[] { S("e_bulwark_trooper", 3), S("e_longwatch", 2, "ledge") }),
                    Wave("Air wing", 8f, true, new[] { S("e_skiff", 4, "air", 1f) }),
                    Wave("Final line", 10f, true, new[] { S("e_line_trooper", 4), S("e_longwatch", 2, "ledge", 1.4f) })
                },
                "Cross the rooftops", "dt_arena_chest", music: "mx_combat_night"));
            content.Encounters.Add(Encounter("enc_ch03_alley", 3, "Alley Picket", "arena_ch03_street", 55,
                new[]
                {
                    Wave("Picket", 0f, true, new[] { S("e_bulwark_trooper", 3), S("e_breach_hound", 3) })
                },
                "Clear the alley", "dt_elite_mid"));

            // ---- Chapter 4 ----
            content.Encounters.Add(Encounter("enc_ch04_yard", 4, "Foundry Yard Security", "arena_ch04_yard", 90,
                new[]
                {
                    Wave("Yard security", 0f, true, new[] { S("e_sentinel_turret", 2, "ledge", 1.4f), S("e_line_trooper", 4) }),
                    Wave("Brute squad", 10f, true, new[] { S("e_ironjaw_brute", 2, "spawn_right", 1.6f) })
                },
                "Break yard security", "dt_elite_mid", music: "mx_combat_foundry"));
            content.Encounters.Add(Encounter("enc_ch04_floor", 4, "Casting Floor Push", "arena_ch04_floor", 110,
                new[]
                {
                    Wave("Line crew", 0f, true, new[] { S("e_line_trooper", 5), S("e_sentinel_turret", 2, "ledge") }),
                    Wave("Crawlers", 8f, true, new[] { S("e_toxin_crawler", 4, "spawn_left", 0.8f) }),
                    Wave("Brutes", 10f, true, new[] { S("e_ironjaw_brute", 3, "spawn_right", 1.4f) })
                },
                "Clear the casting floor", "dt_arena_chest", music: "mx_combat_foundry"));
            content.Encounters.Add(Encounter("enc_ch04_slag", 4, "Slag Run", "arena_ch04_yard", 60,
                new[]
                {
                    Wave("Slag crew", 0f, true, new[] { S("e_toxin_crawler", 5, "ground", 0.6f), S("e_ironjaw_brute", 1) })
                },
                "Cross the slag run", "dt_elite_mid"));

            // ---- Chapter 5 ----
            content.Encounters.Add(Encounter("enc_ch05_foyer", 5, "Foyer Picket", "arena_ch05_foyer", 90,
                new[]
                {
                    Wave("Foyer guard", 0f, true, new[] { S("e_line_trooper", 5), S("e_mortar_team", 2, "ledge", 1.4f) }),
                    Wave("Chaplains", 10f, true, new[] { S("e_field_chaplain", 2, "ledge", 2f), S("e_ironjaw_brute", 2, "spawn_left") })
                },
                "Clear the foyer", "dt_elite_mid", music: "mx_combat_opera"));
            content.Encounters.Add(Encounter("enc_ch05_stalls", 5, "Stalls and Pit", "arena_ch05_stalls", 110,
                new[]
                {
                    Wave("Pit crowd", 0f, true, new[] { S("e_line_trooper", 5), S("e_bulwark_trooper", 3) }),
                    Wave("Choir", 8f, true, new[] { S("e_field_chaplain", 3, "ledge", 1.8f) }),
                    Wave("Mortars", 10f, true, new[] { S("e_mortar_team", 3, "ledge", 1.4f), S("e_breach_hound", 4, "spawn_right") })
                },
                "Hold the stalls", "dt_arena_chest", music: "mx_combat_opera"));
            content.Encounters.Add(Encounter("enc_ch05_wings", 5, "Stage Wings", "arena_ch05_foyer", 60,
                new[]
                {
                    Wave("Wings guard", 0f, true, new[] { S("e_field_chaplain", 2, "ledge", 1.8f), S("e_mortar_team", 2, "ledge") })
                },
                "Clear the wings", "dt_elite_mid"));

            // ---- Chapter 6 ----
            content.Encounters.Add(Encounter("enc_ch06_nave", 6, "Nave Swarm", "arena_ch06_nave", 95,
                new[]
                {
                    Wave("Nave guard", 0f, true, new[] { S("e_line_trooper", 4), S("e_memory_mites", 12, "air", 0.25f) }),
                    Wave("Mite surge", 8f, true, new[] { S("e_memory_mites", 20, "air", 0.2f) }),
                    Wave("Chaplains", 10f, true, new[] { S("e_field_chaplain", 3, "ledge", 1.6f), S("e_memory_mites", 10, "air", 0.25f) })
                },
                "Survive the nave", "dt_elite_mid", music: "mx_combat_choir"));
            content.Encounters.Add(Encounter("enc_ch06_crypt", 6, "Crypt Descent", "arena_ch06_crypt", 105,
                new[]
                {
                    Wave("Crypt guard", 0f, true, new[] { S("e_toxin_crawler", 5, "ground", 0.6f), S("e_memory_mites", 14, "air", 0.25f) }),
                    Wave("Brutes", 10f, true, new[] { S("e_ironjaw_brute", 3, "spawn_left", 1.3f) }),
                    Wave("Swarm", 10f, true, new[] { S("e_memory_mites", 22, "air", 0.18f) })
                },
                "Reach the crypt", "dt_arena_chest", music: "mx_combat_choir"));
            content.Encounters.Add(Encounter("enc_ch06_cloister", 6, "Cloister Escort", "arena_ch06_nave", 70,
                new[]
                {
                    Wave("Cloister pack", 0f, true, new[] { S("e_memory_mites", 18, "air", 0.2f), S("e_toxin_crawler", 4) }),
                    Wave("Reinforcements", 8f, true, new[] { S("e_line_trooper", 5) })
                },
                "Clear the cloister", "dt_elite_mid"));

            // ---- Chapter 7 ----
            content.Encounters.Add(Encounter("enc_ch07_atrium", 7, "Atrium Ambush", "arena_ch07_atrium", 90,
                new[]
                {
                    Wave("Stalkers", 0f, true, new[] { S("e_veil_stalker", 3, "ground"), S("e_line_trooper", 4, "ledge") }),
                    Wave("Second wave", 8f, true, new[] { S("e_veil_stalker", 4, "spawn_right", 0.9f) })
                },
                "Clear the atrium", "dt_elite_late", music: "mx_combat_glass"));
            content.Encounters.Add(Encounter("enc_ch07_hall", 7, "Mirror Hall Run", "arena_ch07_hall", 110,
                new[]
                {
                    Wave("Hall guard", 0f, true, new[] { S("e_veil_stalker", 4, "ground"), S("e_longwatch", 3, "ledge", 1.3f) }),
                    Wave("Cloak wave", 8f, true, new[] { S("e_veil_stalker", 6, "spawn_left", 0.8f) }),
                    Wave("Support", 10f, true, new[] { S("e_field_chaplain", 3, "ledge", 1.6f) })
                },
                "Cross the mirror hall", "dt_arena_chest", music: "mx_combat_glass"));
            content.Encounters.Add(Encounter("enc_ch07_gallery", 7, "Reflection Gallery", "arena_ch07_atrium", 65,
                new[]
                {
                    Wave("Gallery guard", 0f, true, new[] { S("e_veil_stalker", 5, "ground", 0.9f) })
                },
                "Clear the gallery", "dt_elite_late"));

            // ---- Chapter 8 ----
            content.Encounters.Add(Encounter("enc_ch08_maze", 8, "Pack in the Maze", "arena_ch08_container", 100,
                new[]
                {
                    Wave("Pack", 0f, true, new[] { S("e_hollow_hound", 6, "ground", 0.5f), S("e_line_trooper", 4, "ledge") }),
                    Wave("Second pack", 8f, true, new[] { S("e_hollow_hound", 8, "spawn_left", 0.4f) }),
                    Wave("Siege support", 10f, true, new[] { S("e_siege_walker", 2, "ledge", 2.5f) })
                },
                "Clear the maze", "dt_elite_late", music: "mx_combat_yard"));
            content.Encounters.Add(Encounter("enc_ch08_gantry", 8, "Gantry Assault", "arena_ch08_gantry", 115,
                new[]
                {
                    Wave("Gantry guard", 0f, true, new[] { S("e_siege_walker", 2, "ledge", 2.5f), S("e_hollow_hound", 5, "ground") }),
                    Wave("Mortars", 10f, true, new[] { S("e_mortar_team", 3, "ledge", 1.3f) }),
                    Wave("Pack", 10f, true, new[] { S("e_hollow_hound", 9, "spawn_right", 0.4f) })
                },
                "Hold the gantry", "dt_arena_chest", music: "mx_combat_yard"));
            content.Encounters.Add(Encounter("enc_ch08_floor", 8, "Feeding Floor Approach", "arena_ch08_container", 70,
                new[]
                {
                    Wave("Yard guard", 0f, true, new[] { S("e_hollow_hound", 6, "ground", 0.5f), S("e_veil_stalker", 2, "spawn_right") })
                },
                "Reach the feeding floor", "dt_elite_late"));

            // ---- Chapter 9 ----
            content.Encounters.Add(Encounter("enc_ch09_stair", 9, "Stair Crawlers", "arena_ch09_stair", 90,
                new[]
                {
                    Wave("Crawler nest", 0f, true, new[] { S("e_toxin_crawler", 6, "ground", 0.6f), S("e_echo_wisp", 4, "air", 1f) }),
                    Wave("Wisps", 8f, true, new[] { S("e_echo_wisp", 7, "air", 0.8f) })
                },
                "Clear the stair", "dt_elite_late", music: "mx_combat_runoff"));
            content.Encounters.Add(Encounter("enc_ch09_cistern", 9, "Lower Cistern", "arena_ch09_cistern", 110,
                new[]
                {
                    Wave("Cistern guard", 0f, true, new[] { S("e_toxin_crawler", 7, "ground", 0.5f), S("e_hollow_hound", 5, "spawn_left") }),
                    Wave("Echoes", 9f, true, new[] { S("e_echo_wisp", 8, "air", 0.8f) }),
                    Wave("Brutes", 10f, true, new[] { S("e_ironjaw_brute", 4, "spawn_right", 1.2f) })
                },
                "Clear the lower cistern", "dt_arena_chest", music: "mx_combat_runoff"));
            content.Encounters.Add(Encounter("enc_ch09_valves", 9, "Valve Run", "arena_ch09_cistern", 65,
                new[]
                {
                    Wave("Valve guard", 0f, true, new[] { S("e_toxin_crawler", 6, "ground", 0.6f) })
                },
                "Seal the valves", "dt_elite_late"));

            // ---- Chapter 10 ----
            content.Encounters.Add(Encounter("enc_ch10_gallery", 10, "Gallery Guard", "arena_ch10_gallery", 100,
                new[]
                {
                    Wave("Veilguard patrol", 0f, true, new[] { S("e_veilguard", 3, "ground"), S("e_line_trooper", 5, "ledge") }),
                    Wave("Second patrol", 10f, true, new[] { S("e_veilguard", 4, "spawn_left", 1.2f) })
                },
                "Break the gallery guard", "dt_elite_late", music: "mx_combat_ossuary"));
            content.Encounters.Add(Encounter("enc_ch10_bridge", 10, "Veil Bridge", "arena_ch10_bridge", 115,
                new[]
                {
                    Wave("Bridge guard", 0f, true, new[] { S("e_veilguard", 4, "ground", 1.2f), S("e_longwatch", 3, "ledge", 1.2f) }),
                    Wave("Nodes", 9f, true, new[] { S("e_veil_engine", 2, "ledge", 2.4f) }),
                    Wave("Last line", 10f, true, new[] { S("e_veilguard", 5, "spawn_right", 1.1f) })
                },
                "Cross the veil bridge", "dt_arena_chest", music: "mx_combat_ossuary"));
            content.Encounters.Add(Encounter("enc_ch10_cells", 10, "Cell Block", "arena_ch10_gallery", 70,
                new[]
                {
                    Wave("Cell guard", 0f, true, new[] { S("e_veilguard", 3, "ground", 1.2f), S("e_field_chaplain", 2, "ledge", 1.6f) })
                },
                "Clear the cell block", "dt_elite_late"));

            // ---- Chapter 11 ----
            content.Encounters.Add(Encounter("enc_ch11_plaza", 11, "Plaza Garrison", "arena_ch11_plaza", 105,
                new[]
                {
                    Wave("Garrison", 0f, true, new[] { S("e_line_trooper", 8, "ground", 0.4f), S("e_signal_conductor", 1, "ledge", 2f) }),
                    Wave("Guards", 10f, true, new[] { S("e_veilguard", 4, "spawn_left", 1.1f) })
                },
                "Clear the plaza", "dt_elite_late", music: "mx_combat_undercity"));
            content.Encounters.Add(Encounter("enc_ch11_exchange", 11, "Relay Exchange", "arena_ch11_exchange", 125,
                new[]
                {
                    Wave("Exchange guard", 0f, true, new[] { S("e_veilguard", 4, "ground", 1.1f), S("e_longwatch", 3, "ledge", 1.2f) }),
                    Wave("Conductors", 10f, true, new[] { S("e_signal_conductor", 2, "ledge", 1.8f) }),
                    Wave("Nodes", 10f, true, new[] { S("e_veil_engine", 3, "ledge", 2.2f) })
                },
                "Clear the exchange", "dt_arena_chest", music: "mx_combat_undercity"));
            content.Encounters.Add(Encounter("enc_ch11_stores", 11, "Directorate Stores", "arena_ch11_plaza", 75,
                new[]
                {
                    Wave("Store guard", 0f, true, new[] { S("e_line_trooper", 9, "ground", 0.4f), S("e_veilguard", 2, "ledge") })
                },
                "Requisition the stores", "dt_elite_late"));

            // ---- Chapter 12 ----
            content.Encounters.Add(Encounter("enc_ch12_spine", 12, "Spine Guard", "arena_ch12_spine", 110,
                new[]
                {
                    Wave("Node guard", 0f, true, new[] { S("e_veil_engine", 3, "ledge", 2.2f), S("e_veil_stalker", 4, "ground") }),
                    Wave("Archive arms", 10f, true, new[] { S("e_veil_engine", 4, "ledge", 2f), S("e_echo_wisp", 6, "air", 0.9f) })
                },
                "Climb the archive spine", "dt_elite_late", music: "mx_combat_veil"));
            content.Encounters.Add(Encounter("enc_ch12_meridian", 12, "Meridian Annex Guard", "arena_ch12_meridian", 130,
                new[]
                {
                    Wave("Annex guard", 0f, true, new[] { S("e_veilguard", 5, "ground", 1.1f), S("e_veil_engine", 2, "ledge", 2.2f) }),
                    Wave("Archive arms", 10f, true, new[] { S("e_veil_engine", 4, "ledge", 2f), S("e_signal_conductor", 2, "ground", 1.8f) }),
                    Wave("Final guard", 12f, true, new[] { S("e_veilguard", 5, "spawn_right", 1f), S("e_veil_engine", 3, "ledge", 2f) })
                },
                "Clear the annex", "dt_chapter_boss", music: "mx_combat_veil"));
            content.Encounters.Add(Encounter("enc_ch12_index", 12, "Index Hall", "arena_ch12_mb2", 90,
                new[]
                {
                    Wave("Index guard", 0f, true, new[] { S("e_veil_engine", 3, "ledge", 2.2f), S("e_veil_stalker", 5, "ground", 0.9f) }),
                    Wave("Echoes", 9f, true, new[] { S("e_echo_wisp", 8, "air", 0.8f) })
                },
                "Clear the index hall", "dt_elite_late", music: "mx_combat_veil"));
        }
    }
}
