using System;
using System.Collections.Generic;
using ACaldeira.Data;
using UnityEngine;

namespace ACaldeira.Pooling
{
    internal interface IRuntimePool
    {
        int Capacity { get; }
        int Available { get; }
        Type ElementType { get; }
        bool TryRent(Vector3 position, Quaternion rotation, out PooledBehaviour instance);
        bool Return(PooledBehaviour instance);
        void ReturnAll();
    }

    public sealed class GenericObjectPool<T> : IRuntimePool where T : PooledBehaviour
    {
        private readonly PoolKeySO _key;
        private readonly T[] _available;
        private readonly T[] _all;
        private int _count;

        public int Capacity => _available.Length;
        public int Available => _count;
        public Type ElementType { get; }

        public GenericObjectPool(PoolKeySO key, T[] prewarmedInstances)
        {
            _key = key != null ? key : throw new ArgumentNullException(nameof(key));
            if (prewarmedInstances == null) throw new ArgumentNullException(nameof(prewarmedInstances));
            _all = (T[])prewarmedInstances.Clone();
            _available = (T[])prewarmedInstances.Clone();
            _count = _available.Length;
            ElementType = _count > 0 && _available[0] != null ? _available[0].GetType() : typeof(T);
            var seen = new HashSet<T>();
            for (int i = 0; i < _available.Length; i++)
            {
                T instance = _available[i];
                if (instance == null || !seen.Add(instance) || instance.Owner != null || instance.GetType() != ElementType)
                {
                    throw new ArgumentException("Pool requires unique, unowned, non-null instances of one concrete type.", nameof(prewarmedInstances));
                }

            }
            for (int i = 0; i < _all.Length; i++) _all[i].PrepareForPool(_key, this);
        }

        public bool TryRent(Vector3 position, Quaternion rotation, out T instance)
        {
            if (_count == 0)
            {
                instance = null;
                return false;
            }

            int index = --_count;
            instance = _available[index];
            _available[index] = null;
            instance.SpawnFromPool(position, rotation);
            return true;
        }

        public bool Return(T instance)
        {
            if (instance == null || !instance.IsSpawned || !ReferenceEquals(instance.Owner, this) || _count >= _available.Length)
            {
                return false;
            }

            instance.ReturnToPool();
            _available[_count++] = instance;
            return true;
        }

        public void ReturnAll()
        {
            for (int i = 0; i < _all.Length; i++) Return(_all[i]);
        }

        bool IRuntimePool.TryRent(Vector3 position, Quaternion rotation, out PooledBehaviour instance)
        {
            bool rented = TryRent(position, rotation, out T typedInstance);
            instance = typedInstance;
            return rented;
        }

        bool IRuntimePool.Return(PooledBehaviour instance)
        {
            return instance is T typedInstance && Return(typedInstance);
        }
    }
}
