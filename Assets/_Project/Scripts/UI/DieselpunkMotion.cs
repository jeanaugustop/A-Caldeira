using UnityEngine;

namespace ACaldeira.UI
{
    // Cosmetic movement only; the player's collision position never bobs.
    public sealed class DieselpunkMotion : MonoBehaviour
    {
        public Transform Body;
        public SpriteRenderer Visual;
        private Vector3 previous;
        private float phase;
        private DirectionalActor directional;
        private void Awake() { directional=GetComponent<DirectionalActor>(); }
        private void OnEnable() { previous = transform.position; }
        private void LateUpdate()
        {
            Vector3 movement = transform.position - previous;
            if(directional!=null && directional.Sprites!=null)
            {
                // Teleports/restarts should not turn the actor toward the old position.
                if(movement.sqrMagnitude>4f)directional.ResetFacing();
                else directional.TickVisual(movement,Time.deltaTime);
                previous=transform.position;return;
            }
            if (Time.timeScale > 0 && movement.sqrMagnitude > .00001f)
            {
                phase += Time.deltaTime * 14f;
                Body.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(phase)) * .055f, 0);
                if (Mathf.Abs(movement.x) > .001f) Visual.flipX = movement.x < 0;
            }
            else Body.localPosition = Vector3.zero;
            previous = transform.position;
        }
    }
}
