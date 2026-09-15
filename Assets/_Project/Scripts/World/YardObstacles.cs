using UnityEngine;

namespace ACaldeira.World
{
    public sealed class YardObstacles : MonoBehaviour
    {
        public Rect[] Footprints;
        private static YardObstacles active;
        private void OnEnable() { active=this; }
        private void OnDisable() { if(active==this)active=null; }
        public static bool IsBlocked(Vector2 point,float radius=.4f)
        {
            if(active==null || active.Footprints==null)return false;
            foreach(var footprint in active.Footprints)
                if(Expanded(footprint,radius).Contains(point))return true;
            return false;
        }
        public static Vector2 ResolveMotion(Vector2 previous,Vector2 proposed,float radius=.4f)
        {
            if(active==null || active.Footprints==null)return proposed;
            foreach(var footprint in active.Footprints)
            {
                Rect r=Expanded(footprint,radius);
                if(!r.Contains(proposed))continue;
                if(previous.x<=r.xMin)proposed.x=r.xMin-.001f;
                else if(previous.x>=r.xMax)proposed.x=r.xMax+.001f;
                else if(previous.y<=r.yMin)proposed.y=r.yMin-.001f;
                else if(previous.y>=r.yMax)proposed.y=r.yMax+.001f;
                else
                {
                    float dx=Mathf.Min(proposed.x-r.xMin,r.xMax-proposed.x);
                    float dy=Mathf.Min(proposed.y-r.yMin,r.yMax-proposed.y);
                    if(dx<dy)proposed.x=proposed.x<r.center.x?r.xMin-.001f:r.xMax+.001f;
                    else proposed.y=proposed.y<r.center.y?r.yMin-.001f:r.yMax+.001f;
                }
            }
            return proposed;
        }
        private static Rect Expanded(Rect r,float radius)=>new Rect(r.x-radius,r.y-radius,r.width+radius*2,r.height+radius*2);
    }
}
