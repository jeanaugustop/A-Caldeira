using ACaldeira.Data;
using ACaldeira.Pooling;
using UnityEngine;

namespace ACaldeira.Combat
{
    public sealed class ProjectileActor : PooledBehaviour
    {
        private WeaponSO _definition;
        private Vector2 _direction;
        private readonly EnemyActorHit[] hits = new EnemyActorHit[16];
        private int hitCount;
        private int maxHits;
        private SpriteRenderer visual;
        [SerializeField] private Sprite puddleSprite;
        private Sprite defaultSprite;
        private Color defaultColor;
        private int defaultSortingOrder;
        private Vector3 defaultScale;
        private bool oilInFlight;
        private bool oilPuddle;
        private Vector2 oilLanding;
        private float oilPulseTimer;
        private float oilRadius;
        private float oilSlow;
        private float oilSlowDuration;
        private float splashRadius;
        private float splashDamageMultiplier;
        private float collisionRadiusBonus;
        private bool pressureTrail;
        private Vector2 trailOrigin;
        private LineRenderer trailVisual;
        private struct EnemyActorHit { internal int Index; internal uint Generation; }
        public Vector2 Position { get; set; }
        public float Remaining { get; set; }
        public float Damage { get; set; }
        public float Speed { get; set; }

        public WeaponSO Definition => _definition;
        public Vector2 Direction => _direction;
        public bool IsOilInFlight => oilInFlight;
        public bool IsOilPuddle => oilPuddle;
        public float OilRadius => oilRadius;
        public float OilSlow => oilSlow;
        public float OilSlowDuration => oilSlowDuration;
        public Vector2 OilLanding => oilLanding;
        public float SplashRadius => splashRadius;
        public float SplashDamageMultiplier => splashDamageMultiplier;
        public float CollisionRadiusBonus => collisionRadiusBonus;
        public float Knockback { get; private set; }
        public bool IsPressureTrail => pressureTrail;
        public Vector2 TrailOrigin => trailOrigin;
        public float OrbitRadius { get; private set; }
        public float OrbitAngularSpeed { get; private set; }

        private void Awake()
        {
            visual = GetComponent<SpriteRenderer>();
            defaultScale = transform.localScale;
            if (visual != null) { defaultSprite=visual.sprite; defaultColor=visual.color; defaultSortingOrder=visual.sortingOrder; }
        }

        public void Configure(WeaponSO definition, Vector2 direction)
        {
            _definition = definition;
            _direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
            Position = transform.position;
            Remaining = definition.Duration;
            Damage = definition.Damage;
            Speed = definition.ProjectileSpeed;
            Knockback = definition.Knockback;
            splashRadius = 0f;
            splashDamageMultiplier = 0f;
            collisionRadiusBonus = 0f;
            pressureTrail = false;
            OrbitRadius = 0f;
            OrbitAngularSpeed = 0f;
            if (trailVisual != null) trailVisual.enabled = false;
            hitCount = 0; maxHits = Mathf.Min(16, definition.Pierce + 1);
            oilInFlight = false; oilPuddle = false;
            RestoreVisual();
            if (visual != null && definition.Id != "Serras Orbitais")
                transform.rotation=Quaternion.Euler(0f,0f,Mathf.Atan2(_direction.y,_direction.x)*Mathf.Rad2Deg);
        }

        public void ConfigureImpact(float impactRadius, float impactDamageMultiplier, float knockback,
            float visualScale, Color visualTint, float hitRadiusBonus = 0f)
        {
            splashRadius = impactRadius;
            splashDamageMultiplier = impactDamageMultiplier;
            Knockback = knockback;
            collisionRadiusBonus = hitRadiusBonus;
            transform.localScale = defaultScale * visualScale;
            if (visual != null) visual.color = visualTint;
        }

        public void ConfigureOrbital(float radius, float angularSpeed, float visualScale)
        {
            OrbitRadius = radius;
            OrbitAngularSpeed = angularSpeed;
            collisionRadiusBonus = visualScale > 1f ? 0.2f : 0f;
            transform.localScale = defaultScale * visualScale;
        }

        public void ConfigurePressureTrail(Vector2 origin)
        {
            pressureTrail = true;
            trailOrigin = origin;
            if (trailVisual == null)
            {
                trailVisual = gameObject.AddComponent<LineRenderer>();
                trailVisual.useWorldSpace = true;
                trailVisual.positionCount = 2;
                trailVisual.startWidth = 0.28f;
                trailVisual.endWidth = 0.12f;
                trailVisual.numCapVertices = 2;
                trailVisual.startColor = new Color(1f, 0.65f, 0.18f, 0.78f);
                trailVisual.endColor = new Color(0.55f, 0.22f, 0.06f, 0.18f);
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null) trailVisual.material = new Material(shader);
                trailVisual.sortingOrder = -1;
            }
            trailVisual.enabled = true;
            UpdatePressureTrail();
        }

        public void UpdatePressureTrail()
        {
            if (!pressureTrail || trailVisual == null) return;
            trailVisual.SetPosition(0, trailOrigin);
            trailVisual.SetPosition(1, Position);
        }

        public void ConfigureOil(WeaponSO definition, Vector2 direction, float landingDistance,
            float radius, float duration, float damage, float slow, float slowDuration)
        {
            Configure(definition, direction);
            oilInFlight = true;
            oilLanding = Position + _direction * landingDistance;
            oilRadius = radius;
            Remaining = duration;
            Damage = damage;
            Speed = Mathf.Max(8f, definition.ProjectileSpeed);
            oilSlow = slow;
            oilSlowDuration = slowDuration;
        }

        public bool TickOilFlight(float deltaTime)
        {
            transform.Rotate(0f,0f,deltaTime*240f);
            Position = Vector2.MoveTowards(Position, oilLanding, Speed * deltaTime);
            if ((Position - oilLanding).sqrMagnitude > 0.001f) return false;
            oilInFlight = false; oilPuddle = true; oilPulseTimer = 0.15f;
            if (visual != null && puddleSprite != null)
            {
                visual.sprite=puddleSprite; visual.color=Color.white; visual.sortingOrder=-5;
                transform.localScale=Vector3.one*(oilRadius*2f/Mathf.Max(puddleSprite.bounds.size.x,puddleSprite.bounds.size.y));
                transform.rotation=Quaternion.identity;
            }
            return true;
        }

        public bool TickOilPuddle(float deltaTime, out bool pulse)
        {
            pulse = false;
            Remaining -= deltaTime;
            oilPulseTimer -= deltaTime;
            if (oilPulseTimer <= 0f)
            {
                oilPulseTimer += 0.5f;
                pulse = true;
            }
            return Remaining <= 0f;
        }

        public void TickVisual(float deltaTime)
        {
            if (_definition != null && _definition.Id == "Serras Orbitais")
                transform.Rotate(0f,0f,-deltaTime*540f);
        }

        public void SetPierce(int pierce) => maxHits = Mathf.Min(16, Mathf.Max(1, pierce + 1));

        public bool HasHit(int index, uint generation)
        {
            for (int i = 0; i < hitCount; i++)
                if (hits[i].Index == index && hits[i].Generation == generation) return true;
            return false;
        }
        public bool RegisterHit(int index, uint generation)
        {
            hits[hitCount++] = new EnemyActorHit { Index = index, Generation = generation };
            return hitCount >= maxHits;
        }

        public override void OnDespawned()
        {
            _definition = null;
            _direction = Vector2.zero;
            oilInFlight = false; oilPuddle = false;
            splashRadius = 0f;
            splashDamageMultiplier = 0f;
            collisionRadiusBonus = 0f;
            Knockback = 0f;
            pressureTrail = false;
            OrbitRadius = 0f;
            OrbitAngularSpeed = 0f;
            if (trailVisual != null) trailVisual.enabled = false;
            RestoreVisual();
        }

        private void RestoreVisual()
        {
            transform.localScale = defaultScale == Vector3.zero ? Vector3.one * 0.3f : defaultScale;
            transform.rotation=Quaternion.identity;
            if (visual != null) { visual.sprite=defaultSprite; visual.sortingOrder=defaultSortingOrder; visual.color=defaultColor; }
        }
    }
}
