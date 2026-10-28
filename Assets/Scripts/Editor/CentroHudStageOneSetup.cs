#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using TCCGame.Editor.UI;
using TCCGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Editor
{
    public static class CentroHudStageOneSetup
    {
        public const string PrefabPath = "Assets/Resources/UI/BuildingCanvas.prefab";

        [MenuItem("TCC Game/UI/Apply Centro HUD Stage 1")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Centro.unity", OpenSceneMode.Single);
            var prototype = AssetDatabase.LoadAssetAtPath<GameObject>(CentroHudProposalSetup.PrefabPath);
            if (prototype == null) throw new InvalidOperationException("Approved Centro prototype is missing.");
            var root = new GameObject("Building Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            GameObject dialog = UnityEngine.Object.Instantiate(prototype.transform.Find("Building Proposal").gameObject, root.transform, false);
            dialog.name = "Building Dialog";
            var panel = dialog.AddComponent<BuildingPanel>();
            var fields = new SerializedObject(panel);
            Bind(fields, "buildingNameText", Text(dialog, "Building Title"));
            Bind(fields, "descriptionText", Text(dialog, "Building Subtitle"));
            Bind(fields, "levelText", Text(dialog, "Building Level"));
            Bind(fields, "productionText", Text(dialog, "Current Benefit"));
            Bind(fields, "nextCaptionText", Text(dialog, "Next Caption"));
            Bind(fields, "nextBenefitText", Text(dialog, "Next Benefit"));
            Bind(fields, "upgradeCostText", Text(dialog, "Upgrade Cost"));
            Bind(fields, "balanceText", Text(dialog, "Cost Balance"));
            Bind(fields, "feedbackText", Text(dialog, "Building Feedback"));
            Bind(fields, "helpText", Text(dialog, "Building Help"));
            Bind(fields, "upgradeButton", Button(dialog, "Upgrade Building"));
            Bind(fields, "upgradeButtonText", Button(dialog, "Upgrade Building").GetComponentInChildren<TMP_Text>(true));
            Bind(fields, "backButton", Button(dialog, "Back To City"));
            Bind(fields, "closeButton", Button(dialog, "Close Building"));
            Bind(fields, "illustration", dialog.GetComponentsInChildren<Image>(true).Single(x => x.name == "Building Illustration"));
            Bind(fields, "theme", AssetDatabase.LoadAssetAtPath<MedievalHudTheme>(MedievalHudKit.ThemeAssetPath));
            fields.ApplyModifiedPropertiesWithoutUndo();
            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            foreach (Button button in root.GetComponentsInChildren<Button>(true)) button.targetGraphic.raycastTarget = true;
            dialog.transform.Find("Overlay").GetComponent<Image>().raycastTarget = true;
            dialog.SetActive(false);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            BuildingPanel oldPanel = UnityEngine.Object.FindFirstObjectByType<BuildingPanel>(FindObjectsInactive.Include);
            CityManager manager = UnityEngine.Object.FindFirstObjectByType<CityManager>();
            if (oldPanel == null || manager == null) throw new InvalidOperationException("Centro building references are missing.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var managerData = new SerializedObject(manager);
            Bind(managerData, "buildingPanel", instance.GetComponentInChildren<BuildingPanel>(true));
            managerData.ApplyModifiedPropertiesWithoutUndo();
            // Replace only the legacy modal; keep gold, tax feedback and all world objects.
            UnityEngine.Object.DestroyImmediate(oldPanel.transform.root.name == "Building Canvas"
                ? oldPanel.transform.root.gameObject : oldPanel.gameObject);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Centro stage 1 applied: production building prefab and scene references.");
        }

        private static TMP_Text Text(GameObject root, string name) => root.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == name);
        private static Button Button(GameObject root, string name) => root.GetComponentsInChildren<Button>(true).Single(x => x.name == name);
        private static void Bind(SerializedObject target, string field, UnityEngine.Object value) => target.FindProperty(field).objectReferenceValue = value;
    }
}
#endif
