using System.Collections.Generic;
using IronVeil.Rules;
using NUnit.Framework;

namespace IronVeil.Tests
{
    /// <summary>
    /// Builds the real campaign once and asserts the whole design contract.
    /// These are the tests that catch a chapter shipping without a boss,
    /// a boss that Easy difficulty cannot finish, or a save file that loses data.
    /// </summary>
    [TestFixture]
    public class CampaignContentTests
    {
        private CampaignContent _content;

        [OneTimeSetUp]
        public void BuildOnce()
        {
            _content = Campaign.Build();
        }

        [Test]
        public void ContentPassesValidation()
        {
            ValidationReport report = ContentValidator.Validate(_content);
            Assert.IsTrue(report.IsValid, report.Summary(80));
        }

        [Test]
        public void TwelveChaptersWithTwentyFiveHoursOfMainPath()
        {
            Assert.AreEqual(12, _content.Chapters.Count);
            int minutes = _content.TotalMinutes();
            Assert.AreEqual(25 * 60, minutes, "chapter lengths should sum to 25 hours");
            for (int i = 1; i <= 12; i++)
            {
                ChapterDefinition chapter = _content.Chapter(i);
                Assert.IsNotNull(chapter, "chapter " + i + " is missing");
                Assert.GreaterOrEqual(chapter.EstimatedMinutes, 100, "chapter " + i + " is too short");
            }
        }

        [Test]
        public void EveryChapterHasABossMiniBossesAndSideQuests()
        {
            for (int i = 1; i <= 12; i++)
            {
                ChapterDefinition chapter = _content.Chapter(i);
                Assert.IsNotNull(_content.Boss(chapter.BossId), "chapter " + i + " boss missing");
                Assert.GreaterOrEqual(chapter.MiniBossIds.Count, 2, "chapter " + i + " needs 2+ mini-bosses");
                Assert.GreaterOrEqual(chapter.SideQuestIds.Count, 2, "chapter " + i + " needs 2+ side quests");
                Assert.IsTrue(chapter.HasPreBossSavepoint(), "chapter " + i + " needs a pre-boss savepoint");
                Assert.GreaterOrEqual(chapter.TotalComicPages, 2, "chapter " + i + " needs comic pages");
            }
        }

        [Test]
        public void BossesGainPatternsWithDifficulty()
        {
            int totalEasy = 0, totalNormal = 0, totalHard = 0;
            for (int i = 0; i < _content.Bosses.Count; i++)
            {
                BossDefinition boss = _content.Bosses[i];
                int easy = boss.TotalPatterns(DifficultyProfile.For(Difficulty.Easy));
                int normal = boss.TotalPatterns(DifficultyProfile.For(Difficulty.Normal));
                int hard = boss.TotalPatterns(DifficultyProfile.For(Difficulty.Hard));

                Assert.Greater(easy, 0, boss.Id + " is unbeatable-empty on Easy");
                Assert.Greater(normal, easy, boss.Id + " does not add patterns on Normal");
                Assert.Greater(hard, normal, boss.Id + " does not add patterns on Hard");

                totalEasy += easy;
                totalNormal += normal;
                totalHard += hard;
            }

            Assert.Greater(totalHard, totalNormal);
            Assert.Greater(totalNormal, totalEasy);
        }

        [Test]
        public void FinalBossHasFourPhases()
        {
            BossDefinition finale = null;
            for (int i = 0; i < _content.Bosses.Count; i++)
            {
                if (_content.Bosses[i].IsFinalBoss) finale = _content.Bosses[i];
            }
            Assert.IsNotNull(finale, "the campaign has no final boss");
            Assert.GreaterOrEqual(finale.PhaseCount, 4, "the finale should escalate through four phases");
        }

        [Test]
        public void BothCharactersHaveFullKitsAndArsenals()
        {
            foreach (CharacterId id in new[] { CharacterId.Kade, CharacterId.Vera })
            {
                CharacterDefinition character = _content.Character(id);
                Assert.IsNotNull(character);
                Assert.AreNotEqual(AbilityKind.None, character.PrimaryAbility.Kind);
                Assert.AreNotEqual(AbilityKind.None, character.SecondaryAbility.Kind);
                Assert.AreNotEqual(PassiveKind.None, character.Passive.Kind);
                Assert.GreaterOrEqual(_content.WeaponsFor(id).Count, 6, character.Id + " needs a full arsenal");
            }
        }

        [Test]
        public void ComicCutsceneVolumeIsSubstantial()
        {
            Assert.GreaterOrEqual(_content.TotalComicPages(), 40, "a 25 hour campaign needs comic pages throughout");
            Assert.GreaterOrEqual(_content.TotalComicPanels(), 200, "each comic page needs real panels");
        }

        [Test]
        public void NewGameSaveIsPlayableForBothCharacters()
        {
            foreach (CharacterId id in new[] { CharacterId.Kade, CharacterId.Vera })
            {
                SaveGame save = SaveCodec.NewGame(id, Difficulty.Normal, _content);
                Assert.AreEqual(id, save.Character);
                Assert.AreEqual(1, save.Level);
                Assert.IsNotEmpty(save.WeaponIds, "a new game should start with a weapon");
                Assert.IsNotNull(_content.Weapon(save.EquippedGear(EquipSlot.Weapon)), "the starting weapon must be equipped and valid");

                StatBlock stats = save.ComputeStats(_content);
                Assert.Greater(stats.MaxHealth, 100f);
                Assert.Greater(stats.Power, 0f);
            }
        }
    }

    [TestFixture]
    public class SaveCodecTests
    {
        private CampaignContent _content;

        [OneTimeSetUp]
        public void BuildOnce()
        {
            _content = Campaign.Build();
        }

        private SaveGame SampleSave()
        {
            SaveGame save = SaveCodec.NewGame(CharacterId.Vera, Difficulty.Hard, _content);
            save.Level = 34;
            save.XpIntoLevel = 1200;
            save.ChapterIndex = 7;
            save.BeatId = "ch07_beat_hall";
            save.SavepointId = "sp07_preboss";
            save.Salvage = 2450;
            save.PlayTimeSeconds = 8123.5f;
            save.SaveCounter = 42;
            save.StatPoints[StatId.Power] = 12;
            save.StatPoints[StatId.Shield] = 9;
            save.StatPoints[StatId.MaxHealth] = 8;
            save.AddWeapon("w_stiletto", true);
            save.AddWeapon("w_arc_caster");
            save.WeaponUpgradeLevels["w_stiletto"] = 3;
            save.AddGear("g_weave_mk2", true);
            save.Equip(EquipSlot.Trinket, "g_trinket_marksman");
            save.GrantFlag("flag_ch06_complete");
            save.CompleteChapter("ch06");
            save.DefeatBoss("boss_choir_engine");
            save.DefeatMiniBoss("mb_hook");
            save.UnlockPerk("perk_scavenger");
            save.UnlockComicPage("cp06_outro");
            save.TotalKills = 1450;
            save.Deaths = 6;
            save.RegisterBossClear("boss_choir_engine", 96.5f);
            save.Quests.Add(new QuestSaveEntry
            {
                QuestId = "q06_catalogue",
                State = QuestState.ReadyToTurnIn,
                Counts = new List<int> { 5, 20 },
                ElapsedSeconds = 240f
            });
            save.Sanitize();
            return save;
        }

        [Test]
        public void RoundTripsEveryField()
        {
            SaveGame original = SampleSave();
            string json = SaveCodec.Encode(original);

            Assert.IsTrue(SaveCodec.TryDecode(json, out SaveGame loaded, out string error), error);

            Assert.AreEqual(original.Character, loaded.Character);
            Assert.AreEqual(original.Difficulty, loaded.Difficulty);
            Assert.AreEqual(original.Level, loaded.Level);
            Assert.AreEqual(original.XpIntoLevel, loaded.XpIntoLevel);
            Assert.AreEqual(original.ChapterIndex, loaded.ChapterIndex);
            Assert.AreEqual(original.BeatId, loaded.BeatId);
            Assert.AreEqual(original.SavepointId, loaded.SavepointId);
            Assert.AreEqual(original.Salvage, loaded.Salvage);
            Assert.AreEqual(original.PlayTimeSeconds, loaded.PlayTimeSeconds, 0.01f);
            Assert.AreEqual(original.SaveCounter, loaded.SaveCounter);
            Assert.AreEqual(StatComputation.SpentIn(original.StatPoints, StatId.Power),
                StatComputation.SpentIn(loaded.StatPoints, StatId.Power));
            Assert.AreEqual(3, loaded.WeaponUpgradeLevel(original.EquippedGear(EquipSlot.Weapon)) == 3 ? 3 : loaded.WeaponUpgradeLevel("w_stiletto"));
            Assert.AreEqual("g_weave_mk2", loaded.EquippedGear(EquipSlot.Armor));
            Assert.AreEqual("g_trinket_marksman", loaded.EquippedGear(EquipSlot.Trinket));
            Assert.IsTrue(loaded.HasFlag("flag_ch06_complete"));
            Assert.IsTrue(loaded.IsChapterComplete("ch06"));
            Assert.IsTrue(loaded.HasDefeatedBoss("boss_choir_engine"));
            Assert.Contains("mb_hook", loaded.DefeatedMiniBosses);
            Assert.Contains("perk_scavenger", loaded.UnlockedPerkIds);
            Assert.Contains("cp06_outro", loaded.UnlockedComicPageIds);
            Assert.AreEqual(1450, loaded.TotalKills);
            Assert.AreEqual(6, loaded.Deaths);

            BossClearRecord record = loaded.RecordFor("boss_choir_engine");
            Assert.IsTrue(record.Defeated);
            Assert.AreEqual(96.5f, record.BestSeconds, 0.01f);
            Assert.AreEqual(Difficulty.Hard, record.HighestDifficulty);

            QuestSaveEntry quest = loaded.QuestEntry("q06_catalogue");
            Assert.IsNotNull(quest);
            Assert.AreEqual(QuestState.ReadyToTurnIn, quest.State);
            Assert.AreEqual(5, quest.Counts[0]);
            Assert.AreEqual(20, quest.Counts[1]);
        }

        [Test]
        public void RoundTripIsByteStable()
        {
            SaveGame save = SampleSave();
            string first = SaveCodec.Encode(save);
            Assert.IsTrue(SaveCodec.TryDecode(first, out SaveGame loaded, out _));
            string second = SaveCodec.Encode(loaded);
            Assert.AreEqual(first, second, "encoding must be deterministic or the checksum is meaningless");
        }

        [Test]
        public void CorruptedFileIsRejected()
        {
            string json = SaveCodec.Encode(SampleSave());
            string tampered = json.Replace("\"salvage\": 2450", "\"salvage\": 999999");
            Assert.AreNotEqual(json, tampered, "test setup failed to modify the payload");

            Assert.IsFalse(SaveCodec.TryDecode(tampered, out _, out string error));
            Assert.IsNotNull(error);
            StringAssert.Contains("integrity", error);
        }

        [Test]
        public void GarbageInputIsRejected()
        {
            Assert.IsFalse(SaveCodec.TryDecode("not json at all", out _, out string error));
            Assert.IsNotNull(error);
            Assert.IsFalse(SaveCodec.TryDecode("{\"version\":3}", out _, out _));
            Assert.IsFalse(SaveCodec.TryDecode(string.Empty, out _, out _));
        }

        [Test]
        public void FutureVersionIsRejected()
        {
            string json = SaveCodec.Encode(SampleSave())
                .Replace("\"version\": 3", "\"version\": 99");
            Assert.IsFalse(SaveCodec.TryDecode(json, out _, out string error));
            StringAssert.Contains("newer", error);
        }

        [Test]
        public void OldSaveMigratesToCurrentVersion()
        {
            // Hand-built v1 save: no perks, no boss records, no gallery, no save counter.
            string legacy =
                "{\"version\":1,\"payload\":{" +
                "\"slotId\":\"slot1\",\"character\":2,\"difficulty\":1,\"level\":22,\"xpIntoLevel\":400," +
                "\"chapterIndex\":5,\"salvage\":900,\"perks\":[],\"flags\":[\"flag_ch04_complete\"]," +
                "\"chapters\":[\"ch01\",\"ch02\",\"ch03\",\"ch04\"],\"bosses\":[\"boss_skarn\",\"boss_tidewalker\"]," +
                "\"miniBosses\":[],\"weapons\":[\"w_needle\"],\"gear\":[\"g_weave_mk1\"]," +
                "\"comicPages\":[],\"gallery\":[],\"quests\":[],\"equipped\":{\"Weapon\":\"w_needle\"}," +
                "\"statPoints\":{\"Power\":10},\"weaponUpgrades\":{\"w_needle\":2}," +
                "\"stats\":{\"kills\":300,\"deaths\":3}}}";

            Assert.IsTrue(SaveCodec.TryDecode(legacy, out SaveGame loaded, out string error), error);
            Assert.AreEqual(SaveFormat.CurrentVersion, loaded.Version);
            Assert.IsNotNull(loaded.BossRecords);
            Assert.AreEqual(2, loaded.BossRecords.Count, "boss clears should be reconstructed during migration");
            Assert.AreEqual(2, loaded.BossesCleared);
            Assert.AreEqual(1, loaded.SaveCounter);
            Assert.IsFalse(loaded.IsAutosave);
            Assert.AreEqual(300, loaded.TotalKills);
            Assert.AreEqual(StatComputation.SpentIn(loaded.StatPoints, StatId.Power), 10);

            // The migrated save must still be loadable by the current content.
            loaded.PruneUnknownContent(_content);
            Assert.IsNotNull(loaded.ComputeStats(_content));
        }

        [Test]
        public void PruningDropsContentThatNoLongerExists()
        {
            SaveGame save = SampleSave();
            save.WeaponIds.Add("w_removed_weapon");
            save.GearIds.Add("g_removed_gear");
            save.Equip(EquipSlot.Trinket, "g_removed_gear");
            save.Quests.Add(new QuestSaveEntry { QuestId = "q_deleted_quest" });
            save.UnlockPerk("perk_deleted");

            save.PruneUnknownContent(_content);

            Assert.IsFalse(save.WeaponIds.Contains("w_removed_weapon"));
            Assert.IsFalse(save.GearIds.Contains("g_removed_gear"));
            Assert.AreEqual(string.Empty, save.EquippedGear(EquipSlot.Trinket));
            Assert.IsNull(save.QuestEntry("q_deleted_quest"));
            Assert.IsFalse(save.UnlockedPerkIds.Contains("perk_deleted"));
        }

        [Test]
        public void SanitizeClampsHostileValues()
        {
            SaveGame save = new SaveGame
            {
                Level = 900,
                ChapterIndex = 77,
                Salvage = -500,
                XpIntoLevel = 999999
            };
            save.StatPoints[StatId.Power] = 5000;
            save.Sanitize();

            Assert.AreEqual(StatComputation.MaxLevel, save.Level);
            Assert.AreEqual(ChapterRules.ChapterCount, save.ChapterIndex);
            Assert.AreEqual(0, save.Salvage);
            Assert.AreEqual(0, save.XpIntoLevel);
            Assert.LessOrEqual(StatComputation.PointsAlreadySpent(save.StatPoints),
                StatComputation.PointsForLevel(save.Level), "an edited save must not keep unearned points");
        }

        [Test]
        public void ComputedStatsGrowWithLevelAndGear()
        {
            SaveGame low = SaveCodec.NewGame(CharacterId.Kade, Difficulty.Normal, _content);
            StatBlock lowStats = low.ComputeStats(_content);

            SaveGame high = low.Clone();
            high.Level = 40;
            high.AddGear("g_plating_mk3", true);
            high.Equip(EquipSlot.ShieldCore, "g_core_bastion");
            StatBlock highStats = high.ComputeStats(_content);

            Assert.Greater(highStats.MaxHealth, lowStats.MaxHealth);
            Assert.Greater(highStats.Defense, lowStats.Defense);
            Assert.Greater(highStats.Shield, lowStats.Shield);
        }
    }

    [TestFixture]
    public class JsonTests
    {
        [Test]
        public void ParsesNestedStructures()
        {
            string text = "{\"a\":[1,2,{\"b\":\"c\"}],\"d\":{\"e\":true,\"f\":null},\"g\":-1.5e3}";
            JsonValue root = JsonValue.Parse(text);
            Assert.AreEqual(1, root["a"][0].AsInt());
            Assert.AreEqual("c", root["a"][2]["b"].AsString());
            Assert.IsTrue(root["d"]["e"].AsBool());
            Assert.IsTrue(root["d"]["f"].IsNull);
            Assert.AreEqual(-1500f, root["g"].AsFloat(), 0.001f);
        }

        [Test]
        public void HandlesEscapesAndUnicode()
        {
            string text = JsonValue.From("quote:\" slash:\\ newline:\n tab:\t uni:\u00e9").ToJson();
            JsonValue parsed = JsonValue.Parse(text);
            Assert.AreEqual("quote:\" slash:\\ newline:\n tab:\t uni:\u00e9", parsed.AsString());
        }

        [Test]
        public void RoundTripsAnObjectWithoutLosingKeys()
        {
            JsonValue obj = JsonValue.NewObject();
            obj.Add("z", 1);
            obj.Add("a", "two");
            obj.Add("m", JsonValue.NewArray().Add(JsonValue.From(true)));
            string text = obj.ToJson();
            JsonValue parsed = JsonValue.Parse(text);
            Assert.AreEqual(1, parsed["z"].AsInt());
            Assert.AreEqual("two", parsed["a"].AsString());
            Assert.IsTrue(parsed["m"][0].AsBool());
            Assert.AreEqual(text, parsed.ToJson(), "re-serialising must not reorder keys");
        }

        [TestCase("{")]
        [TestCase("{\"a\":}")]
        [TestCase("[1,2")]
        [TestCase("\"unterminated")]
        [TestCase("{\"a\" 1}")]
        [TestCase("tru")]
        [TestCase("{\"a\":1} trailing")]
        public void MalformedInputIsRejected(string text)
        {
            Assert.IsFalse(JsonValue.TryParse(text, out _, out string error), "should have failed: " + text);
            Assert.IsNotNull(error);
        }

        [Test]
        public void EmptyCollectionsAreSupported()
        {
            Assert.AreEqual("{}", JsonValue.NewObject().ToJson());
            Assert.AreEqual("[]", JsonValue.NewArray().ToJson());
            Assert.AreEqual(0, JsonValue.Parse("{}").Count);
            Assert.AreEqual(0, JsonValue.Parse("[]").Count);
        }

        [Test]
        public void ChecksumIsStableAndOrderSensitive()
        {
            Assert.AreEqual(Checksum.Fnv1a32Hex("iron veil"), Checksum.Fnv1a32Hex("iron veil"));
            Assert.AreNotEqual(Checksum.Fnv1a32Hex("iron veil"), Checksum.Fnv1a32Hex("veil iron"));
        }
    }

    [TestFixture]
    public class QuestRulesTests
    {
        private CampaignContent _content;

        [OneTimeSetUp]
        public void BuildOnce()
        {
            _content = Campaign.Build();
        }

        [Test]
        public void KillObjectiveTracksAndCompletes()
        {
            QuestDefinition quest = _content.Quest("q01_scrap_run");
            Assert.IsNotNull(quest);
            QuestProgress progress = QuestRules.NewProgress(quest);
            progress.State = QuestState.Active;

            Assert.IsTrue(QuestRules.Report(quest, progress, ObjectiveType.KillCount, "e_rust_drone", 6, out List<int> changed));
            Assert.AreEqual(1, changed.Count);
            Assert.AreEqual(6, progress.CountAt(0));
            Assert.AreEqual(QuestState.Active, progress.State, "neither objective is finished yet");

            // Over-reporting is clamped to the objective requirement.
            QuestRules.Report(quest, progress, ObjectiveType.KillCount, "e_rust_drone", 99, out _);
            Assert.AreEqual(12, progress.CountAt(0));
            Assert.AreEqual(QuestState.Active, progress.State, "the trooper objective is still open");

            QuestRules.Report(quest, progress, ObjectiveType.KillCount, "e_line_trooper", 6, out _);
            Assert.AreEqual(QuestState.ReadyToTurnIn, progress.State);
            Assert.AreEqual(1f, QuestRules.CompletionPercent(quest, progress), 0.001f);
        }

        [Test]
        public void WrongTargetDoesNotProgress()
        {
            QuestDefinition quest = _content.Quest("q01_scrap_run");
            QuestProgress progress = QuestRules.NewProgress(quest);
            progress.State = QuestState.Active;
            Assert.IsFalse(QuestRules.Report(quest, progress, ObjectiveType.KillCount, "e_veil_stalker", 5, out _));
            Assert.AreEqual(0, progress.CountAt(0));
        }

        [Test]
        public void OptionalObjectivesDoNotBlockCompletion()
        {
            QuestDefinition quest = _content.Quest("q01_lost_cartography");
            QuestProgress progress = QuestRules.NewProgress(quest);
            progress.State = QuestState.Active;
            QuestRules.Report(quest, progress, ObjectiveType.CollectItem, "itm_carto_shard", 3, out _);
            Assert.AreEqual(QuestState.ReadyToTurnIn, progress.State);
            Assert.Less(QuestRules.CompletionPercent(quest, progress), 1f, "the optional objective is still open");
        }

        [Test]
        public void TimedObjectiveAdvancesAndCompletes()
        {
            QuestDefinition quest = _content.Quest("q02_deep_salvage");
            QuestProgress progress = QuestRules.NewProgress(quest);
            progress.State = QuestState.Active;

            for (int i = 0; i < 45; i++) QuestRules.TickTimed(quest, progress, 1f);
            Assert.AreEqual(45, progress.CountAt(0));
            Assert.AreEqual(QuestState.Active, progress.State);

            for (int i = 0; i < 46; i++) QuestRules.TickTimed(quest, progress, 1f);
            Assert.AreEqual(90, progress.CountAt(0), "the timer must clamp at its requirement");
            Assert.AreEqual(QuestState.ReadyToTurnIn, progress.State,
                "the mandatory survive objective is done, so the quest is ready to hand in");
        }

        [Test]
        public void OptionalNoDamageObjectiveDoesNotFailTheQuest()
        {
            // Every no-damage objective in the campaign is a bonus objective, so taking
            // a hit costs the bonus but never the quest.
            QuestDefinition quest = _content.Quest("q05_stage_fright");
            QuestProgress progress = QuestRules.NewProgress(quest);
            progress.State = QuestState.Active;
            QuestRules.NoteDamageTaken(quest, progress);
            Assert.AreEqual(QuestState.Active, progress.State);
            Assert.IsTrue(progress.TookDamage);
        }

        [Test]
        public void MandatoryNoDamageObjectiveFailsTheQuest()
        {
            QuestDefinition quest = new QuestDefinition
            {
                Id = "test_perfect",
                Title = "Perfect run",
                GiverName = "Test",
                ChapterIndex = 1,
                Objectives = new List<QuestObjective>
                {
                    Objective(ObjectiveType.NoDamageRun, string.Empty, 1, "Take no damage", false),
                    Objective(ObjectiveType.ReachArea, "area_x", 1, "Reach the exit", false)
                }
            };
            QuestProgress progress = QuestRules.NewProgress(quest);
            progress.State = QuestState.Active;
            QuestRules.NoteDamageTaken(quest, progress);
            Assert.AreEqual(QuestState.Failed, progress.State);
        }

        private static QuestObjective Objective(ObjectiveType type, string target, int count, string description, bool optional)
        {
            return new QuestObjective
            {
                Id = "test_obj",
                Type = type,
                TargetId = target,
                RequiredCount = count,
                Description = description,
                Optional = optional
            };
        }

        [Test]
        public void PrerequisitesGateAvailability()
        {
            QuestDefinition locked = _content.Quest("q02_tide_charts");
            Assert.IsFalse(QuestRules.RequirementsMet(locked, new List<string>(), new List<string>()));
            Assert.IsTrue(QuestRules.RequirementsMet(locked, new List<string> { "q02_dock_rights" }, new List<string>()));
        }

        [Test]
        public void EverySideQuestCanBeAcceptedEventually()
        {
            for (int i = 0; i < _content.Quests.Count; i++)
            {
                QuestDefinition quest = _content.Quests[i];
                if (quest == null || quest.Kind == QuestKind.MainQuest) continue;
                Assert.IsNotNull(quest.PrimaryObjective, quest.Id + " has no primary objective");
                Assert.Greater(quest.EstimatedMinutes, 0, quest.Id + " has no length");
            }
        }
    }

    [TestFixture]
    public class SpawnRulesTests
    {
        private CampaignContent _content;

        [OneTimeSetUp]
        public void BuildOnce()
        {
            _content = Campaign.Build();
        }

        [Test]
        public void DifficultyChangesSpawnPressure()
        {
            EncounterDefinition encounter = _content.Encounter("enc_ch04_floor");
            Rng rngA = new Rng(11);
            Rng rngB = new Rng(11);

            List<PlannedSpawn> easy = SpawnRules.BuildSchedule(encounter, DifficultyProfile.For(Difficulty.Easy), 4, rngA);
            List<PlannedSpawn> hard = SpawnRules.BuildSchedule(encounter, DifficultyProfile.For(Difficulty.Hard), 4, rngB);

            Assert.Greater(easy.Count, 0);
            Assert.Greater(hard.Count, easy.Count, "Hard should spawn more enemies than Easy");
        }

        [Test]
        public void EliteCountIsRespected()
        {
            EncounterDefinition encounter = _content.Encounter("enc_ch08_gantry");
            List<PlannedSpawn> plan = SpawnRules.BuildSchedule(encounter, DifficultyProfile.For(Difficulty.Hard), 8, new Rng(4));
            int elites = 0;
            for (int i = 0; i < plan.Count; i++)
            {
                if (plan[i].IsElite) elites++;
            }
            Assert.LessOrEqual(elites, plan.Count);

            List<PlannedSpawn> easy = SpawnRules.BuildSchedule(encounter, DifficultyProfile.For(Difficulty.Easy), 8, new Rng(4));
            int easyElites = 0;
            for (int i = 0; i < easy.Count; i++)
            {
                if (easy[i].IsElite) easyElites++;
            }
            Assert.AreEqual(0, easyElites, "Easy difficulty spawns no elites");
        }

        [Test]
        public void ScheduleIsOrderedByTime()
        {
            EncounterDefinition encounter = _content.Encounter("enc_ch06_nave");
            List<PlannedSpawn> plan = SpawnRules.BuildSchedule(encounter, DifficultyProfile.For(Difficulty.Normal), 6, new Rng(7));
            Assert.Greater(plan.Count, 0);
            for (int i = 1; i < plan.Count; i++)
            {
                Assert.GreaterOrEqual(plan[i].Time, plan[i - 1].Time, "spawn schedule is out of order at " + i);
            }
        }

        [Test]
        public void ConcurrentCapIsSane()
        {
            Assert.GreaterOrEqual(SpawnRules.MaxConcurrentEnemies(DifficultyProfile.For(Difficulty.Easy)), 6);
            Assert.LessOrEqual(SpawnRules.MaxConcurrentEnemies(DifficultyProfile.For(Difficulty.Hard)), 28);
        }

        [Test]
        public void EveryEncounterReferencesRealContent()
        {
            for (int i = 0; i < _content.Encounters.Count; i++)
            {
                EncounterDefinition encounter = _content.Encounters[i];
                Assert.IsNotNull(_content.Chapter(encounter.ChapterIndex), encounter.Id + " has a bad chapter");
                for (int w = 0; w < encounter.Waves.Count; w++)
                {
                    for (int s = 0; s < encounter.Waves[w].Spawns.Count; s++)
                    {
                        EnemyArchetype enemy = _content.Enemy(encounter.Waves[w].Spawns[s].ArchetypeId);
                        Assert.IsNotNull(enemy, encounter.Id + " spawns unknown enemy");
                        Assert.LessOrEqual(enemy.FirstChapter, encounter.ChapterIndex,
                            encounter.Id + " spawns " + enemy.Id + " before its introduction");
                    }
                }
            }
        }
    }

    [TestFixture]
    public class BalanceTests
    {
        private CampaignContent _content;

        [OneTimeSetUp]
        public void BuildOnce()
        {
            _content = Campaign.Build();
        }

        [Test]
        public void PlayerSurvivesTheChapterTheyAreMeantToPlay()
        {
            DifficultyProfile normal = DifficultyProfile.For(Difficulty.Normal);
            foreach (CharacterId id in new[] { CharacterId.Kade, CharacterId.Vera })
            {
                for (int chapter = 1; chapter <= 12; chapter++)
                {
                    StatBlock stats = ContentValidator.PlayerStatsAt(_content, id, chapter);
                    float pool = stats.MaxHealth + stats.Shield;

                    EnemyArchetype baseline = null;
                    for (int i = 0; i < _content.Enemies.Count; i++)
                    {
                        EnemyArchetype enemy = _content.Enemies[i];
                        if (enemy.FirstChapter > chapter || enemy.IsSupport) continue;
                        if (baseline == null || enemy.Tier > baseline.Tier) baseline = enemy;
                    }
                    Assert.IsNotNull(baseline);

                    float perHit = EnemyScaling.Damage(baseline, chapter, normal) * DamageRules.MitigationMultiplier(stats.Defense);
                    float hits = pool / perHit;
                    Assert.GreaterOrEqual(hits, 3f, id + " dies too fast in chapter " + chapter);
                }
            }
        }

        [Test]
        public void BossFightsLastALongEnoughTime()
        {
            DifficultyProfile normal = DifficultyProfile.For(Difficulty.Normal);
            foreach (CharacterId id in new[] { CharacterId.Kade, CharacterId.Vera })
            {
                for (int chapter = 1; chapter <= 12; chapter++)
                {
                    StatBlock stats = ContentValidator.PlayerStatsAt(_content, id, chapter);
                    WeaponDefinition weapon = ContentValidator.WeaponForChapter(_content, id, chapter);
                    BossDefinition boss = _content.Boss(_content.Chapter(chapter).BossId);
                    Assert.IsNotNull(weapon, id + " has no weapon available in chapter " + chapter);

                    float dps = WeaponMath.SustainedDps(weapon, stats, boss.BaseDefense);
                    float health = boss.BaseHealth * normal.EnemyHealthMultiplier * ChapterRules.EnemyHealthMultiplier(chapter);
                    float ttk = health / dps;
                    Assert.GreaterOrEqual(ttk, 15f, id + " melts " + boss.Id + " in " + ttk.ToString("0.0") + "s");
                    Assert.LessOrEqual(ttk, 150f, boss.Id + " takes " + ttk.ToString("0.0") + "s for " + id);
                }
            }
        }

        [Test]
        public void WeaponUpgradeCostsRiseAndCap()
        {
            WeaponDefinition weapon = _content.Weapon("w_repeater");
            int previous = 0;
            for (int level = 1; level < weapon.MaxUpgradeLevel; level++)
            {
                int cost = WeaponMath.NextUpgradeCost(weapon, level);
                Assert.Greater(cost, previous);
                previous = cost;
            }
            Assert.AreEqual(-1, WeaponMath.NextUpgradeCost(weapon, weapon.MaxUpgradeLevel));
        }

        [Test]
        public void LevelCapIsReachableOnEveryDifficulty()
        {
            foreach (Difficulty difficulty in new[] { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard })
            {
                DifficultyProfile profile = DifficultyProfile.For(difficulty);
                int xp = ChapterRules.TotalMainPathXp(170, profile) + 40000; // plus side quests
                int level = XpCurve.LevelFromTotalXp(xp);
                Assert.GreaterOrEqual(level, 55, difficulty + " cannot reach the endgame level range");
            }
        }

        [Test]
        public void AbilitiesScaleWithAbilityPower()
        {
            CharacterDefinition kade = _content.Character(CharacterId.Kade);
            StatBlock baseStats = StatComputation.Compute(kade.BaseStats, kade.PerLevelGrowth, 1, null);
            Assert.Greater(AbilityMath.Damage(kade.SecondaryAbility, baseStats), 0f);

            StatBlock powered = baseStats.Clone();
            powered.AbilityPower = 2f;
            Assert.Greater(AbilityMath.Damage(kade.SecondaryAbility, powered), AbilityMath.Damage(kade.SecondaryAbility, baseStats));
            Assert.Less(AbilityMath.Cooldown(kade.SecondaryAbility, powered), AbilityMath.Cooldown(kade.SecondaryAbility, baseStats));
        }
    }
}
