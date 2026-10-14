using System;
using System.Collections.Generic;

namespace IronVeil.Rules
{
    /// <summary>
    /// The shipped campaign, authored as code. Fifteen hundred lines of data beat
    /// fifteen thousand of hand-edited ScriptableObject YAML: it diffs, it is
    /// version controlled, and every reference is validated by ContentValidator.
    /// </summary>
    public static partial class Campaign
    {
        public const string Kade = "kade";
        public const string Vera = "vera";

        public static CampaignContent Build()
        {
            CampaignContent content = new CampaignContent
            {
                Title = "IRON VEIL",
                Subtitle = "A run-and-gun war for the last signal",
                PlotSummary =
                    "The Helion Directorate sealed the sky over the coastal states with the Iron Veil, " +
                    "a machine that turns harvested memory into obedient light. Two survivors of the " +
                    "program that built it are going back in: Kade Rourke, who carried the door charges, " +
                    "and Vera Voss, who drew the maps that condemned the cities. Twelve chapters, " +
                    "one signal to break, and the person who is still keeping the Veil alive."
            };

            BuildCharacters(content);
            BuildWeapons(content);
            BuildGear(content);
            BuildPerks(content);
            BuildDropTables(content);
            BuildEnemies(content);
            BuildBosses(content);
            BuildQuests(content);
            BuildChapters(content);
            BuildEncounters(content);

            content.Index();
            return content;
        }

        /// <summary>Validates the shipped campaign. Used by tests and the editor tool.</summary>
        public static ValidationReport Validate()
        {
            return ContentValidator.Validate(Build());
        }

        // ------------------------------------------------------------------
        // Characters
        // ------------------------------------------------------------------

        private static void BuildCharacters(CampaignContent content)
        {
            // ---- Kade "Anvil" Rourke: breach gunner. Trades mobility for armour. ----
            CharacterDefinition kade = new CharacterDefinition
            {
                Id = CharacterId.Kade,
                Name = "Kade Rourke",
                Callsign = "Anvil",
                Role = "Breach Gunner",
                Bio = "Eleven years planting door charges for the Directorate's Cleansing Division. " +
                      "He carried the charges that opened the Meridian shelter, and he has been " +
                      "carrying the sound of it ever since. Kade shoots like a demolition crew: slowly, " +
                      "loudly, and always through the wall.",
                BaseStats = new StatBlock
                {
                    Power = 22f,
                    Defense = 15f,
                    Shield = 20f,
                    ShieldRegen = 5f,
                    MaxHealth = 150f,
                    FireRateMul = 0.98f,
                    CritChance = 0.04f,
                    CritDamageMul = 1.7f,
                    MoveSpeed = 6.2f,
                    AbilityPower = 1f,
                    ReloadSpeedMul = 0.98f,
                    Luck = 0f
                },
                // Per level growth is authored as deltas: anything left out does not grow.
                PerLevelGrowth = Growth(power: 1.7f, defense: 1.25f, shield: 1.2f, maxHealth: 9f,
                    moveSpeed: 0.02f, shieldRegen: 0.08f, luck: 0.2f),
                SignatureWeaponId = "w_repeater",
                StartingWeaponIds = new[] { "w_repeater", "w_scattergun" },
                UnlockableWeaponIds = new[] { "w_breach", "w_havoc", "w_rotary", "w_hammerfall", "w_bulwark_cannon" },
                UnlockableGearIds = new[]
                {
                    "g_plating_mk1", "g_plating_mk2", "g_plating_mk3", "g_plating_mk4",
                    "g_core_basic", "g_core_reactive", "g_core_bastion"
                },
                PrimaryAbility = new AbilityDefinition
                {
                    Id = "ab_bulwark",
                    DisplayName = "Bulwark",
                    Description = "Deploys a directional barrier that absorbs incoming fire for 6 seconds.",
                    Kind = AbilityKind.Bulwark,
                    Activation = AbilityActivation.Deployable,
                    Cooldown = 14f,
                    Duration = 6f,
                    Radius = 2.6f,
                    ShieldAmount = 220f,
                    Tags = new[] { "defensive", "deployable" }
                },
                SecondaryAbility = new AbilityDefinition
                {
                    Id = "ab_groundslam",
                    DisplayName = "Ground Slam",
                    Description = "Slams the floor for area damage and briefly stuns everything nearby.",
                    Kind = AbilityKind.GroundSlam,
                    Activation = AbilityActivation.Instant,
                    Cooldown = 9f,
                    Radius = 5.5f,
                    Damage = 90f,
                    PowerScaling = 0.6f,
                    StunSeconds = 1.1f,
                    Tags = new[] { "aoe", "stun" }
                },
                Passive = new PassiveDefinition
                {
                    Id = "pas_reinforced",
                    DisplayName = "Reinforced",
                    Description = "Takes 20% less damage while braced (standing still for half a second).",
                    Kind = PassiveKind.Reinforced,
                    DamageTakenMultiplier = 0.8f,
                    StationaryThreshold = 0.5f
                },
                SecondAbilityUnlockChapter = 3,
                PaletteId = "kade",
                LevelUpBarks = new[]
                {
                    "More rounds. More armour. Same door.",
                    "They built the Veil. I built the way through it.",
                    "Every plate I bolt on is one they have to get past."
                },
                LowHealthBarks = new[] { "Still standing. Barely.", "Not yet. Not here." },
                BossIntroLines = new[] { "Open it.", "I have charges left." },
                Tags = new[] { "heavy", "defensive", "sustained" }
            };

            // ---- Vera "Wisp" Voss: scout skirmisher. Fragile, fast, precise. ----
            CharacterDefinition vera = new CharacterDefinition
            {
                Id = CharacterId.Vera,
                Name = "Vera Voss",
                Callsign = "Wisp",
                Role = "Pathfinder Skirmisher",
                Bio = "The Directorate's best cartographer until she mapped the wrong street and " +
                      "learned what the Veil does to the people inside it. Vera fights the way she " +
                      "drew: fast lines, no wasted strokes, and always three exits planned.",
                BaseStats = new StatBlock
                {
                    Power = 25f,
                    Defense = 8f,
                    Shield = 45f,
                    ShieldRegen = 9f,
                    MaxHealth = 115f,
                    FireRateMul = 1.1f,
                    CritChance = 0.09f,
                    CritDamageMul = 1.85f,
                    MoveSpeed = 7.3f,
                    AbilityPower = 1.05f,
                    ReloadSpeedMul = 1.06f,
                    Luck = 4f
                },
                // Per level growth is authored as deltas: anything left out does not grow.
                PerLevelGrowth = Growth(power: 1.8f, defense: 0.9f, shield: 2.2f, maxHealth: 7f,
                    critChance: 0.0012f, moveSpeed: 0.03f, shieldRegen: 0.16f, luck: 1.2f),
                SignatureWeaponId = "w_needle",
                StartingWeaponIds = new[] { "w_needle", "w_burstline" },
                UnlockableWeaponIds = new[] { "w_stiletto", "w_arc_caster", "w_lobber", "w_shear", "w_verdict" },
                UnlockableGearIds = new[]
                {
                    "g_weave_mk1", "g_weave_mk2", "g_weave_mk3", "g_weave_mk4",
                    "g_core_basic", "g_core_reactive", "g_core_bastion"
                },
                PrimaryAbility = new AbilityDefinition
                {
                    Id = "ab_phaseblink",
                    DisplayName = "Phase Blink",
                    Description = "Dashes through enemies and bullets, leaving a decoy that draws fire.",
                    Kind = AbilityKind.PhaseBlink,
                    Activation = AbilityActivation.Dash,
                    Cooldown = 4.5f,
                    Distance = 7.5f,
                    Duration = 2.5f,
                    InvulnerabilitySeconds = 0.35f,
                    ShieldAmount = 30f,
                    Tags = new[] { "mobility", "evasion", "decoy" }
                },
                SecondaryAbility = new AbilityDefinition
                {
                    Id = "ab_overclock",
                    DisplayName = "Overclock",
                    Description = "Overdrives her weapon rig for 5 seconds: +60% fire rate and no reload.",
                    Kind = AbilityKind.Overclock,
                    Activation = AbilityActivation.SelfBuff,
                    Cooldown = 16f,
                    Duration = 5f,
                    FireRateBonus = 0.6f,
                    Tags = new[] { "buff", "offensive" }
                },
                Passive = new PassiveDefinition
                {
                    Id = "pas_momentum",
                    DisplayName = "Momentum",
                    Description = "Consecutive hits stack +3% damage each, up to +30%. Missing resets the stack.",
                    Kind = PassiveKind.Momentum,
                    PerStackBonus = 0.03f,
                    MaxStacks = 10,
                    StackDecaySeconds = 3f
                },
                SecondAbilityUnlockChapter = 3,
                PaletteId = "vera",
                LevelUpBarks = new[]
                {
                    "Faster lines. Cleaner exits.",
                    "I already know where this corridor ends.",
                    "Keep pace."
                },
                LowHealthBarks = new[] { "One more step.", "I have an exit." },
                BossIntroLines = new[] { "I have your map.", "You should have moved." },
                Tags = new[] { "agile", "precision", "glass_cannon" }
            };

            content.Characters.Add(kade);
            content.Characters.Add(vera);
        }

        // ------------------------------------------------------------------
        // Authoring helpers (kept short so the content files stay readable)
        // ------------------------------------------------------------------

        internal static PatternDefinition P(
            string id,
            string name,
            AttackPatternKind kind,
            PatternTier tier,
            float cooldown,
            float telegraph,
            float duration,
            float damage,
            float weight = 1f,
            int projectiles = 1,
            float spread = 0f,
            float speed = 9f,
            float range = 14f,
            float radius = 3f,
            string hint = null,
            params string[] tags)
        {
            return new PatternDefinition
            {
                Id = id,
                DisplayName = name,
                Kind = kind,
                RequiredTier = tier,
                Weight = weight,
                Cooldown = cooldown,
                Telegraph = telegraph,
                Duration = duration,
                Recovery = 0.35f + duration * 0.4f,
                Damage = damage,
                Speed = speed,
                ProjectileCount = projectiles,
                SpreadAngle = spread,
                Range = range,
                Radius = radius,
                EasyHint = hint ?? string.Empty,
                Tags = tags ?? Array.Empty<string>()
            };
        }

        internal static BossPhaseDefinition Phase(
            string id,
            string name,
            float threshold,
            PatternDefinition[] patterns,
            float cooldownMultiplier = 1f,
            float speedMultiplier = 1f,
            float damageMultiplier = 1f,
            float incomingDamageMultiplier = 1f,
            bool spawnsAdds = false,
            int addsPerWave = 0,
            string announcement = null)
        {
            return new BossPhaseDefinition
            {
                Id = id,
                DisplayName = name,
                EnterAtHealthRatio = threshold,
                CooldownMultiplier = cooldownMultiplier,
                SpeedMultiplier = speedMultiplier,
                DamageMultiplier = damageMultiplier,
                IncomingDamageMultiplier = incomingDamageMultiplier,
                SpawnsAdds = spawnsAdds,
                AddsPerWave = addsPerWave,
                Announcement = announcement ?? string.Empty,
                Patterns = new List<PatternDefinition>(patterns ?? Array.Empty<PatternDefinition>())
            };
        }

        internal static BossDefinition Boss(
            string id,
            string name,
            string title,
            int chapter,
            float health,
            float damage,
            float defense,
            float speed,
            BossPhaseDefinition[] phases,
            bool isMiniBoss = false,
            bool isFinalBoss = false,
            string arenaId = null,
            int weakPoints = 1,
            string[] upgradeDrops = null,
            string[] weaponDrops = null,
            string musicId = null,
            string introComicId = null,
            string outroComicId = null,
            string bestiary = null,
            string[] resistances = null,
            float enrageAfter = 0f)
        {
            return new BossDefinition
            {
                Id = id,
                DisplayName = name,
                Title = title,
                ChapterIndex = chapter,
                IsMiniBoss = isMiniBoss,
                IsFinalBoss = isFinalBoss,
                ArenaId = arenaId ?? string.Empty,
                BaseHealth = health,
                BaseDamage = damage,
                BaseDefense = defense,
                BaseSpeed = speed,
                WeakPointCount = weakPoints,
                Phases = new List<BossPhaseDefinition>(phases ?? Array.Empty<BossPhaseDefinition>()),
                UpgradeDrops = new List<string>(upgradeDrops ?? Array.Empty<string>()),
                WeaponDrops = new List<string>(weaponDrops ?? Array.Empty<string>()),
                MusicId = musicId ?? string.Empty,
                IntroComicId = introComicId ?? string.Empty,
                OutroComicId = outroComicId ?? string.Empty,
                BestiaryEntry = bestiary ?? string.Empty,
                Resistances = resistances ?? Array.Empty<string>(),
                EnrageAfterSeconds = enrageAfter,
                EnrageCooldownMultiplier = 0.65f
            };
        }

        internal static WeaponDefinition Weapon(
            string id,
            string name,
            WeaponKind kind,
            FireMode mode,
            float damage,
            float roundsPerSecond,
            int magazine,
            float reload,
            CharacterId exclusiveTo,
            int tier,
            string description,
            float powerScaling = 1f,
            int pellets = 1,
            float spread = 2f,
            float armorPierce = 0f,
            int ammoPerPickup = 45,
            int reserve = 360,
            float projectileSpeed = 26f,
            float range = 22f,
            bool pierces = false,
            int burstCount = 1,
            float burstInterval = 0.06f,
            float chargeTime = 0f,
            string unlockSource = null,
            string[] tags = null,
            int upgradeCost = 120)
        {
            return new WeaponDefinition
            {
                Id = id,
                DisplayName = name,
                Description = description,
                Kind = kind,
                Mode = mode,
                ExclusiveTo = exclusiveTo,
                DamagePerShot = damage,
                PowerScaling = powerScaling,
                ArmorPierce = armorPierce,
                RoundsPerSecond = roundsPerSecond,
                PelletsPerShot = pellets,
                SpreadDegrees = spread,
                MagazineSize = magazine,
                ReloadTime = reload,
                BurstCount = burstCount,
                BurstInterval = burstInterval,
                ChargeTime = chargeTime,
                AmmoPerPickup = ammoPerPickup,
                MaxReserveAmmo = reserve,
                ProjectileSpeed = projectileSpeed,
                Range = range,
                Pierces = pierces,
                Tier = tier,
                UnlockSource = unlockSource ?? string.Empty,
                Tags = tags ?? Array.Empty<string>(),
                UpgradeSalvageCost = upgradeCost
            };
        }

        internal static GearDefinition Gear(
            string id,
            string name,
            EquipSlot slot,
            Rarity rarity,
            int tier,
            int chapterAvailable,
            string description,
            StatBlock bonus,
            string droppedBy = null,
            bool characterSpecific = false,
            CharacterId exclusiveTo = CharacterId.None,
            string[] tags = null)
        {
            return new GearDefinition
            {
                Id = id,
                DisplayName = name,
                Description = description,
                Slot = slot,
                Rarity = rarity,
                Tier = tier,
                ChapterAvailable = chapterAvailable,
                Bonus = bonus,
                DroppedBy = droppedBy ?? string.Empty,
                IsCharacterSpecific = characterSpecific,
                ExclusiveTo = exclusiveTo,
                Tags = tags ?? Array.Empty<string>()
            };
        }

        internal static StatBlock Bonus(
            float power = 0f,
            float defense = 0f,
            float shield = 0f,
            float maxHealth = 0f,
            float fireRate = 0f,
            float critChance = 0f,
            float critDamage = 0f,
            float moveSpeed = 0f,
            float abilityPower = 0f,
            float reloadSpeed = 0f,
            float luck = 0f,
            float shieldRegen = 0f)
        {
            // Bonuses are deltas: 0 means "no change" even for the multiplier stats.
            StatBlock block = StatBlock.Delta();
            block.Power = power;
            block.Defense = defense;
            block.Shield = shield;
            block.ShieldRegen = shieldRegen;
            block.MaxHealth = maxHealth;
            block.FireRateMul = fireRate;
            block.CritChance = critChance;
            block.CritDamageMul = critDamage;
            block.MoveSpeed = moveSpeed;
            block.AbilityPower = abilityPower;
            block.ReloadSpeedMul = reloadSpeed;
            block.Luck = luck;
            return block;
        }

        /// <summary>
        /// Per-level growth block. Starts from zero deltas, so leaving a field out
        /// means "this stat does not grow" for both flat and multiplier stats.
        /// </summary>
        internal static StatBlock Growth(
            float power = 0f,
            float defense = 0f,
            float shield = 0f,
            float shieldRegen = 0f,
            float maxHealth = 0f,
            float fireRate = 0f,
            float critChance = 0f,
            float critDamage = 0f,
            float moveSpeed = 0f,
            float abilityPower = 0f,
            float reloadSpeed = 0f,
            float luck = 0f)
        {
            StatBlock growth = StatBlock.Delta();
            growth.Power = power;
            growth.Defense = defense;
            growth.Shield = shield;
            growth.ShieldRegen = shieldRegen;
            growth.MaxHealth = maxHealth;
            growth.FireRateMul = fireRate;
            growth.CritChance = critChance;
            growth.CritDamageMul = critDamage;
            growth.MoveSpeed = moveSpeed;
            growth.AbilityPower = abilityPower;
            growth.ReloadSpeedMul = reloadSpeed;
            growth.Luck = luck;
            return growth;
        }

        internal static EnemyArchetype Enemy(
            string id,
            string name,
            EnemyClass enemyClass,
            int tier,
            int firstChapter,
            float health,
            float damage,
            float defense,
            float speed,
            float attackRange,
            float attackCooldown,
            float telegraph,
            string bestiary,
            PatternDefinition[] attacks,
            bool canFly = false,
            bool eliteCandidate = false,
            bool support = false,
            float detectionRange = 14f,
            int projectileCount = 1,
            float projectileSpeed = 11f,
            string[] vulnerabilities = null,
            string[] resistances = null,
            bool canBeStaggered = true)
        {
            return new EnemyArchetype
            {
                Id = id,
                DisplayName = name,
                Class = enemyClass,
                Tier = tier,
                FirstChapter = firstChapter,
                BaseHealth = health,
                BaseDamage = damage,
                BaseDefense = defense,
                MoveSpeed = speed,
                AttackRange = attackRange,
                AttackCooldown = attackCooldown,
                Telegraph = telegraph,
                DetectionRange = detectionRange,
                CanFly = canFly,
                IsEliteCandidate = eliteCandidate,
                IsSupport = support,
                ProjectileCount = projectileCount,
                ProjectileSpeed = projectileSpeed,
                Vulnerabilities = vulnerabilities ?? Array.Empty<string>(),
                Resistances = resistances ?? Array.Empty<string>(),
                Bestiary = bestiary,
                Attacks = new List<PatternDefinition>(attacks ?? Array.Empty<PatternDefinition>()),
                CanBeStaggered = canBeStaggered,
                WeakPointMultiplier = 1.75f
            };
        }

        internal static QuestObjective Objective(
            ObjectiveType type,
            string targetId,
            int requiredCount,
            string description,
            bool optional = false)
        {
            return new QuestObjective
            {
                Id = type + "_" + (string.IsNullOrEmpty(targetId) ? "any" : targetId),
                Type = type,
                TargetId = targetId,
                RequiredCount = requiredCount,
                Description = description,
                Optional = optional
            };
        }

        internal static QuestDefinition Quest(
            string id,
            string title,
            QuestKind kind,
            int chapter,
            string giver,
            string locationHint,
            string summary,
            QuestObjective[] objectives,
            int xpReward,
            int salvageReward,
            int estimatedMinutes,
            string[] prerequisiteQuestIds = null,
            string[] prerequisiteFlags = null,
            string[] grantedFlags = null,
            string gearRewardId = null,
            string weaponRewardId = null,
            string perkRewardId = null,
            string turnInName = null,
            bool repeatable = false)
        {
            return new QuestDefinition
            {
                Id = id,
                Title = title,
                Kind = kind,
                ChapterIndex = chapter,
                GiverName = giver,
                LocationHint = locationHint,
                Summary = summary,
                Objectives = new List<QuestObjective>(objectives ?? Array.Empty<QuestObjective>()),
                XpReward = xpReward,
                SalvageReward = salvageReward,
                EstimatedMinutes = estimatedMinutes,
                PrerequisiteQuestIds = prerequisiteQuestIds ?? Array.Empty<string>(),
                PrerequisiteFlags = prerequisiteFlags ?? Array.Empty<string>(),
                GrantedFlags = grantedFlags ?? Array.Empty<string>(),
                GearRewardId = gearRewardId ?? string.Empty,
                WeaponRewardId = weaponRewardId ?? string.Empty,
                PerkRewardId = perkRewardId ?? string.Empty,
                TurnInName = turnInName ?? giver,
                IsRepeatable = repeatable
            };
        }
    }
}
