using UnityEngine;

namespace ACaldeira.Data
{
    [CreateAssetMenu(menuName = "A Caldeira/Data/Enemy", fileName = "Enemy_")]
    public sealed class EnemySO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite sprite;
        [SerializeField] private PoolKeySO actorPool;

        [Header("Base stats")]
        [SerializeField, Min(1f)] private float maxHealth = 20f;
        [SerializeField, Min(0f)] private float moveSpeed = 2f;
        [SerializeField, Min(0f)] private float contactDamage = 5f;
        [SerializeField, Min(0.01f)] private float attackCooldown = 1f;
        [SerializeField, Min(0f)] private float armor;
        [SerializeField, Min(0f)] private float mass = 1f;

        [Header("Rewards")]
        [SerializeField] private CollectibleKind dropKind = CollectibleKind.HotScrap;
        [SerializeField, Min(0)] private int experienceValue = 1;
        [SerializeField, Range(0f, 1f)] private float dropChance = 1f;
        [SerializeField] private PoolKeySO collectiblePool;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
        public PoolKeySO ActorPool => actorPool;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float ContactDamage => contactDamage;
        public float AttackCooldown => attackCooldown;
        public float Armor => armor;
        public float Mass => mass;
        public CollectibleKind DropKind => dropKind;
        public int ExperienceValue => experienceValue;
        public float DropChance => dropChance;
        public PoolKeySO CollectiblePool => collectiblePool;
    }
}
