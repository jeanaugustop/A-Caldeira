using System;
using System.Collections.Generic;
using ACaldeira.Data;
using ACaldeira.Pooling;
using UnityEngine;

namespace ACaldeira.Core
{
    public sealed class PoolManager : MonoBehaviour
    {
        [Serializable]
        private sealed class PoolRegistration
        {
            [SerializeField] private PoolKeySO key;
            [SerializeField] private PooledBehaviour[] prewarmedInstances;

            public PoolKeySO Key => key;
            public PooledBehaviour[] Instances => prewarmedInstances;
        }

        [SerializeField] private PoolRegistration[] registrations;

        private Dictionary<PoolKeySO, IRuntimePool> _pools;
        private IRuntimePool[] _orderedPools;
        public int ExhaustedRequests { get; private set; }

        public void Initialize()
        {
            if (_pools != null)
            {
                return;
            }

            int capacity = registrations != null ? registrations.Length : 0;
            var seen = new HashSet<PooledBehaviour>();
            var keys = new HashSet<PoolKeySO>();
            for (int i = 0; i < capacity; i++)
            {
                var r = registrations[i];
                if (r == null || r.Key == null || r.Instances == null || !keys.Add(r.Key))
                    throw new InvalidOperationException("Invalid or duplicate pool registration.");
                for (int j = 0; j < r.Instances.Length; j++)
                    if (r.Instances[j] == null || !seen.Add(r.Instances[j]) || r.Instances[j].Owner != null || r.Instances[j].GetType() != r.Instances[0].GetType())
                        throw new InvalidOperationException("Null or duplicate pooled instance.");
            }
            _pools = new Dictionary<PoolKeySO, IRuntimePool>(capacity);
            _orderedPools = new IRuntimePool[capacity];

            for (int i = 0; i < capacity; i++)
            {
                PoolRegistration registration = registrations[i];
                if (registration == null || registration.Key == null || registration.Instances == null)
                {
                    continue;
                }

                _orderedPools[i] = new GenericObjectPool<PooledBehaviour>(registration.Key, registration.Instances);
                _pools.Add(registration.Key, _orderedPools[i]);
            }
        }

        public bool TrySpawn<T>(PoolKeySO key, Vector3 position, Quaternion rotation, out T instance)
            where T : PooledBehaviour
        {
            instance = null;
            if (key == null || _pools == null || !_pools.TryGetValue(key, out IRuntimePool pool))
            {
                return false;
            }

            if (!typeof(T).IsAssignableFrom(pool.ElementType)) return false;
            if (!pool.TryRent(position, rotation, out PooledBehaviour rented))
            {
                ExhaustedRequests++;
                return false;
            }

            instance = rented as T;
            if (instance != null)
            {
                return true;
            }

            pool.Return(rented);
            return false;
        }

        public bool Despawn(PooledBehaviour instance)
        {
            return instance != null &&
                   instance.PoolKey != null &&
                   _pools != null &&
                   _pools.TryGetValue(instance.PoolKey, out IRuntimePool pool) &&
                   pool.Return(instance);
        }

        public void ReturnAll()
        {
            if (_orderedPools == null) return;
            for (int i = 0; i < _orderedPools.Length; i++) _orderedPools[i].ReturnAll();
            ExhaustedRequests = 0;
        }

        public bool TryGetAvailability(PoolKeySO key, out int available, out int capacity)
        {
            if (_pools != null && key != null && _pools.TryGetValue(key, out IRuntimePool pool))
            {
                available = pool.Available;
                capacity = pool.Capacity;
                return true;
            }

            available = 0;
            capacity = 0;
            return false;
        }
    }
}
