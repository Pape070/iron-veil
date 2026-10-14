namespace IronVeil.Rules
{
    public static partial class Campaign
    {
        private static void BuildBosses(CampaignContent content)
        {
            BuildChapterBosses(content);
            BuildMiniBosses(content);
        }

        // ------------------------------------------------------------------
        // Chapter bosses: three phases, ten patterns each, escalating with
        // difficulty. Core patterns are always live, Extended patterns appear on
        // Normal, Full patterns only on Hard.
        // ------------------------------------------------------------------

        private static void BuildChapterBosses(CampaignContent content)
        {
            // ---- Chapter 1: Warden Skarn ----
            content.Bosses.Add(Boss(
                "boss_skarn", "Warden Skarn", "Keeper of the Rust Gate", 1,
                1250f, 14f, 10f, 3.6f,
                new[]
                {
                    Phase("skarn_p1", "Riot Protocol", 1f, new[]
                    {
                        P("skarn_shield_bash", "Shield Bash", AttackPatternKind.ChargeDash, PatternTier.Core, 4f, 0.7f, 0.5f, 14f, speed: 13f, range: 8f, hint: "He winds up before the bash - step aside"),
                        P("skarn_riot_line", "Riot Line", AttackPatternKind.AimedVolley, PatternTier.Core, 3.2f, 0.6f, 0.5f, 11f, projectiles: 4, spread: 14f, speed: 12f, range: 12f),
                        P("skarn_turrets", "Deploy Sentries", AttackPatternKind.TurretDeploy, PatternTier.Extended, 11f, 0.9f, 1.2f, 8f, projectiles: 2, range: 16f, tags: "adds")
                    }, announcement: "Warden Skarn locks the gate."),
                    Phase("skarn_p2", "Breach Protocol", 0.65f, new[]
                    {
                        P("skarn_sweep", "Shield Sweep", AttackPatternKind.TailSweep, PatternTier.Core, 4.4f, 0.65f, 0.6f, 16f, spread: 180f, range: 6f),
                        P("skarn_grenades", "Stun Grenades", AttackPatternKind.Minefield, PatternTier.Core, 5.5f, 0.8f, 2.2f, 12f, projectiles: 4, spread: 50f, range: 12f, tags: "hazard"),
                        P("skarn_reinforce", "Reinforce Plating", AttackPatternKind.ReflectShield, PatternTier.Extended, 13f, 0.8f, 3.5f, 0f, range: 8f, tags: "defensive")
                    }, cooldownMultiplier: 0.9f, announcement: "Armour plates crack open. He stops holding back."),
                    Phase("skarn_p3", "Last Stand", 0.3f, new[]
                    {
                        P("skarn_overdrive", "Overdrive Charge", AttackPatternKind.ChargeDash, PatternTier.Core, 3.4f, 0.55f, 0.6f, 19f, speed: 17f, range: 11f),
                        P("skarn_rage", "Riot Fury", AttackPatternKind.RadialBurst, PatternTier.Core, 5f, 0.75f, 0.8f, 13f, projectiles: 14, spread: 360f, speed: 11f, range: 12f),
                        P("skarn_finale", "Gate Collapse", AttackPatternKind.ArenaHazard, PatternTier.Extended, 8f, 1f, 3f, 18f, projectiles: 6, spread: 360f, range: 18f, tags: "hazard"),
                        P("skarn_double_bash", "Twin Bash", AttackPatternKind.ChargeDash, PatternTier.Full, 7f, 0.45f, 0.5f, 21f, speed: 19f, range: 12f)
                    }, cooldownMultiplier: 0.75f, speedMultiplier: 1.15f, damageMultiplier: 1.2f,
                        announcement: "The gate is his last argument.")
                },
                arenaId: "arena_ch01_boss", weakPoints: 1,
                musicId: "mx_boss_iron", introComicId: "cp01_boss", outroComicId: "cp01_outro",
                bestiary: "A career checkpoint officer in riot plate. Fights like a door: patient, and very hard to get past.",
                resistances: new[] { "knockback" }, enrageAfter: 150f));

            // ---- Chapter 2: Tidewalker ----
            content.Bosses.Add(Boss(
                "boss_tidewalker", "Tidewalker", "Harbour Siege Frame", 2,
                1500f, 19f, 20f, 3.2f,
                new[]
                {
                    Phase("tide_p1", "Shallow Water", 1f, new[]
                    {
                        P("tide_missiles", "Harpoon Salvo", AttackPatternKind.MissileBarrage, PatternTier.Core, 4.6f, 0.9f, 1f, 19f, projectiles: 3, spread: 24f, speed: 9f, range: 18f),
                        P("tide_stomp", "Water Stomp", AttackPatternKind.GroundPound, PatternTier.Core, 5f, 0.8f, 0.6f, 17f, radius: 5f, range: 6f),
                        P("tide_flood", "Rising Tide", AttackPatternKind.ArenaHazard, PatternTier.Extended, 12f, 1.2f, 5f, 10f, projectiles: 5, spread: 360f, range: 20f, tags: "hazard", hint: "Water rises - stay on the crates")
                    }, announcement: "Tidewalker wades in."),
                    Phase("tide_p2", "Undertow", 0.6f, new[]
                    {
                        P("tide_beam", "Cutting Beam", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.2f, 0.95f, 1.6f, 24f, speed: 0f, range: 22f, hint: "The beam sweeps low - jump it"),
                        P("tide_charge", "Hull Charge", AttackPatternKind.ChargeDash, PatternTier.Core, 5.5f, 0.8f, 0.7f, 26f, speed: 15f, range: 12f),
                        P("tide_depth_charges", "Depth Charges", AttackPatternKind.Minefield, PatternTier.Extended, 9f, 1f, 3f, 18f, projectiles: 7, spread: 360f, range: 16f, tags: "hazard")
                    }, cooldownMultiplier: 0.9f, announcement: "The frame submerges and comes back angry."),
                    Phase("tide_p3", "Deep Breach", 0.3f, new[]
                    {
                        P("tide_torpedo", "Torpedo Fan", AttackPatternKind.MissileBarrage, PatternTier.Core, 3.8f, 0.8f, 1.2f, 24f, projectiles: 5, spread: 40f, speed: 11f, range: 20f),
                        P("tide_whirl", "Whirlpool", AttackPatternKind.RadialBurst, PatternTier.Core, 5.4f, 0.9f, 1f, 20f, projectiles: 18, spread: 360f, speed: 10f, range: 14f),
                        P("tide_crush", "Hull Crush", AttackPatternKind.GrappleSlam, PatternTier.Extended, 8f, 0.9f, 1.1f, 28f, range: 14f),
                        P("tide_storm", "Squall Barrage", AttackPatternKind.MissileBarrage, PatternTier.Full, 9f, 0.7f, 2f, 22f, projectiles: 9, spread: 90f, speed: 12f, range: 22f)
                    }, cooldownMultiplier: 0.8f, damageMultiplier: 1.15f, spawnsAdds: true, addsPerWave: 2,
                        announcement: "Tidewalker opens every valve it has.")
                },
                arenaId: "arena_ch02_boss", weakPoints: 2,
                musicId: "mx_boss_tide", introComicId: "cp02_boss", outroComicId: "cp02_outro",
                bestiary: "Pre-war harbour crane refitted into a siege frame. Its legs are the only thing that ever stops it.",
                resistances: new[] { "knockback", "water" }, enrageAfter: 170f));

            // ---- Chapter 3: Overseer Halden ----
            content.Bosses.Add(Boss(
                "boss_halden", "Overseer Halden", "The Long Watch", 3,
                1500f, 26f, 30f, 4.6f,
                new[]
                {
                    Phase("halden_p1", "Rooftop Watch", 1f, new[]
                    {
                        P("halden_rail", "Rail Shot", AttackPatternKind.AimedVolley, PatternTier.Core, 3.2f, 1.3f, 0.3f, 34f, speed: 44f, range: 30f, hint: "Red laser means the shot is already coming"),
                        P("halden_relocate", "Relocate", AttackPatternKind.PhaseBlink, PatternTier.Core, 6f, 0.35f, 0.6f, 0f, range: 24f),
                        P("halden_markers", "Mark the Ground", AttackPatternKind.Minefield, PatternTier.Extended, 8f, 1f, 2.6f, 20f, projectiles: 5, spread: 70f, range: 20f, tags: "hazard")
                    }, announcement: "Halden has the high ground and a clear line."),
                    Phase("halden_p2", "Crossfire", 0.65f, new[]
                    {
                        P("halden_drone", "Spotter Drones", AttackPatternKind.TurretDeploy, PatternTier.Core, 9f, 0.8f, 1f, 12f, projectiles: 3, range: 22f, tags: "adds"),
                        P("halden_triple", "Triple Tap", AttackPatternKind.AimedVolley, PatternTier.Core, 4f, 1.05f, 0.5f, 24f, projectiles: 3, spread: 2f, speed: 40f, range: 28f),
                        P("halden_smoke", "Smoke and Shift", AttackPatternKind.CloakAndStrike, PatternTier.Extended, 7f, 0.6f, 1.4f, 22f, speed: 16f, range: 12f)
                    }, cooldownMultiplier: 0.9f, announcement: "He calls in spotters and stops standing still."),
                    Phase("halden_p3", "Nothing Left To Aim At", 0.32f, new[]
                    {
                        P("halden_barrage", "Precision Barrage", AttackPatternKind.AimedVolley, PatternTier.Core, 3.4f, 0.9f, 0.9f, 26f, projectiles: 5, spread: 10f, speed: 38f, range: 30f),
                        P("halden_grenade", "Frag Grenade", AttackPatternKind.Minefield, PatternTier.Core, 5f, 0.9f, 2.4f, 24f, projectiles: 6, spread: 60f, range: 18f, tags: "hazard"),
                        P("halden_grapple", "Grapple Takedown", AttackPatternKind.GrappleSlam, PatternTier.Extended, 7.5f, 0.8f, 0.9f, 30f, range: 16f),
                        P("halden_final_shot", "The Last Round", AttackPatternKind.AimedVolley, PatternTier.Full, 10f, 1.6f, 0.25f, 60f, speed: 52f, range: 34f, hint: "One shot, one line - break it")
                    }, cooldownMultiplier: 0.8f, damageMultiplier: 1.2f, announcement: "Halden stops reloading.")
                },
                arenaId: "arena_ch03_boss", weakPoints: 1,
                musicId: "mx_boss_watch", introComicId: "cp03_boss", outroComicId: "cp03_outro",
                bestiary: "The Directorate's finest marksman, and the officer who signed the Meridian order. Fights from range and cheats when cornered.",
                resistances: new[] { "physical" }, enrageAfter: 190f));

            // ---- Chapter 4: Foundry Prime ----
            content.Bosses.Add(Boss(
                "boss_foundry", "Foundry Prime", "Assembly Line One", 4,
                1900f, 32f, 35f, 2.6f,
                new[]
                {
                    Phase("foundry_p1", "Casting Floor", 1f, new[]
                    {
                        P("foundry_crushers", "Piston Crushers", AttackPatternKind.ArenaHazard, PatternTier.Core, 4.4f, 1f, 2.4f, 30f, projectiles: 4, spread: 360f, range: 22f, tags: "hazard", hint: "Dark squares show where the pistons fall"),
                        P("foundry_lance", "Casting Lance", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.8f, 1f, 1.8f, 32f, speed: 0f, range: 22f),
                        P("foundry_ingots", "Molten Ingots", AttackPatternKind.MissileBarrage, PatternTier.Extended, 8f, 0.9f, 1.6f, 26f, projectiles: 5, spread: 50f, speed: 9f, range: 18f)
                    }, announcement: "The line restarts. Foundry Prime is the line."),
                    Phase("foundry_p2", "Overpressure", 0.62f, new[]
                    {
                        P("foundry_steam", "Steam Vents", AttackPatternKind.Minefield, PatternTier.Core, 5.6f, 0.9f, 3f, 24f, projectiles: 8, spread: 360f, range: 20f, tags: "hazard"),
                        P("foundry_claw", "Assembly Claw", AttackPatternKind.GrappleSlam, PatternTier.Core, 6f, 0.85f, 0.9f, 34f, range: 14f),
                        P("foundry_drones", "Maintenance Drones", AttackPatternKind.TurretDeploy, PatternTier.Extended, 11f, 0.9f, 1.4f, 14f, projectiles: 3, range: 20f, tags: "adds")
                    }, cooldownMultiplier: 0.9f, announcement: "Safety interlocks disengage."),
                    Phase("foundry_p3", "Meltdown Shift", 0.3f, new[]
                    {
                        P("foundry_beam_grid", "Laser Grid", AttackPatternKind.LaserGrid, PatternTier.Core, 4.6f, 1.1f, 2.6f, 30f, projectiles: 6, spread: 90f, range: 24f),
                        P("foundry_radial", "Slag Burst", AttackPatternKind.RadialBurst, PatternTier.Core, 5f, 0.9f, 0.9f, 28f, projectiles: 18, spread: 360f, speed: 11f, range: 16f),
                        P("foundry_pistons", "Rapid Pistons", AttackPatternKind.ArenaHazard, PatternTier.Extended, 6.5f, 0.8f, 2f, 26f, projectiles: 7, spread: 360f, range: 22f, tags: "hazard"),
                        P("foundry_reforge", "Reforge", AttackPatternKind.ShieldRecharge, PatternTier.Full, 14f, 1f, 3f, 0f, range: 12f, tags: "heal")
                    }, cooldownMultiplier: 0.78f, damageMultiplier: 1.15f, announcement: "Prime starts rebuilding itself mid-fight.")
                },
                arenaId: "arena_ch04_boss", weakPoints: 3,
                musicId: "mx_boss_foundry", introComicId: "cp04_boss", outroComicId: "cp04_outro",
                bestiary: "A self-repairing assembly machine that never learned the difference between a chassis and a person. Three exposed couplings are its weak points.",
                resistances: new[] { "knockback", "fire" }, enrageAfter: 200f));

            // ---- Chapter 5: The Siren ----
            content.Bosses.Add(Boss(
                "boss_siren", "The Siren", "Voice of the Veil", 5,
                2000f, 36f, 40f, 3.4f,
                new[]
                {
                    Phase("siren_p1", "First Verse", 1f, new[]
                    {
                        P("siren_scream", "Sonic Scream", AttackPatternKind.RadialBurst, PatternTier.Core, 4.2f, 0.9f, 0.9f, 34f, projectiles: 20, spread: 360f, speed: 12f, range: 15f, hint: "Rings pulse outward - use the gaps"),
                        P("siren_notes", "Piercing Notes", AttackPatternKind.AimedVolley, PatternTier.Core, 3.4f, 0.8f, 0.6f, 30f, projectiles: 5, spread: 18f, speed: 20f, range: 20f),
                        P("siren_choir", "Call the Choir", AttackPatternKind.SummonAdds, PatternTier.Extended, 10f, 1f, 1f, 8f, projectiles: 3, range: 20f, tags: "adds")
                    }, announcement: "The Siren begins to sing."),
                    Phase("siren_p2", "Dissonance", 0.6f, new[]
                    {
                        P("siren_wave", "Wave of Silence", AttackPatternKind.SweepingBeam, PatternTier.Core, 5f, 1.1f, 2.2f, 38f, speed: 0f, range: 24f),
                        P("siren_mirror", "Mirror Voice", AttackPatternKind.PhaseBlink, PatternTier.Core, 6f, 0.5f, 1.5f, 0f, range: 16f, tags: "decoy"),
                        P("siren_crescendo", "Crescendo", AttackPatternKind.HomingSwarm, PatternTier.Extended, 8f, 0.9f, 1.4f, 30f, projectiles: 8, speed: 11f, range: 18f)
                    }, cooldownMultiplier: 0.9f, announcement: "Her voice splits into two that do not agree."),
                    Phase("siren_p3", "The Last Note", 0.28f, new[]
                    {
                        P("siren_arena_pulse", "Resonance Field", AttackPatternKind.ArenaHazard, PatternTier.Core, 5.2f, 1.2f, 3.2f, 34f, projectiles: 6, spread: 360f, range: 24f, tags: "hazard"),
                        P("siren_dissonant_blast", "Dissonant Blast", AttackPatternKind.RadialBurst, PatternTier.Core, 4f, 0.7f, 0.8f, 32f, projectiles: 24, spread: 360f, speed: 13f, range: 16f),
                        P("siren_echo_lances", "Echo Lances", AttackPatternKind.BeamCross, PatternTier.Extended, 7f, 0.85f, 1.8f, 36f, projectiles: 3, spread: 90f, range: 22f),
                        P("siren_requiem", "Requiem", AttackPatternKind.ReflectShield, PatternTier.Full, 16f, 1f, 4f, 0f, range: 14f, tags: "defensive")
                    }, cooldownMultiplier: 0.75f, damageMultiplier: 1.2f, spawnsAdds: true, addsPerWave: 2,
                        announcement: "She holds one note and the room holds with her.")
                },
                arenaId: "arena_ch05_boss", weakPoints: 2,
                musicId: "mx_boss_siren", introComicId: "cp05_boss", outroComicId: "cp05_outro",
                bestiary: "A Veil cantor: a person whose voice was harvested and left conducting the sound. Harmless at range, lethal in a room.",
                resistances: new[] { "physical" }, enrageAfter: 210f));

            // ---- Chapter 6: The Hollow Choir ----
            content.Bosses.Add(Boss(
                "boss_choir_engine", "The Hollow Choir", "Swarm Engine", 6,
                2100f, 40f, 45f, 3.8f,
                new[]
                {
                    Phase("choir_p1", "First Chorus", 1f, new[]
                    {
                        P("choir_mites", "Unleash Mites", AttackPatternKind.SummonAdds, PatternTier.Core, 7f, 0.9f, 1f, 10f, projectiles: 4, range: 22f, tags: "adds"),
                        P("choir_swarm_volley", "Swarm Volley", AttackPatternKind.HomingSwarm, PatternTier.Core, 4.4f, 0.8f, 1.2f, 32f, projectiles: 10, speed: 12f, range: 18f),
                        P("choir_pulse", "Chorus Pulse", AttackPatternKind.RadialBurst, PatternTier.Extended, 6f, 0.9f, 0.9f, 36f, projectiles: 22, spread: 360f, speed: 12f, range: 16f)
                    }, announcement: "The Choir inhales."),
                    Phase("choir_p2", "Harmonics", 0.6f, new[]
                    {
                        P("choir_lanes", "Harmonic Lanes", AttackPatternKind.LaserGrid, PatternTier.Core, 4.8f, 1f, 2.6f, 34f, projectiles: 6, spread: 90f, range: 22f, tags: "hazard"),
                        P("choir_devour", "Devour Voice", AttackPatternKind.GrappleSlam, PatternTier.Core, 6.5f, 0.85f, 1f, 38f, range: 15f),
                        P("choir_mimic", "Mimic", AttackPatternKind.PhaseBlink, PatternTier.Extended, 9f, 0.5f, 2f, 0f, range: 18f, tags: "decoy")
                    }, cooldownMultiplier: 0.88f, announcement: "The Choir learns your rhythm and answers it."),
                    Phase("choir_p3", "Discordant End", 0.28f, new[]
                    {
                        P("choir_scream_wall", "Wall of Voices", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.6f, 1f, 2.4f, 38f, speed: 0f, range: 24f),
                        P("choir_swarm_storm", "Swarm Storm", AttackPatternKind.HomingSwarm, PatternTier.Core, 4f, 0.8f, 1.6f, 34f, projectiles: 14, speed: 13f, range: 20f),
                        P("choir_mine_field", "Memory Shards", AttackPatternKind.Minefield, PatternTier.Extended, 7f, 0.9f, 3f, 32f, projectiles: 9, spread: 360f, range: 20f, tags: "hazard"),
                        P("choir_second_voice", "Second Voice", AttackPatternKind.BeamCross, PatternTier.Full, 8f, 0.8f, 2f, 40f, projectiles: 3, spread: 90f, range: 22f)
                    }, cooldownMultiplier: 0.72f, speedMultiplier: 1.1f, damageMultiplier: 1.2f, spawnsAdds: true, addsPerWave: 3,
                        announcement: "Every voice it has ever taken speaks at once.")
                },
                arenaId: "arena_ch06_boss", weakPoints: 3,
                musicId: "mx_boss_choir", introComicId: "cp06_boss", outroComicId: "cp06_outro",
                bestiary: "Not a soldier. A machine built from harvested voices, and the closest thing the Veil has to a heart.",
                resistances: new[] { "knockback", "ego" }, enrageAfter: 220f));

            // ---- Chapter 7: Nightglass ----
            content.Bosses.Add(Boss(
                "boss_nightglass", "Nightglass", "The Face You Know", 7,
                2250f, 44f, 50f, 6.2f,
                new[]
                {
                    Phase("night_p1", "Reflection", 1f, new[]
                    {
                        P("night_blink_strike", "Blink Strike", AttackPatternKind.CloakAndStrike, PatternTier.Core, 3.4f, 0.6f, 0.5f, 44f, speed: 22f, range: 10f, hint: "Watch for the shimmer - it always commits"),
                        P("night_clones", "Mirror Clones", AttackPatternKind.PhaseBlink, PatternTier.Core, 7f, 0.4f, 2.5f, 0f, range: 18f, tags: "decoy"),
                        P("night_hooks", "Hook Line", AttackPatternKind.GrappleSlam, PatternTier.Extended, 6f, 0.8f, 0.9f, 40f, range: 15f)
                    }, announcement: "Nightglass steps out of the glass wearing your face."),
                    Phase("night_p2", "Shatter", 0.58f, new[]
                    {
                        P("night_shard_storm", "Shard Storm", AttackPatternKind.RadialBurst, PatternTier.Core, 4.4f, 0.85f, 0.9f, 40f, projectiles: 24, spread: 360f, speed: 14f, range: 16f),
                        P("night_fan_blades", "Fan Blades", AttackPatternKind.AimedVolley, PatternTier.Core, 3.6f, 0.7f, 0.7f, 42f, projectiles: 7, spread: 30f, speed: 24f, range: 18f),
                        P("night_mirror_field", "Mirror Field", AttackPatternKind.Minefield, PatternTier.Extended, 8f, 0.9f, 3f, 36f, projectiles: 8, spread: 360f, range: 18f, tags: "hazard")
                    }, cooldownMultiplier: 0.85f, speedMultiplier: 1.1f, announcement: "It splits into six and all six move wrong."),
                    Phase("night_p3", "A Thousand Faces", 0.26f, new[]
                    {
                        P("night_assassinate", "Assassinate", AttackPatternKind.CloakAndStrike, PatternTier.Core, 3f, 0.5f, 0.5f, 48f, speed: 24f, range: 12f),
                        P("night_cross_slash", "Cross Slash", AttackPatternKind.TailSweep, PatternTier.Core, 4.2f, 0.6f, 0.7f, 44f, spread: 200f, range: 7f),
                        P("night_shard_rain", "Shard Rain", AttackPatternKind.MissileBarrage, PatternTier.Extended, 6.5f, 0.9f, 2f, 38f, projectiles: 10, spread: 120f, speed: 13f, range: 20f),
                        P("night_true_face", "True Face", AttackPatternKind.ArenaHazard, PatternTier.Full, 10f, 1.1f, 3.5f, 46f, projectiles: 8, spread: 360f, range: 24f, tags: "hazard")
                    }, cooldownMultiplier: 0.7f, damageMultiplier: 1.15f, announcement: "The face comes off. It was never a person.")
                },
                arenaId: "arena_ch07_boss", weakPoints: 2,
                musicId: "mx_boss_glass", introComicId: "cp07_boss", outroComicId: "cp07_outro",
                bestiary: "A Veil stalker wearing a stolen outline. Fastest thing in the campaign and always attacking from behind.",
                resistances: new[] { "physical", "knockback" }, enrageAfter: 180f));

            // ---- Chapter 8: Ironmaw ----
            content.Bosses.Add(Boss(
                "boss_ironmaw", "Ironmaw", "Siege Beast", 8,
                2350f, 48f, 55f, 3.4f,
                new[]
                {
                    Phase("maw_p1", "Feeding", 1f, new[]
                    {
                        P("maw_charge", "Bone Charge", AttackPatternKind.ChargeDash, PatternTier.Core, 3.6f, 0.9f, 0.8f, 48f, speed: 16f, range: 14f, hint: "It lowers its head before charging - get off the line"),
                        P("maw_sweep", "Maw Sweep", AttackPatternKind.TailSweep, PatternTier.Core, 4.4f, 0.7f, 0.8f, 44f, spread: 220f, range: 9f),
                        P("maw_bile", "Bile Spray", AttackPatternKind.AimedVolley, PatternTier.Extended, 5.6f, 0.9f, 1f, 38f, projectiles: 8, spread: 40f, speed: 11f, range: 16f)
                    }, announcement: "Ironmaw has been fed on the road behind you."),
                    Phase("maw_p2", "Bloodied", 0.6f, new[]
                    {
                        P("maw_pound", "Seismic Pound", AttackPatternKind.GroundPound, PatternTier.Core, 5f, 0.85f, 0.7f, 46f, radius: 6f, range: 7f),
                        P("maw_hounds", "Call the Pack", AttackPatternKind.SummonAdds, PatternTier.Core, 9f, 0.9f, 1f, 12f, projectiles: 3, range: 24f, tags: "adds"),
                        P("maw_trample", "Trample Line", AttackPatternKind.SweepingBeam, PatternTier.Extended, 6.5f, 1f, 1.8f, 44f, speed: 0f, range: 24f)
                    }, cooldownMultiplier: 0.88f, speedMultiplier: 1.1f, announcement: "It keeps coming with its ribs showing."),
                    Phase("maw_p3", "Starving", 0.28f, new[]
                    {
                        P("maw_frenzy", "Frenzy Charge", AttackPatternKind.ChargeDash, PatternTier.Core, 3f, 0.6f, 0.7f, 52f, speed: 20f, range: 16f),
                        P("maw_roar", "Blood Roar", AttackPatternKind.RadialBurst, PatternTier.Core, 4.6f, 0.9f, 1f, 44f, projectiles: 22, spread: 360f, speed: 12f, range: 16f),
                        P("maw_stomp_ring", "Stomp Ring", AttackPatternKind.GroundPound, PatternTier.Extended, 5.8f, 0.8f, 0.8f, 48f, radius: 8f, range: 9f),
                        P("maw_feast", "Feast", AttackPatternKind.ShieldRecharge, PatternTier.Full, 15f, 1.1f, 3f, 0f, range: 14f, tags: "heal")
                    }, cooldownMultiplier: 0.7f, damageMultiplier: 1.2f, spawnsAdds: true, addsPerWave: 3,
                        announcement: "It bites its own plating off to get at you faster.")
                },
                arenaId: "arena_ch08_boss", weakPoints: 2,
                musicId: "mx_boss_beast", introComicId: "cp08_boss", outroComicId: "cp08_outro",
                bestiary: "A Hollow Hound grown to the size of a truck. The Veil does not make animals, only appetites.",
                resistances: new[] { "knockback" }, enrageAfter: 190f));

            // ---- Chapter 9: Sump ----
            content.Bosses.Add(Boss(
                "boss_sump", "Sump", "Runoff Cathedral", 9,
                2400f, 50f, 60f, 2.8f,
                new[]
                {
                    Phase("sump_p1", "Seep", 1f, new[]
                    {
                        P("sump_spit", "Acid Fan", AttackPatternKind.AimedVolley, PatternTier.Core, 3.8f, 0.9f, 1f, 46f, projectiles: 9, spread: 45f, speed: 11f, range: 18f),
                        P("sump_pools", "Runoff Pools", AttackPatternKind.Minefield, PatternTier.Core, 5.2f, 0.9f, 3.2f, 40f, projectiles: 8, spread: 360f, range: 20f, tags: "hazard", hint: "Green pools stay on the floor - keep moving"),
                        P("sump_tendril", "Tendril Whip", AttackPatternKind.TailSweep, PatternTier.Extended, 5.6f, 0.7f, 0.8f, 44f, spread: 240f, range: 10f)
                    }, announcement: "Sump rises out of the runoff it lives in."),
                    Phase("sump_p2", "Flood", 0.58f, new[]
                    {
                        P("sump_beam", "Corrosive Beam", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.4f, 1f, 2f, 48f, speed: 0f, range: 24f),
                        P("sump_burrow", "Burrow", AttackPatternKind.PhaseBlink, PatternTier.Core, 6.5f, 0.4f, 1.8f, 0f, range: 18f),
                        P("sump_eruption", "Eruption", AttackPatternKind.ArenaHazard, PatternTier.Extended, 7.5f, 1.1f, 3.4f, 42f, projectiles: 7, spread: 360f, range: 22f, tags: "hazard")
                    }, cooldownMultiplier: 0.85f, announcement: "The floor becomes its body."),
                    Phase("sump_p3", "Bottom of the Vat", 0.26f, new[]
                    {
                        P("sump_geyser", "Geyser", AttackPatternKind.RadialBurst, PatternTier.Core, 4f, 0.8f, 1f, 46f, projectiles: 26, spread: 360f, speed: 13f, range: 18f),
                        P("sump_crush", "Devour", AttackPatternKind.GrappleSlam, PatternTier.Core, 5.4f, 0.8f, 1f, 52f, range: 16f),
                        P("sump_spore", "Spore Cloud", AttackPatternKind.Minefield, PatternTier.Extended, 6.5f, 0.9f, 3.6f, 44f, projectiles: 10, spread: 360f, range: 22f, tags: "hazard"),
                        P("sump_reform", "Reform", AttackPatternKind.ShieldRecharge, PatternTier.Full, 13f, 0.9f, 2.6f, 0f, range: 14f, tags: "heal")
                    }, cooldownMultiplier: 0.7f, damageMultiplier: 1.18f, announcement: "It pulls itself back together from the floor up.")
                },
                arenaId: "arena_ch09_boss", weakPoints: 3,
                musicId: "mx_boss_sump", introComicId: "cp09_boss", outroComicId: "cp09_outro",
                bestiary: "Everything the Veil flushed out of the city, given a shape by pressure. Cut the tendrils and it starts over.",
                resistances: new[] { "acid", "knockback" }, enrageAfter: 200f));

            // ---- Chapter 10: Veilwarden Ossuary ----
            content.Bosses.Add(Boss(
                "boss_veilwarden", "Veilwarden Ossuary", "First of the Honoured", 10,
                3400f, 58f, 70f, 3.2f,
                new[]
                {
                    Phase("warden_p1", "Ceremony", 1f, new[]
                    {
                        P("warden_wall", "Advancing Wall", AttackPatternKind.ReflectShield, PatternTier.Core, 7f, 0.9f, 3.2f, 0f, range: 12f, tags: "defensive", hint: "Front armour reflects - get behind it"),
                        P("warden_slam", "Vehicle Slam", AttackPatternKind.GroundPound, PatternTier.Core, 5f, 0.9f, 0.8f, 54f, radius: 6f, range: 8f),
                        P("warden_beam", "Honour Beam", AttackPatternKind.SweepingBeam, PatternTier.Extended, 6f, 1.1f, 2f, 56f, speed: 0f, range: 26f)
                    }, announcement: "The Veilwarden salutes, then removes the salute."),
                    Phase("warden_p2", "Ossuary", 0.62f, new[]
                    {
                        P("warden_cross", "Cross Beam", AttackPatternKind.BeamCross, PatternTier.Core, 5.4f, 1f, 2f, 56f, projectiles: 3, spread: 90f, range: 22f),
                        P("warden_guard", "Summon Guard", AttackPatternKind.SummonAdds, PatternTier.Core, 10f, 0.9f, 1.2f, 18f, projectiles: 3, range: 24f, tags: "adds"),
                        P("warden_shockwave", "Shock Ring", AttackPatternKind.GroundPound, PatternTier.Extended, 6f, 0.85f, 0.9f, 52f, radius: 9f, range: 10f),
                        P("warden_lance_wall", "Lance Wall", AttackPatternKind.LaserGrid, PatternTier.Full, 9f, 1.1f, 2.6f, 58f, projectiles: 8, spread: 90f, range: 26f)
                    }, cooldownMultiplier: 0.85f, announcement: "It calls the rest of the honoured dead into the room."),
                    Phase("warden_p3", "Final Honour", 0.3f, new[]
                    {
                        P("warden_barrage", "Plate Barrage", AttackPatternKind.MissileBarrage, PatternTier.Core, 4.8f, 0.9f, 2.2f, 54f, projectiles: 8, spread: 80f, speed: 12f, range: 24f),
                        P("warden_charge", "Break Line", AttackPatternKind.ChargeDash, PatternTier.Core, 4.4f, 0.7f, 0.8f, 60f, speed: 17f, range: 16f),
                        P("warden_pulse", "Veil Pulse", AttackPatternKind.RadialBurst, PatternTier.Extended, 5.6f, 0.9f, 1f, 52f, projectiles: 26, spread: 360f, speed: 13f, range: 18f),
                        P("warden_second_wind", "Ossuary Reforged", AttackPatternKind.ReflectShield, PatternTier.Full, 15f, 0.8f, 4f, 0f, range: 14f, tags: "defensive")
                    }, cooldownMultiplier: 0.72f, damageMultiplier: 1.15f, spawnsAdds: true, addsPerWave: 3,
                        announcement: "The last Veilwarden takes its helmet off. There is nothing underneath.")
                },
                arenaId: "arena_ch10_boss", weakPoints: 2,
                musicId: "mx_boss_warden", introComicId: "cp10_boss", outroComicId: "cp10_outro",
                bestiary: "Armour forged from the Veil itself. Reflects anything that hits the front plate, which is why nobody has ever killed one.",
                resistances: new[] { "knockback", "physical" }, enrageAfter: 210f));

            // ---- Chapter 11: The Conductor ----
            content.Bosses.Add(Boss(
                "boss_conductor", "The Conductor", "Voice of the Directorate", 11,
                3600f, 62f, 75f, 3.8f,
                new[]
                {
                    Phase("cond_p1", "Programme", 1f, new[]
                    {
                        P("cond_orders", "Give Orders", AttackPatternKind.SummonAdds, PatternTier.Core, 8f, 0.9f, 1.2f, 20f, projectiles: 4, range: 26f, tags: "adds"),
                        P("cond_chain", "Chain Lightning", AttackPatternKind.BeamCross, PatternTier.Core, 5f, 0.9f, 1.6f, 58f, projectiles: 3, spread: 70f, range: 22f),
                        P("cond_feedback", "Feedback Wave", AttackPatternKind.RadialBurst, PatternTier.Extended, 5.6f, 1f, 1f, 54f, projectiles: 24, spread: 360f, speed: 13f, range: 18f)
                    }, announcement: "The Conductor raises a hand and the Directorate answers."),
                    Phase("cond_p2", "Crescendo", 0.6f, new[]
                    {
                        P("cond_lances", "Signal Lances", AttackPatternKind.SweepingBeam, PatternTier.Core, 5f, 1f, 2.2f, 60f, speed: 0f, range: 28f),
                        P("cond_shift", "Phase Shift", AttackPatternKind.PhaseBlink, PatternTier.Core, 6f, 0.4f, 1.6f, 0f, range: 20f),
                        P("cond_homings", "Pursuit Drones", AttackPatternKind.HomingSwarm, PatternTier.Extended, 6.5f, 0.9f, 1.8f, 52f, projectiles: 12, speed: 12f, range: 22f),
                        P("cond_grid", "Order Grid", AttackPatternKind.LaserGrid, PatternTier.Full, 8.5f, 1.1f, 2.8f, 56f, projectiles: 8, spread: 90f, range: 26f)
                    }, cooldownMultiplier: 0.85f, announcement: "He stops speaking to you and speaks to the Veil."),
                    Phase("cond_p3", "Everything At Once", 0.28f, new[]
                    {
                        P("cond_full_signal", "Full Signal", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.6f, 0.9f, 2.6f, 62f, speed: 0f, range: 30f),
                        P("cond_barrage", "Command Barrage", AttackPatternKind.MissileBarrage, PatternTier.Core, 4.4f, 0.85f, 2.4f, 56f, projectiles: 10, spread: 100f, speed: 13f, range: 26f),
                        P("cond_wave", "Final Address", AttackPatternKind.RadialBurst, PatternTier.Extended, 5f, 0.8f, 1.2f, 58f, projectiles: 30, spread: 360f, speed: 14f, range: 20f),
                        P("cond_guard", "Hardlight Guard", AttackPatternKind.ReflectShield, PatternTier.Full, 14f, 0.9f, 3.6f, 0f, range: 16f, tags: "defensive")
                    }, cooldownMultiplier: 0.68f, damageMultiplier: 1.2f, speedMultiplier: 1.1f, spawnsAdds: true, addsPerWave: 4,
                        announcement: "The person and the office are the same thing now.")
                },
                arenaId: "arena_ch11_boss", weakPoints: 2,
                musicId: "mx_boss_conductor", introComicId: "cp11_boss", outroComicId: "cp11_outro",
                bestiary: "The Director who commissioned the Veil, kept alive by it. Every order the Directorate ever gave is still in there.",
                resistances: new[] { "knockback" }, enrageAfter: 220f));

            // ---- Chapter 12: The Iron Veil (two-stage finale) ----
            content.Bosses.Add(Boss(
                "boss_veil_engine", "The Iron Veil", "Last Machine of the Directorate", 12,
                3900f, 66f, 80f, 3.6f,
                new[]
                {
                    Phase("veil_p1", "Harvest Protocol", 1f, new[]
                    {
                        P("veil_archive", "Archive Beam", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.8f, 1.1f, 2.4f, 64f, speed: 0f, range: 30f, hint: "The beam archives anything it touches - never stand still"),
                        P("veil_nodes", "Node Volley", AttackPatternKind.MissileBarrage, PatternTier.Core, 4.4f, 0.9f, 2.2f, 58f, projectiles: 9, spread: 90f, speed: 13f, range: 28f),
                        P("veil_null", "Null Pulse", AttackPatternKind.RadialBurst, PatternTier.Extended, 5.4f, 0.9f, 1.1f, 56f, projectiles: 28, spread: 360f, speed: 14f, range: 20f)
                    }, announcement: "The Veil recognises you. It has your file."),
                    Phase("veil_p2", "City of Memory", 0.72f, new[]
                    {
                        P("veil_harvest", "Harvest Squares", AttackPatternKind.ArenaHazard, PatternTier.Core, 5.6f, 1.2f, 4f, 58f, projectiles: 8, spread: 360f, range: 26f, tags: "hazard"),
                        P("veil_choir_nodes", "Node Choir", AttackPatternKind.SummonAdds, PatternTier.Core, 8f, 0.9f, 1.4f, 24f, projectiles: 4, range: 28f, tags: "adds"),
                        P("veil_grid", "Archive Grid", AttackPatternKind.LaserGrid, PatternTier.Extended, 7f, 1.1f, 2.8f, 60f, projectiles: 9, spread: 90f, range: 28f),
                        P("veil_mirrors", "Mirror Nodes", AttackPatternKind.PhaseBlink, PatternTier.Full, 9f, 0.5f, 2f, 0f, range: 22f, tags: "decoy")
                    }, cooldownMultiplier: 0.85f, announcement: "Every district it ever archived stands up inside the room."),
                    Phase("veil_p3", "Curator Sel", 0.45f, new[]
                    {
                        P("sel_spear", "Veil Spear", AttackPatternKind.GrappleSlam, PatternTier.Core, 4.2f, 0.8f, 1f, 68f, range: 18f),
                        P("sel_storm", "Memory Storm", AttackPatternKind.HomingSwarm, PatternTier.Core, 4.6f, 0.9f, 2f, 60f, projectiles: 16, speed: 14f, range: 24f),
                        P("sel_walls", "Hardlight Walls", AttackPatternKind.ArenaHazard, PatternTier.Extended, 6.5f, 1f, 3.6f, 58f, projectiles: 10, spread: 360f, range: 26f, tags: "hazard"),
                        P("sel_passage", "Passage", AttackPatternKind.BeamCross, PatternTier.Full, 8f, 0.9f, 2.2f, 62f, projectiles: 4, spread: 90f, range: 26f)
                    }, cooldownMultiplier: 0.8f, damageMultiplier: 1.15f, speedMultiplier: 1.1f,
                        announcement: "The machine opens. The person who built it steps out of the light."),
                    Phase("veil_p4", "Last Signal", 0.18f, new[]
                    {
                        P("sel_desperate", "Desperate Lance", AttackPatternKind.SweepingBeam, PatternTier.Core, 4f, 0.8f, 2.6f, 70f, speed: 0f, range: 32f),
                        P("sel_all_voices", "All Voices", AttackPatternKind.RadialBurst, PatternTier.Core, 4.6f, 0.85f, 1.3f, 62f, projectiles: 32, spread: 360f, speed: 15f, range: 22f),
                        P("sel_engine", "Engine Surge", AttackPatternKind.BeamCross, PatternTier.Extended, 6f, 1f, 2.4f, 64f, projectiles: 4, spread: 90f, range: 28f),
                        P("sel_final_stand", "Final Stand", AttackPatternKind.ReflectShield, PatternTier.Full, 13f, 0.8f, 4f, 0f, range: 16f, tags: "defensive")
                    }, cooldownMultiplier: 0.65f, damageMultiplier: 1.25f, speedMultiplier: 1.15f, spawnsAdds: true, addsPerWave: 4,
                        announcement: "She has nothing left but the machine she built and you.")
                },
                arenaId: "arena_ch12_boss", weakPoints: 3, isFinalBoss: true,
                musicId: "mx_boss_veil", introComicId: "cp12_boss", outroComicId: "cp12_outro",
                bestiary: "The Iron Veil itself: an archiving engine the size of a cathedral, and the curator who would not let it go dark.",
                resistances: new[] { "knockback", "physical" }, enrageAfter: 240f));
        }

        // ------------------------------------------------------------------
        // Mini-bosses: 24 upgrade carriers, two phases each. Quick, distinct,
        // and each one drops the gear that answers the chapter's pressure.
        // ------------------------------------------------------------------

        private static void BuildMiniBosses(CampaignContent content)
        {
            // ---- Chapter 1 ----
            content.Bosses.Add(Boss(
                "mb_rustpan", "Rustpan", "Scrap Scavenger", 1, 560f, 10f, 5f, 5f,
                new[]
                {
                    Phase("rust_p1", "Scavenge", 1f, new[]
                    {
                        P("rust_grab", "Scrap Grab", AttackPatternKind.ChargeDash, PatternTier.Core, 3.4f, 0.6f, 0.5f, 10f, speed: 14f, range: 7f),
                        P("rust_throw", "Debris Toss", AttackPatternKind.MissileBarrage, PatternTier.Core, 3f, 0.7f, 0.8f, 7f, projectiles: 3, spread: 30f, speed: 10f, range: 12f)
                    }),
                    Phase("rust_p2", "Cornered", 0.45f, new[]
                    {
                        P("rust_flail", "Chain Flail", AttackPatternKind.TailSweep, PatternTier.Core, 3.6f, 0.6f, 0.6f, 12f, spread: 200f, range: 6f),
                        P("rust_scrapstorm", "Scrap Storm", AttackPatternKind.RadialBurst, PatternTier.Extended, 6f, 0.8f, 0.8f, 9f, projectiles: 14, spread: 360f, range: 10f),
                        P("rust_avalanche", "Scrap Avalanche", AttackPatternKind.RadialBurst, PatternTier.Full, 8f, 0.7f, 1f, 12f, projectiles: 18, spread: 360f, range: 12f)
                    }, speedMultiplier: 1.15f)
                },
                isMiniBoss: true, arenaId: "arena_ch01_mb1", upgradeDrops: new[] { "g_trinket_sprinter" },
                bestiary: "A junction scavenger with a magnetic winch and no sense of self-preservation."));

            content.Bosses.Add(Boss(
                "mb_turnkey", "Turnkey", "Gate Operator", 1, 600f, 11f, 20f, 3f,
                new[]
                {
                    Phase("turn_p1", "Lockdown", 1f, new[]
                    {
                        P("turn_gate", "Gate Slam", AttackPatternKind.GroundPound, PatternTier.Core, 4.4f, 0.7f, 0.6f, 12f, radius: 4.5f, range: 6f),
                        P("turn_bolts", "Bolt Volley", AttackPatternKind.AimedVolley, PatternTier.Core, 3.2f, 0.65f, 0.5f, 9f, projectiles: 4, spread: 20f, range: 12f)
                    }),
                    Phase("turn_p2", "Overload", 0.5f, new[]
                    {
                        P("turn_welder", "Welder Torch", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.2f, 0.75f, 1.6f, 11f, speed: 0f, range: 14f),
                        P("turn_turrets", "Drop Sentries", AttackPatternKind.TurretDeploy, PatternTier.Extended, 8f, 0.8f, 1f, 6f, projectiles: 2, range: 14f, tags: "adds"),
                        P("turn_lockdown", "Full Lockdown", AttackPatternKind.ArenaHazard, PatternTier.Full, 9f, 0.9f, 3f, 10f, projectiles: 6, spread: 360f, range: 14f, tags: "hazard")
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch01_mb2", upgradeDrops: new[] { "g_core_basic" },
                bestiary: "Gate crew chief with a winch arm. Drops whatever he was carrying the moment he stops."));

            // ---- Chapter 2 ----
            content.Bosses.Add(Boss(
                "mb_wreck_diver", "Wreck Diver", "Salvage Suit", 2, 700f, 16f, 18f, 3.6f,
                new[]
                {
                    Phase("diver_p1", "Descent", 1f, new[]
                    {
                        P("diver_harpoon", "Harpoon", AttackPatternKind.GrappleSlam, PatternTier.Core, 4f, 0.8f, 0.8f, 17f, range: 12f),
                        P("diver_spikes", "Spike Launcher", AttackPatternKind.AimedVolley, PatternTier.Core, 3.4f, 0.8f, 0.6f, 13f, projectiles: 4, spread: 24f, range: 14f)
                    }),
                    Phase("diver_p2", "Pressure", 0.45f, new[]
                    {
                        P("diver_vent", "Pressure Vent", AttackPatternKind.RadialBurst, PatternTier.Core, 4.6f, 0.85f, 0.8f, 15f, projectiles: 16, spread: 360f, range: 12f),
                        P("diver_drag", "Drag Under", AttackPatternKind.PhaseBlink, PatternTier.Extended, 7f, 0.5f, 1.4f, 0f, range: 14f),
                        P("diver_crush_depth", "Crush Depth", AttackPatternKind.SweepingBeam, PatternTier.Full, 7f, 1f, 2f, 18f, speed: 0f, range: 16f)
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch02_mb1", upgradeDrops: new[] { "g_trinket_salvager" },
                bestiary: "Harbour salvager in a pressure suit. Fights like the wreck he came out of."));

            content.Bosses.Add(Boss(
                "mb_salt_tongue", "Salt Tongue", "Dock Enforcer", 2, 780f, 17f, 12f, 4.4f,
                new[]
                {
                    Phase("salt_p1", "Shakedown", 1f, new[]
                    {
                        P("salt_cleaver", "Cleaver Rush", AttackPatternKind.ChargeDash, PatternTier.Core, 3f, 0.6f, 0.5f, 16f, speed: 16f, range: 10f),
                        P("salt_pistol", "Point Blank", AttackPatternKind.AimedVolley, PatternTier.Core, 3.6f, 0.6f, 0.4f, 12f, projectiles: 3, spread: 8f, range: 10f)
                    }),
                    Phase("salt_p2", "Blood in the Water", 0.5f, new[]
                    {
                        P("salt_whirl", "Whirl", AttackPatternKind.TailSweep, PatternTier.Core, 3.4f, 0.55f, 0.7f, 18f, spread: 240f, range: 7f),
                        P("salt_crew", "Call the Crew", AttackPatternKind.SummonAdds, PatternTier.Extended, 10f, 0.8f, 1f, 6f, projectiles: 2, range: 16f, tags: "adds"),
                        P("salt_frenzy", "Dock Frenzy", AttackPatternKind.ChargeDash, PatternTier.Full, 4f, 0.5f, 0.6f, 20f, speed: 20f, range: 12f)
                    }, speedMultiplier: 1.12f)
                },
                isMiniBoss: true, arenaId: "arena_ch02_mb2", upgradeDrops: new[] { "g_core_reactive" },
                bestiary: "Dock boss with a cargo hook and a crew that does not stop coming."));

            // ---- Chapter 3 ----
            content.Bosses.Add(Boss(
                "mb_signal_thief", "Signal Thief", "Comms Interceptor", 3, 820f, 22f, 22f, 5f,
                new[]
                {
                    Phase("thief_p1", "Jamming", 1f, new[]
                    {
                        P("thief_dart", "Dart Volley", AttackPatternKind.AimedVolley, PatternTier.Core, 3f, 0.7f, 0.5f, 20f, projectiles: 5, spread: 22f, speed: 20f, range: 18f),
                        P("thief_ghost", "Ghost Walk", AttackPatternKind.PhaseBlink, PatternTier.Core, 5.5f, 0.4f, 1.2f, 0f, range: 16f)
                    }),
                    Phase("thief_p2", "Broadcast", 0.45f, new[]
                    {
                        P("thief_jam", "Jam Field", AttackPatternKind.Minefield, PatternTier.Core, 5f, 0.8f, 2.6f, 18f, projectiles: 6, spread: 360f, range: 16f, tags: "hazard"),
                        P("thief_triple", "Triple Shot", AttackPatternKind.AimedVolley, PatternTier.Extended, 3.4f, 0.6f, 0.6f, 24f, projectiles: 3, spread: 4f, speed: 24f, range: 20f),
                        P("thief_blackout", "Blackout", AttackPatternKind.ArenaHazard, PatternTier.Full, 8f, 0.9f, 3f, 20f, projectiles: 6, spread: 360f, range: 18f, tags: "hazard")
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch03_mb1", upgradeDrops: new[] { "g_trinket_sprinter" },
                bestiary: "Directorate signals officer who sells other people's orders to both sides."));

            content.Bosses.Add(Boss(
                "mb_dust_devil", "Dust Devil", "Storm Runner", 3, 900f, 23f, 18f, 6f,
                new[]
                {
                    Phase("dust_p1", "Gust", 1f, new[]
                    {
                        P("dust_dash", "Sand Dash", AttackPatternKind.ChargeDash, PatternTier.Core, 3f, 0.5f, 0.45f, 21f, speed: 20f, range: 11f),
                        P("dust_shards", "Shard Fan", AttackPatternKind.RadialBurst, PatternTier.Core, 4f, 0.7f, 0.7f, 16f, projectiles: 12, spread: 360f, range: 12f)
                    }),
                    Phase("dust_p2", "Sandstorm", 0.5f, new[]
                    {
                        P("dust_hide", "Sand Screen", AttackPatternKind.CloakAndStrike, PatternTier.Core, 4.6f, 0.6f, 1.2f, 22f, speed: 18f, range: 12f),
                        P("dust_vortex", "Vortex", AttackPatternKind.ArenaHazard, PatternTier.Extended, 7f, 0.9f, 3f, 18f, projectiles: 6, spread: 360f, range: 16f, tags: "hazard"),
                        P("dust_wall", "Wall of Sand", AttackPatternKind.SweepingBeam, PatternTier.Full, 6.5f, 1f, 2f, 21f, speed: 0f, range: 20f)
                    }, speedMultiplier: 1.1f)
                },
                isMiniBoss: true, arenaId: "arena_ch03_mb2", upgradeDrops: new[] { "g_core_reactive" },
                bestiary: "Courier who runs the rooftop gaps faster than the wind that named her."));

            // ---- Chapter 4 ----
            content.Bosses.Add(Boss(
                "mb_rangefinder", "Rangefinder", "Foundry Targeting Rig", 4, 1050f, 27f, 34f, 3.6f,
                new[]
                {
                    Phase("range_p1", "Bracket", 1f, new[]
                    {
                        P("range_mark", "Target Markers", AttackPatternKind.Minefield, PatternTier.Core, 4.6f, 0.9f, 2.4f, 22f, projectiles: 5, spread: 60f, range: 18f, tags: "hazard"),
                        P("range_lance", "Range Lance", AttackPatternKind.AimedVolley, PatternTier.Core, 3.6f, 1f, 0.5f, 30f, projectiles: 2, spread: 2f, speed: 34f, range: 24f)
                    }),
                    Phase("range_p2", "Correction", 0.5f, new[]
                    {
                        P("range_sweep", "Search Sweep", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.4f, 1f, 1.8f, 28f, speed: 0f, range: 22f),
                        P("range_battery", "Battery Fire", AttackPatternKind.MissileBarrage, PatternTier.Extended, 6f, 0.9f, 1.8f, 24f, projectiles: 6, spread: 60f, range: 20f),
                        P("range_full_bracket", "Full Bracket", AttackPatternKind.LaserGrid, PatternTier.Full, 8f, 1.1f, 2.6f, 26f, projectiles: 7, spread: 90f, range: 24f)
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch04_mb1", upgradeDrops: new[] { "g_plating_mk2", "g_weave_mk2" },
                weaponDrops: new[] { "w_breach", "w_stiletto" },
                bestiary: "Rangefinding rig that paints targets for the whole foundry. Kill it and every gun in the room gets slower."));

            content.Bosses.Add(Boss(
                "mb_lockjaw", "Lockjaw", "Press Operator", 4, 1150f, 28f, 40f, 3.4f,
                new[]
                {
                    Phase("lock_p1", "Press", 1f, new[]
                    {
                        P("lock_crush", "Hydraulic Crush", AttackPatternKind.GrappleSlam, PatternTier.Core, 4.6f, 0.85f, 0.9f, 30f, range: 12f),
                        P("lock_slam", "Floor Slam", AttackPatternKind.GroundPound, PatternTier.Core, 5f, 0.85f, 0.7f, 26f, radius: 6f, range: 8f)
                    }),
                    Phase("lock_p2", "Molten", 0.45f, new[]
                    {
                        P("lock_pour", "Molten Pour", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.8f, 1f, 2f, 30f, speed: 0f, range: 20f),
                        P("lock_sparks", "Spark Burst", AttackPatternKind.RadialBurst, PatternTier.Extended, 5.6f, 0.8f, 0.9f, 24f, projectiles: 20, spread: 360f, range: 14f),
                        P("lock_overpressure", "Overpressure", AttackPatternKind.RadialBurst, PatternTier.Full, 6.5f, 0.9f, 1f, 28f, projectiles: 22, spread: 360f, range: 16f)
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch04_mb2", upgradeDrops: new[] { "g_trinket_reactor" },
                bestiary: "A press machine with someone still inside the cab, still working the pedal."));

            // ---- Chapter 5 ----
            content.Bosses.Add(Boss(
                "mb_siren", "Lesser Siren", "Cantor of the Choir", 5, 1150f, 32f, 36f, 4f,
                new[]
                {
                    Phase("lsiren_p1", "Hymn", 1f, new[]
                    {
                        P("lsiren_note", "Piercing Note", AttackPatternKind.AimedVolley, PatternTier.Core, 3.2f, 0.8f, 0.6f, 30f, projectiles: 4, spread: 18f, speed: 18f, range: 18f),
                        P("lsiren_pulse", "Pulse Ring", AttackPatternKind.RadialBurst, PatternTier.Core, 4.4f, 0.85f, 0.9f, 28f, projectiles: 18, spread: 360f, range: 15f)
                    }),
                    Phase("lsiren_p2", "Chorus", 0.45f, new[]
                    {
                        P("lsiren_choir", "Raise the Choir", AttackPatternKind.SummonAdds, PatternTier.Core, 9f, 0.9f, 1f, 10f, projectiles: 3, range: 18f, tags: "adds"),
                        P("lsiren_wail", "Wail", AttackPatternKind.SweepingBeam, PatternTier.Extended, 5.4f, 1f, 2f, 32f, speed: 0f, range: 22f),
                        P("lsiren_requiem", "Lesser Requiem", AttackPatternKind.BeamCross, PatternTier.Full, 7f, 1f, 2f, 34f, projectiles: 3, spread: 90f, range: 22f)
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch05_mb1", upgradeDrops: new[] { "g_core_reactive" },
                weaponDrops: new[] { "w_havoc", "w_arc_caster" },
                bestiary: "A cantor half-taken by the Veil. Still singing the part the machine gave her."));

            content.Bosses.Add(Boss(
                "mb_cinder", "Cinder", "Veil Burnout", 5, 1250f, 33f, 30f, 4.6f,
                new[]
                {
                    Phase("cinder_p1", "Ignition", 1f, new[]
                    {
                        P("cinder_dash", "Cinder Dash", AttackPatternKind.ChargeDash, PatternTier.Core, 3.2f, 0.55f, 0.5f, 30f, speed: 18f, range: 12f),
                        P("cinder_fire", "Fire Fan", AttackPatternKind.AimedVolley, PatternTier.Core, 3.8f, 0.75f, 0.8f, 26f, projectiles: 7, spread: 36f, speed: 12f, range: 16f)
                    }),
                    Phase("cinder_p2", "Burnout", 0.5f, new[]
                    {
                        P("cinder_field", "Scorch Field", AttackPatternKind.ArenaHazard, PatternTier.Core, 5.6f, 0.9f, 3.2f, 26f, projectiles: 7, spread: 360f, range: 18f, tags: "hazard"),
                        P("cinder_nova", "Flare Nova", AttackPatternKind.RadialBurst, PatternTier.Extended, 5f, 0.8f, 0.9f, 28f, projectiles: 22, spread: 360f, range: 15f),
                        P("cinder_inferno", "Inferno", AttackPatternKind.ArenaHazard, PatternTier.Full, 7.5f, 0.9f, 3.4f, 30f, projectiles: 9, spread: 360f, range: 20f, tags: "hazard")
                    }, damageMultiplier: 1.1f)
                },
                isMiniBoss: true, arenaId: "arena_ch05_mb2", upgradeDrops: new[] { "g_trinket_reactor" },
                bestiary: "A heat exchange that kept one city block warm and now keeps nothing at all."));

            // ---- Chapter 6 ----
            content.Bosses.Add(Boss(
                "mb_brood_keeper", "Brood Keeper", "Keeper of the Mites", 6, 1350f, 36f, 40f, 4.2f,
                new[]
                {
                    Phase("brood_p1", "Brood", 1f, new[]
                    {
                        P("brood_spawn", "Spawn Mites", AttackPatternKind.SummonAdds, PatternTier.Core, 8f, 0.9f, 1f, 12f, projectiles: 4, range: 20f, tags: "adds"),
                        P("brood_spit", "Brood Spit", AttackPatternKind.AimedVolley, PatternTier.Core, 3.4f, 0.7f, 0.6f, 32f, projectiles: 5, spread: 26f, range: 16f)
                    }),
                    Phase("brood_p2", "Swarm Call", 0.45f, new[]
                    {
                        P("brood_storm", "Swarm Storm", AttackPatternKind.HomingSwarm, PatternTier.Core, 4.6f, 0.85f, 1.6f, 30f, projectiles: 12, speed: 12f, range: 18f),
                        P("brood_molt", "Molt", AttackPatternKind.ShieldRecharge, PatternTier.Extended, 12f, 0.9f, 2.4f, 0f, range: 12f, tags: "heal"),
                        P("brood_swarm_king", "Swarm King", AttackPatternKind.HomingSwarm, PatternTier.Full, 6.5f, 0.85f, 2f, 34f, projectiles: 18, speed: 13f, range: 20f)
                    }, spawnsAdds: true, addsPerWave: 2)
                },
                isMiniBoss: true, arenaId: "arena_ch06_mb1", upgradeDrops: new[] { "g_trinket_marksman" },
                bestiary: "Something assembled out of leftover memory that decided it was a mother."));

            content.Bosses.Add(Boss(
                "mb_static_priest", "Static Priest", "Veil Liturgist", 6, 1300f, 37f, 45f, 3.4f,
                new[]
                {
                    Phase("priest_p1", "Litany", 1f, new[]
                    {
                        P("priest_arcs", "Crackling Arcs", AttackPatternKind.BeamCross, PatternTier.Core, 4.6f, 0.9f, 1.6f, 34f, projectiles: 3, spread: 70f, range: 20f),
                        P("priest_ward", "Warding", AttackPatternKind.ReflectShield, PatternTier.Core, 9f, 0.8f, 3f, 0f, range: 12f, tags: "defensive")
                    }),
                    Phase("priest_p2", "Sermon", 0.5f, new[]
                    {
                        P("priest_grid", "Litany Grid", AttackPatternKind.LaserGrid, PatternTier.Core, 5.6f, 1f, 2.6f, 32f, projectiles: 6, spread: 90f, range: 22f),
                        P("priest_judgment", "Judgment", AttackPatternKind.SweepingBeam, PatternTier.Extended, 5.2f, 1.1f, 2f, 36f, speed: 0f, range: 24f),
                        P("priest_excommunicate", "Excommunicate", AttackPatternKind.RadialBurst, PatternTier.Full, 6.5f, 0.9f, 1.2f, 36f, projectiles: 26, spread: 360f, range: 18f)
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch06_mb2", upgradeDrops: new[] { "g_core_bastion" },
                bestiary: "A Veil priest who preaches to machines. The machines listen."));

            // ---- Chapter 7 ----
            content.Bosses.Add(Boss(
                "mb_nightglass", "Glass Twin", "Second Reflection", 7, 1450f, 40f, 45f, 6f,
                new[]
                {
                    Phase("twin_p1", "Mirror", 1f, new[]
                    {
                        P("twin_strike", "Mirror Strike", AttackPatternKind.CloakAndStrike, PatternTier.Core, 3f, 0.55f, 0.5f, 42f, speed: 20f, range: 10f),
                        P("twin_shards", "Shard Fan", AttackPatternKind.RadialBurst, PatternTier.Core, 4.2f, 0.8f, 0.8f, 36f, projectiles: 20, spread: 360f, range: 14f)
                    }),
                    Phase("twin_p2", "Crack", 0.45f, new[]
                    {
                        P("twin_clone", "Split", AttackPatternKind.PhaseBlink, PatternTier.Core, 6f, 0.4f, 2f, 0f, range: 16f, tags: "decoy"),
                        P("twin_hook", "Hook Line", AttackPatternKind.GrappleSlam, PatternTier.Extended, 5.5f, 0.8f, 0.8f, 40f, range: 14f),
                        P("twin_mirror_hall", "Mirror Hall", AttackPatternKind.ArenaHazard, PatternTier.Full, 7f, 0.6f, 3f, 40f, projectiles: 8, spread: 360f, range: 20f, tags: "hazard")
                    }, speedMultiplier: 1.1f)
                },
                isMiniBoss: true, arenaId: "arena_ch07_mb1", upgradeDrops: new[] { "g_trinket_marksman" },
                weaponDrops: new[] { "w_rotary", "w_shear" },
                bestiary: "One of Nightglass's reflections that stopped following orders."));

            content.Bosses.Add(Boss(
                "mb_hooks", "Hooks", "Butcher of the Docks", 7, 1500f, 41f, 40f, 5.4f,
                new[]
                {
                    Phase("hooks_p1", "Line Out", 1f, new[]
                    {
                        P("hooks_grapple", "Grapple", AttackPatternKind.GrappleSlam, PatternTier.Core, 4f, 0.8f, 0.9f, 44f, range: 15f),
                        P("hooks_sweep", "Chain Sweep", AttackPatternKind.TailSweep, PatternTier.Core, 3.8f, 0.6f, 0.7f, 38f, spread: 220f, range: 9f)
                    }),
                    Phase("hooks_p2", "Butcher's Work", 0.5f, new[]
                    {
                        P("hooks_hang", "Hang Them", AttackPatternKind.Minefield, PatternTier.Core, 5.4f, 0.9f, 3f, 36f, projectiles: 7, spread: 60f, range: 18f, tags: "hazard"),
                        P("hooks_frenzy", "Frenzy", AttackPatternKind.ChargeDash, PatternTier.Extended, 3.6f, 0.5f, 0.6f, 46f, speed: 20f, range: 13f),
                        P("hooks_trawl", "Trawl Line", AttackPatternKind.Minefield, PatternTier.Full, 6.5f, 0.9f, 3f, 42f, projectiles: 9, spread: 80f, range: 20f, tags: "hazard")
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch07_mb2", upgradeDrops: new[] { "g_trinket_kinetic" },
                bestiary: "Dock butcher who kept working after the Veil took the docks. The hooks are his, the hunger is not."));

            // ---- Chapter 8 ----
            content.Bosses.Add(Boss(
                "mb_ironjaw", "Ironjaw", "Alpha of the Yard", 8, 1700f, 44f, 50f, 4.2f,
                new[]
                {
                    Phase("mur_ironjaw_p1", "Alpha", 1f, new[]
                    {
                        P("mjaw_charge", "Alpha Charge", AttackPatternKind.ChargeDash, PatternTier.Core, 3.6f, 0.85f, 0.8f, 46f, speed: 17f, range: 14f),
                        P("mjaw_bite", "Bite Combo", AttackPatternKind.TailSweep, PatternTier.Core, 3.6f, 0.6f, 0.8f, 42f, spread: 220f, range: 8f)
                    }),
                    Phase("mur_ironjaw_p2", "Apex", 0.45f, new[]
                    {
                        P("mjaw_pack", "Call the Yard", AttackPatternKind.SummonAdds, PatternTier.Core, 9f, 0.9f, 1f, 14f, projectiles: 4, range: 22f, tags: "adds"),
                        P("mjaw_quake", "Quake Stomp", AttackPatternKind.GroundPound, PatternTier.Extended, 5.4f, 0.9f, 0.8f, 44f, radius: 8f, range: 10f),
                        P("mjaw_apex_fury", "Apex Fury", AttackPatternKind.ChargeDash, PatternTier.Full, 4.4f, 0.6f, 0.7f, 48f, speed: 20f, range: 15f)
                    }, speedMultiplier: 1.1f, spawnsAdds: true, addsPerWave: 2)
                },
                isMiniBoss: true, arenaId: "arena_ch08_mb1", upgradeDrops: new[] { "g_plating_mk3", "g_weave_mk3" },
                bestiary: "The biggest Hollow Hound in the yard, and the reason the yard has no other dogs."));

            content.Bosses.Add(Boss(
                "mb_hollow_choir", "Hollow Cantor", "Echo of the Choir", 8, 1650f, 45f, 48f, 4f,
                new[]
                {
                    Phase("cantor_p1", "Echo", 1f, new[]
                    {
                        P("cantor_echo", "Echo Lance", AttackPatternKind.BeamCross, PatternTier.Core, 4.4f, 0.95f, 1.8f, 46f, projectiles: 3, spread: 80f, range: 22f),
                        P("cantor_swarm", "Voice Swarm", AttackPatternKind.HomingSwarm, PatternTier.Core, 4.6f, 0.85f, 1.6f, 42f, projectiles: 10, speed: 13f, range: 20f)
                    }),
                    Phase("cantor_p2", "Dissonance", 0.45f, new[]
                    {
                        P("cantor_wall", "Voice Wall", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.8f, 1f, 2.2f, 48f, speed: 0f, range: 24f),
                        P("cantor_split", "Split Voice", AttackPatternKind.PhaseBlink, PatternTier.Extended, 8f, 0.5f, 2f, 0f, range: 18f, tags: "decoy"),
                        P("cantor_requiem", "Cantor's Requiem", AttackPatternKind.LaserGrid, PatternTier.Full, 7f, 1.05f, 2.8f, 48f, projectiles: 7, spread: 90f, range: 24f)
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch08_mb2", upgradeDrops: new[] { "g_core_bastion" },
                bestiary: "What is left of the Siren's choir, still singing a song nobody wrote down."));

            // ---- Chapter 9 ----
            content.Bosses.Add(Boss(
                "mb_drillmaster", "Drillmaster", "Vein Bore", 9, 1600f, 48f, 55f, 3.6f,
                new[]
                {
                    Phase("drill_p1", "Bore", 1f, new[]
                    {
                        P("drill_bore", "Drill Rush", AttackPatternKind.ChargeDash, PatternTier.Core, 3.4f, 0.75f, 0.75f, 52f, speed: 18f, range: 14f),
                        P("drill_slam", "Rig Slam", AttackPatternKind.GroundPound, PatternTier.Core, 5f, 0.85f, 0.7f, 46f, radius: 6.5f, range: 9f)
                    }),
                    Phase("drill_p2", "Deep Cut", 0.45f, new[]
                    {
                        P("drill_vent", "Vent Blast", AttackPatternKind.RadialBurst, PatternTier.Core, 4.6f, 0.85f, 0.9f, 46f, projectiles: 22, spread: 360f, range: 15f),
                        P("drill_beam", "Bore Beam", AttackPatternKind.SweepingBeam, PatternTier.Extended, 5.6f, 1.05f, 2.2f, 50f, speed: 0f, range: 24f),
                        P("drill_full_bore", "Full Bore", AttackPatternKind.ChargeDash, PatternTier.Full, 4.2f, 0.6f, 0.8f, 54f, speed: 21f, range: 16f)
                    }, damageMultiplier: 1.1f)
                },
                isMiniBoss: true, arenaId: "arena_ch09_mb1", upgradeDrops: new[] { "g_core_bastion" },
                weaponDrops: new[] { "w_hammerfall", "w_lobber" },
                bestiary: "Boring rig that came up through the wrong floor and kept going."));

            content.Bosses.Add(Boss(
                "mb_hound_mother", "Hound Mother", "Den Keeper", 9, 1560f, 47f, 50f, 4.4f,
                new[]
                {
                    Phase("hmother_p1", "Den", 1f, new[]
                    {
                        P("hmother_litter", "Litter Call", AttackPatternKind.SummonAdds, PatternTier.Core, 8f, 0.9f, 1f, 16f, projectiles: 5, range: 22f, tags: "adds"),
                        P("hmother_lunge", "Lunge", AttackPatternKind.ChargeDash, PatternTier.Core, 3.4f, 0.55f, 0.55f, 48f, speed: 20f, range: 12f)
                    }),
                    Phase("hmother_p2", "Fury", 0.5f, new[]
                    {
                        P("hmother_howl", "Fury Howl", AttackPatternKind.RadialBurst, PatternTier.Core, 4.8f, 0.9f, 1f, 46f, projectiles: 24, spread: 360f, range: 16f),
                        P("hmother_tail", "Tail Sweep", AttackPatternKind.TailSweep, PatternTier.Extended, 4.2f, 0.6f, 0.7f, 50f, spread: 240f, range: 8f),
                        P("hmother_den_fury", "Den Fury", AttackPatternKind.ChargeDash, PatternTier.Full, 4.4f, 0.55f, 0.7f, 50f, speed: 22f, range: 14f)
                    }, speedMultiplier: 1.12f, spawnsAdds: true, addsPerWave: 2)
                },
                isMiniBoss: true, arenaId: "arena_ch09_mb2", upgradeDrops: new[] { "g_trinket_kinetic" },
                bestiary: "The thing that makes more Hollow Hounds. Currently making three."));

            // ---- Chapter 10 ----
            content.Bosses.Add(Boss(
                "mb_tollkeeper", "Tollkeeper", "Bridge Warden", 10, 2100f, 52f, 62f, 3.4f,
                new[]
                {
                    Phase("toll_p1", "Toll", 1f, new[]
                    {
                        P("toll_wall", "Toll Wall", AttackPatternKind.ReflectShield, PatternTier.Core, 7f, 0.85f, 3f, 0f, range: 12f, tags: "defensive"),
                        P("toll_slam", "Bar Slam", AttackPatternKind.GroundPound, PatternTier.Core, 4.8f, 0.85f, 0.8f, 50f, radius: 6f, range: 9f)
                    }),
                    Phase("toll_p2", "Unpaid", 0.45f, new[]
                    {
                        P("toll_beam", "Warden Beam", AttackPatternKind.SweepingBeam, PatternTier.Core, 5.2f, 1.05f, 2.2f, 54f, speed: 0f, range: 26f),
                        P("toll_guard", "Honour Guard", AttackPatternKind.SummonAdds, PatternTier.Extended, 11f, 0.9f, 1.2f, 20f, projectiles: 3, range: 24f, tags: "adds"),
                        P("toll_seal", "Seal the Bridge", AttackPatternKind.ArenaHazard, PatternTier.Full, 8f, 1f, 3.6f, 52f, projectiles: 8, spread: 360f, range: 24f, tags: "hazard")
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch10_mb1", upgradeDrops: new[] { "g_trinket_kinetic" },
                bestiary: "Veilwarden posted to a bridge that no longer leads anywhere, still charging a toll."));

            content.Bosses.Add(Boss(
                "mb_penitent", "The Penitent", "Repentant Warden", 10, 2200f, 53f, 66f, 3.8f,
                new[]
                {
                    Phase("pen_p1", "Confession", 1f, new[]
                    {
                        P("pen_hammer", "Hammer Fall", AttackPatternKind.GrappleSlam, PatternTier.Core, 4.4f, 0.85f, 0.9f, 56f, range: 14f),
                        P("pen_ring", "Shock Ring", AttackPatternKind.GroundPound, PatternTier.Core, 5.2f, 0.9f, 0.9f, 50f, radius: 9f, range: 11f)
                    }),
                    Phase("pen_p2", "Absolution", 0.45f, new[]
                    {
                        P("pen_cross", "Cross Beam", AttackPatternKind.BeamCross, PatternTier.Core, 5f, 0.95f, 2f, 54f, projectiles: 3, spread: 90f, range: 24f),
                        P("pen_drone", "Serve the Veil", AttackPatternKind.TurretDeploy, PatternTier.Extended, 10f, 0.9f, 1.2f, 18f, projectiles: 3, range: 22f, tags: "adds"),
                        P("pen_final_oath", "Final Oath", AttackPatternKind.SweepingBeam, PatternTier.Full, 6.5f, 1f, 2.4f, 56f, speed: 0f, range: 26f)
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch10_mb2", upgradeDrops: new[] { "g_core_bastion" },
                bestiary: "A Veilwarden that broke its own oath. It fights you for the right to stop."));

            // ---- Chapter 11 ----
            content.Bosses.Add(Boss(
                "mb_gutterking", "The Gutterking", "Lord of the Undercity", 11, 2400f, 57f, 70f, 4.2f,
                new[]
                {
                    Phase("gutter_p1", "Crown", 1f, new[]
                    {
                        P("gutter_charge", "Throne Charge", AttackPatternKind.ChargeDash, PatternTier.Core, 3.4f, 0.7f, 0.8f, 60f, speed: 18f, range: 15f),
                        P("gutter_retinue", "Retinue", AttackPatternKind.SummonAdds, PatternTier.Core, 9f, 0.9f, 1.2f, 22f, projectiles: 4, range: 24f, tags: "adds")
                    }),
                    Phase("gutter_p2", "Undercity", 0.45f, new[]
                    {
                        P("gutter_grenades", "Sewer Charges", AttackPatternKind.Minefield, PatternTier.Core, 5f, 0.9f, 3.2f, 48f, projectiles: 10, spread: 360f, range: 22f, tags: "hazard"),
                        P("gutter_volley", "Gutter Volley", AttackPatternKind.MissileBarrage, PatternTier.Extended, 5.4f, 0.9f, 2f, 52f, projectiles: 9, spread: 90f, range: 24f),
                        P("gutter_crown_rage", "Crown Rage", AttackPatternKind.ChargeDash, PatternTier.Full, 4.2f, 0.6f, 0.8f, 62f, speed: 20f, range: 16f)
                    }, damageMultiplier: 1.12f, spawnsAdds: true, addsPerWave: 3)
                },
                isMiniBoss: true, arenaId: "arena_ch11_mb1", upgradeDrops: new[] { "g_plating_mk4", "g_weave_mk4" },
                weaponDrops: new[] { "w_bulwark_cannon", "w_verdict" },
                bestiary: "The Directorate left the undercity to whoever could hold it. This is who could hold it."));

            content.Bosses.Add(Boss(
                "mb_signal_adjunct", "Signal Adjunct", "Voice of the Office", 11, 2300f, 56f, 68f, 3.8f,
                new[]
                {
                    Phase("adj_p1", "Memo", 1f, new[]
                    {
                        P("adj_lance", "Directive Lance", AttackPatternKind.SweepingBeam, PatternTier.Core, 5f, 1f, 2.2f, 58f, speed: 0f, range: 28f),
                        P("adj_shift", "Reassign", AttackPatternKind.PhaseBlink, PatternTier.Core, 6f, 0.4f, 1.6f, 0f, range: 20f)
                    }),
                    Phase("adj_p2", "Requisition", 0.45f, new[]
                    {
                        P("adj_grid", "Directive Grid", AttackPatternKind.LaserGrid, PatternTier.Core, 5.6f, 1.05f, 2.8f, 54f, projectiles: 8, spread: 90f, range: 26f),
                        P("adj_barrage", "Requisition Fire", AttackPatternKind.MissileBarrage, PatternTier.Extended, 5.4f, 0.9f, 2.2f, 52f, projectiles: 9, spread: 90f, range: 24f),
                        P("adj_final_memo", "Final Memo", AttackPatternKind.SweepingBeam, PatternTier.Full, 6.6f, 1f, 2.4f, 58f, speed: 0f, range: 28f)
                    })
                },
                isMiniBoss: true, arenaId: "arena_ch11_mb2", upgradeDrops: new[] { "g_trinket_reactor" },
                bestiary: "The Conductor's second voice: the part of the Directorate that only ever wrote memos."));

            // ---- Chapter 12 ----
            content.Bosses.Add(Boss(
                "mb_veil_hand", "Veil Hand", "Curator's Reach", 12, 2500f, 60f, 72f, 5f,
                new[]
                {
                    Phase("hand_p1", "Reach", 1f, new[]
                    {
                        P("hand_grasp", "Archive Grasp", AttackPatternKind.GrappleSlam, PatternTier.Core, 4.2f, 0.8f, 1f, 64f, range: 16f),
                        P("hand_shards", "Record Shards", AttackPatternKind.MissileBarrage, PatternTier.Core, 4.6f, 0.9f, 2f, 58f, projectiles: 10, spread: 100f, range: 24f)
                    }),
                    Phase("hand_p2", "Withdraw", 0.45f, new[]
                    {
                        P("hand_pulse", "Null Pulse", AttackPatternKind.RadialBurst, PatternTier.Core, 4.8f, 0.9f, 1.1f, 60f, projectiles: 30, spread: 360f, range: 18f),
                        P("hand_lances", "Lance Fan", AttackPatternKind.BeamCross, PatternTier.Extended, 5.6f, 0.9f, 2f, 62f, projectiles: 4, spread: 90f, range: 26f),
                        P("hand_close", "Closing Fist", AttackPatternKind.GrappleSlam, PatternTier.Full, 5.4f, 0.8f, 1f, 66f, range: 18f)
                    }, damageMultiplier: 1.1f)
                },
                isMiniBoss: true, arenaId: "arena_ch12_mb1", upgradeDrops: new[] { "g_trinket_kinetic" },
                bestiary: "A limb of the Veil, severed and still reaching for the nearest archive."));

            content.Bosses.Add(Boss(
                "mb_last_archivist", "The Last Archivist", "Keeper of Names", 12, 2600f, 62f, 75f, 4.2f,
                new[]
                {
                    Phase("arch_p1", "Catalogue", 1f, new[]
                    {
                        P("arch_beam", "Catalogue Beam", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.8f, 1f, 2.4f, 64f, speed: 0f, range: 30f),
                        P("arch_shields", "Index Shields", AttackPatternKind.ReflectShield, PatternTier.Core, 8f, 0.85f, 3.4f, 0f, range: 14f, tags: "defensive")
                    }),
                    Phase("arch_p2", "Erase", 0.45f, new[]
                    {
                        P("arch_erase", "Erase", AttackPatternKind.ArenaHazard, PatternTier.Core, 6f, 1.1f, 3.6f, 62f, projectiles: 10, spread: 360f, range: 26f, tags: "hazard"),
                        P("arch_names", "Screaming Names", AttackPatternKind.HomingSwarm, PatternTier.Extended, 5.4f, 0.9f, 2f, 60f, projectiles: 16, speed: 14f, range: 24f),
                        P("arch_final_entry", "Final Entry", AttackPatternKind.BeamCross, PatternTier.Full, 6.4f, 0.95f, 2.2f, 64f, projectiles: 4, spread: 90f, range: 28f)
                    }, damageMultiplier: 1.12f)
                },
                isMiniBoss: true, arenaId: "arena_ch12_mb2", upgradeDrops: new[] { "g_core_bastion" },
                bestiary: "The last person who catalogued the Veil's dead. They know every name in the building, including yours."));
        }
    }
}
