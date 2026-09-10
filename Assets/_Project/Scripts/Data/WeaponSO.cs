using UnityEngine;

namespace ACaldeira.Data
{
    [CreateAssetMenu(menuName = "A Caldeira/Data/Weapon", fileName = "Weapon_")]
    public sealed class WeaponSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;

        [Header("Delivery")]
        [SerializeField] private WeaponDeliveryMode deliveryMode;
        [SerializeField] private DamageType damageType;
        [SerializeField] private PoolKeySO projectilePool;

        [Header("Base stats")]
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.01f)] private float cooldown = 0.5f;
        [SerializeField, Min(0f)] private float projectileSpeed = 12f;
        [SerializeField, Min(0)] private int pierce;
        [SerializeField, Min(0f)] private float knockback;
        [SerializeField, Min(0f)] private float range = 8f;
        [SerializeField, Min(0f)] private float duration = 1f;
        [SerializeField, Min(1)] private int projectilesPerShot = 1;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public WeaponDeliveryMode DeliveryMode => deliveryMode;
        public DamageType DamageType => damageType;
        public PoolKeySO ProjectilePool => projectilePool;
        public float Damage => damage;
        public float Cooldown => cooldown;
        public float ProjectileSpeed => projectileSpeed;
        public int Pierce => pierce;
        public float Knockback => knockback;
        public float Range => range;
        public float Duration => duration;
        public int ProjectilesPerShot => projectilesPerShot;
    }
}
