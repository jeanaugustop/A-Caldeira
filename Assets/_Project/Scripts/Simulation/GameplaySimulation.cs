using System;
using ACaldeira.Core;
using ACaldeira.Combat;
using ACaldeira.Collectibles;
using ACaldeira.Data;
using ACaldeira.Enemies;
using ACaldeira.Input;
using ACaldeira.Progression;
using ACaldeira.World;
using UnityEngine;

namespace ACaldeira.Simulation
{
    [DefaultExecutionOrder(100)]
    public sealed class GameplaySimulation : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PoolManager pools;
        [SerializeField] private WaveSpawner waves;
        [SerializeField] private WeaponManager weapons;
        [SerializeField] private RunProgression progression;
        [SerializeField] private PermanentProgression permanent;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private Transform player;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private EnemyActor[] enemies;
        [SerializeField] private ProjectileActor[] projectiles;
        [SerializeField] private CollectibleActor[] collectibles;
        [SerializeField] private HazardActor[] hazards;
        private SpatialGrid grid;
        private StageSO stage;
        private float[] hazardTimers;
        private float invulnerability;
        private float hudTimer;
        private float knownMaxHealth;
        private float largestEnemyRadius=.45f;
        public event Action<float, float, float, int> HudChanged;
        public event Action EnemyKilled;
        public float Health { get; private set; }
        public float Elapsed { get; private set; }
        public int Kills { get; private set; }
        public int Alive { get; private set; }
        public bool Won { get; private set; }
        public bool StressMode { get; private set; }
        public float MaxHealth => progression.Stat(StatId.MaxHealth, 100f + permanent.ArmorLevel * 10f + progression.MaxHealthBonus);
        private void Awake() { EnsureGrid(); }
        private void EnsureGrid()
        {
            if (grid != null) return;
            Rect bounds = stage != null ? stage.SpawnArea : new Rect(-90f, -60f, 180f, 120f);
            const float padding = 24f;
            grid = new SpatialGrid(enemies.Length,
                Mathf.CeilToInt((bounds.width + padding * 2) / 2f),
                Mathf.CeilToInt((bounds.height + padding * 2) / 2f),
                bounds.xMin - padding, bounds.yMin - padding, 2f);
        }
        public void Begin(StageSO definition)
        {
            stage = definition; grid = null; EnsureGrid();
            Elapsed = 0; Kills = 0; Alive = 0; Won = false;
            StressMode = stage.Id == "stress";
            hazardTimers = new float[stage.Hazards.Length];
            for (int i = 0; i < hazardTimers.Length; i++) hazardTimers[i] = stage.Hazards[i].StartTime;
            player.position = Vector3.zero; progression.Begin(); weapons.BeginRun(StressMode);
            var directional=player.GetComponent<ACaldeira.UI.DirectionalActor>();
            if(directional!=null)directional.ResetFacing();
            Health = MaxHealth; knownMaxHealth = MaxHealth; invulnerability = 0; hudTimer = 0; grid.Clear();
            waves.Begin(stage);
            if (StressMode)
            {
                UnityEngine.Random.InitState(2026);
                EnemySO enemyDefinition = stage.Waves[0].Enemies[0].Enemy;
                for (int i = 0; i < 1200; i++)
                    if (pools.TrySpawn(enemyDefinition.ActorPool, new Vector3(UnityEngine.Random.Range(-16f, 16f), UnityEngine.Random.Range(-9f, 9f), 0), Quaternion.identity, out EnemyActor e))
                        e.Configure(enemyDefinition, -1, null);
            }
        }
        private void Update()
        {
            if (gameManager.State != GameState.Playing) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            Elapsed += dt; invulnerability -= dt;
            progression.TickEquipment(dt, this);
            float currentMaxHealth = MaxHealth;
            if (currentMaxHealth > knownMaxHealth) Health += currentMaxHealth - knownMaxHealth;
            knownMaxHealth = currentMaxHealth;
            Health = Mathf.Min(currentMaxHealth, Health + dt);
            Vector2 p = (Vector2)player.position + input.ReadMovement() * progression.Stat(StatId.MoveSpeed, 5f) * progression.MovementMultiplier * dt;
            Rect bounds = stage.SpawnArea;
            p.x = Mathf.Clamp(p.x, bounds.xMin + 1, bounds.xMax - 1); p.y = Mathf.Clamp(p.y, bounds.yMin + 1, bounds.yMax - 1);
            p = YardObstacles.ResolveMotion(player.position, p);
            player.position = p;
            worldCamera.transform.position = new Vector3(p.x, p.y, -10f);
            if (waves == null) waves = FindFirstObjectByType<WaveSpawner>();
            if (waves == null)
            {
                Debug.LogError("A Caldeira: WaveSpawner não foi encontrado.");
                gameManager.EndRun();
                return;
            }
            EnsureGrid();
            waves.Tick(dt); grid.Clear(); Alive = 0;largestEnemyRadius=.45f;
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyActor e = enemies[i]; if (!e.IsSpawned) continue;
                e.TickStatus(dt);
                Vector2 oldEnemyPosition=e.Position;
                float bodyRadius=e.Definition.CollisionRadius;
                largestEnemyRadius=Mathf.Max(largestEnemyRadius,bodyRadius);
                Vector2 delta = p - e.Position;
                float distance = delta.magnitude;
                if (StressMode)
                {
                    float angle = Elapsed * 0.12f + i * 2.39996f;
                    float radius = 2f + (i % 140) * 0.1f;
                    e.Position = p + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                }
                else if (distance > bodyRadius+.25f)
                {
                    Vector2 previous=e.Position;
                    float step=e.EffectiveMoveSpeed*dt;
                    e.Position=YardObstacles.ResolveMotion(previous,previous+delta/distance*step,bodyRadius);
                    if((e.Position-previous).sqrMagnitude<step*step*.04f)
                    {
                        Vector2 tangent=new Vector2(-delta.y,delta.x).normalized;
                        e.Position=YardObstacles.ResolveMotion(previous,previous+tangent*step,bodyRadius);
                    }
                }
                e.transform.position = e.Position;
                e.TickVisual(e.Position-oldEnemyPosition,dt);
                e.AttackTimer -= dt;
                if ((p-e.Position).sqrMagnitude < (bodyRadius+.4f)*(bodyRadius+.4f) && e.AttackTimer <= 0f)
                { Hurt(e.Definition.ContactDamage); e.AttackTimer = e.Definition.AttackCooldown; }
                grid.Insert(i, e.Position.x, e.Position.y); Alive++;
            }
            if (gameManager.State != GameState.Playing) return;
            weapons.Tick(dt, this);
            for (int i = 0; i < projectiles.Length; i++)
            {
                ProjectileActor shot = projectiles[i]; if (!shot.IsSpawned) continue;
                shot.TickVisual(dt);
                if (shot.IsOilInFlight)
                {
                    shot.TickOilFlight(dt);
                    shot.transform.position = shot.Position;
                    continue;
                }
                if (shot.IsOilPuddle)
                {
                    bool ended = shot.TickOilPuddle(dt, out bool pulse);
                    if (pulse) DamageOilPuddle(shot);
                    if (ended) pools.Despawn(shot);
                    continue;
                }
                Vector2 old = shot.Position;
                if (shot.Definition.DeliveryMode == WeaponDeliveryMode.Orbital)
                {
                    float angle = Mathf.Atan2(shot.Direction.y, shot.Direction.x) + Elapsed * 3f;
                    shot.Position = p + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * progression.Stat(StatId.Area, 2.4f);
                    old = shot.Position;
                }
                else shot.Position += shot.Direction * shot.Speed * dt;
                shot.transform.position = shot.Position;
                shot.Remaining -= dt;
                if (HitSegment(shot, old, shot.Position) || shot.Remaining <= 0f) pools.Despawn(shot);
            }
            for (int i = 0; i < collectibles.Length; i++)
            {
                var c = collectibles[i]; if (!c.IsSpawned) continue;
                Vector2 delta = p - c.Position; float d = delta.sqrMagnitude;
                if (d < 0.65f)
                {
                    int value = c.Value; pools.Despawn(c); progression.Gain(value);
                    if (gameManager.State != GameState.Playing) break;
                }
                else if (d < Mathf.Pow(progression.Stat(StatId.PickupRadius, 3f), 2))
                { c.Position = Vector2.MoveTowards(c.Position, p, dt * 10f); c.transform.position = c.Position; }
            }
            if (gameManager.State != GameState.Playing) return;
            TickHazards(dt, p);
            hudTimer -= dt;
            if (hudTimer <= 0f) { hudTimer = 0.1f; HudChanged?.Invoke(Health, MaxHealth, Elapsed, Alive); }
            if (Elapsed >= stage.DurationSeconds) { Won = true; gameManager.EndRun(); }
        }
        public void Hurt(float damage)
        {
            if (StressMode || invulnerability > 0 || gameManager.State != GameState.Playing) return;
            if (progression.TryDodge()) return;
            if (progression.TryBlockDamage(out bool cable))
            {
                invulnerability = 0.25f;
                if (cable) PushEnemies(4f + progression.AccessoryLevel(6), 3f + progression.AccessoryLevel(6));
                return;
            }
            float dealt = Mathf.Max(1f, damage - progression.Stat(StatId.Armor, permanent.ArmorLevel));
            dealt *= progression.DamageMultiplier(Health, MaxHealth);
            if (Health - dealt <= 0f && progression.TryConsumeFuse(out float healthFraction))
            {
                Health = Mathf.Max(1f, MaxHealth * healthFraction); invulnerability = 5f;
                PushEnemies(6f, 6f); return;
            }
            Health = Mathf.Max(0f, Health - dealt);
            progression.OnDamaged();
            invulnerability = 0.4f;
            if (Health <= 0f) gameManager.EndRun();
        }
        public void AttractExperience(float radius)
        {
            Vector2 p = player.position; float sqr = radius * radius;
            for (int i = 0; i < collectibles.Length; i++)
            {
                CollectibleActor c = collectibles[i];
                if (!c.IsSpawned || ((Vector2)c.Position - p).sqrMagnitude > sqr) continue;
                c.Position = Vector2.MoveTowards(c.Position, p, radius); c.transform.position = c.Position;
            }
        }
        public void PushEnemies(float radius, float force)
        {
            Vector2 p = player.position; float sqr = radius * radius;
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyActor e = enemies[i]; if (!e.IsSpawned) continue;
                Vector2 delta = e.Position - p; if (delta.sqrMagnitude > sqr) continue;
                e.Position += delta.sqrMagnitude > 0.001f ? delta.normalized * force : UnityEngine.Random.insideUnitCircle * force;
                e.transform.position = e.Position;
            }
        }
        public bool TryAim(float range, out Vector2 direction)
        {
            Vector2 p = player.position; direction = Vector2.right;
            float best = range * range; bool found = false;
            for (int y = grid.Row(p.y - range); y <= grid.Row(p.y + range); y++)
                for (int x = grid.Column(p.x - range); x <= grid.Column(p.x + range); x++)
                    for (int i = grid.Head(x, y); i >= 0; i = grid.Next(i))
                    {
                        var e = enemies[i]; if (!e.IsSpawned) continue;
                        Vector2 delta = e.Position - p;
                        if (delta.sqrMagnitude < best) { best = delta.sqrMagnitude; direction = delta.normalized; found = true; }
                    }
            return found;
        }
        private bool HitSegment(ProjectileActor shot, Vector2 a, Vector2 b)
        {
            float radius = largestEnemyRadius+.15f;
            Vector2 segment = b - a; float lengthSquared = segment.sqrMagnitude;
            for (int y = grid.Row(Mathf.Min(a.y, b.y) - radius); y <= grid.Row(Mathf.Max(a.y, b.y) + radius); y++)
                for (int x = grid.Column(Mathf.Min(a.x, b.x) - radius); x <= grid.Column(Mathf.Max(a.x, b.x) + radius); x++)
                    for (int i = grid.Head(x, y); i >= 0; i = grid.Next(i))
                    {
                        EnemyActor e = enemies[i];
                        if (!e.IsSpawned || shot.HasHit(i, e.Generation)) continue;
                        float t = lengthSquared > 0f ? Mathf.Clamp01(Vector2.Dot(e.Position - a, segment) / lengthSquared) : 0;
                        float hitRadius=e.Definition.CollisionRadius+.15f;
                        if ((e.Position - a - segment * t).sqrMagnitude > hitRadius * hitRadius) continue;
                        bool exhausted = shot.RegisterHit(i, e.Generation);
                        if (!StressMode && e.Damage(shot.Damage)) Kill(e);
                        if (exhausted) return true;
                    }
            return false;
        }
        private void Kill(EnemyActor enemy)
        {
            EnemySO definition = enemy.Definition; Vector2 position = enemy.Position;
            pools.Despawn(enemy); Kills++; EnemyKilled?.Invoke();
            if (UnityEngine.Random.value <= definition.DropChance &&
                pools.TrySpawn(definition.CollectiblePool, position, Quaternion.identity, out CollectibleActor c))
                c.Configure(definition.DropKind, definition.ExperienceValue);
        }
        private void DamageOilPuddle(ProjectileActor puddle)
        {
            float radius = puddle.OilRadius;
            Vector2 center = puddle.Position;
            // Há poucas poças simultâneas. A varredura direta evita que um alvo deixe de receber
            // o pulso por estar numa célula de grade diferente durante a atualização do quadro.
            for(int i=0;i<enemies.Length;i++)
            {
                EnemyActor enemy=enemies[i];
                if(!enemy.IsSpawned)continue;
                float reach=radius+enemy.Definition.CollisionRadius;
                if((enemy.Position-center).sqrMagnitude>reach*reach)continue;
                if(puddle.OilSlow>0f)enemy.ApplySlow(puddle.OilSlow,puddle.OilSlowDuration);
                if(!StressMode && enemy.Damage(puddle.Damage))Kill(enemy);
            }
        }
        private void TickHazards(float dt, Vector2 p)
        {
            for (int i = 0; i < hazardTimers.Length; i++)
            {
                if (Elapsed < hazardTimers[i]) continue;
                HazardDefinition h = stage.Hazards[i]; hazardTimers[i] = Elapsed + h.Interval;
                if (pools.TrySpawn(h.HazardPool, p + UnityEngine.Random.insideUnitCircle * 4f, Quaternion.identity, out HazardActor actor))
                    actor.Configure(h.TelegraphDuration, h.ActiveDuration, h.Damage);
            }
            for (int i = 0; i < hazards.Length; i++)
            {
                var h = hazards[i]; if (!h.IsSpawned) continue;
                bool ended = h.Tick(dt, p, out float damage);
                if (damage > 0) Hurt(damage);
                if (ended) pools.Despawn(h);
            }
        }
    }
}
