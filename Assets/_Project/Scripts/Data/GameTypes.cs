using System;
using UnityEngine;

namespace ACaldeira.Data
{
    public enum GameState { MainMenu, Loading, Playing, Paused, LevelUp, GameOver }
    public enum WeaponDeliveryMode { Projectile, Beam, Area, Orbital }
    public enum DamageType { Impact, Heat, Cutting, Hydraulic }
    public enum UpgradeCategory { StatBoost, WeaponUnlock, WeaponEvolution }
    public enum UpgradeRarity { Common, Uncommon, Rare, Epic, Legendary }
    public enum StatId { MaxHealth, Armor, MoveSpeed, Damage, Cooldown, Area, Duration, ProjectileSpeed, PickupRadius }
    public enum ModifierOperation { Flat, AdditivePercent, MultiplicativePercent }
    public enum CollectibleKind { HotScrap, Oil }
    public enum WindowMode { Windowed, Borderless, ExclusiveFullscreen }

    [Serializable]
    public struct StatModifier
    {
        [SerializeField] private StatId stat;
        [SerializeField] private ModifierOperation operation;
        [SerializeField] private float value;

        public StatId Stat => stat;
        public ModifierOperation Operation => operation;
        public float Value => value;
    }
}
