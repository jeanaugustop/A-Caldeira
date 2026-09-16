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
        private float _slowTimer;
        private float _slowMultiplier = 1f;
        private float _weakenTimer;
        private float _damageMultiplier = 1f;
        private ACaldeira.UI.DirectionalActor _visual;
        private void Awake() { _visual=GetComponent<ACaldeira.UI.DirectionalActor>(); }
        public void TickVisual(Vector2 movement,float dt) { if(_visual!=null)_visual.TickVisual(movement,dt); }
        public Vector2 Position { get; set; }
        public float AttackTimer { get; set; }
        public uint Generation { get; private set; }

        public EnemySO Definition => _definition;
        public float Health => _health;
        public float EffectiveMoveSpeed => _definition == null ? 0f : _definition.MoveSpeed * _slowMultiplier;
        public float ContactDamageMultiplier => _damageMultiplier;

        public void Configure(EnemySO definition, int ownerWaveIndex, IEnemyLifecycleListener lifecycleListener)
        {
            _definition = definition;
            _ownerWaveIndex = ownerWaveIndex;
            _lifecycleListener = lifecycleListener;
            _health = definition != null ? definition.MaxHealth : 0f;
            _slowTimer = 0f;
            _slowMultiplier = 1f;
            _weakenTimer = 0f;
            _damageMultiplier = 1f;
            Position = transform.position;
            AttackTimer = 0f;
            Generation++;
        }

        public bool Damage(float amount)
        {
            _health -= Mathf.Max(1f, amount - _definition.Armor);
            return _health <= 0f;
        }

        public void ApplySlow(float fraction, float duration)
        {
            if (fraction <= 0f || duration <= 0f) return;
            _slowMultiplier = Mathf.Min(_slowMultiplier, Mathf.Clamp01(1f - fraction));
            _slowTimer = Mathf.Max(_slowTimer, duration);
        }

        public void ApplyDamageReduction(float fraction, float duration)
        {
            if (fraction <= 0f || duration <= 0f) return;
            _damageMultiplier = Mathf.Min(_damageMultiplier, Mathf.Clamp01(1f - fraction));
            _weakenTimer = Mathf.Max(_weakenTimer, duration);
        }

        public void TickStatus(float deltaTime)
        {
            if (_slowTimer > 0f)
            {
                _slowTimer -= deltaTime;
                if (_slowTimer <= 0f) _slowMultiplier = 1f;
            }
            if (_weakenTimer > 0f)
            {
                _weakenTimer -= deltaTime;
                if (_weakenTimer <= 0f) _damageMultiplier = 1f;
            }
        }

        public override void OnDespawned()
        {
            _lifecycleListener?.OnEnemyReleased(_ownerWaveIndex);
            _definition = null;
            _lifecycleListener = null;
            _ownerWaveIndex = -1;
            _health = 0f;
            _slowTimer = 0f;
            _slowMultiplier = 1f;
            _weakenTimer = 0f;
            _damageMultiplier = 1f;
        }
    }
}
