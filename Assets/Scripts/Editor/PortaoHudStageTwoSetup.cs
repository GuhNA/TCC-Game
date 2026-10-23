#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using TCCGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Editor
{
    public static class PortaoHudStageTwoSetup
    {
        [MenuItem("TCC Game/UI/Apply Portao HUD Stage 2")]
        public static void Apply()
        {
            GameObject prototype = AssetDatabase.LoadAssetAtPath<GameObject>(PortaoHudPrototypeSetup.PrefabPath);
            if (prototype == null) throw new InvalidOperationException("Approved Portao prototype is missing.");

            GameObject wave = CanvasRoot("Wave Alert Canvas", 300, false);
            GameObject banner = Copy(prototype, "Wave Alert", wave.transform);
            CanvasGroup group = banner.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            WaveAlertView view = banner.AddComponent<WaveAlertView>();
            TMP_Text title = Label(banner, "BATEDORES AVISTARAM INIMIGOS");
            TMP_Text message = Label(banner, "Ameaça 42  •  avanço pelo caminho norte");
            title.name = "Alert Title";
            message.name = "Alert Message";
            message.rectTransform.anchoredPosition = new Vector2(35f, -18f);
            message.rectTransform.sizeDelta = new Vector2(620f, 46f);
            AutoSize(title, 21, 27);
            AutoSize(message, 15, 20);
            Bind(view, "title", title);
            Bind(view, "message", message);
            NonBlocking(wave);
            Save(wave, "Assets/Resources/UI/WaveAlertCanvas.prefab");

            GameObject marker = CanvasRoot("Wave Alert Indicator", 20, false);
            Canvas canvas = marker.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            marker.GetComponent<RectTransform>().sizeDelta = new Vector2(110, 116);
            marker.transform.localScale = Vector3.one * .01f;
            GameObject markerPanel = Copy(prototype, "World Alert Sample", marker.transform);
            markerPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            NonBlocking(marker);
            Save(marker, "Assets/Resources/UI/WaveAlertIndicator.prefab");

            GameObject feedback = CanvasRoot("Portao Alert Feedback Canvas", 6000, true);
            GameObject alert = Copy(prototype, "Alert Modal", feedback.transform);
            alert.SetActive(true);
            TMP_Text feedbackMessage = Label(alert,
                "A reserva não possui soldados\nsuficientes para essa formação.");
            feedbackMessage.name = "Feedback Message";
            AutoSize(feedbackMessage, 17, 25);
            NonBlocking(feedback);
            Button dismiss = feedback.GetComponentInChildren<Button>(true);
            dismiss.targetGraphic.raycastTarget = true;
            Save(feedback, "Assets/Resources/UI/PortaoAlertFeedbackCanvas.prefab");

            // Change only the gate's legacy alert controller. Other maps retain their configuration.
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Portão.unity", OpenSceneMode.Single);
            AlertPopup controller = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AlertPopup>(true)).Single();
            Bind(controller, "themedFeedbackPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Resources/UI/PortaoAlertFeedbackCanvas.prefab"));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Portao HUD stage 2 applied: wave banner, captain marker and gate feedback.");
        }

        private static GameObject CanvasRoot(string name, int order, bool interactive)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            if (interactive) root.AddComponent<GraphicRaycaster>();
            return root;
        }

        private static void NonBlocking(GameObject root)
        {
            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        private static void AutoSize(TMP_Text text, float minimum, float maximum)
        {
            text.enableAutoSizing = true;
            text.fontSizeMin = minimum;
            text.fontSizeMax = maximum;
        }

        private static TMP_Text Label(GameObject root, string value) =>
            root.GetComponentsInChildren<TMP_Text>(true).Single(text => text.text == value);

        private static GameObject Copy(GameObject prototype, string name, Transform parent)
        {
            Transform source = prototype.GetComponentsInChildren<Transform>(true).Single(item => item.name == name);
            GameObject result = UnityEngine.Object.Instantiate(source.gameObject, parent, false);
            result.name = source.name;
            return result;
        }

        private static void Bind(UnityEngine.Object target, string property, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Save(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
#endif
