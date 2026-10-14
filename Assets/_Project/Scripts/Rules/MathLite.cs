using System;
using System.Collections.Generic;

namespace IronVeil.Rules
{
    /// <summary>
    /// Engine-independent math helpers. The rules assembly deliberately has no
    /// reference to UnityEngine (enforced by noEngineReferences in its asmdef),
    /// so it cannot use Mathf.
    /// </summary>
    public static class MathLite
    {
        public const float Epsilon = 1e-5f;

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Clamp01(float value) => Clamp(value, 0f, 1f);

        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);

        public static float InverseLerp(float a, float b, float value)
        {
            if (Math.Abs(b - a) < Epsilon) return 0f;
            return Clamp01((value - a) / (b - a));
        }

        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta) return target;
            return current + Math.Sign(target - current) * maxDelta;
        }

        public static float Round(float value) => (float)Math.Round(value, MidpointRounding.AwayFromZero);

        public static int RoundToInt(float value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

        public static float Abs(float value) => value < 0f ? -value : value;

        public static float Max(float a, float b) => a > b ? a : b;

        public static float Min(float a, float b) => a < b ? a : b;

        public static int Max(int a, int b) => a > b ? a : b;

        public static int Min(int a, int b) => a < b ? a : b;

        public static float Pow(float value, float power) => (float)Math.Pow(value, power);

        public static float Sqrt(float value) => value <= 0f ? 0f : (float)Math.Sqrt(value);

        public static float Repeat(float t, float length)
        {
            if (length <= 0f) return 0f;
            float result = t - (float)Math.Floor(t / length) * length;
            return result < 0f ? result + length : result;
        }

        public static bool NearlyEqual(float a, float b) => Abs(a - b) <= Epsilon;
    }

    /// <summary>
    /// Deterministic xorshift128 pseudo random generator. A reference type on
    /// purpose: passing a struct by value would silently clone the sequence and
    /// break reproducibility in tests and replays.
    /// </summary>
    public sealed class Rng
    {
        private uint _x, _y, _z, _w;

        public Rng(int seed)
        {
            // SplitMix32 expansion so that small/sequential seeds still produce
            // well distributed state.
            uint s = unchecked((uint)seed);
            _x = SplitMix32(ref s);
            _y = SplitMix32(ref s);
            _z = SplitMix32(ref s);
            _w = SplitMix32(ref s);
            if ((_x | _y | _z | _w) == 0) _x = 0x9E3779B9u;
        }

        private static uint SplitMix32(ref uint state)
        {
            unchecked
            {
                state += 0x9E3779B9u;
                uint z = state;
                z = (z ^ (z >> 16)) * 0x85EBCA6Bu;
                z = (z ^ (z >> 13)) * 0xC2B2AE35u;
                return z ^ (z >> 16);
            }
        }

        public uint NextUInt()
        {
            unchecked
            {
                uint t = _x ^ (_x << 11);
                _x = _y; _y = _z; _z = _w;
                _w = _w ^ (_w >> 19) ^ t ^ (t >> 8);
                return _w;
            }
        }

        /// <summary>Uniform float in [0, 1).</summary>
        public float NextFloat() => (NextUInt() >> 8) * (1f / 16777216f);

        /// <summary>Uniform float in [min, max).</summary>
        public float Range(float min, float max) => min + NextFloat() * (max - min);

        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            uint span = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % span);
        }

        public bool Chance(float probability)
        {
            if (probability <= 0f) return false;
            if (probability >= 1f) return true;
            return NextFloat() < probability;
        }

        /// <summary>Picks an index from a weight list. Returns -1 when all weights are non-positive.</summary>
        public int WeightedIndex(IReadOnlyList<float> weights)
        {
            if (weights == null || weights.Count == 0) return -1;
            float total = 0f;
            for (int i = 0; i < weights.Count; i++)
            {
                if (weights[i] > 0f) total += weights[i];
            }
            if (total <= 0f) return -1;

            float roll = NextFloat() * total;
            for (int i = 0; i < weights.Count; i++)
            {
                float w = weights[i];
                if (w <= 0f) continue;
                roll -= w;
                if (roll <= 0f) return i;
            }
            for (int i = weights.Count - 1; i >= 0; i--)
            {
                if (weights[i] > 0f) return i;
            }
            return -1;
        }

        /// <summary>Picks an element from a list (null-safe, returns default when empty).</summary>
        public T Pick<T>(IReadOnlyList<T> items)
        {
            if (items == null || items.Count == 0) return default;
            return items[Range(0, items.Count)];
        }
    }
}
