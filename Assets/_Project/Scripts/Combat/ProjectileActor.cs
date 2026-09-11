using ACaldeira.Data;
using ACaldeira.Pooling;
using UnityEngine;

namespace ACaldeira.Combat
{
    public sealed class ProjectileActor : PooledBehaviour
    {
        private WeaponSO _definition;
        private Vector2 _direction;
        private readonly EnemyActorHit[] hits = new EnemyActorHit[16];
        private int hitCount;
        private int maxHits;
        private struct EnemyActorHit { internal int Index; internal uint Generation; }
        public Vector2 Position { get; set; }
        public float Remaining { get; set; }
        public float Damage { get; set; }
        public float Speed { get; set; }

        public WeaponSO Definition => _definition;
        public Vector2 Direction => _direction;

        public void Configure(WeaponSO definition, Vector2 direction)
        {
            _definition = definition;
            _direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
            Position = transform.position;
            Remaining = definition.Duration;
            Damage = definition.Damage;
            Speed = definition.ProjectileSpeed;
            hitCount = 0; maxHits = Mathf.Min(16, definition.Pierce + 1);
        }

        public void SetPierce(int pierce) => maxHits = Mathf.Min(16, Mathf.Max(1, pierce + 1));

        public bool HasHit(int index, uint generation)
        {
            for (int i = 0; i < hitCount; i++)
                if (hits[i].Index == index && hits[i].Generation == generation) return true;
            return false;
        }
        public bool RegisterHit(int index, uint generation)
        {
            hits[hitCount++] = new EnemyActorHit { Index = index, Generation = generation };
            return hitCount >= maxHits;
        }

        public override void OnDespawned()
        {
            _definition = null;
            _direction = Vector2.zero;
        }
    }
}
