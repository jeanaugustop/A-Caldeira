using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Enemies;
using UnityEngine;

namespace ACaldeira.World
{
    public sealed class WaveSpawner : MonoBehaviour, IEnemyLifecycleListener
    {
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private Transform player;

        private StageSO _stage;
        private float[] _spawnTimers;
        private int[] _aliveByWave;
        private float _elapsed;
        private bool _running;

        public void Begin(StageSO stage)
        {
            _stage = stage;
            int waveCount = stage != null && stage.Waves != null ? stage.Waves.Length : 0;
            _spawnTimers = new float[waveCount];
            _aliveByWave = new int[waveCount];
            _elapsed = 0f;
            _running = waveCount > 0;
        }

        public void Stop()
        {
            _running = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_running)
            {
                return;
            }

            _elapsed += deltaTime;
            WaveDefinition[] waves = _stage.Waves;

            for (int i = 0; i < waves.Length; i++)
            {
                WaveDefinition wave = waves[i];
                if (_elapsed < wave.StartTime || _elapsed >= wave.EndTime || _aliveByWave[i] >= wave.MaxAlive)
                {
                    continue;
                }

                _spawnTimers[i] += deltaTime;
                int budget = 32;
                while (_spawnTimers[i] >= wave.SpawnInterval && _aliveByWave[i] < wave.MaxAlive && budget-- > 0)
                {
                    _spawnTimers[i] -= Mathf.Max(0.001f, wave.SpawnInterval);
                    TrySpawn(wave, i);
                }
            }

            if (_elapsed >= _stage.DurationSeconds)
            {
                _running = false;
            }
        }

        public void OnEnemyReleased(int ownerWaveIndex)
        {
            if (_aliveByWave != null && ownerWaveIndex >= 0 && ownerWaveIndex < _aliveByWave.Length)
            {
                _aliveByWave[ownerWaveIndex] = Mathf.Max(0, _aliveByWave[ownerWaveIndex] - 1);
            }
        }

        private void TrySpawn(WaveDefinition wave, int waveIndex)
        {
            EnemySO definition = SelectEnemy(wave.Enemies);
            if (definition == null || definition.ActorPool == null)
            {
                return;
            }

            Rect area = _stage.SpawnArea;
            Vector2 point = (Vector2)player.position + Random.insideUnitCircle.normalized * Random.Range(11f, 16f);
            Vector3 position = new Vector3(Mathf.Clamp(point.x, area.xMin, area.xMax), Mathf.Clamp(point.y, area.yMin, area.yMax), 0f);

            if (poolManager.TrySpawn(definition.ActorPool, position, Quaternion.identity, out EnemyActor actor))
            {
                actor.Configure(definition, waveIndex, this);
                _aliveByWave[waveIndex]++;
            }
        }

        private static EnemySO SelectEnemy(EnemySpawnEntry[] entries)
        {
            if (entries == null || entries.Length == 0)
            {
                return null;
            }

            int totalWeight = 0;
            for (int i = 0; i < entries.Length; i++)
            {
                totalWeight += Mathf.Max(0, entries[i].Weight);
            }

            if (totalWeight == 0)
            {
                return null;
            }

            int roll = Random.Range(0, totalWeight);
            for (int i = 0; i < entries.Length; i++)
            {
                roll -= Mathf.Max(0, entries[i].Weight);
                if (roll < 0)
                {
                    return entries[i].Enemy;
                }
            }

            return entries[entries.Length - 1].Enemy;
        }
    }
}
