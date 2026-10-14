using System;
using System.Collections.Generic;
using System.Text;

namespace IronVeil.Rules
{
    public sealed class ValidationReport
    {
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();

        public bool IsValid => Errors.Count == 0;

        public void Error(string message) => Errors.Add(message);

        public void Warn(string message) => Warnings.Add(message);

        public string Summary(int maxLines = 60)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(IsValid ? "CONTENT OK" : "CONTENT INVALID");
            builder.Append(" - errors: ").Append(Errors.Count);
            builder.Append(", warnings: ").Append(Warnings.Count);

            int emitted = 0;
            for (int i = 0; i < Errors.Count && emitted < maxLines; i++, emitted++)
            {
                builder.Append("\n  ERROR   ").Append(Errors[i]);
            }
            for (int i = 0; i < Warnings.Count && emitted < maxLines; i++, emitted++)
            {
                builder.Append("\n  WARNING ").Append(Warnings[i]);
            }
            if (emitted < Errors.Count + Warnings.Count)
            {
                builder.Append("\n  ... ").Append(Errors.Count + Warnings.Count - emitted).Append(" more");
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// Enforces the design contract. These are the invariants that make the
    /// campaign shippable: twelve chapters each with a boss and pre-boss savepoint,
    /// every boss solvable on Easy, mini-boss upgrades for side content, and
    /// combat math that a player at the recommended level can actually survive.
    /// </summary>
    public static class ContentValidator
    {
        /// <summary>Minimum total main-path minutes (20 hours).</summary>
        public const int MinCampaignMinutes = 20 * 60;

        /// <summary>Maximum total main-path minutes (30 hours).</summary>
        public const int MaxCampaignMinutes = 30 * 60;

        public static ValidationReport Validate(CampaignContent content)
        {
            ValidationReport report = new ValidationReport();
            if (content == null)
            {
                report.Error("content is null");
                return report;
            }

            content.Index();

            ValidateCharacters(content, report);
            ValidateWeapons(content, report);
            ValidateGear(content, report);
            ValidateEnemies(content, report);
            ValidateBosses(content, report);
            ValidateQuests(content, report);
            ValidateChapters(content, report);
            ValidateEncounters(content, report);
            ValidateDifficultyReachability(content, report);
            ValidateBalance(content, report);

            return report;
        }

        // ---------------- characters ----------------

        private static void ValidateCharacters(CampaignContent content, ValidationReport report)
        {
            if (content.Characters.Count != 2)
            {
                report.Error("expected exactly 2 playable characters, found " + content.Characters.Count);
            }

            for (int i = 0; i < content.Characters.Count; i++)
            {
                CharacterDefinition character = content.Characters[i];
                if (character == null)
                {
                    report.Error("character entry " + i + " is null");
                    continue;
                }

                string prefix = "character '" + character.Id + "': ";
                if (character.Id == CharacterId.None) report.Error(prefix + "invalid id");
                if (string.IsNullOrEmpty(character.Name)) report.Error(prefix + "missing name");
                if (string.IsNullOrEmpty(character.Callsign)) report.Error(prefix + "missing callsign");
                if (string.IsNullOrEmpty(character.Bio)) report.Warn(prefix + "empty bio");
                if (character.BaseStats == null) report.Error(prefix + "missing base stats");
                if (character.PerLevelGrowth == null) report.Error(prefix + "missing per-level growth");
                if (character.BaseStats != null && character.BaseStats.MaxHealth < 100f)
                {
                    report.Error(prefix + "base health " + character.BaseStats.MaxHealth + " is too low to survive combat");
                }
                if (character.PerLevelGrowth != null && character.PerLevelGrowth.MaxHealth <= 0f)
                {
                    report.Error(prefix + "no per-level health growth");
                }

                // Weapon loadouts: starting weapon, signature weapon and unlocks must resolve.
                int loadout = 0;
                if (character.StartingWeaponIds == null || character.StartingWeaponIds.Length == 0)
                {
                    report.Error(prefix + "has no starting weapon");
                }
                else
                {
                    for (int w = 0; w < character.StartingWeaponIds.Length; w++)
                    {
                        WeaponDefinition weapon = content.Weapon(character.StartingWeaponIds[w]);
                        if (weapon == null) report.Error(prefix + "starting weapon '" + character.StartingWeaponIds[w] + "' does not exist");
                        else if (weapon.ExclusiveTo != CharacterId.None && weapon.ExclusiveTo != character.Id)
                        {
                            report.Error(prefix + "starting weapon '" + weapon.Id + "' belongs to another character");
                        }
                        else loadout++;
                    }
                }

                if (string.IsNullOrEmpty(character.SignatureWeaponId) || content.Weapon(character.SignatureWeaponId) == null)
                {
                    report.Error(prefix + "signature weapon '" + character.SignatureWeaponId + "' does not exist");
                }

                if (character.UnlockableWeaponIds != null)
                {
                    for (int w = 0; w < character.UnlockableWeaponIds.Length; w++)
                    {
                        WeaponDefinition weapon = content.Weapon(character.UnlockableWeaponIds[w]);
                        if (weapon == null)
                        {
                            report.Error(prefix + "unlockable weapon '" + character.UnlockableWeaponIds[w] + "' does not exist");
                            continue;
                        }
                        if (weapon.ExclusiveTo != CharacterId.None && weapon.ExclusiveTo != character.Id)
                        {
                            report.Error(prefix + "unlockable weapon '" + weapon.Id + "' belongs to another character");
                        }
                        loadout++;
                    }
                }

                // Each character needs a full arsenal: weapons plus their armour upgrades.
                if (loadout < 6) report.Error(prefix + "only " + loadout + " weapons available; at least 6 are required");

                if (character.UnlockableGearIds == null || character.UnlockableGearIds.Length == 0)
                {
                    report.Error(prefix + "has no unlockable gear");
                }
                else
                {
                    for (int g = 0; g < character.UnlockableGearIds.Length; g++)
                    {
                        GearDefinition gear = content.Gear(character.UnlockableGearIds[g]);
                        if (gear == null) report.Error(prefix + "gear '" + character.UnlockableGearIds[g] + "' does not exist");
                        else if (gear.IsCharacterSpecific && gear.ExclusiveTo != character.Id)
                        {
                            report.Error(prefix + "gear '" + gear.Id + "' belongs to another character");
                        }
                    }
                }

                ValidateAbility(character.PrimaryAbility, prefix + "primary ability", report);
                ValidateAbility(character.SecondaryAbility, prefix + "secondary ability", report);
                if (character.Passive == null || character.Passive.Kind == PassiveKind.None)
                {
                    report.Error(prefix + "has no passive ability");
                }
            }

            // The two characters must play differently.
            if (content.Characters.Count == 2)
            {
                CharacterDefinition a = content.Characters[0];
                CharacterDefinition b = content.Characters[1];
                if (a.PrimaryAbility.Kind == b.PrimaryAbility.Kind)
                {
                    report.Error("both characters share the same primary ability; kits must differ");
                }
                if (a.Passive.Kind == b.Passive.Kind)
                {
                    report.Error("both characters share the same passive");
                }
                if (string.Equals(a.SignatureWeaponId, b.SignatureWeaponId, StringComparison.OrdinalIgnoreCase))
                {
                    report.Error("both characters share the same signature weapon");
                }
            }
        }

        private static void ValidateAbility(AbilityDefinition ability, string prefix, ValidationReport report)
        {
            if (ability == null || ability.Kind == AbilityKind.None)
            {
                report.Error(prefix + " is missing");
                return;
            }
            if (string.IsNullOrEmpty(ability.DisplayName)) report.Error(prefix + " has no display name");
            if (ability.Cooldown <= 0f) report.Error(prefix + " has a non-positive cooldown");
            if (ability.Damage <= 0f && ability.ShieldAmount <= 0f && ability.FireRateBonus <= 0f && ability.Distance <= 0f)
            {
                report.Error(prefix + " has no effect (no damage, shield, buff or dash)");
            }
        }

        // ---------------- weapons ----------------

        private static void ValidateWeapons(CampaignContent content, ValidationReport report)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < content.Weapons.Count; i++)
            {
                WeaponDefinition weapon = content.Weapons[i];
                if (weapon == null)
                {
                    report.Error("weapon entry " + i + " is null");
                    continue;
                }

                string prefix = "weapon '" + weapon.Id + "': ";
                if (string.IsNullOrEmpty(weapon.Id)) report.Error("weapon entry " + i + " has no id");
                else if (!seen.Add(weapon.Id)) report.Error("duplicate weapon id '" + weapon.Id + "'");

                if (string.IsNullOrEmpty(weapon.DisplayName)) report.Error(prefix + "missing display name");
                if (weapon.DamagePerShot <= 0f) report.Error(prefix + "damage per shot must be positive");
                if (weapon.RoundsPerSecond <= 0f) report.Error(prefix + "rounds per second must be positive");
                if (weapon.PelletsPerShot < 1) report.Error(prefix + "must fire at least one pellet");
                if (weapon.MagazineSize < 1) report.Error(prefix + "magazine size must be at least 1");
                if (weapon.ReloadTime < 0f) report.Error(prefix + "negative reload time");
                if (weapon.PowerScaling < 0f || weapon.PowerScaling > 1f) report.Error(prefix + "power scaling must be within 0..1");
                if (weapon.Tier < 1 || weapon.Tier > 4) report.Error(prefix + "tier " + weapon.Tier + " outside 1..4");
                if (weapon.MaxUpgradeLevel < 1) report.Error(prefix + "must allow at least one upgrade level");
                if (weapon.MaxReserveAmmo < weapon.MagazineSize) report.Error(prefix + "reserve ammo is smaller than the magazine");
            }
        }

        // ---------------- gear ----------------

        private static void ValidateGear(CampaignContent content, ValidationReport report)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> slotsFilled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < content.GearItems.Count; i++)
            {
                GearDefinition gear = content.GearItems[i];
                if (gear == null)
                {
                    report.Error("gear entry " + i + " is null");
                    continue;
                }

                string prefix = "gear '" + gear.Id + "': ";
                if (string.IsNullOrEmpty(gear.Id)) report.Error("gear entry " + i + " has no id");
                else if (!seen.Add(gear.Id)) report.Error("duplicate gear id '" + gear.Id + "'");

                if (string.IsNullOrEmpty(gear.DisplayName)) report.Error(prefix + "missing display name");
                if (gear.Bonus == null) report.Error(prefix + "has no stat bonus");
                if (gear.ChapterAvailable < 1 || gear.ChapterAvailable > ChapterRules.ChapterCount)
                {
                    report.Error(prefix + "chapter " + gear.ChapterAvailable + " outside 1.." + ChapterRules.ChapterCount);
                }
                if (!string.IsNullOrEmpty(gear.DroppedBy) && content.Boss(gear.DroppedBy) == null)
                {
                    report.Error(prefix + "drops from unknown boss '" + gear.DroppedBy + "'");
                }
                slotsFilled.Add(gear.Slot.ToString());
            }

            // Weapons are validated by ValidateWeapons; the gear list only covers
            // the armour style slots.
            for (int slot = 0; slot <= (int)EquipSlot.Trinket; slot++)
            {
                EquipSlot value = (EquipSlot)slot;
                if (value == EquipSlot.Weapon) continue;
                if (!slotsFilled.Contains(value.ToString()))
                {
                    report.Error("no gear exists for equipment slot " + value);
                }
            }
        }

        // ---------------- enemies ----------------

        private static void ValidateEnemies(CampaignContent content, ValidationReport report)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<EnemyClass> classes = new HashSet<EnemyClass>();
            for (int i = 0; i < content.Enemies.Count; i++)
            {
                EnemyArchetype enemy = content.Enemies[i];
                if (enemy == null)
                {
                    report.Error("enemy entry " + i + " is null");
                    continue;
                }

                string prefix = "enemy '" + enemy.Id + "': ";
                if (string.IsNullOrEmpty(enemy.Id)) report.Error("enemy entry " + i + " has no id");
                else if (!seen.Add(enemy.Id)) report.Error("duplicate enemy id '" + enemy.Id + "'");

                if (string.IsNullOrEmpty(enemy.DisplayName)) report.Error(prefix + "missing display name");
                if (enemy.Tier < 1 || enemy.Tier > 5) report.Error(prefix + "tier " + enemy.Tier + " outside 1..5");
                if (enemy.FirstChapter < 1 || enemy.FirstChapter > ChapterRules.ChapterCount)
                {
                    report.Error(prefix + "first chapter " + enemy.FirstChapter + " outside 1.." + ChapterRules.ChapterCount);
                }
                if (enemy.BaseHealth <= 0f) report.Error(prefix + "health must be positive");
                if (enemy.BaseDamage <= 0f) report.Error(prefix + "damage must be positive");
                if (enemy.Attacks == null || enemy.Attacks.Count == 0)
                {
                    report.Error(prefix + "has no attacks");
                }
                else
                {
                    for (int a = 0; a < enemy.Attacks.Count; a++)
                    {
                        if (enemy.Attacks[a] == null || string.IsNullOrEmpty(enemy.Attacks[a].Id))
                        {
                            report.Error(prefix + "attack " + a + " is missing its id");
                        }
                    }
                }
                classes.Add(enemy.Class);
            }

            if (classes.Count < 8) report.Warn("only " + classes.Count + " distinct enemy behaviours authored; variety is low");
        }

        // ---------------- bosses ----------------

        private static void ValidateBosses(CampaignContent content, ValidationReport report)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int chapterBosses = 0;
            int miniBosses = 0;

            for (int i = 0; i < content.Bosses.Count; i++)
            {
                BossDefinition boss = content.Bosses[i];
                if (boss == null)
                {
                    report.Error("boss entry " + i + " is null");
                    continue;
                }

                string prefix = "boss '" + boss.Id + "': ";
                if (string.IsNullOrEmpty(boss.Id)) report.Error("boss entry " + i + " has no id");
                else if (!seen.Add(boss.Id)) report.Error("duplicate boss id '" + boss.Id + "'");

                if (string.IsNullOrEmpty(boss.DisplayName)) report.Error(prefix + "missing display name");
                if (boss.ChapterIndex < 1 || boss.ChapterIndex > ChapterRules.ChapterCount)
                {
                    report.Error(prefix + "chapter " + boss.ChapterIndex + " outside 1.." + ChapterRules.ChapterCount);
                }
                if (boss.BaseHealth <= 0f) report.Error(prefix + "health must be positive");
                if (boss.BaseDamage <= 0f) report.Error(prefix + "damage must be positive");
                if (boss.Phases == null || boss.Phases.Count < 2)
                {
                    report.Error(prefix + "needs at least 2 phases");
                    continue;
                }

                float previousThreshold = float.MaxValue;
                int easyPatterns = 0;
                int normalPatterns = 0;
                int hardPatterns = 0;
                List<string> patternIds = new List<string>();

                for (int p = 0; p < boss.Phases.Count; p++)
                {
                    BossPhaseDefinition phase = boss.Phases[p];
                    if (phase == null)
                    {
                        report.Error(prefix + "phase " + p + " is null");
                        continue;
                    }

                    string phasePrefix = prefix + "phase '" + phase.Id + "': ";
                    if (string.IsNullOrEmpty(phase.Id)) report.Error(prefix + "phase " + p + " has no id");
                    if (p == 0 && phase.EnterAtHealthRatio < 1f)
                    {
                        report.Error(prefix + "first phase must start at full health (threshold " + phase.EnterAtHealthRatio + ")");
                    }
                    if (phase.EnterAtHealthRatio >= previousThreshold)
                    {
                        report.Error(phasePrefix + "health threshold " + phase.EnterAtHealthRatio + " does not descend");
                    }
                    previousThreshold = phase.EnterAtHealthRatio;

                    if (phase.Patterns == null || phase.Patterns.Count == 0)
                    {
                        report.Error(phasePrefix + "has no attack patterns");
                        continue;
                    }

                    bool hasCore = false;
                    for (int a = 0; a < phase.Patterns.Count; a++)
                    {
                        PatternDefinition pattern = phase.Patterns[a];
                        if (pattern == null)
                        {
                            report.Error(phasePrefix + "pattern " + a + " is null");
                            continue;
                        }
                        if (string.IsNullOrEmpty(pattern.Id))
                        {
                            report.Error(phasePrefix + "pattern " + a + " has no id");
                            continue;
                        }
                        if (patternIds.Contains(pattern.Id))
                        {
                            report.Error(phasePrefix + "pattern id '" + pattern.Id + "' is reused inside the same boss");
                        }
                        patternIds.Add(pattern.Id);

                        if (pattern.Cooldown <= 0f) report.Error(phasePrefix + "pattern '" + pattern.Id + "' has a non-positive cooldown");
                        // Defensive, summon and repositioning patterns are allowed to deal no
                        // damage; anything that is meant to be an attack must hurt.
                        if (pattern.Damage <= 0f && !IsUtilityPattern(pattern.Kind))
                        {
                            report.Error(phasePrefix + "pattern '" + pattern.Id + "' deals no damage");
                        }
                        if (pattern.Telegraph <= 0f) report.Error(phasePrefix + "pattern '" + pattern.Id + "' has no telegraph");

                        if (pattern.RequiredTier == PatternTier.Core) hasCore = true;
                    }

                    if (!hasCore)
                    {
                        report.Error(phasePrefix + "has no Core-tier pattern, so Easy difficulty would stall here");
                    }

                    easyPatterns += phase.AvailablePatterns(DifficultyProfile.For(Difficulty.Easy)).Count;
                    normalPatterns += phase.AvailablePatterns(DifficultyProfile.For(Difficulty.Normal)).Count;
                    hardPatterns += phase.AvailablePatterns(DifficultyProfile.For(Difficulty.Hard)).Count;
                }

                // Difficulty must meaningfully change the fight, not just the numbers.
                if (normalPatterns <= easyPatterns)
                {
                    report.Error(prefix + "Normal difficulty does not add patterns over Easy (" + easyPatterns + " vs " + normalPatterns + ")");
                }
                if (hardPatterns <= normalPatterns)
                {
                    report.Error(prefix + "Hard difficulty does not add patterns over Normal (" + normalPatterns + " vs " + hardPatterns + ")");
                }

                if (boss.IsMiniBoss)
                {
                    miniBosses++;
                    if ((boss.UpgradeDrops == null || boss.UpgradeDrops.Count == 0) &&
                        (boss.WeaponDrops == null || boss.WeaponDrops.Count == 0))
                    {
                        report.Error(prefix + "mini-boss grants no upgrade or weapon");
                    }
                }
                else
                {
                    chapterBosses++;
                }

                if (boss.UpgradeDrops != null)
                {
                    for (int u = 0; u < boss.UpgradeDrops.Count; u++)
                    {
                        string id = boss.UpgradeDrops[u];
                        if (content.Gear(id) == null && content.Perk(id) == null)
                        {
                            report.Error(prefix + "upgrade drop '" + id + "' does not exist as gear or perk");
                        }
                    }
                }
                if (boss.WeaponDrops != null)
                {
                    for (int w = 0; w < boss.WeaponDrops.Count; w++)
                    {
                        if (content.Weapon(boss.WeaponDrops[w]) == null)
                        {
                            report.Error(prefix + "weapon drop '" + boss.WeaponDrops[w] + "' does not exist");
                        }
                    }
                }
            }

            if (chapterBosses != ChapterRules.ChapterCount)
            {
                report.Error("expected " + ChapterRules.ChapterCount + " chapter bosses, found " + chapterBosses);
            }
            if (miniBosses < ChapterRules.ChapterCount * 2)
            {
                report.Error("expected at least 2 mini-bosses per chapter (" + ChapterRules.ChapterCount * 2 +
                             "), found " + miniBosses);
            }
        }

        // ---------------- quests ----------------

        private static void ValidateQuests(CampaignContent content, ValidationReport report)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < content.Quests.Count; i++)
            {
                QuestDefinition quest = content.Quests[i];
                if (quest == null)
                {
                    report.Error("quest entry " + i + " is null");
                    continue;
                }

                string prefix = "quest '" + quest.Id + "': ";
                if (string.IsNullOrEmpty(quest.Id)) report.Error("quest entry " + i + " has no id");
                else if (!seen.Add(quest.Id)) report.Error("duplicate quest id '" + quest.Id + "'");

                if (string.IsNullOrEmpty(quest.Title)) report.Error(prefix + "missing title");
                if (string.IsNullOrEmpty(quest.Summary)) report.Warn(prefix + "has no summary text");
                if (string.IsNullOrEmpty(quest.GiverName)) report.Error(prefix + "has no quest giver");
                if (quest.ChapterIndex < 1 || quest.ChapterIndex > ChapterRules.ChapterCount)
                {
                    report.Error(prefix + "chapter " + quest.ChapterIndex + " outside 1.." + ChapterRules.ChapterCount);
                }
                if (quest.Objectives == null || quest.Objectives.Count == 0)
                {
                    report.Error(prefix + "has no objectives");
                }
                else
                {
                    for (int o = 0; o < quest.Objectives.Count; o++)
                    {
                        QuestObjective objective = quest.Objectives[o];
                        if (objective == null)
                        {
                            report.Error(prefix + "objective " + o + " is null");
                            continue;
                        }
                        if (objective.RequiredCount <= 0) report.Error(prefix + "objective '" + objective.Id + "' requires nothing");
                        if (string.IsNullOrEmpty(objective.Description)) report.Error(prefix + "objective '" + objective.Id + "' has no description");
                    }
                }

                if (quest.XpReward <= 0 && quest.SalvageReward <= 0 && string.IsNullOrEmpty(quest.GearRewardId) &&
                    string.IsNullOrEmpty(quest.WeaponRewardId) && string.IsNullOrEmpty(quest.PerkRewardId))
                {
                    report.Error(prefix + "grants no reward");
                }
                if (quest.Kind != QuestKind.MainQuest && quest.EstimatedMinutes <= 0) report.Error(prefix + "has no estimated length");
                if (!string.IsNullOrEmpty(quest.GearRewardId) && content.Gear(quest.GearRewardId) == null)
                {
                    report.Error(prefix + "gear reward '" + quest.GearRewardId + "' does not exist");
                }
                if (!string.IsNullOrEmpty(quest.WeaponRewardId) && content.Weapon(quest.WeaponRewardId) == null)
                {
                    report.Error(prefix + "weapon reward '" + quest.WeaponRewardId + "' does not exist");
                }
                if (!string.IsNullOrEmpty(quest.PerkRewardId) && content.Perk(quest.PerkRewardId) == null)
                {
                    report.Error(prefix + "perk reward '" + quest.PerkRewardId + "' does not exist");
                }

                if (quest.PrerequisiteQuestIds != null)
                {
                    for (int p = 0; p < quest.PrerequisiteQuestIds.Length; p++)
                    {
                        QuestDefinition prerequisite = content.Quest(quest.PrerequisiteQuestIds[p]);
                        if (prerequisite == null)
                        {
                            report.Error(prefix + "prerequisite quest '" + quest.PrerequisiteQuestIds[p] + "' does not exist");
                        }
                        else if (prerequisite.ChapterIndex > quest.ChapterIndex)
                        {
                            report.Error(prefix + "depends on '" + prerequisite.Id + "' which happens in a later chapter");
                        }
                    }
                }
            }

            for (int chapter = 1; chapter <= ChapterRules.ChapterCount; chapter++)
            {
                int sideQuests = content.SideQuestsInChapter(chapter).Count;
                if (sideQuests < 2)
                {
                    report.Error("chapter " + chapter + " has " + sideQuests + " side quests; at least 2 are required");
                }
            }
        }

        // ---------------- chapters ----------------

        private static void ValidateChapters(CampaignContent content, ValidationReport report)
        {
            if (content.Chapters.Count != ChapterRules.ChapterCount)
            {
                report.Error("expected " + ChapterRules.ChapterCount + " chapters, found " + content.Chapters.Count);
            }

            int previousRecommendedLevel = 0;
            HashSet<string> allQuestIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < content.Quests.Count; i++)
            {
                if (content.Quests[i] != null) allQuestIds.Add(content.Quests[i].Id);
            }

            for (int index = 1; index <= ChapterRules.ChapterCount; index++)
            {
                ChapterDefinition chapter = content.Chapter(index);
                if (chapter == null)
                {
                    report.Error("chapter " + index + " is missing");
                    continue;
                }

                string prefix = "chapter " + index + ": ";
                if (chapter.Index != index) report.Error(prefix + "declares index " + chapter.Index);
                if (string.IsNullOrEmpty(chapter.Title)) report.Error(prefix + "missing title");
                if (string.IsNullOrEmpty(chapter.Synopsis)) report.Error(prefix + "missing synopsis");
                if (string.IsNullOrEmpty(chapter.LocationName)) report.Error(prefix + "missing location name");
                if (chapter.EstimatedMinutes <= 0) report.Error(prefix + "has no estimated length");
                else if (chapter.EstimatedMinutes < 60) report.Warn(prefix + "is shorter than an hour");
                if (chapter.RecommendedLevel < previousRecommendedLevel)
                {
                    report.Error(prefix + "recommended level " + chapter.RecommendedLevel + " drops below the previous chapter");
                }
                previousRecommendedLevel = chapter.RecommendedLevel;
                if (chapter.RecommendedLevel < 1 || chapter.RecommendedLevel > StatComputation.MaxLevel)
                {
                    report.Error(prefix + "recommended level outside 1.." + StatComputation.MaxLevel);
                }
                if (chapter.MinimumLevel > chapter.RecommendedLevel)
                {
                    report.Error(prefix + "minimum level exceeds the recommended level");
                }

                // Beats
                if (chapter.Beats == null || chapter.Beats.Count < 3)
                {
                    report.Error(prefix + "needs at least 3 story beats");
                }
                else
                {
                    HashSet<string> beatIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    bool hasBossBeat = false;
                    bool hasComicBeat = false;
                    bool hasSavepointBeat = false;
                    for (int b = 0; b < chapter.Beats.Count; b++)
                    {
                        ChapterBeat beat = chapter.Beats[b];
                        if (beat == null)
                        {
                            report.Error(prefix + "beat " + b + " is null");
                            continue;
                        }
                        if (string.IsNullOrEmpty(beat.Id)) report.Error(prefix + "beat " + b + " has no id");
                        else if (!beatIds.Add(beat.Id)) report.Error(prefix + "duplicate beat id '" + beat.Id + "'");
                        if (string.IsNullOrEmpty(beat.Title)) report.Error(prefix + "beat '" + beat.Id + "' has no title");
                        if (string.IsNullOrEmpty(beat.Objective)) report.Warn(prefix + "beat '" + beat.Id + "' has no objective text");

                        switch (beat.Kind)
                        {
                            case BeatKind.Comic:
                                if (content.FindComicPage(beat.ReferenceId) == null)
                                {
                                    report.Error(prefix + "beat '" + beat.Id + "' references unknown comic page '" + beat.ReferenceId + "'");
                                }
                                hasComicBeat = true;
                                break;
                            case BeatKind.BossFight:
                                if (content.Boss(beat.ReferenceId) == null)
                                {
                                    report.Error(prefix + "beat '" + beat.Id + "' references unknown boss '" + beat.ReferenceId + "'");
                                }
                                hasBossBeat = true;
                                break;
                            case BeatKind.MiniBossFight:
                                BossDefinition mini = content.Boss(beat.ReferenceId);
                                if (mini == null) report.Error(prefix + "beat '" + beat.Id + "' references unknown mini-boss '" + beat.ReferenceId + "'");
                                else if (!mini.IsMiniBoss) report.Error(prefix + "beat '" + beat.Id + "' references '" + mini.Id + "' which is not a mini-boss");
                                break;
                            case BeatKind.Arena:
                                if (content.Encounter(beat.ReferenceId) == null && FindArena(chapter, beat.ReferenceId) == null)
                                {
                                    report.Error(prefix + "beat '" + beat.Id + "' references unknown encounter or arena '" + beat.ReferenceId + "'");
                                }
                                break;
                            case BeatKind.Savepoint:
                                if (chapter.Savepoint(beat.ReferenceId) == null)
                                {
                                    report.Error(prefix + "beat '" + beat.Id + "' references unknown savepoint '" + beat.ReferenceId + "'");
                                }
                                hasSavepointBeat = true;
                                break;
                            case BeatKind.SideQuestHook:
                                if (!string.IsNullOrEmpty(beat.ReferenceId) && content.Quest(beat.ReferenceId) == null)
                                {
                                    report.Error(prefix + "beat '" + beat.Id + "' references unknown quest '" + beat.ReferenceId + "'");
                                }
                                break;
                        }
                    }

                    if (!hasBossBeat) report.Error(prefix + "has no boss fight beat");
                    if (!hasComicBeat) report.Error(prefix + "has no comic cutscene beat");
                    if (!hasSavepointBeat) report.Error(prefix + "has no savepoint beat");
                }

                // Comic pages (the cutscene format for the whole game)
                if (chapter.ComicPages == null || chapter.ComicPages.Count < 2)
                {
                    report.Error(prefix + "needs at least 2 comic pages");
                }
                else
                {
                    HashSet<string> pageIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    for (int p = 0; p < chapter.ComicPages.Count; p++)
                    {
                        ComicPageDefinition page = chapter.ComicPages[p];
                        if (page == null)
                        {
                            report.Error(prefix + "comic page " + p + " is null");
                            continue;
                        }
                        if (string.IsNullOrEmpty(page.Id)) report.Error(prefix + "comic page " + p + " has no id");
                        else if (!pageIds.Add(page.Id)) report.Error(prefix + "duplicate comic page id '" + page.Id + "'");
                        if (page.ChapterIndex != index) report.Error(prefix + "comic page '" + page.Id + "' declares chapter " + page.ChapterIndex);
                        if (page.Panels == null || page.Panels.Count == 0)
                        {
                            report.Error(prefix + "comic page '" + page.Id + "' has no panels");
                            continue;
                        }
                        if (page.Panels.Count > 9) report.Warn(prefix + "comic page '" + page.Id + "' has " + page.Panels.Count + " panels; split it for readability");

                        bool pageHasContent = false;
                        for (int panelIndex = 0; panelIndex < page.Panels.Count; panelIndex++)
                        {
                            ComicPanelDefinition panel = page.Panels[panelIndex];
                            if (panel == null)
                            {
                                report.Error(prefix + "comic page '" + page.Id + "' panel " + panelIndex + " is null");
                                continue;
                            }
                            bool hasText = !string.IsNullOrEmpty(panel.Speech) || !string.IsNullOrEmpty(panel.Caption) ||
                                           !string.IsNullOrEmpty(panel.SfxText);
                            if (!hasText && (panel.Actors == null || panel.Actors.Count == 0))
                            {
                                report.Error(prefix + "comic page '" + page.Id + "' panel " + panelIndex + " is empty");
                            }
                            if (hasText) pageHasContent = true;
                            if (!string.IsNullOrEmpty(panel.Speaker) && string.IsNullOrEmpty(panel.Speech))
                            {
                                report.Error(prefix + "panel " + panelIndex + " names a speaker but has no dialogue");
                            }
                            if (panel.Actors != null)
                            {
                                for (int a = 0; a < panel.Actors.Count; a++)
                                {
                                    ComicActorPlacement actor = panel.Actors[a];
                                    if (actor == null) continue;
                                    if (string.IsNullOrEmpty(actor.CharacterId))
                                    {
                                        report.Error(prefix + "panel " + panelIndex + " has an actor with no character");
                                    }
                                    if (actor.X < 0f || actor.X > 1f)
                                    {
                                        report.Error(prefix + "panel " + panelIndex + " places an actor outside the frame (x=" + actor.X + ")");
                                    }
                                }
                            }
                        }
                        if (!pageHasContent) report.Warn(prefix + "comic page '" + page.Id + "' is entirely wordless");
                    }
                }

                // Arenas
                if (chapter.Arenas == null || chapter.Arenas.Count == 0)
                {
                    report.Error(prefix + "has no arenas");
                }
                else
                {
                    bool hasBossArena = false;
                    for (int a = 0; a < chapter.Arenas.Count; a++)
                    {
                        ArenaDefinition arena = chapter.Arenas[a];
                        if (arena == null)
                        {
                            report.Error(prefix + "arena " + a + " is null");
                            continue;
                        }
                        if (string.IsNullOrEmpty(arena.Id)) report.Error(prefix + "arena " + a + " has no id");
                        if (arena.Width < 10f || arena.Height < 6f) report.Error(prefix + "arena '" + arena.Id + "' is too small to fight in");
                        if (arena.ChapterIndex != index) report.Error(prefix + "arena '" + arena.Id + "' declares chapter " + arena.ChapterIndex);
                        if (arena.IsBossArena) hasBossArena = true;
                    }
                    if (!hasBossArena) report.Error(prefix + "has no boss arena");
                }

                // Savepoints and checkpoints
                if (chapter.Savepoints == null || chapter.Savepoints.Count < 2)
                {
                    report.Error(prefix + "needs at least 2 savepoints");
                }
                else if (!chapter.HasPreBossSavepoint())
                {
                    report.Error(prefix + "has no savepoint before the boss fight");
                }
                else
                {
                    for (int s = 0; s < chapter.Savepoints.Count; s++)
                    {
                        SavepointDefinition savepoint = chapter.Savepoints[s];
                        if (savepoint == null)
                        {
                            report.Error(prefix + "savepoint " + s + " is null");
                            continue;
                        }
                        if (string.IsNullOrEmpty(savepoint.Id)) report.Error(prefix + "savepoint " + s + " has no id");
                        if (savepoint.ChapterIndex != index) report.Error(prefix + "savepoint '" + savepoint.Id + "' declares chapter " + savepoint.ChapterIndex);
                        if (string.IsNullOrEmpty(savepoint.Label)) report.Error(prefix + "savepoint '" + savepoint.Id + "' has no label");
                        if (!string.IsNullOrEmpty(savepoint.ArenaId) && FindArena(chapter, savepoint.ArenaId) == null)
                        {
                            report.Error(prefix + "savepoint '" + savepoint.Id + "' references unknown arena '" + savepoint.ArenaId + "'");
                        }
                    }
                }

                if (chapter.Checkpoints == null || chapter.Checkpoints.Count == 0)
                {
                    report.Error(prefix + "has no checkpoints");
                }
                else
                {
                    int autosaveCheckpoints = 0;
                    for (int c = 0; c < chapter.Checkpoints.Count; c++)
                    {
                        CheckpointDefinition checkpoint = chapter.Checkpoints[c];
                        if (checkpoint == null)
                        {
                            report.Error(prefix + "checkpoint " + c + " is null");
                            continue;
                        }
                        if (string.IsNullOrEmpty(checkpoint.Id)) report.Error(prefix + "checkpoint " + c + " has no id");
                        if (checkpoint.ChapterIndex != index) report.Error(prefix + "checkpoint '" + checkpoint.Id + "' declares chapter " + checkpoint.ChapterIndex);
                        if (checkpoint.WritesAutosave) autosaveCheckpoints++;
                    }
                    if (autosaveCheckpoints == 0) report.Error(prefix + "has no checkpoint that writes an autosave");
                }

                // Chapter boss
                BossDefinition boss = content.Boss(chapter.BossId);
                if (boss == null)
                {
                    report.Error(prefix + "boss '" + chapter.BossId + "' does not exist");
                }
                else
                {
                    if (boss.IsMiniBoss) report.Error(prefix + "boss '" + boss.Id + "' is flagged as a mini-boss");
                    if (boss.ChapterIndex != index) report.Error(prefix + "boss '" + boss.Id + "' belongs to chapter " + boss.ChapterIndex);
                }

                // Mini-bosses
                int miniBossCount = chapter.MiniBossIds == null ? 0 : chapter.MiniBossIds.Count;
                if (miniBossCount < 2) report.Error(prefix + "has " + miniBossCount + " mini-bosses; at least 2 are required");
                if (chapter.MiniBossIds != null)
                {
                    for (int m = 0; m < chapter.MiniBossIds.Count; m++)
                    {
                        BossDefinition mini = content.Boss(chapter.MiniBossIds[m]);
                        if (mini == null)
                        {
                            report.Error(prefix + "mini-boss '" + chapter.MiniBossIds[m] + "' does not exist");
                        }
                        else
                        {
                            if (!mini.IsMiniBoss) report.Error(prefix + "mini-boss '" + mini.Id + "' is not flagged as a mini-boss");
                            if (mini.ChapterIndex != index) report.Error(prefix + "mini-boss '" + mini.Id + "' belongs to chapter " + mini.ChapterIndex);
                        }
                    }
                }

                // Side quests
                int questCount = chapter.SideQuestIds == null ? 0 : chapter.SideQuestIds.Count;
                if (questCount < 2) report.Error(prefix + "lists " + questCount + " side quests; at least 2 are required");
                if (chapter.SideQuestIds != null)
                {
                    for (int q = 0; q < chapter.SideQuestIds.Count; q++)
                    {
                        QuestDefinition quest = content.Quest(chapter.SideQuestIds[q]);
                        if (quest == null)
                        {
                            report.Error(prefix + "side quest '" + chapter.SideQuestIds[q] + "' does not exist");
                        }
                        else if (quest.ChapterIndex != index)
                        {
                            report.Error(prefix + "side quest '" + quest.Id + "' belongs to chapter " + quest.ChapterIndex);
                        }
                    }
                }

                // Rewards and unlocks
                if (chapter.ChapterRewardIds != null)
                {
                    for (int r = 0; r < chapter.ChapterRewardIds.Count; r++)
                    {
                        string reward = chapter.ChapterRewardIds[r];
                        if (content.Weapon(reward) == null && content.Gear(reward) == null && content.Perk(reward) == null)
                        {
                            report.Error(prefix + "chapter reward '" + reward + "' does not exist");
                        }
                    }
                }

                if (chapter.NewEnemyIds != null)
                {
                    for (int e = 0; e < chapter.NewEnemyIds.Count; e++)
                    {
                        EnemyArchetype enemy = content.Enemy(chapter.NewEnemyIds[e]);
                        if (enemy == null) report.Error(prefix + "introduces unknown enemy '" + chapter.NewEnemyIds[e] + "'");
                        else if (enemy.FirstChapter != index)
                        {
                            report.Warn(prefix + "introduces '" + enemy.Id + "' which first appears in chapter " + enemy.FirstChapter);
                        }
                    }
                }
            }

            // Every side quest should be reachable from a chapter listing.
            for (int i = 0; i < content.Quests.Count; i++)
            {
                QuestDefinition quest = content.Quests[i];
                if (quest == null || quest.Kind == QuestKind.MainQuest) continue;
                ChapterDefinition chapter = content.Chapter(quest.ChapterIndex);
                if (chapter == null) continue;
                if (chapter.SideQuestIds == null || !chapter.SideQuestIds.Contains(quest.Id))
                {
                    report.Error("side quest '" + quest.Id + "' is not listed by chapter " + quest.ChapterIndex);
                }
            }

            int totalMinutes = content.TotalMinutes();
            if (totalMinutes < MinCampaignMinutes || totalMinutes > MaxCampaignMinutes)
            {
                report.Error("campaign length is " + totalMinutes + " minutes; the target is " +
                             MinCampaignMinutes + "-" + MaxCampaignMinutes + " minutes (20-30 hours)");
            }
            else if (totalMinutes < 24 * 60)
            {
                report.Warn("campaign length is " + totalMinutes + " minutes, below the 25 hour target");
            }

            // 12 chapters x 4 pages x ~5 panels. A comic-book cutscene campaign needs
            // enough panels that the story is actually told in panels.
            const int minimumPanels = 240;
            if (content.TotalComicPanels() < minimumPanels)
            {
                report.Error("only " + content.TotalComicPanels() + " comic panels authored; at least " +
                             minimumPanels + " are required");
            }
        }

        /// <summary>Pattern kinds that legitimately deal no direct damage.</summary>
        public static bool IsUtilityPattern(AttackPatternKind kind)
        {
            switch (kind)
            {
                case AttackPatternKind.PhaseBlink:
                case AttackPatternKind.ShieldRecharge:
                case AttackPatternKind.ReflectShield:
                case AttackPatternKind.SummonAdds:
                case AttackPatternKind.TurretDeploy:
                    return true;
                default:
                    return false;
            }
        }

        private static ArenaDefinition FindArena(ChapterDefinition chapter, string arenaId)
        {
            if (chapter == null || chapter.Arenas == null || string.IsNullOrEmpty(arenaId)) return null;
            for (int i = 0; i < chapter.Arenas.Count; i++)
            {
                if (chapter.Arenas[i] != null && chapter.Arenas[i].Id == arenaId) return chapter.Arenas[i];
            }
            return null;
        }

        // ---------------- encounters ----------------

        private static void ValidateEncounters(CampaignContent content, ValidationReport report)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < content.Encounters.Count; i++)
            {
                EncounterDefinition encounter = content.Encounters[i];
                if (encounter == null)
                {
                    report.Error("encounter entry " + i + " is null");
                    continue;
                }

                string prefix = "encounter '" + encounter.Id + "': ";
                if (string.IsNullOrEmpty(encounter.Id)) report.Error("encounter entry " + i + " has no id");
                else if (!seen.Add(encounter.Id)) report.Error("duplicate encounter id '" + encounter.Id + "'");

                if (encounter.ChapterIndex < 1 || encounter.ChapterIndex > ChapterRules.ChapterCount)
                {
                    report.Error(prefix + "chapter " + encounter.ChapterIndex + " outside 1.." + ChapterRules.ChapterCount);
                }
                if (encounter.TargetSeconds < 20 || encounter.TargetSeconds > 600)
                {
                    report.Error(prefix + "target length " + encounter.TargetSeconds + "s is outside 20..600s");
                }
                if (encounter.Waves == null || encounter.Waves.Count == 0)
                {
                    report.Error(prefix + "has no waves");
                    continue;
                }

                for (int w = 0; w < encounter.Waves.Count; w++)
                {
                    EncounterWave wave = encounter.Waves[w];
                    if (wave == null)
                    {
                        report.Error(prefix + "wave " + w + " is null");
                        continue;
                    }
                    if (wave.Spawns == null || wave.Spawns.Count == 0)
                    {
                        report.Error(prefix + "wave " + w + " has no spawns");
                        continue;
                    }
                    for (int s = 0; s < wave.Spawns.Count; s++)
                    {
                        SpawnEntry spawn = wave.Spawns[s];
                        if (spawn == null)
                        {
                            report.Error(prefix + "wave " + w + " spawn " + s + " is null");
                            continue;
                        }
                        EnemyArchetype enemy = content.Enemy(spawn.ArchetypeId);
                        if (enemy == null)
                        {
                            report.Error(prefix + "wave " + w + " spawns unknown enemy '" + spawn.ArchetypeId + "'");
                        }
                        else if (enemy.FirstChapter > encounter.ChapterIndex)
                        {
                            report.Error(prefix + "spawns '" + enemy.Id + "' before its introduction in chapter " + enemy.FirstChapter);
                        }
                        if (spawn.Count < 1) report.Error(prefix + "wave " + w + " spawn count must be at least 1");
                        if (!string.IsNullOrEmpty(spawn.SpawnPointTag) && FindArena(content.Chapter(encounter.ChapterIndex), encounter.ArenaId) is ArenaDefinition arena)
                        {
                            if (arena.SpawnPointTags != null && arena.SpawnPointTags.Count > 0 && !arena.SpawnPointTags.Contains(spawn.SpawnPointTag))
                            {
                                report.Error(prefix + "wave " + w + " uses spawn tag '" + spawn.SpawnPointTag +
                                             "' which arena '" + arena.Id + "' does not define");
                            }
                        }
                    }
                }
            }

            // Chapters should have combat content beyond the boss fight.
            for (int chapter = 1; chapter <= ChapterRules.ChapterCount; chapter++)
            {
                int encounterCount = 0;
                for (int i = 0; i < content.Encounters.Count; i++)
                {
                    if (content.Encounters[i] != null && content.Encounters[i].ChapterIndex == chapter) encounterCount++;
                }
                if (encounterCount < 3)
                {
                    report.Error("chapter " + chapter + " has " + encounterCount + " combat encounters; at least 3 are required");
                }
            }
        }

        // ---------------- reachability ----------------

        private static void ValidateDifficultyReachability(CampaignContent content, ValidationReport report)
        {
            for (int i = 0; i < content.Bosses.Count; i++)
            {
                BossDefinition boss = content.Bosses[i];
                if (boss == null || boss.Phases == null) continue;

                for (int d = 0; d <= (int)Difficulty.Hard; d++)
                {
                    DifficultyProfile profile = DifficultyProfile.For((Difficulty)d);
                    int patterns = boss.TotalPatterns(profile);
                    if (patterns == 0)
                    {
                        report.Error("boss '" + boss.Id + "' has no executable patterns on " + profile.DisplayName);
                    }
                }

                // Hard must be the pattern-rich version, Easy the trimmed one.
                int easy = boss.TotalPatterns(DifficultyProfile.For(Difficulty.Easy));
                int hard = boss.TotalPatterns(DifficultyProfile.For(Difficulty.Hard));
                if (hard - easy < 1)
                {
                    report.Error("boss '" + boss.Id + "' does not gain extra patterns on Hard");
                }
            }
        }

        // ---------------- balance probe ----------------

        /// <summary>Stats a typical player has at the start of the given chapter.</summary>
        public static StatBlock PlayerStatsAt(CampaignContent content, CharacterId characterId, int chapterIndex)
        {
            CharacterDefinition character = content.Character(characterId);
            if (character == null) return new StatBlock();

            int level = ChapterRules.RecommendedLevel(chapterIndex);
            Dictionary<StatId, int> spent = TypicalPointSpending(level);

            List<StatBlock> bonuses = new List<StatBlock>();
            // Assume the best armour the character can own by this chapter.
            if (character.UnlockableGearIds != null)
            {
                GearDefinition best = null;
                for (int i = 0; i < character.UnlockableGearIds.Length; i++)
                {
                    GearDefinition gear = content.Gear(character.UnlockableGearIds[i]);
                    if (gear == null || gear.ChapterAvailable > chapterIndex) continue;
                    if (gear.Slot != EquipSlot.Armor) continue;
                    if (best == null || gear.Tier > best.Tier) best = gear;
                }
                if (best?.Bonus != null) bonuses.Add(best.Bonus);
            }

            return StatComputation.Compute(character.BaseStats, character.PerLevelGrowth, level, spent, bonuses);
        }

        /// <summary>
        /// Models a balanced build: points spread across offence, armour, shield and health.
        /// Used only for balance validation, never for the player's real save.
        /// </summary>
        public static Dictionary<StatId, int> TypicalPointSpending(int level)
        {
            Dictionary<StatId, int> spent = new Dictionary<StatId, int>();
            StatId[] priorities = { StatId.Power, StatId.MaxHealth, StatId.Defense, StatId.Shield, StatId.CritChance };
            int budget = StatComputation.PointsForLevel(level);
            int index = 0;

            // The guard is independent of the shrinking budget, otherwise the loop
            // stops early and the probe understates the player.
            int guard = 0;
            while (budget > 0 && guard < 4000)
            {
                guard++;
                StatId id = priorities[index++ % priorities.Length];
                if (StatComputation.TrySpend(spent, level, id)) budget--;
            }
            return spent;
        }

        /// <summary>Weapon a player is likely carrying in the given chapter.</summary>
        public static WeaponDefinition WeaponForChapter(CampaignContent content, CharacterId characterId, int chapterIndex)
        {
            int expectedTier = MathLite.Clamp((int)Math.Floor((chapterIndex - 1) / 3.0) + 1, 1, 4);
            WeaponDefinition best = null;
            List<WeaponDefinition> candidates = content.WeaponsFor(characterId);
            for (int i = 0; i < candidates.Count; i++)
            {
                WeaponDefinition weapon = candidates[i];
                if (weapon == null || weapon.Tier != expectedTier) continue;
                // Pellets count: a shotgun that fires seven slugs per trigger pull is
                // not weaker than a rifle because its per-shell number is smaller.
                if (best == null || BurstPotential(weapon) > BurstPotential(best))
                {
                    best = weapon;
                }
            }
            if (best != null) return best;

            // Fall back to the highest tier at or below the expected tier.
            for (int i = 0; i < candidates.Count; i++)
            {
                WeaponDefinition weapon = candidates[i];
                if (weapon == null || weapon.Tier > expectedTier) continue;
                if (best == null || weapon.Tier > best.Tier) best = weapon;
            }
            return best ?? (candidates.Count > 0 ? candidates[0] : null);
        }

        /// <summary>Damage per second of one trigger pull, including pellets.</summary>
        public static float BurstPotential(WeaponDefinition weapon)
        {
            if (weapon == null) return 0f;
            return weapon.DamagePerShot * weapon.RoundsPerSecond * MathLite.Max(1, weapon.PelletsPerShot);
        }

        private static void ValidateBalance(CampaignContent content, ValidationReport report)
        {
            DifficultyProfile normal = DifficultyProfile.For(Difficulty.Normal);
            DifficultyProfile hard = DifficultyProfile.For(Difficulty.Hard);

            for (int i = 0; i < content.Characters.Count; i++)
            {
                CharacterDefinition character = content.Characters[i];
                if (character == null) continue;

                for (int chapter = 1; chapter <= ChapterRules.ChapterCount; chapter++)
                {
                    string prefix = "balance (chapter " + chapter + ", " + character.Id + "): ";
                    StatBlock stats = PlayerStatsAt(content, character.Id, chapter);
                    float effectiveHealth = stats.MaxHealth + stats.Shield;

                    // 1. The player must survive a few hits from the chapter's baseline enemy.
                    EnemyArchetype baseline = BaselineEnemyFor(content, chapter);
                    if (baseline != null)
                    {
                        float incoming = EnemyScaling.Damage(baseline, chapter, normal);
                        float afterArmor = incoming * DamageRules.MitigationMultiplier(stats.Defense);
                        float hitsToDie = afterArmor <= 0f ? 999f : effectiveHealth / afterArmor;
                        if (hitsToDie < 3f)
                        {
                            report.Error(prefix + "dies in " + hitsToDie.ToString("0.0") + " hits from '" + baseline.Id +
                                         "' on Normal; at least 3 hits of reaction time are required");
                        }
                        // The first two chapters are the tutorial ramp: a weak baseline is
                        // intentional there, so only flag it once the game expects a fight.
                        if (hitsToDie > 40f && chapter >= 3)
                        {
                            report.Warn(prefix + "takes " + hitsToDie.ToString("0.0") + " hits from '" + baseline.Id + "'; combat may feel trivial");
                        }

                        float hardIncoming = EnemyScaling.Damage(baseline, chapter, hard) * DamageRules.MitigationMultiplier(stats.Defense);
                        float hardHits = hardIncoming <= 0f ? 999f : effectiveHealth / hardIncoming;
                        if (hardHits < 2f)
                        {
                            report.Error(prefix + "dies in " + hardHits.ToString("0.0") + " hits on Hard; Hard must stay playable");
                        }
                    }

                    // 2. Weapon and boss time-to-kill must stay in a sane window.
                    WeaponDefinition weapon = WeaponForChapter(content, character.Id, chapter);
                    BossDefinition boss = content.Boss(content.Chapter(chapter)?.BossId);
                    if (weapon != null && boss != null)
                    {
                        float dps = WeaponMath.SustainedDps(weapon, stats, boss.BaseDefense);
                        if (dps <= 0f)
                        {
                            report.Error(prefix + "weapon '" + weapon.Id + "' produces no damage against boss '" + boss.Id + "'");
                        }
                        else
                        {
                            float bossHealth = boss.BaseHealth * normal.EnemyHealthMultiplier * ChapterRules.EnemyHealthMultiplier(chapter);
                            float ttk = bossHealth / dps;

                            // This model assumes perfect uptime: no dodging, no reloads
                            // wasted, no misses. Real fights run two to three times
                            // longer, so the floor here is deliberately low. Mini-bosses
                            // are meant to be short upgrade fights.
                            float minimumTtk = boss.IsMiniBoss ? 8f : 20f;
                            float maximumTtk = boss.IsMiniBoss ? 60f : 150f;
                            if (ttk < minimumTtk)
                            {
                                report.Error(prefix + "boss '" + boss.Id + "' dies in " + ttk.ToString("0.0") +
                                             "s with '" + weapon.Id + "'; at least " + minimumTtk + "s of uptime is required");
                            }
                            if (ttk > maximumTtk)
                            {
                                report.Error(prefix + "boss '" + boss.Id + "' takes " + ttk.ToString("0.0") +
                                             "s with '" + weapon.Id + "'; that is a slog rather than a fight");
                            }
                        }
                    }

                    // 3. The campaign must carry the player to the level cap region.
                    if (chapter == ChapterRules.ChapterCount)
                    {
                        int xp = ChapterRules.TotalMainPathXp(170, normal);
                        int level = XpCurve.LevelFromTotalXp(xp);
                        if (level < 55)
                        {
                            report.Error("main-path XP only reaches level " + level + "; the level cap of 60 should be within reach");
                        }
                    }
                }
            }
        }

        private static EnemyArchetype BaselineEnemyFor(CampaignContent content, int chapter)
        {
            EnemyArchetype best = null;
            for (int i = 0; i < content.Enemies.Count; i++)
            {
                EnemyArchetype enemy = content.Enemies[i];
                if (enemy == null || enemy.FirstChapter > chapter || enemy.IsSupport) continue;
                if (best == null || enemy.Tier > best.Tier) best = enemy;
            }
            return best;
        }
    }
}
