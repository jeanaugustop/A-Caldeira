using ACaldeira.Pooling;
using UnityEngine;

namespace ACaldeira.World
{
    public sealed class HazardActor : PooledBehaviour
    {
        [SerializeField] private SpriteRenderer visual;
        public Vector2 Position { get; private set; }
        public float Timer { get; private set; }
        private float activeDuration;
        private float damage;
        private bool hit;
        public void Configure(float telegraph, float duration, float amount)
        {
            Position = transform.position; Timer = -telegraph;
            activeDuration = duration; damage = amount; hit = false;
            visual.color = new Color(1f, 0.65f, 0.05f, 0.35f);
        }
        public bool Tick(float dt, Vector2 player, out float dealt)
        {
            Timer += dt; dealt = 0f;
            if (Timer >= 0f)
            {
                visual.color = new Color(1f, 0.2f, 0.02f, 0.8f);
                if (!hit && (player - Position).sqrMagnitude <= 2.25f) { dealt = damage; hit = true; }
            }
            return Timer >= activeDuration;
        }
    }
}
