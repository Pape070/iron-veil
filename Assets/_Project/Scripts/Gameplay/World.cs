using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    /// <summary>Axis aligned box used for all collision and hit detection.</summary>
    public struct Aabb2
    {
        public Vector2 Min;
        public Vector2 Max;

        public Aabb2(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public static Aabb2 FromCenter(Vector2 center, Vector2 size)
        {
            Vector2 half = size * 0.5f;
            return new Aabb2(center - half, center + half);
        }

        public Vector2 Center => (Min + Max) * 0.5f;
        public Vector2 Size => Max - Min;
        public float Width => Max.x - Min.x;
        public float Height => Max.y - Min.y;

        public Aabb2 Moved(Vector2 delta) => new Aabb2(Min + delta, Max + delta);

        public bool Overlaps(Aabb2 other)
        {
            return Min.x < other.Max.x && Max.x > other.Min.x &&
                   Min.y < other.Max.y && Max.y > other.Min.y;
        }

        public bool Contains(Vector2 point)
        {
            return point.x >= Min.x && point.x <= Max.x && point.y >= Min.y && point.y <= Max.y;
        }

        /// <summary>Expanded box, used for melee and explosion hit tests.</summary>
        public Aabb2 Expanded(float amount)
        {
            return new Aabb2(Min - new Vector2(amount, amount), Max + new Vector2(amount, amount));
        }

        public static bool CircleOverlaps(Vector2 center, float radius, Aabb2 box)
        {
            float closestX = Mathf.Clamp(center.x, box.Min.x, box.Max.x);
            float closestY = Mathf.Clamp(center.y, box.Min.y, box.Max.y);
            float dx = center.x - closestX;
            float dy = center.y - closestY;
            return dx * dx + dy * dy <= radius * radius;
        }
    }

    public sealed class Platform
    {
        public Rect Rect;
        /// <summary>One-way platforms can be jumped through from below and dropped through.</summary>
        public bool OneWay;
        public bool Hazard;
    }

    /// <summary>
    /// The playable space for one arena. Geometry is generated deterministically from
    /// the chapter and arena data, so every chapter is playable and testable without
    /// hand-built level art, while still being identical on every run.
    /// </summary>
    public sealed class LevelGeometry
    {
        public Rect Bounds;
        public readonly List<Platform> Platforms = new List<Platform>();
        public readonly List<Rect> Hazards = new List<Rect>();
        private readonly Dictionary<string, List<Vector2>> _spawns = new Dictionary<string, List<Vector2>>();
        public float GroundY;
        public string ArenaId = string.Empty;
        public bool CameraLocked;
        public string PaletteId = "ruins_day";

        public static LevelGeometry Build(ArenaDefinition arena, int chapterIndex)
        {
            LevelGeometry level = new LevelGeometry
            {
                ArenaId = arena?.Id ?? string.Empty,
                CameraLocked = arena?.CameraLocked ?? false,
                PaletteId = arena?.BackdropPaletteId ?? "ruins_day"
            };

            float width = arena != null && arena.Width > 0f ? arena.Width * 2.2f : 96f;
            float height = arena != null && arena.Height > 0f ? arena.Height : 18f;
            level.Bounds = new Rect(0f, 0f, width, height);
            level.GroundY = 0f;

            // Deterministic per chapter+arena so retries and saves behave identically.
            Rng rng = new Rng(StableSeed(level.ArenaId, chapterIndex));
            bool boss = arena != null && arena.IsBossArena;

            float safeLeft = 14f;
            float safeRight = width - 14f;

            if (boss)
            {
                level.Platforms.Add(new Platform { Rect = new Rect(0f, -1f, width, 3f) });
                // A couple of side ledges to dodge onto; kept clear of the centre.
                level.Platforms.Add(new Platform { Rect = new Rect(6f, 4.5f, 8f, 0.8f), OneWay = true });
                level.Platforms.Add(new Platform { Rect = new Rect(width - 14f, 4.5f, 8f, 0.8f), OneWay = true });
                level.Platforms.Add(new Platform { Rect = new Rect(width * 0.5f - 5f, 7.5f, 10f, 0.8f), OneWay = true });
                AddSpawn(level, "ground", new Vector2(4f, 1f));
                AddSpawn(level, "air", new Vector2(10f, 5.5f));
                AddSpawn(level, "high", new Vector2(width * 0.5f, 8.6f));
                AddSpawn(level, "boss", new Vector2(width * 0.72f, 1f));
                return level;
            }

            // Main run of ground with pits carved out.
            float cursor = 0f;
            int segments = rng.Range(3, 6);
            float segmentLength = (width - 2f) / segments;
            for (int i = 0; i < segments; i++)
            {
                float start = cursor;
                float end = Mathf.Min(width, cursor + segmentLength);
                bool edge = i == 0 || i == segments - 1;
                float carve = edge ? 0f : rng.Range(3.5f, 6.5f);
                float carveStart = start + (end - start) * 0.45f;
                float carveEnd = Mathf.Min(end - 3f, carveStart + carve);

                float firstEnd = carve > 0f ? carveStart : end;
                level.Platforms.Add(new Platform { Rect = new Rect(start, -1f, firstEnd - start, 3f) });

                if (carve > 0f)
                {
                    level.Hazards.Add(new Rect(carveStart, -1f, carveEnd - carveStart, 1.2f));
                    level.Platforms.Add(new Platform { Rect = new Rect(carveEnd, -1f, end - carveEnd, 3f) });
                    AddSpawn(level, "ground", new Vector2(carveStart - 2f, 1f));
                }
                else
                {
                    AddSpawn(level, "ground", new Vector2(Mathf.Lerp(start, end, 0.5f), 1f));
                }

                cursor = end;
            }

            level.Platforms.Add(new Platform { Rect = new Rect(0f, -2f, width, 2f) });

            // Floating platforms, more of them the later the chapter.
            int platformCount = Mathf.Clamp(5 + chapterIndex / 2, 5, 12);
            for (int i = 0; i < platformCount; i++)
            {
                float x = rng.Range(safeLeft, safeRight);
                float y = rng.Range(2.8f, height * 0.62f);
                float w = rng.Range(4f, 9f);
                level.Platforms.Add(new Platform { Rect = new Rect(x, y, w, 0.8f), OneWay = true });
                AddSpawn(level, y > height * 0.4f ? "high" : "air", new Vector2(x + w * 0.5f, y + 1.2f));
            }

            AddSpawn(level, "ground", new Vector2(6f, 1f));
            AddSpawn(level, "ground", new Vector2(width - 6f, 1f));
            AddSpawn(level, "start", new Vector2(3.5f, 1f));

            return level;
        }

        private static void AddSpawn(LevelGeometry level, string tag, Vector2 position)
        {
            if (!level._spawns.TryGetValue(tag, out List<Vector2> list))
            {
                list = new List<Vector2>();
                level._spawns[tag] = list;
            }
            list.Add(position);
        }

        private static int StableSeed(string text, int extra)
        {
            unchecked
            {
                int hash = 17 + extra * 7919;
                if (!string.IsNullOrEmpty(text))
                {
                    for (int i = 0; i < text.Length; i++) hash = hash * 31 + text[i];
                }
                return hash;
            }
        }

        public bool PointInHazard(Vector2 point)
        {
            for (int i = 0; i < Hazards.Count; i++)
            {
                if (Hazards[i].Contains(point)) return true;
            }
            return false;
        }

        public Vector2 PickSpawn(string tag, Rng rng, float preferredX)
        {
            if (string.IsNullOrEmpty(tag)) tag = "ground";
            if (!_spawns.TryGetValue(tag, out List<Vector2> candidates) || candidates.Count == 0)
            {
                if (!_spawns.TryGetValue("ground", out candidates) || candidates.Count == 0)
                {
                    return new Vector2(preferredX, GroundY + 2f);
                }
            }

            // Prefer a spawn that is off screen of the current player position.
            Vector2 best = candidates[0];
            float bestScore = float.MinValue;
            int attempts = Mathf.Min(6, candidates.Count);
            for (int i = 0; i < attempts; i++)
            {
                Vector2 candidate = candidates[(rng ?? new Rng(7)).Range(0, candidates.Count)];
                float distance = Mathf.Abs(candidate.x - preferredX);
                float score = distance >= 9f ? distance : -distance;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }
            return best;
        }

        /// <summary>Ground height at a horizontal position, or the level floor as a fallback.</summary>
        public float GroundHeightAt(float x)
        {
            float best = float.NegativeInfinity;
            for (int i = 0; i < Platforms.Count; i++)
            {
                Rect rect = Platforms[i].Rect;
                if (x < rect.xMin || x > rect.xMax) continue;
                if (Platforms[i].Hazard) continue;
                if (rect.yMax > best) best = rect.yMax;
            }
            return best == float.NegativeInfinity ? GroundY : best;
        }
    }

    /// <summary>
    /// Kinematic movement with AABB collision against the level. Physics is done by
    /// hand so gameplay is deterministic and testable rather than at the mercy of
    /// solver settings.
    /// </summary>
    public sealed class CharacterBody
    {
        public Vector2 Position;
        public Vector2 Size = new Vector2(0.85f, 1.7f);
        public Vector2 Velocity;
        public bool Grounded;
        public bool HitCeiling;
        public bool HitWall;
        public bool Enabled = true;
        /// <summary>Set by the jump input while standing on a one-way platform.</summary>
        public bool DropThrough;

        public Aabb2 Bounds => Aabb2.FromCenter(Position, Size);
        public float HalfWidth => Size.x * 0.5f;
        public float HalfHeight => Size.y * 0.5f;

        public CharacterBody(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public const float Gravity = -46f;
        public const float MaxFallSpeed = -34f;

        public void Step(float deltaTime, LevelGeometry level, float gravityScale = 1f)
        {
            if (!Enabled) return;

            Velocity.y += Gravity * gravityScale * deltaTime;
            if (Velocity.y < MaxFallSpeed) Velocity.y = MaxFallSpeed;

            bool wasGrounded = Grounded;
            Grounded = false;
            HitCeiling = false;
            HitWall = false;

            // --- vertical ---
            float previousBottom = Position.y - HalfHeight;
            float previousTop = Position.y + HalfHeight;
            Vector2 verticalMove = new Vector2(0f, Velocity.y * deltaTime);
            Position += verticalMove;

            Aabb2 box = Bounds;
            for (int i = 0; i < level.Platforms.Count; i++)
            {
                Platform platform = level.Platforms[i];
                Rect rect = platform.Rect;
                if (!box.Overlaps(new Aabb2(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMax)))) continue;

                if (Velocity.y <= 0f)
                {
                    // Landing: only if we were above the surface before moving.
                    if (previousBottom >= rect.yMax - 0.25f)
                    {
                        if (platform.OneWay && DropThrough)
                        {
                            continue;
                        }
                        Position.y = rect.yMax + HalfHeight;
                        Velocity.y = 0f;
                        Grounded = true;
                        box = Bounds;
                    }
                }
                else if (previousTop <= rect.yMin + 0.05f && !platform.OneWay)
                {
                    Position.y = rect.yMin - HalfHeight;
                    Velocity.y = 0f;
                    HitCeiling = true;
                    box = Bounds;
                }
            }

            // --- horizontal ---
            Vector2 horizontalMove = new Vector2(Velocity.x * deltaTime, 0f);
            Position += horizontalMove;
            box = Bounds;

            for (int i = 0; i < level.Platforms.Count; i++)
            {
                Platform platform = level.Platforms[i];
                if (platform.OneWay) continue;
                Rect rect = platform.Rect;
                if (!box.Overlaps(new Aabb2(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMax)))) continue;

                if (Velocity.x > 0f)
                {
                    Position.x = rect.xMin - HalfWidth;
                }
                else if (Velocity.x < 0f)
                {
                    Position.x = rect.xMax + HalfWidth;
                }
                else
                {
                    // Pushed sideways by something else: nudge out on the nearest side.
                    float leftPush = rect.xMin - HalfWidth - Position.x;
                    float rightPush = rect.xMax + HalfWidth - Position.x;
                    Position.x += Mathf.Abs(leftPush) < Mathf.Abs(rightPush) ? leftPush : rightPush;
                }
                Velocity.x = 0f;
                HitWall = true;
                box = Bounds;
            }

            // Never leave the arena, and definitely never fall out of the world.
            Position.x = Mathf.Clamp(Position.x, level.Bounds.xMin + HalfWidth, level.Bounds.xMax - HalfWidth);
            if (Position.y < level.Bounds.yMin - 4f)
            {
                Position.y = level.Bounds.yMin - 4f;
                Velocity.y = 0f;
            }

            if (!wasGrounded && Grounded) OnLanded?.Invoke();
            DropThrough = false;
        }

        public System.Action OnLanded;

        /// <summary>Moves the actor and resolves collisions, for scripted dashes.</summary>
        public void Teleport(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.zero;
        }

        public void Stop()
        {
            Velocity = Vector2.zero;
        }
    }
}
