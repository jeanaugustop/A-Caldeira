using ACaldeira.Data;
using ACaldeira.Pooling;
using UnityEngine;

namespace ACaldeira.Enemies
{
    public interface IEnemyLifecycleListener
    {
        void OnEnemyReleased(int ownerWaveIndex);
    }

    public sealed class EnemyActor : PooledBehaviour
    {
        private EnemySO _definition;
        private IEnemyLifecycleListener _lifecycleListener;
        private int _ownerWaveIndex;
        private float _health;
        public Vector2 Position { get; set; }
        public float AttackTimer { get; set; }
        public uint Generation { get; private set; }

        public EnemySO Definition => _definition;
        public float Health => _health;

        public void Configure(EnemySO definition, int ownerWaveIndex, IEnemyLifecycleListener lifecycleListener)
        {
            _definition = definition;
            _ownerWaveIndex = ownerWaveIndex;
            _lifecycleListener = lifecycleListener;
            _health = definition != null ? definition.MaxHealth : 0f;
            Position = transform.position;
            AttackTimer = 0f;
            Generation++;
        }

        public bool Damage(float amount)
        {
            _health -= Mathf.Max(1f, amount - _definition.Armor);
            return _health <= 0f;
        }

        public override void OnDespawned()
        {
            _lifecycleListener?.OnEnemyReleased(_ownerWaveIndex);
            _definition = null;
            _lifecycleListener = null;
            _ownerWaveIndex = -1;
            _health = 0f;
        }
    }
}
