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
    public static class PortaoHudStageThreeSetup
    {
        [MenuItem("TCC Game/UI/Apply Portao HUD Stage 3")]
        public static void Apply()
        {
            GameObject prototype = AssetDatabase.LoadAssetAtPath<GameObject>(PortaoHudPrototypeSetup.PrefabPath);
            if (prototype == null) throw new InvalidOperationException("Approved HUD prototype is missing.");
            BuildTravel(prototype);
            BuildGarrison(prototype);
            AssetDatabase.SaveAssets();
            Debug.Log("Portao HUD stage 3 applied: travel and garrison production prefabs.");
        }

        private static void BuildTravel(GameObject prototype)
        {
            GameObject root = CreateRoot("Travel Conversation Canvas", 900);
            GameObject dialog = Copy(prototype, "Travel Modal", root.transform);
            dialog.name = "Travel Dialog";
            TravelHudView view = dialog.AddComponent<TravelHudView>();
            NameLabel(dialog, "CONDUTOR DA CIDADE", "Title");
            Bind(view, "destination", NameLabel(dialog, "CIDADELA CENTRAL", "Travel Destination"));
            Bind(view, "healthLabel", NameLabel(dialog, "Integridade do portão", "Travel Health Label"));
            Bind(view, "healthValue", NameLabel(dialog, "100 / 100", "Travel Health"));
            Bind(view, "waveLabel", NameLabel(dialog, "Tempo de viagem", "Travel Wave Label"));
            Bind(view, "waveValue", NameLabel(dialog, "00:18", "Travel Wave Value"));
            TMP_Text body = NameLabel(dialog, "A viagem encerra conversas e interfaces abertas.", "Body");
            body.rectTransform.anchoredPosition = new Vector2(0, -119);
            body.rectTransform.sizeDelta = new Vector2(620, 62);
            AutoSize(body, 16, 20);
            Rename(dialog, "Confirm Travel", "Confirm");
            Rename(dialog, "Cancel Travel", "Cancel");
            ConfigureGraphics(root);
            Save(root, "Assets/Resources/UI/TravelConversationCanvas.prefab");
        }

        private static void BuildGarrison(GameObject prototype)
        {
            GameObject root = CreateRoot("Garrison Management Canvas", 910);
            GameObject dialog = Copy(prototype, "Garrison Modal", root.transform);
            dialog.name = "Garrison Dialog";
            GarrisonHudView view = dialog.AddComponent<GarrisonHudView>();
            NameLabel(dialog, "CAPITÃO — PORTÃO NORTE", "Title");
            Bind(view, "health", NameLabel(dialog, "100 / 100", "Gate Health"));
            Bind(view, "threat", NameLabel(dialog, "42", "Gate Threat"));
            Bind(view, "countdown", NameLabel(dialog, "01:25", "Wave Countdown"));
            NameLabel(dialog, "Destino dos envios: PORTÃO SUL", "Destination");
            NameLabel(dialog, "10 guerreiros em marcha.", "Status");
            Label(dialog, "10 guerreiros em marcha.").rectTransform.anchoredPosition = new Vector2(-230, -390);
            Rename(dialog, "Close Garrison", "Close");

            Transform paper = Find(dialog, "Battle Summary");
            // Keep the top edge and strategic stats in place; use the lower paper for real reports.
            RectTransform paperRect = (RectTransform)paper;
            paperRect.anchoredPosition += new Vector2(0, -80);
            paperRect.sizeDelta = new Vector2(420, 590);
            foreach (RectTransform child in paper)
                child.anchoredPosition += new Vector2(0, 80);
            UnityEngine.Object.DestroyImmediate(Label(dialog, "Reforce a linha de frente antes\ndo próximo ataque.").gameObject);
            UnityEngine.Object.DestroyImmediate(paper.Find("Helmet").gameObject);
            TMP_Text report = NameLabel(dialog,
                "ONDA 3 REPELIDA\nDefesa 48 / Ameaça 42\nPerdas: G 2 • A 1 • M 0\nEspólio: +8 ouro", "Battle Report");
            report.color = new Color(.22f, .16f, .1f);
            ScrollText(paper, report, "Report Scroll", new Vector2(0, -158), new Vector2(365, 230), 19);
            ((RectTransform)Find(dialog, "Acknowledge Report")).anchoredPosition = new Vector2(-465, -328);

            TMP_Text transit = NameLabel(dialog,
                "EM TRÂNSITO: 10 guerreiros • Norte → Sul • 18s", "Summary");
            ScrollText(Find(dialog, "Garrison Board"), transit, "Transit Scroll",
                new Vector2(255, -316), new Vector2(810, 64), 21);

            string[] units = { "Warrior", "Archer", "Mage" };
            string[] labels = { "GUERREIROS", "ARQUEIROS", "MAGOS" };
            float[] y = { 90, -18, -126 };
            string[] prototypeCommands = { "RECEBER 1", "RECEBER 10", "ENVIAR 1", "ENVIAR 10" };
            string[] commandNames = { "Receive 1", "Receive 10", "Send 1", "Send 10" };
            string[] rowFields = { "receiveOne", "receiveTen", "sendOne", "sendTen" };
            SerializedObject serializedView = new SerializedObject(view);
            SerializedProperty rows = serializedView.FindProperty("rows");
            rows.arraySize = 3;
            Transform roster = Find(dialog, "Garrison Roster");
            for (int i = 0; i < units.Length; i++)
            {
                NameLabel(dialog, labels[i], units[i]);
                SerializedProperty row = rows.GetArrayElementAtIndex(i);
                TMP_Text stationed = Count(roster, 78, y[i]);
                TMP_Text reserve = Count(roster, 258, y[i]);
                stationed.name = units[i] + " Stationed";
                reserve.name = units[i] + " Reserve";
                row.FindPropertyRelative("stationed").objectReferenceValue = stationed;
                row.FindPropertyRelative("reserve").objectReferenceValue = reserve;
                AutoSize(stationed, 18, 28);
                AutoSize(reserve, 18, 28);
                for (int c = 0; c < 4; c++)
                {
                    Transform button = Find(dialog, labels[i] + " " + prototypeCommands[c]);
                    button.name = commandNames[c] + " " + units[i];
                    row.FindPropertyRelative(rowFields[c]).objectReferenceValue = button.GetComponent<Button>();
                }
            }
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            ConfigureGraphics(root);
            Save(root, "Assets/Resources/UI/GarrisonManagementCanvas.prefab");
        }

        private static void ScrollText(Transform parent, TMP_Text text, string name, Vector2 position, Vector2 size, float fontSize)
        {
            GameObject viewport = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            viewport.transform.SetParent(parent, false);
            RectTransform rect = viewport.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            viewport.GetComponent<Image>().color = Color.clear;
            text.transform.SetParent(viewport.transform, false);
            RectTransform content = text.rectTransform;
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            text.enableAutoSizing = false;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.overflowMode = TextOverflowModes.Overflow;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ScrollRect scroll = viewport.GetComponent<ScrollRect>();
            scroll.viewport = rect;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28;

            GameObject track = new GameObject(name + " Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            track.transform.SetParent(viewport.transform, false);
            RectTransform trackRect = track.GetComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(1, 0);
            trackRect.anchorMax = Vector2.one;
            trackRect.sizeDelta = new Vector2(10, -4);
            trackRect.anchoredPosition = new Vector2(-5, 0);
            track.GetComponent<Image>().color = new Color(.25f, .2f, .13f, .7f);
            GameObject handle = new GameObject("Scroll Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(track.transform, false);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = Vector2.one;
            handleRect.offsetMax = -Vector2.one;
            handle.GetComponent<Image>().color = new Color(.75f, .51f, .18f);
            Scrollbar scrollbar = track.GetComponent<Scrollbar>();
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handle.GetComponent<Image>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            content.sizeDelta = new Vector2(-16, 0);
            content.anchoredPosition = new Vector2(-8, 0);
        }

        private static TMP_Text Count(Transform roster, float x, float y) =>
            roster.GetComponentsInChildren<TMP_Text>(true).Single(text =>
                Mathf.Approximately(text.rectTransform.anchoredPosition.x, x) &&
                Mathf.Approximately(text.rectTransform.anchoredPosition.y, y));

        private static void ConfigureGraphics(GameObject root)
        {
            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                button.targetGraphic.raycastTarget = true;
                AutoSize(button.GetComponentInChildren<TMP_Text>(), 16, button.GetComponentInChildren<TMP_Text>().fontSize);
                button.GetComponentInChildren<TMP_Text>().textWrappingMode = TextWrappingModes.NoWrap;
                button.GetComponentInChildren<TMP_Text>().rectTransform.sizeDelta -= new Vector2(28, 0);
            }
            foreach (ScrollRect scroll in root.GetComponentsInChildren<ScrollRect>(true))
                scroll.GetComponent<Image>().raycastTarget = true;
            foreach (Scrollbar scrollbar in root.GetComponentsInChildren<Scrollbar>(true))
            {
                scrollbar.targetGraphic.raycastTarget = true;
                scrollbar.GetComponent<Image>().raycastTarget = true;
            }
            Find(root, "Modal Overlay").GetComponent<Image>().raycastTarget = true;
        }

        private static GameObject CreateRoot(string name, int order)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            return root;
        }

        private static GameObject Copy(GameObject prototype, string name, Transform parent)
        {
            GameObject copy = UnityEngine.Object.Instantiate(Find(prototype, name).gameObject, parent, false);
            copy.name = name;
            copy.SetActive(true);
            foreach (TMP_Text text in copy.GetComponentsInChildren<TMP_Text>(true)) text.name = "Decoration";
            return copy;
        }

        private static Transform Find(GameObject root, string name) =>
            root.GetComponentsInChildren<Transform>(true).Single(item => item.name == name);
        private static TMP_Text Label(GameObject root, string value) =>
            root.GetComponentsInChildren<TMP_Text>(true).Single(text => text.text == value);
        private static TMP_Text NameLabel(GameObject root, string value, string name)
        {
            TMP_Text text = Label(root, value);
            text.name = name;
            AutoSize(text, Mathf.Min(18, text.fontSize), text.fontSize);
            return text;
        }
        private static void Rename(GameObject root, string previous, string next) => Find(root, previous).name = next;
        private static void AutoSize(TMP_Text text, float min, float max)
        {
            text.enableAutoSizing = true;
            text.fontSizeMin = min;
            text.fontSizeMax = max;
        }
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
