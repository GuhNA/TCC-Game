#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using TCCGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace TCCGame.Editor.UI
{
    /// <summary>Shared assets and primitives for the medieval pixel HUD.</summary>
    public static class MedievalHudKit
    {
        public const string ThemeAssetPath =
            "Assets/Resources/UI/Theme/MedievalPixelHudTheme.asset";
        private const string ThemeFolder =
            "Assets/Resources/UI/Theme/MedievalPixel";
        private const string SpriteSourceFolder =
            "Assets/Images/UI/Recruitment/Sprites";
        private const string FontSourcePath =
            "Assets/Fonts/PixelifySans/PixelifySans-Variable.ttf";
        private const string FontAssetPath =
            "Assets/Fonts/PixelifySans/PixelifySans SDF.asset";

        public static MedievalHudTheme CreateOrUpdateTheme()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");
            EnsureFolder("Assets/Resources/UI/Theme");
            EnsureFolder(ThemeFolder);

            Sprite wood = CreatePanelSprite("WoodPanel",
                new Color32(82, 48, 27, 255), new Color32(34, 27, 24, 255),
                new Color32(150, 92, 44, 255), 13, true);
            Sprite parchment = CreatePanelSprite("ParchmentPanel",
                new Color32(225, 202, 154, 255), new Color32(94, 61, 34, 255),
                new Color32(250, 229, 183, 255), 29, false);
            Sprite metal = CreatePanelSprite("MetalPanel",
                new Color32(47, 49, 50, 255), new Color32(18, 20, 22, 255),
                new Color32(105, 102, 91, 255), 41, false);
            Sprite gold = CreatePanelSprite("GoldButton",
                new Color32(204, 132, 28, 255), new Color32(80, 44, 17, 255),
                new Color32(255, 198, 57, 255), 53, false);
            Sprite bronze = CreatePanelSprite("BronzeButton",
                new Color32(105, 82, 57, 255), new Color32(43, 32, 25, 255),
                new Color32(183, 139, 80, 255), 59, false);
            Sprite red = CreatePanelSprite("RedButton",
                new Color32(139, 40, 31, 255), new Color32(55, 23, 20, 255),
                new Color32(222, 87, 54, 255), 67, false);

            MedievalHudTheme theme = AssetDatabase.LoadAssetAtPath<MedievalHudTheme>(
                ThemeAssetPath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<MedievalHudTheme>();
                AssetDatabase.CreateAsset(theme, ThemeAssetPath);
            }
            SerializedObject serialized = new SerializedObject(theme);
            Set(serialized, "font", LoadOrCreateFont());
            Set(serialized, "woodPanel", wood);
            Set(serialized, "parchmentPanel", parchment);
            Set(serialized, "metalPanel", metal);
            Set(serialized, "goldButton", gold);
            Set(serialized, "bronzeButton", bronze);
            Set(serialized, "redButton", red);
            Set(serialized, "warriorBanner", LoadAuthoredSprite("WarriorBanner"));
            Set(serialized, "archerBanner", LoadAuthoredSprite("ArcherBanner"));
            Set(serialized, "mageBanner", LoadAuthoredSprite("MageBanner"));
            Set(serialized, "coinIcon", LoadAuthoredSprite("CoinIcon"));
            Set(serialized, "hourglassIcon", LoadAuthoredSprite("HourglassIcon"));
            Set(serialized, "checkIcon", LoadAuthoredSprite("CheckIcon"));
            Set(serialized, "helmetIcon", LoadAuthoredSprite("HelmetIcon"));
            Set(serialized, "headerDrape", LoadAuthoredSprite("HeaderDrape"));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            return theme;
        }

        public static GameObject AddPanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin,
            Vector2 offsetMax, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            go.GetComponent<Image>().color = color;
            return go;
        }

        public static GameObject AddFixedPanel(Transform parent, string name,
            Vector2 position, Vector2 size, Color color)
        {
            GameObject go = AddPanel(parent, name,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, color);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return go;
        }

        public static TMP_Text AddText(MedievalHudTheme theme, Transform parent,
            string name, string value, Vector2 position, Vector2 size,
            float fontSize, TextAlignmentOptions alignment, Color color)
        {
            GameObject go = new GameObject(name,
                typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.font = theme.Font != null ? theme.Font : TMP_Settings.defaultFontAsset;
            label.text = value;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            label.extraPadding = true;
            if (name == "Title" || name == "Unit Name" || name == "Label")
            {
                label.fontStyle = FontStyles.Bold;
                label.outlineWidth = name == "Label" ? 0.14f : 0.10f;
                label.outlineColor = theme.TextOutline;
            }
            return label;
        }

        public static Button AddButton(MedievalHudTheme theme, Transform parent,
            string name, Vector2 position, Vector2 size, string label,
            Color color, float fontSize, Sprite sprite = null)
        {
            GameObject go = new GameObject(name,
                typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.color = color;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
            }
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.92f, 0.7f, 1f);
            colors.pressedColor = new Color(0.72f, 0.66f, 0.56f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.42f, 0.42f, 0.42f, 0.82f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.Automatic;
            button.navigation = navigation;
            bool golden = sprite == theme.GoldButton;
            TMP_Text buttonLabel = AddText(theme, go.transform, "Label", label,
                Vector2.zero, size, fontSize, TextAlignmentOptions.Center,
                golden ? theme.GoldButtonLabel : Color.white);
            buttonLabel.characterSpacing = golden ? 1.5f : 0.5f;
            AddButtonDetail(go.transform, size, golden);
            return button;
        }

        public static Image AddIcon(Transform parent, string name, Sprite sprite,
            Vector2 position, Vector2 size)
        {
            GameObject icon = AddFixedPanel(parent, name, position, size, Color.white);
            Image image = icon.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        public static GameObject AddFramePiece(Transform parent, string name,
            Vector2 position, Vector2 size, Sprite sprite)
        {
            GameObject piece = AddFixedPanel(parent, name, position, size, Color.white);
            ApplyStyle(piece, sprite, Color.white);
            piece.GetComponent<Image>().raycastTarget = false;
            return piece;
        }

        public static void ApplyStyle(GameObject target, Sprite sprite, Color tint)
        {
            Image image = target.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = tint;
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void AddButtonDetail(Transform button, Vector2 size,
            bool golden)
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
            if (label != null) label.SetAsLastSibling();
        }

        private static void Set(SerializedObject target, string property,
            UnityEngine.Object value)
        {
            target.FindProperty(property).objectReferenceValue = value;
        }

        private static TMP_FontAsset LoadOrCreateFont()
        {
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                FontAssetPath);
            if (existing != null) return existing;
            AssetDatabase.ImportAsset(FontSourcePath,
                ImportAssetOptions.ForceSynchronousImport);
            Font source = AssetDatabase.LoadAssetAtPath<Font>(FontSourcePath);
            if (source == null)
                throw new InvalidOperationException("Pixelify Sans could not be imported.");
            TMP_FontAsset created = TMP_FontAsset.CreateFontAsset(source, 64, 8,
                GlyphRenderMode.SDFAA, 1024, 1024,
                AtlasPopulationMode.Dynamic, true);
            if (created == null)
                throw new InvalidOperationException("Pixelify Sans TMP asset failed.");
            created.name = "PixelifySans SDF";
            Texture2D atlas = created.atlasTexture;
            Material material = created.material;
            AssetDatabase.CreateAsset(created, FontAssetPath);
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

        private static Sprite CreatePanelSprite(string assetName, Color32 fill,
            Color32 border, Color32 highlight, int seed, bool woodGrain)
        {
            const int size = 32;
            const int edge = 6;
            string path = $"{ThemeFolder}/{assetName}.png";
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[size * size];
            System.Random random = new System.Random(seed);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int distance = Math.Min(Math.Min(x, size - 1 - x),
                    Math.Min(y, size - 1 - y));
                Color32 source = distance < 3 ? border :
                    distance < edge ? highlight : fill;
                int noise = distance < edge ? random.Next(-5, 6) : 0;
                if (woodGrain && distance < edge)
                    noise += ((y + seed) % 7 == 0) ? -7 : 0;
                pixels[(y * size) + x] = Shift(source, noise);
            }
            AddRivet(pixels, size, 4, 4);
            AddRivet(pixels, size, size - 5, 4);
            AddRivet(pixels, size, 4, size - 5);
            AddRivet(pixels, size, size - 5, size - 5);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            ImportSprite(path, new Vector4(edge, edge, edge, edge));
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite LoadAuthoredSprite(string assetName)
        {
            string sourcePath = $"{SpriteSourceFolder}/{assetName}.png";
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"HUD sprite not found: {sourcePath}");
            Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            source.LoadImage(File.ReadAllBytes(sourcePath), false);
            Color32[] pixels = source.GetPixels32();
            int minX = source.width, minY = source.height, maxX = -1, maxY = -1;
            for (int y = 0; y < source.height; y++)
            for (int x = 0; x < source.width; x++)
            {
                if (pixels[(y * source.width) + x].a < 96) continue;
                minX = Math.Min(minX, x); minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
            }
            if (maxX < minX || maxY < minY)
            {
                UnityEngine.Object.DestroyImmediate(source);
                throw new InvalidOperationException($"HUD sprite has no pixels: {sourcePath}");
            }
            const int padding = 8;
            minX = Math.Max(0, minX - padding); minY = Math.Max(0, minY - padding);
            maxX = Math.Min(source.width - 1, maxX + padding);
            maxY = Math.Min(source.height - 1, maxY + padding);
            int width = maxX - minX + 1, height = maxY - minY + 1;
            Texture2D trimmed = new Texture2D(width, height, TextureFormat.RGBA32, false);
            trimmed.SetPixels(source.GetPixels(minX, minY, width, height));
            trimmed.Apply();
            string output = $"{ThemeFolder}/{assetName}.png";
            File.WriteAllBytes(output, trimmed.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(source);
            UnityEngine.Object.DestroyImmediate(trimmed);
            ImportSprite(output, Vector4.zero);
            return AssetDatabase.LoadAssetAtPath<Sprite>(output);
        }

        private static void ImportSprite(string path, Vector4 border)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.spriteBorder = border;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static Color32 Shift(Color32 color, int amount)
        {
            return new Color32((byte)Mathf.Clamp(color.r + amount, 0, 255),
                (byte)Mathf.Clamp(color.g + amount, 0, 255),
                (byte)Mathf.Clamp(color.b + amount, 0, 255), color.a);
        }

        private static void AddRivet(Color32[] pixels, int size, int x, int y)
        {
            pixels[(y * size) + x] = new Color32(222, 172, 66, 255);
            pixels[((y - 1) * size) + x] = new Color32(110, 70, 28, 255);
            pixels[(y * size) + x - 1] = new Color32(110, 70, 28, 255);
        }
    }
}
#endif
