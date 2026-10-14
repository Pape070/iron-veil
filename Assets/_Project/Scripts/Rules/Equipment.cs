using System;
using System.Collections.Generic;

namespace IronVeil.Rules
{
    public enum WeaponKind
    {
        Sidearm = 0,
        Rifle = 1,
        Shotgun = 2,
        Lmg = 3,
        RocketLauncher = 4,
        RailPistol = 5,
        Smg = 6,
        ArcEmitter = 7,
        GrenadeLauncher = 8,
        RotaryCannon = 9
    }

    public enum FireMode
    {
        Automatic = 0,
        SemiAutomatic = 1,
        Burst = 2,
        Charge = 3,
        Beam = 4
    }

    public enum Rarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3
    }

    /// <summary>A single gun. Six weapons per character, all rebindable and swap-able.</summary>
    [Serializable]
    public sealed class WeaponDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public WeaponKind Kind = WeaponKind.Rifle;
        public FireMode Mode = FireMode.Automatic;

        /// <summary>Character who starts with it; None means both can use it.</summary>
        public CharacterId ExclusiveTo = CharacterId.None;

        public float DamagePerShot = 9f;
        public float PowerScaling = 1f;
        public float ArmorPierce = 0f;
        public float RoundsPerSecond = 7f;

        public int PelletsPerShot = 1;
        public float SpreadDegrees = 2f;
        public int MagazineSize = 30;
        public float ReloadTime = 1.6f;
        public int BurstCount = 1;
        public float BurstInterval = 0.06f;
        public float ChargeTime = 0f;
        public int AmmoPerPickup = 45;
        public int MaxReserveAmmo = 360;
        public float ProjectileSpeed = 26f;
        public float Range = 22f;
        public float Recoil = 0.6f;
        public float Knockback = 2f;
        public bool Pierces = false;
        public bool IsStarter = false;
        public int Tier = 1;

        /// <summary>Salvage cost to raise this weapon one upgrade level.</summary>
        public int UpgradeSalvageCost = 120;
        public int MaxUpgradeLevel = 5;

        /// <summary>True when the weapon is earned from a mini-boss or a chapter.</summary>
        public string UnlockSource = string.Empty;
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

    /// <summary>Derived weapon numbers. Keeps the UI, tests and combat in agreement.</summary>
    public static class WeaponMath
    {
        /// <summary>Shots per second after the Fire Rate stat.</summary>
        public static float ShotsPerSecond(WeaponDefinition weapon, StatBlock stats)
        {
            if (weapon == null) return 0f;
            float fireRate = stats == null ? 1f : stats.FireRateMul;
            return MathLite.Max(0.05f, weapon.RoundsPerSecond * fireRate);
        }

        /// <summary>Seconds between shots.</summary>
        public static float SecondsBetweenShots(WeaponDefinition weapon, StatBlock stats)
        {
            float sps = ShotsPerSecond(weapon, stats);
            return sps <= 0f ? 0f : 1f / sps;
        }

        public static float ReloadTime(WeaponDefinition weapon, StatBlock stats)
        {
            if (weapon == null) return 0f;
            float reloadSpeed = stats == null ? 1f : stats.ReloadSpeedMul;
            return weapon.ReloadTime / MathLite.Max(0.25f, reloadSpeed);
        }

        /// <summary>Seconds to empty a full magazine.</summary>
        public static float MagazineDuration(WeaponDefinition weapon, StatBlock stats)
        {
            if (weapon == null) return 0f;
            float sps = ShotsPerSecond(weapon, stats);
            float burstDuration = weapon.BurstCount > 1
                ? (weapon.BurstCount - 1) * weapon.BurstInterval
                : 0f;
            float cycles = weapon.MagazineSize / (float)MathLite.Max(1, weapon.BurstCount);
            return cycles / MathLite.Max(0.05f, sps) + cycles * burstDuration;
        }

        /// <summary>Sustained DPS including reload downtime and weak-point-free hits.</summary>
        public static float SustainedDps(WeaponDefinition weapon, StatBlock stats, float targetDefense)
        {
            if (weapon == null) return 0f;
            DamageRequest request = DamageRequest.FromWeapon(weapon, stats);
            float perShot = DamageRules.ExpectedDamage(request, targetDefense) * MathLite.Max(1, weapon.PelletsPerShot);
            float magazine = MathLite.Max(1, weapon.MagazineSize);
            float fireWindow = MagazineDuration(weapon, stats);
            float reload = ReloadTime(weapon, stats);
            float cycle = fireWindow + reload;
            if (cycle <= 0f) return 0f;
            return perShot * magazine / cycle;
        }

        /// <summary>Damage multiplier from weapon upgrade level (1-5).</summary>
        public static float UpgradeDamageMultiplier(int upgradeLevel)
        {
            return 1f + 0.12f * (MathLite.Clamp(upgradeLevel, 1, 5) - 1);
        }

        /// <summary>Salvage cost of the next upgrade level, or -1 when maxed.</summary>
        public static int NextUpgradeCost(WeaponDefinition weapon, int currentLevel)
        {
            if (weapon == null) return -1;
            if (currentLevel >= weapon.MaxUpgradeLevel) return -1;
            float scale = 1f + 0.65f * currentLevel;
            return MathLite.RoundToInt(weapon.UpgradeSalvageCost * scale);
        }

        /// <summary>Shot spread in degrees including the weapon's accuracy weakness on the move.</summary>
        public static float SpreadDegrees(WeaponDefinition weapon, bool moving, bool airborne)
        {
            if (weapon == null) return 0f;
            float spread = weapon.SpreadDegrees;
            if (moving) spread *= 1.35f;
            if (airborne) spread *= 1.6f;
            return spread;
        }
    }

    /// <summary>Armor, shield cores and trinkets. Slotted and swappable at savepoints.</summary>
    [Serializable]
    public sealed class GearDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public EquipSlot Slot = EquipSlot.Armor;
        public Rarity Rarity = Rarity.Common;
        public int Tier = 1;
        public StatBlock Bonus = new StatBlock();

        /// <summary>Chapter that makes this gear available, 1-12.</summary>
        public int ChapterAvailable = 1;

        /// <summary>Mini-boss id that drops it, when it is not shop/story gear.</summary>
        public string DroppedBy = string.Empty;

        public bool IsCharacterSpecific = false;
        public CharacterId ExclusiveTo = CharacterId.None;

        public string[] Tags = Array.Empty<string>();
    }

    /// <summary>Weighted drop table entry.</summary>
    [Serializable]
    public sealed class DropEntry
    {
        public string GearId = string.Empty;
        public float Weight = 1f;
        public Rarity MinimumRarity = Rarity.Common;
    }

    [Serializable]
    public sealed class DropTable
    {
        public string Id = string.Empty;
        public List<DropEntry> Entries = new List<DropEntry>();
        public int SalvageMin = 10;
        public int SalvageMax = 30;

        public List<float> Weights()
        {
            List<float> weights = new List<float>(Entries.Count);
            for (int i = 0; i < Entries.Count; i++)
            {
                weights.Add(Entries[i] == null ? 0f : MathLite.Max(0f, Entries[i].Weight));
            }
            return weights;
        }
    }

    /// <summary>Loot rolls for elites, mini-bosses and chests.</summary>
    public static class LootRules
    {
        /// <summary>Rarity weights shift upward with chapter depth, Luck and difficulty.</summary>
        public static float[] RarityWeights(int chapterIndex, float luck, float rarityBonus)
        {
            float depth = MathLite.Clamp01((chapterIndex - 1) / 11f);
            float luckFactor = MathLite.Clamp01(luck / 200f);
            float rare = 22f + 30f * depth + 18f * luckFactor + 30f * rarityBonus;
            float epic = 6f + 18f * depth + 12f * luckFactor + 20f * rarityBonus;
            float legendary = 1f + 6f * depth + 5f * luckFactor + 12f * rarityBonus;
            float common = 100f;
            return new[] { common, rare, epic, legendary };
        }

        public static Rarity RollRarity(int chapterIndex, float luck, float rarityBonus, Rng rng)
        {
            int index = rng.WeightedIndex(RarityWeights(chapterIndex, luck, rarityBonus));
            return (Rarity)MathLite.Clamp(index < 0 ? 0 : index, 0, 3);
        }

        /// <summary>Salvage (currency for weapon upgrades) from a defeated elite or boss.</summary>
        public static int SalvageReward(int chapterIndex, int tier, bool isBoss, bool isMiniBoss, float luck, Rng rng)
        {
            float chapterMul = 1f + 0.55f * MathLite.Max(0, chapterIndex - 1);
            float baseAmount = isBoss ? 260f : isMiniBoss ? 140f : 18f * MathLite.Max(1, tier);
            float luckMul = 1f + MathLite.Clamp01(luck / 200f) * 0.25f;
            float roll = rng == null ? 1f : rng.Range(0.85f, 1.15f);
            return MathLite.Max(1, MathLite.RoundToInt(baseAmount * chapterMul * luckMul * roll));
        }

        /// <summary>Health flask value scaled with chapter depth.</summary>
        public static float FlaskHealAmount(int chapterIndex, float maxHealth)
        {
            return maxHealth * (0.3f + 0.012f * MathLite.Max(0, chapterIndex - 1));
        }

        /// <summary>Picks a gear id from a table; returns null when the table is empty.</summary>
        public static string RollGear(DropTable table, Rng rng)
        {
            if (table == null || table.Entries == null || table.Entries.Count == 0) return null;
            int index = rng.WeightedIndex(table.Weights());
            if (index < 0) return null;
            return table.Entries[index].GearId;
        }

        /// <summary>Chance for an elite or mini-boss to drop its signature upgrade.</summary>
        public static bool RollsUpgradeDrop(float baseChance, DifficultyProfile difficulty, float luck)
        {
            float chance = baseChance * (difficulty == null ? 1f : difficulty.LootChanceMultiplier);
            chance += MathLite.Clamp01(luck / 400f) * 0.15f;
            return chance >= 1f;
        }
    }
}
