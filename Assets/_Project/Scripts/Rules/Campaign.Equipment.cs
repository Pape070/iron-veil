using System.Collections.Generic;

namespace IronVeil.Rules
{
    public static partial class Campaign
    {
        private static void BuildWeapons(CampaignContent content)
        {
            // ---------------- Kade: weight of fire ----------------
            content.Weapons.Add(Weapon(
                "w_repeater", "Repeater", WeaponKind.Rifle, FireMode.Automatic,
                11f, 6.5f, 24, 1.7f, CharacterId.Kade, 1,
                "Standard Directorate rifle, rebuilt with a heavier bolt. Reliable, unremarkable, always loaded.",
                tags: new[] { "starter", "reliable" }));

            content.Weapons.Add(Weapon(
                "w_scattergun", "Scattergun", WeaponKind.Shotgun, FireMode.SemiAutomatic,
                6.5f, 1.4f, 6, 1.9f, CharacterId.Kade, 1,
                "Six shells of break-the-door. Devastating in a corridor, useless at range.",
                pellets: 6, spread: 12f, ammoPerPickup: 18, reserve: 90,
                tags: new[] { "starter", "close_range" }));

            content.Weapons.Add(Weapon(
                "w_breach", "Breach Cutter", WeaponKind.Shotgun, FireMode.SemiAutomatic,
                8.5f, 1.5f, 8, 2f, CharacterId.Kade, 2,
                "Cutting charges repurposed into a choke. Shreds plating instead of doors.",
                pellets: 7, spread: 10f, armorPierce: 0.15f, ammoPerPickup: 20, reserve: 110,
                unlockSource: "Chapter 4 mini-boss - Rangefinder",
                tags: new[] { "close_range", "armor_pierce" }));

            content.Weapons.Add(Weapon(
                "w_havoc", "Havoc Launcher", WeaponKind.RocketLauncher, FireMode.SemiAutomatic,
                62f, 0.9f, 4, 2.4f, CharacterId.Kade, 2,
                "Shoulder rocket pod. Wide blast, slow hands, punishes crowds.",
                spread: 1.5f, ammoPerPickup: 6, reserve: 30, projectileSpeed: 18f,
                unlockSource: "Chapter 5 mini-boss - Lesser Siren",
                tags: new[] { "explosive", "aoe" }));

            content.Weapons.Add(Weapon(
                "w_rotary", "Widowmaker Rotary", WeaponKind.RotaryCannon, FireMode.Automatic,
                8f, 11f, 90, 3.4f, CharacterId.Kade, 3,
                "Six barrels and a coolant line that screams. Hold the trigger, walk the stream.",
                spread: 4.5f, ammoPerPickup: 90, reserve: 540,
                unlockSource: "Chapter 7 mini-boss - Glass Twin",
                tags: new[] { "suppression", "spin_up" }));

            content.Weapons.Add(Weapon(
                "w_hammerfall", "Hammerfall", WeaponKind.RocketLauncher, FireMode.Charge,
                150f, 0.55f, 3, 3f, CharacterId.Kade, 3,
                "Siege cannon. One round, one room, one very short conversation.",
                spread: 0.5f, armorPierce: 0.3f, ammoPerPickup: 4, reserve: 20,
                projectileSpeed: 16f, chargeTime: 1.1f,
                unlockSource: "Chapter 9 mini-boss - Drillmaster",
                tags: new[] { "explosive", "heavy" }));

            content.Weapons.Add(Weapon(
                "w_bulwark_cannon", "Bulwark Cannon", WeaponKind.Lmg, FireMode.Automatic,
                17f, 9.5f, 120, 4.2f, CharacterId.Kade, 4,
                "Belt-fed and bolted to a brace plate. The Directorate's answer to holding a street.",
                spread: 3.2f, ammoPerPickup: 120, reserve: 720,
                unlockSource: "Chapter 11 mini-boss - The Gutterking",
                tags: new[] { "sustained", "heavy" }));

            // ---------------- Vera: reach and precision ----------------
            content.Weapons.Add(Weapon(
                "w_needle", "Needle", WeaponKind.RailPistol, FireMode.SemiAutomatic,
                19f, 3.6f, 12, 1.3f, CharacterId.Vera, 1,
                "Rail pistol. Punches through light plating and never asks twice.",
                spread: 0.8f, armorPierce: 0.2f, ammoPerPickup: 36, reserve: 300,
                projectileSpeed: 40f,
                tags: new[] { "starter", "precision" }));

            content.Weapons.Add(Weapon(
                "w_burstline", "Burstline", WeaponKind.Smg, FireMode.Burst,
                7.5f, 9f, 30, 1.5f, CharacterId.Vera, 1,
                "Three-round burst rig. Tight, quick, forgiving of a moving target.",
                spread: 2.2f, burstCount: 3, burstInterval: 0.06f, ammoPerPickup: 60, reserve: 380,
                tags: new[] { "starter", "burst" }));

            content.Weapons.Add(Weapon(
                "w_stiletto", "Stiletto", WeaponKind.Smg, FireMode.Automatic,
                8.5f, 12f, 36, 1.6f, CharacterId.Vera, 2,
                "Machine pistol built for cartographers who stopped drawing and started running.",
                spread: 3.6f, ammoPerPickup: 70, reserve: 420,
                unlockSource: "Chapter 4 mini-boss - Rangefinder",
                tags: new[] { "fast" }));

            content.Weapons.Add(Weapon(
                "w_arc_caster", "Arc Caster", WeaponKind.ArcEmitter, FireMode.Automatic,
                13f, 4.2f, 20, 1.8f, CharacterId.Vera, 2,
                "Veil-tech emitter. The beam links targets and does not care about cover.",
                spread: 1.2f, armorPierce: 0.3f, pierces: true, ammoPerPickup: 30, reserve: 200,
                projectileSpeed: 34f,
                unlockSource: "Chapter 5 mini-boss - Lesser Siren",
                tags: new[] { "energy", "pierce" }));

            content.Weapons.Add(Weapon(
                "w_lobber", "Lobber", WeaponKind.GrenadeLauncher, FireMode.SemiAutomatic,
                52f, 1.2f, 6, 2.2f, CharacterId.Vera, 3,
                "Arcing grenade launcher. Reach over the wall you were never meant to cross.",
                spread: 2f, ammoPerPickup: 12, reserve: 60, projectileSpeed: 17f,
                unlockSource: "Chapter 9 mini-boss - Drillmaster",
                tags: new[] { "explosive", "arcing" }));

            content.Weapons.Add(Weapon(
                "w_shear", "Shear", WeaponKind.Rifle, FireMode.Automatic,
                10f, 12.5f, 45, 1.9f, CharacterId.Vera, 3,
                "Prototype line rifle. Fire rate of a suggestion, accuracy of a decision.",
                spread: 1.6f, ammoPerPickup: 90, reserve: 500,
                unlockSource: "Chapter 7 mini-boss - Glass Twin",
                tags: new[] { "precision", "fast" }));

            content.Weapons.Add(Weapon(
                "w_verdict", "Verdict", WeaponKind.RailPistol, FireMode.Charge,
                150f, 1f, 5, 2.4f, CharacterId.Vera, 4,
                "Charged rail spike. Fires a line of white through whatever the Directorate built.",
                spread: 0.2f, armorPierce: 0.5f, pierces: true, ammoPerPickup: 8, reserve: 40,
                projectileSpeed: 60f, chargeTime: 0.9f,
                unlockSource: "Chapter 11 mini-boss - The Gutterking",
                tags: new[] { "precision", "heavy", "charge" }));
        }

        private static void BuildGear(CampaignContent content)
        {
            // ---- Kade: plating ----
            content.GearItems.Add(Gear("g_plating_mk1", "Breacher Plate", EquipSlot.Armor, Rarity.Common, 1, 1,
                "Salvaged blast plate. Stops the first thing that hits you.",
                Bonus(defense: 6f, maxHealth: 20f), characterSpecific: true, exclusiveTo: CharacterId.Kade));
            content.GearItems.Add(Gear("g_plating_mk2", "Reinforced Plate", EquipSlot.Armor, Rarity.Common, 2, 4,
                "Layered plating with a ceramic strike face.",
                Bonus(defense: 15f, maxHealth: 48f, shield: 10f), droppedBy: "mb_rangefinder",
                characterSpecific: true, exclusiveTo: CharacterId.Kade));
            content.GearItems.Add(Gear("g_plating_mk3", "Assault Plate", EquipSlot.Armor, Rarity.Rare, 3, 8,
                "Powered shoulder rig that spreads impact across the whole frame.",
                Bonus(defense: 26f, maxHealth: 85f, shield: 20f, power: 4f), droppedBy: "mb_ironjaw",
                characterSpecific: true, exclusiveTo: CharacterId.Kade));
            content.GearItems.Add(Gear("g_plating_mk4", "Veilbreaker Plate", EquipSlot.Armor, Rarity.Epic, 4, 11,
                "Veil-alloy shell. The last thing the Directorate issued him, and the last thing he kept.",
                Bonus(defense: 40f, maxHealth: 125f, shield: 35f, power: 8f, luck: 5f), droppedBy: "mb_gutterking",
                characterSpecific: true, exclusiveTo: CharacterId.Kade));

            // ---- Vera: weave ----
            content.GearItems.Add(Gear("g_weave_mk1", "Pathfinder Weave", EquipSlot.Armor, Rarity.Common, 1, 1,
                "Light mapping coat with a shielding thread.",
                Bonus(shield: 18f, maxHealth: 10f, moveSpeed: 0.3f), characterSpecific: true, exclusiveTo: CharacterId.Vera));
            content.GearItems.Add(Gear("g_weave_mk2", "Skirmish Weave", EquipSlot.Armor, Rarity.Common, 2, 4,
                "Jointless cloth armour that follows the dodge instead of fighting it.",
                Bonus(shield: 34f, maxHealth: 22f, moveSpeed: 0.5f, critChance: 0.02f, defense: 6f),
                droppedBy: "mb_rangefinder", characterSpecific: true, exclusiveTo: CharacterId.Vera));
            content.GearItems.Add(Gear("g_weave_mk3", "Ghost Weave", EquipSlot.Armor, Rarity.Rare, 3, 8,
                "Phase-threaded weave that flickers when she moves.",
                Bonus(shield: 58f, maxHealth: 40f, moveSpeed: 0.7f, critChance: 0.04f, defense: 11f),
                droppedBy: "mb_hollow_choir", characterSpecific: true, exclusiveTo: CharacterId.Vera));
            content.GearItems.Add(Gear("g_weave_mk4", "Verdict Weave", EquipSlot.Armor, Rarity.Epic, 4, 11,
                "Her own design, stitched from Veil filament. Nothing slows her down.",
                Bonus(shield: 88f, maxHealth: 62f, moveSpeed: 1f, critChance: 0.07f, critDamage: 0.15f, defense: 16f),
                droppedBy: "mb_gutterking", characterSpecific: true, exclusiveTo: CharacterId.Vera));

            // ---- Shield cores (shared) ----
            content.GearItems.Add(Gear("g_core_basic", "Field Core", EquipSlot.ShieldCore, Rarity.Common, 1, 1,
                "Standard regeneration core. Keeps a shield up between firefights.",
                Bonus(shield: 25f, shieldRegen: 3f)));
            content.GearItems.Add(Gear("g_core_reactive", "Reactive Core", EquipSlot.ShieldCore, Rarity.Rare, 2, 5,
                "Reactive core that dumps its charge into the first hit it takes.",
                Bonus(shield: 65f, shieldRegen: 6f, defense: 4f), droppedBy: "mb_siren"));
            content.GearItems.Add(Gear("g_core_bastion", "Bastion Core", EquipSlot.ShieldCore, Rarity.Epic, 3, 9,
                "Veil-grade core. Holds a shield long enough to walk into fire.",
                Bonus(shield: 120f, shieldRegen: 9f, defense: 12f), droppedBy: "mb_drillmaster"));

            // ---- Trinkets (shared) ----
            content.GearItems.Add(Gear("g_trinket_sprinter", "Sprinter Coil", EquipSlot.Trinket, Rarity.Common, 1, 3,
                "Coil trinket that shaves dead weight off every step.",
                Bonus(moveSpeed: 0.6f, reloadSpeed: 0.08f)));
            content.GearItems.Add(Gear("g_trinket_salvager", "Salvager's Token", EquipSlot.Trinket, Rarity.Common, 2, 2,
                "Directorate scrip slug that always finds more of itself.",
                Bonus(luck: 14f), tags: new[] { "loot" }));
            content.GearItems.Add(Gear("g_trinket_reactor", "Micro Reactor", EquipSlot.Trinket, Rarity.Rare, 3, 6,
                "Hand reactor that overdrives both weapon and ability.",
                Bonus(fireRate: 0.07f, abilityPower: 0.12f, reloadSpeed: 0.06f)));
            content.GearItems.Add(Gear("g_trinket_marksman", "Marksman Lens", EquipSlot.Trinket, Rarity.Rare, 3, 7,
                "Range lens that turns a good line into a critical one.",
                Bonus(critChance: 0.05f, critDamage: 0.18f)));
            content.GearItems.Add(Gear("g_trinket_kinetic", "Kinetic Sink", EquipSlot.Trinket, Rarity.Epic, 4, 10,
                "Dampens incoming force into stored shield charge.",
                Bonus(defense: 18f, shield: 45f, shieldRegen: 4f), droppedBy: "mb_tollkeeper"));
        }

        private static void BuildPerks(CampaignContent content)
        {
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_pack_rat",
                DisplayName = "Pack Rat",
                Description = "Enemies drop 15% more salvage.",
                UnlockLevel = 8,
                EffectKey = "salvage_bonus",
                EffectValue = 0.15f
            });
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_second_wind",
                DisplayName = "Second Wind",
                Description = "Once per checkpoint, survive a lethal hit at 25% health.",
                UnlockLevel = 16,
                EffectKey = "cheat_death",
                EffectValue = 0.25f
            });
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_scavenger",
                DisplayName = "Scavenger",
                Description = "Ammo pickups also restore 6% shield.",
                UnlockLevel = 22,
                Bonus = Bonus(shieldRegen: 2f),
                EffectKey = "ammo_shield",
                EffectValue = 0.06f
            });
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_veteran",
                DisplayName = "Veteran Instinct",
                Description = "Gain 10% damage and defence after every third consecutive chapter clear.",
                UnlockLevel = 30,
                Bonus = Bonus(power: 6f, defense: 6f),
                EffectKey = "chapter_streak",
                EffectValue = 0.1f
            });
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_marksman_focus",
                DisplayName = "Focus Fire",
                Description = "Weak point hits refund 15% of the ability cooldown.",
                UnlockLevel = 36,
                EffectKey = "weakpoint_cooldown",
                EffectValue = 0.15f
            });
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_iron_will",
                DisplayName = "Iron Will",
                Description = "Shield regeneration continues while taking fire, at half rate.",
                UnlockLevel = 44,
                Bonus = Bonus(shieldRegen: 4f),
                EffectKey = "shield_under_fire",
                EffectValue = 0.5f
            });

            // Kade-only
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_kade_anchor",
                DisplayName = "Anchor",
                Description = "Braced damage reduction rises to 32% and reloads cannot be interrupted while braced.",
                UnlockLevel = 20,
                ExclusiveTo = CharacterId.Kade,
                Bonus = Bonus(defense: 10f),
                EffectKey = "brace_upgrade",
                EffectValue = 0.32f
            });
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_kade_concussive",
                DisplayName = "Concussive Rounds",
                Description = "Ground Slam stuns for a further 0.8 seconds.",
                UnlockLevel = 34,
                ExclusiveTo = CharacterId.Kade,
                EffectKey = "slam_stun",
                EffectValue = 0.8f
            });
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_kade_demolition",
                DisplayName = "Demolitionist",
                Description = "Explosive weapons deal 20% more damage with a wider blast.",
                UnlockLevel = 48,
                ExclusiveTo = CharacterId.Kade,
                Bonus = Bonus(power: 12f),
                EffectKey = "explosive_bonus",
                EffectValue = 0.2f
            });

            // Vera-only
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_vera_slipstream",
                DisplayName = "Slipstream",
                Description = "Phase Blink leaves a shockwave that staggers enemies at the exit point.",
                UnlockLevel = 20,
                ExclusiveTo = CharacterId.Vera,
                Bonus = Bonus(moveSpeed: 0.4f),
                EffectKey = "blink_shockwave",
                EffectValue = 45f
            });
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_vera_perfect_line",
                DisplayName = "Perfect Line",
                Description = "Momentum stacks persist for 6 seconds instead of 3.",
                UnlockLevel = 34,
                ExclusiveTo = CharacterId.Vera,
                Bonus = Bonus(critChance: 0.03f),
                EffectKey = "momentum_duration",
                EffectValue = 6f
            });
            content.Perks.Add(new PerkDefinition
            {
                Id = "perk_vera_overdraw",
                DisplayName = "Overdraw",
                Description = "Overclock also reloads instantly and adds 20% crit chance for its duration.",
                UnlockLevel = 48,
                ExclusiveTo = CharacterId.Vera,
                Bonus = Bonus(critDamage: 0.1f),
                EffectKey = "overclock_crit",
                EffectValue = 0.2f
            });
        }

        private static void BuildDropTables(CampaignContent content)
        {
            content.DropTables.Add(new DropTable
            {
                Id = "dt_elite_early",
                SalvageMin = 12,
                SalvageMax = 30,
                Entries = new List<DropEntry>
                {
                    new DropEntry { GearId = "g_trinket_sprinter", Weight = 3f },
                    new DropEntry { GearId = "g_trinket_salvager", Weight = 2f },
                    new DropEntry { GearId = "g_core_basic", Weight = 3f }
                }
            });

            content.DropTables.Add(new DropTable
            {
                Id = "dt_elite_mid",
                SalvageMin = 30,
                SalvageMax = 70,
                Entries = new List<DropEntry>
                {
                    new DropEntry { GearId = "g_core_reactive", Weight = 3f },
                    new DropEntry { GearId = "g_trinket_reactor", Weight = 2f },
                    new DropEntry { GearId = "g_trinket_marksman", Weight = 2f },
                    new DropEntry { GearId = "g_plating_mk2", Weight = 1f },
                    new DropEntry { GearId = "g_weave_mk2", Weight = 1f }
                }
            });

            content.DropTables.Add(new DropTable
            {
                Id = "dt_elite_late",
                SalvageMin = 70,
                SalvageMax = 150,
                Entries = new List<DropEntry>
                {
                    new DropEntry { GearId = "g_core_bastion", Weight = 3f },
                    new DropEntry { GearId = "g_trinket_kinetic", Weight = 2f },
                    new DropEntry { GearId = "g_plating_mk3", Weight = 1f },
                    new DropEntry { GearId = "g_weave_mk3", Weight = 1f }
                }
            });

            content.DropTables.Add(new DropTable
            {
                Id = "dt_arena_chest",
                SalvageMin = 25,
                SalvageMax = 60,
                Entries = new List<DropEntry>
                {
                    new DropEntry { GearId = "g_core_basic", Weight = 2f },
                    new DropEntry { GearId = "g_trinket_sprinter", Weight = 2f },
                    new DropEntry { GearId = "g_trinket_salvager", Weight = 2f },
                    new DropEntry { GearId = "g_core_reactive", Weight = 1f }
                }
            });

            content.DropTables.Add(new DropTable
            {
                Id = "dt_chapter_boss",
                SalvageMin = 180,
                SalvageMax = 320,
                Entries = new List<DropEntry>
                {
                    new DropEntry { GearId = "g_core_reactive", Weight = 2f },
                    new DropEntry { GearId = "g_core_bastion", Weight = 2f },
                    new DropEntry { GearId = "g_trinket_reactor", Weight = 2f },
                    new DropEntry { GearId = "g_trinket_kinetic", Weight = 1f }
                }
            });
        }
    }
}
