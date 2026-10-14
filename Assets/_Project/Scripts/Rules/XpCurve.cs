using System;

namespace IronVeil.Rules
{
    /// <summary>
    /// Level curve for a level cap of 60 across a ~24 hour campaign.
    /// Total XP required to reach 60 is roughly 450,000 which matches an average
    /// of about 5 XP per second over 25 hours of play (combat + quests + chapter clears).
    /// </summary>
    public static class XpCurve
    {
        public const int MaxLevel = StatComputation.MaxLevel;

        /// <summary>XP required to advance from <paramref name="level"/> to the next level.</summary>
        public static int XpToNext(int level)
        {
            int capped = MathLite.Clamp(level, 1, MaxLevel);
            if (capped >= MaxLevel) return 0;
            float raw = 60f * MathLite.Pow(capped, 1.35f) + 45f * capped;
            return MathLite.RoundToInt(raw);
        }

        /// <summary>Total XP required to reach <paramref name="level"/> from level 1.</summary>
        public static int TotalXpForLevel(int level)
        {
            int capped = MathLite.Clamp(level, 1, MaxLevel);
            int total = 0;
            for (int i = 1; i < capped; i++)
            {
                total += XpToNext(i);
            }
            return total;
        }

        /// <summary>Highest level reachable with the given lifetime XP total.</summary>
        public static int LevelFromTotalXp(int totalXp)
        {
            if (totalXp <= 0) return 1;
            int level = 1;
            int remaining = totalXp;
            while (level < MaxLevel)
            {
                int need = XpToNext(level);
                if (need <= 0 || remaining < need) break;
                remaining -= need;
                level++;
            }
            return level;
        }

        /// <summary>XP progress inside the current level, in [0, 1). Always 1 at max level.</summary>
        public static float Progress(int level, int xpIntoLevel)
        {
            if (level >= MaxLevel) return 1f;
            int need = XpToNext(level);
            if (need <= 0) return 1f;
            return MathLite.Clamp01(xpIntoLevel / (float)need);
        }

        /// <summary>XP still needed inside the current level.</summary>
        public static int RemainingInLevel(int level, int xpIntoLevel)
        {
            if (level >= MaxLevel) return 0;
            int need = XpToNext(level);
            return MathLite.Max(0, need - xpIntoLevel);
        }

        /// <summary>
        /// Applies an XP gain, resolving multiple level-ups and the level cap.
        /// </summary>
        public static LevelUpResult Apply(int level, int xpIntoLevel, int amount)
        {
            LevelUpResult result = new LevelUpResult
            {
                Level = MathLite.Clamp(level, 1, MaxLevel),
                XpIntoLevel = MathLite.Max(0, xpIntoLevel)
            };

            if (amount <= 0 || result.Level >= MaxLevel)
            {
                if (result.Level >= MaxLevel) result.XpIntoLevel = 0;
                result.LevelsGained = 0;
                result.PointsGained = 0;
                return result;
            }

            int startLevel = result.Level;
            result.XpIntoLevel += amount;

            while (result.Level < MaxLevel)
            {
                int need = XpToNext(result.Level);
                if (need <= 0 || result.XpIntoLevel < need) break;
                result.XpIntoLevel -= need;
                result.Level++;
            }

            if (result.Level >= MaxLevel) result.XpIntoLevel = 0;

            result.LevelsGained = result.Level - startLevel;
            result.PointsGained = StatComputation.PointsForLevel(result.Level) -
                                  StatComputation.PointsForLevel(startLevel);
            return result;
        }

        /// <summary>XP awarded for a kill, scaled by chapter depth, enemy tier and difficulty.</summary>
        public static int KillReward(int enemyTier, int chapterIndex, DifficultyProfile difficulty)
        {
            float tierMul = 1f + 0.18f * (MathLite.Clamp(enemyTier, 1, 5) - 1);
            float chapterMul = ChapterRules.XpMultiplier(chapterIndex);
            float diffMul = difficulty == null ? 1f : difficulty.XpMultiplier;
            return MathLite.Max(1, MathLite.RoundToInt(enemyTier * 13f * tierMul * chapterMul * diffMul));
        }

        /// <summary>XP awarded for a chapter boss, which carries the bulk of progression.</summary>
        public static int BossReward(int chapterIndex, DifficultyProfile difficulty)
        {
            float chapterMul = ChapterRules.XpMultiplier(chapterIndex);
            float diffMul = difficulty == null ? 1f : difficulty.XpMultiplier;
            return MathLite.RoundToInt(4200f * chapterMul * diffMul);
        }

        /// <summary>XP awarded for a mini-boss (upgrade carrier).</summary>
        public static int MiniBossReward(int chapterIndex, DifficultyProfile difficulty)
        {
            float chapterMul = ChapterRules.XpMultiplier(chapterIndex);
            float diffMul = difficulty == null ? 1f : difficulty.XpMultiplier;
            return MathLite.RoundToInt(1400f * chapterMul * diffMul);
        }
    }

    /// <summary>Outcome of applying XP, including how many points a level-up awarded.</summary>
    [Serializable]
    public struct LevelUpResult
    {
        public int Level;
        public int XpIntoLevel;
        public int LevelsGained;
        public int PointsGained;
    }
}
