// Put this file in Assets/Scripts/Editor, then select TCC Game/Tavern/Rebuild Fire Effects.
// The generated prefab is intended to be a child of the GameObject that displays taverna.png.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class TavernFireEffectsInstaller
{
    private const string TavernTexturePath = "Assets/Images/Buildings/taverna.png";
    private const string FireplaceSheetPath = "Assets/Images/Effects/Tavern/tavern_fireplace_loop.png";
    private const string CandleSheetPath = "Assets/Images/Effects/Tavern/tavern_candle_loop.png";
    private const string LanternSheetPath = "Assets/Images/Effects/Tavern/tavern_lantern_loop.png";
    private const string OutputFolder = "Assets/Animation/TavernFireEffects";
    private const string PrefabPath = "Assets/Prefabs/TavernFireEffects.prefab";

    // Native dimensions of taverna.png. Keeping positions in source pixels makes the prefab
    // follow the background exactly when it is parented to the taverna SpriteRenderer.
    private const float TavernWidth = 1317f;
    private const float TavernHeight = 1194f;

    [MenuItem("TCC Game/Tavern/Rebuild Fire Effects")]
    public static void Rebuild()
    {
        EnsureFolder("Assets/Images/Effects");
        EnsureFolder("Assets/Images/Effects/Tavern");
        EnsureFolder("Assets/Animation");
        EnsureFolder(OutputFolder);
        EnsureFolder("Assets/Prefabs");

        var fireplace = SliceFourFrames(FireplaceSheetPath, "TavernFireplace");
        var candle = SliceFourFrames(CandleSheetPath, "TavernCandle");
        var lantern = SliceFourFrames(LanternSheetPath, "TavernLantern");

        if (fireplace.Count != 4 || candle.Count != 4 || lantern.Count != 4)
        {
            Debug.LogError("Tavern fire effects: one or more sprite sheets could not be imported. Confirm that the three PNG files are in Assets/Images/Effects/Tavern.");
            return;
        }

        var fireplaceClip = CreateClip(OutputFolder + "/TavernFireplace.anim", fireplace, 7f);
        var candleClip = CreateClip(OutputFolder + "/TavernCandle.anim", candle, 6f);
        var lanternClip = CreateClip(OutputFolder + "/TavernLantern.anim", lantern, 5f);

        var fireplaceController = CreateController(OutputFolder + "/TavernFireplace.controller", fireplaceClip);
        var candleController = CreateController(OutputFolder + "/TavernCandle.controller", candleClip);
        var lanternController = CreateController(OutputFolder + "/TavernLantern.controller", lanternClip);

        BuildPrefab(fireplace, candle, lantern, fireplaceController, candleController, lanternController);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Tavern fire effects created. Drag the TavernFireEffects prefab onto the object that displays taverna.png, then reset its local position to (0, 0, 0).");
    }

    private static List<Sprite> SliceFourFrames(string path, string namePrefix)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("Tavern fire effects: missing texture at " + path);
            return new List<Sprite>();
        }

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null)
            return new List<Sprite>();

        int cellWidth = texture.width / 2;
        int cellHeight = texture.height / 2;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = GetTavernPixelsPerUnit();
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;

#pragma warning disable CS0618
        importer.spritesheet = new[]
        {
            SpriteMeta(namePrefix + "_0", new Rect(0, cellHeight, cellWidth, cellHeight)),
            SpriteMeta(namePrefix + "_1", new Rect(cellWidth, cellHeight, cellWidth, cellHeight)),
            SpriteMeta(namePrefix + "_2", new Rect(0, 0, cellWidth, cellHeight)),
            SpriteMeta(namePrefix + "_3", new Rect(cellWidth, 0, cellWidth, cellHeight))
        };
#pragma warning restore CS0618

        importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .Where(sprite => sprite.name.StartsWith(namePrefix))
            .OrderBy(sprite => sprite.name)
            .ToList();
    }

    private static SpriteMetaData SpriteMeta(string name, Rect rect)
    {
        return new SpriteMetaData
        {
            name = name,
            rect = rect,
            alignment = (int)SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f)
        };
    }

    private static AnimationClip CreateClip(string path, List<Sprite> sprites, float framesPerSecond)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null)
            AssetDatabase.DeleteAsset(path);

        var clip = new AnimationClip { frameRate = framesPerSecond };
        var frames = new ObjectReferenceKeyframe[sprites.Count];
        for (var i = 0; i < sprites.Count; i++)
        {
            frames[i] = new ObjectReferenceKeyframe
            {
                time = i / framesPerSecond,
                value = sprites[i]
            };
        }

        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = string.Empty,
            propertyName = "m_Sprite"
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
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
        var state = controller.layers[0].stateMachine.AddState("Loop");
        state.motion = clip;
        controller.layers[0].stateMachine.defaultState = state;
        return controller;
    }

    private static void BuildPrefab(
        List<Sprite> fireplace,
        List<Sprite> candle,
        List<Sprite> lantern,
        RuntimeAnimatorController fireplaceController,
        RuntimeAnimatorController candleController,
        RuntimeAnimatorController lanternController)
    {
        var root = new GameObject("TavernFireEffects");
        try
        {
            AddEffect(root.transform, "Fireplace", fireplace[0], fireplaceController, new Vector2(849f, 323f), 0.25f);
            AddEffect(root.transform, "Candle_UpperLeft", candle[0], candleController, new Vector2(665f, 192f), 0.065f);
            AddEffect(root.transform, "Candle_UpperRight", candle[0], candleController, new Vector2(981f, 192f), 0.065f);
            AddEffect(root.transform, "Candle_LowerLeft", candle[0], candleController, new Vector2(112f, 803f), 0.065f);
            AddEffect(root.transform, "Candle_LowerRight", candle[0], candleController, new Vector2(1200f, 884f), 0.065f);

            // The two requested lantern markers point at the railing above them; these are
            // anchored to the actual glass centres of the lamps directly underneath.
            AddEffect(root.transform, "Lantern_Left", lantern[0], lanternController, new Vector2(326f, 1054f), 0.10f);
            AddEffect(root.transform, "Lantern_Right", lantern[0], lanternController, new Vector2(982f, 1054f), 0.10f);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static void AddEffect(Transform parent, string effectName, Sprite firstFrame, RuntimeAnimatorController controller, Vector2 imagePosition, float scale)
    {
        var effect = new GameObject(effectName);
        effect.transform.SetParent(parent, false);
        effect.transform.localPosition = new Vector3(
            (imagePosition.x - TavernWidth * 0.5f) / GetTavernPixelsPerUnit(),
            (TavernHeight * 0.5f - imagePosition.y) / GetTavernPixelsPerUnit(),
            -0.01f);
        effect.transform.localScale = Vector3.one * scale;

        var renderer = effect.AddComponent<SpriteRenderer>();
        renderer.sprite = firstFrame;
        renderer.sortingOrder = 1;
        var animator = effect.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
    }

    private static float GetTavernPixelsPerUnit()
    {
        var importer = AssetImporter.GetAtPath(TavernTexturePath) as TextureImporter;
        return importer != null ? importer.spritePixelsPerUnit : 100f;
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
