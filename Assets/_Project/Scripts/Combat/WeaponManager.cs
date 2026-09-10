using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Progression;
using ACaldeira.Simulation;
using UnityEngine;

namespace ACaldeira.Combat
{
    public sealed class WeaponManager : MonoBehaviour
    {
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private RunProgression progression;
        [SerializeField] private PermanentProgression permanent;
        [SerializeField] private Transform muzzle;
        [SerializeField] private WeaponSO[] startingWeapons;
        [SerializeField] private WeaponSO[] stressWeapons;
        [SerializeField, Range(1, 12)] private int maxWeaponSlots = 6;
        private WeaponSO[] definitions;
        private float[] cooldowns;
        public bool HasSpace { get { for (int i = 0; i < definitions.Length; i++) if (definitions[i] == null) return true; return false; } }
        private void Awake() { definitions = new WeaponSO[maxWeaponSlots]; cooldowns = new float[maxWeaponSlots]; }
        public void BeginRun(bool stress = false)
        {
            for (int i = 0; i < definitions.Length; i++) { definitions[i] = null; cooldowns[i] = 0f; }
            var loadout = stress ? stressWeapons : startingWeapons;
            for (int i = 0; i < loadout.Length; i++) TryEquip(loadout[i]);
        }
        public bool Has(WeaponSO weapon)
        {
            for (int i = 0; i < definitions.Length; i++) if (definitions[i] == weapon) return true;
            return false;
        }
        public bool TryEquip(WeaponSO weapon)
        {
            if (weapon == null || Has(weapon)) return false;
            for (int i = 0; i < definitions.Length; i++)
                if (definitions[i] == null) { definitions[i] = weapon; cooldowns[i] = 0; return true; }
            return false;
        }
        public void Evolve(WeaponSO original, WeaponSO evolved)
        {
            if (evolved == null) return;
            for (int i = 0; i < definitions.Length; i++)
                if (definitions[i] == original) { definitions[i] = evolved; cooldowns[i] = 0; return; }
        }
        public void Tick(float dt, GameplaySimulation simulation)
        {
            for (int i = 0; i < definitions.Length; i++)
            {
                if (definitions[i] == null) continue;
                cooldowns[i] -= dt;
                if (cooldowns[i] > 0f) continue;
                if (definitions[i].DeliveryMode == WeaponDeliveryMode.Orbital) TryFire(i, Vector2.right);
                else if (simulation.TryAim(definitions[i].Range, out Vector2 direction)) TryFire(i, direction);
            }
        }
        public bool TryFire(int slot, Vector2 direction)
        {
            if (gameManager.State != GameState.Playing || slot < 0 || slot >= definitions.Length || cooldowns[slot] > 0f) return false;
            WeaponSO w = definitions[slot];
            if (w == null || w.ProjectilePool == null) return false;
            bool fired = false;
            for (int i = 0; i < w.ProjectilesPerShot; i++)
            {
                float angle = w.DeliveryMode == WeaponDeliveryMode.Orbital ? i * 360f / w.ProjectilesPerShot : (i - (w.ProjectilesPerShot - 1) * 0.5f) * 8f;
                Vector2 d = Quaternion.Euler(0, 0, angle) * direction;
                if (!poolManager.TrySpawn(w.ProjectilePool, muzzle.position, Quaternion.identity, out ProjectileActor p)) break;
                p.Configure(w, d);
                p.Damage = progression.Stat(StatId.Damage, w.Damage * (1f + permanent.DamageLevel * 0.05f));
                p.Speed = progression.Stat(StatId.ProjectileSpeed, w.ProjectileSpeed);
                p.Remaining = progression.Stat(StatId.Duration, w.Duration);
                fired = true;
            }
            if (fired) cooldowns[slot] = Mathf.Max(0.04f, progression.Stat(StatId.Cooldown, w.Cooldown));
            return fired;
        }
    }
}
