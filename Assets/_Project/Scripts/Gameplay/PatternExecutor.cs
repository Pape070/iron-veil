using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    /// <summary>
    /// What a pattern needs from whatever is casting it. Bosses and enemies both
    /// implement this, so all twenty attack patterns are written exactly once.
    /// </summary>
    public interface IPatternHost
    {
        Vector2 PatternOrigin { get; }

        /// <summary>Damage scale applied to the pattern's authored damage.</summary>
        float PatternDamageScale { get; }

        void FireProjectile(Vector2 position, Vector2 direction, float damage, float speed, float radius, bool homing, float life, string tag, int visualIndex);

        void SpawnExplosion(Vector2 position, float radius, float damage, string tag);

        void SpawnHazard(Vector2 position, float radius, float duration, float damage, string tag);

        void SpawnBeam(Vector2 from, Vector2 direction, float length, float width, float damage, float duration, string tag);

        void SummonAdds(int count, string archetypeId);

        void Relocate(Vector2 position);

        void Heal(float amount);

        void TelegraphMarker(Vector2 position, float radius, float seconds);
    }

    /// <summary>
    /// Turns an authored PatternDefinition into things happening in the world. The
    /// same code runs for a chapter boss, a mini-boss and an elite enemy, which is
    /// why an elite artillery unit reads as a miniature version of a real boss.
    /// </summary>
    public static class PatternExecutor
    {
        public static void Execute(PatternDefinition pattern, IPatternHost host, Vector2 targetPosition, Rng rng)
        {
            if (pattern == null || host == null) return;

            Rng random = rng ?? new Rng(12345);
            Vector2 origin = host.PatternOrigin;
            Vector2 toTarget = targetPosition - origin;
            Vector2 facing = toTarget.sqrMagnitude < 0.0004f ? Vector2.right : toTarget.normalized;
            float damage = Mathf.Max(1f, pattern.Damage * host.PatternDamageScale);
            float speed = Mathf.Max(1f, pattern.Speed);
            int count = Mathf.Max(1, pattern.ProjectileCount);
            float range = Mathf.Max(2f, pattern.Range);

            switch (pattern.Kind)
            {
                case AttackPatternKind.RadialBurst:
                    FireSpread(host, origin, facing, count, pattern.SpreadAngle, damage, speed, false, 1f, pattern.Id);
                    break;

                case AttackPatternKind.AimedVolley:
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 direction = CombatMath.WithSpread(facing, Mathf.Max(6f, pattern.SpreadAngle), random);
                        host.FireProjectile(origin + direction * 0.6f, direction, damage, speed, 0.2f, false, range / speed, pattern.Id, 1);
                    }
                    break;

                case AttackPatternKind.SweepingBeam:
                    SweepingBeam(host, origin, facing, pattern, damage, range, random);
                    break;

                case AttackPatternKind.ChargeDash:
                    host.TelegraphMarker(targetPosition, Mathf.Max(1.4f, pattern.Radius), 0.25f);
                    host.Relocate(targetPosition - facing * 1.2f);
                    host.SpawnExplosion(targetPosition, Mathf.Max(1.6f, pattern.Radius), damage, pattern.Id);
                    break;

                case AttackPatternKind.GroundPound:
                {
                    float radius = Mathf.Max(2.5f, pattern.Radius);
                    Vector2 pound = new Vector2(origin.x, origin.y - 0.8f);
                    host.SpawnExplosion(pound, radius, damage, pattern.Id);
                    // A ring of debris so the slam also covers the air above it.
                    float[] ring = { 30f, 60f, 120f, 150f };
                    for (int i = 0; i < ring.Length; i++)
                    {
                        Vector2 direction = CombatMath.FromAngle(ring[i]);
                        host.FireProjectile(pound, direction, damage * 0.6f, speed, 0.24f, false, 1.4f, pattern.Id, 5);
                    }
                    break;
                }

                case AttackPatternKind.MissileBarrage:
                {
                    int missiles = Mathf.Max(3, count);
                    for (int i = 0; i < missiles; i++)
                    {
                        float t = missiles == 1 ? 0.5f : i / (float)(missiles - 1);
                        Vector2 direction = CombatMath.FromAngle(Mathf.Lerp(60f, 120f, t));
                        Vector2 spawn = origin + new Vector2(0f, 1.6f);
                        host.FireProjectile(spawn, direction, damage, speed * 0.8f, 0.3f, true, 5f, pattern.Id, 3);
                    }
                    break;
                }

                case AttackPatternKind.SummonAdds:
                    host.SummonAdds(Mathf.Max(1, pattern.ProjectileCount / 2), "summon");
                    break;

                case AttackPatternKind.PhaseBlink:
                {
                    float distance = Mathf.Max(4f, pattern.Range * 0.6f);
                    bool leftSide = origin.x > targetPosition.x;
                    Vector2 destination = new Vector2(targetPosition.x + (leftSide ? distance : -distance), origin.y);
                    host.TelegraphMarker(destination, 1.6f, 0.3f);
                    host.Relocate(destination);
                    host.FireProjectile(destination, -facing, damage, speed, 0.24f, false, 1.6f, pattern.Id, 4);
                    break;
                }

                case AttackPatternKind.ArenaHazard:
                {
                    int hazards = Mathf.Clamp(count, 1, 4);
                    for (int i = 0; i < hazards; i++)
                    {
                        Vector2 position = targetPosition + new Vector2(random.Range(-6f, 6f), 0f);
                        host.TelegraphMarker(position, Mathf.Max(1.5f, pattern.Radius), 0.6f);
                        host.SpawnHazard(position, Mathf.Max(1.5f, pattern.Radius), Mathf.Max(2f, pattern.Duration * 4f), damage, pattern.Id);
                    }
                    break;
                }

                case AttackPatternKind.HomingSwarm:
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 spawn = origin + CombatMath.FromAngle(i * (360f / count)) * 1.2f;
                        Vector2 direction = CombatMath.WithSpread(facing, 70f, random);
                        host.FireProjectile(spawn, direction, damage * 0.7f, speed * 0.7f, 0.22f, true, 6f, pattern.Id, 2);
                    }
                    break;

                case AttackPatternKind.ShieldRecharge:
                    host.Heal(Mathf.Max(20f, pattern.Damage));
                    host.TelegraphMarker(origin, 2.4f, Mathf.Max(0.5f, pattern.Duration));
                    break;

                case AttackPatternKind.TurretDeploy:
                    host.SummonAdds(Mathf.Clamp(count, 1, 3), "turret");
                    break;

                case AttackPatternKind.ShockwaveTrap:
                    for (int i = 0; i < Mathf.Clamp(count, 1, 4); i++)
                    {
                        Vector2 position = targetPosition + new Vector2(random.Range(-4f, 4f), 0f);
                        host.TelegraphMarker(position, 1.5f, 0.5f);
                        host.SpawnHazard(position, 1.5f, 1.2f, damage, pattern.Id);
                    }
                    break;

                case AttackPatternKind.CloakAndStrike:
                    host.Relocate(targetPosition - facing * 1.5f);
                    host.SpawnExplosion(targetPosition, Mathf.Max(1.8f, pattern.Radius), damage * 1.2f, pattern.Id);
                    host.TelegraphMarker(targetPosition, 2.2f, 0.2f);
                    break;

                case AttackPatternKind.BeamCross:
                {
                    float length = range;
                    Vector2 centre = targetPosition;
                    host.SpawnBeam(centre, Vector2.right, length * 0.5f, 0.5f, damage, 0.35f, pattern.Id);
                    host.SpawnBeam(centre, Vector2.left, length * 0.5f, 0.5f, damage, 0.35f, pattern.Id);
                    host.SpawnBeam(centre, Vector2.up, length * 0.35f, 0.5f, damage, 0.35f, pattern.Id);
                    host.SpawnBeam(centre, Vector2.down, length * 0.35f, 0.5f, damage, 0.35f, pattern.Id);
                    break;
                }

                case AttackPatternKind.Minefield:
                {
                    int mines = Mathf.Clamp(count, 2, 8);
                    for (int i = 0; i < mines; i++)
                    {
                        Vector2 position = targetPosition + new Vector2((i - mines / 2f) * 2.4f, 0.4f);
                        host.SpawnHazard(position, 0.9f, 6f, damage, pattern.Id);
                    }
                    break;
                }

                case AttackPatternKind.GrappleSlam:
                    host.TelegraphMarker(targetPosition, 2.2f, 0.3f);
                    host.Relocate(targetPosition - facing * 0.8f);
                    host.SpawnExplosion(targetPosition, Mathf.Max(2f, pattern.Radius), damage * 1.1f, pattern.Id);
                    break;

                case AttackPatternKind.ReflectShield:
                    host.Heal(Mathf.Max(15f, pattern.Damage * 0.5f));
                    FireSpread(host, origin, facing, Mathf.Max(4, count / 2), 360f, damage * 0.8f, speed, false, 1f, pattern.Id);
                    break;

                case AttackPatternKind.LaserGrid:
                    for (int i = 0; i < Mathf.Clamp(count, 2, 6); i++)
                    {
                        Vector2 position = targetPosition + new Vector2(0f, -2f + i * 1.2f);
                        host.SpawnBeam(position, Vector2.right, range, 0.35f, damage * 0.8f, 0.4f, pattern.Id);
                    }
                    break;

                case AttackPatternKind.TailSweep:
                {
                    float arc = Mathf.Clamp(pattern.SpreadAngle <= 0f ? 140f : pattern.SpreadAngle, 90f, 200f);
                    FireSpread(host, origin, facing, Mathf.Max(5, count), arc, damage, speed * 1.1f, false, 1f, pattern.Id);
                    break;
                }

                default:
                    FireSpread(host, origin, facing, count, pattern.SpreadAngle, damage, speed, false, 1f, pattern.Id);
                    break;
            }
        }

        private static void FireSpread(IPatternHost host, Vector2 origin, Vector2 facing, int count, float spread, float damage, float speed, bool homing, float damageScale, string tag)
        {
            Vector2[] directions = CombatMath.Radial(facing, count, spread <= 0f ? 360f : spread);
            for (int i = 0; i < directions.Length; i++)
            {
                host.FireProjectile(origin + directions[i] * 0.7f, directions[i], damage * damageScale, speed, 0.22f, homing, 4f, tag, 4);
            }
        }

        private static void SweepingBeam(IPatternHost host, Vector2 origin, Vector2 facing, PatternDefinition pattern, float damage, float range, Rng rng)
        {
            float baseAngle = CombatMath.AngleOf(facing);
            float sweep = Mathf.Max(20f, pattern.SpreadAngle);
            int steps = Mathf.Clamp(pattern.ProjectileCount, 3, 10);
            for (int i = 0; i < steps; i++)
            {
                float t = steps == 1 ? 0.5f : i / (float)(steps - 1);
                Vector2 direction = CombatMath.FromAngle(baseAngle + Mathf.Lerp(-sweep * 0.5f, sweep * 0.5f, t));
                host.SpawnBeam(origin, direction, range, 0.45f, damage, 0.3f, pattern.Id);
            }
        }
    }
}
