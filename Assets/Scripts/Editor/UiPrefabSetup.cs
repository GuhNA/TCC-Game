#if UNITY_EDITOR
using TMPro;
using TCCGame.Gameplay.Campaign;
using TCCGame.Gameplay.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Editor
{
    public static class UiPrefabSetup
    {
        [MenuItem("TCC Game/UI/Rebuild All UI Prefabs")]
        public static void RebuildAllUiPrefabs()
        {
            CreateInteractionPromptPrefab();
            CreateWaveAlertPrefabs();
            CreateStage3Prefabs();
            CreateSceneFadePrefab();
            AssignUiPrefabsToScenes();
            PortaoHudStageOneSetup.Apply();
            PortaoHudStageTwoSetup.Apply();
            PortaoHudStageThreeSetup.Apply();
        }

        [MenuItem("TCC Game/UI/Assign UI Prefabs To Scenes")]
        public static void AssignUiPrefabsToScenes()
        {
            const string gateScenePath = "Assets/Scenes/Portão.unity";
            UnityEngine.SceneManagement.Scene previous =
                UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.Scene gateScene =
                EditorSceneManager.OpenScene(gateScenePath, OpenSceneMode.Single);
            CaptainNpc captain = Object.FindFirstObjectByType<CaptainNpc>();
            GateContextPresenter context =
                Object.FindFirstObjectByType<GateContextPresenter>();
            GameObject indicator = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Resources/UI/WaveAlertIndicator.prefab"
            );
            GameObject contextPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Resources/UI/GateContextCanvas.prefab"
            );
            if (captain == null || context == null || indicator == null ||
                contextPrefab == null)
            {
                throw new System.InvalidOperationException(
                    "Captain or WaveAlertIndicator prefab was not found."
                );
            }

            SerializedObject serializedCaptain = new SerializedObject(captain);
            serializedCaptain.FindProperty("alertIndicatorPrefab")
                .objectReferenceValue = indicator;
            serializedCaptain.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedContext = new SerializedObject(context);
            serializedContext.FindProperty("contextPrefab")
                .objectReferenceValue = contextPrefab;
            serializedContext.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(gateScene);
            EditorSceneManager.SaveScene(gateScene);

            if (previous.IsValid() && !string.IsNullOrEmpty(previous.path) &&
                previous.path != gateScenePath)
            {
                EditorSceneManager.OpenScene(previous.path, OpenSceneMode.Single);
            }
        }

        [MenuItem("TCC Game/UI/Create Interaction Prompt Prefab")]
        public static void CreateInteractionPromptPrefab()
        {
            const string directory = "Assets/Resources/UI";
            const string path = directory + "/InteractionPrompt.prefab";
            EnsureFolder("Assets/Resources");
            EnsureFolder(directory);

            GameObject root = new GameObject(
                "Interaction Prompt Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup)
            );
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            GraphicRaycaster raycaster = root.GetComponent<GraphicRaycaster>();
            raycaster.enabled = false;

            GameObject textObject = new GameObject(
                "Prompt Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );
            textObject.transform.SetParent(root.transform, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 38f);
            rect.sizeDelta = new Vector2(650f, 54f);
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.text = "[E] Interagir — Exemplo de NPC";
            text.fontSize = 28f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("TCC Game/UI/Create Wave Alert Prefabs")]
        public static void CreateWaveAlertPrefabs()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");
            CreateWaveAlertIndicator("Assets/Resources/UI/WaveAlertIndicator.prefab");
            CreateWaveAlertCanvas("Assets/Resources/UI/WaveAlertCanvas.prefab");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateWaveAlertIndicator(string path)
        {
            GameObject root = new GameObject(
                "Wave Alert Indicator",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler)
            );
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 20;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
            root.transform.localScale = Vector3.one * 0.01f;
            GameObject textObject = CreateTmpText(
                root.transform, "!", 64f, TextAlignmentOptions.Center
            );
            textObject.GetComponent<RectTransform>().sizeDelta =
                new Vector2(100f, 100f);
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.color = new Color(1f, 0.82f, 0.12f, 1f);
            text.outlineWidth = 0.18f;
            text.outlineColor = Color.black;
            SavePrefab(root, path);
        }

        private static void CreateWaveAlertCanvas(string path)
        {
            GameObject root = new GameObject(
                "Wave Alert Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            root.GetComponent<GraphicRaycaster>().enabled = false;

            GameObject banner = new GameObject(
                "Wave Alert Banner",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(Image)
            );
            banner.transform.SetParent(root.transform, false);
            RectTransform bannerRect = banner.GetComponent<RectTransform>();
            bannerRect.anchorMin = bannerRect.anchorMax = new Vector2(0.5f, 1f);
            bannerRect.pivot = new Vector2(0.5f, 1f);
            bannerRect.anchoredPosition = new Vector2(0f, -278f);
            bannerRect.sizeDelta = new Vector2(920f, 64f);
            banner.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.96f);
            TextMeshProUGUI text = CreateTmpText(
                banner.transform,
                "Batedor: inimigos avançam para o Portão Norte.",
                25f,
                TextAlignmentOptions.Center
            ).GetComponent<TextMeshProUGUI>();
            text.margin = new Vector4(14f, 8f, 14f, 8f);
            CanvasGroup group = banner.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            SavePrefab(root, path);
        }

        private static GameObject CreateTmpText(
            Transform parent, string value, float size,
            TextAlignmentOptions alignment
        )
        {
            GameObject textObject = new GameObject(
                "Text", typeof(RectTransform), typeof(TextMeshProUGUI)
            );
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.raycastTarget = false;
            return textObject;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        [MenuItem("TCC Game/UI/Create Stage 3 Prefabs")]
        public static void CreateStage3Prefabs()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");
            CreateGateContextPrefab();
            CreateTravelPrefab();
            CreateGarrisonPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("TCC Game/UI/Create Scene Fade Prefab")]
        public static void CreateSceneFadePrefab()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");
            GameObject root = CreateScreenCanvas("Scene Fade Canvas", 1000, true);
            GameObject imageObject = new GameObject(
                "Fade Image", typeof(RectTransform), typeof(Image)
            );
            imageObject.transform.SetParent(root.transform, false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = imageObject.GetComponent<Image>();
            // Visible in Prefab Mode; runtime immediately applies alpha zero.
            image.color = new Color(0f, 0f, 0f, 0.65f);
            image.raycastTarget = false;
            SavePrefab(root, "Assets/Resources/UI/SceneFadeCanvas.prefab");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateGateContextPrefab()
        {
            GameObject root = CreateScreenCanvas("Gate Context Canvas", 200, false);
            TextMeshProUGUI text = AddText(root.transform, "Gate Context Text",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(26f, -24f), new Vector2(480f, 170f), 27f);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.text = "PORTÃO NORTE\nIntegridade: 100/100\n" +
                "ANUNCIADO · 01:25 · Ameaça 42";
            SavePrefab(root, "Assets/Resources/UI/GateContextCanvas.prefab");
        }

        private static void CreateTravelPrefab()
        {
            GameObject root = CreateScreenCanvas("Travel Conversation Canvas", 900, true);
            GameObject dialog = AddOverlay(root.transform, "Travel Dialog", 0.75f);
            GameObject card = AddCard(dialog.transform, new Vector2(760f, 470f));
            TextMeshProUGUI title = AddText(card.transform, "Title",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(28f, -26f), new Vector2(-28f, -88f), 30f);
            title.text = "CONDUTOR DA CIDADE";
            TextMeshProUGUI body = AddText(card.transform, "Body",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(28f, -105f), new Vector2(-28f, -174f), 23f);
            body.alignment = TextAlignmentOptions.TopLeft;
            body.textWrappingMode = TextWrappingModes.Normal;
            body.text = "Posso levá-lo ao Portão Norte.\n\n" +
                "Integridade 100/100 · Defesa 35 · Ataque em 01:25.";
            AddButton(card.transform, "Confirm", new Vector2(-170f, 65f),
                new Vector2(310f, 70f), "Viajar");
            AddButton(card.transform, "Cancel", new Vector2(170f, 65f),
                new Vector2(310f, 70f), "Cancelar [Esc]");
            dialog.SetActive(true);
            SavePrefab(root, "Assets/Resources/UI/TravelConversationCanvas.prefab");
        }

        private static void CreateGarrisonPrefab()
        {
            GameObject root = CreateScreenCanvas("Garrison Management Canvas", 910, true);
            GameObject dialog = AddOverlay(root.transform, "Garrison Dialog", 0.78f);
            GameObject card = AddCard(dialog.transform, new Vector2(1040f, 900f));
            TextMeshProUGUI title = AddText(card.transform, "Title",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(35f, -20f), new Vector2(-35f, -68f), 30f);
            title.text = "CAPITÃO — PORTÃO NORTE";
            TextMeshProUGUI report = AddText(card.transform, "Battle Report",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(35f, -78f), new Vector2(-250f, -252f), 18f);
            report.text = "ONDA 3 — ONDA REPELIDA   Defesa 48 / Ameaça 42\n" +
                "G 20→18 (-2)   A 12→11 (-1)   M 6→6 (-0)\n" +
                "Portão 100→96 (-4)   Espólio +8 ouro";
            AddButton(card.transform, "Acknowledge Report", new Vector2(385f, -165f),
                new Vector2(210f, 58f), "Confirmar relatório");
            TextMeshProUGUI summary = AddText(card.transform, "Summary",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(35f, -270f), new Vector2(-35f, -382f), 22f);
            summary.text = "RESERVA   G 30  A 20  M 10\n" +
                "GUARNIÇÃO  G 18  A 11  M 6\n" +
                "EM TRÂNSITO  10 G Norte → Sul (18,7s)";
            TextMeshProUGUI destination = AddText(card.transform, "Destination",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(35f, -397f), new Vector2(-315f, -447f), 22f);
            destination.text = "Destino dos envios: Portão Sul";
            AddButton(card.transform, "Change Destination", new Vector2(350f, -422f),
                new Vector2(260f, 52f), "Trocar destino");

            string[] units = { "Warrior", "Archer", "Mage" };
            string[] labels = { "guerreiros", "arqueiros", "magos" };
            float rowY = -512f;
            for (int i = 0; i < units.Length; i++)
            {
                TextMeshProUGUI unit = AddText(card.transform, units[i],
                    new Vector2(0f, 1f), new Vector2(1f, 1f),
                    new Vector2(35f, rowY + 25f), new Vector2(-805f, rowY - 25f), 22f);
                unit.text = labels[i];
                AddButton(card.transform, $"Receive 1 {units[i]}", new Vector2(-190f, rowY), new Vector2(155f, 52f), "Receber 1");
                AddButton(card.transform, $"Receive 10 {units[i]}", new Vector2(-20f, rowY), new Vector2(155f, 52f), "Receber 10");
                AddButton(card.transform, $"Send 1 {units[i]}", new Vector2(170f, rowY), new Vector2(155f, 52f), "Enviar 1");
                AddButton(card.transform, $"Send 10 {units[i]}", new Vector2(340f, rowY), new Vector2(155f, 52f), "Enviar 10");
                rowY -= 72f;
            }

            TextMeshProUGUI status = AddText(card.transform, "Status",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(35f, -744f), new Vector2(-210f, -806f), 20f);
            status.color = new Color(1f, 0.86f, 0.48f, 1f);
            status.text = "10 guerreiros em marcha.";
            AddButton(card.transform, "Close", new Vector2(390f, -790f),
                new Vector2(190f, 58f), "Fechar [Esc]");
            dialog.SetActive(true);
            SavePrefab(root, "Assets/Resources/UI/GarrisonManagementCanvas.prefab");
        }

        private static GameObject CreateScreenCanvas(string name, int order, bool raycasts)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            root.GetComponent<GraphicRaycaster>().enabled = raycasts;
            return root;
        }

        private static GameObject AddOverlay(Transform parent, string name, float alpha)
        {
            GameObject overlay = new GameObject(name, typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(parent, false);
            RectTransform rect = overlay.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            overlay.GetComponent<Image>().color = new Color(0.02f, 0.025f, 0.04f, alpha);
            return overlay;
        }

        private static GameObject AddCard(Transform parent, Vector2 size)
        {
            GameObject card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            card.GetComponent<Image>().color = new Color(0.09f, 0.12f, 0.17f, 0.99f);
            return card;
        }

        private static TextMeshProUGUI AddText(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin,
            Vector2 offsetMax, float size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
            return text;
        }

        private static Button AddButton(Transform parent, string name,
            Vector2 position, Vector2 size, string label)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.22f, 0.36f, 0.5f, 1f);
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            TextMeshProUGUI text = AddText(go.transform, "Label", Vector2.zero,
                Vector2.one, Vector2.zero, Vector2.zero, 18f);
            text.alignment = TextAlignmentOptions.Center;
            text.text = label;
            return button;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path)
                .Replace('\\', '/');
            string name = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
