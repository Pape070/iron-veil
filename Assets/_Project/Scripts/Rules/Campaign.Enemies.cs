namespace IronVeil.Rules
{
    public static partial class Campaign
    {
        private static void BuildEnemies(CampaignContent content)
        {
            content.Enemies.Add(Enemy(
                "e_rust_drone", "Rust Drone", EnemyClass.Drone, 1, 1,
                38f, 5f, 0f, 3.4f, 6f, 1.5f, 0.5f,
                "A maintenance drone pressed into patrol duty. Fragile, blind, and built in the thousands.",
                new[]
                {
                    P("ed_buzz", "Buzz Shot", AttackPatternKind.AimedVolley, PatternTier.Core, 1.6f, 0.5f, 0.2f, 5f, speed: 10f, range: 8f)
                },
                canFly: true));
            content.Enemies.Add(Enemy(
                "e_line_trooper", "Line Trooper", EnemyClass.Grunt, 1, 1,
                55f, 8f, 6f, 3f, 8f, 1.7f, 0.55f,
                "The Directorate's standard infantry. Trained to advance behind a shield wall and never stop firing.",
                new[]
                {
                    P("eg_rifle_burst", "Rifle Burst", AttackPatternKind.AimedVolley, PatternTier.Core, 1.8f, 0.55f, 0.3f, 8f, projectiles: 3, spread: 8f, speed: 12f, range: 10f),
                    P("eg_suppress", "Suppressing Fire", AttackPatternKind.AimedVolley, PatternTier.Extended, 4.5f, 0.7f, 0.9f, 6f, projectiles: 6, spread: 14f, speed: 12f, range: 10f)
                },
                eliteCandidate: true));
            content.Enemies.Add(Enemy(
                "e_breach_hound", "Breach Hound", EnemyClass.Rusher, 1, 1,
                42f, 11f, 2f, 6.4f, 1.6f, 1.2f, 0.35f,
                "Bred to run at a door and keep running through it. Fast, blind with hunger, and always in threes.",
                new[]
                {
                    P("eh_lunge", "Lunge", AttackPatternKind.ChargeDash, PatternTier.Core, 2.2f, 0.35f, 0.5f, 11f, speed: 16f, range: 6f),
                    P("eh_pack_howl", "Pack Howl", AttackPatternKind.SummonAdds, PatternTier.Extended, 12f, 0.9f, 0.6f, 1f, range: 18f, tags: "adds")
                },
                vulnerabilities: new[] { "explosive" }));

            content.Enemies.Add(Enemy(
                "e_sentinel_turret", "Sentinel Turret", EnemyClass.Turret, 2, 2,
                120f, 13f, 25f, 0f, 14f, 1.4f, 0.6f,
                "Ceiling-mounted autocannon. Does not retreat, does not reload, does not miss twice.",
                new[]
                {
                    P("et_sweep", "Sweep Fire", AttackPatternKind.SweepingBeam, PatternTier.Core, 2.6f, 0.6f, 1.2f, 13f, speed: 0f, range: 16f),
                    P("et_lock", "Lock On", AttackPatternKind.HomingSwarm, PatternTier.Extended, 7f, 0.9f, 0.8f, 10f, projectiles: 4, speed: 9f, range: 16f)
                },
                resistances: new[] { "knockback" }));
            content.Enemies.Add(Enemy(
                "e_skiff", "Skiff", EnemyClass.Flyer, 2, 2,
                95f, 11f, 4f, 4.6f, 7f, 1.6f, 0.5f,
                "Cheap grav-skiff with a door gunner. Circles, strafes, and always keeps the gun pointed in.",
                new[]
                {
                    P("es_strafe", "Strafing Run", AttackPatternKind.AimedVolley, PatternTier.Core, 2.4f, 0.5f, 0.7f, 9f, projectiles: 5, spread: 10f, speed: 13f, range: 10f),
                    P("es_dive", "Dive Bomb", AttackPatternKind.ChargeDash, PatternTier.Extended, 6f, 0.75f, 0.6f, 14f, speed: 18f, range: 8f)
                },
                canFly: true));

            content.Enemies.Add(Enemy(
                "e_longwatch", "Longwatch", EnemyClass.Sniper, 2, 3,
                85f, 24f, 6f, 2.6f, 22f, 3.4f, 1.35f,
                "Marksman on a rooftop with a rail rifle and a comms officer telling them where you are.",
                new[]
                {
                    P("el_railshot", "Rail Shot", AttackPatternKind.AimedVolley, PatternTier.Core, 3.6f, 1.35f, 0.25f, 24f, speed: 42f, range: 26f, hint: "Sniper laser - break line of sight"),
                    P("el_relocate", "Relocate", AttackPatternKind.PhaseBlink, PatternTier.Extended, 6.5f, 0.4f, 0.5f, 1f, range: 20f)
                },
                vulnerabilities: new[] { "explosive" }));
            content.Enemies.Add(Enemy(
                "e_bulwark_trooper", "Bulwark Trooper", EnemyClass.Shielder, 2, 3,
                150f, 10f, 45f, 2.2f, 6f, 2f, 0.7f,
                "Trooper behind a full-body riot plate. Flank them or bring something that goes through armour.",
                new[]
                {
                    P("eb_shield_bash", "Shield Bash", AttackPatternKind.ChargeDash, PatternTier.Core, 3f, 0.7f, 0.4f, 10f, speed: 12f, range: 5f),
                    P("eb_shield_wall", "Shield Wall", AttackPatternKind.ReflectShield, PatternTier.Extended, 9f, 0.8f, 2.5f, 0f, range: 8f, tags: "defensive")
                },
                eliteCandidate: true,
                vulnerabilities: new[] { "armor_pierce", "energy" }));

            content.Enemies.Add(Enemy(
                "e_toxin_crawler", "Toxin Crawler", EnemyClass.Spitter, 3, 4,
                140f, 15f, 8f, 3.4f, 9f, 2.1f, 0.65f,
                "Hollow-touched crawler that vomits the Veil's runoff. The puddles do not dry.",
                new[]
                {
                    P("ec_spit", "Acid Spit", AttackPatternKind.AimedVolley, PatternTier.Core, 2.2f, 0.65f, 0.35f, 15f, projectiles: 2, spread: 12f, speed: 11f, range: 11f),
                    P("ec_minefield", "Runoff", AttackPatternKind.Minefield, PatternTier.Extended, 8f, 0.8f, 3f, 9f, projectiles: 5, spread: 60f, range: 8f, tags: "hazard")
                }));
            content.Enemies.Add(Enemy(
                "e_ironjaw_brute", "Ironjaw Brute", EnemyClass.Brute, 3, 4,
                260f, 22f, 30f, 2.8f, 3.2f, 2.6f, 0.95f,
                "Veil-program meat with a hydraulic jaw. Its charge flattens cover and people.",
                new[]
                {
                    P("ebr_charge", "Gore Charge", AttackPatternKind.ChargeDash, PatternTier.Core, 3.4f, 0.95f, 0.7f, 22f, speed: 15f, range: 9f, hint: "He braces before the charge - sidestep it"),
                    P("ebr_pound", "Seismic Pound", AttackPatternKind.GroundPound, PatternTier.Extended, 6.5f, 0.8f, 0.6f, 18f, radius: 5f, range: 5f),
                    P("ebr_roar", "Rally Roar", AttackPatternKind.SummonAdds, PatternTier.Full, 14f, 1.1f, 0.8f, 1f, range: 16f, tags: "adds")
                },
                eliteCandidate: true,
                canBeStaggered: false));

            content.Enemies.Add(Enemy(
                "e_mortar_team", "Mortar Team", EnemyClass.Bombardier, 3, 5,
                130f, 26f, 10f, 2.4f, 18f, 4.2f, 1.5f,
                "Two soldiers and a tube. They never aim at you, only at where you will be.",
                new[]
                {
                    P("em_lob", "Lobbed Shell", AttackPatternKind.MissileBarrage, PatternTier.Core, 4.2f, 1.5f, 0.9f, 26f, projectiles: 3, spread: 30f, speed: 8f, range: 20f, hint: "Shells land where you were standing"),
                    P("em_barrage", "Walking Barrage", AttackPatternKind.MissileBarrage, PatternTier.Extended, 9f, 1.2f, 1.6f, 20f, projectiles: 6, spread: 60f, speed: 8f, range: 20f)
                },
                vulnerabilities: new[] { "explosive" }));
            content.Enemies.Add(Enemy(
                "e_field_chaplain", "Field Chaplain", EnemyClass.Medic, 3, 5,
                115f, 9f, 12f, 3.2f, 12f, 2.4f, 0.6f,
                "Veil priest with a repair litany. Kill them first or fight everything twice.",
                new[]
                {
                    P("ef_mend", "Litany of Repair", AttackPatternKind.ShieldRecharge, PatternTier.Core, 5f, 0.8f, 1.4f, 0f, range: 12f, tags: "heal"),
                    P("ef_ward", "Warding Hymn", AttackPatternKind.ReflectShield, PatternTier.Extended, 11f, 0.9f, 3.5f, 0f, range: 12f, tags: "buff")
                },
                support: true,
                resistances: new[] { "physical" }));

            content.Enemies.Add(Enemy(
                "e_memory_mites", "Memory Mites", EnemyClass.Swarm, 3, 6,
                34f, 8f, 0f, 6f, 1.4f, 1.1f, 0.3f,
                "Fragments of harvested memory with teeth. They arrive as dust and leave as nothing.",
                new[]
                {
                    P("emi_bite", "Swarm Bite", AttackPatternKind.AimedVolley, PatternTier.Core, 1.2f, 0.3f, 0.2f, 8f, speed: 14f, range: 3f),
                    P("emi_split", "Shed Skin", AttackPatternKind.SummonAdds, PatternTier.Extended, 15f, 0.6f, 0.4f, 1f, range: 10f, tags: "adds")
                },
                vulnerabilities: new[] { "aoe", "explosive" },
                canBeStaggered: false));

            content.Enemies.Add(Enemy(
                "e_veil_stalker", "Veil Stalker", EnemyClass.Assassin, 4, 7,
                210f, 28f, 18f, 6.8f, 2.6f, 2.6f, 0.55f,
                "It wears a dead person's outline. It blinks, it strikes, it is elsewhere when your rounds arrive.",
                new[]
                {
                    P("ev_blink_strike", "Blink Strike", AttackPatternKind.CloakAndStrike, PatternTier.Core, 3.2f, 0.55f, 0.5f, 28f, speed: 20f, range: 8f, hint: "Watch the shimmer before it commits"),
                    P("ev_shadow_clone", "Shadow Clone", AttackPatternKind.PhaseBlink, PatternTier.Extended, 7f, 0.4f, 1.5f, 0f, range: 12f, tags: "decoy"),
                    P("ev_gut_hook", "Hook Step", AttackPatternKind.GrappleSlam, PatternTier.Full, 10f, 0.8f, 0.9f, 24f, range: 12f)
                },
                eliteCandidate: true));
            content.Enemies.Add(Enemy(
                "e_siege_walker", "Siege Walker", EnemyClass.Artillery, 4, 8,
                330f, 30f, 38f, 1.9f, 24f, 3.8f, 1.5f,
                "Four legs, one gun, no hurry. If you hear the second clank, move.",
                new[]
                {
                    P("ew_beam", "Siege Beam", AttackPatternKind.SweepingBeam, PatternTier.Core, 4f, 1.5f, 1.6f, 30f, speed: 0f, range: 24f, hint: "Beam sweeps left to right - go under it"),
                    P("ew_grid", "Laser Grid", AttackPatternKind.LaserGrid, PatternTier.Extended, 9f, 1.2f, 2.4f, 24f, projectiles: 7, spread: 90f, range: 24f),
                    P("ew_walkers", "Deploy Drones", AttackPatternKind.TurretDeploy, PatternTier.Full, 15f, 1f, 1.2f, 6f, projectiles: 2, range: 20f, tags: "adds")
                },
                resistances: new[] { "knockback" },
                canBeStaggered: false));

            content.Enemies.Add(Enemy(
                "e_hollow_hound", "Hollow Hound", EnemyClass.Beast, 4, 8,
                240f, 27f, 12f, 7.2f, 2.2f, 1.8f, 0.45f,
                "Something the Veil made out of a guard dog and a promise. Runs in twos, hunts in packs of six.",
                new[]
                {
                    P("eho_pounce", "Pounce", AttackPatternKind.ChargeDash, PatternTier.Core, 2.6f, 0.45f, 0.5f, 27f, speed: 18f, range: 9f),
                    P("eho_tail_sweep", "Tail Sweep", AttackPatternKind.TailSweep, PatternTier.Extended, 5.5f, 0.6f, 0.5f, 19f, spread: 180f, range: 5f),
                    P("eho_hunt_call", "Hunt Call", AttackPatternKind.SummonAdds, PatternTier.Full, 16f, 0.9f, 0.6f, 1f, range: 20f, tags: "adds")
                },
                eliteCandidate: true));
            content.Enemies.Add(Enemy(
                "e_echo_wisp", "Echo Wisp", EnemyClass.Flyer, 4, 9,
                150f, 20f, 8f, 5.4f, 10f, 2.2f, 0.5f,
                "A recording of someone's last minute, still repeating. It homes in on warmth.",
                new[]
                {
                    P("ee_homing", "Echo Volley", AttackPatternKind.HomingSwarm, PatternTier.Core, 3f, 0.5f, 0.6f, 20f, projectiles: 4, speed: 10f, range: 14f),
                    P("ee_scream", "Static Scream", AttackPatternKind.RadialBurst, PatternTier.Extended, 8f, 0.8f, 0.7f, 16f, projectiles: 12, spread: 360f, speed: 9f, range: 10f)
                },
                canFly: true));

            content.Enemies.Add(Enemy(
                "e_veilguard", "Veilguard", EnemyClass.Shielder, 5, 10,
                430f, 32f, 60f, 2.4f, 7f, 2.2f, 0.8f,
                "The Directorate's honour guard, wrapped in the same alloy as the Veil itself.",
                new[]
                {
                    P("evg_wall", "Advancing Wall", AttackPatternKind.ReflectShield, PatternTier.Core, 6f, 0.8f, 3f, 0f, range: 10f, tags: "defensive", hint: "Reflects front damage - get behind"),
                    P("evg_slam", "Vehicle Slam", AttackPatternKind.GroundPound, PatternTier.Extended, 5f, 0.9f, 0.7f, 26f, radius: 5.5f, range: 6f),
                    P("evg_cross", "Cross Beam", AttackPatternKind.BeamCross, PatternTier.Full, 11f, 1.1f, 1.8f, 30f, projectiles: 2, spread: 90f, range: 18f)
                },
                eliteCandidate: true,
                vulnerabilities: new[] { "armor_pierce" }));
            content.Enemies.Add(Enemy(
                "e_signal_conductor", "Signal Conductor", EnemyClass.Brute, 5, 11,
                520f, 34f, 55f, 3f, 5f, 2.8f, 1f,
                "A person the Veil kept, wearing the antennae it grew into them. They still give orders.",
                new[]
                {
                    P("esc_direct", "Direct the Choir", AttackPatternKind.SummonAdds, PatternTier.Core, 8f, 1f, 0.9f, 4f, projectiles: 3, range: 20f, tags: "adds"),
                    P("esc_feedback", "Feedback Wave", AttackPatternKind.RadialBurst, PatternTier.Extended, 5f, 1f, 0.8f, 28f, projectiles: 16, spread: 360f, speed: 11f, range: 12f),
                    P("esc_chain", "Chain Lightning", AttackPatternKind.BeamCross, PatternTier.Full, 10f, 0.9f, 1.4f, 32f, projectiles: 3, spread: 60f, range: 18f)
                },
                canBeStaggered: false));
            content.Enemies.Add(Enemy(
                "e_veil_engine", "Veil Engine Node", EnemyClass.Turret, 5, 10,
                560f, 34f, 70f, 0f, 26f, 2.6f, 1.1f,
                "A socket of the Iron Veil itself. It does not want you dead, it wants you archived.",
                new[]
                {
                    P("eve_archive_beam", "Archive Beam", AttackPatternKind.SweepingBeam, PatternTier.Core, 4.4f, 1.1f, 2f, 34f, speed: 0f, range: 26f),
                    P("eve_radial", "Null Pulse", AttackPatternKind.RadialBurst, PatternTier.Extended, 6.5f, 1f, 0.9f, 30f, projectiles: 20, spread: 360f, speed: 12f, range: 16f),
                    P("eve_harvest", "Harvest Protocol", AttackPatternKind.ArenaHazard, PatternTier.Full, 13f, 1.4f, 4f, 26f, projectiles: 6, spread: 360f, range: 24f, tags: "hazard")
                },
                resistances: new[] { "knockback", "physical" },
                canBeStaggered: false));
        }
    }
}
