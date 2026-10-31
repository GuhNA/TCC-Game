using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

// Isolated review only. Never changes existing player assets or saves existing scenes.
public sealed class WalkRightReviewWindow : EditorWindow
{
    const string Folder = "Assets/animation/WalkRightReview";
    const string Sheet = Folder + "/RIGHT_CANDIDATE_NOT_APPROVED.png";
    const string ScenePath = Folder + "/WalkRight_Test.unity";
    const float Fps = 11f;
    const float Duration = 12f / Fps;
    GameObject actor;
    SpriteRenderer renderer2d;
    Animator animator;
    AnimatorController controller;
    PlayableGraph graph;
    AnimatorControllerPlayable playable;
    bool playing, reported;
    double previousTime;
    float elapsed, speed = 1f;
    readonly HashSet<string> seen = new HashSet<string>();
    Vector3 initialPosition;
    Vector3 initialScale;
    Quaternion initialRotation;
    bool rootStable = true;

    [MenuItem("TCC Game/Animation Tests/Walk Right - Open Review")]
    public static void OpenReview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        { Debug.LogWarning("Stop Play Mode before opening the isolated walk review."); return; }
        var window = GetWindow<WalkRightReviewWindow>("Walk Right Review");
        window.minSize = new Vector2(420, 540);
        window.Setup();
        window.Show();
    }

    void Setup()
    {
        StopGraph();
        var importer = AssetImporter.GetAtPath(Sheet) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing review sheet: " + Sheet);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Sheet);
        if (texture.width != 1956 || texture.height != 602)
            throw new InvalidOperationException("Expected 1956x602 study sheet.");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 2048;
        var slices = new SpriteMetaData[12];
        for (int i = 0; i < 12; i++)
            slices[i] = new SpriteMetaData {
                name = "WalkRight_" + i.ToString("00"),
                rect = new Rect(i % 6 * 326, (1 - i / 6) * 301, 326, 301),
                alignment = (int)SpriteAlignment.Custom,
                pivot = new Vector2(.5f, 16f / 301f)
            };
#pragma warning disable CS0618
        importer.spritesheet = slices;
#pragma warning restore CS0618
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (sprites.Length != 12) throw new InvalidOperationException("Expected 12 imported sprites.");
        string clipPath = Folder + "/WalkRight_Candidate.anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null) {
            clip = new AnimationClip { name = "WalkRight_Candidate", frameRate = Fps };
            var keys = sprites.Select((s, i) => new ObjectReferenceKeyframe { time = i / Fps, value = s }).ToArray();
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, clipPath);
        }
        string controllerPath = Folder + "/WalkRight_Test.controller";
        controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null) {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var state = controller.layers[0].stateMachine.AddState("WalkRight_Candidate");
            state.motion = clip;
            controller.layers[0].stateMachine.defaultState = state;
        }
        var previousScene = SceneManager.GetActiveScene();
        var testScene = SceneManager.GetSceneByPath(ScenePath);
        bool newScene = !testScene.IsValid() || !testScene.isLoaded;
        if (newScene) {
            testScene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        }
        actor = testScene.GetRootGameObjects().FirstOrDefault(o => o.name == "WalkRight_Review_ONLY");
        if (actor == null) {
            actor = new GameObject("WalkRight_Review_ONLY");
            SceneManager.MoveGameObjectToScene(actor, testScene);
            actor.transform.position = new Vector3(1000, 1000, 0);
            renderer2d = actor.AddComponent<SpriteRenderer>();
            renderer2d.sprite = sprites[0];
            animator = actor.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            if (!File.Exists(Folder + "/WalkRight_Test.prefab"))
                PrefabUtility.SaveAsPrefabAsset(actor, Folder + "/WalkRight_Test.prefab");
            EditorSceneManager.SaveScene(testScene, ScenePath);
        }
        renderer2d = actor.GetComponent<SpriteRenderer>();
        animator = actor.GetComponent<Animator>();
        if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
        AssetDatabase.SaveAssets();
        initialPosition = actor.transform.position;
        initialScale = actor.transform.localScale;
        initialRotation = actor.transform.rotation;
        Selection.activeGameObject = actor;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.Frame(new Bounds(actor.transform.position + Vector3.up * 1.3f, new Vector3(4, 3.5f, 1)), false);
        CreateGraph();
        playing = false;
        Debug.Log("WalkRight review ready: isolated SpriteRenderer + Animator, 12 sprites, 11 FPS. Art remains NOT APPROVED.", actor);
    }

    void CreateGraph()
    {
        graph = PlayableGraph.Create("WalkRight_Isolated_Animator_Review");
        graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        playable = AnimatorControllerPlayable.Create(graph, controller);
        AnimationPlayableOutput.Create(graph, "SpriteRenderer", animator).SetSourcePlayable(playable);
        graph.Play();
        playable.Play("WalkRight_Candidate", 0, 0);
        graph.Evaluate(0);
        elapsed = 0; seen.Clear(); reported = false; rootStable = true;
        previousTime = EditorApplication.timeSinceStartup;
    }
    void OnEnable() { EditorApplication.update += Tick; }
    void OnDisable() { EditorApplication.update -= Tick; StopGraph(); }
    void StopGraph() { playing = false; if (graph.IsValid()) graph.Destroy(); }
    void Tick()
    {
        double now = EditorApplication.timeSinceStartup;
        float dt = Mathf.Min((float)(now - previousTime), .1f); previousTime = now;
        if (!playing || !graph.IsValid() || actor == null) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) { StopGraph(); return; }
        graph.Evaluate(dt * speed); elapsed += dt * speed;
        if (renderer2d.sprite != null) seen.Add(renderer2d.sprite.name);
        rootStable &= actor.transform.position == initialPosition && actor.transform.rotation == initialRotation && actor.transform.localScale == initialScale;
        if (elapsed >= Duration * 3 && !reported) {
            reported = true;
            var report = new TechnicalReport { inspectedInEditor = true, playback = "AnimatorControllerPlayable -> Animator -> SpriteRenderer (Edit Mode)",
                distinctSprites = seen.Count, observedSprites = seen.OrderBy(s => s).ToArray(), secondsEvaluated = elapsed,
                rootTransformUnchanged = rootStable, clipLength = Duration, fps = Fps, artisticApproval = false };
            File.WriteAllText(Folder + "/technical-report.json", JsonUtility.ToJson(report, true));
            Debug.Log("WalkRight playback: " + seen.Count + "/12 sprites observed; fixed root=" + rootStable + ". Art not approved.");
        }
        Repaint(); SceneView.RepaintAll(); EditorApplication.QueuePlayerLoopUpdate();
    }
    [Serializable] class TechnicalReport {
        public bool inspectedInEditor, rootTransformUnchanged, artisticApproval;
        public string playback;
        public int distinctSprites;
        public string[] observedSprites;
        public float secondsEvaluated, clipLength, fps;
    }
    void OnGUI()
    {
        EditorGUILayout.LabelField("WALK RIGHT — CANDIDATA NAO APROVADA", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Objeto separado | 12 frames | 326 x 301 | 11 FPS");
        if (actor == null || !graph.IsValid()) {
            if (GUILayout.Button("Preparar teste isolado")) Setup(); return;
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(playing ? "Pausar" : "Reproduzir")) { playing = !playing; previousTime = EditorApplication.timeSinceStartup; }
        if (GUILayout.Button("Reiniciar")) { StopGraph(); CreateGraph(); }
        if (GUILayout.Button("Proximo frame")) { playing = false; graph.Evaluate(1f/Fps); elapsed += 1f/Fps; SceneView.RepaintAll(); }
        EditorGUILayout.EndHorizontal();
        speed = EditorGUILayout.Slider("Velocidade de revisao", speed, .15f, 1f);
        EditorGUILayout.LabelField("Sprite atual", renderer2d.sprite != null ? renderer2d.sprite.name : "NENHUM");
        EditorGUILayout.LabelField("Sprites observados", seen.Count + " / 12; raiz fixa: " + rootStable);
        var area = GUILayoutUtility.GetRect(326, 301, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        EditorGUI.DrawRect(area, new Color(.17f,.19f,.23f));
        float zoom = Mathf.Min(area.width/326,area.height/301);
        var target = new Rect(area.center.x-326*zoom/2,area.center.y-301*zoom/2,326*zoom,301*zoom);
        var sprite = renderer2d.sprite;
        if (sprite != null) {
            var rect = sprite.rect; var tex = sprite.texture;
            GUI.DrawTextureWithTexCoords(target, tex, new Rect(rect.x/tex.width,rect.y/tex.height,rect.width/tex.width,rect.height/tex.height), true);
        }
        EditorGUI.DrawRect(new Rect(target.x,target.y+285*zoom,target.width,1),new Color(.6f,.85f,.7f));
        EditorGUILayout.HelpBox("A linha marca o solo/pivot. Esta janela reproduz o controller no Animator sem iniciar o jogo. Conferir pernas, mao/pomo e capa; raiz fixa nao implica arte correta.",MessageType.Info);
        if (GUILayout.Button("Selecionar objeto para Animation / Animator")) Selection.activeGameObject = actor;
    }
}
