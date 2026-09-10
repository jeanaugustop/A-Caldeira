using ACaldeira.Data;
using ACaldeira.Pooling;
using UnityEngine;

namespace ACaldeira.Collectibles
{
    public sealed class CollectibleActor : PooledBehaviour
    {
        private CollectibleKind _kind;
        private int _value;
        public Vector2 Position { get; set; }

        public CollectibleKind Kind => _kind;
        public int Value => _value;

        public void Configure(CollectibleKind kind, int value)
        {
            _kind = kind;
            _value = value;
            Position = transform.position;
        }

        public override void OnDespawned()
        {
            _value = 0;
        }
    }
}
