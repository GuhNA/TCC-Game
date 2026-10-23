#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using TCCGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Editor
{
    public static class PortaoHudStageOneSetup
    {
        [MenuItem("TCC Game/UI/Apply Portao HUD Stage 1")]
        public static void Apply()
        {
            GameObject prototype = AssetDatabase.LoadAssetAtPath<GameObject>(PortaoHudPrototypeSetup.PrefabPath);
            if (prototype == null) throw new InvalidOperationException("Build the approved Portao prototype first.");
            GameObject gate = CreateCanvas(prototype, "Gate Context Canvas", 200);
            GameObject panel = Copy(prototype, "Gate Status", gate.transform);
            GateHudView view = panel.AddComponent<GateHudView>();
            Bind(view, "title", Label(panel, "PORTÃO NORTE"));
            Bind(view, "health", Label(panel, "100 / 100"));
            TMP_Text wave = Label(panel, "Próxima onda  01:25");
            wave.enableAutoSizing = true;
            wave.fontSizeMin = 15;
            wave.fontSizeMax = 23;
            wave.textWrappingMode = TextWrappingModes.NoWrap;
            Bind(view, "wave", wave);
            Image fill = panel.GetComponentsInChildren<Image>(true).Single(x => x.name == "Bar Value");
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            Bind(view, "healthFill", fill);
            Save(gate, "Assets/Resources/UI/GateContextCanvas.prefab");

            GameObject prompt = CreateCanvas(prototype, "Interaction Prompt Canvas", 400);
            CanvasGroup group = prompt.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            GameObject promptPanel = Copy(prototype, "Interaction Prompt", prompt.transform);
            TMP_Text description = Label(promptPanel, "FALAR COM O CAPITÃO DO PORTÃO");
            description.enableAutoSizing = true;
            description.fontSizeMin = 17;
            description.fontSizeMax = 25;
            description.textWrappingMode = TextWrappingModes.NoWrap;
            Bind(promptPanel.AddComponent<InteractionPromptView>(), "description", description);
            Save(prompt, "Assets/Resources/UI/InteractionPrompt.prefab");

            const string goldPath = "Assets/Prefabs/UI/GoldBalanceHUD.prefab";
            GameObject gold = PrefabUtility.LoadPrefabContents(goldPath);
            try
            {
                Transform previous = gold.transform.Find("Gold Balance");
                if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
                // Retain the legacy binding for existing scenes and callers during migration.
                foreach (Text legacy in gold.GetComponentsInChildren<Text>(true)) legacy.enabled = false;
                GameObject goldPanel = Copy(prototype, "Gold Balance", gold.transform);
                TMP_Text amount = Label(goldPanel, "145 319");
                amount.enableAutoSizing = true;
                amount.fontSizeMin = 22;
                amount.fontSizeMax = 35;
                amount.textWrappingMode = TextWrappingModes.NoWrap;
                amount.text = "0";
                Bind(gold.GetComponent<GoldBalancePresenter>(), "pixelGoldText", amount);
                ConfigureCanvas(gold, 10);
                gold.transform.localScale = Vector3.one;
                PrefabUtility.SaveAsPrefabAsset(gold, goldPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(gold); }
            AssetDatabase.SaveAssets();
            Debug.Log("Portao HUD stage 1 applied: gate context, gold and interaction prompt.");
        }

        private static GameObject CreateCanvas(GameObject prototype, string name, int order)
        {
            GameObject result = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            ConfigureCanvas(result, order);
            return result;
        }

        private static void ConfigureCanvas(GameObject root, int order)
        {
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
        }

        private static GameObject Copy(GameObject prototype, string name, Transform parent)
        {
            Transform source = prototype.GetComponentsInChildren<Transform>(true).Single(x => x.name == name);
            GameObject result = UnityEngine.Object.Instantiate(source.gameObject, parent, false);
            result.name = source.name;
            return result;
        }

        private static TMP_Text Label(GameObject root, string value) =>
            root.GetComponentsInChildren<TMP_Text>(true).Single(x => x.text == value);

        private static void Bind(UnityEngine.Object target, string name, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(name).objectReferenceValue = value;
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
