#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TCCGame.Gameplay.Exploration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TCCGame.EditorTools
{
    /// <summary>
    /// Generates the reusable combat scene and one encounter asset per map area.
    /// Running it again replaces only the generated scene and keeps encounter data.
    /// </summary>
    public static class ExplorationCombatSceneSetup
    {
        internal const string ScenePath =
            "Assets/Scenes/ExplorationCombat.unity";
        private const string CatalogPath =
            "Assets/Resources/Exploration/EncounterCatalog.asset";
        private const string EncountersDirectory =
            "Assets/Exploration/Encounters";

        [MenuItem("TCC/Exploration/Create or Update Combat Scene")]
        public static void CreateOrUpdate()
        {
            EnsureFolder("Assets/Resources", "Exploration");
            EnsureFolder("Assets", "Exploration");
            EnsureFolder("Assets/Exploration", "Encounters");
            EnsureFolder("Assets/Exploration", "Enemies");
            EnsureFolder("Assets/Exploration", "Resources");

            ExplorationEncounterCatalog catalog =
                GetOrCreateEncounterCatalog();
            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Additive
            );
            SceneManager.SetActiveScene(scene);

            Camera camera = CreateCamera();
            CreateEventSystem();
            ExplorationBattleBackground background =
                CreateWorldBackground(camera);

            ExplorationCombatantView[] adventurerViews = CreateWorldTeam(
                "Adventurer Team",
                new Vector3(-5.25f, -1.05f, 0f),
                1.7f,
                3.35f
            );
            ExplorationCombatantView[] enemyViews = CreateWorldTeam(
                "Enemy Team",
                new Vector3(2.05f, -0.65f, 0f),
                1.65f,
                2.35f
            );

            Canvas canvas = CreateCanvas();
            TMP_Text title = CreateText(
                canvas.transform,
                "Environment Title",
                "LOCAL DE COMBATE",
                42,
                TextAlignmentOptions.Center,
                new Vector2(0.2f, 0.9f),
                new Vector2(0.8f, 0.985f)
            );
            TMP_Text round = CreateText(
                canvas.transform,
                "Round",
                "Rodada 1",
                28,
                TextAlignmentOptions.Center,
                new Vector2(0.4f, 0.82f),
                new Vector2(0.6f, 0.89f)
            );

            TMP_Text details = CreateTextPanel(
                canvas.transform,
                "Selected Unit Details",
                "Clique em um personagem para ver seus atributos.",
                new Vector2(0.015f, 0.68f),
                new Vector2(0.245f, 0.9f)
            );
            TMP_Text log = CreateScrollableTextPanel(
                canvas.transform,
                "Combat Log",
                "Pressione Batalhar para iniciar.",
                new Vector2(0.28f, 0.015f),
                new Vector2(0.7f, 0.13f),
                out ScrollRect logScrollRect
            );
            TMP_Text rewards = CreateTextPanel(
                canvas.transform,
                "Rewards",
                "Recursos possíveis",
                new Vector2(0.75f, 0.015f),
                new Vector2(0.985f, 0.13f)
            );

            Button battleButton = CreateButton(
                canvas.transform,
                "Battle Button",
                "Batalhar",
                new Vector2(0.38f, 0.14f),
                new Vector2(0.53f, 0.215f),
                out TMP_Text battleButtonText
            );
            Button retreatButton = CreateButton(
                canvas.transform,
                "Retreat Button",
                "Recuar",
                new Vector2(0.55f, 0.14f),
                new Vector2(0.7f, 0.215f),
                out _
            );

            var controllerObject = new GameObject(
                "Exploration Combat Controller"
            );
            var controller = controllerObject.AddComponent<
                ExplorationCombatSceneController>();
            var serializedController = new SerializedObject(controller);
            SetObject(
                serializedController,
                "encounterCatalog",
                catalog
            );
            SetObject(serializedController, "battleBackground", background);
            SetArray(
                serializedController,
                "adventurerViews",
                adventurerViews.Cast<UnityEngine.Object>().ToArray()
            );
            SetArray(
                serializedController,
                "enemyViews",
                enemyViews.Cast<UnityEngine.Object>().ToArray()
            );
            SetArray(
                serializedController,
                "adventurerCatalog",
                FindAssets<Char_base>().Cast<UnityEngine.Object>().ToArray()
            );
            SetObject(serializedController, "environmentTitleText", title);
            SetObject(serializedController, "roundText", round);
            SetObject(serializedController, "combatLogText", log);
            SetObject(
                serializedController,
                "combatLogScrollRect",
                logScrollRect
            );
            SetObject(serializedController, "detailsText", details);
            SetObject(serializedController, "rewardsText", rewards);
            SetObject(serializedController, "battleButton", battleButton);
            SetObject(
                serializedController,
                "battleButtonText",
                battleButtonText
            );
            SetObject(serializedController, "retreatButton", retreatButton);
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            if (previousScene.IsValid() && previousScene.isLoaded)
            {
                SceneManager.SetActiveScene(previousScene);
            }
            EditorSceneManager.CloseScene(scene, true);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = SceneAssetAtPath(ScenePath);
            Debug.Log(
                "ExplorationCombat scene and encounter catalog are ready. " +
                "Configure each asset under Assets/Exploration/Encounters."
            );
        }

        private static ExplorationEncounterCatalog GetOrCreateEncounterCatalog()
        {
            ExplorationEncounterCatalog catalog =
                AssetDatabase.LoadAssetAtPath<ExplorationEncounterCatalog>(
                    CatalogPath
                );
            if (catalog == null)
            {
                if (AssetDatabase.LoadMainAssetAtPath(CatalogPath) != null)
                {
                    AssetDatabase.DeleteAsset(CatalogPath);
                }

                catalog = ScriptableObject.CreateInstance<
                    ExplorationEncounterCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var encounters = new List<ExplorationEncounterDefinition>();
            foreach (ExplorationEnvironment environment in
                     Enum.GetValues(typeof(ExplorationEnvironment)))
            {
                string path = EncountersDirectory + "/" + environment +
                    ".asset";
                ExplorationEncounterDefinition encounter =
                    AssetDatabase.LoadAssetAtPath<
                        ExplorationEncounterDefinition>(path);
                if (encounter == null)
                {
                    if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                    {
                        AssetDatabase.DeleteAsset(path);
                    }

                    encounter = ScriptableObject.CreateInstance<
                        ExplorationEncounterDefinition>();
                    var serializedEncounter = new SerializedObject(encounter);
                    serializedEncounter.FindProperty("environment").enumValueIndex =
                        (int)environment;
                    serializedEncounter.ApplyModifiedPropertiesWithoutUndo();
                    AssetDatabase.CreateAsset(encounter, path);
                }

                encounters.Add(encounter);
            }

            var serializedCatalog = new SerializedObject(catalog);
            SetArray(
                serializedCatalog,
                "encounters",
                encounters.Cast<UnityEngine.Object>().ToArray()
            );
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.05f, 0.07f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<Physics2DRaycaster>();
            return camera;
        }

        private static ExplorationBattleBackground CreateWorldBackground(
            Camera camera
        )
        {
            var backgroundObject = new GameObject(
                "Environment Background",
                typeof(SpriteRenderer),
                typeof(ExplorationBattleBackground)
            );
            backgroundObject.transform.position = new Vector3(0f, 0f, 5f);
            SpriteRenderer renderer =
                backgroundObject.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = -100;
            renderer.color = Color.white;

            ExplorationBattleBackground background =
                backgroundObject.GetComponent<ExplorationBattleBackground>();
            var serialized = new SerializedObject(background);
            SetObject(serialized, "spriteRenderer", renderer);
            SetObject(serialized, "targetCamera", camera);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return background;
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule)
            );
            eventSystem.transform.position = Vector3.zero;
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject(
                "Exploration Combat Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static ExplorationCombatantView[] CreateWorldTeam(
            string name,
            Vector3 firstPosition,
            float spacing,
            float fallbackHeight
        )
        {
            var teamRoot = new GameObject(name);

            var views = new ExplorationCombatantView[
                ExplorationEncounterDefinition.MaximumTeamSize];
            for (int index = 0; index < views.Length; index++)
            {
                views[index] = CreateWorldCombatantView(
                    teamRoot.transform,
                    "Slot " + (index + 1),
                    firstPosition + Vector3.right * (spacing * index),
                    fallbackHeight
                );
            }

            return views;
        }

        private static ExplorationCombatantView CreateWorldCombatantView(
            Transform parent,
            string name,
            Vector3 position,
            float fallbackHeight
        )
        {
            var slot = new GameObject(
                name,
                typeof(BoxCollider2D),
                typeof(ExplorationCombatantView)
            );
            slot.transform.SetParent(parent, false);
            slot.transform.position = position;
            BoxCollider2D collider = slot.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(1.55f, fallbackHeight + 1.05f);
            collider.offset = new Vector2(0f, 0.15f);

            var visualRoot = new GameObject("Visual Root").transform;
            visualRoot.SetParent(slot.transform, false);
            SpriteRenderer fallback = visualRoot.gameObject.AddComponent<
                SpriteRenderer>();
            fallback.sortingOrder = 10;

            TMP_Text unitName = CreateWorldText(
                slot.transform,
                "Name",
                "Combatente",
                new Vector3(0f, fallbackHeight * 0.58f + 0.62f, 0f),
                3.2f,
                new Vector2(3.3f, 0.65f)
            );
            TMP_Text health = CreateWorldText(
                slot.transform,
                "Health",
                "10/10",
                new Vector3(0f, -fallbackHeight * 0.53f - 0.4f, 0f),
                2.1f,
                new Vector2(2.2f, 0.4f)
            );
            Transform healthFill = CreateWorldHealthBar(
                slot.transform,
                new Vector3(0f, -fallbackHeight * 0.53f - 0.15f, 0f)
            );

            var view = slot.GetComponent<ExplorationCombatantView>();
            var serializedView = new SerializedObject(view);
            SetObject(serializedView, "visualRoot", visualRoot);
            SetObject(serializedView, "fallbackRenderer", fallback);
            SetObject(serializedView, "nameText", unitName);
            SetObject(serializedView, "healthText", health);
            SetObject(serializedView, "healthFill", healthFill);
            serializedView.FindProperty("fallbackTargetHeight").floatValue =
                fallbackHeight;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static Transform CreateWorldHealthBar(
            Transform parent,
            Vector3 localPosition
        )
        {
            Sprite barSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(
                "UI/Skin/UISprite.psd"
            );
            var root = new GameObject("Health Bar").transform;
            root.SetParent(parent, false);
            root.localPosition = localPosition;

            SpriteRenderer background = new GameObject(
                "Background",
                typeof(SpriteRenderer)
            ).GetComponent<SpriteRenderer>();
            background.transform.SetParent(root, false);
            background.sprite = barSprite;
            background.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);
            background.sortingOrder = 20;
            background.drawMode = SpriteDrawMode.Sliced;
            background.size = new Vector2(1.72f, 0.18f);

            var fillRoot = new GameObject("Fill Root").transform;
            fillRoot.SetParent(root, false);
            fillRoot.localPosition = new Vector3(-0.79f, 0f, -0.01f);

            SpriteRenderer fill = new GameObject(
                "Fill",
                typeof(SpriteRenderer)
            ).GetComponent<SpriteRenderer>();
            fill.transform.SetParent(fillRoot, false);
            fill.sprite = barSprite;
            fill.color = new Color(0.85f, 0.08f, 0.08f, 1f);
            fill.sortingOrder = 21;
            fill.drawMode = SpriteDrawMode.Sliced;
            fill.size = new Vector2(1.58f, 0.1f);
            fill.transform.localPosition = new Vector3(0.79f, 0f, 0f);
            return fillRoot;
        }

        private static TMP_Text CreateWorldText(
            Transform parent,
            string name,
            string value,
            Vector3 localPosition,
            float fontSize,
            Vector2 size
        )
        {
            var textObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshPro)
            );
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = localPosition;
            var text = textObject.GetComponent<TextMeshPro>();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.outlineColor = Color.black;
            text.outlineWidth = 0.28f;
            text.rectTransform.sizeDelta = size;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.GetComponent<Renderer>().sortingOrder = 30;
            return text;
        }

        private static TMP_Text CreateTextPanel(
            Transform parent,
            string name,
            string value,
            Vector2 anchorMin,
            Vector2 anchorMax
        )
        {
            Image panel = CreateImage(
                parent,
                name + " Panel",
                new Color(0f, 0f, 0f, 0.68f),
                anchorMin,
                anchorMax,
                Vector2.zero,
                Vector2.zero
            );
            return CreateText(
                panel.transform,
                name,
                value,
                22,
                TextAlignmentOptions.TopLeft,
                Vector2.zero,
                Vector2.one,
                new Vector2(18f, 12f),
                new Vector2(-18f, -12f)
            );
        }

        private static TMP_Text CreateScrollableTextPanel(
            Transform parent,
            string name,
            string value,
            Vector2 anchorMin,
            Vector2 anchorMax,
            out ScrollRect scrollRect
        )
        {
            Image panel = CreateImage(
                parent,
                name + " Panel",
                new Color(0f, 0f, 0f, 0.68f),
                anchorMin,
                anchorMax,
                Vector2.zero,
                Vector2.zero
            );
            scrollRect = panel.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 28f;

            Image viewportImage = CreateImage(
                panel.transform,
                "Viewport",
                new Color(0f, 0f, 0f, 0.001f),
                Vector2.zero,
                Vector2.one,
                new Vector2(14f, 10f),
                new Vector2(-14f, -10f)
            );
            viewportImage.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            RectTransform viewport = viewportImage.rectTransform;

            RectTransform content = CreateRect(
                viewport,
                "Content",
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                Vector2.zero,
                Vector2.zero
            );
            content.pivot = new Vector2(0.5f, 1f);
            var contentLayout = content.gameObject.AddComponent<
                VerticalLayoutGroup>();
            contentLayout.childControlHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            TMP_Text text = CreateText(
                content,
                name,
                value,
                20,
                TextAlignmentOptions.TopLeft,
                Vector2.zero,
                Vector2.one
            );
            text.textWrappingMode = TextWrappingModes.Normal;

            scrollRect.viewport = viewport;
            scrollRect.content = content;
            return text;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            out TMP_Text text
        )
        {
            Image image = CreateImage(
                parent,
                name,
                new Color(0.72f, 0.55f, 0.2f, 1f),
                anchorMin,
                anchorMax,
                Vector2.zero,
                Vector2.zero
            );
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            text = CreateText(
                image.transform,
                "Label",
                label,
                28,
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.one
            );
            return button;
        }

        private static TMP_Text CreateText(
            Transform parent,
            string name,
            string value,
            float fontSize,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2? offsetMin = null,
            Vector2? offsetMax = null
        )
        {
            RectTransform rect = CreateRect(
                parent,
                name,
                anchorMin,
                anchorMax,
                offsetMin ?? Vector2.zero,
                offsetMax ?? Vector2.zero
            );
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateImage(
            Transform parent,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax
        )
        {
            RectTransform rect = CreateRect(
                parent,
                name,
                anchorMin,
                anchorMax,
                offsetMin,
                offsetMax
            );
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static RectTransform CreateRect(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax
        )
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes =
                EditorBuildSettings.scenes.ToList();
            if (scenes.All(scene => scene.path != scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        private static T[] FindAssets<T>() where T : UnityEngine.Object
        {
            return AssetDatabase.FindAssets("t:" + typeof(T).Name)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null)
                .ToArray();
        }

        private static void SetObject(
            SerializedObject serializedObject,
            string propertyName,
            UnityEngine.Object value
        )
        {
            serializedObject.FindProperty(propertyName).objectReferenceValue =
                value;
        }

        private static void SetArray(
            SerializedObject serializedObject,
            string propertyName,
            IReadOnlyList<UnityEngine.Object> values
        )
        {
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);
            property.arraySize = values.Count;
            for (int index = 0; index < values.Count; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue =
                    values[index];
            }
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static SceneAsset SceneAssetAtPath(string path)
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }
    }

    /// <summary>
    /// Applies this one-time layout migration after scripts reload. The generated
    /// scene is opened additively, so the scene currently being edited is kept.
    /// </summary>
    [InitializeOnLoad]
    internal static class ExplorationCombatSceneLayoutMigration
    {
        private const string MigrationKey =
            "TCCGame.ExplorationCombat.WorldLayout.V4";

        static ExplorationCombatSceneLayoutMigration()
        {
            EditorApplication.delayCall += TryRun;
        }

        private static void TryRun()
        {
            if (EditorPrefs.GetBool(MigrationKey, false))
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += TryRun;
                return;
            }

            Scene alreadyOpen = SceneManager.GetSceneByPath(
                ExplorationCombatSceneSetup.ScenePath
            );
            if (alreadyOpen.IsValid() && alreadyOpen.isLoaded)
            {
                Debug.LogWarning(
                    "Close the ExplorationCombat scene and use " +
                    "TCC/Exploration/Create or Update Combat Scene to apply " +
                    "the new world-space layout."
                );
                return;
            }

            ExplorationCombatSceneSetup.CreateOrUpdate();
            EditorPrefs.SetBool(MigrationKey, true);
        }
    }
}
#endif
