// Put this file in Assets/Scripts/Editor, then select TCC Game/Effects/Rebuild Attention Exclamation.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AttentionExclamationInstaller
{
    private const string SheetPath = "Assets/Images/Effects/Attention/attention_exclamation_sequence.png";
    private const string OutputFolder = "Assets/Animation/Attention";
    private const string PrefabPath = "Assets/Prefabs/AttentionExclamation.prefab";

    [MenuItem("TCC Game/Effects/Rebuild Attention Exclamation")]
    public static void Rebuild()
    {
        EnsureFolder("Assets/Images/Effects");
        EnsureFolder("Assets/Images/Effects/Attention");
        EnsureFolder("Assets/Animation");
        EnsureFolder(OutputFolder);
        EnsureFolder("Assets/Prefabs");

        var sprites = SliceNineFrames();
        if (sprites.Count != 9)
        {
            Debug.LogError("Attention exclamation: the sprite sheet was not found at " + SheetPath);
            return;
        }

        var clip = CreateClip(OutputFolder + "/AttentionExclamation.anim", sprites);
        var controller = CreateController(OutputFolder + "/AttentionExclamation.controller", clip);
        BuildPrefab(sprites[0], controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Attention exclamation created. Use the AttentionExclamation prefab where the alert should appear.");
    }

    private static List<Sprite> SliceNineFrames()
    {
        var importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath);
        if (importer == null || texture == null)
            return new List<Sprite>();

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100f;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;

        var metadata = new List<SpriteMetaData>();
        for (var row = 0; row < 3; row++)
        {
            for (var column = 0; column < 3; column++)
            {
                var xMin = column * texture.width / 3;
                var xMax = (column + 1) * texture.width / 3;
                var yMin = texture.height - (row + 1) * texture.height / 3;
                var yMax = texture.height - row * texture.height / 3;
                metadata.Add(new SpriteMetaData
                {
                    name = "Attention_" + (row * 3 + column).ToString("00"),
                    rect = Rect.MinMaxRect(xMin, yMin, xMax, yMax),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });
            }
        }

#pragma warning disable CS0618
        importer.spritesheet = metadata.ToArray();
#pragma warning restore CS0618
        importer.SaveAndReimport();

        return AssetDatabase.LoadAllAssetsAtPath(SheetPath)
            .OfType<Sprite>()
            .Where(sprite => sprite.name.StartsWith("Attention_"))
            .OrderBy(sprite => sprite.name)
            .ToList();
    }

    private static AnimationClip CreateClip(string path, List<Sprite> sprites)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null)
            AssetDatabase.DeleteAsset(path);

        const float framesPerSecond = 8f;
        var clip = new AnimationClip { frameRate = framesPerSecond };
        // The final empty key makes the alert disappear after the third pop.
        var frames = new ObjectReferenceKeyframe[sprites.Count + 1];
        for (var i = 0; i < sprites.Count; i++)
        {
            frames[i] = new ObjectReferenceKeyframe { time = i / framesPerSecond, value = sprites[i] };
        }
        frames[sprites.Count] = new ObjectReferenceKeyframe { time = sprites.Count / framesPerSecond, value = null };

        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = string.Empty,
            propertyName = "m_Sprite"
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static RuntimeAnimatorController CreateController(string path, AnimationClip clip)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null)
            AssetDatabase.DeleteAsset(path);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        var state = controller.layers[0].stateMachine.AddState("AttentionLoop");
        state.motion = clip;
        controller.layers[0].stateMachine.defaultState = state;
        return controller;
    }

    private static void BuildPrefab(Sprite firstFrame, RuntimeAnimatorController controller)
    {
        var root = new GameObject("AttentionExclamation");
        try
        {
            // A good in-world default; scale the prefab in the scene if this alert is UI-sized.
            root.transform.localScale = Vector3.one * 0.35f;
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = firstFrame;
            renderer.sortingOrder = 20;
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return;

        var parent = System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/');
        var child = System.IO.Path.GetFileName(folder);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, child);
    }
}
