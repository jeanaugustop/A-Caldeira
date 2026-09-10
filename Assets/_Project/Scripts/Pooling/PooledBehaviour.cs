using ACaldeira.Data;
using UnityEngine;

namespace ACaldeira.Pooling
{
    public abstract class PooledBehaviour : MonoBehaviour, IPoolable
    {
        public bool IsSpawned { get; private set; }
        public PoolKeySO PoolKey { get; private set; }
        internal object Owner { get; private set; }

        internal void PrepareForPool(PoolKeySO poolKey, object owner)
        {
            if (Owner != null) throw new System.InvalidOperationException("Instance already belongs to a pool.");
            Owner = owner;
            PoolKey = poolKey;
            IsSpawned = false;
            gameObject.SetActive(false);
        }

        internal void SpawnFromPool(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            IsSpawned = true;
            gameObject.SetActive(true);
            OnSpawned();
        }

        internal void ReturnToPool()
        {
            if (!IsSpawned)
            {
                return;
            }

            IsSpawned = false;
            OnDespawned();
            gameObject.SetActive(false);
        }

        public virtual void OnSpawned() { }
        public virtual void OnDespawned() { }
    }
}
