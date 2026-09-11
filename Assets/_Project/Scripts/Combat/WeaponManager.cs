using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Progression;
using ACaldeira.Simulation;
using UnityEngine;

namespace ACaldeira.Combat
{
    public sealed class WeaponManager : MonoBehaviour
    {
        // Mantém o asset preservado para rebalanceamento, mas o impede de entrar
        // em qualquer loadout ou carta até novo playtest.
        private const string SuspendedWeaponId = "Oleo Cru";
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private RunProgression progression;
        [SerializeField] private PermanentProgression permanent;
        [SerializeField] private Transform muzzle;
        [SerializeField] private WeaponSO[] startingWeapons;
        [SerializeField] private WeaponSO[] stressWeapons;
        [SerializeField, Range(1, 12)] private int maxWeaponSlots = 8;
        private WeaponSO[] definitions;
        private float[] cooldowns;
        private int[] levels;
        public bool HasSpace { get { for (int i = 0; i < definitions.Length; i++) if (definitions[i] == null) return true; return false; } }
        public int EquippedCount { get { int count = 0; for (int i = 0; i < definitions.Length; i++) if (definitions[i] != null) count++; return count; } }
        private void Awake() { definitions = new WeaponSO[maxWeaponSlots]; cooldowns = new float[maxWeaponSlots]; levels = new int[maxWeaponSlots]; }
        public void BeginRun(bool stress = false)
        {
            for (int i = 0; i < definitions.Length; i++) { definitions[i] = null; cooldowns[i] = 0f; levels[i] = 0; }
            var loadout = stress ? stressWeapons : startingWeapons;
            for (int i = 0; i < loadout.Length; i++)
                if (IsAvailable(loadout[i])) TryEquip(loadout[i]);
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
                if (definitions[i] == null) { definitions[i] = weapon; cooldowns[i] = 0; levels[i] = 1; return true; }
            return false;
        }
        public int LevelOf(WeaponSO weapon)
        {
            for (int i = 0; i < definitions.Length; i++) if (definitions[i] == weapon) return levels[i];
            return 0;
        }
        public bool TryGetCandidate(bool allowNew, out WeaponSO candidate, out bool isNew)
        {
            candidate = null; isNew = false; int count = 0;
            CollectCandidates(startingWeapons, allowNew, ref candidate, ref isNew, ref count);
            CollectCandidates(stressWeapons, allowNew, ref candidate, ref isNew, ref count, startingWeapons);
            return candidate != null;
        }
        private void CollectCandidates(WeaponSO[] source, bool allowNew, ref WeaponSO candidate, ref bool isNew, ref int count, WeaponSO[] skip = null)
        {
            if (source == null) return;
            for (int i = 0; i < source.Length; i++)
            {
                WeaponSO weapon = source[i]; if (!IsAvailable(weapon)) continue;
                bool alreadySeen = false;
                for (int j = 0; j < i; j++) if (source[j] == weapon) alreadySeen = true;
                if (skip != null)
                    for (int j = 0; j < skip.Length; j++) if (skip[j] == weapon) alreadySeen = true;
                if (alreadySeen) continue;
                bool has = Has(weapon); bool eligible = has ? LevelOf(weapon) < 6 : allowNew;
                if (!eligible) continue;
                if (Random.Range(0, ++count) == 0) { candidate = weapon; isNew = !has; }
            }
        }
        public void ApplyCandidate(int instanceId, bool isNew)
        {
            WeaponSO weapon = FindKnownWeapon(instanceId); if (weapon == null) return;
            if (isNew) { TryEquip(weapon); return; }
            for (int i = 0; i < definitions.Length; i++)
                if (definitions[i] == weapon) { levels[i] = Mathf.Min(6, levels[i] + 1); return; }
        }
        private WeaponSO FindKnownWeapon(int instanceId)
        {
            WeaponSO found = FindKnownWeapon(startingWeapons, instanceId);
            return found != null ? found : FindKnownWeapon(stressWeapons, instanceId);
        }
        private static WeaponSO FindKnownWeapon(WeaponSO[] source, int instanceId)
        {
            if (source == null) return null;
            for (int i = 0; i < source.Length; i++) if (source[i] != null && source[i].GetInstanceID() == instanceId) return source[i];
            return null;
        }
        private static bool IsAvailable(WeaponSO weapon) => weapon != null && weapon.Id != SuspendedWeaponId;
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
            int level = levels[slot];
            int count = w.ProjectilesPerShot + (level >= 2 ? 1 : 0);
            for (int i = 0; i < count; i++)
            {
                float angle = w.DeliveryMode == WeaponDeliveryMode.Orbital ? i * 360f / count : (i - (count - 1) * 0.5f) * 8f;
                Vector2 d = Quaternion.Euler(0, 0, angle) * direction;
                if (!poolManager.TrySpawn(w.ProjectilePool, muzzle.position, Quaternion.identity, out ProjectileActor p)) break;
                p.Configure(w, d);
                p.Damage = progression.Stat(StatId.Damage, w.Damage * (1f + permanent.DamageLevel * 0.05f) * (1f + 0.16f * (level - 1)));
                p.Speed = progression.Stat(StatId.ProjectileSpeed, w.ProjectileSpeed * (level >= 4 ? 1.2f : 1f));
                p.Remaining = progression.Stat(StatId.Duration, w.Duration * (level >= 5 ? 1.2f : 1f));
                p.SetPierce(w.Pierce + (level >= 3 ? 1 : 0));
                fired = true;
            }
            if (fired) cooldowns[slot] = Mathf.Max(0.04f, w.Cooldown * (level >= 4 ? 0.85f : 1f) / progression.CadenceMultiplier);
            return fired;
        }
    }
}
