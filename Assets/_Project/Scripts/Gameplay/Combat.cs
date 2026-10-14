using System;
using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    public enum Team
    {
        Player = 0,
        Enemy = 1,
        Neutral = 2
    }

    /// <summary>Anything that can be shot at. Implemented by the player, enemies and bosses.</summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        Team Team { get; }
        Vector2 Center { get; }
        float HealthRatio { get; }
        Aabb2 HitBox { get; }

        /// <summary>Damage from a player weapon or ability.</summary>
        DamageOutcome TakePlayerDamage(DamageRequest request, Vector2 hitPoint, bool isWeakPoint, string sourceTag);

        /// <summary>Damage from an enemy attack; shields absorb first.</summary>
        DamageOutcome TakeEnemyDamage(float attackDamage, Vector2 hitPoint, string sourceTag);
    }

    /// <summary>Hit and effect helpers shared by weapons, enemies and bosses.</summary>
    public static class CombatMath
    {
        /// <summary>Damage multiplier applied to enemy damage against the player, per difficulty.</summary>
        public static float EnemyDamageMultiplier(DifficultyProfile profile) => profile == null ? 1f : profile.EnemyDamageMultiplier;

        /// <summary>Damage multiplier applied to player damage against enemies, per difficulty.</summary>
        public static float PlayerDamageMultiplier(DifficultyProfile profile) => profile == null ? 1f : profile.PlayerDamageMultiplier;

        public static Vector2 FromAngle(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        public static float AngleOf(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        /// <summary>Evenly spaced angles around a circle, used by radial burst patterns.</summary>
        public static Vector2[] Radial(Vector2 facing, int count, float spreadDegrees, float offsetDegrees = 0f)
        {
            count = Mathf.Max(1, count);
            Vector2[] directions = new Vector2[count];
            if (count == 1)
            {
                directions[0] = facing.sqrMagnitude < 0.0001f ? Vector2.right : facing.normalized;
                return directions;
            }

            float sweep = Mathf.Abs(spreadDegrees) >= 359f ? 360f / count : spreadDegrees / Mathf.Max(1, count - 1);
            float baseAngle = AngleOf(facing.sqrMagnitude < 0.0001f ? Vector2.right : facing.normalized) + offsetDegrees;
            float start = Mathf.Abs(spreadDegrees) >= 359f ? 0f : -spreadDegrees * 0.5f;

            for (int i = 0; i < count; i++)
            {
                directions[i] = FromAngle(baseAngle + start + sweep * i);
            }
            return directions;
        }

        public static Vector2 WithSpread(Vector2 direction, float spreadDegrees, Rng rng)
        {
            if (spreadDegrees <= 0.01f) return direction;
            float offset = rng == null ? 0f : rng.Range(-spreadDegrees * 0.5f, spreadDegrees * 0.5f);
            return FromAngle(AngleOf(direction) + offset);
        }

        /// <summary>Finds every damageable overlapping a box, respecting the team filter.</summary>
        public static List<IDamageable> Overlap(Aabb2 box, IList<IDamageable> candidates, Team exclude)
        {
            List<IDamageable> hits = new List<IDamageable>();
            if (candidates == null) return hits;
            for (int i = 0; i < candidates.Count; i++)
            {
                IDamageable target = candidates[i];
                if (target == null || !target.IsAlive) continue;
                if (target.Team == exclude) continue;
                if (target.HitBox.Overlaps(box)) hits.Add(target);
            }
            return hits;
        }

        public static List<IDamageable> InRadius(Vector2 center, float radius, IList<IDamageable> candidates, Team exclude)
        {
            List<IDamageable> hits = new List<IDamageable>();
            if (candidates == null) return hits;
            for (int i = 0; i < candidates.Count; i++)
            {
                IDamageable target = candidates[i];
                if (target == null || !target.IsAlive) continue;
                if (target.Team == exclude) continue;
                if (Aabb2.CircleOverlaps(center, radius, target.HitBox)) hits.Add(target);
            }
            return hits;
        }
    }

    /// <summary>One live projectile. Pooled: no allocation while the game is running.</summary>
    public sealed class Projectile
    {
        public bool Active;
        public Team Owner = Team.Enemy;
        public Vector2 Position;
        public Vector2 Velocity;
        public DamageRequest Damage;
        public float Life;
        public float Radius = 0.18f;
        public float Gravity;
        public float HomingStrength;
        public IDamageable HomingTarget;
        public bool Pierce;
        public int PierceRemaining;
        public float ExplosionRadius;
        public string SourceTag = string.Empty;
        public int VisualIndex;
        public float SpinDegrees;
        public SpriteRenderer Renderer;
    }

    /// <summary>
    /// Owns every projectile in play. Bullets, enemy shots, boss patterns and
    /// explosives all go through here, so pooling, lifetime and hit rules stay in
    /// one place.
    /// </summary>
    public sealed class ProjectileSystem
    {
        private const int PoolSize = 320;

        private readonly List<Projectile> _pool = new List<Projectile>();
        private readonly Rng _rng = new Rng(424242);
        private Transform _root;
        private Sprite[] _visuals;
        private int _activeCount;

        public int ActiveCount => _activeCount;

        /// <summary>
        /// Raised whenever one of the player's shots connects. The runtime uses it for
        /// on-hit effects such as Vera's Momentum passive without the projectile
        /// system needing to know about the player.
        /// </summary>
        public Action<Vector2> PlayerHitCallback;

        public void Initialize(Transform parent)
        {
            _root = parent;

            GameObject root = new GameObject("Projectiles");
            root.transform.SetParent(parent, false);
            _root = root.transform;

            _visuals = new Sprite[]
            {
                PlaceholderArt.Rect("bullet_player", new Color(1f, 0.94f, 0.72f), 12, 4, 16f),
                PlaceholderArt.Rect("bullet_enemy", new Color(0.98f, 0.45f, 0.42f), 12, 6, 16f),
                PlaceholderArt.Circle("bullet_energy", new Color(0.62f, 0.86f, 1f), 10, 16f),
                PlaceholderArt.Rect("bullet_missile", new Color(0.95f, 0.78f, 0.36f), 18, 6, 16f),
                PlaceholderArt.Circle("bullet_boss", new Color(0.85f, 0.4f, 0.95f), 14, 16f),
                PlaceholderArt.Rect("bullet_heavy", new Color(1f, 0.6f, 0.25f), 22, 8, 16f)
            };

            for (int i = 0; i < PoolSize; i++)
            {
                GameObject go = new GameObject("projectile_" + i);
                go.transform.SetParent(_root, false);
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = _visuals[0];
                renderer.sortingOrder = 20;
                go.SetActive(false);

                _pool.Add(new Projectile { Renderer = renderer, Active = false });
            }
        }

        public Projectile Spawn(
            Vector2 position,
            Vector2 direction,
            DamageRequest damage,
            Team owner,
            float speed,
            float life = 3f,
            float radius = 0.18f,
            float gravity = 0f,
            bool homing = false,
            bool pierce = false,
            float explosionRadius = 0f,
            string sourceTag = null,
            int visualIndex = -1,
            float spin = 0f)
        {
            Projectile projectile = null;
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i].Active) continue;
                projectile = _pool[i];
                break;
            }
            if (projectile == null)
            {
                // Pool exhausted: recycle the oldest-looking slot rather than allocating.
                projectile = _pool[0];
            }

            if (!projectile.Active)
            {
                projectile.Active = true;
                projectile.Renderer.gameObject.SetActive(true);
                _activeCount++;
            }

            projectile.Owner = owner;
            projectile.Position = position;
            projectile.Velocity = direction.sqrMagnitude < 0.0001f ? Vector2.right * speed : direction.normalized * speed;
            projectile.Damage = damage;
            projectile.Life = life;
            projectile.Radius = radius;
            projectile.Gravity = gravity;
            projectile.HomingStrength = homing ? 3.5f : 0f;
            projectile.HomingTarget = null;
            projectile.Pierce = pierce;
            projectile.PierceRemaining = pierce ? 3 : 0;
            projectile.ExplosionRadius = explosionRadius;
            projectile.SourceTag = sourceTag ?? string.Empty;
            projectile.SpinDegrees = spin;

            int visual = visualIndex >= 0 ? visualIndex : owner == Team.Player ? 0 : 1;
            projectile.VisualIndex = Mathf.Clamp(visual, 0, _visuals.Length - 1);
            projectile.Renderer.sprite = _visuals[projectile.VisualIndex];
            projectile.Renderer.transform.position = new Vector3(projectile.Position.x, projectile.Position.y, 0f);
            projectile.Renderer.transform.localRotation = Quaternion.Euler(0f, 0f, CombatMath.AngleOf(projectile.Velocity));
            return projectile;
        }

        /// <summary>Advances every projectile and resolves hits against the target list.</summary>
        public void Tick(float deltaTime, LevelGeometry level, IList<IDamageable> targets, EffectSystem effects, bool enemyShotsAgainstPlayer = true)
        {
            if (_activeCount == 0 && effects == null) return;

            for (int i = 0; i < _pool.Count; i++)
            {
                Projectile projectile = _pool[i];
                if (!projectile.Active) continue;

                projectile.Life -= deltaTime;
                if (projectile.Life <= 0f)
                {
                    Retire(projectile, effects);
                    continue;
                }

                if (projectile.HomingStrength > 0f)
                {
                    IDamageable target = projectile.HomingTarget != null && projectile.HomingTarget.IsAlive
                        ? projectile.HomingTarget
                        : FindNearestTarget(projectile, targets);
                    projectile.HomingTarget = target;
                    if (target != null)
                    {
                        Vector2 desired = (target.Center - projectile.Position).normalized * projectile.Velocity.magnitude;
                        projectile.Velocity = Vector2.Lerp(projectile.Velocity, desired, Mathf.Clamp01(projectile.HomingStrength * deltaTime)).normalized * projectile.Velocity.magnitude;
                    }
                }

                if (Mathf.Abs(projectile.Gravity) > 0.001f) projectile.Velocity.y += projectile.Gravity * deltaTime;

                Vector2 previous = projectile.Position;
                projectile.Position += projectile.Velocity * deltaTime;

                if (level != null && (projectile.Position.x < level.Bounds.xMin || projectile.Position.x > level.Bounds.xMax))
                {
                    Retire(projectile, effects);
                    continue;
                }

                // Terrain: a shot into a solid platform dies on it.
                if (level != null && HitSolidLevel(level, previous, projectile.Position, projectile.Radius))
                {
                    Retire(projectile, effects);
                    continue;
                }

                projectile.Renderer.transform.position = new Vector3(projectile.Position.x, projectile.Position.y, 0f);
                if (Mathf.Abs(projectile.SpinDegrees) > 0.001f)
                {
                    projectile.Renderer.transform.Rotate(0f, 0f, projectile.SpinDegrees * deltaTime);
                }
                else
                {
                    projectile.Renderer.transform.localRotation = Quaternion.Euler(0f, 0f, CombatMath.AngleOf(projectile.Velocity));
                }

                if (targets == null) continue;

                Team hitTeam = projectile.Owner == Team.Player ? Team.Enemy : Team.Player;
                for (int t = 0; t < targets.Count; t++)
                {
                    IDamageable target = targets[t];
                    if (target == null || !target.IsAlive) continue;
                    if (target.Team != hitTeam) continue;
                    if (!Aabb2.CircleOverlaps(projectile.Position, projectile.Radius, target.HitBox)) continue;

                    ApplyHit(projectile, target, effects);

                    if (projectile.Pierce && projectile.PierceRemaining > 0)
                    {
                        projectile.PierceRemaining--;
                        continue;
                    }

                    if (!projectile.Active) break;
                    Retire(projectile, effects);
                    break;
                }
            }
        }

        private static bool HitSolidLevel(LevelGeometry level, Vector2 from, Vector2 to, float radius)
        {
            Aabb2 path = new Aabb2(Vector2.Min(from, to) - new Vector2(radius, radius), Vector2.Max(from, to) + new Vector2(radius, radius));
            for (int i = 0; i < level.Platforms.Count; i++)
            {
                Platform platform = level.Platforms[i];
                if (platform.OneWay) continue;
                Rect rect = platform.Rect;
                if (path.Overlaps(new Aabb2(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMax)))) return true;
            }
            return false;
        }

        private static IDamageable FindNearestTarget(Projectile projectile, IList<IDamageable> targets)
        {
            if (targets == null) return null;
            Team wanted = projectile.Owner == Team.Player ? Team.Enemy : Team.Player;
            IDamageable best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < targets.Count; i++)
            {
                IDamageable target = targets[i];
                if (target == null || !target.IsAlive || target.Team != wanted) continue;
                float distance = (target.Center - projectile.Position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
            }
            return best;
        }

        private void ApplyHit(Projectile projectile, IDamageable target, EffectSystem effects)
        {
            if (projectile.Owner == Team.Player)
            {
                DamageOutcome outcome = target.TakePlayerDamage(projectile.Damage, projectile.Position, false, projectile.SourceTag);
                if (outcome.AnyDamage)
                {
                    effects?.Hit(projectile.Position, outcome, true);
                    PlayerHitCallback?.Invoke(projectile.Position);
                }
            }
            else
            {
                DamageOutcome outcome = target.TakeEnemyDamage(projectile.Damage.BaseDamage, projectile.Position, projectile.SourceTag);
                if (outcome.AnyDamage) effects?.Hit(projectile.Position, outcome, false);
            }

            if (projectile.ExplosionRadius > 0f)
            {
                effects?.Explosion(projectile.Position, projectile.ExplosionRadius);
            }
        }

        private void Retire(Projectile projectile, EffectSystem effects)
        {
            if (projectile.ExplosionRadius > 0f) effects?.Explosion(projectile.Position, projectile.ExplosionRadius);
            projectile.Active = false;
            projectile.HomingTarget = null;
            projectile.Renderer.gameObject.SetActive(false);
            _activeCount = Mathf.Max(0, _activeCount - 1);
        }

        public void Clear()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                Projectile projectile = _pool[i];
                if (!projectile.Active) continue;
                projectile.Active = false;
                projectile.HomingTarget = null;
                if (projectile.Renderer != null) projectile.Renderer.gameObject.SetActive(false);
            }
            _activeCount = 0;
        }
    }

    /// <summary>
    /// Pooled visual effects: hit sparks, explosions, shockwaves and telegraph
    /// rings. Everything is tinted geometry, so the game reads clearly with no art.
    /// </summary>
    public sealed class EffectSystem
    {
        private const int PoolSize = 96;

        private sealed class Effect
        {
            public bool Active;
            public Vector2 Position;
            public Vector2 Velocity;
            public float Life;
            public float TotalLife;
            public float StartSize;
            public float EndSize;
            public Color StartColor;
            public Color EndColor;
            public SpriteRenderer Renderer;
            public int SortingOrder;
        }

        private readonly List<Effect> _pool = new List<Effect>();
        private Transform _root;
        private Sprite _spark;
        private Sprite _ring;
        private Sprite _square;

        public void Initialize(Transform parent)
        {
            GameObject root = new GameObject("Effects");
            root.transform.SetParent(parent, false);
            _root = root.transform;

            _spark = PlaceholderArt.Circle("fx_spark", Color.white, 16, 16f);
            _ring = PlaceholderArt.Circle("fx_ring", Color.white, 64, 16f);
            _square = PlaceholderArt.Rect("fx_square", Color.white, 8, 8, 16f);

            for (int i = 0; i < PoolSize; i++)
            {
                GameObject go = new GameObject("effect_" + i);
                go.transform.SetParent(_root, false);
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 30;
                go.SetActive(false);
                _pool.Add(new Effect { Renderer = renderer });
            }
        }

        private Effect Next()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (!_pool[i].Active) return _pool[i];
            }
            return _pool[0];
        }

        private Effect Emit(Vector2 position, Vector2 velocity, float life, float startSize, float endSize, Color start, Color end, Sprite sprite, int order)
        {
            Effect effect = Next();
            effect.Active = true;
            effect.Position = position;
            effect.Velocity = velocity;
            effect.Life = life;
            effect.TotalLife = Mathf.Max(0.01f, life);
            effect.StartSize = startSize;
            effect.EndSize = endSize;
            effect.StartColor = start;
            effect.EndColor = end;
            effect.SortingOrder = order;
            effect.Renderer.sprite = sprite;
            effect.Renderer.gameObject.SetActive(true);
            effect.Renderer.transform.position = new Vector3(position.x, position.y, 0f);
            effect.Renderer.transform.localScale = Vector3.one * startSize;
            effect.Renderer.color = start;
            return effect;
        }

        public void Hit(Vector2 position, DamageOutcome outcome, bool againstEnemy)
        {
            Color color = outcome.IsCrit
                ? new Color(1f, 0.82f, 0.28f)
                : againstEnemy ? new Color(1f, 0.88f, 0.62f) : new Color(1f, 0.42f, 0.38f);

            int count = outcome.IsCrit ? 7 : 4;
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * 360f + UnityEngine.Random.Range(-12f, 12f);
                Vector2 velocity = CombatMath.FromAngle(angle) * UnityEngine.Random.Range(2.5f, 6.5f);
                Emit(position, velocity, 0.26f, outcome.IsCrit ? 0.5f : 0.34f, 0.05f, color, new Color(color.r, color.g, color.b, 0f), _spark, 31);
            }
        }

        public void Explosion(Vector2 position, float radius)
        {
            Emit(position, Vector2.zero, 0.32f, radius * 0.6f, radius * 2.1f, new Color(1f, 0.72f, 0.32f, 0.9f), new Color(1f, 0.3f, 0.15f, 0f), _ring, 29);
            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = CombatMath.FromAngle(i * 36f) * UnityEngine.Random.Range(4f, 9f);
                Emit(position, velocity, 0.4f, 0.55f, 0.08f, new Color(1f, 0.85f, 0.5f), new Color(1f, 0.4f, 0.2f, 0f), _spark, 30);
            }
        }

        public void MuzzleFlash(Vector2 position, Vector2 direction, float scale = 1f)
        {
            Vector2 offset = position + direction.normalized * 0.5f;
            Emit(offset, direction.normalized * 2f, 0.08f, 0.6f * scale, 0.15f * scale, new Color(1f, 0.96f, 0.78f, 0.95f), new Color(1f, 0.7f, 0.3f, 0f), _spark, 32);
        }

        /// <summary>Warning circle drawn during a pattern's telegraph phase.</summary>
        public void Telegraph(Vector2 position, float radius, float duration, Color color)
        {
            Effect effect = Emit(position, Vector2.zero, duration, radius * 2f, radius * 2f, color, new Color(color.r, color.g, color.b, 0.15f), _ring, 12);
            effect.StartSize = radius * 1.4f;
            effect.EndSize = radius * 2f;
        }

        public void Shockwave(Vector2 position, float radius, float duration, Color color)
        {
            Emit(position, Vector2.zero, duration, 1f, radius * 2f, color, new Color(color.r, color.g, color.b, 0f), _ring, 28);
        }

        public void Dust(Vector2 position, int count = 6)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(0.5f, 2.5f));
                Emit(position, velocity, 0.5f, 0.4f, 0.05f, new Color(0.75f, 0.72f, 0.68f, 0.8f), new Color(0.6f, 0.58f, 0.55f, 0f), _square, 18);
            }
        }

        public void Beam(Vector2 from, Vector2 to, float width, float duration, Color color)
        {
            Vector2 delta = to - from;
            float length = delta.magnitude;
            if (length < 0.05f) return;
            Vector2 direction = delta / length;
            int steps = Mathf.Clamp(Mathf.RoundToInt(length), 1, 40);
            for (int i = 0; i <= steps; i++)
            {
                Vector2 point = from + direction * (length * i / steps);
                Emit(point, Vector2.zero, duration, width, width * 0.6f, color, new Color(color.r, color.g, color.b, 0f), _spark, 27);
            }
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                Effect effect = _pool[i];
                if (!effect.Active) continue;

                effect.Life -= deltaTime;
                if (effect.Life <= 0f)
                {
                    effect.Active = false;
                    effect.Renderer.gameObject.SetActive(false);
                    continue;
                }

                float t = 1f - Mathf.Clamp01(effect.Life / effect.TotalLife);
                effect.Position += effect.Velocity * deltaTime;
                effect.Velocity *= 1f - Mathf.Clamp01(deltaTime * 3f);
                effect.Renderer.transform.position = new Vector3(effect.Position.x, effect.Position.y, 0f);
                effect.Renderer.transform.localScale = Vector3.one * Mathf.Lerp(effect.StartSize, effect.EndSize, t);
                effect.Renderer.color = Color.Lerp(effect.StartColor, effect.EndColor, t);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                Effect effect = _pool[i];
                effect.Active = false;
                if (effect.Renderer != null) effect.Renderer.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>Floating combat text: damage numbers, crits, pickups and callouts.</summary>
    public sealed class FloatingTextSystem
    {
        private const int PoolSize = 48;

        private sealed class FloatingText
        {
            public TextMesh Mesh;
            public GameObject GameObject;
            public bool Active;
            public Vector2 Position;
            public Vector2 Velocity;
            public float Life;
            public float TotalLife;
            public Color StartColor;
            public float Scale;
        }

        private readonly List<FloatingText> _pool = new List<FloatingText>();
        private Transform _root;

        public void Initialize(Transform parent)
        {
            GameObject root = new GameObject("FloatingText");
            root.transform.SetParent(parent, false);
            _root = root.transform;

            for (int i = 0; i < PoolSize; i++)
            {
                GameObject go = new GameObject("text_" + i);
                go.transform.SetParent(_root, false);
                TextMesh mesh = go.AddComponent<TextMesh>();
                mesh.anchor = TextAnchor.MiddleCenter;
                mesh.alignment = TextAlignment.Center;
                mesh.characterSize = 0.1f;
                mesh.fontSize = 48;
                mesh.font = FontProvider.Get();
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = 40;
                    renderer.material = mesh.font != null ? mesh.font.material : renderer.material;
                }
                go.SetActive(false);
                _pool.Add(new FloatingText { Mesh = mesh, GameObject = go });
            }
        }

        public void Spawn(Vector2 position, string text, Color color, float scale = 1f, float life = 0.85f, Vector2? velocity = null)
        {
            FloatingText slot = null;
            for (int i = 0; i < _pool.Count; i++)
            {
                if (!_pool[i].Active)
                {
                    slot = _pool[i];
                    break;
                }
            }
            if (slot == null) return;

            slot.Active = true;
            slot.Position = position;
            slot.Velocity = velocity ?? new Vector2(UnityEngine.Random.Range(-0.6f, 0.6f), 2.6f);
            slot.Life = life;
            slot.TotalLife = Mathf.Max(0.01f, life);
            slot.StartColor = color;
            slot.Scale = scale;
            slot.Mesh.text = text;
            slot.Mesh.color = color;
            slot.GameObject.SetActive(true);
            slot.GameObject.transform.position = new Vector3(position.x, position.y, -0.5f);
            slot.GameObject.transform.localScale = Vector3.one * scale;
        }

        public void Damage(Vector2 position, DamageOutcome outcome, bool againstEnemy)
        {
            if (!outcome.AnyDamage) return;
            int amount = Mathf.Max(1, Mathf.RoundToInt(outcome.Total));
            bool crit = outcome.IsCrit;
            Color color = crit
                ? new Color(1f, 0.85f, 0.32f)
                : againstEnemy ? new Color(0.96f, 0.96f, 0.9f) : new Color(1f, 0.5f, 0.45f);
            Spawn(position + new Vector2(0f, 0.4f), crit ? amount + "!" : amount.ToString(), color, crit ? 1.35f : 1f, crit ? 1.05f : 0.8f);
        }

        public void Callout(Vector2 position, string text, Color color, float scale = 1.2f, float life = 1.3f)
        {
            Spawn(position + new Vector2(0f, 1.4f), text, color, scale, life, new Vector2(0f, 1.1f));
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                FloatingText text = _pool[i];
                if (!text.Active) continue;

                text.Life -= deltaTime;
                if (text.Life <= 0f)
                {
                    text.Active = false;
                    text.GameObject.SetActive(false);
                    continue;
                }

                float t = 1f - Mathf.Clamp01(text.Life / text.TotalLife);
                text.Velocity.y -= 3.4f * deltaTime;
                text.Position += text.Velocity * deltaTime;
                text.GameObject.transform.position = new Vector3(text.Position.x, text.Position.y, -0.5f);
                Color color = text.StartColor;
                color.a = Mathf.Clamp01(1f - t * t);
                text.Mesh.color = color;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                _pool[i].Active = false;
                _pool[i].GameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Screen shake and hit stop. Both are gated by the gameplay and accessibility
    /// settings, so a player who does not want shake or strobe never sees it.
    /// </summary>
    public sealed class Feedback
    {
        private readonly SettingsService _settings;
        private float _shakeAmount;
        private float _shakeDecay = 6f;
        private float _hitStopRemaining;

        public Feedback(SettingsService settings)
        {
            _settings = settings;
        }

        public float ShakeAmount => _shakeAmount;
        public bool IsHitStopped => _hitStopRemaining > 0f;

        public void Shake(float amount)
        {
            if (_settings != null)
            {
                if (!_settings.Settings.ScreenShake) return;
                amount *= _settings.Settings.ScreenShakeScale;
                if (_settings.Settings.ReducedFlashing) amount *= 0.5f;
            }
            _shakeAmount = Mathf.Max(_shakeAmount, amount);
        }

        public void HitStop(float seconds)
        {
            if (_settings != null && !_settings.Settings.HitStop) return;
            _hitStopRemaining = Mathf.Max(_hitStopRemaining, seconds);
        }

        public float Tick(float deltaTime)
        {
            float scale = 1f;
            if (_hitStopRemaining > 0f)
            {
                _hitStopRemaining -= deltaTime;
                scale = 0.08f;
            }

            _shakeAmount = Mathf.Max(0f, _shakeAmount - _shakeDecay * deltaTime * _shakeAmount - 0.05f * deltaTime);
            return scale;
        }

        public void Clear()
        {
            _shakeAmount = 0f;
            _hitStopRemaining = 0f;
        }
    }
}
