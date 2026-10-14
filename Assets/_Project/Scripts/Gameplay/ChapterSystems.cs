using System.Collections.Generic;
using IronVeil.Rules;
using UnityEngine;

namespace IronVeil
{
    /// <summary>
    /// Turns an authored encounter into a live spawn schedule: waves, elites, pacing
    /// and wave-clear gating all come from the rules layer's spawn planner.
    /// </summary>
    public sealed class SpawnDirector
    {
        private readonly ChapterRuntime _ctx;
        private readonly Rng _rng = new Rng(31415);

        private EncounterDefinition _encounter;
        private List<PlannedSpawn> _plan = new List<PlannedSpawn>();
        private int _nextSpawn;
        private int _currentWave;
        private float _clock;
        private float _waveAdvanceDelay;
        private bool _running;

        public SpawnDirector(ChapterRuntime context)
        {
            _ctx = context;
        }

        public EncounterDefinition Encounter => _encounter;
        public bool IsRunning => _running;

        /// <summary>True once every spawn has been placed; combat may still be alive.</summary>
        public bool IsFinished => !_running || _nextSpawn >= _plan.Count;

        public int Remaining => Mathf.Max(0, _plan.Count - _nextSpawn);

        public void Begin(EncounterDefinition encounter)
        {
            _encounter = encounter;
            _plan = SpawnRules.BuildSchedule(encounter, _ctx.Difficulty, _ctx.ChapterIndex, _rng);
            _nextSpawn = 0;
            _currentWave = 0;
            _clock = 0f;
            _waveAdvanceDelay = 0f;
            _running = true;

            if (_plan.Count == 0)
            {
                // Nothing authored to spawn: an empty schedule must not stall the chapter.
                _ctx.SummonAdds(_ctx.PlayerPosition + new Vector2(8f, 1f), 3, string.Empty);
                _running = false;
            }
        }

        public void Reset()
        {
            _plan.Clear();
            _nextSpawn = 0;
            _running = false;
            _encounter = null;
        }

        public void Tick(float deltaTime)
        {
            if (!_running) return;
            if (_plan.Count == 0)
            {
                _running = false;
                return;
            }

            _clock += deltaTime;

            if (!WaveCleared())
            {
                return;
            }

            if (_waveAdvanceDelay > 0f)
            {
                _waveAdvanceDelay -= deltaTime;
                return;
            }

            // Spawn everything whose scheduled time has arrived in the current wave.
            while (_nextSpawn < _plan.Count)
            {
                PlannedSpawn spawn = _plan[_nextSpawn];
                if (spawn.WaveIndex > _currentWave) break;
                if (spawn.Time > _clock) break;

                Spawn(spawn);
                _nextSpawn++;
            }

            if (_nextSpawn < _plan.Count && _plan[_nextSpawn].WaveIndex > _currentWave)
            {
                RewardWaveClear();
                _currentWave = _plan[_nextSpawn].WaveIndex;
                _clock = 0f;
                _waveAdvanceDelay = 0.8f;
            }
            else if (_nextSpawn >= _plan.Count && AliveCount() == 0)
            {
                _running = false;
            }
        }

        private void RewardWaveClear()
        {
            EncounterWave wave = WaveAt(_currentWave);
            if (wave != null && !string.IsNullOrEmpty(wave.Name))
            {
                _ctx.Text?.Callout(_ctx.PlayerPosition + new Vector2(0f, 2.4f), wave.Name + " clear", Palette.Success, 1.2f, 1.6f);
            }
        }

        private bool WaveCleared()
        {
            EncounterWave wave = WaveAt(_currentWave);
            if (wave == null) return true;
            if (!wave.RequiresWaveClear) return true;
            // Still spawning this wave? Keep going.
            for (int i = _nextSpawn; i < _plan.Count; i++)
            {
                if (_plan[i].WaveIndex == _currentWave) return false;
            }
            return AliveCount() == 0;
        }

        private EncounterWave WaveAt(int index)
        {
            if (_encounter == null || _encounter.Waves == null) return null;
            return index >= 0 && index < _encounter.Waves.Count ? _encounter.Waves[index] : null;
        }

        private int AliveCount() => _ctx.AliveEnemyCount();

        private void Spawn(PlannedSpawn planned)
        {
            float preferredX = _ctx.PlayerPosition.x;
            Vector2 position = _ctx.Level.PickSpawn(planned.SpawnPointTag, _rng, preferredX);
            _ctx.SpawnEnemy(planned.ArchetypeId, position, planned.IsElite);
            _ctx.Effects?.Dust(position, 4);

            if (planned.IsElite)
            {
                _ctx.Text?.Callout(position + new Vector2(0f, 1.6f), "ELITE", Palette.Accent, 1.1f, 1.4f);
            }
        }
    }

    /// <summary>
    /// Side-scrolling camera: follows the player with a small dead zone and a look
    /// ahead in the aim direction, locks to the arena, widens during boss fights and
    /// applies screen shake.
    /// </summary>
    public sealed class CameraRig
    {
        private readonly Camera _camera;
        private readonly ChapterRuntime _ctx;

        private Vector2 _position;
        private float _shakeTime;
        private bool _bossMode;
        private float _currentSize = 6.2f;
        private float _targetSize = 6.2f;

        public CameraRig(Camera camera, ChapterRuntime context)
        {
            _camera = camera;
            _ctx = context;
            if (_camera != null)
            {
                _camera.orthographic = true;
                _camera.orthographicSize = _currentSize;
            }
        }

        public void SnapTo(Vector2 target)
        {
            _position = target + new Vector2(0f, 1.4f);
            Apply(Vector2.zero);
        }

        public void SetBossMode(bool boss)
        {
            _bossMode = boss;
            _targetSize = boss ? 8.2f : 6.2f;
        }

        public void Tick(float deltaTime)
        {
            if (_camera == null) return;
            if (_ctx == null || !_ctx.IsLoaded) return;

            Vector2 player = _ctx.PlayerPosition;
            Vector2 desired = player + new Vector2(0f, 1.6f);

            if (_bossMode && _ctx.Boss != null)
            {
                Vector2 boss = _ctx.Boss.Center;
                desired = (player + boss) * 0.5f + new Vector2(0f, 1.8f);
            }

            float follow = 1f - Mathf.Exp(-6.5f * deltaTime);
            _position = Vector2.Lerp(_position, desired, follow);

            _currentSize = Mathf.Lerp(_currentSize, _targetSize, 1f - Mathf.Exp(-3f * deltaTime));

            float shake = _ctx.Feedback?.ShakeAmount ?? 0f;
            Vector2 offset = Vector2.zero;
            if (shake > 0.001f)
            {
                _shakeTime += deltaTime * 37f;
                float amount = shake * (_ctx.Settings != null && _ctx.Settings.Settings.ReducedFlashing ? 0.5f : 1f);
                offset = new Vector2(Mathf.Sin(_shakeTime * 1.7f), Mathf.Cos(_shakeTime * 2.3f)) * amount * 0.35f;
            }

            Apply(offset);
        }

        private void Apply(Vector2 shakeOffset)
        {
            if (_camera == null) return;

            _camera.orthographicSize = _currentSize;
            float halfHeight = _currentSize;
            float halfWidth = halfHeight * Mathf.Max(0.5f, _camera.aspect);

            Vector2 min = new Vector2(_ctx.Level.Bounds.xMin + halfWidth, _ctx.Level.Bounds.yMin + halfHeight);
            Vector2 max = new Vector2(_ctx.Level.Bounds.xMax - halfWidth, _ctx.Level.Bounds.yMax - halfHeight);
            if (min.x > max.x) min.x = max.x = (_ctx.Level.Bounds.xMin + _ctx.Level.Bounds.xMax) * 0.5f;
            if (min.y > max.y) min.y = max.y = (_ctx.Level.Bounds.yMin + _ctx.Level.Bounds.yMax) * 0.5f;

            Vector2 clamped = new Vector2(
                Mathf.Clamp(_position.x, min.x, max.x),
                Mathf.Clamp(_position.y, min.y, max.y));

            _camera.transform.position = new Vector3(clamped.x + shakeOffset.x, clamped.y + shakeOffset.y, -10f);
        }
    }
}
