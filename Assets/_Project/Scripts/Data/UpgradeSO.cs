using UnityEngine;

namespace ACaldeira.Data
{
    [CreateAssetMenu(menuName = "A Caldeira/Data/Upgrade", fileName = "Upgrade_")]
    public sealed class UpgradeSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private UpgradeCategory category;
        [SerializeField] private UpgradeRarity rarity;

        [Header("Rules")]
        [SerializeField, Min(1)] private int maxStacks = 1;
        [SerializeField] private WeaponSO grantedWeapon;
        [SerializeField] private WeaponSO evolvedWeapon;
        [SerializeField] private UpgradeSO[] prerequisites;
        [SerializeField] private StatModifier[] modifiers;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public UpgradeCategory Category => category;
        public UpgradeRarity Rarity => rarity;
        public int MaxStacks => maxStacks;
        public WeaponSO GrantedWeapon => grantedWeapon;
        public WeaponSO EvolvedWeapon => evolvedWeapon;
        public UpgradeSO[] Prerequisites => prerequisites;
        public StatModifier[] Modifiers => modifiers;
    }
}
