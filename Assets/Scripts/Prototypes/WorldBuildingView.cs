using TCCGame.Core;
using UnityEngine;

namespace TCCGame.Prototypes
{
    // Presentation only; economic rules and physical footprints remain owned by existing systems.
    public class WorldBuildingView : MonoBehaviour
    {
        public BuildingType Kind = BuildingType.Tavern;
        public SpriteRenderer Facade;
        public SpriteRenderer Placeholder;
        public SpriteRenderer Foundation;
        public Collider2D Footprint;
        public Collider2D Player;
        public bool FadeBehindRoof = true;
        public Rect Courtyard;
        [System.NonSerialized] public int PreviewLevel = -1;

        public void Refresh()
        {
            bool built = Application.isPlaying
                ? GameSessionProvider.Current.Buildings.GetLevel(Kind) > 0
                : PreviewLevel != 0;
            Facade.enabled = built;
            if (Foundation != null) Foundation.enabled = !built;
            Placeholder.enabled = !built && Foundation == null;
            float playerGroundY = Player != null ? WorldPrototypeDepthSort.GroundCenter(Player).y : 0;
            bool inside = Player != null && Courtyard.Contains(WorldPrototypeDepthSort.GroundCenter(Player));
            float sortY = inside ? playerGroundY + .02f : Footprint.bounds.min.y;
            Facade.sortingOrder = WorldPrototypeDepthSort.OrderAt(sortY);
            bool behindRoof = !inside && FadeBehindRoof && Player != null &&
                playerGroundY > Footprint.bounds.max.y - .05f && Facade.bounds.Intersects(Player.bounds);
            Facade.color = new Color(1, 1, 1, behindRoof ? .30f : 1);
        }
        protected virtual void LateUpdate() => Refresh();
    }
}
