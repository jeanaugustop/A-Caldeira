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
        [SerializeField, Range(1, 12)] private int maxWeaponSlots = 8;
        private WeaponSO[] definitions;
        private float[] cooldowns;
        private int[] levels;
        private int[] oilShotCounts;
        private int[] rivetFireCounts;
        public bool HasSpace { get { for (int i = 0; i < definitions.Length; i++) if (definitions[i] == null) return true; return false; } }
        public int EquippedCount { get { int count = 0; for (int i = 0; i < definitions.Length; i++) if (definitions[i] != null) count++; return count; } }
        private void Awake()
        {
            definitions = new WeaponSO[maxWeaponSlots]; cooldowns = new float[maxWeaponSlots];
            levels = new int[maxWeaponSlots]; oilShotCounts = new int[maxWeaponSlots]; rivetFireCounts = new int[maxWeaponSlots];
        }
        public void BeginRun(bool stress = false)
        {
            for (int i = 0; i < definitions.Length; i++) { definitions[i] = null; cooldowns[i] = 0f; levels[i] = 0; oilShotCounts[i] = 0; rivetFireCounts[i] = 0; }
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
                WeaponSO weapon = source[i]; if (weapon == null) continue;
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
        public WeaponSO KnownWeapon(int instanceId) => FindKnownWeapon(instanceId);
        public WeaponSO EquippedAt(int index) => definitions != null && index >= 0 && index < definitions.Length ? definitions[index] : null;
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
                if (definitions[i].Id == "Oleo Cru" && simulation.TryAim(12f, out Vector2 oilDirection)) TryFire(i, oilDirection);
                else if (definitions[i].DeliveryMode == WeaponDeliveryMode.Orbital) TryFire(i, Vector2.right);
                else
                {
                    float aimRange = definitions[i].Id == "Estacas Hidraulicas" && levels[i] >= 4
                        ? definitions[i].Range * 1.3f : definitions[i].Range;
                    if (simulation.TryAim(aimRange, out Vector2 direction)) TryFire(i, direction);
                }
            }
        }
        public bool TryFire(int slot, Vector2 direction)
        {
            if (gameManager.State != GameState.Playing || slot < 0 || slot >= definitions.Length || cooldowns[slot] > 0f) return false;
            WeaponSO w = definitions[slot];
            if (w == null || w.ProjectilePool == null) return false;
            bool fired = false;
            int level = levels[slot];
            if (w.Id == "Oleo Cru")
            {
                int puddleCount = level >= 2 ? 2 : 1;
                bool reinforced = level >= 6 && (++oilShotCounts[slot] % 4 == 0);
                float radius = 1.25f + (level >= 5 ? 0.35f : 0f) + (reinforced ? 0.45f : 0f);
                float duration = 3f + (level >= 5 ? 1.25f : 0f);
                // A poça básica precisa produzir uma leitura clara de dano sem voltar a ser uma rajada.
                float damage = progression.Stat(StatId.Damage, w.Damage * (0.75f + (reinforced ? 0.25f : 0f)) * (1f + permanent.DamageLevel * 0.05f));
                float slow = level >= 3 ? 0.18f + (reinforced ? 0.08f : 0f) : 0f;
                float distance = level >= 4 ? 6.5f : 5f;
                Vector2 aim = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
                Vector2 lateralAxis = new Vector2(-aim.y,aim.x);
                for (int i = 0; i < puddleCount; i++)
                {
                    float centered = i-(puddleCount-1)*.5f;
                    float lateral = centered*Mathf.Max(1.6f,radius*1.35f);
                    float depth = puddleCount>1 ? (i%2==0 ? -.35f : .35f) : 0f;
                    Vector2 landingOffset=aim*(distance+depth)+lateralAxis*lateral;
                    if (!poolManager.TrySpawn(w.ProjectilePool, muzzle.position, Quaternion.identity, out ProjectileActor p)) break;
                    p.ConfigureOil(w,landingOffset.normalized,landingOffset.magnitude,radius,duration,damage,slow,0.75f);
                    fired = true;
                }
                if (fired) cooldowns[slot] = 3f / progression.CadenceMultiplier;
                return fired;
            }
            bool isRivet = w.Id == "Rebites de Pressao";
            bool isSaw = w.Id == "Serras Orbitais";
            bool isStake = w.Id == "Estacas Hidraulicas";
            int count = isSaw ? (level >= 6 ? 4 : Mathf.Min(3, level)) : w.ProjectilesPerShot + (level >= 2 ? 1 : 0);
            float shotCooldown = w.Cooldown * (level >= 4 ? (isRivet ? 0.75f : 0.85f) : 1f) / progression.CadenceMultiplier;
            for (int i = 0; i < count; i++)
            {
                float angle = w.DeliveryMode == WeaponDeliveryMode.Orbital ? i * 360f / count : (isRivet || isStake ? 0f : (i - (count - 1) * 0.5f) * 8f);
                Vector2 d = Quaternion.Euler(0, 0, angle) * direction;
                Vector3 origin = muzzle.position;
                if ((isRivet || isStake) && count > 1)
                {
                    float spacing = isStake ? 0.48f : 0.28f;
                    Vector2 lateral = new Vector2(-d.y, d.x) * ((i - (count - 1) * 0.5f) * spacing);
                    origin += (Vector3)lateral;
                }
                if (!poolManager.TrySpawn(w.ProjectilePool, origin, Quaternion.identity, out ProjectileActor p)) break;
                p.Configure(w, d);
                p.Damage = progression.Stat(StatId.Damage, w.Damage * (1f + permanent.DamageLevel * 0.05f) * (1f + 0.16f * (level - 1)));
                p.Speed = progression.Stat(StatId.ProjectileSpeed, w.ProjectileSpeed * (level >= 4 ? (isStake ? 1.3f : 1.2f) : 1f));
                p.Remaining = isSaw ? Mathf.Max(0.05f, shotCooldown) : progression.Stat(StatId.Duration, w.Duration * (level >= 5 && !isStake ? 1.2f : 1f));
                p.SetPierce(w.Pierce + (level >= 3 ? (isStake ? 3 : 1) : 0));
                if (isRivet && level >= 5)
                    p.ConfigureImpact(0.55f, 0.35f, 0f, 1.35f, Color.white);
                if (isSaw)
                {
                    float orbitRadius = progression.Stat(StatId.Area, level >= 4 ? 3.2f : 2.4f);
                    float angularSpeed = level >= 5 ? 4.5f : 3f;
                    p.ConfigureOrbital(orbitRadius, angularSpeed, level >= 6 ? 1.35f : 1f);
                }
                if (isStake && level >= 5)
                {
                    p.Damage *= 1.25f;
                    p.ConfigureImpact(0f, 0f, 2.2f, 1.4f, Color.white, 0.18f);
                    if (level >= 6) p.ConfigurePressureTrail(origin);
                }
                fired = true;
            }
            if (fired && isRivet && level >= 6 && (++rivetFireCounts[slot] % 5 == 0))
            {
                if (poolManager.TrySpawn(w.ProjectilePool, muzzle.position, Quaternion.identity, out ProjectileActor piston))
                {
                    piston.Configure(w, direction);
                    piston.Damage = progression.Stat(StatId.Damage, w.Damage * (1f + permanent.DamageLevel * 0.05f) * 3f);
                    piston.Speed = progression.Stat(StatId.ProjectileSpeed, w.ProjectileSpeed * 1.1f);
                    piston.Remaining = progression.Stat(StatId.Duration, w.Duration * 1.35f);
                    piston.SetPierce(w.Pierce + 5);
                    piston.ConfigureImpact(0.8f, 0.5f, 3f, 1.8f, new Color(1f, 0.78f, 0.3f));
                }
            }
            if (fired) cooldowns[slot] = Mathf.Max(0.04f, shotCooldown);
            return fired;
        }
    }
}
