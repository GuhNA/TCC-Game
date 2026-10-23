#if UNITY_EDITOR
using TMPro;
using TCCGame.Editor.UI;
using TCCGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Editor
{
    /// <summary>
    /// Builds the Portao HUD showcase from the shared medieval pixel kit.
    /// It is intentionally presentation-only: gameplay prefabs stay untouched
    /// until the prototype is explicitly approved.
    /// </summary>
    public static class PortaoHudPrototypeSetup
    {
        public const string PrefabPath =
            "Assets/Resources/UI/Prototypes/PortaoHudPrototypeCanvas.prefab";

        [MenuItem("TCC Game/UI/Rebuild Portao HUD Prototype")]
        public static void Rebuild()
        {
            MedievalHudKit.EnsureFolder("Assets/Resources/UI/Prototypes");
            MedievalHudTheme theme = AssetDatabase.LoadAssetAtPath<MedievalHudTheme>(
                MedievalHudKit.ThemeAssetPath);
            if (theme == null || !theme.IsComplete)
                theme = MedievalHudKit.CreateOrUpdateTheme();
            GameObject root = Build(theme);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Portao HUD prototype rebuilt: {PrefabPath}");
        }

        private static GameObject Build(MedievalHudTheme theme)
        {
            GameObject root = new GameObject(
                "Portao HUD Prototype Canvas",
                typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 4800;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            BuildGameplay(theme, root.transform);
            BuildTravel(theme, root.transform);
            BuildGarrison(theme, root.transform);
            BuildAlert(theme, root.transform);
            return root;
        }

        private static void BuildGameplay(MedievalHudTheme theme, Transform root)
        {
            GameObject group = Group(root, "Gameplay HUD");

            GameObject gate = Panel(theme, group.transform, "Gate Status",
                new Vector2(-710f, 420f), new Vector2(450f, 170f), theme.WoodPanel);
            AddHeader(theme, gate.transform, "PORTÃO NORTE", 450f, 52f, 57f);
            Text(theme, gate.transform, "Integridade", new Vector2(-92f, 10f),
                new Vector2(230f, 34f), 25f, TextAlignmentOptions.Left, theme.Subtitle);
            Text(theme, gate.transform, "100 / 100", new Vector2(130f, 10f),
                new Vector2(150f, 34f), 27f, TextAlignmentOptions.Right, theme.Success);
            Bar(theme, gate.transform, new Vector2(0f, -20f), 390f, 18f, 1f);
            MedievalHudKit.AddIcon(gate.transform, "Hourglass", theme.HourglassIcon,
                new Vector2(-170f, -49f), new Vector2(34f, 34f));
            Text(theme, gate.transform, "Próxima onda  01:25", new Vector2(28f, -49f),
                new Vector2(340f, 34f), 23f, TextAlignmentOptions.Left, theme.GoldText);

            GameObject gold = Panel(theme, group.transform, "Gold Balance",
                new Vector2(740f, 466f), new Vector2(330f, 78f), theme.MetalPanel);
            MedievalHudKit.AddIcon(gold.transform, "Coin", theme.CoinIcon,
                new Vector2(-120f, 0f), new Vector2(52f, 52f));
            Text(theme, gold.transform, "145 319", new Vector2(36f, 0f),
                new Vector2(225f, 54f), 35f, TextAlignmentOptions.Center, theme.GoldText, true);

            GameObject wave = Panel(theme, group.transform, "Wave Alert",
                new Vector2(0f, 424f), new Vector2(760f, 102f), theme.MetalPanel);
            MedievalHudKit.AddIcon(wave.transform, "Helmet", theme.HelmetIcon,
                new Vector2(-322f, 0f), new Vector2(62f, 62f));
            Text(theme, wave.transform, "BATEDORES AVISTARAM INIMIGOS",
                new Vector2(35f, 18f), new Vector2(620f, 38f), 27f,
                TextAlignmentOptions.Center, theme.Title, true);
            Text(theme, wave.transform, "Ameaça 42  •  avanço pelo caminho norte",
                new Vector2(35f, -21f), new Vector2(620f, 31f), 20f,
                TextAlignmentOptions.Center, theme.Subtitle);

            GameObject prompt = Panel(theme, group.transform, "Interaction Prompt",
                new Vector2(0f, -470f), new Vector2(720f, 76f), theme.MetalPanel);
            GameObject key = Panel(theme, prompt.transform, "Key",
                new Vector2(-302f, 0f), new Vector2(54f, 48f), theme.GoldButton);
            Text(theme, key.transform, "E", Vector2.zero, new Vector2(50f, 44f), 27f,
                TextAlignmentOptions.Center, theme.GoldButtonLabel, true);
            Text(theme, prompt.transform, "FALAR COM O CAPITÃO DO PORTÃO",
                new Vector2(30f, 0f), new Vector2(590f, 46f), 25f,
                TextAlignmentOptions.Center, Color.white, true);

            GameObject marker = Panel(theme, group.transform, "World Alert Sample",
                new Vector2(-185f, 285f), new Vector2(110f, 116f), theme.WoodPanel);
            GameObject badge = Panel(theme, marker.transform, "Alert Badge",
                new Vector2(0f, 15f), new Vector2(58f, 60f), theme.RedButton);
            Text(theme, badge.transform, "!", Vector2.zero, new Vector2(54f, 54f), 37f,
                TextAlignmentOptions.Center, Color.white, true);
            Text(theme, marker.transform, "CAPITÃO", new Vector2(0f, -35f),
                new Vector2(100f, 25f), 16f, TextAlignmentOptions.Center,
                theme.GoldText, true);
        }

        private static void BuildTravel(MedievalHudTheme theme, Transform root)
        {
            GameObject group = ModalGroup(theme, root, "Travel Modal");
            GameObject board = Panel(theme, group.transform, "Travel Board", Vector2.zero,
                new Vector2(880f, 590f), theme.WoodPanel);
            AddFrame(theme, board.transform, 880f, 590f);
            AddHeader(theme, board.transform, "CONDUTOR DA CIDADE", 790f, 78f, 230f);

            GameObject paper = Panel(theme, board.transform, "Travel Details",
                new Vector2(0f, 5f), new Vector2(720f, 320f), theme.ParchmentPanel);
            MedievalHudKit.AddIcon(paper.transform, "Helmet", theme.HelmetIcon,
                new Vector2(-272f, 86f), new Vector2(82f, 82f));
            Text(theme, paper.transform, "DESTINO", new Vector2(-72f, 104f),
                new Vector2(300f, 36f), 22f, TextAlignmentOptions.Left, theme.PriceText, true);
            Text(theme, paper.transform, "CIDADELA CENTRAL", new Vector2(64f, 64f),
                new Vector2(430f, 48f), 34f, TextAlignmentOptions.Left,
                theme.ParchmentText, true);
            Divider(paper.transform, new Vector2(0f, 25f), 640f);
            Stat(theme, paper.transform, "Integridade do portão", "100 / 100", 0f);
            Stat(theme, paper.transform, "Tempo de viagem", "00:18", -63f,
                theme.HourglassIcon);
            Text(theme, paper.transform,
                "A viagem encerra conversas e interfaces abertas.",
                new Vector2(0f, -126f), new Vector2(620f, 32f), 18f,
                TextAlignmentOptions.Center, theme.PriceText);

            MedievalHudKit.AddButton(theme, board.transform, "Confirm Travel",
                new Vector2(-170f, -244f), new Vector2(300f, 66f),
                "VIAJAR", Color.white, 28f, theme.GoldButton);
            MedievalHudKit.AddButton(theme, board.transform, "Cancel Travel",
                new Vector2(170f, -244f), new Vector2(300f, 66f),
                "CANCELAR  [ESC]", Color.white, 25f, theme.BronzeButton);
            group.SetActive(false);
        }

        private static void BuildGarrison(MedievalHudTheme theme, Transform root)
        {
            GameObject group = ModalGroup(theme, root, "Garrison Modal");
            GameObject board = Panel(theme, group.transform, "Garrison Board", Vector2.zero,
                new Vector2(1480f, 900f), theme.WoodPanel);
            AddFrame(theme, board.transform, 1480f, 900f);
            AddHeader(theme, board.transform, "CAPITÃO — PORTÃO NORTE", 1350f, 82f, 364f);

            GameObject summary = Panel(theme, board.transform, "Battle Summary",
                new Vector2(-465f, 86f), new Vector2(420f, 430f), theme.ParchmentPanel);
            Text(theme, summary.transform, "RELATÓRIO DA GUARDA", new Vector2(0f, 172f),
                new Vector2(370f, 42f), 27f, TextAlignmentOptions.Center,
                theme.PriceText, true);
            Divider(summary.transform, new Vector2(0f, 140f), 350f);
            SummaryLine(theme, summary.transform, "Integridade", "100 / 100", 94f, theme.Success);
            SummaryLine(theme, summary.transform, "Ameaça", "42", 41f, new Color(0.66f, 0.18f, 0.12f));
            SummaryLine(theme, summary.transform, "Próxima onda", "01:25", -12f, theme.PriceText);
            Divider(summary.transform, new Vector2(0f, -52f), 350f);
            Text(theme, summary.transform, "Reforce a linha de frente antes\ndo próximo ataque.",
                new Vector2(0f, -108f), new Vector2(350f, 74f), 22f,
                TextAlignmentOptions.Center, theme.ParchmentText);
            MedievalHudKit.AddIcon(summary.transform, "Helmet", theme.HelmetIcon,
                new Vector2(0f, -172f), new Vector2(58f, 58f));

            Text(theme, summary.transform, "ONDA 3 REPELIDA\nDefesa 48 / Ameaça 42\nPerdas: G 2 • A 1 • M 0\nEspólio: +8 ouro",
                new Vector2(0f, -255f), new Vector2(370f, 110f), 20f,
                TextAlignmentOptions.Center, theme.Subtitle);
            MedievalHudKit.AddButton(theme, board.transform, "Acknowledge Report",
                new Vector2(-465f, -270f), new Vector2(370f, 54f),
                "CONFIRMAR RELATÓRIO", Color.white, 22f, theme.BronzeButton);

            GameObject roster = Panel(theme, board.transform, "Garrison Roster",
                new Vector2(255f, 34f), new Vector2(840f, 470f), theme.ParchmentPanel);
            Text(theme, roster.transform, "GUARNIÇÃO E RESERVA", new Vector2(0f, 190f),
                new Vector2(760f, 43f), 29f, TextAlignmentOptions.Center,
                theme.PriceText, true);
            Text(theme, roster.transform, "NO PORTÃO", new Vector2(80f, 152f),
                new Vector2(150f, 30f), 18f, TextAlignmentOptions.Center, theme.PriceText, true);
            Text(theme, roster.transform, "RESERVA", new Vector2(260f, 152f),
                new Vector2(140f, 30f), 18f, TextAlignmentOptions.Center, theme.PriceText, true);
            UnitRow(theme, roster.transform, "GUERREIROS", theme.WarriorBanner, 90f, 18, 50);
            UnitRow(theme, roster.transform, "ARQUEIROS", theme.ArcherBanner, -18f, 12, 50);
            UnitRow(theme, roster.transform, "MAGOS", theme.MageBanner, -126f, 6, 40);
            Text(theme, roster.transform,
                "Receber: reserva → portão   •   Enviar: portão → destino",
                new Vector2(0f, -202f), new Vector2(760f, 34f), 18f,
                TextAlignmentOptions.Center, theme.PriceText);

            Text(theme, board.transform, "Destino dos envios: PORTÃO SUL",
                new Vector2(90f, -250f), new Vector2(510f, 42f), 25f,
                TextAlignmentOptions.Left, theme.GoldText);
            MedievalHudKit.AddButton(theme, board.transform, "Change Destination",
                new Vector2(530f, -250f), new Vector2(270f, 54f),
                "TROCAR DESTINO", Color.white, 22f, theme.BronzeButton);
            Text(theme, board.transform, "EM TRÂNSITO: 10 guerreiros • Norte → Sul • 18s",
                new Vector2(255f, -310f), new Vector2(810f, 42f), 23f,
                TextAlignmentOptions.Center, theme.Subtitle);
            Text(theme, board.transform, "10 guerreiros em marcha.",
                new Vector2(-230f, -360f), new Vector2(770f, 44f), 24f,
                TextAlignmentOptions.Left, theme.Success);
            MedievalHudKit.AddButton(theme, board.transform, "Close Garrison",
                new Vector2(580f, -360f), new Vector2(200f, 68f),
                "FECHAR  [ESC]", Color.white, 23f, theme.BronzeButton);
            group.SetActive(false);
        }

        private static void BuildAlert(MedievalHudTheme theme, Transform root)
        {
            GameObject group = ModalGroup(theme, root, "Alert Modal");
            GameObject board = Panel(theme, group.transform, "Alert Board", Vector2.zero,
                new Vector2(760f, 360f), theme.WoodPanel);
            AddFrame(theme, board.transform, 760f, 360f);
            AddHeader(theme, board.transform, "ATENÇÃO NO PORTÃO", 660f, 70f, 125f);
            GameObject paper = Panel(theme, board.transform, "Alert Content",
                new Vector2(0f, 5f), new Vector2(620f, 150f), theme.ParchmentPanel);
            MedievalHudKit.AddIcon(paper.transform, "Helmet", theme.HelmetIcon,
                new Vector2(-242f, 0f), new Vector2(76f, 76f));
            Text(theme, paper.transform, "A reserva não possui soldados\nsuficientes para essa formação.",
                new Vector2(54f, 0f), new Vector2(450f, 90f), 25f,
                TextAlignmentOptions.Center, theme.ParchmentText);
            MedievalHudKit.AddButton(theme, board.transform, "Acknowledge Alert",
                new Vector2(0f, -132f), new Vector2(280f, 58f),
                "ENTENDIDO", Color.white, 25f, theme.RedButton);
            group.SetActive(false);
        }

        private static GameObject ModalGroup(MedievalHudTheme theme, Transform root, string name)
        {
            GameObject group = Group(root, name);
            GameObject overlay = MedievalHudKit.AddPanel(group.transform, "Modal Overlay",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, theme.ModalOverlay);
            overlay.transform.SetAsFirstSibling();
            return group;
        }

        private static GameObject Group(Transform parent, string name)
        {
            GameObject group = new GameObject(name, typeof(RectTransform));
            group.transform.SetParent(parent, false);
            RectTransform rect = group.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return group;
        }

        private static GameObject Panel(MedievalHudTheme theme, Transform parent, string name,
            Vector2 position, Vector2 size, Sprite sprite)
        {
            GameObject panel = MedievalHudKit.AddFixedPanel(parent, name, position, size, Color.white);
            MedievalHudKit.ApplyStyle(panel, sprite, Color.white);
            return panel;
        }

        private static TMP_Text Text(MedievalHudTheme theme, Transform parent, string value,
            Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment,
            Color color, bool bold = false)
        {
            TMP_Text text = MedievalHudKit.AddText(theme, parent, bold ? "Title" : "Text",
                value, position, size, fontSize, alignment, color);
            if (bold) text.fontStyle = FontStyles.Bold;
            return text;
        }

        private static void AddHeader(MedievalHudTheme theme, Transform parent, string title,
            float width, float height, float y = 0f)
        {
            GameObject header = Panel(theme, parent, "Header", new Vector2(0f, y),
                new Vector2(width, height), theme.MetalPanel);
            MedievalHudKit.AddIcon(header.transform, "Header Drape", theme.HeaderDrape,
                new Vector2(-width * .5f + 40f, 0f), new Vector2(44f, height - 10f));
            Text(theme, header.transform, title, Vector2.zero, new Vector2(width - 70f, height - 10f),
                Mathf.Min(35f, height * 0.48f), TextAlignmentOptions.Center, theme.Title, true);
        }

        private static void AddFrame(MedievalHudTheme theme, Transform parent, float width, float height)
        {
            const float rail = 28f;
            MedievalHudKit.AddFramePiece(parent, "Frame Top", new Vector2(0f, height * .5f - 14f),
                new Vector2(width, rail), theme.MetalPanel);
            MedievalHudKit.AddFramePiece(parent, "Frame Bottom", new Vector2(0f, -height * .5f + 14f),
                new Vector2(width, rail), theme.MetalPanel);
            MedievalHudKit.AddFramePiece(parent, "Frame Left", new Vector2(-width * .5f + 14f, 0f),
                new Vector2(rail, height), theme.MetalPanel);
            MedievalHudKit.AddFramePiece(parent, "Frame Right", new Vector2(width * .5f - 14f, 0f),
                new Vector2(rail, height), theme.MetalPanel);
        }

        private static void Bar(MedievalHudTheme theme, Transform parent, Vector2 position,
            float width, float height, float fill)
        {
            GameObject back = Panel(theme, parent, "Bar Frame", position,
                new Vector2(width, height), theme.MetalPanel);
            GameObject value = MedievalHudKit.AddFixedPanel(back.transform, "Bar Value",
                new Vector2((width - 12f) * (fill - 1f) * .5f, 0f),
                new Vector2((width - 12f) * fill, height - 8f),
                new Color(0.42f, 0.70f, 0.24f, 1f));
            value.GetComponent<Image>().raycastTarget = false;
        }

        private static void Divider(Transform parent, Vector2 position, float width)
        {
            GameObject line = MedievalHudKit.AddFixedPanel(parent, "Divider", position,
                new Vector2(width, 3f), new Color(0.38f, 0.24f, 0.12f, .7f));
            line.GetComponent<Image>().raycastTarget = false;
        }

        private static void Stat(MedievalHudTheme theme, Transform parent, string label,
            string value, float y, Sprite icon = null)
        {
            if (icon != null)
                MedievalHudKit.AddIcon(parent, label + " Icon", icon,
                    new Vector2(-268f, y), new Vector2(38f, 38f));
            Text(theme, parent, label, new Vector2(-95f, y), new Vector2(310f, 36f), 21f,
                TextAlignmentOptions.Left, theme.ParchmentText, true);
            Text(theme, parent, value, new Vector2(220f, y), new Vector2(160f, 36f), 23f,
                TextAlignmentOptions.Right, theme.PriceText, true);
        }

        private static void SummaryLine(MedievalHudTheme theme, Transform parent, string label,
            string value, float y, Color valueColor)
        {
            Text(theme, parent, label, new Vector2(-75f, y), new Vector2(200f, 34f), 22f,
                TextAlignmentOptions.Left, theme.ParchmentText);
            Text(theme, parent, value, new Vector2(115f, y), new Vector2(130f, 34f), 23f,
                TextAlignmentOptions.Right, valueColor, true);
        }

        private static void UnitRow(MedievalHudTheme theme, Transform parent, string unit,
            Sprite banner, float y, int stationed, int reserve)
        {
            Divider(parent, new Vector2(0f, y - 50f), 760f);
            MedievalHudKit.AddIcon(parent, unit + " Banner", banner,
                new Vector2(-337f, y), new Vector2(64f, 78f));
            Text(theme, parent, unit, new Vector2(-215f, y + 18f), new Vector2(170f, 40f), 23f,
                TextAlignmentOptions.Left, theme.ParchmentText, true);
            Text(theme, parent, stationed.ToString(), new Vector2(78f, y),
                new Vector2(90f, 40f), 28f, TextAlignmentOptions.Center, theme.PriceText, true);
            Text(theme, parent, reserve.ToString(), new Vector2(258f, y),
                new Vector2(90f, 40f), 28f, TextAlignmentOptions.Center, theme.PriceText, true);
            string[] labels = { "RECEBER 1", "RECEBER 10", "ENVIAR 1", "ENVIAR 10" };
            for (int i = 0; i < labels.Length; i++)
                MedievalHudKit.AddButton(theme, parent, unit + " " + labels[i],
                    new Vector2(-180f + i * 151f, y - 28f), new Vector2(143f, 36f),
                    labels[i], Color.white, 17f,
                    i < 2 ? theme.GoldButton : theme.BronzeButton);
        }
    }
}
#endif
