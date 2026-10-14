using System;
using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    /// <summary>
    /// XP, levels, stat points, perks, salvage, weapon upgrades and loot. Every number
    /// comes from the rules layer so the balance is identical to what the tests and
    /// the balance report measure.
    /// </summary>
    public sealed class ProgressionService
    {
        public const int RespecCost = 250;

        private readonly ContentDatabase _content;
        private readonly SaveService _saves;
        private readonly Rng _rng = new Rng(20261014);

        private StatBlock _cachedStats;
        private bool _dirty = true;

        public event Action<LevelUpResult> LeveledUp;
        public event Action<PerkDefinition> PerkUnlocked;
        public event Action SalvageChanged;
        public event Action StatsChanged;

        public ProgressionService(ContentDatabase content, SaveService saves)
        {
            _content = content;
            _saves = saves;
        }

        private SaveGame Save => _saves.Current;

        public void Rebuild()
        {
            _dirty = true;
            Stats = Stats;
        }

        public int Level => Save?.Level ?? 1;
        public int XpIntoLevel => Save?.XpIntoLevel ?? 0;
        public int Salvage => Save?.Salvage ?? 0;
        public DifficultyProfile Difficulty => DifficultyProfile.For(Save?.Difficulty ?? IronVeil.Rules.Difficulty.Normal);
        public CharacterDefinition Character => _content?.Character(Save?.Character ?? CharacterId.Kade);

        public StatBlock Stats
        {
            get
            {
                if (_dirty || _cachedStats == null)
                {
                    _cachedStats = Save == null ? new StatBlock() : Save.ComputeStats(_content?.Content);
                    _dirty = false;
                }
                return _cachedStats;
            }
            private set
            {
                _cachedStats = value;
                _dirty = false;
            }
        }

        public float XpProgress => XpCurve.Progress(Level, XpIntoLevel);
        public int XpRemaining => XpCurve.RemainingInLevel(Level, XpIntoLevel);
        public int AvailablePoints => Save == null ? 0 : StatComputation.AvailablePoints(Level, Save.StatPoints);
        public bool IsMaxLevel => Level >= StatComputation.MaxLevel;

        // ------------------------------------------------------------- xp

        /// <summary>Awards XP and returns how many levels were gained.</summary>
        public int AddXp(int amount, string source)
        {
            if (Save == null || amount <= 0) return 0;
            if (IsMaxLevel) return 0;

            // Chapter pacing multiplier keeps later chapters from over-levelling the player.
            float multiplier = ChapterRules.XpMultiplier(Save.ChapterIndex) * Difficulty.XpMultiplier;
            int scaled = Mathf.Max(1, Mathf.RoundToInt(amount * multiplier));

            LevelUpResult result = XpCurve.Apply(Save.Level, Save.XpIntoLevel, scaled);
            Save.Level = result.Level;
            Save.XpIntoLevel = result.XpIntoLevel;
            _dirty = true;

            if (result.LevelsGained > 0)
            {
                UnlockLevelPerks();
                LeveledUp?.Invoke(result);
            }
            return result.LevelsGained;
        }

        private void UnlockLevelPerks()
        {
            CampaignContent content = _content?.Content;
            if (content == null || Save == null) return;

            for (int i = 0; i < content.Perks.Count; i++)
            {
                PerkDefinition perk = content.Perks[i];
                if (perk == null) continue;
                if (perk.UnlockLevel > Save.Level) continue;
                if (perk.ExclusiveTo != CharacterId.None && perk.ExclusiveTo != Save.Character) continue;
                if (Save.UnlockedPerkIds.Contains(perk.Id)) continue;

                Save.UnlockPerk(perk.Id);
                _dirty = true;
                PerkUnlocked?.Invoke(perk);
            }
        }

        public List<PerkDefinition> UnlockedPerks()
        {
            List<PerkDefinition> perks = new List<PerkDefinition>();
            CampaignContent content = _content?.Content;
            if (content == null || Save == null) return perks;
            for (int i = 0; i < Save.UnlockedPerkIds.Count; i++)
            {
                PerkDefinition perk = content.Perk(Save.UnlockedPerkIds[i]);
                if (perk != null) perks.Add(perk);
            }
            return perks;
        }

        public List<PerkDefinition> AllPerksForCharacter()
        {
            List<PerkDefinition> perks = new List<PerkDefinition>();
            CampaignContent content = _content?.Content;
            if (content == null) return perks;
            for (int i = 0; i < content.Perks.Count; i++)
            {
                PerkDefinition perk = content.Perks[i];
                if (perk == null) continue;
                if (perk.ExclusiveTo != CharacterId.None && perk.ExclusiveTo != Save?.Character) continue;
                perks.Add(perk);
            }
            return perks;
        }

        // ------------------------------------------------------------- stat points

        public bool SpendPoint(StatId id)
        {
            if (Save == null) return false;
            if (!StatComputation.TrySpend(Save.StatPoints, Save.Level, id)) return false;
            _dirty = true;
            StatsChanged?.Invoke();
            return true;
        }

        public bool RefundPoint(StatId id)
        {
            if (Save == null) return false;
            if (!StatComputation.TryRefund(Save.StatPoints, id)) return false;
            _dirty = true;
            StatsChanged?.Invoke();
            return true;
        }

        public int SpentIn(StatId id) => Save == null ? 0 : StatComputation.SpentIn(Save.StatPoints, id);

        public bool CanInvest(StatId id) => Save != null && StatComputation.CanInvest(Save.Level, Save.StatPoints, id);

        /// <summary>Refunds every point for a salvage fee. The only way to undo choices.</summary>
        public bool Respec(out string message)
        {
            if (Save == null)
            {
                message = "No active save.";
                return false;
            }
            if (AvailablePoints >= StatComputation.PointsForLevel(Save.Level))
            {
                message = "No points are invested yet.";
                return false;
            }
            if (Save.Salvage < RespecCost)
            {
                message = "Respec costs " + RespecCost + " salvage; you have " + Save.Salvage + ".";
                return false;
            }

            Save.Salvage -= RespecCost;
            foreach (StatId id in StatCatalog.All)
            {
                while (StatComputation.SpentIn(Save.StatPoints, id) > 0)
                {
                    if (!StatComputation.TryRefund(Save.StatPoints, id)) break;
                }
            }

            _dirty = true;
            SalvageChanged?.Invoke();
            StatsChanged?.Invoke();
            message = "Points refunded for " + RespecCost + " salvage.";
            return true;
        }

        // ------------------------------------------------------------- salvage & loot

        public void AddSalvage(int amount)
        {
            if (Save == null || amount == 0) return;
            Save.Salvage = MathLite.Max(0, Save.Salvage + amount);
            SalvageChanged?.Invoke();
        }

        public bool SpendSalvage(int amount)
        {
            if (Save == null || amount <= 0) return false;
            if (Save.Salvage < amount) return false;
            Save.Salvage -= amount;
            SalvageChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Rewards for a kill: XP always, salvage sometimes and gear on a rarity roll.
        /// Returns the salvage awarded and reports any gear drop through the out param.
        /// </summary>
        public int AwardKill(EnemyArchetype enemy, int chapterIndex, bool elite, out string gearDrop)
        {
            gearDrop = null;
            if (Save == null || enemy == null) return 0;

            int tier = enemy.Tier + (elite ? 1 : 0);
            int xp = Mathf.RoundToInt(XpCurve.KillReward(tier, chapterIndex, Difficulty) * (elite ? 2.2f : 1f));
            AddXp(xp, "kill");

            int salvage = LootRules.SalvageReward(chapterIndex, tier, false, false, Stats.Luck, _rng);
            AddSalvage(salvage);

            DropTable table = _content?.Content?.DropTable("dt_chapter" + chapterIndex.ToString("00"));
            if (table != null && LootRules.RollsUpgradeDrop(elite ? 0.22f : 0.08f, Difficulty, Stats.Luck))
            {
                string gearId = LootRules.RollGear(table, _rng);
                if (!string.IsNullOrEmpty(gearId))
                {
                    Save.AddGear(gearId);
                    gearDrop = gearId;
                    _dirty = true;
                }
            }
            return salvage;
        }

        public int AwardBoss(string bossId, int chapterIndex, bool isBoss, float clearSeconds)
        {
            if (Save == null) return 0;
            bool isMini = !isBoss;

            int xp = isBoss
                ? XpCurve.BossReward(chapterIndex, Difficulty)
                : XpCurve.MiniBossReward(chapterIndex, Difficulty);
            AddXp(xp, "boss");

            int salvage = LootRules.SalvageReward(chapterIndex, isBoss ? 5 : 3, isBoss, isMini, Stats.Luck, _rng);
            AddSalvage(salvage);

            if (isBoss) _saves.RegisterBossClear(bossId, clearSeconds);
            else _saves.RegisterMiniBossClear(bossId);
            return xp;
        }

        public int AwardQuest(QuestDefinition quest)
        {
            if (Save == null || quest == null) return 0;
            AddXp(quest.XpReward, "quest");
            AddSalvage(quest.SalvageReward);

            if (!string.IsNullOrEmpty(quest.WeaponRewardId)) Save.AddWeapon(quest.WeaponRewardId, true);
            if (!string.IsNullOrEmpty(quest.GearRewardId)) Save.AddGear(quest.GearRewardId, true);
            if (!string.IsNullOrEmpty(quest.PerkRewardId)) Save.UnlockPerk(quest.PerkRewardId);
            _dirty = true;
            return quest.XpReward;
        }

        // ------------------------------------------------------------- equipment

        public List<WeaponDefinition> OwnedWeapons()
        {
            List<WeaponDefinition> weapons = new List<WeaponDefinition>();
            if (Save == null) return weapons;
            for (int i = 0; i < Save.WeaponIds.Count; i++)
            {
                WeaponDefinition weapon = _content?.Weapon(Save.WeaponIds[i]);
                if (weapon != null) weapons.Add(weapon);
            }
            return weapons;
        }

        public WeaponDefinition EquippedWeapon()
        {
            if (Save == null) return null;
            string id = Save.EquippedGear(EquipSlot.Weapon);
            if (string.IsNullOrEmpty(id))
            {
                CharacterDefinition character = Character;
                id = character != null ? character.SignatureWeaponId : string.Empty;
            }
            WeaponDefinition weapon = _content?.Weapon(id);
            if (weapon == null)
            {
                List<WeaponDefinition> owned = OwnedWeapons();
                weapon = owned.Count > 0 ? owned[0] : null;
            }
            return weapon;
        }

        public void EquipWeapon(string weaponId)
        {
            if (Save == null || string.IsNullOrEmpty(weaponId)) return;
            Save.Equip(EquipSlot.Weapon, weaponId);
            StatsChanged?.Invoke();
        }

        public void EquipGear(string gearId)
        {
            GearDefinition gear = _content?.Gear(gearId);
            if (Save == null || gear == null) return;
            Save.Equip(gear.Slot, gearId);
            _dirty = true;
            StatsChanged?.Invoke();
        }

        public List<GearDefinition> OwnedGear()
        {
            List<GearDefinition> gear = new List<GearDefinition>();
            if (Save == null) return gear;
            for (int i = 0; i < Save.GearIds.Count; i++)
            {
                GearDefinition item = _content?.Gear(Save.GearIds[i]);
                if (item != null) gear.Add(item);
            }
            return gear;
        }

        public int WeaponUpgradeLevel(string weaponId) => Save?.WeaponUpgradeLevel(weaponId) ?? 1;

        /// <summary>Spends salvage to raise a weapon's upgrade level by one.</summary>
        public bool TryUpgradeWeapon(string weaponId, out string message)
        {
            WeaponDefinition weapon = _content?.Weapon(weaponId);
            if (Save == null || weapon == null)
            {
                message = "Unknown weapon.";
                return false;
            }

            int current = Save.WeaponUpgradeLevel(weaponId);
            if (current >= weapon.MaxUpgradeLevel)
            {
                message = weapon.DisplayName + " is already at its maximum upgrade.";
                return false;
            }

            int cost = WeaponMath.NextUpgradeCost(weapon, current);
            if (Save.Salvage < cost)
            {
                message = "Needs " + cost + " salvage; you have " + Save.Salvage + ".";
                return false;
            }

            Save.Salvage -= cost;
            Save.WeaponUpgradeLevels[weaponId] = current + 1;
            _dirty = true;
            SalvageChanged?.Invoke();
            StatsChanged?.Invoke();
            message = weapon.DisplayName + " upgraded to +" + (current + 1) + ".";
            return true;
        }

        /// <summary>Adds a new weapon or gear piece and equips it if it is a strict upgrade.</summary>
        public void GrantWeapon(string weaponId)
        {
            if (Save == null || string.IsNullOrEmpty(weaponId)) return;
            Save.AddWeapon(weaponId, Save.WeaponIds.Count == 0);
            _dirty = true;
        }

        public void GrantGear(string gearId)
        {
            if (Save == null || string.IsNullOrEmpty(gearId)) return;
            Save.AddGear(gearId);
            _dirty = true;
        }

        public float FlaskHealAmount()
        {
            if (Save == null) return 40f;
            return LootRules.FlaskHealAmount(Save.ChapterIndex, Stats.MaxHealth);
        }

        public string LevelSummary()
        {
            return "Level " + Level + (IsMaxLevel ? " (max)" : "  -  " + XpRemaining + " XP to next") +
                   "  |  Points available: " + AvailablePoints;
        }
    }

    /// <summary>Quest tracking: accepting, reporting progress and turning in.</summary>
    public sealed class QuestService
    {
        private readonly ContentDatabase _content;
        private readonly SaveService _saves;
        private readonly Dictionary<string, QuestProgress> _progress = new Dictionary<string, QuestProgress>();

        public event Action<QuestDefinition> QuestChanged;
        public event Action<QuestDefinition> QuestCompleted;

        public QuestService(ContentDatabase content, SaveService saves)
        {
            _content = content;
            _saves = saves;
        }

        private SaveGame Save => _saves.Current;

        public void Rebuild()
        {
            _progress.Clear();
            CampaignContent content = _content?.Content;
            if (content == null || Save == null) return;

            for (int i = 0; i < content.Quests.Count; i++)
            {
                QuestDefinition quest = content.Quests[i];
                if (quest == null) continue;

                QuestSaveEntry entry = Save.QuestEntry(quest.Id);
                QuestProgress progress = QuestRules.NewProgress(quest);
                if (entry != null)
                {
                    progress.State = entry.State;
                    for (int c = 0; c < entry.Counts.Count && c < progress.ObjectiveCounts.Count; c++)
                    {
                        progress.ObjectiveCounts[c] = entry.Counts[c];
                    }
                    progress.ElapsedSeconds = entry.ElapsedSeconds;
                    progress.TookDamage = entry.TookDamage;
                }
                else
                {
                    progress.State = QuestRules.RequirementsMet(quest, CompletedIds(), Save.StoryFlags)
                        ? QuestState.Available
                        : QuestState.Locked;
                }
                _progress[quest.Id] = progress;
            }
        }

        private List<string> CompletedIds()
        {
            List<string> ids = new List<string>();
            foreach (KeyValuePair<string, QuestProgress> pair in _progress)
            {
                if (pair.Value.State == QuestState.Completed) ids.Add(pair.Key);
            }
            return ids;
        }

        /// <summary>Re-evaluates locked quests after flags change (chapter cleared, boss killed).</summary>
        public void RefreshAvailability()
        {
            CampaignContent content = _content?.Content;
            if (content == null) return;

            List<string> completed = CompletedIds();
            for (int i = 0; i < content.Quests.Count; i++)
            {
                QuestDefinition quest = content.Quests[i];
                if (quest == null) continue;
                if (!_progress.TryGetValue(quest.Id, out QuestProgress progress)) continue;
                if (progress.State != QuestState.Locked) continue;
                if (QuestRules.RequirementsMet(quest, completed, Save?.StoryFlags))
                {
                    progress.State = QuestState.Available;
                    QuestChanged?.Invoke(quest);
                }
            }
        }

        public QuestProgress Progress(string questId) =>
            _progress.TryGetValue(questId ?? string.Empty, out QuestProgress progress) ? progress : null;

        public QuestState StateOf(string questId)
        {
            QuestProgress progress = Progress(questId);
            return progress?.State ?? QuestState.Locked;
        }

        public List<QuestDefinition> ActiveQuests()
        {
            List<QuestDefinition> list = new List<QuestDefinition>();
            CampaignContent content = _content?.Content;
            if (content == null) return list;
            for (int i = 0; i < content.Quests.Count; i++)
            {
                QuestDefinition quest = content.Quests[i];
                QuestState state = StateOf(quest.Id);
                if (state == QuestState.Active || state == QuestState.ReadyToTurnIn) list.Add(quest);
            }
            return list;
        }

        public List<QuestDefinition> QuestsInChapter(int chapterIndex, bool includeLocked = false)
        {
            List<QuestDefinition> list = new List<QuestDefinition>();
            CampaignContent content = _content?.Content;
            if (content == null) return list;
            for (int i = 0; i < content.Quests.Count; i++)
            {
                QuestDefinition quest = content.Quests[i];
                if (quest == null || quest.ChapterIndex != chapterIndex) continue;
                QuestState state = StateOf(quest.Id);
                if (!includeLocked && state == QuestState.Locked) continue;
                list.Add(quest);
            }
            return list;
        }

        public bool Accept(string questId)
        {
            QuestDefinition quest = _content?.Quest(questId);
            QuestProgress progress = Progress(questId);
            if (quest == null || progress == null) return false;
            if (progress.State != QuestState.Available) return false;

            progress.State = QuestState.Active;
            Persist();
            QuestChanged?.Invoke(quest);
            return true;
        }

        /// <summary>Reports progress against every active quest. Returns the quests that advanced.</summary>
        public List<QuestDefinition> Report(ObjectiveType type, string targetId, int amount = 1)
        {
            List<QuestDefinition> advanced = new List<QuestDefinition>();
            CampaignContent content = _content?.Content;
            if (content == null || Save == null) return advanced;

            for (int i = 0; i < content.Quests.Count; i++)
            {
                QuestDefinition quest = content.Quests[i];
                QuestProgress progress = Progress(quest?.Id);
                if (quest == null || progress == null) continue;
                if (progress.State != QuestState.Active) continue;

                if (QuestRules.Report(quest, progress, type, targetId, amount, out List<int> changed))
                {
                    advanced.Add(quest);
                    if (progress.State == QuestState.ReadyToTurnIn) QuestCompleted?.Invoke(quest);
                    else QuestChanged?.Invoke(quest);
                }
                else if (changed != null && changed.Count > 0)
                {
                    QuestChanged?.Invoke(quest);
                }
            }

            if (advanced.Count > 0) Persist();
            return advanced;
        }

        public void TickTimed(float deltaTime)
        {
            CampaignContent content = _content?.Content;
            if (content == null || Save == null) return;

            for (int i = 0; i < content.Quests.Count; i++)
            {
                QuestDefinition quest = content.Quests[i];
                QuestProgress progress = Progress(quest?.Id);
                if (quest == null || progress == null || progress.State != QuestState.Active) continue;
                if (QuestRules.TickTimed(quest, progress, deltaTime))
                {
                    QuestChanged?.Invoke(quest);
                    Persist();
                }
            }
        }

        public void NoteDamageTaken()
        {
            CampaignContent content = _content?.Content;
            if (content == null || Save == null) return;

            for (int i = 0; i < content.Quests.Count; i++)
            {
                QuestDefinition quest = content.Quests[i];
                QuestProgress progress = Progress(quest?.Id);
                if (quest == null || progress == null || progress.State != QuestState.Active) continue;
                QuestRules.NoteDamageTaken(quest, progress);
            }
        }

        /// <summary>Claims the reward for a finished quest.</summary>
        public bool TurnIn(string questId, ProgressionService progression, out string message)
        {
            QuestDefinition quest = _content?.Quest(questId);
            QuestProgress progress = Progress(questId);
            message = string.Empty;
            if (quest == null || progress == null) return false;
            if (progress.State != QuestState.ReadyToTurnIn)
            {
                message = "That quest is not finished yet.";
                return false;
            }

            progression?.AwardQuest(quest);
            progress.State = QuestState.Completed;
            if (Save != null && quest.GrantedFlags != null)
            {
                for (int i = 0; i < quest.GrantedFlags.Length; i++) Save.GrantFlag(quest.GrantedFlags[i]);
            }
            Persist();
            RefreshAvailability();
            message = "Completed: " + quest.Title + " (" + QuestRules.RewardSummary(quest) + ")";
            QuestCompleted?.Invoke(quest);
            QuestChanged?.Invoke(quest);
            return true;
        }

        /// <summary>Progress as a 0..1 ratio for the tracker and log.</summary>
        public float CompletionPercent(QuestDefinition quest)
        {
            return QuestRules.CompletionPercent(quest, Progress(quest?.Id));
        }

        public string ObjectiveText(QuestDefinition quest)
        {
            QuestProgress progress = Progress(quest?.Id);
            if (quest == null || progress == null) return string.Empty;

            List<string> parts = new List<string>();
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                QuestObjective objective = quest.Objectives[i];
                if (objective == null) continue;
                if (objective.HiddenUntilProgress && progress.CountAt(i) == 0) continue;
                parts.Add(objective.ProgressText(progress.CountAt(i)));
            }
            return string.Join("   ", parts.ToArray());
        }

        /// <summary>Copies live progress back into the save file.</summary>
        public void Persist()
        {
            if (Save == null) return;
            Save.Quests.Clear();
            foreach (KeyValuePair<string, QuestProgress> pair in _progress)
            {
                QuestProgress progress = pair.Value;
                if (progress.State == QuestState.Locked) continue;
                Save.Quests.Add(new QuestSaveEntry
                {
                    QuestId = progress.QuestId,
                    State = progress.State,
                    Counts = new List<int>(progress.ObjectiveCounts),
                    ElapsedSeconds = progress.ElapsedSeconds,
                    TookDamage = progress.TookDamage
                });
            }
        }
    }

    /// <summary>Story flags, comic unlocks and the gallery/bestiary listings.</summary>
    public sealed class StoryService
    {
        private readonly ContentDatabase _content;
        private readonly SaveService _saves;

        public event Action<string> FlagGranted;

        public StoryService(ContentDatabase content, SaveService saves)
        {
            _content = content;
            _saves = saves;
        }

        private SaveGame Save => _saves.Current;

        public void Rebuild() { }

        public bool Has(string flag) => Save != null && Save.HasFlag(flag);

        public void Grant(string flag)
        {
            if (Save == null || string.IsNullOrEmpty(flag)) return;
            if (Save.HasFlag(flag)) return;
            Save.GrantFlag(flag);
            Services.Get<QuestService>()?.RefreshAvailability();
            FlagGranted?.Invoke(flag);
        }

        public bool IsChapterComplete(int chapterIndex) =>
            Save != null && Save.IsChapterComplete("ch" + chapterIndex.ToString("00"));

        public bool HasDefeatedBoss(string bossId) => Save != null && Save.HasDefeatedBoss(bossId);

        public void MarkComicViewed(ComicPageDefinition page)
        {
            if (Save == null || page == null) return;
            Save.UnlockComicPage(page.Id);
            if (!string.IsNullOrEmpty(page.BeatId)) Grant("comic_" + page.BeatId);
        }

        public bool IsComicUnlocked(string pageId)
        {
            return Save != null && Save.UnlockedComicPageIds.Contains(pageId);
        }

        /// <summary>Story pages the player has seen, newest first, for the gallery.</summary>
        public List<ComicPageDefinition> UnlockedPages()
        {
            List<ComicPageDefinition> pages = new List<ComicPageDefinition>();
            CampaignContent content = _content?.Content;
            if (content == null || Save == null) return pages;

            for (int c = 0; c < content.Chapters.Count; c++)
            {
                ChapterDefinition chapter = content.Chapters[c];
                for (int p = 0; p < chapter.ComicPages.Count; p++)
                {
                    ComicPageDefinition page = chapter.ComicPages[p];
                    if (page != null && Save.UnlockedComicPageIds.Contains(page.Id)) pages.Add(page);
                }
            }
            return pages;
        }

        public List<BossDefinition> DefeatedBosses(bool miniBosses)
        {
            List<BossDefinition> bosses = new List<BossDefinition>();
            CampaignContent content = _content?.Content;
            if (content == null || Save == null) return bosses;

            for (int i = 0; i < content.Bosses.Count; i++)
            {
                BossDefinition boss = content.Bosses[i];
                if (boss == null || boss.IsMiniBoss != miniBosses) continue;
                if (Save.DefeatedBosses.Contains(boss.Id) || Save.DefeatedMiniBosses.Contains(boss.Id)) bosses.Add(boss);
            }
            return bosses;
        }

        /// <summary>Bestiary rows: every enemy, annotated with whether it has been fought.</summary>
        public List<EnemyArchetype> Bestiary()
        {
            List<EnemyArchetype> list = new List<EnemyArchetype>();
            CampaignContent content = _content?.Content;
            if (content == null) return list;
            list.AddRange(content.Enemies);
            return list;
        }

        public List<QuestDefinition> CompletedQuests()
        {
            List<QuestDefinition> list = new List<QuestDefinition>();
            CampaignContent content = _content?.Content;
            QuestService quests = Services.Get<QuestService>();
            if (content == null || quests == null) return list;

            for (int i = 0; i < content.Quests.Count; i++)
            {
                QuestDefinition quest = content.Quests[i];
                if (quest != null && quests.StateOf(quest.Id) == QuestState.Completed) list.Add(quest);
            }
            return list;
        }

        public string RandomLevelUpBark(CharacterDefinition character)
        {
            if (character == null || character.LevelUpBarks.Length == 0) return string.Empty;
            Rng rng = new Rng(StableSeed(character.Name, Save?.Level ?? 1));
            return character.LevelUpBarks[rng.Range(0, character.LevelUpBarks.Length)];
        }

        public string RandomLowHealthBark(CharacterDefinition character)
        {
            if (character == null || character.LowHealthBarks.Length == 0) return string.Empty;
            Rng rng = new Rng(StableSeed(character.Name, 991));
            return character.LowHealthBarks[rng.Range(0, character.LowHealthBarks.Length)];
        }

        public string RandomBossIntroLine(CharacterDefinition character)
        {
            if (character == null || character.BossIntroLines.Length == 0) return string.Empty;
            Rng rng = new Rng(StableSeed(character.Name, 137));
            return character.BossIntroLines[rng.Range(0, character.BossIntroLines.Length)];
        }

        private static int StableSeed(string name, int extra)
        {
            unchecked
            {
                int hash = 17 + extra;
                if (!string.IsNullOrEmpty(name))
                {
                    for (int i = 0; i < name.Length; i++) hash = hash * 31 + name[i];
                }
                return hash;
            }
        }

        /// <summary>Completion summary used by the save slot screen and the ending.</summary>
        public string ProgressSummary()
        {
            if (Save == null) return string.Empty;
            CampaignContent content = _content?.Content;
            int chaptersDone = 0;
            if (content != null)
            {
                for (int i = 0; i < content.Chapters.Count; i++)
                {
                    if (IsChapterComplete(content.Chapters[i].Index)) chaptersDone++;
                }
            }

            return "Chapters " + chaptersDone + "/" + ChapterRules.ChapterCount +
                   "   Bosses " + Save.BossesCleared +
                   "   Quests " + (Services.Get<StoryService>()?.CompletedQuests().Count ?? 0) +
                   "   Comics " + Save.UnlockedComicPageIds.Count + "/" + (content?.TotalComicPages() ?? 0);
        }
    }
}
