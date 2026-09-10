namespace ACaldeira.Pooling
{
    public interface IPoolable
    {
        bool IsSpawned { get; }
        void OnSpawned();
        void OnDespawned();
    }
}
