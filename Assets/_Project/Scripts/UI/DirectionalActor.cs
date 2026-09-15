using UnityEngine;

namespace ACaldeira.UI
{
    // Ticked by the existing simulation, not a separate Update on every enemy.
    public sealed class DirectionalActor : MonoBehaviour
    {
        public DirectionalSprites Sprites;
        public SpriteRenderer Visual;
        public int Facing { get; private set; }
        public int MovementFrame { get; private set; } = -1;
        private float traveledFrames;
        private Vector3 rest;
        private void Awake() { if(Visual!=null)rest=Visual.transform.localPosition; }
        private void OnEnable() { ResetFacing(); }
        public void ResetFacing()
        {
            Facing=0;MovementFrame=-1;traveledFrames=0;
            if(Visual!=null)Visual.transform.localPosition=rest;
            ApplyFrame();
        }
        public static int DirectionIndex(Vector2 movement)
        {
            float angle=Mathf.Atan2(-movement.x,-movement.y)*Mathf.Rad2Deg;
            return (Mathf.RoundToInt(angle/45f)+8)%8;
        }
        public void TickVisual(Vector2 movement,float dt)
        {
            if(Sprites==null || Visual==null || dt<=0)return;
            if(movement.sqrMagnitude>0.000001f)
            {
                float angle=Mathf.Atan2(-movement.x,-movement.y)*Mathf.Rad2Deg;
                // Small dead band prevents alternating poses along a diagonal boundary.
                if(Mathf.Abs(Mathf.DeltaAngle(Facing*45f,angle))>27.5f)
                { Facing=DirectionIndex(movement);ApplyFrame(); }
                if(HasMovementCycle)
                {
                    traveledFrames+=movement.magnitude*Sprites.AnimationFramesPerUnit;
                    int next=Mathf.FloorToInt(traveledFrames)%4;
                    if(next!=MovementFrame){MovementFrame=next;ApplyFrame();}
                    Visual.transform.localPosition=rest;
                }
                else
                {
                    traveledFrames+=dt*Sprites.BobFrequency;
                    Visual.transform.localPosition=rest+Vector3.up*(Mathf.Abs(Mathf.Sin(traveledFrames))*Sprites.BobAmplitude);
                }
            }
            else
            {
                if(MovementFrame!=-1){MovementFrame=-1;ApplyFrame();}
                Visual.transform.localPosition=rest;
            }
        }
        private bool HasMovementCycle=>Sprites.MoveFrames!=null && Sprites.MoveFrames.Length==32 && Sprites.MoveFrames[Facing*4]!=null;
        private void ApplyFrame()
        {
            if(Sprites==null || Visual==null || Sprites.Frames==null || Sprites.Frames.Length!=8)return;
            Sprite sprite=MovementFrame>=0 && HasMovementCycle ? Sprites.MoveFrames[Facing*4+MovementFrame] : Sprites.Frames[Facing];
            if(sprite==null)return;
            Visual.sprite=sprite;
            Visual.flipX=MovementFrame<0 && Sprites.Mirror!=null && Sprites.Mirror.Length==8 && Sprites.Mirror[Facing];
            Visual.flipY=false;
            float scale=Sprites.WorldSize/Mathf.Max(sprite.bounds.size.x,sprite.bounds.size.y);
            Visual.transform.localScale=new Vector3(scale,scale,1);
        }
    }
}
