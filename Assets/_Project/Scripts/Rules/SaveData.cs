using System;
using System.Collections.Generic;
using System.Globalization;

namespace IronVeil.Rules
{
    public static class SaveFormat
    {
        /// <summary>Bump when the shape changes and add a migration step.</summary>
        public const int CurrentVersion = 3;

        public const int ManualSlotCount = 3;
        public const string AutosaveSlotId = "autosave";
        public const string QuickResumeSlotId = "quickresume";
    }

    [Serializable]
    public sealed class QuestSaveEntry
    {
        public string QuestId = string.Empty;
        public QuestState State = QuestState.Available;
        public List<int> Counts = new List<int>();
        public float ElapsedSeconds;
        public bool TookDamage;
    }

    [Serializable]
    public sealed class BossClearRecord
    {
        public string BossId = string.Empty;
        public float BestSeconds = -1f;
        public int Attempts;
        public int Deaths;
        public bool Defeated;
        public Difficulty HighestDifficulty;
    }

    /// <summary>Everything the save file stores. Settings live in their own file.</summary>
    [Serializable]
    public sealed class SaveGame
    {
        public int Version = SaveFormat.CurrentVersion;
        public string SlotId = "slot1";
        public bool IsAutosave;
        public long SavedAtUnixSeconds;
        public float PlayTimeSeconds;
        public int SaveCounter;

        public CharacterId Character = CharacterId.Kade;
        public Difficulty Difficulty = IronVeil.Rules.Difficulty.Normal;

        /// <summary>Difficulty may be lowered at a savepoint but never raised.</summary>
        public bool DifficultyLocked = true;

        public int Level = 1;
        public int XpIntoLevel;
        public Dictionary<StatId, int> StatPoints = new Dictionary<StatId, int>();
        public List<string> UnlockedPerkIds = new List<string>();

        public int ChapterIndex = 1;
        public string BeatId = string.Empty;
        public string CheckpointId = string.Empty;
        public string SavepointId = string.Empty;

        public List<string> StoryFlags = new List<string>();
        public List<string> CompletedChapters = new List<string>();
        public List<string> DefeatedBosses = new List<string>();
        public List<string> DefeatedMiniBosses = new List<string>();

        public List<QuestSaveEntry> Quests = new List<QuestSaveEntry>();

        public List<string> WeaponIds = new List<string>();
        public List<string> GearIds = new List<string>();
        public Dictionary<string, int> WeaponUpgradeLevels = new Dictionary<string, int>();
        public Dictionary<EquipSlot, string> Equipped = new Dictionary<EquipSlot, string>();
        public int Salvage;

        public List<string> UnlockedComicPageIds = new List<string>();
        public List<string> UnlockedGalleryIds = new List<string>();
        public List<BossClearRecord> BossRecords = new List<BossClearRecord>();

        public int TotalKills;
        public int EliteKills;
        public int Deaths;
        public int BossesCleared;
        public int MiniBossesCleared;
        public int ShotsFired;

        // ---------------- convenience ----------------

        public bool HasFlag(string flag) => !string.IsNullOrEmpty(flag) && StoryFlags.Contains(flag);

        public void GrantFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return;
            if (!StoryFlags.Contains(flag)) StoryFlags.Add(flag);
        }

        public bool IsChapterComplete(string chapterId) => !string.IsNullOrEmpty(chapterId) && CompletedChapters.Contains(chapterId);

        public void CompleteChapter(string chapterId)
        {
            if (string.IsNullOrEmpty(chapterId)) return;
            if (!CompletedChapters.Contains(chapterId)) CompletedChapters.Add(chapterId);
        }

        public void DefeatBoss(string bossId)
        {
            if (string.IsNullOrEmpty(bossId)) return;
            if (!DefeatedBosses.Contains(bossId)) DefeatedBosses.Add(bossId);
            BossesCleared = DefeatedBosses.Count;
        }

        public void DefeatMiniBoss(string bossId)
        {
            if (string.IsNullOrEmpty(bossId)) return;
            if (!DefeatedMiniBosses.Contains(bossId)) DefeatedMiniBosses.Add(bossId);
            MiniBossesCleared = DefeatedMiniBosses.Count;
        }

        public bool HasDefeatedBoss(string bossId) => !string.IsNullOrEmpty(bossId) && DefeatedBosses.Contains(bossId);

        public BossClearRecord RecordFor(string bossId)
        {
            for (int i = 0; i < BossRecords.Count; i++)
            {
                if (BossRecords[i] != null && BossRecords[i].BossId == bossId) return BossRecords[i];
            }
            BossClearRecord record = new BossClearRecord { BossId = bossId };
            BossRecords.Add(record);
            return record;
        }

        public void RegisterBossAttempt(string bossId)
        {
            RecordFor(bossId).Attempts++;
        }

        public void RegisterBossClear(string bossId, float seconds)
        {
            BossClearRecord record = RecordFor(bossId);
            record.Defeated = true;
            record.Attempts = MathLite.Max(record.Attempts, 1);
            if (record.BestSeconds < 0f || (seconds > 0f && seconds < record.BestSeconds)) record.BestSeconds = seconds;
            if ((int)Difficulty > (int)record.HighestDifficulty) record.HighestDifficulty = Difficulty;
        }

        public QuestSaveEntry QuestEntry(string questId)
        {
            for (int i = 0; i < Quests.Count; i++)
            {
                if (Quests[i] != null && Quests[i].QuestId == questId) return Quests[i];
            }
            return null;
        }

        public string EquippedGear(EquipSlot slot) => Equipped.TryGetValue(slot, out string id) ? id : string.Empty;

        public void Equip(EquipSlot slot, string gearId)
        {
            if (string.IsNullOrEmpty(gearId)) Equipped.Remove(slot);
            else Equipped[slot] = gearId;
        }

        public int WeaponUpgradeLevel(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return 1;
            return WeaponUpgradeLevels.TryGetValue(weaponId, out int level) ? MathLite.Max(1, level) : 1;
        }

        public void AddWeapon(string weaponId, bool equip = false)
        {
            if (string.IsNullOrEmpty(weaponId)) return;
            if (!WeaponIds.Contains(weaponId)) WeaponIds.Add(weaponId);
            if (!WeaponUpgradeLevels.ContainsKey(weaponId)) WeaponUpgradeLevels[weaponId] = 1;
            if (equip) Equip(EquipSlot.Weapon, weaponId);
        }

        public void AddGear(string gearId, bool equip = false)
        {
            if (string.IsNullOrEmpty(gearId)) return;
            if (!GearIds.Contains(gearId)) GearIds.Add(gearId);
            if (equip) Equipped[EquipSlot.Armor] = gearId;
        }

        public void UnlockComicPage(string pageId)
        {
            if (string.IsNullOrEmpty(pageId)) return;
            if (!UnlockedComicPageIds.Contains(pageId)) UnlockedComicPageIds.Add(pageId);
            if (!UnlockedGalleryIds.Contains(pageId)) UnlockedGalleryIds.Add(pageId);
        }

        public void UnlockGallery(string entryId)
        {
            if (string.IsNullOrEmpty(entryId)) return;
            if (!UnlockedGalleryIds.Contains(entryId)) UnlockedGalleryIds.Add(entryId);
        }

        public void UnlockPerk(string perkId)
        {
            if (string.IsNullOrEmpty(perkId)) return;
            if (!UnlockedPerkIds.Contains(perkId)) UnlockedPerkIds.Add(perkId);
        }

        /// <summary>Final stats for this save: base + growth + points + equipped gear + perks.</summary>
        public StatBlock ComputeStats(CampaignContent content)
        {
            CharacterDefinition character = content == null ? null : content.Character(Character);
            if (character == null) return new StatBlock();

            List<StatBlock> bonuses = new List<StatBlock>();
            for (int slot = 0; slot <= (int)EquipSlot.Trinket; slot++)
            {
                string gearId = EquippedGear((EquipSlot)slot);
                if (string.IsNullOrEmpty(gearId) || content == null) continue;
                GearDefinition gear = content.Gear(gearId);
                if (gear?.Bonus != null) bonuses.Add(gear.Bonus);
            }

            if (content != null)
            {
                for (int i = 0; i < UnlockedPerkIds.Count; i++)
                {
                    PerkDefinition perk = content.Perk(UnlockedPerkIds[i]);
                    if (perk?.Bonus != null) bonuses.Add(perk.Bonus);
                }
            }

            return StatComputation.Compute(character.BaseStats, character.PerLevelGrowth, Level, StatPoints, bonuses);
        }

        /// <summary>Clamps fields into legal ranges. Used after loading untrusted data.</summary>
        public void Sanitize()
        {
            Version = SaveFormat.CurrentVersion;
            Level = MathLite.Clamp(Level, 1, StatComputation.MaxLevel);
            XpIntoLevel = MathLite.Max(0, XpIntoLevel);
            if (Level >= StatComputation.MaxLevel) XpIntoLevel = 0;
            ChapterIndex = MathLite.Clamp(ChapterIndex, 1, ChapterRules.ChapterCount);
            Salvage = MathLite.Max(0, Salvage);
            PlayTimeSeconds = MathLite.Max(0f, PlayTimeSeconds);
            SaveCounter = MathLite.Max(0, SaveCounter);
            TotalKills = MathLite.Max(0, TotalKills);
            Deaths = MathLite.Max(0, Deaths);
            ShotsFired = MathLite.Max(0, ShotsFired);
            EliteKills = MathLite.Max(0, EliteKills);

            if (StatPoints == null) StatPoints = new Dictionary<StatId, int>();
            StoryFlags ??= new List<string>();
            CompletedChapters ??= new List<string>();
            DefeatedBosses ??= new List<string>();
            DefeatedMiniBosses ??= new List<string>();
            Quests ??= new List<QuestSaveEntry>();
            WeaponIds ??= new List<string>();
            GearIds ??= new List<string>();
            WeaponUpgradeLevels ??= new Dictionary<string, int>();
            Equipped ??= new Dictionary<EquipSlot, string>();
            UnlockedComicPageIds ??= new List<string>();
            UnlockedGalleryIds ??= new List<string>();
            BossRecords ??= new List<BossClearRecord>();
            UnlockedPerkIds ??= new List<string>();

            // Never leave the player with more points invested than they have earned.
            int budget = StatComputation.PointsForLevel(Level);
            int spent = StatComputation.PointsAlreadySpent(StatPoints);
            if (spent > budget)
            {
                foreach (StatId id in StatCatalog.All)
                {
                    if (spent <= budget) break;
                    while (spent > budget && StatComputation.SpentIn(StatPoints, id) > 0)
                    {
                        StatComputation.TryRefund(StatPoints, id);
                        spent--;
                    }
                }
            }

            BossesCleared = DefeatedBosses.Count;
            MiniBossesCleared = DefeatedMiniBosses.Count;
        }

        /// <summary>
        /// Drops ids that no longer exist in the content catalog (after a balance
        /// patch, for example) so a stale save cannot crash the game.
        /// </summary>
        public void PruneUnknownContent(CampaignContent content)
        {
            if (content == null) return;

            WeaponIds.RemoveAll(id => content.Weapon(id) == null);
            GearIds.RemoveAll(id => content.Gear(id) == null);
            UnlockedPerkIds.RemoveAll(id => content.Perk(id) == null);
            Quests.RemoveAll(entry => entry == null || content.Quest(entry.QuestId) == null);

            List<EquipSlot> stale = new List<EquipSlot>();
            foreach (KeyValuePair<EquipSlot, string> pair in Equipped)
            {
                bool valid = pair.Key == EquipSlot.Weapon
                    ? content.Weapon(pair.Value) != null
                    : content.Gear(pair.Value) != null;
                if (!valid) stale.Add(pair.Key);
            }
            for (int i = 0; i < stale.Count; i++) Equipped.Remove(stale[i]);

            UnlockedComicPageIds.RemoveAll(id => content.FindComicPage(id) == null);

            foreach (KeyValuePair<string, int> pair in new Dictionary<string, int>(WeaponUpgradeLevels))
            {
                if (content.Weapon(pair.Key) == null) WeaponUpgradeLevels.Remove(pair.Key);
            }
        }

        public SaveGame Clone()
        {
            SaveGame copy = (SaveGame)MemberwiseClone();
            copy.StatPoints = new Dictionary<StatId, int>(StatPoints);
            copy.UnlockedPerkIds = new List<string>(UnlockedPerkIds);
            copy.StoryFlags = new List<string>(StoryFlags);
            copy.CompletedChapters = new List<string>(CompletedChapters);
            copy.DefeatedBosses = new List<string>(DefeatedBosses);
            copy.DefeatedMiniBosses = new List<string>(DefeatedMiniBosses);
            copy.WeaponIds = new List<string>(WeaponIds);
            copy.GearIds = new List<string>(GearIds);
            copy.WeaponUpgradeLevels = new Dictionary<string, int>(WeaponUpgradeLevels);
            copy.Equipped = new Dictionary<EquipSlot, string>(Equipped);
            copy.UnlockedComicPageIds = new List<string>(UnlockedComicPageIds);
            copy.UnlockedGalleryIds = new List<string>(UnlockedGalleryIds);
            copy.BossRecords = new List<BossClearRecord>(BossRecords);
            copy.Quests = new List<QuestSaveEntry>();
            for (int i = 0; i < Quests.Count; i++)
            {
                QuestSaveEntry entry = Quests[i];
                if (entry == null) continue;
                copy.Quests.Add(new QuestSaveEntry
                {
                    QuestId = entry.QuestId,
                    State = entry.State,
                    Counts = new List<int>(entry.Counts),
                    ElapsedSeconds = entry.ElapsedSeconds,
                    TookDamage = entry.TookDamage
                });
            }
            return copy;
        }

        /// <summary>Short description used by the continue screen.</summary>
        public string Summary(CampaignContent content)
        {
            CharacterDefinition character = content == null ? null : content.Character(Character);
            string name = character == null ? Character.ToString() : character.Name;
            ChapterDefinition chapter = content == null ? null : content.Chapter(ChapterIndex);
            string chapterLabel = chapter == null ? "Chapter " + ChapterIndex : "Ch " + ChapterIndex + " - " + chapter.Title;
            string difficulty = DifficultyProfile.For(Difficulty).DisplayName;
            return chapterLabel + "  |  " + name + "  |  Lv " + Level + "  |  " + difficulty + "  |  " + FormatPlayTime();
        }

        public string FormatPlayTime()
        {
            int totalSeconds = (int)MathLite.Max(0f, PlayTimeSeconds);
            int hours = totalSeconds / 3600;
            int minutes = totalSeconds % 3600 / 60;
            return hours + "h " + minutes.ToString("00", CultureInfo.InvariantCulture) + "m";
        }
    }

    /// <summary>Serialises saves with an integrity checksum and version migration.</summary>
    public static class SaveCodec
    {
        public const string PayloadKey = "payload";
        public const string VersionKey = "version";
        public const string ChecksumKey = "checksum";

        public static SaveGame NewGame(CharacterId character, Difficulty difficulty, CampaignContent content)
        {
            SaveGame save = new SaveGame
            {
                Character = character,
                Difficulty = difficulty,
                DifficultyLocked = true,
                Level = 1,
                ChapterIndex = 1,
                SavedAtUnixSeconds = 0
            };

            ChapterDefinition firstChapter = content == null ? null : content.Chapter(1);
            save.BeatId = firstChapter != null && firstChapter.Beats.Count > 0
                ? firstChapter.Beats[0].Id
                : "ch01_beat_01";

            CharacterDefinition definition = content == null ? null : content.Character(character);
            if (definition != null)
            {
                for (int i = 0; i < definition.StartingWeaponIds.Length; i++)
                {
                    save.AddWeapon(definition.StartingWeaponIds[i], i == 0);
                }
                for (int i = 0; i < definition.UnlockableGearIds.Length; i++)
                {
                    // Starting gear is granted, upgrades are found in the world.
                    if (i == 0) save.AddGear(definition.UnlockableGearIds[i], true);
                }
            }

            save.Sanitize();
            return save;
        }

        public static string Encode(SaveGame save)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            save.Version = SaveFormat.CurrentVersion;

            JsonValue payload = ToJson(save);
            string payloadText = payload.ToJson();
            string checksum = Checksum.Fnv1a32Hex(payloadText);

            JsonValue root = JsonValue.NewObject();
            root.Add(VersionKey, SaveFormat.CurrentVersion);
            root.Add(ChecksumKey, checksum);
            root.Add(PayloadKey, payload);
            return root.ToJson(true);
        }

        public static bool TryDecode(string text, out SaveGame save, out string error)
        {
            save = null;
            error = null;

            if (!JsonValue.TryParse(text, out JsonValue root, out string parseError))
            {
                error = "save file is not valid JSON: " + parseError;
                return false;
            }

            if (root.Kind != JsonKind.Object || !root.HasKey(PayloadKey))
            {
                error = "save file is missing its payload";
                return false;
            }

            int version = root[VersionKey].AsInt(1);
            if (version > SaveFormat.CurrentVersion)
            {
                error = "save file version " + version + " is newer than this build supports (" +
                        SaveFormat.CurrentVersion + ")";
                return false;
            }

            JsonValue payload = root[PayloadKey];
            string expected = root[ChecksumKey].AsString();
            if (!string.IsNullOrEmpty(expected))
            {
                string actual = Checksum.Fnv1a32Hex(payload.ToJson());
                if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                {
                    error = "save file failed its integrity check (corrupted or edited)";
                    return false;
                }
            }

            SaveGame loaded = FromJson(payload);
            loaded.Version = version;
            Migrate(loaded, version);
            loaded.Sanitize();
            save = loaded;
            return true;
        }

        /// <summary>Applies every migration step needed to reach the current version.</summary>
        public static void Migrate(SaveGame save, int fromVersion)
        {
            if (save == null) return;

            if (fromVersion < 2)
            {
                // v2 introduced stat points as a sparse dictionary and perks.
                if (save.StatPoints == null) save.StatPoints = new Dictionary<StatId, int>();
                if (save.UnlockedPerkIds == null) save.UnlockedPerkIds = new List<string>();
            }

            if (fromVersion < 3)
            {
                // v3 added boss records, save counters and gallery unlocks.
                if (save.BossRecords == null) save.BossRecords = new List<BossClearRecord>();
                if (save.UnlockedGalleryIds == null) save.UnlockedGalleryIds = new List<string>();
                if (save.SaveCounter <= 0) save.SaveCounter = 1;
                save.IsAutosave = false;

                // Rebuild clear records from the flat lists so old saves keep their stats.
                if (save.DefeatedBosses != null)
                {
                    for (int i = 0; i < save.DefeatedBosses.Count; i++)
                    {
                        BossClearRecord record = save.RecordFor(save.DefeatedBosses[i]);
                        record.Defeated = true;
                        record.Attempts = MathLite.Max(record.Attempts, 1);
                    }
                }
            }

            save.Version = SaveFormat.CurrentVersion;
        }

        public static JsonValue ToJson(SaveGame save)
        {
            JsonValue root = JsonValue.NewObject();
            root.Add("slotId", save.SlotId);
            root.Add("autosave", save.IsAutosave);
            root.Add("savedAt", save.SavedAtUnixSeconds);
            root.Add("playTime", save.PlayTimeSeconds);
            root.Add("saveCounter", save.SaveCounter);
            root.Add("character", (int)save.Character);
            root.Add("difficulty", (int)save.Difficulty);
            root.Add("difficultyLocked", save.DifficultyLocked);
            root.Add("level", save.Level);
            root.Add("xpIntoLevel", save.XpIntoLevel);
            root.Add("chapterIndex", save.ChapterIndex);
            root.Add("beatId", save.BeatId);
            root.Add("checkpointId", save.CheckpointId);
            root.Add("savepointId", save.SavepointId);
            root.Add("salvage", save.Salvage);

            JsonValue points = JsonValue.NewObject();
            foreach (KeyValuePair<StatId, int> pair in save.StatPoints)
            {
                if (pair.Value > 0) points.Add(pair.Key.ToString(), pair.Value);
            }
            root.Add("statPoints", points);

            JsonValue equipped = JsonValue.NewObject();
            foreach (KeyValuePair<EquipSlot, string> pair in save.Equipped)
            {
                equipped.Add(pair.Key.ToString(), pair.Value);
            }
            root.Add("equipped", equipped);

            JsonValue weaponUpgrades = JsonValue.NewObject();
            foreach (KeyValuePair<string, int> pair in save.WeaponUpgradeLevels)
            {
                weaponUpgrades.Add(pair.Key, pair.Value);
            }
            root.Add("weaponUpgrades", weaponUpgrades);

            root.Add("perks", StringArray(save.UnlockedPerkIds));
            root.Add("flags", StringArray(save.StoryFlags));
            root.Add("chapters", StringArray(save.CompletedChapters));
            root.Add("bosses", StringArray(save.DefeatedBosses));
            root.Add("miniBosses", StringArray(save.DefeatedMiniBosses));
            root.Add("weapons", StringArray(save.WeaponIds));
            root.Add("gear", StringArray(save.GearIds));
            root.Add("comicPages", StringArray(save.UnlockedComicPageIds));
            root.Add("gallery", StringArray(save.UnlockedGalleryIds));

            JsonValue quests = JsonValue.NewArray();
            for (int i = 0; i < save.Quests.Count; i++)
            {
                QuestSaveEntry entry = save.Quests[i];
                if (entry == null) continue;
                JsonValue quest = JsonValue.NewObject();
                quest.Add("id", entry.QuestId);
                quest.Add("state", (int)entry.State);
                quest.Add("elapsed", entry.ElapsedSeconds);
                quest.Add("damaged", entry.TookDamage);
                JsonValue counts = JsonValue.NewArray();
                for (int c = 0; c < entry.Counts.Count; c++) counts.Add(JsonValue.From(entry.Counts[c]));
                quest.Add("counts", counts);
                quests.Add(quest);
            }
            root.Add("quests", quests);

            JsonValue records = JsonValue.NewArray();
            for (int i = 0; i < save.BossRecords.Count; i++)
            {
                BossClearRecord record = save.BossRecords[i];
                if (record == null) continue;
                JsonValue entry = JsonValue.NewObject();
                entry.Add("id", record.BossId);
                entry.Add("best", record.BestSeconds);
                entry.Add("attempts", record.Attempts);
                entry.Add("deaths", record.Deaths);
                entry.Add("defeated", record.Defeated);
                entry.Add("difficulty", (int)record.HighestDifficulty);
                records.Add(entry);
            }
            root.Add("bossRecords", records);

            JsonValue stats = JsonValue.NewObject();
            stats.Add("kills", save.TotalKills);
            stats.Add("eliteKills", save.EliteKills);
            stats.Add("deaths", save.Deaths);
            stats.Add("bossesCleared", save.BossesCleared);
            stats.Add("miniBossesCleared", save.MiniBossesCleared);
            stats.Add("shotsFired", save.ShotsFired);
            root.Add("stats", stats);

            return root;
        }

        public static SaveGame FromJson(JsonValue root)
        {
            SaveGame save = new SaveGame();
            if (root == null || root.Kind != JsonKind.Object) return save;

            save.SlotId = root["slotId"].AsString(save.SlotId);
            save.IsAutosave = root["autosave"].AsBool();
            save.SavedAtUnixSeconds = (long)root["savedAt"].AsDouble();
            save.PlayTimeSeconds = root["playTime"].AsFloat();
            save.SaveCounter = root["saveCounter"].AsInt();
            save.Character = (CharacterId)root["character"].AsInt((int)CharacterId.Kade);
            save.Difficulty = (Difficulty)MathLite.Clamp(root["difficulty"].AsInt((int)IronVeil.Rules.Difficulty.Normal), 0, 2);
            save.DifficultyLocked = root["difficultyLocked"].AsBool(true);
            save.Level = root["level"].AsInt(1);
            save.XpIntoLevel = root["xpIntoLevel"].AsInt();
            save.ChapterIndex = root["chapterIndex"].AsInt(1);
            save.BeatId = root["beatId"].AsString();
            save.CheckpointId = root["checkpointId"].AsString();
            save.SavepointId = root["savepointId"].AsString();
            save.Salvage = root["salvage"].AsInt();

            JsonValue points = root["statPoints"];
            if (points.Kind == JsonKind.Object)
            {
                foreach (string key in points.Keys)
                {
                    if (Enum.TryParse(key, out StatId id))
                    {
                        save.StatPoints[id] = MathLite.Max(0, points[key].AsInt());
                    }
                }
            }

            JsonValue equipped = root["equipped"];
            if (equipped.Kind == JsonKind.Object)
            {
                foreach (string key in equipped.Keys)
                {
                    if (Enum.TryParse(key, out EquipSlot slot))
                    {
                        string value = equipped[key].AsString();
                        if (!string.IsNullOrEmpty(value)) save.Equipped[slot] = value;
                    }
                }
            }

            JsonValue upgrades = root["weaponUpgrades"];
            if (upgrades.Kind == JsonKind.Object)
            {
                foreach (string key in upgrades.Keys)
                {
                    save.WeaponUpgradeLevels[key] = MathLite.Max(1, upgrades[key].AsInt(1));
                }
            }

            ReadStrings(root["perks"], save.UnlockedPerkIds);
            ReadStrings(root["flags"], save.StoryFlags);
            ReadStrings(root["chapters"], save.CompletedChapters);
            ReadStrings(root["bosses"], save.DefeatedBosses);
            ReadStrings(root["miniBosses"], save.DefeatedMiniBosses);
            ReadStrings(root["weapons"], save.WeaponIds);
            ReadStrings(root["gear"], save.GearIds);
            ReadStrings(root["comicPages"], save.UnlockedComicPageIds);
            ReadStrings(root["gallery"], save.UnlockedGalleryIds);

            JsonValue quests = root["quests"];
            if (quests.Kind == JsonKind.Array)
            {
                for (int i = 0; i < quests.Count; i++)
                {
                    JsonValue quest = quests[i];
                    if (quest.Kind != JsonKind.Object) continue;
                    QuestSaveEntry entry = new QuestSaveEntry
                    {
                        QuestId = quest["id"].AsString(),
                        State = (QuestState)quest["state"].AsInt(),
                        ElapsedSeconds = quest["elapsed"].AsFloat(),
                        TookDamage = quest["damaged"].AsBool()
                    };
                    JsonValue counts = quest["counts"];
                    if (counts.Kind == JsonKind.Array)
                    {
                        for (int c = 0; c < counts.Count; c++) entry.Counts.Add(MathLite.Max(0, counts[c].AsInt()));
                    }
                    if (!string.IsNullOrEmpty(entry.QuestId)) save.Quests.Add(entry);
                }
            }

            JsonValue records = root["bossRecords"];
            if (records.Kind == JsonKind.Array)
            {
                for (int i = 0; i < records.Count; i++)
                {
                    JsonValue record = records[i];
                    if (record.Kind != JsonKind.Object) continue;
                    BossClearRecord clear = new BossClearRecord
                    {
                        BossId = record["id"].AsString(),
                        BestSeconds = record["best"].AsFloat(-1f),
                        Attempts = record["attempts"].AsInt(),
                        Deaths = record["deaths"].AsInt(),
                        Defeated = record["defeated"].AsBool(),
                        HighestDifficulty = (Difficulty)MathLite.Clamp(record["difficulty"].AsInt(), 0, 2)
                    };
                    if (!string.IsNullOrEmpty(clear.BossId)) save.BossRecords.Add(clear);
                }
            }

            JsonValue stats = root["stats"];
            save.TotalKills = stats["kills"].AsInt();
            save.EliteKills = stats["eliteKills"].AsInt();
            save.Deaths = stats["deaths"].AsInt();
            save.BossesCleared = stats["bossesCleared"].AsInt();
            save.MiniBossesCleared = stats["miniBossesCleared"].AsInt();
            save.ShotsFired = stats["shotsFired"].AsInt();

            return save;
        }

        private static JsonValue StringArray(List<string> values)
        {
            JsonValue array = JsonValue.NewArray();
            if (values == null) return array;
            for (int i = 0; i < values.Count; i++)
            {
                if (!string.IsNullOrEmpty(values[i])) array.Add(JsonValue.From(values[i]));
            }
            return array;
        }

        private static void ReadStrings(JsonValue array, List<string> target)
        {
            if (array.Kind != JsonKind.Array) return;
            for (int i = 0; i < array.Count; i++)
            {
                string value = array[i].AsString();
                if (!string.IsNullOrEmpty(value) && !target.Contains(value)) target.Add(value);
            }
        }
    }
}
