using System;
using System.Globalization;
using IronVeil.Rules;

namespace IronVeil.Tools
{
    /// <summary>
    /// Prints the balance table used to tune boss health, enemy damage and the XP
    /// curve. Also runs the content validator so a broken reference is visible
    /// without opening Unity.
    /// </summary>
    internal static class Program
    {
        private static void Main(string[] args)
        {
            CampaignContent content = Campaign.Build();
            CultureInfo culture = CultureInfo.InvariantCulture;

            Console.WriteLine("=== IRON VEIL balance report ===");
            Console.WriteLine("Chapters: {0}   Comic pages: {1}   Panels: {2}",
                content.Chapters.Count, content.TotalComicPages(), content.TotalComicPanels());
            Console.WriteLine("Bosses: {0} (chapter {1} / mini {2})   Enemies: {3}   Quests: {4}   Weapons: {5}   Gear: {6}",
                content.Bosses.Count,
                CountBosses(content, false),
                CountBosses(content, true),
                content.Enemies.Count,
                content.Quests.Count,
                content.Weapons.Count,
                content.GearItems.Count);
            Console.WriteLine("Main path: {0} min ({1:0.0} h)", content.TotalMinutes(), content.TotalMinutes() / 60.0);

            foreach (CharacterId id in new[] { CharacterId.Kade, CharacterId.Vera })
            {
                Console.WriteLine();
                Console.WriteLine("--- {0} ---", id);
                Console.WriteLine("{0,-4} {1,-5} {2,-8} {3,-7} {4,-7} {5,-7} {6,-16} {7,-9} {8,-8} {9,-8} {10,-8}",
                    "Ch", "Lvl", "Power", "Def", "HP", "Shield", "Weapon", "DPS", "BossHP", "TTK", "HitsToDie");

                for (int chapter = 1; chapter <= ChapterRules.ChapterCount; chapter++)
                {
                    StatBlock stats = ContentValidator.PlayerStatsAt(content, id, chapter);
                    WeaponDefinition weapon = ContentValidator.WeaponForChapter(content, id, chapter);
                    BossDefinition boss = content.Boss(content.Chapter(chapter).BossId);
                    DifficultyProfile normal = DifficultyProfile.For(Difficulty.Normal);

                    float dps = weapon == null ? 0f : WeaponMath.SustainedDps(weapon, stats, boss.BaseDefense);
                    float bossHealth = boss.BaseHealth * normal.EnemyHealthMultiplier * ChapterRules.EnemyHealthMultiplier(chapter);
                    float ttk = dps <= 0f ? 999f : bossHealth / dps;

                    EnemyArchetype baseline = BaselineEnemy(content, chapter);
                    float perHit = EnemyScaling.Damage(baseline, chapter, normal) * DamageRules.MitigationMultiplier(stats.Defense);
                    float hits = perHit <= 0f ? 999f : (stats.MaxHealth + stats.Shield) / perHit;

                    Console.WriteLine("{0,-4} {1,-5} {2,-8:0} {3,-7:0} {4,-7:0} {5,-7:0} {6,-16} {7,-9:0} {8,-8:0} {9,-8:0.0} {10,-8:0.0}",
                        chapter,
                        ChapterRules.RecommendedLevel(chapter),
                        stats.Power,
                        stats.Defense,
                        stats.MaxHealth,
                        stats.Shield,
                        weapon == null ? "-" : weapon.Id,
                        dps,
                        bossHealth,
                        ttk,
                        hits);
                }
            }

            Console.WriteLine();
            Console.WriteLine("=== mini-boss / boss health audit (Normal) ===");
            for (int i = 0; i < content.Bosses.Count; i++)
            {
                BossDefinition boss = content.Bosses[i];
                StatBlock stats = ContentValidator.PlayerStatsAt(content, CharacterId.Kade, boss.ChapterIndex);
                WeaponDefinition weapon = ContentValidator.WeaponForChapter(content, CharacterId.Kade, boss.ChapterIndex);
                float dps = weapon == null ? 0f : WeaponMath.SustainedDps(weapon, stats, boss.BaseDefense);
                float health = boss.BaseHealth * ChapterRules.EnemyHealthMultiplier(boss.ChapterIndex);
                Console.WriteLine("ch{0,2} {1,-22} {2,-10} hp {3,8:0}  ttk {4,6:0.0}s",
                    boss.ChapterIndex, boss.Id, boss.IsMiniBoss ? "mini" : "chapter", health, dps <= 0f ? 999f : health / dps);
            }

            Console.WriteLine();
            Console.WriteLine("=== XP progression ===");
            foreach (Difficulty difficulty in new[] { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard })
            {
                DifficultyProfile profile = DifficultyProfile.For(difficulty);
                int xp = ChapterRules.TotalMainPathXp(170, profile);
                Console.WriteLine("{0,-7} main-path XP {1,9}  -> level {2}  (with side quests ~{3})",
                    difficulty, xp, XpCurve.LevelFromTotalXp(xp), XpCurve.LevelFromTotalXp(xp + 60000));
            }

            Console.WriteLine();
            ValidationReport report = ContentValidator.Validate(content);
            Console.WriteLine(report.Summary(200));

            Environment.ExitCode = report.IsValid ? 0 : 1;
        }

        private static EnemyArchetype BaselineEnemy(CampaignContent content, int chapter)
        {
            EnemyArchetype best = null;
            for (int i = 0; i < content.Enemies.Count; i++)
            {
                EnemyArchetype enemy = content.Enemies[i];
                if (enemy.FirstChapter > chapter || enemy.IsSupport) continue;
                if (best == null || enemy.Tier > best.Tier) best = enemy;
            }
            return best;
        }

        private static int CountBosses(CampaignContent content, bool mini)
        {
            int count = 0;
            for (int i = 0; i < content.Bosses.Count; i++)
            {
                if (content.Bosses[i].IsMiniBoss == mini) count++;
            }
            return count;
        }
    }
}
