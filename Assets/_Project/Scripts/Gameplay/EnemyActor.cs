using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    /// <summary>
    /// One enemy. Behaviour comes from its archetype: class, tier, attack patterns and
    /// stats, all scaled per chapter and difficulty by the rules layer.
    /// </summary>
    public sealed class EnemyActor : IDamageable, IPatternHost
    {
        private readonly ChapterRuntime _ctx;
        private readonly EnemyArchetype _archetype;
        private readonly int _chapterIndex;
        private readonly Rng _rng;

        private CharacterBody _body;
        private GameObject _root;
        private SpriteRenderer _renderer;
        private SpriteRenderer _healthBar;
        private SpriteRenderer _healthBarBack;
        private SpriteRenderer _telegraph;

        private float _health;
        private float _maxHealth;
        private float _damage;
        private float _speed;
        private float _attackCooldown;
        private float _telegraphDuration;
        private float _attackRange;
        private float _detectionRange;
        private float _contactCooldown;
        private float _attackTimer;
        private float _telegraphTimer;
        private float _stunTimer;
        private float _hitFlashTimer;
        private float _spawnGrace;
        private int _patternIndex;
        private float _facing = 1f;
        private float _hoverPhase;
        private bool _dead;
        private PatternDefinition _pendingPattern;

        public EnemyActor(ChapterRuntime context, EnemyArchetype archetype, Vector2 spawn, int chapterIndex, bool elite, Rng rng)
        {
            _ctx = context;
            _archetype = archetype;
            _chapterIndex = chapterIndex;
            IsElite = elite;
            _rng = rng ?? new Rng(1234);

            DifficultyProfile difficulty = context.Difficulty;
            _maxHealth = EnemyScaling.Health(archetype, chapterIndex, difficulty);
            if (elite)
            {
                _maxHealth *= EnemyScaling.EliteHealthMultiplier;
            }
            _damage = EnemyScaling.Damage(archetype, chapterIndex, difficulty) * (elite ? EnemyScaling.EliteDamageMultiplier : 1f);
            _speed = EnemyScaling.Speed(archetype, difficulty) * (elite ? EnemyScaling.EliteSpeedMultiplier : 1f);
            _attackCooldown = EnemyScaling.AttackCooldown(archetype, difficulty);
            _telegraphDuration = EnemyScaling.Telegraph(archetype, difficulty);
            _attackRange = archetype.AttackRange;
            _detectionRange = archetype.DetectionRange;
            _health = _maxHealth;
            _attackTimer = Mathf.Max(0.3f, archetype.AttackCooldown * 0.4f);
            _spawnGrace = 0.25f;

            BuildVisuals(spawn);
        }

        public bool IsElite { get; }
        public string ArchetypeId => _archetype.Id;
        public float DifficultyScaling => 1f;

        // ------------------------------------------------- IDamageable

        public bool IsAlive => !_dead && _health > 0f;
        public Team Team => Team.Enemy;
        public Vector2 Center => _body.Position;
        public float HealthRatio => _maxHealth <= 0f ? 0f : Mathf.Clamp01(_health / _maxHealth);
        public Aabb2 HitBox => _body.Bounds;
        public CharacterBody Body => _body;
        public EnemyArchetype Archetype => _archetype;

        // ------------------------------------------------- visuals

        private void BuildVisuals(Vector2 spawn)
        {
            _root = new GameObject("Enemy_" + _archetype.Id);
            _root.transform.SetParent(_ctx.ActorRoot, false);

            Color body = ColorForClass(_archetype.Class);
            Color accent = IsElite ? new Color(1f, 0.82f, 0.35f) : new Color(1f, 0.55f, 0.45f);
            float scale = Mathf.Clamp(0.7f + _archetype.Tier * 0.09f, 0.7f, 1.5f);

            _renderer = _root.AddComponent<SpriteRenderer>();
            _renderer.sprite = PlaceholderArt.Actor("enemy_" + _archetype.Id, body, accent, 24, 36);
            _renderer.sortingOrder = 12;
            _root.transform.localScale = Vector3.one * scale;

            float height = _archetype.CanFly ? 1.5f : 1.6f;
            float width = Mathf.Clamp(0.7f + _archetype.Tier * 0.05f, 0.7f, 1.4f);
            _body = new CharacterBody(spawn, new Vector2(width, height));

            GameObject barBack = new GameObject("HealthBarBack");
            barBack.transform.SetParent(_ctx.ActorRoot, false);
            _healthBarBack = barBack.AddComponent<SpriteRenderer>();
            _healthBarBack.sprite = PlaceholderArt.Rect("fx_bar_back", new Color(0.05f, 0.05f, 0.07f, 0.75f), 4, 4, 16f);
            _healthBarBack.sortingOrder = 24;

            GameObject bar = new GameObject("HealthBar");
            bar.transform.SetParent(_ctx.ActorRoot, false);
            _healthBar = bar.AddComponent<SpriteRenderer>();
            _healthBar.sprite = PlaceholderArt.Rect("fx_bar_fill", new Color(0.98f, 0.42f, 0.38f), 4, 4, 16f);
            _healthBar.sortingOrder = 25;

            GameObject telegraph = new GameObject("Telegraph");
            telegraph.transform.SetParent(_ctx.ActorRoot, false);
            _telegraph = telegraph.AddComponent<SpriteRenderer>();
            _telegraph.sprite = PlaceholderArt.Circle("fx_enemy_telegraph", new Color(1f, 0.6f, 0.3f, 0.35f), 40, 16f);
            _telegraph.sortingOrder = 8;
            _telegraph.enabled = false;

            _ctx.RegisterDamageable(this);
        }

        private static Color ColorForClass(EnemyClass enemyClass)
        {
            switch (enemyClass)
            {
                case EnemyClass.Grunt: return new Color(0.62f, 0.40f, 0.44f);
                case EnemyClass.Rusher: return new Color(0.80f, 0.42f, 0.30f);
                case EnemyClass.Turret: return new Color(0.42f, 0.46f, 0.50f);
                case EnemyClass.Flyer: return new Color(0.48f, 0.42f, 0.66f);
                case EnemyClass.Sniper: return new Color(0.36f, 0.52f, 0.48f);
                case EnemyClass.Brute: return new Color(0.58f, 0.34f, 0.36f);
                case EnemyClass.Shielder: return new Color(0.40f, 0.50f, 0.66f);
                case EnemyClass.Drone: return new Color(0.50f, 0.50f, 0.58f);
                case EnemyClass.Spitter: return new Color(0.56f, 0.56f, 0.32f);
                case EnemyClass.Bombardier: return new Color(0.66f, 0.48f, 0.28f);
                case EnemyClass.Assassin: return new Color(0.38f, 0.32f, 0.46f);
                case EnemyClass.Medic: return new Color(0.36f, 0.62f, 0.50f);
                case EnemyClass.Swarm: return new Color(0.72f, 0.56f, 0.36f);
                case EnemyClass.Artillery: return new Color(0.48f, 0.44f, 0.36f);
                case EnemyClass.Beast: return new Color(0.54f, 0.40f, 0.30f);
                default: return new Color(0.6f, 0.4f, 0.4f);
            }
        }

        public void Dispose()
        {
            _ctx?.UnregisterDamageable(this);
            if (_root != null) Object.Destroy(_root);
            if (_healthBar != null) Object.Destroy(_healthBar.gameObject);
            if (_healthBarBack != null) Object.Destroy(_healthBarBack.gameObject);
            if (_telegraph != null) Object.Destroy(_telegraph.gameObject);
        }

        public void ApplyStun(float seconds)
        {
            _stunTimer = Mathf.Max(_stunTimer, seconds);
        }

        // ------------------------------------------------- tick

        public void Tick(float deltaTime)
        {
            if (_dead) return;

            _spawnGrace = Mathf.Max(0f, _spawnGrace - deltaTime);
            _hitFlashTimer = Mathf.Max(0f, _hitFlashTimer - deltaTime);
            _contactCooldown = Mathf.Max(0f, _contactCooldown - deltaTime);

            if (_stunTimer > 0f)
            {
                _stunTimer -= deltaTime;
                _body.Velocity.x *= 0.85f;
                _body.Step(deltaTime, _ctx.Level, _archetype.CanFly ? 0f : 1f);
                SyncVisuals();
                return;
            }

            Vector2 playerPosition = _ctx.PlayerPosition;
            Vector2 toPlayer = playerPosition - _body.Position;
            float distance = toPlayer.magnitude;
            Vector2 direction = distance < 0.01f ? Vector2.right : toPlayer / distance;

            bool aware = distance <= _detectionRange && _spawnGrace <= 0f;

            if (_archetype.CanFly)
            {
                _hoverPhase += deltaTime * 2.2f;
                float hoverY = Mathf.Sin(_hoverPhase) * 0.6f;
                if (aware)
                {
                    Vector2 desired = direction * _speed * (distance > _attackRange * 0.7f ? 1f : -0.35f);
                    desired.y = Mathf.Clamp(toPlayer.y * 0.6f, -2.4f, 2.4f) + hoverY;
                    _body.Velocity.x = Mathf.Lerp(_body.Velocity.x, desired.x, deltaTime * 4f);
                    _body.Velocity.y = Mathf.Lerp(_body.Velocity.y, desired.y * 2f, deltaTime * 3f);
                }
                else
                {
                    _body.Velocity.x = Mathf.Sin(_hoverPhase * 0.4f) * _speed * 0.4f;
                    _body.Velocity.y = hoverY;
                }
            }
            else
            {
                float desiredSpeed = 0f;
                switch (_archetype.Class)
                {
                    case EnemyClass.Turret:
                    case EnemyClass.Artillery:
                        desiredSpeed = 0f;
                        break;
                    case EnemyClass.Sniper:
                        desiredSpeed = distance < _attackRange * 0.7f ? -_speed : (distance > _attackRange ? _speed : 0f);
                        break;
                    case EnemyClass.Rusher:
                    case EnemyClass.Swarm:
                    case EnemyClass.Beast:
                    case EnemyClass.Assassin:
                        desiredSpeed = aware ? Mathf.Sign(direction.x) * _speed : 0f;
                        break;
                    case EnemyClass.Brute:
                        desiredSpeed = aware && distance < _attackRange * 1.2f ? Mathf.Sign(direction.x) * _speed * 0.8f : 0f;
                        break;
                    case EnemyClass.Medic:
                        desiredSpeed = aware ? -Mathf.Sign(direction.x) * _speed * 0.6f : 0f;
                        break;
                    default:
                        desiredSpeed = aware ? Mathf.Sign(direction.x) * _speed * (distance > _attackRange * 0.8f ? 1f : 0.15f) : 0f;
                        break;
                }

                _body.Velocity.x = Mathf.Lerp(_body.Velocity.x, desiredSpeed, deltaTime * 6f);

                // Hop over small obstacles when stuck against a wall.
                if (_body.HitWall && _body.Grounded) _body.Velocity.y = 12f;
            }

            if (aware && Mathf.Abs(direction.x) > 0.05f) _facing = Mathf.Sign(direction.x);

            _body.Step(deltaTime, _ctx.Level, _archetype.CanFly ? 0f : 1f);

            if (_archetype.Class == EnemyClass.Medic) HealNearbyAllies(deltaTime);
            if (aware) UpdateAttacks(deltaTime, direction, distance);
            HandleContactDamage(distance);

            SyncVisuals();
        }

        private void HealNearbyAllies(float deltaTime)
        {
            List<EnemyActor> allies = _ctx.Enemies;
            for (int i = 0; i < allies.Count; i++)
            {
                EnemyActor ally = allies[i];
                if (ally == null || ally == this || !ally.IsAlive) continue;
                if ((ally.Center - _body.Position).sqrMagnitude > 36f) continue;
                ally.ReceiveHealing(_maxHealth * 0.06f * deltaTime);
            }
        }

        public void ReceiveHealing(float amount)
        {
            if (_dead || amount <= 0f) return;
            _health = Mathf.Min(_maxHealth, _health + amount);
        }

        private void HandleContactDamage(float distance)
        {
            if (_contactCooldown > 0f) return;
            bool melee = _archetype.Class == EnemyClass.Rusher ||
                         _archetype.Class == EnemyClass.Swarm ||
                         _archetype.Class == EnemyClass.Beast ||
                         _archetype.Class == EnemyClass.Brute;

            float reach = _body.HalfWidth + _ctx.PlayerRadius + 0.35f;
            if (!melee || distance > reach) return;

            _contactCooldown = Mathf.Max(0.5f, _attackCooldown * 0.8f);
            DamageOutcome outcome = _ctx.DamagePlayer(_damage, _body.Position, _archetype.Id);
            if (outcome.AnyDamage)
            {
                Vector2 away = (_ctx.PlayerPosition - _body.Position).normalized;
                _ctx.PushPlayer(away * 6f);
            }
        }

        private void UpdateAttacks(float deltaTime, Vector2 direction, float distance)
        {
            if (_archetype.Class == EnemyClass.Turret || _archetype.Class == EnemyClass.Artillery)
            {
                // Stationary units still need line of sight distance checks.
                if (distance > _detectionRange) return;
            }

            if (_telegraphTimer > 0f)
            {
                _telegraphTimer -= deltaTime;
                if (_telegraphTimer <= 0f)
                {
                    Vector2 target = _ctx.PlayerPosition;
                    if (_pendingPattern != null)
                    {
                        PatternExecutor.Execute(_pendingPattern, this, target, _rng);
                    }
                    else
                    {
                        ExecuteDefaultAttack(direction);
                    }
                    _pendingPattern = null;
                    _attackTimer = _attackCooldown;
                    if (_telegraph != null) _telegraph.enabled = false;
                }
                return;
            }

            _attackTimer -= deltaTime;
            if (_attackTimer > 0f) return;
            if (!InRange(distance)) return;

            _pendingPattern = NextPattern();
            _telegraphTimer = _telegraphDuration;
            _ctx.Effects?.Telegraph(_body.Position, 1.4f, _telegraphDuration, new Color(1f, 0.55f, 0.3f, 0.5f));
            if (_telegraph != null)
            {
                _telegraph.enabled = true;
                _telegraph.transform.position = new Vector3(_body.Position.x, _body.Position.y, 0f);
                _telegraph.transform.localScale = Vector3.one * Mathf.Clamp(_attackRange * 0.25f, 0.6f, 2.4f);
            }
        }

        private bool InRange(float distance)
        {
            if (_archetype.Class == EnemyClass.Turret || _archetype.Class == EnemyClass.Artillery) return distance <= _detectionRange;
            if (_archetype.Class == EnemyClass.Rusher || _archetype.Class == EnemyClass.Swarm) return distance <= 1.8f;
            if (_archetype.Class == EnemyClass.Assassin) return distance <= 7f;
            return distance <= _attackRange;
        }

        private PatternDefinition NextPattern()
        {
            if (_archetype.Attacks == null || _archetype.Attacks.Count == 0) return null;
            PatternDefinition pattern = _archetype.Attacks[_patternIndex % _archetype.Attacks.Count];
            _patternIndex++;
            return pattern;
        }

        private void ExecuteDefaultAttack(Vector2 direction)
        {
            int count = Mathf.Max(1, _archetype.ProjectileCount);
            float speed = Mathf.Max(3f, _archetype.ProjectileSpeed);
            bool melee = _archetype.Class == EnemyClass.Rusher ||
                         _archetype.Class == EnemyClass.Swarm ||
                         _archetype.Class == EnemyClass.Beast;

            if (melee)
            {
                _ctx.DamagePlayer(_damage, _body.Position, _archetype.Id);
                _ctx.Effects?.Shockwave(_body.Position, 1.6f, 0.2f, new Color(1f, 0.6f, 0.35f, 0.7f));
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Vector2 spreadDirection = CombatMath.WithSpread(direction, count > 1 ? 16f : 3f, _rng);
                FireProjectile(_body.Position + spreadDirection * 0.5f, spreadDirection, _damage, speed, 0.18f, false, 2.4f, _archetype.Id, 1);
            }
            _ctx.Effects?.MuzzleFlash(_body.Position, direction, 0.7f);
            _ctx.Audio?.PlaySfx("sfx_enemy_shoot", Random.Range(0.9f, 1.1f));
        }

        private void SyncVisuals()
        {
            _root.transform.position = new Vector3(_body.Position.x, _body.Position.y, 0f);
            _renderer.flipX = _facing < 0f;
            _renderer.color = _hitFlashTimer > 0f ? new Color(1f, 0.6f, 0.6f) : Color.white;

            bool showBar = _health < _maxHealth - 0.01f && (_ctx.Settings == null || _ctx.Settings.Settings.ShowEnemyHealthBars);
            float barWidth = Mathf.Clamp(_body.Size.x * 1.1f, 0.6f, 1.6f);
            Vector2 barPosition = _body.Position + new Vector2(0f, _body.HalfHeight + 0.35f);

            _healthBar.enabled = showBar;
            _healthBarBack.enabled = showBar;
            if (showBar)
            {
                _healthBarBack.transform.position = new Vector3(barPosition.x, barPosition.y, 0f);
                _healthBarBack.transform.localScale = new Vector3(barWidth, 0.16f, 1f);
                float ratio = Mathf.Clamp01(_health / _maxHealth);
                _healthBar.transform.position = new Vector3(barPosition.x - barWidth * 0.5f * (1f - ratio), barPosition.y, 0f);
                _healthBar.transform.localScale = new Vector3(barWidth * ratio, 0.11f, 1f);
                _healthBar.color = IsElite ? new Color(1f, 0.78f, 0.32f) : new Color(0.98f, 0.42f, 0.38f);
            }

            if (_telegraph != null && _telegraph.enabled)
            {
                _telegraph.transform.position = new Vector3(_body.Position.x, _body.Position.y, 0f);
            }
        }

        // ------------------------------------------------- damage

        public DamageOutcome TakePlayerDamage(DamageRequest request, Vector2 hitPoint, bool isWeakPoint, string sourceTag)
        {
            DamageOutcome outcome = new DamageOutcome();
            if (_dead || request.BaseDamage <= 0f) return outcome;

            float defense = _archetype.BaseDefense * (1f + _chapterIndex * 0.12f);
            if (_archetype.Class == EnemyClass.Shielder && Mathf.Sign(hitPoint.x - _body.Position.x) == _facing) defense *= 2.2f;
            if (isWeakPoint) defense *= 0.5f;

            float multiplier = CombatMath.PlayerDamageMultiplier(_ctx.Difficulty);
            outcome = DamageRules.ComputeOutgoing(request, _ctx.Rng, defense, multiplier, _health);
            if (!outcome.AnyDamage) return outcome;

            _health = DamageRules.ApplyToHealth(_health, outcome.Total);
            _hitFlashTimer = 0.12f;
            _ctx.Text?.Damage(_body.Position + new Vector2(0f, _body.HalfHeight * 0.5f), outcome, true);

            if (_archetype.CanBeStaggered && _archetype.Class != EnemyClass.Brute && outcome.Total > _maxHealth * 0.2f)
            {
                _body.Velocity.x += Mathf.Sign(hitPoint.x - _body.Position.x) * 1.2f;
            }

            if (_health <= 0f) Die(sourceTag);
            return outcome;
        }

        public DamageOutcome TakeEnemyDamage(float attackDamage, Vector2 hitPoint, string sourceTag)
        {
            // Enemies do not damage each other.
            return new DamageOutcome();
        }

        private void Die(string sourceTag)
        {
            if (_dead) return;
            _dead = true;
            _health = 0f;
            _ctx.Effects?.Explosion(_body.Position, IsElite ? 2.2f : 1.3f);
            _ctx.Audio?.PlaySfx("sfx_enemy_death", IsElite ? 0.8f : 1.2f);
            _ctx.OnEnemyKilled(this);
        }

        // ------------------------------------------------- IPatternHost

        public Vector2 PatternOrigin => _body.Position;
        public float PatternDamageScale => 1f;

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
            _ctx.Effects?.Dust(_body.Position, 6);
            _body.Teleport(position);
            _ctx.Effects?.Dust(position, 6);
        }

        public void Heal(float amount)
        {
            ReceiveHealing(amount);
        }

        public void TelegraphMarker(Vector2 position, float radius, float seconds)
        {
            _ctx.Effects?.Telegraph(position, radius, seconds, new Color(1f, 0.5f, 0.3f, 0.5f));
        }
    }
}
