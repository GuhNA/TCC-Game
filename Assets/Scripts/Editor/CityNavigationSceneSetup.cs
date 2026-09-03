using TCCGame.Gameplay.Campaign;
using TCCGame.Gameplay.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TCCGame.Editor
{
    /// <summary>
    /// Mantem a primeira versao navegavel das cenas de cidade idempotente. Pode ser
    /// executado pelo menu apos uma alteracao de layout sem recriar NPCs em duplicata.
    /// </summary>
    public static class CityNavigationSceneSetup
    {
        private const string CenterPath = "Assets/Scenes/Centro.unity";
        private const string GatePath = "Assets/Scenes/Portão.unity";

        [MenuItem("TCC Game/City/Configure Navigation Scenes")]
        public static void CreateOrUpdate()
        {
            ConfigureCenter();
            ConfigureGate();
            ConfigureBuildSettings();
            EditorSceneManager.OpenScene(CenterPath, OpenSceneMode.Single);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureCenter()
        {
            EditorSceneManager.OpenScene(CenterPath, OpenSceneMode.Single);
            SpriteRenderer referenceSprite = FindReferenceSprite();
            RemoveGenerated("NavigationMarkers");
            RemoveGenerated("NavigationNPCs");

            GameObject markers = new GameObject("NavigationMarkers");
            CreateMarker(markers.transform, "CenterDefaultSpawn", new Vector2(1.66f, 1.13f));
            CreateMarker(markers.transform, "CenterFromNorthSpawn", new Vector2(0f, 3.15f));
            CreateMarker(markers.transform, "CenterFromEastSpawn", new Vector2(6.2f, 1.1f));
            CreateMarker(markers.transform, "CenterFromSouthSpawn", new Vector2(0f, -2.55f));
            CreateMarker(markers.transform, "CenterFromWestSpawn", new Vector2(-6.2f, 1.1f));

            GameObject npcs = new GameObject("NavigationNPCs");
            CreateTravelNpc(npcs.transform, "Sentinela Norte", new Vector2(0f, 3.55f), referenceSprite, TravelDestinationKind.Gate, GateId.North);
            CreateTravelNpc(npcs.transform, "Sentinela Leste", new Vector2(6.65f, 1.1f), referenceSprite, TravelDestinationKind.Gate, GateId.East);
            CreateTravelNpc(npcs.transform, "Sentinela Sul", new Vector2(0f, -3.1f), referenceSprite, TravelDestinationKind.Gate, GateId.South);
            CreateTravelNpc(npcs.transform, "Sentinela Oeste", new Vector2(-6.65f, 1.1f), referenceSprite, TravelDestinationKind.Gate, GateId.West);

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        private static void ConfigureGate()
        {
            EditorSceneManager.OpenScene(GatePath, OpenSceneMode.Single);
            SpriteRenderer referenceSprite = FindReferenceSprite();
            RemoveGenerated("NavigationMarkers");
            RemoveGenerated("NavigationNPCs");
            RemoveGenerated("NavigationPresentation");
            RemoveGenerated("GarrisonNPCs");

            GameObject markers = new GameObject("NavigationMarkers");
            CreateMarker(markers.transform, "GateEntranceSpawn", new Vector2(-1.6767f, -1.3787f));

            GameObject npcs = new GameObject("NavigationNPCs");
            CreateTravelNpc(npcs.transform, "Batedor do Portão", new Vector2(-5.2f, 1.7f), referenceSprite, TravelDestinationKind.Center, GateId.North);

            GameObject presentation = new GameObject("NavigationPresentation");
            presentation.AddComponent<GateContextPresenter>();

            GameObject garrisonNpcs = new GameObject("GarrisonNPCs");
            CreateCaptainNpc(
                garrisonNpcs.transform,
                new Vector2(3.9f, -1.7f),
                referenceSprite
            );
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(CenterPath, true),
                new EditorBuildSettingsScene(GatePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/Taverna.unity", true)
            };
        }

        private static SpriteRenderer FindReferenceSprite()
        {
            GameObject preferred = GameObject.Find("NPCVenda") ??
                GameObject.Find("TaxCollectorNPC");
            SpriteRenderer preferredRenderer =
                preferred?.GetComponentInChildren<SpriteRenderer>(true);
            if (preferredRenderer != null && preferredRenderer.sprite != null)
            {
                return preferredRenderer;
            }

            foreach (SpriteRenderer renderer in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (renderer.sprite != null)
                {
                    return renderer;
                }
            }

            return null;
        }

        private static void CreateCaptainNpc(
            Transform parent,
            Vector2 position,
            SpriteRenderer referenceSprite
        )
        {
            GameObject npc = new GameObject(
                "Capitão da Guarda",
                typeof(SpriteRenderer),
                typeof(BoxCollider2D),
                typeof(CaptainNpc)
            );
            npc.transform.SetParent(parent, false);
            npc.transform.position = position;

            SpriteRenderer spriteRenderer = npc.GetComponent<SpriteRenderer>();
            if (referenceSprite != null)
            {
                spriteRenderer.sprite = referenceSprite.sprite;
                spriteRenderer.material = referenceSprite.sharedMaterial;
                spriteRenderer.sortingLayerID = referenceSprite.sortingLayerID;
                spriteRenderer.sortingOrder = referenceSprite.sortingOrder + 5;
            }
            spriteRenderer.color = new Color(1f, 0.82f, 0.45f, 1f);

            BoxCollider2D collider = npc.GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.9f, 1.2f);
        }

        private static void CreateMarker(Transform parent, string markerName, Vector2 position)
        {
            GameObject marker = new GameObject(markerName);
            marker.transform.SetParent(parent, false);
            marker.transform.position = position;
        }

        private static void CreateTravelNpc(Transform parent, string npcName, Vector2 position, SpriteRenderer referenceSprite, TravelDestinationKind destination, GateId gateId)
        {
            GameObject npc = new GameObject(npcName, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(CityTravelNpc));
            npc.transform.SetParent(parent, false);
            npc.transform.position = position;

            SpriteRenderer spriteRenderer = npc.GetComponent<SpriteRenderer>();
            if (referenceSprite != null)
            {
                spriteRenderer.sprite = referenceSprite.sprite;
                spriteRenderer.material = referenceSprite.sharedMaterial;
                spriteRenderer.sortingLayerID = referenceSprite.sortingLayerID;
                spriteRenderer.sortingOrder = referenceSprite.sortingOrder + 5;
            }
            spriteRenderer.color = new Color(0.65f, 0.88f, 1f, 1f);

            BoxCollider2D collider = npc.GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.9f, 1.2f);
            npc.GetComponent<CityTravelNpc>().Configure(destination, gateId, npcName);
        }

        private static void RemoveGenerated(string objectName)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }
    }
}
