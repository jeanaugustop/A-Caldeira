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
        private float cableFieldTimer;
        private Vector2 cableFieldCenter;
        private LineRenderer cableFieldVisual;
        private float cablePulseVisualTimer;
        private float cableArcVisualTimer;
        private Vector2 cablePulseCenter;
        private float cablePulseRadius;
        private LineRenderer cablePulseVisual;
        private readonly LineRenderer[] cableArcVisuals = new LineRenderer[3];
        private Material effectVisualMaterial;
        private readonly LineRenderer[] ringChargeVisuals = new LineRenderer[2];
        private LineRenderer ringBlockVisual;
        private float ringBlockVisualTimer;
        private Vector2 ringBlockCenter;
        private float ringBlockRadius;
        private LineRenderer fuseActivationVisual;
        private LineRenderer fuseStateVisual;
        private float fuseActivationVisualTimer;
        private float fuseStateVisualTimer;
        private Vector2 fuseActivationCenter;
        private float fuseActivationRadius;
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
            cableFieldTimer = 0f;
            if (cableFieldVisual != null) cableFieldVisual.enabled = false;
            cablePulseVisualTimer = cableArcVisualTimer = 0f;
            if (cablePulseVisual != null) cablePulseVisual.enabled = false;
            for (int i = 0; i < cableArcVisuals.Length; i++) if (cableArcVisuals[i] != null) cableArcVisuals[i].enabled = false;
            ringBlockVisualTimer = 0f;
            if (ringBlockVisual != null) ringBlockVisual.enabled = false;
            for (int i = 0; i < ringChargeVisuals.Length; i++) if (ringChargeVisuals[i] != null) ringChargeVisuals[i].enabled = false;
            fuseActivationVisualTimer = fuseStateVisualTimer = 0f;
            if (fuseActivationVisual != null) fuseActivationVisual.enabled = false;
            if (fuseStateVisual != null) fuseStateVisual.enabled = false;
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
            TickRingVisuals(dt, p);
            TickFuseVisuals(dt, p);
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
                { Hurt(e.Definition.ContactDamage * e.ContactDamageMultiplier); e.AttackTimer = e.Definition.AttackCooldown; }
                grid.Insert(i, e.Position.x, e.Position.y); Alive++;
            }
            TickCableField(dt);
            TickCableVisuals(dt);
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
                    float angle = Mathf.Atan2(shot.Direction.y, shot.Direction.x) + Elapsed * shot.OrbitAngularSpeed;
                    shot.Position = p + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * shot.OrbitRadius;
                    old = shot.Position;
                }
                else shot.Position += shot.Direction * shot.Speed * dt;
                shot.transform.position = shot.Position;
                shot.UpdatePressureTrail();
                shot.Remaining -= dt;
                Vector2 collisionStart = shot.IsPressureTrail ? shot.TrailOrigin : old;
                if (HitSegment(shot, collisionStart, shot.Position) || shot.Remaining <= 0f) pools.Despawn(shot);
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
            if (progression.TryConsumeFuseShield())
            {
                invulnerability = 0.25f;
                ShowRingBlock(player.position, 1.5f);
                return;
            }
            if (progression.TryDodge(out bool dodgeCable))
            {
                if (dodgeCable) TriggerCable();
                return;
            }
            if (progression.TryBlockDamage(out bool cable))
            {
                invulnerability = progression.RingBlockInvulnerability;
                ShowRingBlock(player.position, progression.RingPushes ? 4f : 1.5f);
                if (progression.RingPushes) PushEnemies(4f, 3f);
                if (cable) TriggerCable();
                return;
            }
            float dealt = Mathf.Max(1f, damage - progression.Stat(StatId.Armor, permanent.ArmorLevel));
            dealt *= progression.DamageMultiplier(Health, MaxHealth);
            if (Health - dealt <= 0f && progression.TryConsumeFuse(out float healthFraction, out int fuseLevel))
            {
                Health = Mathf.Max(1f, MaxHealth * healthFraction); invulnerability = 5f;
                if (fuseLevel >= 3) PushEnemies(6f, 6f);
                ShowFuseActivation(player.position, fuseLevel >= 3 ? 6f : 1.8f);
                return;
            }
            Health = Mathf.Max(0f, Health - dealt);
            bool damageCable = progression.OnDamaged();
            invulnerability = 0.4f;
            if (Health <= 0f) gameManager.EndRun();
            else if (damageCable) TriggerCable();
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
        public void PulseSiren(float radius, float force, float slowFraction, float statusDuration,
            float damageReduction)
        {
            Vector2 p = player.position; float sqr = radius * radius;
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyActor e = enemies[i]; if (!e.IsSpawned) continue;
                Vector2 delta = e.Position - p; if (delta.sqrMagnitude > sqr) continue;
                Vector2 push = delta.sqrMagnitude > 0.001f ? delta.normalized : UnityEngine.Random.insideUnitCircle.normalized;
                e.Position = YardObstacles.ResolveMotion(e.Position,
                    e.Position + push * force, e.Definition.CollisionRadius);
                e.transform.position = e.Position;
                e.ApplySlow(slowFraction, statusDuration);
                if (damageReduction > 0f) e.ApplyDamageReduction(damageReduction, statusDuration);
            }
        }
        private void TriggerCable()
        {
            int level = progression.AccessoryLevel(6);
            if (level <= 0) return;
            Vector2 center = player.position;
            float radius = level >= 2 ? 5f : 3f;
            float radiusSquared = radius * radius;
            float pulseDamage = level >= 3 ? progression.Stat(StatId.Damage, 18f) : 0f;
            ShowCablePulse(center, radius);
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyActor enemy = enemies[i]; if (!enemy.IsSpawned) continue;
                Vector2 delta = enemy.Position - center; if (delta.sqrMagnitude > radiusSquared) continue;
                Vector2 push = delta.sqrMagnitude > 0.001f ? delta.normalized : UnityEngine.Random.insideUnitCircle.normalized;
                enemy.Position = YardObstacles.ResolveMotion(enemy.Position,
                    enemy.Position + push * 3f, enemy.Definition.CollisionRadius);
                enemy.transform.position = enemy.Position;
                if (pulseDamage > 0f && !StressMode && enemy.Damage(pulseDamage)) Kill(enemy);
            }
            if (level >= 5) DamageCableArcs(center, pulseDamage * 0.6f, 3);
            if (level >= 6)
            {
                cableFieldCenter = center;
                cableFieldTimer = 3f;
                ShowCableField(center, 4f);
            }
        }
        private void DamageCableArcs(Vector2 center, float damage, int maximumTargets)
        {
            if (damage <= 0f || StressMode) return;
            EnemyActor[] selected = new EnemyActor[maximumTargets];
            for (int target = 0; target < maximumTargets; target++)
            {
                EnemyActor nearest = null; float best = 64f;
                for (int i = 0; i < enemies.Length; i++)
                {
                    EnemyActor enemy = enemies[i]; if (!enemy.IsSpawned) continue;
                    bool alreadySelected = false;
                    for (int j = 0; j < target; j++) if (selected[j] == enemy) { alreadySelected = true; break; }
                    if (alreadySelected) continue;
                    float distance = (enemy.Position - center).sqrMagnitude;
                    if (distance < best) { best = distance; nearest = enemy; }
                }
                if (nearest == null) return;
                selected[target] = nearest;
                Vector2 nextCenter = nearest.Position;
                ShowCableArc(target, center, nextCenter);
                if (nearest.Damage(damage)) Kill(nearest);
                else nearest.ApplySlow(0.12f, 0.5f);
                center = nextCenter;
            }
        }
        private void TickCableField(float dt)
        {
            if (cableFieldTimer <= 0f) return;
            cableFieldTimer -= dt;
            const float radius = 4f; float sqr = radius * radius;
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyActor enemy = enemies[i];
                if (enemy.IsSpawned && (enemy.Position - cableFieldCenter).sqrMagnitude <= sqr)
                    enemy.ApplySlow(0.2f, 0.2f);
            }
            if (cableFieldTimer <= 0f && cableFieldVisual != null) cableFieldVisual.enabled = false;
        }
        private void ShowCableField(Vector2 center, float radius)
        {
            if (cableFieldVisual == null)
            {
                cableFieldVisual = CreateEffectLine("CaboAterramentoCampo", true, 32, 0.1f,
                    new Color(0.2f, 0.9f, 0.82f, 0.75f), -1);
            }
            SetEffectCircle(cableFieldVisual, center, radius);
            cableFieldVisual.enabled = true;
        }
        private void ShowCablePulse(Vector2 center, float radius)
        {
            if (cablePulseVisual == null)
                cablePulseVisual = CreateEffectLine("CaboAterramentoPulso", true, 32, 0.2f,
                    new Color(0.35f, 1f, 0.92f, 1f), 20);
            cablePulseCenter = center;
            cablePulseRadius = radius;
            cablePulseVisualTimer = 0.4f;
            SetEffectCircle(cablePulseVisual, center, radius * 0.25f);
            cablePulseVisual.enabled = true;
        }
        private void ShowCableArc(int index, Vector2 start, Vector2 end)
        {
            if (index < 0 || index >= cableArcVisuals.Length) return;
            if (cableArcVisuals[index] == null)
                cableArcVisuals[index] = CreateEffectLine("CaboAterramentoArco" + index, false, 4, 0.14f,
                    new Color(0.5f, 1f, 0.9f, 1f), 21);
            LineRenderer arc = cableArcVisuals[index];
            Vector2 delta = end - start;
            Vector2 normal = delta.sqrMagnitude > 0.001f ? new Vector2(-delta.y, delta.x).normalized : Vector2.up;
            arc.SetPosition(0, start);
            arc.SetPosition(1, Vector2.Lerp(start, end, 0.33f) + normal * 0.18f);
            arc.SetPosition(2, Vector2.Lerp(start, end, 0.66f) - normal * 0.14f);
            arc.SetPosition(3, end);
            arc.enabled = true;
            cableArcVisualTimer = 0.3f;
        }
        private void TickCableVisuals(float dt)
        {
            if (cablePulseVisualTimer > 0f)
            {
                cablePulseVisualTimer -= dt;
                float progress = 1f - Mathf.Clamp01(cablePulseVisualTimer / 0.4f);
                SetEffectCircle(cablePulseVisual, cablePulseCenter, Mathf.Lerp(cablePulseRadius * 0.25f, cablePulseRadius, progress));
                Color color = new Color(0.35f, 1f, 0.92f, 1f - progress);
                cablePulseVisual.startColor = cablePulseVisual.endColor = color;
                if (cablePulseVisualTimer <= 0f) cablePulseVisual.enabled = false;
            }
            if (cableArcVisualTimer > 0f)
            {
                cableArcVisualTimer -= dt;
                float alpha = Mathf.Clamp01(cableArcVisualTimer / 0.3f);
                for (int i = 0; i < cableArcVisuals.Length; i++)
                {
                    LineRenderer arc = cableArcVisuals[i]; if (arc == null || !arc.enabled) continue;
                    arc.startColor = arc.endColor = new Color(0.5f, 1f, 0.9f, alpha);
                    if (cableArcVisualTimer <= 0f) arc.enabled = false;
                }
            }
        }
        private void TickRingVisuals(float dt, Vector2 center)
        {
            int charges = progression.RingCharges;
            for (int i = 0; i < ringChargeVisuals.Length; i++)
            {
                if (ringChargeVisuals[i] == null && charges > i)
                    ringChargeVisuals[i] = CreateEffectLine("AnelContingenciaCarga" + i, true, 32, 0.07f,
                        i == 0 ? new Color(0.3f, 0.75f, 1f, 0.9f) : new Color(1f, 0.72f, 0.25f, 0.9f), 18 + i);
                if (ringChargeVisuals[i] == null) continue;
                ringChargeVisuals[i].enabled = charges > i;
                if (charges > i) SetEffectCircle(ringChargeVisuals[i], center, 0.72f + i * 0.18f);
            }
            if (ringBlockVisualTimer <= 0f) return;
            ringBlockVisualTimer -= dt;
            float progress = 1f - Mathf.Clamp01(ringBlockVisualTimer / 0.4f);
            SetEffectCircle(ringBlockVisual, ringBlockCenter, Mathf.Lerp(0.6f, ringBlockRadius, progress));
            Color flash = new Color(0.65f, 0.9f, 1f, 1f - progress);
            ringBlockVisual.startColor = ringBlockVisual.endColor = flash;
            if (ringBlockVisualTimer <= 0f) ringBlockVisual.enabled = false;
        }
        private void ShowRingBlock(Vector2 center, float radius)
        {
            if (ringBlockVisual == null)
                ringBlockVisual = CreateEffectLine("AnelContingenciaBloqueio", true, 32, 0.22f,
                    new Color(0.65f, 0.9f, 1f, 1f), 22);
            ringBlockCenter = center;
            ringBlockRadius = radius;
            ringBlockVisualTimer = 0.4f;
            SetEffectCircle(ringBlockVisual, center, 0.6f);
            ringBlockVisual.enabled = true;
        }
        private void ShowFuseActivation(Vector2 center, float radius)
        {
            if (fuseActivationVisual == null)
                fuseActivationVisual = CreateEffectLine("FusivelSacrificialAtivacao", true, 32, 0.28f,
                    new Color(1f, 0.5f, 0.08f, 1f), 24);
            if (fuseStateVisual == null)
                fuseStateVisual = CreateEffectLine("FusivelSacrificialInvulneravel", true, 32, 0.1f,
                    new Color(1f, 0.68f, 0.18f, 0.9f), 19);
            fuseActivationCenter = center;
            fuseActivationRadius = radius;
            fuseActivationVisualTimer = 0.6f;
            fuseStateVisualTimer = 5f;
            SetEffectCircle(fuseActivationVisual, center, 0.5f);
            SetEffectCircle(fuseStateVisual, center, 1f);
            fuseActivationVisual.enabled = true;
            fuseStateVisual.enabled = true;
        }
        private void TickFuseVisuals(float dt, Vector2 playerPosition)
        {
            if (fuseActivationVisualTimer > 0f)
            {
                fuseActivationVisualTimer -= dt;
                float progress = 1f - Mathf.Clamp01(fuseActivationVisualTimer / 0.6f);
                SetEffectCircle(fuseActivationVisual, fuseActivationCenter, Mathf.Lerp(0.5f, fuseActivationRadius, progress));
                Color flash = new Color(1f, 0.5f, 0.08f, 1f - progress);
                fuseActivationVisual.startColor = fuseActivationVisual.endColor = flash;
                if (fuseActivationVisualTimer <= 0f) fuseActivationVisual.enabled = false;
            }
            if (fuseStateVisualTimer > 0f)
            {
                fuseStateVisualTimer -= dt;
                float remaining = Mathf.Clamp01(fuseStateVisualTimer / 5f);
                SetEffectCircle(fuseStateVisual, playerPosition, Mathf.Lerp(0.65f, 1f, remaining));
                float pulse = 0.55f + Mathf.Sin(Time.unscaledTime * 14f) * 0.3f;
                fuseStateVisual.startColor = fuseStateVisual.endColor = new Color(1f, 0.68f, 0.18f, pulse);
                if (fuseStateVisualTimer <= 0f) fuseStateVisual.enabled = false;
            }
            if (progression.FuseShieldReady)
            {
                if (fuseStateVisual == null)
                    fuseStateVisual = CreateEffectLine("FusivelSacrificialEscudo", true, 32, 0.1f,
                        new Color(0.45f, 0.8f, 1f, 0.9f), 19);
                SetEffectCircle(fuseStateVisual, playerPosition, 0.85f);
                fuseStateVisual.startColor = fuseStateVisual.endColor = new Color(0.45f, 0.8f, 1f, 0.9f);
                fuseStateVisual.enabled = true;
            }
            else if (fuseStateVisualTimer <= 0f && fuseStateVisual != null) fuseStateVisual.enabled = false;
        }
        private LineRenderer CreateEffectLine(string objectName, bool loop, int points, float width, Color color, int sortingOrder)
        {
            GameObject visual = new GameObject(objectName);
            visual.transform.SetParent(transform, false);
            LineRenderer line = visual.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = loop;
            line.positionCount = points;
            line.widthMultiplier = width;
            line.numCapVertices = 2;
            line.startColor = line.endColor = color;
            if (effectVisualMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null) effectVisualMaterial = new Material(shader);
            }
            if (effectVisualMaterial != null) line.sharedMaterial = effectVisualMaterial;
            line.sortingOrder = sortingOrder;
            return line;
        }
        private static void SetEffectCircle(LineRenderer line, Vector2 center, float radius)
        {
            if (line == null) return;
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / line.positionCount;
                line.SetPosition(i, center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
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
            float radius = largestEnemyRadius + .15f + shot.CollisionRadiusBonus;
            Vector2 segment = b - a; float lengthSquared = segment.sqrMagnitude;
            for (int y = grid.Row(Mathf.Min(a.y, b.y) - radius); y <= grid.Row(Mathf.Max(a.y, b.y) + radius); y++)
                for (int x = grid.Column(Mathf.Min(a.x, b.x) - radius); x <= grid.Column(Mathf.Max(a.x, b.x) + radius); x++)
                    for (int i = grid.Head(x, y); i >= 0; i = grid.Next(i))
                    {
                        EnemyActor e = enemies[i];
                        if (!e.IsSpawned || shot.HasHit(i, e.Generation)) continue;
                        float t = lengthSquared > 0f ? Mathf.Clamp01(Vector2.Dot(e.Position - a, segment) / lengthSquared) : 0;
                        float hitRadius = e.Definition.CollisionRadius + .15f + shot.CollisionRadiusBonus;
                        if ((e.Position - a - segment * t).sqrMagnitude > hitRadius * hitRadius) continue;
                        bool exhausted = shot.RegisterHit(i, e.Generation);
                        Vector2 impactPosition = e.Position;
                        if (!StressMode && e.Damage(shot.Damage)) Kill(e);
                        if (shot.Knockback > 0f && e.IsSpawned)
                        {
                            Vector2 pushed = YardObstacles.ResolveMotion(e.Position,
                                e.Position + shot.Direction * shot.Knockback, e.Definition.CollisionRadius);
                            e.Position = pushed; e.transform.position = pushed;
                        }
                        if (shot.SplashRadius > 0f) DamageImpact(shot, impactPosition, i);
                        if (exhausted) return true;
                    }
            return false;
        }
        private void DamageImpact(ProjectileActor shot, Vector2 center, int primaryIndex)
        {
            float radius = shot.SplashRadius;
            float radiusSquared = radius * radius;
            for (int i = 0; i < enemies.Length; i++)
            {
                if (i == primaryIndex) continue;
                EnemyActor enemy = enemies[i];
                if (!enemy.IsSpawned) continue;
                float reach = radius + enemy.Definition.CollisionRadius;
                if ((enemy.Position - center).sqrMagnitude > Mathf.Max(radiusSquared, reach * reach)) continue;
                if (!StressMode && enemy.Damage(shot.Damage * shot.SplashDamageMultiplier)) Kill(enemy);
            }
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
