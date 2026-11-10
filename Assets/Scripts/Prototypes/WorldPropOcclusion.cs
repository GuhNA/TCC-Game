using UnityEngine;

namespace TCCGame.Prototypes
{
    public sealed class WorldPropOcclusion:MonoBehaviour
    {
        public SpriteRenderer Target;
        public Collider2D Footprint;
        public Collider2D Player;
        public void Refresh()
        {
            bool behind=Player!=null&&WorldPrototypeDepthSort.GroundCenter(Player).y>Footprint.bounds.max.y-.05f&&Target.bounds.Intersects(Player.bounds);
            Target.color=new Color(1,1,1,behind ? .35f : 1);
        }
        private void LateUpdate()=>Refresh();
    }
}
