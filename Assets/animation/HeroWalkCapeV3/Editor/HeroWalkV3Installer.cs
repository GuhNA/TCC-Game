using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

// Review assets only: never modifies the production player or existing scenes.
[InitializeOnLoad]
public static class HeroWalkV3Installer
{
    const string Folder = "Assets/animation/HeroWalkCapeV3";
    const string Sheet = Folder + "/HERO_WALK_36F_v3.png";
    static HeroWalkV3Installer() { EditorApplication.delayCall += InstallOnce; }
    static void InstallOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        if (!File.Exists(Sheet) || File.Exists(Folder + "/HeroWalkV3_Review.prefab")) return;
        Install();
    }
    [MenuItem("TCC Game/Animation Tests/Prepare Hero Walk V3")]
    public static void Install()
    {
        var importer = AssetImporter.GetAtPath(Sheet) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 2048;
        var names = new[] { "WalkUp", "WalkDown", "WalkRight" };
        var slices = new SpriteMetaData[36];
        for (int i=0; i<36; i++) slices[i] = new SpriteMetaData {
            name = names[i/12]+"_"+(i%12).ToString("00"),
            rect = new Rect(i%6*326,(5-i/6)*301,326,301),
            alignment = (int)SpriteAlignment.Custom, pivot = new Vector2(.5f,16f/301f)
        };
#pragma warning disable CS0618
        importer.spritesheet = slices;
#pragma warning restore CS0618
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().ToArray();
        if (sprites.Length!=36) throw new InvalidOperationException("Expected 36 sprites");
        var controllerPath = Folder+"/HeroWalkV3_Review.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller==null) controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var machine=controller.layers[0].stateMachine;
        foreach(var name in names) {
            var clipPath=Folder+"/"+name+".anim";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip==null) { clip=new AnimationClip {name=name,frameRate=11}; AssetDatabase.CreateAsset(clip,clipPath); }
            var frames=sprites.Where(s=>s.name.StartsWith(name+"_")).OrderBy(s=>s.name).ToArray();
            var keys=new ObjectReferenceKeyframe[13];
            for(int i=0;i<13;i++) keys[i]=new ObjectReferenceKeyframe { time=i/11f,value=frames[i%12] };
            AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),keys);
            var settings=AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime=true; settings.stopTime=12f/11f;
            AnimationUtility.SetAnimationClipSettings(clip,settings);
            var state=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name==name) ?? machine.AddState(name);
            state.motion=clip; if(name=="WalkRight") machine.defaultState=state;
            EditorUtility.SetDirty(clip);
        }
        var actor=new GameObject("HeroWalkV3_Review");
        try {
            actor.AddComponent<SpriteRenderer>().sprite=sprites.First(s=>s.name=="WalkRight_00");
            var animator=actor.AddComponent<Animator>(); animator.runtimeAnimatorController=controller; animator.applyRootMotion=false;
            PrefabUtility.SaveAsPrefabAsset(actor,Folder+"/HeroWalkV3_Review.prefab");
        } finally { UnityEngine.Object.DestroyImmediate(actor); }
        AssetDatabase.SaveAssets();
        Debug.Log("Hero Walk V3: 36 sprites, 3 clips at 11 FPS, isolated review prefab ready. Visual playback not certified.");
    }
}
