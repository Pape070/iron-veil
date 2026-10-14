using System;

namespace IronVeil.Rules
{
    /// <summary>Everything needed to resolve one hit.</summary>
    [Serializable]
    public struct DamageRequest
    {
        /// <summary>Flat damage from the weapon or enemy attack.</summary>
        public float BaseDamage;

        /// <summary>Attacker Power stat. Scaled against the level-1 reference.</summary>
        public float Power;

        /// <summary>How strongly Power influences this attack. 0 = flat damage weapon.</summary>
        public float PowerScaling;

        public float CritChance;
        public float CritDamageMultiplier;

        /// <summary>Weak point hits ignore part of the target's armor.</summary>
        public bool IsWeakPoint;

        public float ArmorPierce;

        /// <summary>Set for damage-over-time or environment damage that cannot crit.</summary>
        public bool CanCrit;

        public static DamageRequest FromWeapon(WeaponDefinition weapon, StatBlock stats, bool canCrit = true)
        {
            return new DamageRequest
            {
                BaseDamage = weapon == null ? 5f : weapon.DamagePerShot,
                Power = stats == null ? 25f : stats.Power,
                PowerScaling = weapon == null ? 1f : weapon.PowerScaling,
                CritChance = stats == null ? 0f : stats.CritChance,
                CritDamageMultiplier = stats == null ? 1.5f : stats.CritDamageMul,
                ArmorPierce = weapon == null ? 0f : weapon.ArmorPierce,
                CanCrit = canCrit
            };
        }
    }

    /// <summary>Result of one damage resolution.</summary>
    [Serializable]
    public struct DamageOutcome
    {
        public float Total;
        public float ShieldAbsorbed;
        public float HealthDamage;
        public bool IsCrit;
        public bool IsLethal;

        public bool AnyDamage => Total > 0f;
    }

    /// <summary>
    /// Mitigation uses a hyperbolic curve: defense / (defense + 100), hard capped
    /// at 75% so stacking Defense never trivialises the game.
    /// </summary>
    public static class DamageRules
    {
        public const float DefenseConstant = 100f;
        public const float MaxMitigation = 0.75f;

        /// <summary>Baseline Power for a level-1 character; used to scale weapon damage.</summary>
        public const float ReferencePower = 25f;

        /// <summary>Fraction of damage removed by armor, in [0, 0.75].</summary>
        public static float Mitigation(float defense)
        {
            float value = MathLite.Max(0f, defense);
            float mitigated = value / (value + DefenseConstant);
            return MathLite.Min(MaxMitigation, mitigated);
        }

        /// <summary>Effective damage multiplier applied to a target with the given armor.</summary>
        public static float MitigationMultiplier(float defense, float armorPierce = 0f)
        {
            float effectiveDefense = MathLite.Max(0f, defense * (1f - MathLite.Clamp01(armorPierce)));
            return 1f - Mitigation(effectiveDefense);
        }

        /// <summary>Weapon damage growth from the Power stat.</summary>
        public static float PowerMultiplier(float power, float powerScaling)
        {
            float ratio = MathLite.Max(0.1f, power / ReferencePower);
            return 1f + (ratio - 1f) * MathLite.Clamp01(powerScaling);
        }

        /// <summary>
        /// Resolves damage from the player (or any attacker) onto an enemy.
        /// </summary>
        public static DamageOutcome ComputeOutgoing(
            DamageRequest request,
            Rng rng,
            float targetDefense,
            float difficultyMultiplier,
            float targetHealth)
        {
            DamageOutcome outcome = new DamageOutcome();
            if (request.BaseDamage <= 0f) return outcome;

            float damage = request.BaseDamage * PowerMultiplier(request.Power, request.PowerScaling);

            bool crit = request.CanCrit && rng != null && rng.Chance(request.CritChance);
            if (crit)
            {
                damage *= MathLite.Max(1f, request.CritDamageMultiplier);
                outcome.IsCrit = true;
            }

            float pierce = request.ArmorPierce + (request.IsWeakPoint ? 0.35f : 0f);
            damage *= MitigationMultiplier(targetDefense, pierce);
            damage *= difficultyMultiplier;

            outcome.Total = MathLite.Max(0f, damage);
            outcome.HealthDamage = outcome.Total;
            outcome.IsLethal = outcome.Total >= targetHealth;
            return outcome;
        }

        /// <summary>
        /// Resolves damage onto the player: shields absorb first, then health.
        /// </summary>
        public static DamageOutcome ComputeIncoming(
            float enemyAttack,
            float playerDefense,
            float difficultyMultiplier,
            float currentShield,
            float currentHealth)
        {
            DamageOutcome outcome = new DamageOutcome();
            if (enemyAttack <= 0f) return outcome;

            float damage = enemyAttack * difficultyMultiplier * MitigationMultiplier(playerDefense);
            outcome.Total = MathLite.Max(0f, damage);

            float shield = MathLite.Max(0f, currentShield);
            outcome.ShieldAbsorbed = MathLite.Min(shield, outcome.Total);
            outcome.HealthDamage = outcome.Total - outcome.ShieldAbsorbed;
            outcome.IsLethal = outcome.HealthDamage >= currentHealth;
            return outcome;
        }

        /// <summary>
        /// Shield regeneration tick. Shields only recharge after a quiet period,
        /// so getting shot keeps them down.
        /// </summary>
        public static float RechargeShield(
            float current,
            float maximum,
            float regenPerSecond,
            float secondsSinceDamage,
            float regenDelaySeconds,
            float deltaTime)
        {
            if (maximum <= 0f || current >= maximum) return MathLite.Min(current, maximum);
            if (secondsSinceDamage < regenDelaySeconds) return current;
            float rate = regenPerSecond * deltaTime;
            return MathLite.Min(maximum, current + rate);
        }

        /// <summary>Health after a hit; health never goes below zero.</summary>
        public static float ApplyToHealth(float health, float damage)
        {
            return MathLite.Max(0f, health - MathLite.Max(0f, damage));
        }

        /// <summary>Health after healing, clamped to the maximum.</summary>
        public static float ApplyHealing(float health, float amount, float maximum)
        {
            return MathLite.Min(MathLite.Max(0f, maximum), health + MathLite.Max(0f, amount));
        }

        /// <summary>
        /// Expected damage per shot including crit average — used by tests and the
        /// balance validator to compare weapons fairly.
        /// </summary>
        public static float ExpectedDamage(DamageRequest request, float targetDefense)
        {
            float damage = request.BaseDamage * PowerMultiplier(request.Power, request.PowerScaling);
            if (request.CanCrit)
            {
                float crit = MathLite.Clamp01(request.CritChance);
                damage *= 1f - crit + crit * MathLite.Max(1f, request.CritDamageMultiplier);
            }
            return damage * MitigationMultiplier(targetDefense, request.ArmorPierce);
        }

        /// <summary>Number of hits needed to kill a target; useful for balance assertions.</summary>
        public static int HitsToKill(DamageRequest request, float targetDefense, float targetHealth)
        {
            float perHit = ExpectedDamage(request, targetDefense);
            if (perHit <= 0f) return int.MaxValue;
            return (int)Math.Ceiling(targetHealth / perHit);
        }
    }
}
