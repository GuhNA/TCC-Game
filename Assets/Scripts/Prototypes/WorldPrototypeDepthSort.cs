using UnityEngine;

namespace TCCGame.Prototypes
{
    // Shared depth convention for characters and the existing Centro scenery.
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class WorldPrototypeDepthSort : MonoBehaviour
    {
        public SpriteRenderer Target;
        public Collider2D Feet;

        [Tooltip("Characters use the fixed collider offset; scenery keeps its footprint's lower edge.")]
        public bool UseFeetCenter;

        [Tooltip("A one-step player offset gives equal-height characters a stable drawing order.")]
        public int OrderOffset;

        public Vector3 GroundPosition => Feet == null ? transform.position
            : UseFeetCenter ? GroundCenter(Feet)
            : new Vector3(Feet.bounds.center.x, Feet.bounds.min.y, Feet.transform.position.z);

        // Collider offsets stay fixed when animation changes the sprite bounds.
        public static Vector3 GroundCenter(Collider2D feet)
            => feet.transform.TransformPoint(feet.offset);

        public static int OrderAt(float groundY, int offset = 0)
            => 2000 - Mathf.RoundToInt(groundY * 100) + offset;

        public void Refresh()
        {
            if (Target != null && Feet != null)
                Target.sortingOrder = OrderAt(GroundPosition.y, OrderOffset);
        }

        private void OnEnable() => Refresh();
        private void LateUpdate() => Refresh();
    }
}
