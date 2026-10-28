#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using TCCGame.Core;
using TCCGame.Editor.UI;
using TCCGame.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace TCCGame.Editor
{
    public static class RecruitmentPrototypeSetup
    {
        private const string PrefabPath =
            "Assets/Resources/UI/RecruitmentCanvas.prefab";
        private const string GateScenePath = "Assets/Scenes/Portão.unity";
        private const string StyleFolder =
            "Assets/Resources/UI/RecruitmentStyle";
        private const string ConceptPath =
            "Assets/Images/UI/Recruitment/RecruitmentHudVisualDirection.png";
        private const string SpriteSourceFolder =
            "Assets/Images/UI/Recruitment/Sprites";
        private const string PortraitFolder =
            "Assets/Images/UI/Recruitment/PortraitsV2";
        private const string UiFontSourcePath =
            "Assets/Fonts/PixelifySans/PixelifySans-Variable.ttf";
        private const string UiFontAssetPath =
            "Assets/Fonts/PixelifySans/PixelifySans SDF.asset";

        private static Sprite woodPanel;
        private static Sprite parchmentPanel;
        private static Sprite metalPanel;
        private static Sprite goldButton;
        private static Sprite bronzeButton;
        private static Sprite redButton;
        private static Sprite warriorBanner;
        private static Sprite archerBanner;
        private static Sprite mageBanner;
        private static Sprite coinIcon;
        private static Sprite hourglassIcon;
        private static Sprite checkIcon;
        private static Sprite helmetIcon;
        private static Sprite drapeSprite;
        private static TMP_FontAsset uiFont;
        private static MedievalHudTheme theme;

        [MenuItem("TCC Game/UI/Rebuild Recruitment Prototype")]
        public static void RebuildAndAssign()
        {
            CreateStyleAssets();
            CreatePrefab();
            AssignToGateScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreatePrefab()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");

            GameObject root = new GameObject(
                "Recruitment Prototype Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject overlay = AddPanel(
                root.transform,
                "Recruitment Dialog",
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                theme.ModalOverlay
            );
            GameObject board = AddFixedPanel(
                overlay.transform,
                "Prototype Board",
                Vector2.zero,
                new Vector2(1540f, 860f),
                Color.white
            );
            ApplyStyle(board, woodPanel, Color.white);

            GameObject headerPlate = AddFixedPanel(
                board.transform,
                "Header Plate",
                new Vector2(-105f, 378f),
                new Vector2(1100f, 72f),
                Color.white
            );
            ApplyStyle(headerPlate, metalPanel, Color.white);

            AddDrape(board.transform, "Left Header Drape", new Vector2(-690f, 358f));
            AddDrape(board.transform, "Right Header Drape", new Vector2(470f, 358f));

            GameObject subtitlePlate = AddFixedPanel(board.transform, "Subtitle Plate",
                new Vector2(-105f, 320f), new Vector2(1020f, 32f), Color.white);
            ApplyStyle(subtitlePlate, metalPanel, Color.white);

            GameObject goldPlate = AddFixedPanel(
                board.transform,
                "Gold Plate",
                new Vector2(610f, 378f),
                new Vector2(190f, 58f),
                Color.white
            );
            ApplyStyle(goldPlate, metalPanel, Color.white);

            AddText(board.transform, "Title", "POSTO DE RECRUTAMENTO",
                new Vector2(-105f, 378f), new Vector2(980f, 46f), 32f,
                TextAlignmentOptions.Center,
                theme.Title);
            AddText(board.transform, "Subtitle",
                "Portão atual  •  Destino: Reserva central",
                new Vector2(-105f, 320f), new Vector2(960f, 24f), 18f,
                TextAlignmentOptions.Center,
                theme.Subtitle);
            TMP_Text goldLabel = AddText(board.transform, "Gold", "Ouro: 1000",
                new Vector2(633f, 378f), new Vector2(132f, 42f), 22f,
                TextAlignmentOptions.Left,
                theme.GoldText);
            goldLabel.enableAutoSizing = true;
            goldLabel.fontSizeMin = 14f;
            goldLabel.fontSizeMax = 22f;
            goldLabel.textWrappingMode = TextWrappingModes.NoWrap;
            AddIcon(board.transform, "Gold Icon", coinIcon,
                new Vector2(540f, 378f), new Vector2(34f, 34f));

            Button close = AddButton(board.transform, "Close",
                new Vector2(735f, 372f), new Vector2(54f, 54f), "X",
                Color.white, 24f, redButton);

            CreateUnitCard(board.transform, UnitType.Warrior,
                "GUERREIRO", "Linha de frente", "Warrior",
                new Vector2(-480f, -20f), new Color(0.58f, 0.16f, 0.13f, 1f));
            CreateUnitCard(board.transform, UnitType.Archer,
                "ARQUEIRO", "Ataque à distância", "Archer",
                new Vector2(0f, -20f), new Color(0.25f, 0.37f, 0.30f, 1f));
            CreateUnitCard(board.transform, UnitType.Mage,
                "MAGO", "Dano arcano", "Mage",
                new Vector2(480f, -20f), new Color(0.18f, 0.28f, 0.57f, 1f));

            GameObject timerPlate = AddFixedPanel(
                board.transform, "Timer Plate", new Vector2(-452f, -382f),
                new Vector2(540f, 50f), Color.white);
            ApplyStyle(timerPlate, metalPanel, Color.white);
            GameObject feedbackPlate = AddFixedPanel(
                board.transform, "Feedback Plate", new Vector2(270f, -382f),
                new Vector2(890f, 50f), Color.white);
            ApplyStyle(feedbackPlate, metalPanel, Color.white);

            AddText(board.transform, "Timer", "Novos recrutas em 06:00:00",
                new Vector2(-432f, -382f), new Vector2(455f, 36f), 19f,
                TextAlignmentOptions.Left, Color.white);
            AddIcon(board.transform, "Timer Icon", hourglassIcon,
                new Vector2(-693f, -382f), new Vector2(30f, 30f));
            AddText(board.transform, "Feedback",
                "Selecione a quantidade e confirme a contratação.",
                new Vector2(305f, -382f), new Vector2(770f, 36f), 19f,
                TextAlignmentOptions.Right,
                theme.Success);
            AddIcon(board.transform, "Feedback Icon", checkIcon,
                new Vector2(-130f, -382f), new Vector2(36f, 36f));

            AddBoardFrame(board.transform);
            close.transform.SetAsLastSibling();

            Navigation closeNavigation = close.navigation;
            closeNavigation.mode = Navigation.Mode.Automatic;
            close.navigation = closeNavigation;

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void CreateUnitCard(
            Transform parent,
            UnitType type,
            string title,
            string role,
            string portraitAssetName,
            Vector2 position,
            Color accent
        )
        {
            GameObject card = AddFixedPanel(parent, $"{title} Card", position,
                new Vector2(430f, 620f), Color.white);
            ApplyStyle(card, woodPanel, Color.white);
            RecruitmentRowView row = card.AddComponent<RecruitmentRowView>();

            GameObject parchment = AddPanel(card.transform, "Parchment",
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(12f, 14f), new Vector2(-12f, -82f),
                Color.white);
            ApplyStyle(parchment, parchmentPanel, Color.white);

            AddPanel(card.transform, "Accent", new Vector2(0f, 1f),
                new Vector2(1f, 1f), Vector2.zero,
                new Vector2(0f, 8f), accent);
            GameObject portrait = AddFixedPanel(card.transform, "Portrait Placeholder",
                new Vector2(0f, 176f), new Vector2(374f, 224f),
                Color.white);
            ApplyStyle(portrait, metalPanel, Color.white);
            Sprite portraitSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                $"{PortraitFolder}/{portraitAssetName}.png"
            );
            GameObject portraitImage = AddFixedPanel(
                portrait.transform, "Portrait", Vector2.zero,
                new Vector2(350f, 200f), Color.white);
            Image portraitGraphic = portraitImage.GetComponent<Image>();
            portraitGraphic.sprite = portraitSprite;
            portraitGraphic.preserveAspect = true;

            Sprite banner = type == UnitType.Warrior
                ? warriorBanner
                : type == UnitType.Archer ? archerBanner : mageBanner;
            AddIcon(card.transform, "Class Banner", banner,
                new Vector2(-142f, 213f), new Vector2(68f, 108f));

            GameObject nameRibbon = AddFixedPanel(card.transform, "Name Ribbon",
                new Vector2(0f, 66f), new Vector2(360f, 50f), Color.white);
            ApplyStyle(nameRibbon, parchmentPanel, Color.white);
            AddText(nameRibbon.transform, "Unit Name", title,
                Vector2.zero, new Vector2(330f, 42f), 25f,
                TextAlignmentOptions.Center,
                new Color(0.17f, 0.11f, 0.07f, 1f));
            AddIcon(card.transform, "Reserve Icon", helmetIcon,
                new Vector2(-140f, 10f), new Vector2(46f, 46f));

            TMP_Text stock = AddText(card.transform, "Stock", "Estoque: 10",
                new Vector2(18f, 22f), new Vector2(245f, 27f), 17f,
                TextAlignmentOptions.Left,
                theme.ParchmentText);
            TMP_Text owned = AddText(card.transform, "Owned", "Na reserva: 0",
                new Vector2(18f, -7f), new Vector2(245f, 27f), 17f,
                TextAlignmentOptions.Left,
                theme.ParchmentText);
            TMP_Text price = AddText(card.transform, "Price", "Unidade: 3 ouro",
                new Vector2(20f, -155f), new Vector2(250f, 24f), 17f,
                TextAlignmentOptions.Center,
                theme.PriceText);
            AddIcon(card.transform, "Price Icon", coinIcon,
                new Vector2(-80f, -155f), new Vector2(30f, 30f));

            Button decrease = AddButton(card.transform, "Decrease",
                new Vector2(-146f, -52f), new Vector2(58f, 52f), "−",
                Color.white, 25f, metalPanel);
            TMP_Text quantity = AddText(card.transform, "Quantity", "1",
                new Vector2(0f, -52f), new Vector2(190f, 52f), 25f,
                TextAlignmentOptions.Center,
                new Color(0.18f, 0.13f, 0.09f, 1f));
            GameObject quantityPlate = AddFixedPanel(card.transform, "Quantity Plate",
                new Vector2(0f, -52f), new Vector2(184f, 52f), Color.white);
            ApplyStyle(quantityPlate, parchmentPanel, Color.white);
            quantity.transform.SetAsLastSibling();
            Button increase = AddButton(card.transform, "Increase",
                new Vector2(146f, -52f), new Vector2(58f, 52f), "+",
                Color.white, 25f, metalPanel);

            Button one = AddButton(card.transform, "Select 1",
                new Vector2(-132f, -112f), new Vector2(68f, 40f), "1",
                Color.white, 18f, bronzeButton);
            Button five = AddButton(card.transform, "Select 5",
                new Vector2(-44f, -112f), new Vector2(68f, 40f), "5",
                Color.white, 18f, bronzeButton);
            Button ten = AddButton(card.transform, "Select 10",
                new Vector2(44f, -112f), new Vector2(68f, 40f), "10",
                Color.white, 18f, bronzeButton);
            Button maximum = AddButton(card.transform, "Select Maximum",
                new Vector2(132f, -112f), new Vector2(86f, 40f), "MAX",
                Color.white, 17f, bronzeButton);

            TMP_Text total = AddText(card.transform, "Total", "Total: 3 ouro",
                new Vector2(0f, -183f), new Vector2(340f, 24f), 17f,
                TextAlignmentOptions.Center,
                new Color(0.21f, 0.15f, 0.09f, 1f));
            TMP_Text status = AddText(card.transform, "Status",
                "Pronto para contratar", new Vector2(0f, -210f),
                new Vector2(350f, 20f), 15f,
                TextAlignmentOptions.Center,
                new Color(0.62f, 0.82f, 0.58f, 1f));
            Button purchase = AddButton(card.transform, "Confirm Purchase",
                new Vector2(0f, -249f), new Vector2(350f, 48f),
                "CONTRATAR", Color.white, 21f, goldButton);
            TMP_Text actionLabel = purchase.GetComponentInChildren<TMP_Text>();

            UnityEventTools.AddPersistentListener(
                decrease.onClick, row.DecreaseQuantity);
            UnityEventTools.AddPersistentListener(
                increase.onClick, row.IncreaseQuantity);
            UnityEventTools.AddPersistentListener(one.onClick, row.SelectOne);
            UnityEventTools.AddPersistentListener(five.onClick, row.SelectFive);
            UnityEventTools.AddPersistentListener(ten.onClick, row.SelectTen);
            UnityEventTools.AddPersistentListener(
                maximum.onClick, row.SelectMaximum);
            UnityEventTools.AddPersistentListener(
                purchase.onClick, row.ConfirmPurchase);

            SerializedObject serializedRow = new SerializedObject(row);
            serializedRow.FindProperty("unitType").enumValueIndex = (int)type;
            serializedRow.FindProperty("stockText").objectReferenceValue = stock;
            serializedRow.FindProperty("ownedText").objectReferenceValue = owned;
            serializedRow.FindProperty("priceText").objectReferenceValue = price;
            serializedRow.FindProperty("quantityText").objectReferenceValue = quantity;
            serializedRow.FindProperty("totalCostText").objectReferenceValue = total;
            serializedRow.FindProperty("statusText").objectReferenceValue = status;
            serializedRow.FindProperty("actionButtonText").objectReferenceValue = actionLabel;
            serializedRow.FindProperty("decreaseButton").objectReferenceValue = decrease;
            serializedRow.FindProperty("increaseButton").objectReferenceValue = increase;
            serializedRow.FindProperty("purchaseButton").objectReferenceValue = purchase;
            serializedRow.ApplyModifiedPropertiesWithoutUndo();

            AddCardFrame(card.transform);
        }

        private static void AssignToGateScene()
        {
            var scene = EditorSceneManager.OpenScene(
                GateScenePath, OpenSceneMode.Single);
            NPCVenda vendor = UnityEngine.Object.FindFirstObjectByType<NPCVenda>();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (vendor == null || prefab == null)
            {
                throw new InvalidOperationException(
                    "Vendor or recruitment prototype prefab was not found.");
            }

            GameObject previous = vendor.popup;
            if (previous != null)
            {
                UnityEngine.Object.DestroyImmediate(previous);
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "Recruitment Prototype Canvas";
            RecruitmentRowView[] rows = instance
                .GetComponentsInChildren<RecruitmentRowView>(true);
            RecruitmentRowView archer = rows.Single(row =>
                row.UnitType == UnitType.Archer);
            RecruitmentRowView warrior = rows.Single(row =>
                row.UnitType == UnitType.Warrior);
            RecruitmentRowView mage = rows.Single(row =>
                row.UnitType == UnitType.Mage);

            SerializedObject serializedVendor = new SerializedObject(vendor);
            serializedVendor.FindProperty("popup").objectReferenceValue = instance;
            serializedVendor.FindProperty("arqueiro_texto").objectReferenceValue =
                archer.StockText;
            serializedVendor.FindProperty("guerreiro_texto").objectReferenceValue =
                warrior.StockText;
            serializedVendor.FindProperty("mago_texto").objectReferenceValue =
                mage.StockText;
            serializedVendor.FindProperty("tempo_texto").objectReferenceValue =
                FindNamed<TMP_Text>(instance, "Timer");
            serializedVendor.FindProperty("goldText").objectReferenceValue =
                FindNamed<TMP_Text>(instance, "Gold");
            serializedVendor.FindProperty("feedbackText").objectReferenceValue =
                FindNamed<TMP_Text>(instance, "Feedback");
            serializedVendor.FindProperty("closeButton").objectReferenceValue =
                FindNamed<Button>(instance, "Close");
            serializedVendor.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static T FindNamed<T>(GameObject root, string name)
            where T : Component
        {
            return root.GetComponentsInChildren<T>(true)
                .Single(item => item.name == name);
        }

        private static GameObject AddPanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin,
            Vector2 offsetMax, Color color)
        {
            return MedievalHudKit.AddPanel(parent, name, anchorMin, anchorMax,
                offsetMin, offsetMax, color);
        }

        private static GameObject AddFixedPanel(Transform parent, string name,
            Vector2 position, Vector2 size, Color color)
        {
            return MedievalHudKit.AddFixedPanel(parent, name, position, size,
                color);
        }

        private static TMP_Text AddText(Transform parent, string name,
            string value, Vector2 position, Vector2 size, float fontSize,
            TextAlignmentOptions alignment, Color color)
        {
            return MedievalHudKit.AddText(theme, parent, name, value, position,
                size, fontSize, alignment, color);
        }

        private static Button AddButton(Transform parent, string name,
            Vector2 position, Vector2 size, string label, Color color,
            float fontSize, Sprite sprite = null)
        {
            return MedievalHudKit.AddButton(theme, parent, name, position, size,
                label, color, fontSize, sprite);
        }

        private static void AddDrape(
            Transform parent,
            string name,
            Vector2 position
        )
        {
            GameObject drape = AddFixedPanel(parent, name, position,
                new Vector2(62f, 112f), Color.white);
            Image image = drape.GetComponent<Image>();
            image.sprite = drapeSprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        private static void AddBoardFrame(Transform board)
        {
            AddFramePiece(board, "Top Wood Rail", new Vector2(0f, 422f),
                new Vector2(1495f, 30f), woodPanel);
            AddFramePiece(board, "Bottom Wood Rail", new Vector2(0f, -422f),
                new Vector2(1495f, 30f), woodPanel);
            AddFramePiece(board, "Left Iron Post", new Vector2(-754f, 0f),
                new Vector2(34f, 820f), metalPanel);
            AddFramePiece(board, "Right Iron Post", new Vector2(754f, 0f),
                new Vector2(34f, 820f), metalPanel);
            AddFramePiece(board, "Left Divider", new Vector2(-240f, -10f),
                new Vector2(18f, 625f), woodPanel);
            AddFramePiece(board, "Right Divider", new Vector2(240f, -10f),
                new Vector2(18f, 625f), woodPanel);

            foreach (Vector2 corner in new[]
            {
                new Vector2(-754f, -422f), new Vector2(754f, -422f),
                new Vector2(-754f, 422f), new Vector2(754f, 422f)
            })
            {
                AddFramePiece(board, "Forged Corner", corner,
                    new Vector2(54f, 62f), metalPanel);
            }
        }

        private static void AddCardFrame(Transform card)
        {
            AddFramePiece(card, "Left Card Post", new Vector2(-207f, 0f),
                new Vector2(18f, 590f), woodPanel);
            AddFramePiece(card, "Right Card Post", new Vector2(207f, 0f),
                new Vector2(18f, 590f), woodPanel);
            AddFramePiece(card, "Top Card Rail", new Vector2(0f, 298f),
                new Vector2(408f, 18f), woodPanel);
            AddFramePiece(card, "Bottom Card Rail", new Vector2(0f, -298f),
                new Vector2(408f, 18f), woodPanel);

            foreach (Vector2 corner in new[]
            {
                new Vector2(-207f, -298f), new Vector2(207f, -298f),
                new Vector2(-207f, 298f), new Vector2(207f, 298f)
            })
            {
                AddFramePiece(card, "Card Corner", corner,
                    new Vector2(25f, 25f), metalPanel);
            }
        }

        private static GameObject AddFramePiece(Transform parent, string name,
            Vector2 position, Vector2 size, Sprite sprite)
        {
            return MedievalHudKit.AddFramePiece(parent, name, position, size,
                sprite);
        }

        private static Image AddIcon(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            Vector2 size
        )
        {
            return MedievalHudKit.AddIcon(parent, name, sprite, position, size);
        }

        private static void AddButtonDetail(
            Transform button,
            Vector2 size,
            bool golden
        )
        {
            Color top = golden
                ? new Color(1f, 0.84f, 0.28f, 0.92f)
                : new Color(0.72f, 0.69f, 0.59f, 0.78f);
            Color bottom = new Color(0.08f, 0.07f, 0.06f, 0.82f);
            GameObject highlight = AddFixedPanel(button, "Top Bevel",
                new Vector2(0f, (size.y * 0.5f) - 7f),
                new Vector2(Mathf.Max(8f, size.x - 18f), 3f), top);
            GameObject shadow = AddFixedPanel(button, "Bottom Bevel",
                new Vector2(0f, (-size.y * 0.5f) + 7f),
                new Vector2(Mathf.Max(8f, size.x - 18f), 4f), bottom);
            highlight.GetComponent<Image>().raycastTarget = false;
            shadow.GetComponent<Image>().raycastTarget = false;

            Color rivetColor = golden
                ? new Color(0.43f, 0.23f, 0.07f, 1f)
                : new Color(0.92f, 0.66f, 0.18f, 1f);
            float rx = (size.x * 0.5f) - 8f;
            float ry = (size.y * 0.5f) - 8f;
            foreach (Vector2 corner in new[]
            {
                new Vector2(-rx, -ry), new Vector2(rx, -ry),
                new Vector2(-rx, ry), new Vector2(rx, ry)
            })
            {
                GameObject rivet = AddFixedPanel(button, "Rivet", corner,
                    new Vector2(4f, 4f), rivetColor);
                rivet.GetComponent<Image>().raycastTarget = false;
            }

            Transform label = button.Find("Label");
            if (label != null)
            {
                label.SetAsLastSibling();
            }
        }

        private static void CreateStyleAssets()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");
            EnsureFolder(StyleFolder);

            theme = AssetDatabase.LoadAssetAtPath<MedievalHudTheme>(MedievalHudKit.ThemeAssetPath);
            if (theme == null || !theme.IsComplete) theme = MedievalHudKit.CreateOrUpdateTheme();
            woodPanel = theme.WoodPanel;
            parchmentPanel = theme.ParchmentPanel;
            metalPanel = theme.MetalPanel;
            goldButton = theme.GoldButton;
            bronzeButton = theme.BronzeButton;
            redButton = theme.RedButton;
            warriorBanner = theme.WarriorBanner;
            archerBanner = theme.ArcherBanner;
            mageBanner = theme.MageBanner;
            coinIcon = theme.CoinIcon;
            hourglassIcon = theme.HourglassIcon;
            checkIcon = theme.CheckIcon;
            helmetIcon = theme.HelmetIcon;
            drapeSprite = theme.HeaderDrape;
            uiFont = theme.Font;

            foreach (string portrait in new[] { "Warrior", "Archer", "Mage" })
            {
                string path = $"{PortraitFolder}/{portrait}.png";
                if (!File.Exists(path)) throw new FileNotFoundException("Authored troop portrait is missing", path);
                ImportAsSlicedSprite(path, Vector4.zero);
            }
        }

        private static TMP_FontAsset LoadOrCreateUiFont()
        {
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                UiFontAssetPath);
            if (existing != null)
            {
                return existing;
            }

            AssetDatabase.ImportAsset(
                UiFontSourcePath,
                ImportAssetOptions.ForceSynchronousImport);
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(UiFontSourcePath);
            if (sourceFont == null)
            {
                throw new InvalidOperationException(
                    "Pixelify Sans source font could not be imported.");
            }

            TMP_FontAsset created = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                64,
                8,
                GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic,
                true);
            if (created == null)
            {
                throw new InvalidOperationException(
                    "Pixelify Sans TMP font asset could not be created.");
            }

            created.name = "PixelifySans SDF";
            Texture2D atlas = created.atlasTexture;
            Material material = created.material;
            AssetDatabase.CreateAsset(created, UiFontAssetPath);
            if (atlas != null)
            {
                atlas.name = "PixelifySans SDF Atlas";
                AssetDatabase.AddObjectToAsset(atlas, created);
            }
            if (material != null)
            {
                material.name = "PixelifySans SDF Material";
                AssetDatabase.AddObjectToAsset(material, created);
            }
            EditorUtility.SetDirty(created);
            AssetDatabase.SaveAssets();
            return created;
        }

        private static Sprite CreatePanelSprite(
            string assetName,
            Color32 fill,
            Color32 border,
            Color32 highlight,
            int seed,
            bool woodGrain
        )
        {
            const int size = 32;
            const int edge = 6;
            string path = $"{StyleFolder}/{assetName}.png";
            Texture2D texture = new Texture2D(
                size, size, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[size * size];
            System.Random random = new System.Random(seed);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int distance = Math.Min(
                        Math.Min(x, size - 1 - x),
                        Math.Min(y, size - 1 - y)
                    );
                    Color32 source = distance < 3
                        ? border
                        : distance < edge
                            ? highlight
                            : fill;
                    int noise = distance < edge
                        ? random.Next(-5, 6)
                        : 0;
                    if (woodGrain && distance < edge)
                    {
                        noise += ((y + seed) % 7 == 0) ? -7 : 0;
                    }

                    pixels[(y * size) + x] = Shift(source, noise);
                }
            }

            AddRivet(pixels, size, 4, 4);
            AddRivet(pixels, size, size - 5, 4);
            AddRivet(pixels, size, 4, size - 5);
            AddRivet(pixels, size, size - 5, size - 5);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            ImportAsSlicedSprite(path, new Vector4(edge, edge, edge, edge));
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateClassBanner(
            string assetName,
            Color32 cloth,
            int emblem
        )
        {
            const int width = 48;
            const int height = 72;
            string path = $"{StyleFolder}/{assetName}.png";
            Texture2D texture = new Texture2D(
                width, height, TextureFormat.RGBA32, false);
            Color32 clear = new Color32(0, 0, 0, 0);
            Color32 gold = new Color32(238, 174, 48, 255);
            Color32 ivory = new Color32(255, 241, 191, 255);
            Color32[] pixels = Enumerable.Repeat(clear, width * height).ToArray();

            for (int y = 8; y < height - 3; y++)
            {
                int inset = y < 18 ? 2 : 5;
                if (y < 17)
                {
                    inset = 2;
                }
                if (y < 13)
                {
                    inset = 0;
                }
                for (int x = inset; x < width - inset; x++)
                {
                    bool notch = y < 17 && Mathf.Abs(x - (width / 2)) < (17 - y);
                    if (notch)
                    {
                        continue;
                    }
                    bool edge = x == inset || x == width - inset - 1 ||
                        y == height - 4 || y == 17;
                    pixels[(y * width) + x] = edge ? gold : cloth;
                }
            }

            if (emblem == 0)
            {
                DrawRect(pixels, width, height, 22, 31, 4, 24, ivory);
                DrawRect(pixels, width, height, 17, 32, 14, 3, ivory);
                DrawRect(pixels, width, height, 20, 53, 8, 3, ivory);
            }
            else if (emblem == 1)
            {
                for (int i = 0; i < 24; i++)
                {
                    SetPixel(pixels, width, height, 13 + (i / 5), 31 + i, ivory);
                    SetPixel(pixels, width, height, 34 - (i / 5), 31 + i, ivory);
                }
                for (int i = 0; i < 26; i++)
                {
                    SetPixel(pixels, width, height, 11 + i, 42, ivory);
                }
                DrawRect(pixels, width, height, 34, 39, 3, 7, ivory);
            }
            else
            {
                DrawRect(pixels, width, height, 22, 31, 4, 25, ivory);
                DrawRect(pixels, width, height, 18, 51, 12, 3, ivory);
                DrawRect(pixels, width, height, 19, 27, 10, 10, ivory);
                DrawRect(pixels, width, height, 22, 24, 4, 16, gold);
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            ImportAsSlicedSprite(path, Vector4.zero);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreatePixelIcon(string assetName, int icon)
        {
            const int size = 32;
            string path = $"{StyleFolder}/{assetName}.png";
            Color32[] pixels = Enumerable.Repeat(
                new Color32(0, 0, 0, 0), size * size).ToArray();
            Color32 gold = new Color32(247, 178, 35, 255);
            Color32 light = new Color32(255, 230, 121, 255);
            Color32 dark = new Color32(104, 62, 23, 255);

            if (icon == 0)
            {
                DrawDisc(pixels, size, 11, 18, 7, dark);
                DrawDisc(pixels, size, 11, 20, 6, gold);
                DrawDisc(pixels, size, 21, 15, 7, dark);
                DrawDisc(pixels, size, 21, 17, 6, gold);
                DrawRect(pixels, size, size, 18, 21, 7, 2, light);
            }
            else if (icon == 1)
            {
                DrawRect(pixels, size, size, 7, 6, 18, 4, gold);
                DrawRect(pixels, size, size, 7, 23, 18, 4, gold);
                for (int i = 0; i < 11; i++)
                {
                    DrawRect(pixels, size, size, 10 + (i / 3), 10 + i,
                        12 - ((i / 3) * 2), 1, light);
                }
            }
            else if (icon == 2)
            {
                DrawDisc(pixels, size, 16, 16, 14, dark);
                DrawDisc(pixels, size, 16, 16, 11,
                    new Color32(56, 142, 48, 255));
                for (int i = 0; i < 7; i++)
                {
                    DrawRect(pixels, size, size, 8 + i, 15 + i, 4, 3, light);
                }
                for (int i = 0; i < 10; i++)
                {
                    DrawRect(pixels, size, size, 14 + i, 21 - i, 4, 3, light);
                }
            }
            else
            {
                DrawRect(pixels, size, size, 7, 13, 18, 9, dark);
                DrawRect(pixels, size, size, 10, 8, 12, 13,
                    new Color32(104, 91, 72, 255));
                DrawRect(pixels, size, size, 6, 20, 5, 7, dark);
                DrawRect(pixels, size, size, 21, 20, 5, 7, dark);
                DrawRect(pixels, size, size, 14, 8, 4, 14, light);
            }

            Texture2D texture = new Texture2D(
                size, size, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            ImportAsSlicedSprite(path, Vector4.zero);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite CreateDrapeSprite()
        {
            const int width = 48;
            const int height = 88;
            string path = $"{StyleFolder}/HeaderDrape.png";
            Color32 clear = new Color32(0, 0, 0, 0);
            Color32 red = new Color32(143, 39, 31, 255);
            Color32 darkRed = new Color32(91, 24, 23, 255);
            Color32 lightRed = new Color32(190, 57, 39, 255);
            Color32 gold = new Color32(230, 158, 43, 255);
            Color32[] pixels = Enumerable.Repeat(clear, width * height).ToArray();

            for (int y = 8; y < height - 4; y++)
            {
                int half = width / 2;
                int bottomDepth = Mathf.Max(0, 20 - y);
                for (int x = 4; x < width - 4; x++)
                {
                    bool notch = y < 20 && Mathf.Abs(x - half) < bottomDepth;
                    if (notch)
                    {
                        continue;
                    }
                    bool edge = x == 4 || x == width - 5 || y == 20 ||
                        y == height - 5;
                    Color32 color = edge
                        ? gold
                        : x < 13 || x > width - 14 ? darkRed
                        : x > 20 && x < 28 ? lightRed : red;
                    pixels[(y * width) + x] = color;
                }
            }
            DrawRect(pixels, width, height, 1, height - 14, width - 2, 7, gold);
            DrawRect(pixels, width, height, 5, height - 12, width - 10, 3,
                new Color32(72, 42, 23, 255));

            Texture2D texture = new Texture2D(
                width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            ImportAsSlicedSprite(path, Vector4.zero);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void DrawRect(Color32[] pixels, int width, int height,
            int x, int y, int rectWidth, int rectHeight, Color32 color)
        {
            for (int py = y; py < y + rectHeight; py++)
            {
                for (int px = x; px < x + rectWidth; px++)
                {
                    SetPixel(pixels, width, height, px, py, color);
                }
            }
        }

        private static void DrawDisc(Color32[] pixels, int size,
            int centerX, int centerY, int radius, Color32 color)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    if (((x - centerX) * (x - centerX)) +
                        ((y - centerY) * (y - centerY)) <= radius * radius)
                    {
                        SetPixel(pixels, size, size, x, y, color);
                    }
                }
            }
        }

        private static void SetPixel(Color32[] pixels, int width, int height,
            int x, int y, Color32 color)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                pixels[(y * width) + x] = color;
            }
        }

        private static void CreatePortraitCrops()
        {
            AssetDatabase.ImportAsset(
                ConceptPath,
                ImportAssetOptions.ForceSynchronousImport
            );
            TextureImporter conceptImporter = AssetImporter.GetAtPath(
                ConceptPath) as TextureImporter;
            if (conceptImporter == null)
            {
                throw new InvalidOperationException(
                    "Recruitment visual direction image was not found."
                );
            }

            conceptImporter.isReadable = true;
            conceptImporter.mipmapEnabled = false;
            conceptImporter.textureCompression =
                TextureImporterCompression.Uncompressed;
            conceptImporter.filterMode = FilterMode.Point;
            conceptImporter.SaveAndReimport();

            Texture2D concept = AssetDatabase.LoadAssetAtPath<Texture2D>(
                ConceptPath);
            CropPortrait(concept, "WARRIOR", 0.231f, 0.367f, 0.270f, 0.408f);
            CropPortrait(concept, "ARCHER", 0.468f, 0.604f, 0.270f, 0.408f);
            CropPortrait(concept, "MAGE", 0.685f, 0.821f, 0.270f, 0.408f);
        }

        private static Sprite LoadAuthoredSprite(string assetName)
        {
            string sourcePath = $"{SpriteSourceFolder}/{assetName}.png";
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException(
                    $"Authored recruitment sprite was not found: {sourcePath}");
            }

            Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            source.LoadImage(File.ReadAllBytes(sourcePath), false);
            Color32[] sourcePixels = source.GetPixels32();
            int minX = source.width;
            int minY = source.height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    if (sourcePixels[(y * source.width) + x].a < 96)
                    {
                        continue;
                    }

                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }

            if (maxX < minX || maxY < minY)
            {
                UnityEngine.Object.DestroyImmediate(source);
                throw new InvalidOperationException(
                    $"Authored recruitment sprite has no visible pixels: {sourcePath}");
            }

            const int padding = 8;
            minX = Math.Max(0, minX - padding);
            minY = Math.Max(0, minY - padding);
            maxX = Math.Min(source.width - 1, maxX + padding);
            maxY = Math.Min(source.height - 1, maxY + padding);
            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            Texture2D trimmed = new Texture2D(
                width, height, TextureFormat.RGBA32, false);
            trimmed.SetPixels(source.GetPixels(minX, minY, width, height));
            trimmed.Apply();

            string outputPath = $"{StyleFolder}/{assetName}.png";
            File.WriteAllBytes(outputPath, trimmed.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(source);
            UnityEngine.Object.DestroyImmediate(trimmed);
            ImportAsSlicedSprite(outputPath, Vector4.zero);
            return AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
        }

        private static void CropPortrait(
            Texture2D source,
            string assetName,
            float left,
            float right,
            float top,
            float bottom
        )
        {
            int x = Mathf.RoundToInt(source.width * left);
            int width = Mathf.RoundToInt(source.width * (right - left));
            int height = Mathf.RoundToInt(source.height * (bottom - top));
            int y = source.height - Mathf.RoundToInt(source.height * bottom);
            Color[] pixels = source.GetPixels(x, y, width, height);
            Texture2D crop = new Texture2D(
                width, height, TextureFormat.RGBA32, false);
            crop.SetPixels(pixels);
            crop.Apply();

            string path = $"{StyleFolder}/{assetName}.png";
            File.WriteAllBytes(path, crop.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(crop);
            ImportAsSlicedSprite(path, Vector4.zero);
        }

        private static void ImportAsSlicedSprite(
            string path,
            Vector4 border
        )
        {
            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport
            );
            TextureImporter importer = AssetImporter.GetAtPath(path)
                as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.spriteBorder = border;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression =
                TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void ApplyStyle(
            GameObject target,
            Sprite sprite,
            Color tint
        )
        {
            MedievalHudKit.ApplyStyle(target, sprite, tint);
        }

        private static Color32 Shift(Color32 color, int amount)
        {
            return new Color32(
                (byte)Mathf.Clamp(color.r + amount, 0, 255),
                (byte)Mathf.Clamp(color.g + amount, 0, 255),
                (byte)Mathf.Clamp(color.b + amount, 0, 255),
                color.a
            );
        }

        private static void AddRivet(
            Color32[] pixels,
            int size,
            int x,
            int y
        )
        {
            pixels[(y * size) + x] = new Color32(222, 172, 66, 255);
            pixels[((y - 1) * size) + x] = new Color32(110, 70, 28, 255);
            pixels[(y * size) + x - 1] = new Color32(110, 70, 28, 255);
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
