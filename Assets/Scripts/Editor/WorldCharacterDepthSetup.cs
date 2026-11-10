#if UNITY_EDITOR
using System;
using System.Linq;
using TCCGame.Gameplay.Campaign;
using TCCGame.Gameplay.Navigation;
using TCCGame.Prototypes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TCCGame.Editor
{
    public static class WorldCharacterDepthSetup
    {
        [MenuItem("TCC Game/World/Apply Character Depth and NPC Physics")]
        public static void Apply()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string name in new[] { "Arqueiro", "Bandido", "Guerreiro", "Mago" })
                {
                    string path = $"Assets/Prefabs/Aventureiros/Contratação/{name}.prefab";
                    var prefab = PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        ConfigureActor(prefab);
                        PrefabUtility.SaveAsPrefabAsset(prefab, path);
                    }
                    finally { PrefabUtility.UnloadPrefabContents(prefab); }
                }

                foreach (string name in new[] { "Centro", "Portão", "Taverna" })
                {
                    var scene = EditorSceneManager.OpenScene($"Assets/Scenes/{name}.unity");
                    ConfigureScene(scene);
                    EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
                Debug.Log("Character depth and stationary vendor physics applied to Centro, Portão and Taverna.");
            }
            finally
            {
                if (previous.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }

        internal static void ConfigureScene(Scene scene)
        {
            var actors = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                .Where(component => component is PlayerInteractor || component is CityTravelNpc ||
                    component is TaxCollectorNpc || component is NPCVenda ||
                    component is CaptainNpc || component is Aventureiro)
                .Select(component => component.gameObject).Distinct().ToArray();
            foreach (var actor in actors) ConfigureActor(actor);
        }

        internal static void ConfigureActor(GameObject actor)
        {
            var sprite = actor.GetComponent<SpriteRenderer>() ?? actor.GetComponentsInChildren<SpriteRenderer>(true)
                .OrderByDescending(renderer => renderer.sprite == null ? 0 : renderer.bounds.size.sqrMagnitude)
                .FirstOrDefault();
            var feet = actor.GetComponents<Collider2D>().FirstOrDefault(collider => collider.enabled && !collider.isTrigger);
            if (sprite == null || feet == null)
                throw new InvalidOperationException($"{actor.name} needs a body sprite and a solid feet collider for depth sorting.");
            var depth = actor.GetComponent<WorldPrototypeDepthSort>() ?? actor.AddComponent<WorldPrototypeDepthSort>();
            depth.Target = sprite;
            depth.Feet = feet;
            depth.UseFeetCenter = true;
            depth.OrderOffset = actor.GetComponent<PlayerInteractor>() != null ? 1 : 0;
            sprite.sortingLayerID = SortingLayer.NameToID("Default");
            depth.Refresh();
            EditorUtility.SetDirty(depth);
            EditorUtility.SetDirty(sprite);
            PrefabUtility.RecordPrefabInstancePropertyModifications(depth);
            PrefabUtility.RecordPrefabInstancePropertyModifications(sprite);

            if (actor.GetComponent<NPCVenda>() != null) ConfigureStationaryVendor(actor);
        }

        internal static void ConfigureStationaryVendor(GameObject actor)
        {
            var body = actor.GetComponent<Rigidbody2D>();
            if (body == null) return;
            body.bodyType = RigidbodyType2D.Static;
            body.simulated = true;
            EditorUtility.SetDirty(body);
            PrefabUtility.RecordPrefabInstancePropertyModifications(body);
        }
    }
}
#endif
