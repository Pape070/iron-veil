using System;
using System.Collections.Generic;

namespace IronVeil.Rules
{
    public enum AbilityKind
    {
        None = 0,
        Bulwark = 1,
        GroundSlam = 2,
        PhaseBlink = 3,
        Overclock = 4
    }

    public enum AbilityActivation
    {
        Instant = 0,
        Deployable = 1,
        Dash = 2,
        SelfBuff = 3
    }

    public enum PassiveKind
    {
        None = 0,
        Reinforced = 1,
        Momentum = 2
    }

    [Serializable]
    public sealed class AbilityDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public AbilityKind Kind = AbilityKind.None;
        public AbilityActivation Activation = AbilityActivation.Instant;

        public float Cooldown = 10f;
        public float Duration = 0f;

        /// <summary>Area of effect radius in world units.</summary>
        public float Radius = 3f;

        /// <summary>Travel distance for dash abilities.</summary>
        public float Distance = 0f;

        public float Damage = 0f;
        public float PowerScaling = 1f;
        public float ShieldAmount = 0f;
        public float FireRateBonus = 0f;
        public float StunSeconds = 0f;
        public float InvulnerabilitySeconds = 0f;

        /// <summary>Number of stored charges before the cooldown gates usage.</summary>
        public int Charges = 1;

        public string[] Tags = Array.Empty<string>();

        public bool HasTag(string tag)
        {
            if (Tags == null) return false;
            for (int i = 0; i < Tags.Length; i++)
            {
                if (string.Equals(Tags[i], tag, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }

    [Serializable]
    public sealed class PassiveDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public PassiveKind Kind = PassiveKind.None;

        /// <summary>Kade: damage taken multiplier while braced. 0.8 == -20% damage.</summary>
        public float DamageTakenMultiplier = 0.8f;

        /// <summary>Seconds the player must hold still to brace.</summary>
        public float StationaryThreshold = 0.5f;

        /// <summary>Vera: damage bonus per consecutive hit.</summary>
        public float PerStackBonus = 0.03f;

        public int MaxStacks = 10;

        /// <summary>Seconds without a hit before the stack resets.</summary>
        public float StackDecaySeconds = 3f;
    }

    [Serializable]
    public sealed class CharacterDefinition
    {
        public CharacterId Id = CharacterId.None;
        public string Name = string.Empty;
        public string Callsign = string.Empty;
        public string Role = string.Empty;
        public string Bio = string.Empty;

        public StatBlock BaseStats = new StatBlock();

        /// <summary>Delta block per level: 0 means the stat does not grow.</summary>
        public StatBlock PerLevelGrowth = StatBlock.Delta();

        public string SignatureWeaponId = string.Empty;
        public string[] StartingWeaponIds = Array.Empty<string>();
        public string[] UnlockableWeaponIds = Array.Empty<string>();
        public string[] UnlockableGearIds = Array.Empty<string>();

        public AbilityDefinition PrimaryAbility = new AbilityDefinition();
        public AbilityDefinition SecondaryAbility = new AbilityDefinition();
        public PassiveDefinition Passive = new PassiveDefinition();

        /// <summary>Chapter index at which the second ability unlocks.</summary>
        public int SecondAbilityUnlockChapter = 4;

        public string[] LevelUpBarks = Array.Empty<string>();
        public string[] LowHealthBarks = Array.Empty<string>();
        public string[] BossIntroLines = Array.Empty<string>();

        /// <summary>Palette key read by the presentation layer; keeps art data out of rules.</summary>
        public string PaletteId = "default";

        public string[] Tags = Array.Empty<string>();
    }

    /// <summary>Numbers behind the abilities, so UI tooltips and combat agree.</summary>
    public static class AbilityMath
    {
        public static float Cooldown(AbilityDefinition ability, StatBlock stats)
        {
            if (ability == null) return 0f;
            float power = stats == null ? 1f : MathLite.Max(0.1f, stats.AbilityPower);
            return ability.Cooldown / (0.85f + 0.15f * power);
        }

        public static float Damage(AbilityDefinition ability, StatBlock stats)
        {
            if (ability == null) return 0f;
            float power = stats == null ? 1f : MathLite.Max(0.1f, stats.AbilityPower);
            return ability.Damage * power;
        }

        public static float ShieldAmount(AbilityDefinition ability, StatBlock stats)
        {
            if (ability == null) return 0f;
            float power = stats == null ? 1f : MathLite.Max(0.1f, stats.AbilityPower);
            return ability.ShieldAmount * power;
        }

        public static float Duration(AbilityDefinition ability, StatBlock stats)
        {
            if (ability == null) return 0f;
            float power = stats == null ? 1f : stats.AbilityPower;
            return ability.Duration * (1f + 0.1f * (power - 1f));
        }

        public static float FireRateBonus(AbilityDefinition ability, StatBlock stats)
        {
            if (ability == null) return 0f;
            float power = stats == null ? 1f : MathLite.Max(0.1f, stats.AbilityPower);
            return ability.FireRateBonus * power;
        }

        public static float Radius(AbilityDefinition ability, StatBlock stats)
        {
            if (ability == null) return 0f;
            float power = stats == null ? 1f : stats.AbilityPower;
            return ability.Radius * MathLite.Min(1.35f, 1f + 0.08f * (power - 1f));
        }
    }

    /// <summary>Passive behaviour for both characters. Pure so it can be unit tested.</summary>
    public static class PassiveMath
    {
        public static float DamageTakenMultiplier(PassiveDefinition passive, float secondsStationary)
        {
            if (passive == null || passive.Kind != PassiveKind.Reinforced) return 1f;
            return secondsStationary >= passive.StationaryThreshold ? passive.DamageTakenMultiplier : 1f;
        }

        /// <summary>Adds a stack (capped) and returns the outgoing damage multiplier.</summary>
        public static float AddStackAndGetMultiplier(PassiveDefinition passive, ref int stacks)
        {
            if (passive == null || passive.Kind != PassiveKind.Momentum)
            {
                stacks = 0;
                return 1f;
            }
            stacks = MathLite.Clamp(stacks + 1, 0, passive.MaxStacks);
            return 1f + passive.PerStackBonus * stacks;
        }

        public static float DamageMultiplier(PassiveDefinition passive, int stacks)
        {
            if (passive == null || passive.Kind != PassiveKind.Momentum) return 1f;
            return 1f + passive.PerStackBonus * MathLite.Clamp(stacks, 0, passive.MaxStacks);
        }

        /// <summary>Stacks decay to zero after the decay window without a hit.</summary>
        public static int DecayStacks(PassiveDefinition passive, int stacks, float secondsSinceHit)
        {
            if (passive == null || passive.Kind != PassiveKind.Momentum) return 0;
            return secondsSinceHit >= passive.StackDecaySeconds ? 0 : stacks;
        }

        public static string StackLabel(PassiveDefinition passive, int stacks)
        {
            if (passive == null || passive.Kind != PassiveKind.Momentum) return string.Empty;
            return "Momentum x" + MathLite.Clamp(stacks, 0, passive.MaxStacks);
        }
    }

    /// <summary>Late-campaign perks unlocked by level. Adds build identity beyond stat points.</summary>
    [Serializable]
    public sealed class PerkDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public int UnlockLevel = 10;
        public CharacterId ExclusiveTo = CharacterId.None;

        /// <summary>Stat bonus applied while the perk is active (delta block).</summary>
        public StatBlock Bonus = StatBlock.Delta();

        /// <summary>Free-form effect keys read by the runtime (e.g. "reload_on_kill").</summary>
        public string EffectKey = string.Empty;
        public float EffectValue = 0f;
    }
}
