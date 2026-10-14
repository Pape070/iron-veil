using System;

namespace IronVeil.Rules
{
    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    /// <summary>How many of a boss's authored attack patterns are active.</summary>
    public enum PatternTier
    {
        Core = 1,
        Extended = 2,
        Full = 3
    }

    /// <summary>
    /// All difficulty-driven multipliers in one place. Difficulty never changes
    /// story content, only combat pressure, pattern count, telegraphs and rewards.
    /// </summary>
    [Serializable]
    public sealed class DifficultyProfile
    {
        public Difficulty Difficulty;
        public string DisplayName = "Normal";
        public string Summary = string.Empty;

        /// <summary>Enemy health multiplier.</summary>
        public float EnemyHealthMultiplier = 1f;

        /// <summary>Enemy outgoing damage multiplier.</summary>
        public float EnemyDamageMultiplier = 1f;

        /// <summary>Enemy movement / projectile speed multiplier.</summary>
        public float EnemySpeedMultiplier = 1f;

        /// <summary>Spawn count and wave pressure multiplier.</summary>
        public float SpawnMultiplier = 1f;

        /// <summary>Player outgoing damage multiplier (slight reduction on Hard).</summary>
        public float PlayerDamageMultiplier = 1f;

        /// <summary>XP gained multiplier.</summary>
        public float XpMultiplier = 1f;

        /// <summary>Chance multiplier for upgrade drops from mini-bosses and elites.</summary>
        public float LootChanceMultiplier = 1f;

        /// <summary>Rarity roll bonus for dropped gear.</summary>
        public float LootRarityBonus = 0f;

        /// <summary>Extra health flasks and ammo found in the world.</summary>
        public float ResourceMultiplier = 1f;

        /// <summary>How many patterns each boss runs. Easy = Core only, Hard = every pattern.</summary>
        public PatternTier PatternTier = PatternTier.Extended;

        /// <summary>Seconds a pattern telegraphs before it becomes lethal. Lower is harder.</summary>
        public float TelegraphTimeScale = 1f;

        /// <summary>Seconds of player invulnerability after a hit.</summary>
        public float PlayerInvulnerabilitySeconds = 0.9f;

        /// <summary>Multiplier applied to enemy attack cooldowns between patterns. Lower is harder.</summary>
        public float EnemyCooldownMultiplier = 1f;

        /// <summary>Number of simultaneous elite variants allowed in a wave.</summary>
        public int EliteCount = 1;

        /// <summary>Extra enemy adds spawned during a boss fight.</summary>
        public int BossAddCount = 0;

        /// <summary>Enemies deal knockback and stagger the player more aggressively.</summary>
        public float PlayerStaggerMultiplier = 1f;

        public bool PermadeathWarnings = false;

        public static DifficultyProfile For(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    return new DifficultyProfile
                    {
                        Difficulty = Difficulty.Easy,
                        DisplayName = "Easy",
                        Summary = "Story-first. Enemies are softer, patterns telegraph longer, " +
                                  "checkpoints are frequent and the game hints at incoming attacks.",
                        EnemyHealthMultiplier = 0.75f,
                        EnemyDamageMultiplier = 0.65f,
                        EnemySpeedMultiplier = 0.92f,
                        SpawnMultiplier = 0.8f,
                        PlayerDamageMultiplier = 1.15f,
                        XpMultiplier = 0.9f,
                        LootChanceMultiplier = 1.25f,
                        LootRarityBonus = 0.05f,
                        ResourceMultiplier = 1.5f,
                        PatternTier = PatternTier.Core,
                        TelegraphTimeScale = 1.4f,
                        PlayerInvulnerabilitySeconds = 1.2f,
                        EnemyCooldownMultiplier = 1.35f,
                        EliteCount = 0,
                        BossAddCount = 0,
                        PlayerStaggerMultiplier = 0.6f
                    };
                case Difficulty.Hard:
                    return new DifficultyProfile
                    {
                        Difficulty = Difficulty.Hard,
                        DisplayName = "Hard",
                        Summary = "Every boss pattern is live, elites appear in waves, " +
                                  "telegraphs are tight and resources are scarce.",
                        EnemyHealthMultiplier = 1.6f,
                        EnemyDamageMultiplier = 1.85f,
                        EnemySpeedMultiplier = 1.14f,
                        SpawnMultiplier = 1.45f,
                        PlayerDamageMultiplier = 0.92f,
                        XpMultiplier = 1.35f,
                        LootChanceMultiplier = 1.1f,
                        LootRarityBonus = 0.2f,
                        ResourceMultiplier = 0.65f,
                        PatternTier = PatternTier.Full,
                        TelegraphTimeScale = 0.62f,
                        PlayerInvulnerabilitySeconds = 0.7f,
                        EnemyCooldownMultiplier = 0.7f,
                        EliteCount = 3,
                        BossAddCount = 3,
                        PlayerStaggerMultiplier = 1.5f,
                        PermadeathWarnings = true
                    };
                default:
                    return new DifficultyProfile
                    {
                        Difficulty = Difficulty.Normal,
                        DisplayName = "Normal",
                        Summary = "The intended experience. Bosses use extended pattern sets " +
                                  "and resource drops are balanced for steady progression.",
                        EnemyHealthMultiplier = 1f,
                        EnemyDamageMultiplier = 1f,
                        EnemySpeedMultiplier = 1f,
                        SpawnMultiplier = 1f,
                        PlayerDamageMultiplier = 1f,
                        XpMultiplier = 1f,
                        LootChanceMultiplier = 1f,
                        LootRarityBonus = 0.1f,
                        ResourceMultiplier = 1f,
                        PatternTier = PatternTier.Extended,
                        TelegraphTimeScale = 1f,
                        PlayerInvulnerabilitySeconds = 0.9f,
                        EnemyCooldownMultiplier = 1f,
                        EliteCount = 1,
                        BossAddCount = 1,
                        PlayerStaggerMultiplier = 1f
                    };
            }
        }

        public static DifficultyProfile[] All()
        {
            return new[]
            {
                For(Difficulty.Easy),
                For(Difficulty.Normal),
                For(Difficulty.Hard)
            };
        }

        /// <summary>True when the given pattern is active under this difficulty.</summary>
        public bool AllowsPattern(PatternDefinition pattern)
        {
            if (pattern == null) return false;
            return (int)pattern.RequiredTier <= (int)PatternTier;
        }

        public float ScaleTelegraph(float seconds)
        {
            return MathLite.Max(0.05f, seconds * TelegraphTimeScale);
        }

        public float ScaleCooldown(float seconds)
        {
            return MathLite.Max(0.15f, seconds * EnemyCooldownMultiplier);
        }
    }
}
