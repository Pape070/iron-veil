using System;
using System.Collections.Generic;

namespace IronVeil.Rules
{
    public enum CharacterId
    {
        None = 0,
        Kade = 1,
        Vera = 2
    }

    public enum StatId
    {
        Power = 0,
        Defense = 1,
        Shield = 2,
        MaxHealth = 3,
        FireRate = 4,
        CritChance = 5,
        CritDamage = 6,
        MoveSpeed = 7,
        AbilityPower = 8,
        ReloadSpeed = 9,
        Luck = 10
    }

    public enum EquipSlot
    {
        Weapon = 0,
        Armor = 1,
        ShieldCore = 2,
        Trinket = 3
    }

    /// <summary>
    /// A bag of additive stats. Multipliers (FireRate, CritDamage, ReloadSpeed,
    /// AbilityPower) are stored as raw values where 1.0 == baseline.
    /// </summary>
    [Serializable]
    public sealed class StatBlock
    {
        public float Power = 20f;
        public float Defense = 10f;
        public float Shield = 0f;
        public float ShieldRegen = 6f;
        public float MaxHealth = 120f;
        public float FireRateMul = 1f;
        public float CritChance = 0.05f;
        public float CritDamageMul = 1.7f;
        public float MoveSpeed = 6.5f;
        public float AbilityPower = 1f;
        public float ReloadSpeedMul = 1f;
        public float Luck = 0f;

        public StatBlock Clone() => (StatBlock)MemberwiseClone();

        /// <summary>
        /// Adds another block into this one, field by field.
        ///
        /// Every additive source (per-level growth, invested points, gear, perks) is
        /// a *delta* block: flat stats are added directly and multiplier stats carry
        /// the change, so 0 means "unchanged" for both. Absolute values only ever
        /// live in a character's BaseStats. Mixing the two conventions is how a
        /// +1 multiplier per level turns into a 50x damage bug.
        /// </summary>
        public void AddInPlace(StatBlock other)
        {
            if (other == null) return;
            Power += other.Power;
            Defense += other.Defense;
            Shield += other.Shield;
            ShieldRegen += other.ShieldRegen;
            MaxHealth += other.MaxHealth;
            FireRateMul += other.FireRateMul;
            CritChance += other.CritChance;
            CritDamageMul += other.CritDamageMul;
            MoveSpeed += other.MoveSpeed;
            AbilityPower += other.AbilityPower;
            ReloadSpeedMul += other.ReloadSpeedMul;
            Luck += other.Luck;
        }

        public static StatBlock operator +(StatBlock a, StatBlock b)
        {
            if (a == null) return b == null ? new StatBlock() : b.Clone();
            StatBlock result = a.Clone();
            result.AddInPlace(b);
            return result;
        }

        /// <summary>
        /// The additive identity: a delta block with every field at zero, including
        /// the multiplier fields (0 == unchanged). This is what per-level growth,
        /// gear bonuses and perk bonuses start from.
        /// </summary>
        public static StatBlock Delta()
        {
            return new StatBlock
            {
                Power = 0f,
                Defense = 0f,
                Shield = 0f,
                ShieldRegen = 0f,
                MaxHealth = 0f,
                FireRateMul = 0f,
                CritChance = 0f,
                CritDamageMul = 0f,
                MoveSpeed = 0f,
                AbilityPower = 0f,
                ReloadSpeedMul = 0f,
                Luck = 0f
            };
        }
    }

    /// <summary>Maps StatId onto StatBlock fields and describes how stat points behave.</summary>
    public static class StatCatalog
    {
        public static readonly StatId[] All =
        {
            StatId.Power, StatId.Defense, StatId.Shield, StatId.MaxHealth, StatId.FireRate,
            StatId.CritChance, StatId.CritDamage, StatId.MoveSpeed, StatId.AbilityPower,
            StatId.ReloadSpeed, StatId.Luck
        };

        public static float Get(StatBlock block, StatId id)
        {
            if (block == null) return 0f;
            switch (id)
            {
                case StatId.Power: return block.Power;
                case StatId.Defense: return block.Defense;
                case StatId.Shield: return block.Shield;
                case StatId.MaxHealth: return block.MaxHealth;
                case StatId.FireRate: return block.FireRateMul;
                case StatId.CritChance: return block.CritChance;
                case StatId.CritDamage: return block.CritDamageMul;
                case StatId.MoveSpeed: return block.MoveSpeed;
                case StatId.AbilityPower: return block.AbilityPower;
                case StatId.ReloadSpeed: return block.ReloadSpeedMul;
                case StatId.Luck: return block.Luck;
                default: return 0f;
            }
        }

        public static void Add(StatBlock block, StatId id, float amount)
        {
            if (block == null) return;
            switch (id)
            {
                case StatId.Power: block.Power += amount; break;
                case StatId.Defense: block.Defense += amount; break;
                case StatId.Shield: block.Shield += amount; break;
                case StatId.MaxHealth: block.MaxHealth += amount; break;
                case StatId.FireRate: block.FireRateMul += amount; break;
                case StatId.CritChance: block.CritChance += amount; break;
                case StatId.CritDamage: block.CritDamageMul += amount; break;
                case StatId.MoveSpeed: block.MoveSpeed += amount; break;
                case StatId.AbilityPower: block.AbilityPower += amount; break;
                case StatId.ReloadSpeed: block.ReloadSpeedMul += amount; break;
                case StatId.Luck: block.Luck += amount; break;
            }
        }

        /// <summary>Gain granted by a single invested skill point.</summary>
        public static float PerPoint(StatId id)
        {
            switch (id)
            {
                case StatId.Power: return 4f;
                case StatId.Defense: return 3f;
                case StatId.Shield: return 6f;
                case StatId.MaxHealth: return 14f;
                case StatId.FireRate: return 0.015f;
                case StatId.CritChance: return 0.008f;
                case StatId.CritDamage: return 0.03f;
                case StatId.MoveSpeed: return 0.12f;
                case StatId.AbilityPower: return 0.05f;
                case StatId.ReloadSpeed: return 0.012f;
                case StatId.Luck: return 2f;
                default: return 0f;
            }
        }

        /// <summary>Maximum points allowed per stat, per character (keeps builds distinct).</summary>
        public static int PointCap(StatId id)
        {
            switch (id)
            {
                case StatId.CritDamage: return 20;
                case StatId.CritChance: return 25;
                case StatId.MoveSpeed: return 20;
                case StatId.Luck: return 15;
                default: return 40;
            }
        }

        public static bool IsPercent(StatId id)
        {
            switch (id)
            {
                case StatId.CritChance:
                case StatId.CritDamage:
                case StatId.FireRate:
                case StatId.ReloadSpeed:
                case StatId.AbilityPower:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsInteger(StatId id)
        {
            switch (id)
            {
                case StatId.Power:
                case StatId.Defense:
                case StatId.Shield:
                case StatId.MaxHealth:
                case StatId.MoveSpeed:
                case StatId.Luck:
                    return true;
                default:
                    return false;
            }
        }

        public static string DisplayName(StatId id)
        {
            switch (id)
            {
                case StatId.Power: return "Power";
                case StatId.Defense: return "Defense";
                case StatId.Shield: return "Shield";
                case StatId.MaxHealth: return "Health";
                case StatId.FireRate: return "Fire Rate";
                case StatId.CritChance: return "Crit Chance";
                case StatId.CritDamage: return "Crit Damage";
                case StatId.MoveSpeed: return "Move Speed";
                case StatId.AbilityPower: return "Ability Power";
                case StatId.ReloadSpeed: return "Reload Speed";
                case StatId.Luck: return "Luck";
                default: return id.ToString();
            }
        }

        public static string Description(StatId id)
        {
            switch (id)
            {
                case StatId.Power: return "Raises all weapon damage. Applies before armor mitigation.";
                case StatId.Defense: return "Reduces incoming damage on a curve; soft-capped at 75%.";
                case StatId.Shield: return "Adds a regenerating absorb pool that soaks damage before health.";
                case StatId.MaxHealth: return "Increases maximum health.";
                case StatId.FireRate: return "Shots per second multiplier for all weapons.";
                case StatId.CritChance: return "Chance for a hit to critically strike.";
                case StatId.CritDamage: return "Damage multiplier applied to critical strikes.";
                case StatId.MoveSpeed: return "Horizontal and airborne movement speed.";
                case StatId.AbilityPower: return "Strength of Bulwark, Phase Blink, Slam and Overclock effects.";
                case StatId.ReloadSpeed: return "Reduces reload and weapon-swap time.";
                case StatId.Luck: return "Improves drop chance and rarity rolls from mini-bosses.";
                default: return string.Empty;
            }
        }
    }

    /// <summary>Level and skill-point math. Pure: same inputs, same outputs, testable.</summary>
    public static class StatComputation
    {
        public const int MaxLevel = 60;

        /// <summary>Skill points granted when reaching the given level (cumulative).</summary>
        public static int PointsForLevel(int level)
        {
            int capped = MathLite.Clamp(level, 1, MaxLevel);
            int total = (capped - 1) * 3;
            total += (capped - 1) / 5; // +1 bonus point every 5 levels
            return total;
        }

        public static int PointsAlreadySpent(IReadOnlyDictionary<StatId, int> spent)
        {
            if (spent == null) return 0;
            int total = 0;
            foreach (KeyValuePair<StatId, int> pair in spent)
            {
                if (pair.Value > 0) total += pair.Value;
            }
            return total;
        }

        public static int AvailablePoints(int level, IReadOnlyDictionary<StatId, int> spent)
        {
            return MathLite.Max(0, PointsForLevel(level) - PointsAlreadySpent(spent));
        }

        public static int SpentIn(IReadOnlyDictionary<StatId, int> spent, StatId id)
        {
            if (spent == null) return 0;
            return spent.TryGetValue(id, out int value) ? value : 0;
        }

        /// <summary>True when another point may be invested into <paramref name="id"/>.</summary>
        public static bool CanInvest(int level, IReadOnlyDictionary<StatId, int> spent, StatId id)
        {
            if (AvailablePoints(level, spent) <= 0) return false;
            return SpentIn(spent, id) < StatCatalog.PointCap(id);
        }

        /// <summary>
        /// Attempts to spend a point. Returns false (and leaves the dictionary alone)
        /// when the level cap, availability or per-stat cap blocks it.
        /// </summary>
        public static bool TrySpend(IDictionary<StatId, int> spent, int level, StatId id)
        {
            if (spent == null) return false;
            IReadOnlyDictionary<StatId, int> readOnly = (IReadOnlyDictionary<StatId, int>)spent;
            if (!CanInvest(level, readOnly, id)) return false;
            spent[id] = SpentIn(readOnly, id) + 1;
            return true;
        }

        /// <summary>Refunds a single point if one was invested.</summary>
        public static bool TryRefund(IDictionary<StatId, int> spent, StatId id)
        {
            if (spent == null) return false;
            int current = SpentIn((IReadOnlyDictionary<StatId, int>)spent, id);
            if (current <= 0) return false;
            if (current == 1) spent.Remove(id);
            else spent[id] = current - 1;
            return true;
        }

        /// <summary>
        /// Final stats = base + per-level growth * (level-1) + invested points + equipment.
        /// </summary>
        public static StatBlock Compute(
            StatBlock baseStats,
            StatBlock perLevelGrowth,
            int level,
            IReadOnlyDictionary<StatId, int> spent,
            IEnumerable<StatBlock> bonuses = null)
        {
            StatBlock result = baseStats == null ? new StatBlock() : baseStats.Clone();
            int levelsGained = MathLite.Clamp(level, 1, MaxLevel) - 1;

            if (perLevelGrowth != null && levelsGained > 0)
            {
                // Growth is delta-based: multiplier stats carry the change per level
                // (0 means no growth), so multiplying by levels and adding is correct.
                StatBlock scaled = StatBlock.Delta();
                for (int i = 0; i < StatCatalog.All.Length; i++)
                {
                    StatId id = StatCatalog.All[i];
                    float perLevel = StatCatalog.Get(perLevelGrowth, id);
                    if (MathLite.Abs(perLevel) < MathLite.Epsilon) continue;
                    StatCatalog.Add(scaled, id, perLevel * levelsGained);
                }
                result.AddInPlace(scaled);
            }

            if (spent != null)
            {
                foreach (KeyValuePair<StatId, int> pair in spent)
                {
                    if (pair.Value <= 0) continue;
                    StatCatalog.Add(result, pair.Key, StatCatalog.PerPoint(pair.Key) * pair.Value);
                }
            }

            if (bonuses != null)
            {
                foreach (StatBlock bonus in bonuses)
                {
                    result.AddInPlace(bonus);
                }
            }

            Normalize(result);
            return result;
        }

        private static void Normalize(StatBlock block)
        {
            block.MaxHealth = MathLite.Max(1f, block.MaxHealth);
            block.Power = MathLite.Max(1f, block.Power);
            block.Defense = MathLite.Max(0f, block.Defense);
            block.Shield = MathLite.Max(0f, block.Shield);
            block.ShieldRegen = MathLite.Max(0f, block.ShieldRegen);
            block.FireRateMul = MathLite.Clamp(block.FireRateMul, 0.2f, 4f);
            block.CritChance = MathLite.Clamp(block.CritChance, 0f, 0.9f);
            // Hard ceilings on the multipliers as well: a data mistake in a growth or
            // bonus block must not silently multiply damage by orders of magnitude.
            block.CritDamageMul = MathLite.Clamp(block.CritDamageMul, 1f, 5f);
            block.MoveSpeed = MathLite.Clamp(block.MoveSpeed, 1f, 20f);
            block.AbilityPower = MathLite.Clamp(block.AbilityPower, 0.1f, 3f);
            block.ReloadSpeedMul = MathLite.Clamp(block.ReloadSpeedMul, 0.25f, 4f);
            block.Luck = MathLite.Max(0f, block.Luck);
        }
    }
}
