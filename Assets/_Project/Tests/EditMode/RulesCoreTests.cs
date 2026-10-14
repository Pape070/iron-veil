using System.Collections.Generic;
using IronVeil.Rules;
using NUnit.Framework;

namespace IronVeil.Tests
{
    [TestFixture]
    public class RngTests
    {
        [Test]
        public void SameSeedProducesSameSequence()
        {
            Rng a = new Rng(1234);
            Rng b = new Rng(1234);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(a.NextUInt(), b.NextUInt(), "xorshift diverged at step " + i);
            }
        }

        [Test]
        public void DifferentSeedsDiverge()
        {
            Rng a = new Rng(1);
            Rng b = new Rng(2);
            bool sawDifference = false;
            for (int i = 0; i < 20; i++)
            {
                if (a.NextUInt() != b.NextUInt()) sawDifference = true;
            }
            Assert.IsTrue(sawDifference, "different seeds produced identical streams");
        }

        [Test]
        public void NextFloatStaysInRange()
        {
            Rng rng = new Rng(99);
            for (int i = 0; i < 10000; i++)
            {
                float value = rng.NextFloat();
                Assert.GreaterOrEqual(value, 0f);
                Assert.Less(value, 1f);
            }
        }

        [Test]
        public void WeightedIndexRespectsZeroWeights()
        {
            Rng rng = new Rng(7);
            List<float> weights = new List<float> { 0f, 5f, 0f };
            for (int i = 0; i < 200; i++)
            {
                Assert.AreEqual(1, rng.WeightedIndex(weights), "picked a zero-weight entry");
            }
        }

        [Test]
        public void WeightedIndexReturnsMinusOneWhenAllZero()
        {
            Rng rng = new Rng(3);
            Assert.AreEqual(-1, rng.WeightedIndex(new List<float> { 0f, 0f }));
        }
    }

    [TestFixture]
    public class XpCurveTests
    {
        [Test]
        public void CurveIsStrictlyIncreasing()
        {
            for (int level = 1; level < XpCurve.MaxLevel - 1; level++)
            {
                Assert.Greater(XpCurve.XpToNext(level + 1), XpCurve.XpToNext(level),
                    "XP requirement did not increase at level " + level);
            }
        }

        [Test]
        public void MaxLevelRequiresNoFurtherXp()
        {
            Assert.AreEqual(0, XpCurve.XpToNext(XpCurve.MaxLevel));
        }

        [Test]
        public void LevelFromTotalXpMatchesAccumulatedCurve()
        {
            for (int level = 1; level <= XpCurve.MaxLevel; level++)
            {
                int total = XpCurve.TotalXpForLevel(level);
                Assert.AreEqual(level, XpCurve.LevelFromTotalXp(total),
                    "cumulative XP for level " + level + " resolved to the wrong level");
                if (level > 1)
                {
                    Assert.AreEqual(level - 1, XpCurve.LevelFromTotalXp(total - 1),
                        "one XP below the threshold should not reach level " + level);
                }
            }
        }

        [Test]
        public void ApplyResolvesMultipleLevelUps()
        {
            LevelUpResult result = XpCurve.Apply(1, 0, XpCurve.TotalXpForLevel(5));
            Assert.AreEqual(5, result.Level);
            Assert.AreEqual(4, result.LevelsGained);
            Assert.Greater(result.PointsGained, 0);
        }

        [Test]
        public void ApplyStopsAtLevelCap()
        {
            LevelUpResult result = XpCurve.Apply(59, 0, 100000000);
            Assert.AreEqual(XpCurve.MaxLevel, result.Level);
            Assert.AreEqual(0, result.XpIntoLevel);
        }

        [Test]
        public void CampaignReachesLevelCapOnNormal()
        {
            // 12 chapters at ~170 kills each, plus bosses and mini-bosses, should
            // land the player at or near the level cap by the end of the story.
            int xp = ChapterRules.TotalMainPathXp(170, DifficultyProfile.For(Difficulty.Normal));
            Assert.GreaterOrEqual(XpCurve.LevelFromTotalXp(xp), 55,
                "main path XP (" + xp + ") is too low to finish the campaign near the cap");
        }

        [Test]
        public void ProgressStaysNormalised()
        {
            Assert.AreEqual(1f, XpCurve.Progress(XpCurve.MaxLevel, 0));
            Assert.AreEqual(0f, XpCurve.Progress(1, 0));
            Assert.Greater(XpCurve.Progress(10, 1), 0f);
            Assert.Less(XpCurve.Progress(10, 1), 1f);
        }
    }

    [TestFixture]
    public class StatComputationTests
    {
        private static StatBlock Base()
        {
            return new StatBlock { Power = 20f, Defense = 10f, Shield = 20f, MaxHealth = 120f, MoveSpeed = 6.5f };
        }

        private static StatBlock Growth()
        {
            StatBlock growth = StatBlock.Delta();
            growth.Power = 1.6f;
            growth.Defense = 1.1f;
            growth.Shield = 1.5f;
            growth.MaxHealth = 8f;
            return growth;
        }

        [Test]
        public void LevelOneMatchesBaseStats()
        {
            StatBlock stats = StatComputation.Compute(Base(), Growth(), 1, new Dictionary<StatId, int>());
            Assert.AreEqual(20f, stats.Power, 0.001f);
            Assert.AreEqual(120f, stats.MaxHealth, 0.001f);
        }

        [Test]
        public void GrowthAppliesPerLevel()
        {
            StatBlock stats = StatComputation.Compute(Base(), Growth(), 11, new Dictionary<StatId, int>());
            Assert.AreEqual(20f + 1.6f * 10f, stats.Power, 0.001f);
            Assert.AreEqual(120f + 8f * 10f, stats.MaxHealth, 0.001f);
        }

        [Test]
        public void SpendPointsRaisesStats()
        {
            Dictionary<StatId, int> spent = new Dictionary<StatId, int> { { StatId.Power, 5 } };
            StatBlock stats = StatComputation.Compute(Base(), Growth(), 5, spent);
            Assert.AreEqual(20f + 1.6f * 4f + 5f * StatCatalog.PerPoint(StatId.Power), stats.Power, 0.001f);
        }

        [Test]
        public void CannotSpendBeyondAvailablePoints()
        {
            // Spread the points around so the per-stat caps are not what stops us:
            // this test is specifically about the total point budget.
            Dictionary<StatId, int> spent = new Dictionary<StatId, int>();
            int allowed = StatComputation.PointsForLevel(20);
            for (int i = 0; i < allowed; i++)
            {
                StatId id = StatCatalog.All[i % StatCatalog.All.Length];
                Assert.IsTrue(StatComputation.TrySpend(spent, 20, id),
                    "spend " + i + " into " + id + " was rejected while points remained");
            }

            Assert.AreEqual(allowed, StatComputation.PointsAlreadySpent(spent));
            Assert.AreEqual(0, StatComputation.AvailablePoints(20, spent));

            for (int i = 0; i < StatCatalog.All.Length; i++)
            {
                Assert.IsFalse(StatComputation.TrySpend(spent, 20, StatCatalog.All[i]),
                    "overspend was allowed into " + StatCatalog.All[i]);
            }
            Assert.IsFalse(StatComputation.CanInvest(20, spent, StatId.Power));
        }

        [Test]
        public void PerStatCapIsEnforced()
        {
            Dictionary<StatId, int> spent = new Dictionary<StatId, int>();
            int cap = StatCatalog.PointCap(StatId.CritChance);
            for (int i = 0; i < cap; i++)
            {
                StatComputation.TrySpend(spent, StatComputation.MaxLevel, StatId.CritChance);
            }
            Assert.IsFalse(StatComputation.CanInvest(StatComputation.MaxLevel, spent, StatId.CritChance));
            Assert.AreEqual(cap, StatComputation.SpentIn(spent, StatId.CritChance));
        }

        [Test]
        public void RefundReturnsPoints()
        {
            Dictionary<StatId, int> spent = new Dictionary<StatId, int> { { StatId.Shield, 3 } };
            Assert.IsTrue(StatComputation.TryRefund(spent, StatId.Shield));
            Assert.AreEqual(2, StatComputation.SpentIn(spent, StatId.Shield));
            StatComputation.TryRefund(spent, StatId.Shield);
            StatComputation.TryRefund(spent, StatId.Shield);
            Assert.IsFalse(StatComputation.TryRefund(spent, StatId.Shield));
        }

        [Test]
        public void MultiplierStatsAreNormalised()
        {
            // Compute returns a normalised copy; the caller's block is left untouched.
            StatBlock input = new StatBlock { FireRateMul = 99f, CritChance = 5f, MoveSpeed = 900f };
            StatBlock result = StatComputation.Compute(input, null, 1, null);
            Assert.LessOrEqual(result.FireRateMul, 4f);
            Assert.LessOrEqual(result.CritChance, 0.9f);
            Assert.LessOrEqual(result.MoveSpeed, 20f);
            Assert.AreEqual(99f, input.FireRateMul, 0.001f, "Compute must not mutate its input");
        }
    }

    [TestFixture]
    public class DamageRulesTests
    {
        [Test]
        public void MitigationFollowsCurveAndCaps()
        {
            Assert.AreEqual(0f, DamageRules.Mitigation(0f), 0.0001f);
            Assert.AreEqual(0.5f, DamageRules.Mitigation(100f), 0.0001f);
            Assert.AreEqual(0.75f, DamageRules.Mitigation(100000f), 0.0001f);
        }

        [Test]
        public void ArmorPierceReducesMitigation()
        {
            float full = DamageRules.MitigationMultiplier(100f, 0f);
            float pierced = DamageRules.MitigationMultiplier(100f, 1f);
            Assert.AreEqual(1f, pierced, 0.0001f);
            Assert.Greater(pierced, full);
        }

        [Test]
        public void PowerScalingMatchesReference()
        {
            Assert.AreEqual(1f, DamageRules.PowerMultiplier(DamageRules.ReferencePower, 1f), 0.0001f);
            Assert.AreEqual(2f, DamageRules.PowerMultiplier(DamageRules.ReferencePower * 2f, 1f), 0.0001f);
            Assert.AreEqual(1f, DamageRules.PowerMultiplier(1000f, 0f), 0.0001f);
        }

        [Test]
        public void CritIsDeterministicForAGivenSeed()
        {
            DamageRequest request = new DamageRequest
            {
                BaseDamage = 100f,
                Power = DamageRules.ReferencePower,
                PowerScaling = 1f,
                CritChance = 1f,
                CritDamageMultiplier = 2f,
                CanCrit = true
            };
            DamageOutcome outcome = DamageRules.ComputeOutgoing(request, new Rng(5), 0f, 1f, 1000f);
            Assert.IsTrue(outcome.IsCrit);
            Assert.AreEqual(200f, outcome.Total, 0.01f);
        }

        [Test]
        public void NoCritWhenDisabled()
        {
            DamageRequest request = new DamageRequest
            {
                BaseDamage = 50f,
                Power = DamageRules.ReferencePower,
                PowerScaling = 1f,
                CritChance = 1f,
                CritDamageMultiplier = 3f,
                CanCrit = false
            };
            DamageOutcome outcome = DamageRules.ComputeOutgoing(request, new Rng(1), 0f, 1f, 1000f);
            Assert.IsFalse(outcome.IsCrit);
            Assert.AreEqual(50f, outcome.Total, 0.01f);
        }

        [Test]
        public void ShieldsAbsorbBeforeHealth()
        {
            DamageOutcome outcome = DamageRules.ComputeIncoming(60f, 0f, 1f, 100f, 120f);
            Assert.AreEqual(60f, outcome.ShieldAbsorbed, 0.01f);
            Assert.AreEqual(0f, outcome.HealthDamage, 0.01f);
            Assert.IsFalse(outcome.IsLethal);
        }

        [Test]
        public void OverflowDamageReachesHealth()
        {
            DamageOutcome outcome = DamageRules.ComputeIncoming(150f, 0f, 1f, 40f, 120f);
            Assert.AreEqual(40f, outcome.ShieldAbsorbed, 0.01f);
            Assert.AreEqual(110f, outcome.HealthDamage, 0.01f);
            Assert.IsFalse(outcome.IsLethal);
        }

        [Test]
        public void LethalDamageIsFlagged()
        {
            DamageOutcome outcome = DamageRules.ComputeIncoming(150f, 0f, 1f, 0f, 100f);
            Assert.IsTrue(outcome.IsLethal);
        }

        [Test]
        public void DefenseReducesIncomingDamage()
        {
            DamageOutcome soft = DamageRules.ComputeIncoming(100f, 0f, 1f, 0f, 1000f);
            DamageOutcome armored = DamageRules.ComputeIncoming(100f, 100f, 1f, 0f, 1000f);
            Assert.Greater(soft.Total, armored.Total);
            Assert.AreEqual(50f, armored.Total, 0.01f);
        }

        [Test]
        public void HardDifficultyHitsHarder()
        {
            DifficultyProfile hard = DifficultyProfile.For(Difficulty.Hard);
            DamageOutcome normalHit = DamageRules.ComputeIncoming(100f, 50f, 1f, 0f, 10000f);
            DamageOutcome hardHit = DamageRules.ComputeIncoming(100f, 50f, hard.EnemyDamageMultiplier, 0f, 10000f);
            Assert.Greater(hardHit.Total, normalHit.Total);
        }

        [Test]
        public void ShieldRechargeWaitsForTheDelay()
        {
            float noRegen = DamageRules.RechargeShield(0f, 100f, 10f, 0.2f, 3f, 1f);
            Assert.AreEqual(0f, noRegen, 0.001f);
            float regen = DamageRules.RechargeShield(0f, 100f, 10f, 5f, 3f, 1f);
            Assert.AreEqual(10f, regen, 0.001f);
        }

        [Test]
        public void HealingRespectsMaximum()
        {
            Assert.AreEqual(100f, DamageRules.ApplyHealing(95f, 50f, 100f), 0.001f);
            Assert.AreEqual(0f, DamageRules.ApplyToHealth(5f, 50f), 0.001f);
        }

        [Test]
        public void HitsToKillIsFinite()
        {
            DamageRequest request = new DamageRequest { BaseDamage = 10f, Power = 25f, PowerScaling = 1f };
            Assert.AreEqual(10, DamageRules.HitsToKill(request, 0f, 100f));
        }
    }

    [TestFixture]
    public class DifficultyTests
    {
        [Test]
        public void NormalIsTheBaseline()
        {
            DifficultyProfile normal = DifficultyProfile.For(Difficulty.Normal);
            Assert.AreEqual(1f, normal.EnemyHealthMultiplier, 0.0001f);
            Assert.AreEqual(1f, normal.EnemyDamageMultiplier, 0.0001f);
            Assert.AreEqual(PatternTier.Extended, normal.PatternTier);
        }

        [Test]
        public void EasyIsEasierThanNormalWhichIsEasierThanHard()
        {
            DifficultyProfile easy = DifficultyProfile.For(Difficulty.Easy);
            DifficultyProfile normal = DifficultyProfile.For(Difficulty.Normal);
            DifficultyProfile hard = DifficultyProfile.For(Difficulty.Hard);

            Assert.Less(easy.EnemyHealthMultiplier, normal.EnemyHealthMultiplier);
            Assert.Less(normal.EnemyHealthMultiplier, hard.EnemyHealthMultiplier);
            Assert.Less(easy.EnemyDamageMultiplier, normal.EnemyDamageMultiplier);
            Assert.Less(normal.EnemyDamageMultiplier, hard.EnemyDamageMultiplier);
            Assert.Greater(easy.PlayerInvulnerabilitySeconds, hard.PlayerInvulnerabilitySeconds);
            Assert.Greater(easy.TelegraphTimeScale, hard.TelegraphTimeScale);
        }

        [Test]
        public void HardUsesEveryPatternAndEasyUsesOnlyCore()
        {
            PatternDefinition core = new PatternDefinition { RequiredTier = PatternTier.Core };
            PatternDefinition extended = new PatternDefinition { RequiredTier = PatternTier.Extended };
            PatternDefinition full = new PatternDefinition { RequiredTier = PatternTier.Full };

            DifficultyProfile easy = DifficultyProfile.For(Difficulty.Easy);
            DifficultyProfile hard = DifficultyProfile.For(Difficulty.Hard);

            Assert.IsTrue(easy.AllowsPattern(core));
            Assert.IsFalse(easy.AllowsPattern(extended));
            Assert.IsFalse(easy.AllowsPattern(full));
            Assert.IsTrue(hard.AllowsPattern(core));
            Assert.IsTrue(hard.AllowsPattern(extended));
            Assert.IsTrue(hard.AllowsPattern(full));
        }

        [Test]
        public void ThreeProfilesExistWithNormalAsDefault()
        {
            DifficultyProfile[] all = DifficultyProfile.All();
            Assert.AreEqual(3, all.Length);
            Assert.AreEqual(Difficulty.Normal, all[1].Difficulty);
        }
    }

    [TestFixture]
    public class BossPatternRunnerTests
    {
        private static BossDefinition TwoPhaseBoss()
        {
            BossDefinition boss = new BossDefinition
            {
                Id = "test_boss",
                DisplayName = "Test Boss",
                BaseHealth = 1000f,
                Phases = new List<BossPhaseDefinition>
                {
                    new BossPhaseDefinition
                    {
                        Id = "phase1",
                        EnterAtHealthRatio = 1f,
                        CooldownMultiplier = 1f,
                        Patterns = new List<PatternDefinition>
                        {
                            new PatternDefinition { Id = "p1", Cooldown = 3f, Telegraph = 0.5f, Duration = 0.4f, Recovery = 0.3f, Weight = 1f, RequiredTier = PatternTier.Core },
                            new PatternDefinition { Id = "p2", Cooldown = 5f, Telegraph = 0.6f, Duration = 0.5f, Recovery = 0.3f, Weight = 1f, RequiredTier = PatternTier.Core },
                            new PatternDefinition { Id = "p3", Cooldown = 8f, Telegraph = 0.7f, Duration = 0.6f, Recovery = 0.4f, Weight = 1f, RequiredTier = PatternTier.Full }
                        }
                    },
                    new BossPhaseDefinition
                    {
                        Id = "phase2",
                        EnterAtHealthRatio = 0.5f,
                        CooldownMultiplier = 0.8f,
                        Patterns = new List<PatternDefinition>
                        {
                            new PatternDefinition { Id = "p4", Cooldown = 2f, Telegraph = 0.4f, Duration = 0.3f, Recovery = 0.2f, Weight = 2f, RequiredTier = PatternTier.Core },
                            new PatternDefinition { Id = "p5", Cooldown = 4f, Telegraph = 0.5f, Duration = 0.4f, Recovery = 0.3f, Weight = 1f, RequiredTier = PatternTier.Extended }
                        }
                    }
                }
            };
            return boss;
        }

        [Test]
        public void NormalDifficultyExcludesFullTierPatterns()
        {
            BossDefinition boss = TwoPhaseBoss();
            BossPatternRunner runner = new BossPatternRunner(boss, DifficultyProfile.For(Difficulty.Normal), new Rng(1));
            Assert.AreEqual(2, runner.PatternsInCurrentPhase, "Normal should run Core + Extended patterns only");
        }

        [Test]
        public void HardDifficultyRunsEveryPattern()
        {
            BossDefinition boss = TwoPhaseBoss();
            BossPatternRunner runner = new BossPatternRunner(boss, DifficultyProfile.For(Difficulty.Hard), new Rng(1));
            Assert.AreEqual(3, runner.PatternsInCurrentPhase);
        }

        [Test]
        public void EasyDifficultyRunsOnlyCorePatterns()
        {
            BossDefinition boss = TwoPhaseBoss();
            BossPatternRunner runner = new BossPatternRunner(boss, DifficultyProfile.For(Difficulty.Easy), new Rng(1));
            Assert.AreEqual(2, runner.PatternsInCurrentPhase);
        }

        [Test]
        public void PhaseTransitionsOnHealthDrop()
        {
            BossDefinition boss = TwoPhaseBoss();
            BossPatternRunner runner = new BossPatternRunner(boss, DifficultyProfile.For(Difficulty.Normal), new Rng(1));
            PatternEventFlags events = runner.Tick(0.016f, 0.4f);
            Assert.IsTrue((events & PatternEventFlags.PhaseChanged) != 0, "phase change was not reported");
            Assert.AreEqual(1, runner.PhaseIndex);
        }

        [Test]
        public void PatternLifecycleRunsTelegraphActiveRecoveryThenIdle()
        {
            BossDefinition boss = TwoPhaseBoss();
            DifficultyProfile normal = DifficultyProfile.For(Difficulty.Normal);
            BossPatternRunner runner = new BossPatternRunner(boss, normal, new Rng(4));

            PatternDefinition started = runner.StartNextPattern();
            Assert.IsNotNull(started);
            Assert.AreEqual(PatternState.Telegraph, runner.State);

            float step = 0.05f;
            bool fired = false, ended = false;
            for (int i = 0; i < 200; i++)
            {
                PatternEventFlags events = runner.Tick(step, 1f);
                if ((events & PatternEventFlags.PatternFired) != 0) fired = true;
                if ((events & PatternEventFlags.PatternEnded) != 0) ended = true;
                if (ended && runner.State == PatternState.Idle) break;
            }
            Assert.IsTrue(fired, "the pattern never fired");
            Assert.IsTrue(ended, "the pattern never ended");
            Assert.AreEqual(PatternState.Idle, runner.State);
        }

        [Test]
        public void NeverRepeatsTheSamePatternTwiceInARow()
        {
            BossDefinition boss = TwoPhaseBoss();
            BossPatternRunner runner = new BossPatternRunner(boss, DifficultyProfile.For(Difficulty.Hard), new Rng(11));
            string previous = null;
            for (int i = 0; i < 200; i++)
            {
                PatternDefinition picked = runner.StartNextPattern();
                if (picked == null) break;
                Assert.AreNotEqual(previous, picked.Id, "pattern repeated back to back: " + picked.Id);
                previous = picked.Id;
                for (int t = 0; t < 400; t++)
                {
                    runner.Tick(0.05f, 1f);
                    if (runner.State == PatternState.Idle) break;
                }
            }
        }

        [Test]
        public void CooldownsPreventImmediateReuse()
        {
            BossDefinition boss = TwoPhaseBoss();
            BossPatternRunner runner = new BossPatternRunner(boss, DifficultyProfile.For(Difficulty.Hard), new Rng(2));
            PatternDefinition first = runner.StartNextPattern();
            Assert.IsNotNull(first);
            Assert.Greater(runner.CooldownRemaining(first.Id), 0f);

            for (int t = 0; t < 2000; t++)
            {
                runner.Tick(0.01f, 1f);
                if (runner.State == PatternState.Idle) break;
            }
            Assert.Greater(runner.SecondsSinceUse(first.Id), 0f);
        }

        [Test]
        public void EnrageShortensCooldownsAfterTheWindow()
        {
            BossDefinition boss = TwoPhaseBoss();
            boss.EnrageAfterSeconds = 5f;
            BossPatternRunner runner = new BossPatternRunner(boss, DifficultyProfile.For(Difficulty.Normal), new Rng(3));
            PatternEventFlags events = runner.Tick(6f, 1f);
            Assert.IsTrue((events & PatternEventFlags.Enraged) != 0, "enrage was not reported");
            Assert.IsTrue(runner.IsEnraged);
        }

        [Test]
        public void NeverStallsWhenOnlyOnePatternIsAllowed()
        {
            BossDefinition boss = new BossDefinition
            {
                Id = "single",
                Phases = new List<BossPhaseDefinition>
                {
                    new BossPhaseDefinition
                    {
                        EnterAtHealthRatio = 1f,
                        Patterns = new List<PatternDefinition>
                        {
                            new PatternDefinition { Id = "only", Cooldown = 0.5f, Telegraph = 0.2f, Duration = 0.2f, Recovery = 0.2f }
                        }
                    }
                }
            };
            BossPatternRunner runner = new BossPatternRunner(boss, DifficultyProfile.For(Difficulty.Easy), new Rng(8));
            int executed = 0;
            for (int i = 0; i < 60; i++)
            {
                if (runner.StartNextPattern() != null) executed++;
                for (int t = 0; t < 200; t++)
                {
                    runner.Tick(0.02f, 1f);
                    if (runner.State == PatternState.Idle) break;
                }
            }
            Assert.Greater(executed, 5, "a single-pattern boss stalled out");
        }

        [Test]
        public void ForcedPatternBypassesSelection()
        {
            BossDefinition boss = TwoPhaseBoss();
            BossPatternRunner runner = new BossPatternRunner(boss, DifficultyProfile.For(Difficulty.Easy), new Rng(6));
            PatternDefinition forced = runner.ForcePattern(boss.Phases[0].Patterns[0]);
            Assert.IsNotNull(forced);
            Assert.AreEqual("p1", runner.ActivePattern.Id);
        }
    }
}
