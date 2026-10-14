using System;
using System.Collections.Generic;

namespace IronVeil.Rules
{
    public enum EnemyClass
    {
        Grunt = 0,
        Rusher = 1,
        Turret = 2,
        Flyer = 3,
        Sniper = 4,
        Brute = 5,
        Shielder = 6,
        Drone = 7,
        Spitter = 8,
        Bombardier = 9,
        Assassin = 10,
        Medic = 11,
        Swarm = 12,
        Artillery = 13,
        Beast = 14
    }

    /// <summary>Behavioural + stat template for one enemy type.</summary>
    [Serializable]
    public sealed class EnemyArchetype
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public EnemyClass Class = EnemyClass.Grunt;

        /// <summary>1-5, drives XP, salvage and stat scaling.</summary>
        public int Tier = 1;

        public float BaseHealth = 45f;
        public float BaseDamage = 8f;
        public float BaseDefense = 5f;
        public float MoveSpeed = 3.2f;
        public float AttackRange = 7f;
        public float AttackCooldown = 1.6f;
        public float Telegraph = 0.45f;
        public float DetectionRange = 12f;
        public float KnockbackResistance = 0f;

        public bool CanFly = false;
        public bool IsEliteCandidate = false;
        public bool CanBeStaggered = true;
        public bool RetreatsWhenHurt = false;
        public bool IsSupport = false;

        /// <summary>First chapter where the enemy appears.</summary>
        public int FirstChapter = 1;

        public int ProjectileCount = 1;
        public float ProjectileSpeed = 11f;
        public float WeakPointMultiplier = 1.75f;
        public string[] Vulnerabilities = Array.Empty<string>();
        public string[] Resistances = Array.Empty<string>();

        /// <summary>Attacks, reusing the shared pattern vocabulary.</summary>
        public List<PatternDefinition> Attacks = new List<PatternDefinition>();

        public string Bestiary = string.Empty;

        public bool Resists(string tag)
        {
            return Contains(Resistances, tag);
        }

        public bool VulnerableTo(string tag)
        {
            return Contains(Vulnerabilities, tag);
        }

        private static bool Contains(string[] array, string tag)
        {
            if (array == null || string.IsNullOrEmpty(tag)) return false;
            for (int i = 0; i < array.Length; i++)
            {
                if (string.Equals(array[i], tag, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }

    /// <summary>Chapter and difficulty scaling for enemy stats.</summary>
    public static class EnemyScaling
    {
        public static float Health(EnemyArchetype archetype, int chapterIndex, DifficultyProfile difficulty)
        {
            if (archetype == null) return 1f;
            float chapterMul = ChapterRules.EnemyHealthMultiplier(chapterIndex);
            float diffMul = difficulty == null ? 1f : difficulty.EnemyHealthMultiplier;
            float eliteMul = archetype.IsEliteCandidate ? 1f : 1f;
            return archetype.BaseHealth * chapterMul * diffMul * eliteMul;
        }

        public static float Damage(EnemyArchetype archetype, int chapterIndex, DifficultyProfile difficulty)
        {
            if (archetype == null) return 0f;
            float chapterMul = ChapterRules.EnemyDamageMultiplier(chapterIndex);
            float diffMul = difficulty == null ? 1f : difficulty.EnemyDamageMultiplier;
            return archetype.BaseDamage * chapterMul * diffMul;
        }

        public static float Speed(EnemyArchetype archetype, int chapterIndex, DifficultyProfile difficulty)
        {
            if (archetype == null) return 0f;
            float chapterMul = MathLite.Min(1.25f, 1f + 0.02f * MathLite.Max(0, chapterIndex - 1));
            float diffMul = difficulty == null ? 1f : difficulty.EnemySpeedMultiplier;
            return archetype.MoveSpeed * chapterMul * diffMul;
        }

        public static float Telegraph(EnemyArchetype archetype, DifficultyProfile difficulty)
        {
            if (archetype == null) return 0.4f;
            return (difficulty == null ? 1f : difficulty.TelegraphTimeScale) * archetype.Telegraph;
        }

        public static float AttackCooldown(EnemyArchetype archetype, DifficultyProfile difficulty)
        {
            if (archetype == null) return 1f;
            float mul = difficulty == null ? 1f : difficulty.EnemyCooldownMultiplier;
            return archetype.AttackCooldown * mul;
        }

        /// <summary>Multiplier applied to an elite variant of the archetype.</summary>
        public static float EliteHealthMultiplier => 2.35f;

        public static float EliteDamageMultiplier => 1.35f;

        public static float EliteSpeedMultiplier => 1.08f;
    }

    [Serializable]
    public sealed class SpawnEntry
    {
        public string ArchetypeId = string.Empty;
        public int Count = 1;
        public string SpawnPointTag = "ground";
        public float SpawnInterval = 0.6f;
        public bool ForceElite = false;
        public bool SpawnAtStart = true;
    }

    [Serializable]
    public sealed class EncounterWave
    {
        public string Name = string.Empty;

        /// <summary>Seconds after the encounter starts (or previous wave clears).</summary>
        public float Delay = 0f;

        /// <summary>Wait for the previous wave to be cleared before this one starts.</summary>
        public bool RequiresWaveClear = true;

        public List<SpawnEntry> Spawns = new List<SpawnEntry>();
    }

    [Serializable]
    public sealed class EncounterDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public int ChapterIndex = 1;
        public string ArenaId = string.Empty;
        public bool IsArena = false;
        public bool IsBossArena = false;
        public bool HasSavepoint = false;
        public string Objective = string.Empty;

        /// <summary>Roughly how long the fight should last; used for pacing validation.</summary>
        public int TargetSeconds = 90;

        public List<EncounterWave> Waves = new List<EncounterWave>();
        public string RewardTableId = string.Empty;
        public string MusicId = string.Empty;
    }

    /// <summary>A concrete spawn the runtime should perform.</summary>
    [Serializable]
    public struct PlannedSpawn
    {
        public string ArchetypeId;
        public string SpawnPointTag;
        public bool IsElite;
        public float Time;
        public int WaveIndex;
    }

    /// <summary>
    /// Turns encounters into concrete spawn schedules. Difficulty pressure, elite
    /// counts and the spawn multiplier are all resolved here, so the runtime
    /// contains no balance logic.
    /// </summary>
    public static class SpawnRules
    {
        public static List<PlannedSpawn> BuildSchedule(
            EncounterDefinition encounter,
            DifficultyProfile difficulty,
            int chapterIndex,
            Rng rng)
        {
            List<PlannedSpawn> plan = new List<PlannedSpawn>();
            if (encounter == null || encounter.Waves == null) return plan;

            DifficultyProfile profile = difficulty ?? DifficultyProfile.For(Difficulty.Normal);
            float spawnMul = MathLite.Max(0.3f, profile.SpawnMultiplier);
            int elitesAllowed = MathLite.Max(0, profile.EliteCount);
            float clock = 0f;

            for (int w = 0; w < encounter.Waves.Count; w++)
            {
                EncounterWave wave = encounter.Waves[w];
                if (wave == null) continue;

                clock += MathLite.Max(0f, wave.Delay);
                float waveClock = clock;

                for (int e = 0; e < wave.Spawns.Count; e++)
                {
                    SpawnEntry entry = wave.Spawns[e];
                    if (entry == null || string.IsNullOrEmpty(entry.ArchetypeId)) continue;

                    int count = MathLite.Max(0, MathLite.RoundToInt(entry.Count * spawnMul));
                    if (count == 0 && entry.Count > 0) count = 1;

                    for (int i = 0; i < count; i++)
                    {
                        bool elite = entry.ForceElite ||
                                     (elitesAllowed > 0 && IsEliteSlot(plan, w, rng, elitesAllowed));
                        plan.Add(new PlannedSpawn
                        {
                            ArchetypeId = entry.ArchetypeId,
                            SpawnPointTag = string.IsNullOrEmpty(entry.SpawnPointTag) ? "ground" : entry.SpawnPointTag,
                            IsElite = elite,
                            Time = waveClock + entry.SpawnInterval * i,
                            WaveIndex = w
                        });
                    }
                }
            }

            plan.Sort((a, b) => a.Time.CompareTo(b.Time));
            return plan;
        }

        private static bool IsEliteSlot(List<PlannedSpawn> plan, int waveIndex, Rng rng, int elitesAllowed)
        {
            int elitesInWave = 0;
            for (int i = 0; i < plan.Count; i++)
            {
                if (plan[i].WaveIndex == waveIndex && plan[i].IsElite) elitesInWave++;
            }
            if (elitesInWave >= elitesAllowed) return false;
            // Roughly one in six spawns becomes an elite, never the first of a wave.
            return plan.Count > 0 && rng != null && rng.Chance(0.16f);
        }

        /// <summary>Total spawns in a schedule; used for pacing validation.</summary>
        public static int Count(List<PlannedSpawn> plan)
        {
            return plan == null ? 0 : plan.Count;
        }

        /// <summary>Hard cap on simultaneously live enemies to protect frame time.</summary>
        public static int MaxConcurrentEnemies(DifficultyProfile difficulty)
        {
            float mul = difficulty == null ? 1f : difficulty.SpawnMultiplier;
            return MathLite.Clamp(MathLite.RoundToInt(14f * mul), 6, 28);
        }
    }
}
