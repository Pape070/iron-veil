using System;
using System.Collections.Generic;

namespace IronVeil.Rules
{
    /// <summary>
    /// Every attack pattern in the game. Bosses are built by combining these with
    /// different tuning, which is what makes each fight read differently.
    /// </summary>
    public enum AttackPatternKind
    {
        RadialBurst = 0,
        AimedVolley = 1,
        SweepingBeam = 2,
        ChargeDash = 3,
        GroundPound = 4,
        MissileBarrage = 5,
        SummonAdds = 6,
        PhaseBlink = 7,
        ArenaHazard = 8,
        HomingSwarm = 9,
        ShieldRecharge = 10,
        TurretDeploy = 11,
        ShockwaveTrap = 12,
        CloakAndStrike = 13,
        BeamCross = 14,
        Minefield = 15,
        GrappleSlam = 16,
        ReflectShield = 17,
        LaserGrid = 18,
        TailSweep = 19
    }

    /// <summary>One authored attack within a boss phase.</summary>
    [Serializable]
    public sealed class PatternDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public AttackPatternKind Kind = AttackPatternKind.RadialBurst;

        /// <summary>Minimum difficulty tier that unlocks this pattern.</summary>
        public PatternTier RequiredTier = PatternTier.Core;

        /// <summary>Relative pick weight inside the phase.</summary>
        public float Weight = 1f;

        /// <summary>Seconds before the pattern may be used again.</summary>
        public float Cooldown = 4f;

        /// <summary>Seconds of warning before the attack lands. Scaled by difficulty.</summary>
        public float Telegraph = 0.8f;

        /// <summary>Seconds the attack stays dangerous / animates.</summary>
        public float Duration = 0.6f;

        /// <summary>Seconds of recovery before the boss may act again.</summary>
        public float Recovery = 0.5f;

        public float Damage = 12f;
        public float Speed = 9f;
        public int ProjectileCount = 8;
        public float SpreadAngle = 360f;
        public float Range = 14f;

        /// <summary>Area of effect radius for pound, slam and radial patterns.</summary>
        public float Radius = 3f;
        public bool TrackPlayer = true;
        public bool IsWeakPointExposed = false;

        /// <summary>Free-form markers the presentation layer reads (e.g. "air", "adds").</summary>
        public string[] Tags = Array.Empty<string>();

        /// <summary>Hint shown on Easy when the attack begins.</summary>
        public string EasyHint = string.Empty;

        public bool HasTag(string tag)
        {
            if (Tags == null || string.IsNullOrEmpty(tag)) return false;
            for (int i = 0; i < Tags.Length; i++)
            {
                if (string.Equals(Tags[i], tag, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public PatternDefinition Clone()
        {
            return new PatternDefinition
            {
                Id = Id,
                DisplayName = DisplayName,
                Kind = Kind,
                RequiredTier = RequiredTier,
                Weight = Weight,
                Cooldown = Cooldown,
                Telegraph = Telegraph,
                Duration = Duration,
                Recovery = Recovery,
                Damage = Damage,
                Speed = Speed,
                ProjectileCount = ProjectileCount,
                SpreadAngle = SpreadAngle,
                Range = Range,
                Radius = Radius,
                TrackPlayer = TrackPlayer,
                IsWeakPointExposed = IsWeakPointExposed,
                Tags = Tags == null ? Array.Empty<string>() : (string[])Tags.Clone(),
                EasyHint = EasyHint
            };
        }
    }

    /// <summary>A boss phase. Phases unlock in order as health drops.</summary>
    [Serializable]
    public sealed class BossPhaseDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;

        /// <summary>Health ratio (0-1) at or below which this phase begins.</summary>
        public float EnterAtHealthRatio = 1f;

        public float SpeedMultiplier = 1f;
        public float DamageMultiplier = 1f;
        public float CooldownMultiplier = 1f;
        public float IncomingDamageMultiplier = 1f;
        public bool SpawnsAdds = false;
        public int AddsPerWave = 0;
        public string Announcement = string.Empty;
        public List<PatternDefinition> Patterns = new List<PatternDefinition>();

        public List<PatternDefinition> AvailablePatterns(DifficultyProfile difficulty)
        {
            List<PatternDefinition> result = new List<PatternDefinition>();
            if (Patterns == null) return result;
            for (int i = 0; i < Patterns.Count; i++)
            {
                PatternDefinition pattern = Patterns[i];
                if (pattern == null) continue;
                if (difficulty != null && !difficulty.AllowsPattern(pattern)) continue;
                result.Add(pattern);
            }
            return result;
        }
    }

    /// <summary>A boss or mini-boss.</summary>
    [Serializable]
    public sealed class BossDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public string Title = string.Empty;
        public string Epithet = string.Empty;
        public int ChapterIndex = 1;
        public bool IsMiniBoss = false;
        public bool IsFinalBoss = false;
        public string ArenaId = string.Empty;

        public float BaseHealth = 4000f;
        public float BaseDamage = 20f;
        public float BaseDefense = 40f;
        public float BaseSpeed = 4f;

        /// <summary>Weak points that grant bonus damage when hit.</summary>
        public int WeakPointCount = 1;

        /// <summary>Damage types this boss resists; the UI surfaces them as loadout hints.</summary>
        public string[] Resistances = Array.Empty<string>();

        public List<BossPhaseDefinition> Phases = new List<BossPhaseDefinition>();

        /// <summary>Mini-boss reward: upgrade ids granted on defeat.</summary>
        public List<string> UpgradeDrops = new List<string>();

        public List<string> WeaponDrops = new List<string>();
        public string MusicId = string.Empty;
        public string IntroComicId = string.Empty;
        public string OutroComicId = string.Empty;
        public string BestiaryEntry = string.Empty;

        /// <summary>Seconds before the boss enrages (cooldowns shrink). 0 disables enrage.</summary>
        public float EnrageAfterSeconds = 0f;

        /// <summary>Cooldown multiplier once enraged.</summary>
        public float EnrageCooldownMultiplier = 0.6f;

        public int PhaseCount => Phases == null ? 0 : Phases.Count;

        public BossPhaseDefinition PhaseAt(int index)
        {
            if (Phases == null || Phases.Count == 0) return null;
            return Phases[MathLite.Clamp(index, 0, Phases.Count - 1)];
        }

        /// <summary>Highest phase whose health threshold has been reached.</summary>
        public int PhaseIndexForHealthRatio(float healthRatio)
        {
            if (Phases == null || Phases.Count == 0) return 0;
            int index = 0;
            for (int i = 0; i < Phases.Count; i++)
            {
                if (healthRatio <= Phases[i].EnterAtHealthRatio + MathLite.Epsilon) index = i;
            }
            return index;
        }

        public int TotalPatterns(DifficultyProfile difficulty)
        {
            int total = 0;
            if (Phases == null) return 0;
            for (int i = 0; i < Phases.Count; i++)
            {
                total += Phases[i].AvailablePatterns(difficulty).Count;
            }
            return total;
        }
    }

    public enum PatternState
    {
        Idle = 0,
        Telegraph = 1,
        Active = 2,
        Recovery = 3
    }

    [Flags]
    public enum PatternEventFlags
    {
        None = 0,
        PhaseChanged = 1 << 0,
        TelegraphBegan = 1 << 1,
        PatternFired = 1 << 2,
        PatternEnded = 1 << 3,
        Enraged = 1 << 4
    }

    /// <summary>
    /// Pure boss attack scheduler: phase transitions, cooldowns, anti-repeat
    /// weighting, telegraph / active / recovery timing and enrage. No Unity
    /// dependency, so the behaviour is unit tested rather than hoped for.
    /// </summary>
    public sealed class BossPatternRunner
    {
        private const float RepeatWeightPenalty = 0.3f;
        private readonly BossDefinition _boss;
        private readonly DifficultyProfile _difficulty;
        private readonly Rng _rng;
        private readonly Dictionary<string, float> _cooldownRemaining = new Dictionary<string, float>();
        private readonly Dictionary<string, float> _lastUsedAt = new Dictionary<string, float>();
        private readonly List<PatternDefinition> _candidates = new List<PatternDefinition>();
        private readonly List<float> _weights = new List<float>();

        private float _elapsed;
        private float _stateTimer;
        private string _lastPatternId = string.Empty;
        private string _previousPatternId = string.Empty;
        private bool _enraged;

        public BossPatternRunner(BossDefinition boss, DifficultyProfile difficulty, Rng rng)
        {
            _boss = boss ?? throw new ArgumentNullException(nameof(boss));
            // Fully qualified because this class has a Difficulty property that would
            // otherwise shadow the enum type name.
            _difficulty = difficulty ?? DifficultyProfile.For(IronVeil.Rules.Difficulty.Normal);
            _rng = rng ?? new Rng(1337);
            PhaseIndex = 0;
            State = PatternState.Idle;
            ActivePattern = null;
        }

        public BossDefinition Boss => _boss;

        public DifficultyProfile Difficulty => _difficulty;

        public int PhaseIndex { get; private set; }

        public PatternState State { get; private set; }

        public PatternDefinition ActivePattern { get; private set; }

        public float Elapsed => _elapsed;

        public bool IsEnraged => _enraged;

        /// <summary>Seconds remaining in the current state.</summary>
        public float StateTimeRemaining => MathLite.Max(0f, _stateTimer);

        /// <summary>Progress through the current state in [0, 1].</summary>
        public float StateProgress
        {
            get
            {
                float total = DurationOf(State, ActivePattern);
                if (total <= 0f) return 1f;
                return MathLite.Clamp01(1f - _stateTimer / total);
            }
        }

        public BossPhaseDefinition CurrentPhase => _boss.PhaseAt(PhaseIndex);

        public int PatternsInCurrentPhase => CurrentPhase == null ? 0 : CurrentPhase.AvailablePatterns(_difficulty).Count;

        /// <summary>Patterns already cooled down and legal to start right now.</summary>
        public int ReadyPatternCount
        {
            get
            {
                List<PatternDefinition> available = AvailablePatterns();
                int count = 0;
                for (int i = 0; i < available.Count; i++)
                {
                    if (IsReady(available[i])) count++;
                }
                return count;
            }
        }

        /// <summary>Advances timers and returns everything that happened this tick.</summary>
        public PatternEventFlags Tick(float deltaTime, float healthRatio)
        {
            PatternEventFlags events = PatternEventFlags.None;
            if (deltaTime <= 0f) deltaTime = 0f;

            _elapsed += deltaTime;

            if (!_enraged && _boss.EnrageAfterSeconds > 0f && _elapsed >= _boss.EnrageAfterSeconds)
            {
                _enraged = true;
                events |= PatternEventFlags.Enraged;
            }

            foreach (string key in new List<string>(_cooldownRemaining.Keys))
            {
                _cooldownRemaining[key] = MathLite.Max(0f, _cooldownRemaining[key] - deltaTime);
            }

            events |= UpdatePhase(healthRatio);

            if (State == PatternState.Idle) return events;

            _stateTimer -= deltaTime;
            if (_stateTimer > 0f) return events;

            switch (State)
            {
                case PatternState.Telegraph:
                    State = PatternState.Active;
                    _stateTimer = DurationOf(PatternState.Active, ActivePattern);
                    events |= PatternEventFlags.PatternFired;
                    break;
                case PatternState.Active:
                    State = PatternState.Recovery;
                    _stateTimer = DurationOf(PatternState.Recovery, ActivePattern);
                    events |= PatternEventFlags.PatternEnded;
                    break;
                case PatternState.Recovery:
                    State = PatternState.Idle;
                    _stateTimer = 0f;
                    ActivePattern = null;
                    break;
            }

            return events;
        }

        /// <summary>Picks and telegraphs the next pattern. Returns null when nothing is ready.</summary>
        public PatternDefinition StartNextPattern()
        {
            if (State != PatternState.Idle) return null;

            PatternDefinition chosen = PickPattern();
            if (chosen == null) return null;

            ActivePattern = chosen;
            _previousPatternId = _lastPatternId;
            _lastPatternId = chosen.Id;

            _lastUsedAt[chosen.Id] = _elapsed;
            _cooldownRemaining[chosen.Id] = CooldownOf(chosen);

            State = PatternState.Telegraph;
            _stateTimer = _difficulty.ScaleTelegraph(chosen.Telegraph);
            return chosen;
        }

        /// <summary>Weighted pick honouring difficulty tier, cooldowns and anti-repeat.</summary>
        public PatternDefinition PickPattern()
        {
            List<PatternDefinition> available = AvailablePatterns();
            _candidates.Clear();
            _weights.Clear();

            for (int i = 0; i < available.Count; i++)
            {
                PatternDefinition pattern = available[i];
                float weight = pattern.Weight <= 0f ? 1f : pattern.Weight;

                if (!IsReady(pattern)) continue;

                // Never repeat the exact last pattern back to back when alternatives exist.
                if (available.Count > 1 && pattern.Id == _lastPatternId) continue;
                if (available.Count > 2 && pattern.Id == _previousPatternId) weight *= RepeatWeightPenalty;

                _candidates.Add(pattern);
                _weights.Add(weight);
            }

            if (_candidates.Count == 0)
            {
                // Fall back to ignoring anti-repeat so a boss never stalls out.
                for (int i = 0; i < available.Count; i++)
                {
                    if (!IsReady(available[i])) continue;
                    _candidates.Add(available[i]);
                    _weights.Add(available[i].Weight <= 0f ? 1f : available[i].Weight);
                }
            }

            if (_candidates.Count == 0) return null;

            int index = _rng.WeightedIndex(_weights);
            return index < 0 ? null : _candidates[index];
        }

        public List<PatternDefinition> AvailablePatterns()
        {
            BossPhaseDefinition phase = CurrentPhase;
            if (phase == null) return new List<PatternDefinition>();

            // Low health phases may be called while a previous phase's patterns are
            // still cooling down; always work off the current phase only.
            return phase.AvailablePatterns(_difficulty);
        }

        public float CooldownRemaining(string patternId)
        {
            if (string.IsNullOrEmpty(patternId)) return 0f;
            return _cooldownRemaining.TryGetValue(patternId, out float value) ? value : 0f;
        }

        public float SecondsSinceUse(string patternId)
        {
            if (string.IsNullOrEmpty(patternId)) return float.MaxValue;
            return _lastUsedAt.TryGetValue(patternId, out float usedAt) ? _elapsed - usedAt : float.MaxValue;
        }

        /// <summary>Forces a specific pattern, used by scripted phase openers.</summary>
        public PatternDefinition ForcePattern(PatternDefinition pattern)
        {
            if (pattern == null) return null;
            ActivePattern = pattern;
            _previousPatternId = _lastPatternId;
            _lastPatternId = pattern.Id;
            _lastUsedAt[pattern.Id] = _elapsed;
            _cooldownRemaining[pattern.Id] = CooldownOf(pattern);
            State = PatternState.Telegraph;
            _stateTimer = _difficulty.ScaleTelegraph(pattern.Telegraph);
            return pattern;
        }

        /// <summary>Interrupts the current pattern (phase change, stagger).</summary>
        public void Interrupt()
        {
            ActivePattern = null;
            State = PatternState.Idle;
            _stateTimer = 0f;
        }

        private PatternEventFlags UpdatePhase(float healthRatio)
        {
            int target = _boss.PhaseIndexForHealthRatio(MathLite.Clamp01(healthRatio));
            if (target == PhaseIndex) return PatternEventFlags.None;

            PhaseIndex = target;
            Interrupt();

            // Phase changes refresh a share of cooldowns so the new phase opens hot.
            List<string> keys = new List<string>(_cooldownRemaining.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                _cooldownRemaining[keys[i]] *= 0.25f;
            }
            return PatternEventFlags.PhaseChanged;
        }

        private bool IsReady(PatternDefinition pattern)
        {
            if (pattern == null) return false;
            return CooldownRemaining(pattern.Id) <= 0f;
        }

        private float CooldownOf(PatternDefinition pattern)
        {
            BossPhaseDefinition phase = CurrentPhase;
            float phaseMul = phase == null ? 1f : MathLite.Max(0.2f, phase.CooldownMultiplier);
            float enrageMul = _enraged ? _boss.EnrageCooldownMultiplier : 1f;
            return _difficulty.ScaleCooldown(pattern.Cooldown * phaseMul * enrageMul);
        }

        private static float DurationOf(PatternState state, PatternDefinition pattern)
        {
            switch (state)
            {
                case PatternState.Telegraph: return pattern == null ? 0.5f : pattern.Telegraph;
                case PatternState.Active: return pattern == null ? 0.3f : pattern.Duration;
                case PatternState.Recovery: return pattern == null ? 0.3f : pattern.Recovery;
                default: return 0f;
            }
        }
    }
}
