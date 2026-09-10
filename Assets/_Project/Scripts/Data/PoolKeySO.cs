using UnityEngine;

namespace ACaldeira.Data
{
    [CreateAssetMenu(menuName = "A Caldeira/Pooling/Pool Key", fileName = "PoolKey_")]
    public sealed class PoolKeySO : ScriptableObject
    {
        [SerializeField] private string id;

        public string Id => id;
    }
}
