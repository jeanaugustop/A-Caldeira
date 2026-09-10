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
        public event Action<float, float, float, int> HudChanged;
        public event Action EnemyKilled;
        public float Health { get; private set; }
        public float Elapsed { get; private set; }
        public int Kills { get; private set; }
        public int Alive { get; private set; }
        public bool Won { get; private set; }
        public bool StressMode { get; private set; }
        public float MaxHealth => progression.Stat(StatId.MaxHealth, 100f + permanent.ArmorLevel * 10f);
        private void Awake() { grid = new SpatialGrid(enemies.Length, 48, 36, -48f, -36f, 2f); }
        public void Begin(StageSO definition)
        {
            stage = definition; Elapsed = 0; Kills = 0; Alive = 0; Won = false;
            StressMode = stage.Id == "stress";
            hazardTimers = new float[stage.Hazards.Length];
            for (int i = 0; i < hazardTimers.Length; i++) hazardTimers[i] = stage.Hazards[i].StartTime;
            player.position = Vector3.zero; progression.Begin(); weapons.BeginRun(StressMode);
            Health = MaxHealth; invulnerability = 0; hudTimer = 0; grid.Clear();
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
            Vector2 p = (Vector2)player.position + input.ReadMovement() * progression.Stat(StatId.MoveSpeed, 5f) * dt;
            Rect bounds = stage.SpawnArea;
            p.x = Mathf.Clamp(p.x, bounds.xMin + 1, bounds.xMax - 1); p.y = Mathf.Clamp(p.y, bounds.yMin + 1, bounds.yMax - 1);
            player.position = p;
            worldCamera.transform.position = new Vector3(p.x, p.y, -10f);
            waves.Tick(dt); grid.Clear(); Alive = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyActor e = enemies[i]; if (!e.IsSpawned) continue;
                Vector2 delta = p - e.Position;
                float distance = delta.magnitude;
                if (StressMode)
                {
                    float angle = Elapsed * 0.12f + i * 2.39996f;
                    float radius = 2f + (i % 140) * 0.1f;
                    e.Position = p + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                }
                else if (distance > 0.7f) e.Position += delta / distance * e.Definition.MoveSpeed * dt;
                e.transform.position = e.Position;
                e.AttackTimer -= dt;
                if (distance < 0.85f && e.AttackTimer <= 0f)
                { Hurt(e.Definition.ContactDamage); e.AttackTimer = e.Definition.AttackCooldown; }
                grid.Insert(i, e.Position.x, e.Position.y); Alive++;
            }
            if (gameManager.State != GameState.Playing) return;
            weapons.Tick(dt, this);
            for (int i = 0; i < projectiles.Length; i++)
            {
                ProjectileActor shot = projectiles[i]; if (!shot.IsSpawned) continue;
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
            Health = Mathf.Max(0f, Health - Mathf.Max(1f, damage - progression.Stat(StatId.Armor, permanent.ArmorLevel)));
            invulnerability = 0.4f;
            if (Health <= 0f) gameManager.EndRun();
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
            const float radius = 0.6f;
            Vector2 segment = b - a; float lengthSquared = segment.sqrMagnitude;
            for (int y = grid.Row(Mathf.Min(a.y, b.y) - radius); y <= grid.Row(Mathf.Max(a.y, b.y) + radius); y++)
                for (int x = grid.Column(Mathf.Min(a.x, b.x) - radius); x <= grid.Column(Mathf.Max(a.x, b.x) + radius); x++)
                    for (int i = grid.Head(x, y); i >= 0; i = grid.Next(i))
                    {
                        EnemyActor e = enemies[i];
                        if (!e.IsSpawned || shot.HasHit(i, e.Generation)) continue;
                        float t = lengthSquared > 0f ? Mathf.Clamp01(Vector2.Dot(e.Position - a, segment) / lengthSquared) : 0;
                        if ((e.Position - a - segment * t).sqrMagnitude > radius * radius) continue;
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
