using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.Animations;
using UnityEngine.Playables;

// Non-destructive: reads production clips and creates separate timing studies.
public static class WalkCadenceAudit
{
    const string Folder = "Assets/animation/WalkRightReview/CadenceStudy";
    [MenuItem("TCC Game/Animation Tests/Audit All Player Clips - Create Timing Study")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        var report = new Report();
        var controllerPath = Folder + "/Player_TimingStudy.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var machine = controller.layers[0].stateMachine;
        foreach (var action in new[] { "idle", "walk" })
        foreach (var direction in new[] { "down", "right", "up" })
        {
            string name = action + "_" + direction;
            string sourcePath = "Assets/animation/Base/" + (action == "walk" ? "Walk" : "idle") + "/" + name + ".anim";
            var original = AssetDatabase.LoadAssetAtPath<AnimationClip>(sourcePath);
            if (original == null) throw new InvalidOperationException("Missing clip " + sourcePath);
            var binding = AnimationUtility.GetObjectReferenceCurveBindings(original).Single(b => b.type == typeof(SpriteRenderer) && b.propertyName == "m_Sprite");
            var keys = AnimationUtility.GetObjectReferenceCurve(original, binding);
            if (keys.Length == 0 || keys.Any(k => !(k.value is Sprite))) throw new InvalidOperationException("Missing sprite in " + name);
            string clonePath = Folder + "/" + name + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clonePath);
            if (clip == null) { clip = UnityEngine.Object.Instantiate(original); clip.name = name; AssetDatabase.CreateAsset(clip, clonePath); }
            float fps = action == "walk" ? 11 : original.frameRate;
            clip.frameRate = fps;
            if (action == "walk") for (int i = 0; i < keys.Length; i++) keys[i].time = i / fps;
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            settings.startTime = 0;
            settings.stopTime = keys[keys.Length - 1].time + 1f / fps;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == name) ?? machine.AddState(name);
            state.motion = clip;
            if (name == "walk_right") machine.defaultState = state;
            var entry = new Entry { name = name, source = sourcePath, originalFPS = original.frameRate, studyFPS = fps, frames = keys.Length, duration = settings.stopTime, transformCurves = AnimationUtility.GetCurveBindings(clip).Count(b => b.type == typeof(Transform)), artisticApproval = false };
            var actor = new GameObject("Temporary_Clip_Audit", typeof(SpriteRenderer), typeof(Animator));
            actor.hideFlags = HideFlags.HideAndDontSave;
            var animator = actor.GetComponent<Animator>(); animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var renderer = actor.GetComponent<SpriteRenderer>();
            var graph = PlayableGraph.Create("TimingAudit_" + name);
            try {
                var playable = AnimationClipPlayable.Create(graph, clip);
                AnimationPlayableOutput.Create(graph, name, animator).SetSourcePlayable(playable);
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual); graph.Play();
                var observed = new HashSet<string>(); bool stable = true;
                for (int i = 0; i < keys.Length * 3; i++) {
                    playable.SetTime((i % keys.Length + .25) / fps); graph.Evaluate(0);
                    if (renderer.sprite != null) observed.Add(renderer.sprite.name);
                    stable &= actor.transform.position == Vector3.zero && actor.transform.localScale == Vector3.one && actor.transform.rotation == Quaternion.identity;
                }
                entry.observedSprites = observed.Count; entry.rootStable = stable;
                entry.spriteGeometryConsistent = keys.Select(k => (Sprite)k.value).All(s => s.rect.size == ((Sprite)keys[0].value).rect.size && s.pivot == ((Sprite)keys[0].value).pivot && s.pixelsPerUnit == ((Sprite)keys[0].value).pixelsPerUnit);
            } finally { graph.Destroy(); UnityEngine.Object.DestroyImmediate(actor); }
            report.clips.Add(entry);
        }
        EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets();
        File.WriteAllText(Folder + "/audit.json", JsonUtility.ToJson(report, true));
        AssetDatabase.Refresh(); Selection.activeObject = controller;
        Debug.Log("Timing audit saved. Production assets unchanged. Art NOT approved. " + Folder);
    }
    [Serializable] class Report { public string status = "TECHNICAL STUDY ONLY - ART NOT APPROVED"; public List<Entry> clips = new List<Entry>(); }
    [Serializable] class Entry { public string name, source; public float originalFPS, studyFPS, duration; public int frames, observedSprites, transformCurves; public bool rootStable, spriteGeometryConsistent, artisticApproval; }
}

public sealed class PlayerCadencePreview : EditorWindow
{
    readonly string[] names = { "walk_right", "walk_down", "walk_up", "idle_right", "idle_down", "idle_up" };
    int selected;
    bool original, running;
    double previous;
    float time;
    GameObject actor;
    AnimationClip clip;
    PlayableGraph graph;
    AnimationClipPlayable playable;
    [MenuItem("TCC Game/Animation Tests/Compare Original and 11 FPS")]
    static void Open() { var w = GetWindow<PlayerCadencePreview>("Player Cadence"); w.minSize = new Vector2(450,480); w.Load(); w.Show(); }
    void OnEnable() { EditorApplication.update += Tick; }
    void OnDisable() { EditorApplication.update -= Tick; Dispose(); }
    void Dispose() { running = false; if (graph.IsValid()) graph.Destroy(); if (actor != null) DestroyImmediate(actor); }
    void Load()
    {
        Dispose(); time = 0;
        string name = names[selected];
        string path = original ? "Assets/animation/Base/" + (name.StartsWith("walk") ? "Walk" : "idle") + "/" + name + ".anim" : "Assets/animation/WalkRightReview/CadenceStudy/" + name + ".anim";
        clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null) return;
        actor = new GameObject("Temporary_Cadence_Preview", typeof(SpriteRenderer), typeof(Animator));
        actor.hideFlags = HideFlags.HideAndDontSave;
        actor.transform.position = new Vector3(2000,2000,0);
        var animator = actor.GetComponent<Animator>(); animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        graph = PlayableGraph.Create("Player_Cadence_Preview"); graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        playable = AnimationClipPlayable.Create(graph, clip);
        AnimationPlayableOutput.Create(graph,"SpriteRenderer",animator).SetSourcePlayable(playable);
        graph.Play(); graph.Evaluate(0); previous = EditorApplication.timeSinceStartup;
    }
    void Tick()
    {
        double now = EditorApplication.timeSinceStartup; float dt = Mathf.Min((float)(now-previous),.1f); previous = now;
        if (!running || !graph.IsValid()) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) { Dispose(); return; }
        time = Mathf.Repeat(time + dt, clip.length); playable.SetTime(time); graph.Evaluate(0); Repaint();
    }
    void OnGUI()
    {
        EditorGUILayout.LabelField("Comparacao tecnica — arte ainda nao aprovada", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck(); selected = EditorGUILayout.Popup("Animacao", selected, names); original = EditorGUILayout.Toggle("Usar clip original", original);
        if (EditorGUI.EndChangeCheck()) Load();
        if (clip == null || actor == null) { if (GUILayout.Button("Carregar")) Load(); return; }
        EditorGUILayout.LabelField(clip.frameRate + " FPS | " + clip.length.ToString("0.000") + " s | pivot do sprite preservado");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(running ? "Pausar" : "Reproduzir")) { running = !running; previous = EditorApplication.timeSinceStartup; }
        if (GUILayout.Button("Proximo frame")) { running = false; time = Mathf.Repeat(time+1f/clip.frameRate,clip.length); playable.SetTime(time); graph.Evaluate(0); }
        GUILayout.EndHorizontal();
        var sprite = actor.GetComponent<SpriteRenderer>().sprite;
        EditorGUILayout.LabelField("Sprite", sprite != null ? sprite.name : "NENHUM");
        var area = GUILayoutUtility.GetRect(326,301,GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));
        EditorGUI.DrawRect(area,new Color(.17f,.19f,.23f));
        if (sprite != null) {
            var rect = sprite.rect; var tex = sprite.texture; float scale = Mathf.Min(area.width/rect.width,area.height/rect.height);
            var target = new Rect(area.center.x-rect.width*scale/2,area.center.y-rect.height*scale/2,rect.width*scale,rect.height*scale);
            GUI.DrawTextureWithTexCoords(target,tex,new Rect(rect.x/tex.width,rect.y/tex.height,rect.width/tex.width,rect.height/tex.height),true);
            float ground = target.y+(rect.height-sprite.pivot.y)*scale;
            EditorGUI.DrawRect(new Rect(target.x,ground,target.width,1),Color.green);
        }
        EditorGUILayout.HelpBox("Reproducao real via Animator e SpriteRenderer. Apenas a cadencia muda; este estudo nao corrige as poses desenhadas. Nenhuma cena original e salva.",MessageType.Info);
    }
}
