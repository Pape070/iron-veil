using System;
using System.Collections.Generic;

namespace IronVeil.Rules
{
    /// <summary>
    /// Indexed catalog of every authored definition. Built in code (see Campaign)
    /// and validated by ContentValidator, so a broken reference is caught by tests
    /// rather than at runtime.
    /// </summary>
    public sealed class CampaignContent
    {
        public string Title = "Iron Veil";
        public string Subtitle = string.Empty;
        public string PlotSummary = string.Empty;

        public readonly List<CharacterDefinition> Characters = new List<CharacterDefinition>();
        public readonly List<WeaponDefinition> Weapons = new List<WeaponDefinition>();
        public readonly List<GearDefinition> GearItems = new List<GearDefinition>();
        public readonly List<EnemyArchetype> Enemies = new List<EnemyArchetype>();
        public readonly List<BossDefinition> Bosses = new List<BossDefinition>();
        public readonly List<QuestDefinition> Quests = new List<QuestDefinition>();
        public readonly List<ChapterDefinition> Chapters = new List<ChapterDefinition>();
        public readonly List<EncounterDefinition> Encounters = new List<EncounterDefinition>();
        public readonly List<PerkDefinition> Perks = new List<PerkDefinition>();
        public readonly List<DropTable> DropTables = new List<DropTable>();

        private readonly Dictionary<CharacterId, CharacterDefinition> _charactersById = new Dictionary<CharacterId, CharacterDefinition>();
        private readonly Dictionary<string, WeaponDefinition> _weaponsById = new Dictionary<string, WeaponDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GearDefinition> _gearById = new Dictionary<string, GearDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EnemyArchetype> _enemiesById = new Dictionary<string, EnemyArchetype>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, BossDefinition> _bossesById = new Dictionary<string, BossDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, QuestDefinition> _questsById = new Dictionary<string, QuestDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EncounterDefinition> _encountersById = new Dictionary<string, EncounterDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PerkDefinition> _perksById = new Dictionary<string, PerkDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DropTable> _dropTablesById = new Dictionary<string, DropTable>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, ChapterDefinition> _chaptersByIndex = new Dictionary<int, ChapterDefinition>();
        private readonly Dictionary<string, ComicPageDefinition> _comicPagesById = new Dictionary<string, ComicPageDefinition>(StringComparer.OrdinalIgnoreCase);

        private bool _indexed;

        /// <summary>Builds the shipped campaign content.</summary>
        public static CampaignContent Build()
        {
            return Campaign.Build();
        }

        /// <summary>Rebuilds lookup dictionaries. Called automatically on first lookup.</summary>
        public void Index()
        {
            _charactersById.Clear();
            _weaponsById.Clear();
            _gearById.Clear();
            _enemiesById.Clear();
            _bossesById.Clear();
            _questsById.Clear();
            _encountersById.Clear();
            _perksById.Clear();
            _dropTablesById.Clear();
            _chaptersByIndex.Clear();
            _comicPagesById.Clear();

            for (int i = 0; i < Characters.Count; i++)
            {
                CharacterDefinition character = Characters[i];
                if (character != null) _charactersById[character.Id] = character;
            }
            for (int i = 0; i < Weapons.Count; i++)
            {
                WeaponDefinition weapon = Weapons[i];
                if (weapon != null && !string.IsNullOrEmpty(weapon.Id)) _weaponsById[weapon.Id] = weapon;
            }
            for (int i = 0; i < GearItems.Count; i++)
            {
                GearDefinition gear = GearItems[i];
                if (gear != null && !string.IsNullOrEmpty(gear.Id)) _gearById[gear.Id] = gear;
            }
            for (int i = 0; i < Enemies.Count; i++)
            {
                EnemyArchetype enemy = Enemies[i];
                if (enemy != null && !string.IsNullOrEmpty(enemy.Id)) _enemiesById[enemy.Id] = enemy;
            }
            for (int i = 0; i < Bosses.Count; i++)
            {
                BossDefinition boss = Bosses[i];
                if (boss != null && !string.IsNullOrEmpty(boss.Id)) _bossesById[boss.Id] = boss;
            }
            for (int i = 0; i < Quests.Count; i++)
            {
                QuestDefinition quest = Quests[i];
                if (quest != null && !string.IsNullOrEmpty(quest.Id)) _questsById[quest.Id] = quest;
            }
            for (int i = 0; i < Encounters.Count; i++)
            {
                EncounterDefinition encounter = Encounters[i];
                if (encounter != null && !string.IsNullOrEmpty(encounter.Id)) _encountersById[encounter.Id] = encounter;
            }
            for (int i = 0; i < Perks.Count; i++)
            {
                PerkDefinition perk = Perks[i];
                if (perk != null && !string.IsNullOrEmpty(perk.Id)) _perksById[perk.Id] = perk;
            }
            for (int i = 0; i < DropTables.Count; i++)
            {
                DropTable table = DropTables[i];
                if (table != null && !string.IsNullOrEmpty(table.Id)) _dropTablesById[table.Id] = table;
            }
            for (int i = 0; i < Chapters.Count; i++)
            {
                ChapterDefinition chapter = Chapters[i];
                if (chapter == null) continue;
                _chaptersByIndex[chapter.Index] = chapter;
                if (chapter.ComicPages == null) continue;
                for (int p = 0; p < chapter.ComicPages.Count; p++)
                {
                    ComicPageDefinition page = chapter.ComicPages[p];
                    if (page != null && !string.IsNullOrEmpty(page.Id)) _comicPagesById[page.Id] = page;
                }
            }

            _indexed = true;
        }

        private void EnsureIndexed()
        {
            if (!_indexed) Index();
        }

        public CharacterDefinition Character(CharacterId id)
        {
            EnsureIndexed();
            return _charactersById.TryGetValue(id, out CharacterDefinition character) ? character : null;
        }

        public WeaponDefinition Weapon(string id)
        {
            EnsureIndexed();
            if (string.IsNullOrEmpty(id)) return null;
            return _weaponsById.TryGetValue(id, out WeaponDefinition weapon) ? weapon : null;
        }

        public GearDefinition Gear(string id)
        {
            EnsureIndexed();
            if (string.IsNullOrEmpty(id)) return null;
            return _gearById.TryGetValue(id, out GearDefinition gear) ? gear : null;
        }

        public EnemyArchetype Enemy(string id)
        {
            EnsureIndexed();
            if (string.IsNullOrEmpty(id)) return null;
            return _enemiesById.TryGetValue(id, out EnemyArchetype enemy) ? enemy : null;
        }

        public BossDefinition Boss(string id)
        {
            EnsureIndexed();
            if (string.IsNullOrEmpty(id)) return null;
            return _bossesById.TryGetValue(id, out BossDefinition boss) ? boss : null;
        }

        public QuestDefinition Quest(string id)
        {
            EnsureIndexed();
            if (string.IsNullOrEmpty(id)) return null;
            return _questsById.TryGetValue(id, out QuestDefinition quest) ? quest : null;
        }

        public EncounterDefinition Encounter(string id)
        {
            EnsureIndexed();
            if (string.IsNullOrEmpty(id)) return null;
            return _encountersById.TryGetValue(id, out EncounterDefinition encounter) ? encounter : null;
        }

        public PerkDefinition Perk(string id)
        {
            EnsureIndexed();
            if (string.IsNullOrEmpty(id)) return null;
            return _perksById.TryGetValue(id, out PerkDefinition perk) ? perk : null;
        }

        public DropTable DropTable(string id)
        {
            EnsureIndexed();
            if (string.IsNullOrEmpty(id)) return null;
            return _dropTablesById.TryGetValue(id, out DropTable table) ? table : null;
        }

        public ChapterDefinition Chapter(int index)
        {
            EnsureIndexed();
            return _chaptersByIndex.TryGetValue(index, out ChapterDefinition chapter) ? chapter : null;
        }

        public ComicPageDefinition FindComicPage(string id)
        {
            EnsureIndexed();
            if (string.IsNullOrEmpty(id)) return null;
            return _comicPagesById.TryGetValue(id, out ComicPageDefinition page) ? page : null;
        }

        public ChapterDefinition ChapterForBoss(string bossId)
        {
            BossDefinition boss = Boss(bossId);
            return boss == null ? null : Chapter(boss.ChapterIndex);
        }

        /// <summary>Weapons the given character can use, starting plus unlockable.</summary>
        public List<WeaponDefinition> WeaponsFor(CharacterId character)
        {
            List<WeaponDefinition> result = new List<WeaponDefinition>();
            for (int i = 0; i < Weapons.Count; i++)
            {
                WeaponDefinition weapon = Weapons[i];
                if (weapon == null) continue;
                if (weapon.ExclusiveTo == CharacterId.None || weapon.ExclusiveTo == character) result.Add(weapon);
            }
            return result;
        }

        public List<QuestDefinition> SideQuestsInChapter(int chapterIndex)
        {
            List<QuestDefinition> result = new List<QuestDefinition>();
            for (int i = 0; i < Quests.Count; i++)
            {
                QuestDefinition quest = Quests[i];
                if (quest != null && quest.ChapterIndex == chapterIndex && quest.Kind != QuestKind.MainQuest) result.Add(quest);
            }
            return result;
        }

        public List<BossDefinition> MiniBossesInChapter(int chapterIndex)
        {
            List<BossDefinition> result = new List<BossDefinition>();
            for (int i = 0; i < Bosses.Count; i++)
            {
                BossDefinition boss = Bosses[i];
                if (boss != null && boss.IsMiniBoss && boss.ChapterIndex == chapterIndex) result.Add(boss);
            }
            return result;
        }

        public int TotalComicPages()
        {
            int total = 0;
            for (int i = 0; i < Chapters.Count; i++)
            {
                if (Chapters[i] != null) total += Chapters[i].TotalComicPages;
            }
            return total;
        }

        public int TotalComicPanels()
        {
            int total = 0;
            for (int i = 0; i < Chapters.Count; i++)
            {
                if (Chapters[i] != null) total += Chapters[i].TotalPanels();
            }
            return total;
        }

        public int TotalMinutes()
        {
            int total = 0;
            for (int i = 0; i < Chapters.Count; i++)
            {
                if (Chapters[i] != null) total += Chapters[i].EstimatedMinutes;
            }
            return total;
        }
    }
}
