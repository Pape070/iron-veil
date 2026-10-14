using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    /// <summary>
    /// A chapter boss or mini-boss. All scheduling (phase transitions, cooldowns,
    /// anti-repeat, telegraphs, enrage) comes from the tested rules-layer
    /// BossPatternRunner; this class is the body that performs the chosen pattern.
    /// </summary>
    public sealed class BossActor : IDamageable, IPatternHost
    {
        private readonly ChapterRuntime _ctx;
        private readonly BossDefinition _definition;
        private readonly DifficultyProfile _difficulty;
        private readonly BossPatternRunner _runner;
        private readonly Rng _rng;
        private readonly Rng _patternRng;

        private CharacterBody _body;
        private GameObject _root;
        private SpriteRenderer _renderer;
        private SpriteRenderer _weakPoint;
        private SpriteRenderer _healthBar;
        private SpriteRenderer _healthBarBack;
        private SpriteRenderer _telegraph;
        private GameObject _healthBarRoot;

        private float _health;
        private float _maxHealth;
        private float _incomingMultiplier = 1f;
        private float _hitFlashTimer;
        private float _stunTimer;
        private float _facing = -1f;
        private float _bob;
        private float _elapsed;
        private bool _dead;
        private bool _introPlayed;
        private int _phaseAnnounced = -1;

        public BossActor(ChapterRuntime context, BossDefinition definition, Vector2 spawn)
        {
            _ctx = context;
            _definition = definition;
            _difficulty = context.Difficulty;
            _rng = new Rng(StableSeed(definition.Id));
            _patternRng = new Rng(StableSeed(definition.Id + "_patterns"));
            _runner = new BossPatternRunner(definition, _difficulty, _patternRng);

            float chapterScale = ChapterRules.EnemyHealthMultiplier(definition.ChapterIndex);
            _maxHealth = definition.BaseHealth * chapterScale * _difficulty.EnemyHealthMultiplier;
            _health = _maxHealth;

            BuildVisuals(spawn);
        }

        private static int StableSeed(string text)
        {
            unchecked
            {
                int hash = 17;
                if (!string.IsNullOrEmpty(text))
                {
                    for (int i = 0; i < text.Length; i++) hash = hash * 31 + text[i];
                }
                return hash;
            }
        }

        public BossDefinition Definition => _definition;
        public BossPatternRunner Runner => _runner;
        public bool IsMiniBoss => _definition.IsMiniBoss;
        public float ElapsedSeconds => _elapsed;

        // ------------------------------------------------- IDamageable

        public bool IsAlive => !_dead && _health > 0f;
        public Team Team => Team.Enemy;
        public Vector2 Center => _body.Position;
        public float HealthRatio => _maxHealth <= 0f ? 0f : Mathf.Clamp01(_health / _maxHealth);
        public Aabb2 HitBox => _body.Bounds;

        // ------------------------------------------------- setup

        private void BuildVisuals(Vector2 spawn)
        {
            _root = new GameObject("Boss_" + _definition.Id);
            _root.transform.SetParent(_ctx.ActorRoot, false);

            float scale = _definition.IsMiniBoss ? 1.35f : 1f + Mathf.Clamp(_definition.BaseHealth / 6000f, 0.6f, 1.6f);
            Vector2 size = _definition.IsMiniBoss ? new Vector2(1.5f, 2.4f) : new Vector2(2.1f, 3.4f);
            _body = new CharacterBody(spawn, size);

            Color primary = _definition.IsFinalBoss
                ? new Color(0.55f, 0.35f, 0.72f)
                : _definition.IsMiniBoss ? new Color(0.62f, 0.42f, 0.38f) : new Color(0.70f, 0.32f, 0.32f);
            Color accent = _definition.IsFinalBoss ? new Color(0.90f, 0.86f, 1f) : new Color(1f, 0.78f, 0.4f);

            GameObject body = new GameObject("Body");
            body.transform.SetParent(_root.transform, false);
            _renderer = body.AddComponent<SpriteRenderer>();
            _renderer.sprite = PlaceholderArt.Actor("boss_" + _definition.Id, primary, accent, 34, 56);
            _renderer.sortingOrder = 13;
            body.transform.localScale = Vector3.one * scale;

            GameObject weak = new GameObject("WeakPoint");
            weak.transform.SetParent(_root.transform, false);
            _weakPoint = weak.AddComponent<SpriteRenderer>();
            _weakPoint.sprite = PlaceholderArt.Circle("fx_weakpoint", new Color(1f, 0.55f, 0.45f, 0.9f), 20, 16f);
            _weakPoint.sortingOrder = 16;
            weak.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            _weakPoint.enabled = _definition.WeakPointCount > 0;

            GameObject telegraph = new GameObject("Telegraph");
            telegraph.transform.SetParent(_ctx.ActorRoot, false);
            _telegraph = telegraph.AddComponent<SpriteRenderer>();
            _telegraph.sprite = PlaceholderArt.Circle("fx_boss_telegraph", new Color(1f, 0.45f, 0.35f, 0.4f), 64, 16f);
            _telegraph.sortingOrder = 9;
            _telegraph.enabled = false;

            _healthBarRoot = new GameObject("BossHealthBar");
            _healthBarRoot.transform.SetParent(_ctx.ActorRoot, false);

            GameObject back = new GameObject("Back");
            back.transform.SetParent(_healthBarRoot.transform, false);
            _healthBarBack = back.AddComponent<SpriteRenderer>();
            _healthBarBack.sprite = PlaceholderArt.Rect("fx_boss_bar_back", new Color(0.05f, 0.05f, 0.08f, 0.85f), 4, 4, 16f);
            _healthBarBack.sortingOrder = 26;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(_healthBarRoot.transform, false);
            _healthBar = fill.AddComponent<SpriteRenderer>();
            _healthBar.sprite = PlaceholderArt.Rect("fx_boss_bar_fill", new Color(0.90f, 0.28f, 0.30f), 4, 4, 16f);
            _healthBar.sortingOrder = 27;

            _ctx.RegisterDamageable(this);
            _ctx.Saves?.RegisterBossAttempt(_definition.Id);
        }

        public void Dispose()
        {
            _ctx?.UnregisterDamageable(this);
            if (_root != null) Object.Destroy(_root);
            if (_healthBarRoot != null) Object.Destroy(_healthBarRoot);
            if (_telegraph != null) Object.Destroy(_telegraph.gameObject);
        }

        public void ApplyStun(float seconds)
        {
            // Bosses resist stuns: short, and never during a phase transition.
            _stunTimer = Mathf.Max(_stunTimer, seconds * 0.25f * _difficulty.PlayerStaggerMultiplier);
        }

        // ------------------------------------------------- tick

        public void Tick(float deltaTime)
        {
            if (_dead) return;

            float dt = deltaTime;
            if (_ctx.Settings != null && _ctx.Settings.Settings.SlowDownBosses) dt *= 0.75f;

            _elapsed += dt;
            _hitFlashTimer = Mathf.Max(0f, _hitFlashTimer - dt);
            _bob += dt * 3.2f;

            if (!_introPlayed)
            {
                _introPlayed = true;
                _ctx.OnBossIntro(this);
            }

            if (_stunTimer > 0f)
            {
                _stunTimer -= dt;
                _body.Velocity.x *= 0.9f;
                _body.Step(dt, _ctx.Level);
                SyncVisuals();
                return;
            }

            PatternEventFlags events = _runner.Tick(dt, HealthRatio);
            HandleEvents(events);

            if (_runner.State == PatternState.Idle)
            {
                _runner.StartNextPattern();
                if (_runner.State == PatternState.Telegraph) ShowTelegraph();
            }

            Move(dt);
            _body.Step(dt, _ctx.Level);
            SyncVisuals();
        }

        private void HandleEvents(PatternEventFlags events)
        {
            if ((events & PatternEventFlags.PhaseChanged) != 0)
            {
                BossPhaseDefinition phase = _runner.CurrentPhase;
                if (phase != null && _phaseAnnounced != _runner.PhaseIndex)
                {
                    _phaseAnnounced = _runner.PhaseIndex;
                    _incomingMultiplier = Mathf.Max(0.1f, phase.IncomingDamageMultiplier);
                    if (!string.IsNullOrEmpty(phase.Announcement))
                    {
                        _ctx.Text?.Callout(_body.Position + new Vector2(0f, 2f), phase.Announcement, Palette.Accent, 1.3f, 2f);
                    }
                    if (phase.SpawnsAdds && phase.AddsPerWave > 0)
                    {
                        SummonAdds(phase.AddsPerWave, "summon");
                    }
                    _ctx.Feedback?.Shake(0.7f);
                    _ctx.Effects?.Shockwave(_body.Position, 5f, 0.5f, new Color(1f, 0.5f, 0.4f, 0.8f));
                }
            }

            if ((events & PatternEventFlags.Enraged) != 0)
            {
                _ctx.Text?.Callout(_body.Position + new Vector2(0f, 2f), "ENRAGED", Palette.Danger, 1.5f, 2.2f);
                _ctx.Feedback?.Shake(0.9f);
            }

            if ((events & PatternEventFlags.PatternFired) != 0)
            {
                PatternDefinition pattern = _runner.ActivePattern;
                if (pattern != null)
                {
                    PatternExecutor.Execute(pattern, this, _ctx.PlayerPosition, _rng);
                    _ctx.Audio?.PlaySfx("sfx_boss_pattern", 1f);
                    _ctx.Feedback?.Shake(0.25f + Mathf.Clamp01(pattern.Damage / 60f) * 0.4f);
                    if (_telegraph != null) _telegraph.enabled = false;
                }
            }

            if ((events & PatternEventFlags.PatternEnded) != 0 && _telegraph != null)
            {
                _telegraph.enabled = false;
            }
        }

        private void ShowTelegraph()
        {
            if (_telegraph == null) return;
            PatternDefinition pattern = _runner.ActivePattern;
            _telegraph.enabled = true;

            bool overPlayer = pattern != null &&
                              (pattern.Kind == AttackPatternKind.ArenaHazard ||
                               pattern.Kind == AttackPatternKind.GroundPound ||
                               pattern.Kind == AttackPatternKind.ShockwaveTrap);

            Vector2 position = overPlayer ? _ctx.PlayerPosition : _body.Position;
            float radius = pattern == null ? 3f : Mathf.Clamp(Mathf.Max(pattern.Radius, pattern.Range * 0.25f), 1.5f, 9f);
            _telegraph.transform.position = new Vector3(position.x, position.y, 0f);
            _telegraph.transform.localScale = Vector3.one * radius;
        }

        private void Move(float deltaTime)
        {
            BossPhaseDefinition phase = _runner.CurrentPhase;
            float phaseSpeed = phase == null ? 1f : Mathf.Max(0.2f, phase.SpeedMultiplier);
            float speed = _definition.BaseSpeed * phaseSpeed * _difficulty.EnemySpeedMultiplier;
            if (_runner.IsEnraged) speed *= 1.25f;

            Vector2 playerPosition = _ctx.PlayerPosition;
            Vector2 toPlayer = playerPosition - _body.Position;
            float distance = toPlayer.magnitude;
            Vector2 direction = distance < 0.01f ? Vector2.right : toPlayer / distance;
            _facing = Mathf.Sign(direction.x) == 0f ? _facing : Mathf.Sign(direction.x);

            PatternDefinition active = _runner.ActivePattern;
            bool charging = active != null &&
                            (active.Kind == AttackPatternKind.ChargeDash ||
                             active.Kind == AttackPatternKind.CloakAndStrike ||
                             active.Kind == AttackPatternKind.GrappleSlam) &&
                            _runner.State == PatternState.Active;

            float desired;
            if (charging)
            {
                desired = Mathf.Sign(direction.x) * speed * 4.5f;
            }
            else if (_runner.State == PatternState.Telegraph && active != null && active.Kind == AttackPatternKind.ChargeDash)
            {
                desired = 0f;
            }
            else
            {
                float preferred = _definition.IsMiniBoss ? 4.5f : 6.5f;
                if (distance < preferred * 0.7f) desired = -Mathf.Sign(direction.x) * speed;
                else if (distance > preferred * 1.35f) desired = Mathf.Sign(direction.x) * speed;
                else desired = Mathf.Sign(direction.x) * speed * 0.35f;
            }

            _body.Velocity.x = Mathf.Lerp(_body.Velocity.x, desired, deltaTime * 4f);

            // Bosses hop onto ledges rather than getting stuck under them.
            if (_body.Grounded && (_body.HitWall || _body.Velocity.x * direction.x < 0f && distance < 3f))
            {
                _body.Velocity.y = 15f;
            }
        }

        private void SyncVisuals()
        {
            float bobOffset = Mathf.Sin(_bob) * 0.12f;
            _root.transform.position = new Vector3(_body.Position.x, _body.Position.y + bobOffset, 0f);
            _renderer.flipX = _facing > 0f;
            _renderer.color = _hitFlashTimer > 0f
                ? new Color(1f, 0.7f, 0.7f)
                : _runner.IsEnraged ? new Color(1f, 0.85f, 0.8f) : Color.white;

            if (_weakPoint != null && _weakPoint.enabled)
            {
                bool exposed = _runner.State == PatternState.Recovery ||
                               (_runner.ActivePattern != null && _runner.ActivePattern.IsWeakPointExposed);
                _weakPoint.color = exposed ? new Color(1f, 0.45f, 0.35f, 0.95f) : new Color(1f, 0.6f, 0.5f, 0.45f);
                _weakPoint.transform.localPosition = new Vector3(_facing * 0.3f, 0.6f, 0f);
            }

            if (_telegraph != null && _telegraph.enabled)
            {
                PatternDefinition pattern = _runner.ActivePattern;
                float radius = pattern == null ? 3f : Mathf.Clamp(Mathf.Max(pattern.Radius, pattern.Range * 0.25f), 1.5f, 9f);
                float t = _runner.State == PatternState.Telegraph ? Mathf.Clamp01(_runner.StateProgress) : 1f;
                _telegraph.color = new Color(1f, 0.45f, 0.35f, 0.12f + 0.3f * t);
                _telegraph.transform.localScale = Vector3.one * radius * (0.85f + 0.3f * t);
            }

            float barWidth = 16f;
            Vector3 barPosition = new Vector3(_body.Position.x, _body.Position.y + _body.HalfHeight + 1.2f, 0f);
            _healthBarRoot.transform.position = barPosition;
            _healthBarBack.transform.localScale = new Vector3(barWidth, 0.45f, 1f);
            float ratio = Mathf.Clamp01(_health / _maxHealth);
            _healthBar.transform.localScale = new Vector3(barWidth * ratio, 0.3f, 1f);
            _healthBar.transform.localPosition = new Vector3(-barWidth * 0.5f * (1f - ratio), 0f, 0f);
        }

        // ------------------------------------------------- damage

        public DamageOutcome TakePlayerDamage(DamageRequest request, Vector2 hitPoint, bool isWeakPoint, string sourceTag)
        {
            DamageOutcome outcome = new DamageOutcome();
            if (_dead || request.BaseDamage <= 0f) return outcome;

            bool weak = isWeakPoint || IsWeakPointHit(hitPoint);
            float defense = _definition.BaseDefense * (1f + _definition.ChapterIndex * 0.06f);
            if (weak) defense *= 0.45f;

            float multiplier = CombatMath.PlayerDamageMultiplier(_ctx.Difficulty) * Mathf.Max(0.1f, _incomingMultiplier);
            outcome = DamageRules.ComputeOutgoing(request, _ctx.Rng, defense, multiplier, _health);
            if (!outcome.AnyDamage) return outcome;

            if (weak && !outcome.IsCrit)
            {
                outcome.IsCrit = true;
                outcome.Total *= Mathf.Max(1f, request.CritDamageMultiplier);
            }

            _health = DamageRules.ApplyToHealth(_health, outcome.Total);
            _hitFlashTimer = 0.1f;
            _ctx.Text?.Damage(_body.Position + new Vector2(0f, 1.2f), outcome, true);
            _ctx.Feedback?.Shake(0.12f + Mathf.Clamp01(outcome.Total / _maxHealth * 20f) * 0.2f);

            if (_health <= 0f) Die();
            return outcome;
        }

        private bool IsWeakPointHit(Vector2 hitPoint)
        {
            if (_definition.WeakPointCount <= 0) return false;
            Vector2 weakPoint = _body.Position + new Vector2(_facing * 0.3f, 0.6f);
            return (hitPoint - weakPoint).sqrMagnitude <= 0.55f * 0.55f;
        }

        public DamageOutcome TakeEnemyDamage(float attackDamage, Vector2 hitPoint, string sourceTag)
        {
            return new DamageOutcome();
        }

        private void Die()
        {
            if (_dead) return;
            _dead = true;
            _health = 0f;
            _ctx.Effects?.Explosion(_body.Position, _definition.IsFinalBoss ? 6f : 4f);
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = CombatMath.FromAngle(i * 60f) * Random.Range(1f, 3f);
                _ctx.Effects?.Explosion(_body.Position + offset, 2f);
            }
            _ctx.Feedback?.Shake(1.2f);
            _ctx.Audio?.PlaySfx("sfx_boss_death");
            _ctx.OnBossKilled(this, _elapsed);
        }

        // ------------------------------------------------- IPatternHost

        public Vector2 PatternOrigin => _body.Position + new Vector2(0f, 0.4f);

        public float PatternDamageScale
        {
            get
            {
                BossPhaseDefinition phase = _runner.CurrentPhase;
                float phaseMultiplier = phase == null ? 1f : Mathf.Max(0.2f, phase.DamageMultiplier);
                return _definition.BaseDamage * phaseMultiplier * _difficulty.EnemyDamageMultiplier;
            }
        }

        public void FireProjectile(Vector2 position, Vector2 direction, float damage, float speed, float radius, bool homing, float life, string tag, int visualIndex)
        {
            DamageRequest request = new DamageRequest
            {
                BaseDamage = damage,
                Power = DamageRules.ReferencePower,
                PowerScaling = 0f,
                CanCrit = false
            };
            _ctx.Projectiles.Spawn(position, direction, request, Team.Enemy, speed, life, radius, 0f, homing, false, 0f, tag, visualIndex);
        }

        public void SpawnExplosion(Vector2 position, float radius, float damage, string tag)
        {
            _ctx.SpawnEnemyExplosion(position, radius, damage, tag);
        }

        public void SpawnHazard(Vector2 position, float radius, float duration, float damage, string tag)
        {
            _ctx.SpawnHazard(position, radius, duration, damage, tag);
        }

        public void SpawnBeam(Vector2 from, Vector2 direction, float length, float width, float damage, float duration, string tag)
        {
            _ctx.SpawnBeam(from, direction, length, width, damage, duration, tag, Team.Enemy);
        }

        public void SummonAdds(int count, string archetypeId)
        {
            _ctx.SummonAdds(_body.Position, count, archetypeId);
        }

        public void Relocate(Vector2 position)
        {
            _ctx.Effects?.Dust(_body.Position, 10);
            Vector2 clamped = position;
            clamped.x = Mathf.Clamp(clamped.x, _ctx.Level.Bounds.xMin + 2f, _ctx.Level.Bounds.xMax - 2f);
            clamped.y = Mathf.Max(clamped.y, _ctx.Level.GroundHeightAt(clamped.x) + _body.HalfHeight);
            _body.Teleport(clamped);
            _ctx.Effects?.Dust(clamped, 10);
        }

        public void Heal(float amount)
        {
            if (_dead) amount = 0f;
            _health = Mathf.Min(_maxHealth, _health + Mathf.Max(0f, amount));
            _ctx.Text?.Callout(_body.Position + new Vector2(0f, 1.6f), "+" + Mathf.RoundToInt(amount), Palette.AccentCool, 1.1f, 1f);
        }

        public void TelegraphMarker(Vector2 position, float radius, float seconds)
        {
            _ctx.Effects?.Telegraph(position, radius, seconds, new Color(1f, 0.45f, 0.35f, 0.5f));
        }
    }
}
