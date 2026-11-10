#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TCCGame.Editor
{
    /// <summary>Exports planning measurements without saving a scene or running gameplay.</summary>
    public static class CentroLayoutSurvey
    {
        [Serializable] private sealed class Measurements
        {
            public string scene;
            public float playerHeight, playerWidth, playerCollisionWidth, movementSpeed;
            public string playerTexture;
            public Vector2 textureSize;
            public Rect playerSpriteRect;
            public float spritePixelsPerUnit;
            public Rect groundBounds;
            public float estimatedClearGroundArea;
            public BuildingMeasurement[] buildings;
        }
        [Serializable] private sealed class BuildingMeasurement
        {
            public string name;
            public Vector3 position, scale;
            public Vector2 spriteSize, collisionSize;
        }

        [MenuItem("TCC Game/World/Survey Centro Layout")]
        public static void Generate()
        {
            const string scenePath = "Assets/Scenes/Centro.unity";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerInteractor>();
            var renderer = player.GetComponentInChildren<SpriteRenderer>();
            var collider = player.GetComponent<Collider2D>();
            var sprite = renderer.sprite;
            var background = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                .First(x => x.name.StartsWith("Centro Background", StringComparison.Ordinal));
            Bounds ground = background.bounds;
            var blockers = UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None)
                .Where(x => x.enabled && !x.isTrigger && !x.transform.IsChildOf(player.transform)).ToArray();
            const float step = .05f;
            int clear = 0;
            for (float x = ground.min.x + step / 2; x < ground.max.x; x += step)
                for (float y = ground.min.y + step / 2; y < ground.max.y; y += step)
                    if (!blockers.Any(blocker => blocker.OverlapPoint(new Vector2(x, y)))) clear++;
            var data = new Measurements
            {
                scene = scenePath,
                playerHeight = renderer.bounds.size.y,
                playerWidth = renderer.bounds.size.x,
                playerCollisionWidth = collider.bounds.size.x,
                movementSpeed = player.GetComponent<moviment>().velocity,
                playerTexture = AssetDatabase.GetAssetPath(sprite.texture),
                textureSize = new Vector2(sprite.texture.width, sprite.texture.height),
                playerSpriteRect = sprite.rect,
                spritePixelsPerUnit = sprite.pixelsPerUnit,
                groundBounds = new Rect(ground.min.x, ground.min.y, ground.size.x, ground.size.y),
                estimatedClearGroundArea = clear * step * step,
                buildings = UnityEngine.Object.FindObjectsByType<BuildingBehaviour>(FindObjectsSortMode.None)
                    .Select(building => new BuildingMeasurement
                    {
                        name = building.Definition.displayName,
                        position = building.transform.position,
                        scale = building.transform.lossyScale,
                        spriteSize = building.GetComponent<SpriteRenderer>().bounds.size,
                        collisionSize = building.GetComponent<Collider2D>().bounds.size
                    }).ToArray()
            };
            Directory.CreateDirectory("Design/World");
            File.WriteAllText("Design/World/CentroCurrentMeasurements.json", JsonUtility.ToJson(data, true));
            Debug.Log("Centro planning measurements exported. No scene or save modified.");
        }
    }
}
#endif
