namespace IronVeil.Rules
{
    public static partial class Campaign
    {
        private static void BuildQuests(CampaignContent content)
        {
            // ================= Chapter 1: The Rust Gate =================
            content.Quests.Add(Quest(
                "q01_scrap_run", "Scrap Run", QuestKind.Bounty, 1,
                "Quartermaster Ivo", "Rust Gate scrap yard, west of the checkpoint",
                "Ivo needs the yard cleared before his crew can pull the salvage out. Drones first, then the troopers they are guarding.",
                new[]
                {
                    Objective(ObjectiveType.KillCount, "e_rust_drone", 12, "Destroy rusty patrol drones"),
                    Objective(ObjectiveType.KillCount, "e_line_trooper", 6, "Clear the Directorate picket")
                },
                900, 70, 12, grantedFlags: new[] { "flag_q01_scrap_cleared" }));

            content.Quests.Add(Quest(
                "q01_lost_cartography", "Lost Cartography", QuestKind.Collection, 1,
                "Vera Voss", "Rusted survey office above the gate",
                "Three of Vera's survey shards survived the gate's collapse. She wants them back before the Directorate archives them.",
                new[]
                {
                    Objective(ObjectiveType.CollectItem, "itm_carto_shard", 3, "Recover survey shards"),
                    Objective(ObjectiveType.ReachArea, "area_ch01_survey_office", 1, "Reach the survey office", optional: true)
                },
                1100, 60, 15, gearRewardId: "g_core_basic"));

            content.Quests.Add(Quest(
                "q01_wall_of_names", "The Wall of Names", QuestKind.SideQuest, 1,
                "Kade Rourke", "Memorial wall inside the gate",
                "Someone scratched the Meridian names into the gate's concrete. Kade wants them read out loud, one panel at a time.",
                new[]
                {
                    Objective(ObjectiveType.InteractWith, "obj_memorial_panel", 3, "Read the memorial panels")
                },
                800, 40, 8, perkRewardId: "perk_pack_rat",
                grantedFlags: new[] { "flag_meridian_named" }));

            // ================= Chapter 2: Sunken Docks =================
            content.Quests.Add(Quest(
                "q02_dock_rights", "Dock Rights", QuestKind.SideQuest, 2,
                "Dockmaster Ren", "Flooded lifting yard, south pier",
                "The cranes still swing on their own, and they keep dropping containers on Ren's people. Cut the hooks loose.",
                new[]
                {
                    Objective(ObjectiveType.DestroyTarget, "obj_crane_hook", 4, "Sever crane hooks"),
                    Objective(ObjectiveType.KillCount, "e_skiff", 5, "Drive off the harbour skiffs")
                },
                1400, 90, 14));

            content.Quests.Add(Quest(
                "q02_deep_salvage", "Deep Salvage", QuestKind.SideQuest, 2,
                "Diver Pell", "Flooded cargo hold, lower dock",
                "Pell will not surface until the hold is clear. Hold the platform while he works.",
                new[]
                {
                    Objective(ObjectiveType.SurviveSeconds, string.Empty, 90, "Hold the platform for 90 seconds"),
                    Objective(ObjectiveType.KillCount, "e_skiff", 4, "Keep the skiffs off the platform", optional: true)
                },
                1500, 110, 10, gearRewardId: "g_trinket_salvager"));

            content.Quests.Add(Quest(
                "q02_tide_charts", "Tide Charts", QuestKind.Collection, 2,
                "Old Marrow", "Warehouse rooftops, north pier",
                "Marrow's tide charts are the only thing still predicting where the water goes. Two more charts are floating out there.",
                new[]
                {
                    Objective(ObjectiveType.CollectItem, "itm_tide_chart", 2, "Recover tide charts"),
                    Objective(ObjectiveType.ReachArea, "area_ch02_lighthouse", 1, "Climb the harbour lighthouse")
                },
                1300, 80, 12, prerequisiteQuestIds: new[] { "q02_dock_rights" }));

            // ================= Chapter 3: Rooftop Watch =================
            content.Quests.Add(Quest(
                "q03_long_watch_down", "Long Watch Down", QuestKind.Bounty, 3,
                "Sergeant Bek", "Rooftops over the Watch district",
                "Bek's squad is pinned by Directorate marksmen. Take the long watches off the roofs.",
                new[]
                {
                    Objective(ObjectiveType.KillCount, "e_longwatch", 6, "Eliminate Longwatch marksmen"),
                    Objective(ObjectiveType.KillCount, "e_bulwark_trooper", 4, "Break the shield wall")
                },
                1700, 120, 16));

            content.Quests.Add(Quest(
                "q03_signal_echoes", "Signal Echoes", QuestKind.Collection, 3,
                "Operator Senn", "Comms tower, east rooftops",
                "Senn can trace Halden's command channel if you bring back four intact relay cores.",
                new[]
                {
                    Objective(ObjectiveType.CollectItem, "itm_relay_core", 4, "Recover relay cores"),
                    Objective(ObjectiveType.KillCount, "e_longwatch", 3, "Clear the tower approach", optional: true)
                },
                1900, 130, 14, gearRewardId: "g_trinket_sprinter",
                grantedFlags: new[] { "flag_halden_channel_mapped" }));

            content.Quests.Add(Quest(
                "q03_snipers_ledger", "The Sniper's Ledger", QuestKind.SideQuest, 3,
                "Vera Voss", "Halden's forward nest",
                "Halden keeps a paper ledger of every shot he has taken. Vera wants two pages of it, torn out by hand.",
                new[]
                {
                    Objective(ObjectiveType.InteractWith, "obj_halden_ledger", 2, "Tear pages from Halden's ledger"),
                    Objective(ObjectiveType.NoDamageRun, string.Empty, 1, "Do it without taking a hit", optional: true)
                },
                2100, 100, 12, prerequisiteQuestIds: new[] { "q03_signal_echoes" }));

            // ================= Chapter 4: The Foundry =================
            content.Quests.Add(Quest(
                "q04_line_halt", "Assembly Line Halt", QuestKind.SideQuest, 4,
                "Foreman Ulla", "Casting floor seven",
                "Ulla's old crew is still welded into the line. Break the presses so nobody else goes in.",
                new[]
                {
                    Objective(ObjectiveType.DestroyTarget, "obj_press_unit", 5, "Destroy press units"),
                    Objective(ObjectiveType.KillCount, "e_ironjaw_brute", 5, "Put down the brute squad")
                },
                2300, 160, 18));

            content.Quests.Add(Quest(
                "q04_molten_haul", "Molten Haul", QuestKind.Collection, 4,
                "Quartermaster Ivo", "Slag tunnels under the foundry",
                "Ivo wants six crucibles of unspent Veil alloy. They are heavy, hot, and guarded.",
                new[]
                {
                    Objective(ObjectiveType.CollectItem, "itm_veil_crucible", 6, "Recover crucibles"),
                    Objective(ObjectiveType.KillCount, "e_toxin_crawler", 6, "Clear the tunnels")
                },
                2500, 180, 16, gearRewardId: "g_core_reactive"));

            content.Quests.Add(Quest(
                "q04_foundry_whistle", "Whistle Test", QuestKind.Escort, 4,
                "Engineer Pada", "Pressure gallery, level two",
                "Pada has to restart the vent fans or the gallery cooks. Cover her for two minutes.",
                new[]
                {
                    Objective(ObjectiveType.EscortTo, "npc_pada", 1, "Escort Pada to the vent controls"),
                    Objective(ObjectiveType.SurviveSeconds, string.Empty, 120, "Hold the gallery while she works"),
                    Objective(ObjectiveType.KillCount, "e_mortar_team", 3, "Silence the mortar teams", optional: true)
                },
                2600, 170, 20, prerequisiteQuestIds: new[] { "q04_line_halt" }));

            // ================= Chapter 5: The Siren's Stage =================
            content.Quests.Add(Quest(
                "q05_silence_choir", "Silence the Choir", QuestKind.Bounty, 5,
                "Chaplain Ord", "Old opera house, upper balconies",
                "Ord can still hear the Siren's choir in his teeth. Kill the chaplains holding the harmony.",
                new[]
                {
                    Objective(ObjectiveType.KillCount, "e_field_chaplain", 5, "Eliminate Veil chaplains"),
                    Objective(ObjectiveType.KillCount, "e_echo_wisp", 6, "Scatter the echo wisps")
                },
                2900, 200, 16));

            content.Quests.Add(Quest(
                "q05_stage_fright", "Stage Fright", QuestKind.SideQuest, 5,
                "Stagehand Miri", "Understage, beneath the orchestra pit",
                "Miri has the score the Siren was written from. Get it out without letting the music touch you.",
                new[]
                {
                    Objective(ObjectiveType.NoDamageRun, string.Empty, 1, "Reach the score without taking damage", optional: true),
                    Objective(ObjectiveType.ReachArea, "area_ch05_understage", 1, "Reach the understage archive"),
                    Objective(ObjectiveType.CollectItem, "itm_siren_score", 1, "Take the score")
                },
                3000, 210, 14, perkRewardId: "perk_second_wind"));

            content.Quests.Add(Quest(
                "q05_encore", "Encore", QuestKind.Bounty, 5,
                "Operator Senn", "Balcony rail above the pit",
                "The mites come back every time the song restarts. Senn wants thirty of them archived properly this time.",
                new[]
                {
                    Objective(ObjectiveType.KillCount, "e_memory_mites", 30, "Clear mite swarms"),
                    Objective(ObjectiveType.UseAbilityCount, string.Empty, 15, "Use abilities to break the swarm", optional: true)
                },
                2800, 190, 12, prerequisiteQuestIds: new[] { "q05_silence_choir" }));

            // ================= Chapter 6: Hollow Cathedral =================
            content.Quests.Add(Quest(
                "q06_catalogue", "Catalogue the Choir", QuestKind.Collection, 6,
                "The Last Archivist", "Cathedral nave, archive alcove",
                "A voice in the archive asks for five recordings of the Choir before you break it. It will pay in things you need.",
                new[]
                {
                    Objective(ObjectiveType.CollectItem, "itm_choir_recording", 5, "Capture Choir recordings"),
                    Objective(ObjectiveType.KillCount, "e_memory_mites", 20, "Survive the nave swarm")
                },
                3300, 240, 16, gearRewardId: "g_trinket_marksman"));

            content.Quests.Add(Quest(
                "q06_den_mother", "Den Mother", QuestKind.Bounty, 6,
                "Sergeant Bek", "Collapsed transept, north side",
                "Bek will not move the wounded through the transept while the Brood Keeper is still in it.",
                new[]
                {
                    Objective(ObjectiveType.DefeatBoss, "mb_brood_keeper", 1, "Defeat the Brood Keeper"),
                    Objective(ObjectiveType.KillCount, "e_memory_mites", 15, "Clear the transept", optional: true)
                },
                3500, 260, 15, grantedFlags: new[] { "flag_transept_open" }));

            content.Quests.Add(Quest(
                "q06_sanctuary", "Sanctuary", QuestKind.Escort, 6,
                "Chaplain Ord", "South cloister to the sealed chapel",
                "Ord is carrying the last civilian roster. Walk him to the sealed chapel and keep the Choir off him.",
                new[]
                {
                    Objective(ObjectiveType.EscortTo, "npc_ord", 1, "Escort Ord to the chapel"),
                    Objective(ObjectiveType.KillCount, "e_toxin_crawler", 8, "Clear the cloister"),
                    Objective(ObjectiveType.NoDamageRun, string.Empty, 1, "Keep Ord unhurt", optional: true)
                },
                3400, 230, 18, prerequisiteQuestIds: new[] { "q06_den_mother" }));

            // ================= Chapter 7: Glass District =================
            content.Quests.Add(Quest(
                "q07_shatter_twins", "Shatter the Twins", QuestKind.Bounty, 7,
                "Nightglass (intercepted)", "Mirrored atrium, district centre",
                "Something wearing your outline is walking the glass halls giving orders. Kill it before it gives another.",
                new[]
                {
                    Objective(ObjectiveType.DefeatBoss, "mb_nightglass", 1, "Destroy the Glass Twin"),
                    Objective(ObjectiveType.KillCount, "e_veil_stalker", 6, "Clear the hall")
                },
                3900, 300, 16));

            content.Quests.Add(Quest(
                "q07_glass_run", "Glass Run", QuestKind.SideQuest, 7,
                "Runner Tace", "Glass rooftops, district edge",
                "Tace runs messages across the mirror rooftops. She needs the stalkers off the lines before the next run.",
                new[]
                {
                    Objective(ObjectiveType.KillCount, "e_veil_stalker", 8, "Clear stalkers from the roof run"),
                    Objective(ObjectiveType.SurviveSeconds, string.Empty, 100, "Hold the roof run"),
                    Objective(ObjectiveType.ReachArea, "area_ch07_glass_bridge", 1, "Cross the glass bridge", optional: true)
                },
                3800, 280, 14, gearRewardId: "g_trinket_reactor"));

            content.Quests.Add(Quest(
                "q07_reflections", "Reflections", QuestKind.Collection, 7,
                "Vera Voss", "Mirror gallery above the atrium",
                "Four mirrored plates still hold images of the district before the Veil. Vera wants them.",
                new[]
                {
                    Objective(ObjectiveType.CollectItem, "itm_mirror_plate", 4, "Recover mirror plates"),
                    Objective(ObjectiveType.KillCount, "e_veil_stalker", 5, "Clear the gallery", optional: true)
                },
                4000, 290, 15, perkRewardId: "perk_scavenger",
                prerequisiteQuestIds: new[] { "q07_glass_run" }));

            // ================= Chapter 8: Bone Yards =================
            content.Quests.Add(Quest(
                "q08_yard_cull", "Yard Cull", QuestKind.Bounty, 8,
                "Dockmaster Ren", "Bone yards, container maze",
                "Ren's crews cannot work the yards while the hounds are breeding in them. Twelve down, then we talk about the alpha.",
                new[]
                {
                    Objective(ObjectiveType.KillCount, "e_hollow_hound", 12, "Cull hollow hounds"),
                    Objective(ObjectiveType.KillCount, "e_siege_walker", 4, "Disable the siege walkers")
                },
                4300, 340, 18));

            content.Quests.Add(Quest(
                "q08_alpha_down", "Alpha Down", QuestKind.Bounty, 8,
                "Dockmaster Ren", "Container maze, deep yard",
                "The hounds will keep coming until the alpha is dead. That is the whole job.",
                new[]
                {
                    Objective(ObjectiveType.DefeatBoss, "mb_ironjaw", 1, "Defeat Ironjaw"),
                    Objective(ObjectiveType.KillCount, "e_hollow_hound", 8, "Survive the pack", optional: true)
                },
                4500, 380, 16, prerequisiteQuestIds: new[] { "q08_yard_cull" },
                gearRewardId: "g_core_bastion"));

            content.Quests.Add(Quest(
                "q08_feeding_time", "Feeding Time", QuestKind.Escort, 8,
                "Engineer Pada", "Yard gantry, north crane",
                "Pada has to refuel the gantry lift while the pack is still loose. Hold the gantry for her.",
                new[]
                {
                    Objective(ObjectiveType.EscortTo, "npc_pada", 1, "Cover Pada at the gantry"),
                    Objective(ObjectiveType.SurviveSeconds, string.Empty, 120, "Hold the gantry for two minutes")
                },
                4400, 350, 18, gearRewardId: "g_trinket_kinetic"));

            // ================= Chapter 9: Runoff Cathedral =================
            content.Quests.Add(Quest(
                "q09_clean_vat", "Clean the Vat", QuestKind.Bounty, 9,
                "Quartermaster Ivo", "Runoff cisterns, lower level",
                "Ivo wants the cistern walkable again. Crawlers first, then the drill that keeps opening new holes.",
                new[]
                {
                    Objective(ObjectiveType.KillCount, "e_toxin_crawler", 10, "Burn out crawler nests"),
                    Objective(ObjectiveType.DestroyTarget, "obj_runoff_valve", 4, "Seal runoff valves")
                },
                4700, 400, 18));

            content.Quests.Add(Quest(
                "q09_bore_no_more", "Bore No More", QuestKind.Bounty, 9,
                "Foreman Ulla", "Cistern roof, bore shaft",
                "Ulla spent six weeks hearing that drill come up through her floor. Finish it.",
                new[]
                {
                    Objective(ObjectiveType.DefeatBoss, "mb_drillmaster", 1, "Destroy the Drillmaster"),
                    Objective(ObjectiveType.KillCount, "e_hollow_hound", 6, "Clear the shaft", optional: true)
                },
                4900, 430, 16, perkRewardId: "perk_veteran",
                prerequisiteQuestIds: new[] { "q09_clean_vat" }));

            content.Quests.Add(Quest(
                "q09_purge", "Purge", QuestKind.SideQuest, 9,
                "Chaplain Ord", "Chapel of runoff, east cistern",
                "Ord has a litany for the runoff, and it involves strong explosives and a lot of running.",
                new[]
                {
                    Objective(ObjectiveType.UseAbilityCount, string.Empty, 15, "Use abilities against the runoff"),
                    Objective(ObjectiveType.KillCount, "e_echo_wisp", 10, "Clear echo wisps"),
                    Objective(ObjectiveType.NoDamageRun, string.Empty, 1, "Purge without taking runoff damage", optional: true)
                },
                4800, 410, 14));

            // ================= Chapter 10: Ossuary =================
            content.Quests.Add(Quest(
                "q10_honour_guard", "Honour Guard", QuestKind.Bounty, 10,
                "The Last Archivist", "Ossuary approach, bone gallery",
                "The Archivist knows the names of the eight Veilguard in the gallery. It would like them recorded before they are broken.",
                new[]
                {
                    Objective(ObjectiveType.KillCount, "e_veilguard", 8, "Break the Veilguard"),
                    Objective(ObjectiveType.KillCount, "e_veil_engine", 3, "Disable engine nodes")
                },
                5300, 470, 20));

            content.Quests.Add(Quest(
                "q10_reflect_nothing", "Reflect Nothing", QuestKind.SideQuest, 10,
                "Operator Senn", "Ossuary plates, west wall",
                "Senn has a trick for the front plates: hit the seams, not the faces. Prove it works.",
                new[]
                {
                    Objective(ObjectiveType.HitWeakPointCount, string.Empty, 12, "Land weak point hits on Veilwardens"),
                    Objective(ObjectiveType.KillCount, "e_veilguard", 4, "Prove the seams are enough")
                },
                5400, 460, 16, gearRewardId: "g_plating_mk3", prerequisiteQuestIds: new[] { "q10_honour_guard" }));

            content.Quests.Add(Quest(
                "q10_bridge_toll", "Bridge Toll", QuestKind.Bounty, 10,
                "Dockmaster Ren", "Veil bridge, ossuary crossing",
                "The Tollkeeper has been charging a toll on a bridge to nothing for eleven years. Cancel the contract.",
                new[]
                {
                    Objective(ObjectiveType.DefeatBoss, "mb_tollkeeper", 1, "Defeat the Tollkeeper"),
                    Objective(ObjectiveType.ReachArea, "area_ch10_veil_bridge", 1, "Cross the veil bridge", optional: true)
                },
                5500, 480, 15, grantedFlags: new[] { "flag_veil_bridge_open" }));

            // ================= Chapter 11: Undercity =================
            content.Quests.Add(Quest(
                "q11_crown_of_gutters", "Crown of Gutters", QuestKind.Bounty, 11,
                "Runner Tace", "Undercity throne, drowned plaza",
                "The Gutterking holds the only route to the Veil's service shaft. Tace has already mapped the way through him.",
                new[]
                {
                    Objective(ObjectiveType.DefeatBoss, "mb_gutterking", 1, "Take the Gutterking's route"),
                    Objective(ObjectiveType.KillCount, "e_line_trooper", 12, "Clear the plaza")
                },
                6100, 540, 20));

            content.Quests.Add(Quest(
                "q11_memo_war", "Memo War", QuestKind.SideQuest, 11,
                "Operator Senn", "Directorate relay, undercity exchange",
                "The Directorate is still issuing orders to units that no longer exist. Senn wants the relay stack torn down.",
                new[]
                {
                    Objective(ObjectiveType.DestroyTarget, "obj_relay_stack", 6, "Destroy relay stacks"),
                    Objective(ObjectiveType.KillCount, "e_signal_conductor", 3, "Silence the conductors")
                },
                6000, 520, 18, gearRewardId: "g_trinket_reactor"));

            content.Quests.Add(Quest(
                "q11_requisition", "Requisition", QuestKind.Collection, 11,
                "Quartermaster Ivo", "Directorate stores, level three",
                "Ivo has found where the Directorate kept the good ammunition. Eight crates of it.",
                new[]
                {
                    Objective(ObjectiveType.CollectItem, "itm_ammo_crate", 8, "Requisition ammo crates"),
                    Objective(ObjectiveType.SurviveSeconds, string.Empty, 90, "Hold the stores while loading", optional: true)
                },
                6200, 560, 16, perkRewardId: "perk_marksman_focus",
                prerequisiteQuestIds: new[] { "q11_crown_of_gutters" }));

            // ================= Chapter 12: The Iron Veil =================
            content.Quests.Add(Quest(
                "q12_last_names", "Last Names", QuestKind.Collection, 12,
                "The Last Archivist", "Veil archive spine",
                "Ten names are still missing from the archive. The Archivist will not let the Veil go dark until they are in the record.",
                new[]
                {
                    Objective(ObjectiveType.CollectItem, "itm_name_tablet", 10, "Recover name tablets"),
                    Objective(ObjectiveType.KillCount, "e_veil_engine", 4, "Clear the archive spine")
                },
                7000, 640, 20, perkRewardId: "perk_iron_will"));

            content.Quests.Add(Quest(
                "q12_veil_hand", "Veil Hand", QuestKind.Bounty, 12,
                "Vera Voss", "Curator's reach, collapsed archive",
                "One limb of the Veil is still reaching for the district below. Cut it off before it finishes the job.",
                new[]
                {
                    Objective(ObjectiveType.DefeatBoss, "mb_veil_hand", 1, "Sever the Veil Hand"),
                    Objective(ObjectiveType.KillCount, "e_veil_engine", 6, "Hold the archive floor", optional: true)
                },
                7200, 660, 18, grantedFlags: new[] { "flag_veil_hand_severed" }));

            content.Quests.Add(Quest(
                "q12_archive_erased", "Archive Erased", QuestKind.SideQuest, 12,
                "Kade Rourke", "Meridian annex, sealed floor",
                "There is one annex the Veil keeps rewriting. Kade wants to see what it is hiding before it hides it again.",
                new[]
                {
                    Objective(ObjectiveType.ReachArea, "area_ch12_meridian_annex", 1, "Reach the Meridian annex"),
                    Objective(ObjectiveType.DestroyTarget, "obj_archive_writer", 4, "Destroy archive writers"),
                    Objective(ObjectiveType.KillCount, "e_signal_conductor", 4, "Survive the archive guard"),
                    Objective(ObjectiveType.NoDamageRun, string.Empty, 1, "Leave without the Veil touching you", optional: true)
                },
                7400, 680, 22, prerequisiteQuestIds: new[] { "q12_last_names" },
                grantedFlags: new[] { "flag_meridian_annex_read" }));
        }
    }
}
