#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using TCCGame.Editor.UI;
using TCCGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Editor
{
    /// <summary>Read-only scene inventory and isolated proposal; never saves Centro.</summary>
    public static class CentroHudProposalSetup
    {
        private static MedievalHudTheme theme;
        public const string PrefabPath = "Assets/Resources/UI/Prototypes/CentroHudProposalCanvas.prefab";

        [MenuItem("TCC Game/UI/Prepare Centro HUD Proposals")]
        public static void Generate()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Centro.unity", OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            var buildings = roots.SelectMany(root => root.GetComponentsInChildren<BuildingBehaviour>(true)).ToArray();
            StringBuilder report = new StringBuilder("# Inventário de interfaces — Centro\n\nLevantamento da cena salva; inclui objetos inativos. Nenhuma alteração foi salva na cena.\n\n");
            report.AppendLine("| Canvas | Caminho | Ordem | TMP / legado | Botões |\n|---|---|---:|---:|---:|");
            foreach (Canvas canvas in roots.SelectMany(root => root.GetComponentsInChildren<Canvas>(true)).OrderBy(x => x.sortingOrder))
                report.AppendLine($"| {canvas.name} | `{PathOf(canvas.transform)}` | {canvas.sortingOrder} | {canvas.GetComponentsInChildren<TMP_Text>(true).Length} / {canvas.GetComponentsInChildren<Text>(true).Length} | {canvas.GetComponentsInChildren<Button>(true).Length} |");
            report.AppendLine("\n## Edifícios que compartilham o modal\n\n| Edifício | Limite | Custo 2 → 3 | Definição |\n|---|---:|---:|---|");
            foreach (BuildingBehaviour building in buildings)
            {
                var d = building.Definition;
                report.AppendLine($"| {d.displayName} | {d.maximumLevel} | {BuildingCalculator.GetUpgradeCost(d, 2)} ouro | `{AssetDatabase.GetAssetPath(d)}` |");
            }
            report.AppendLine("\n## Textos e controles do modal atual\n");
            BuildingPanel panel = roots.SelectMany(root => root.GetComponentsInChildren<BuildingPanel>(true)).Single();
            report.AppendLine($"- Escala salva do modal: `{panel.transform.localScale}`; a abertura em runtime restaura escala 1.");
            foreach (TMP_Text label in panel.GetComponentsInChildren<TMP_Text>(true))
                report.AppendLine($"- `{label.name}`: fonte `{label.font?.name}`, tamanho {label.fontSize:0.#}, texto `{label.text.Replace("\n", " / ")}`.");
            foreach (Button button in panel.GetComponentsInChildren<Button>(true))
                report.AppendLine($"- Botão `{button.name}`: `{button.GetComponentInChildren<TMP_Text>(true)?.text.Trim()}`.");
            foreach (AlertPopup alert in roots.SelectMany(root => root.GetComponentsInChildren<AlertPopup>(true)))
            {
                SerializedObject data = new SerializedObject(alert);
                report.AppendLine($"\n- Feedback `{PathOf(alert.transform)}`: duração {data.FindProperty("visibleDurationSeconds").floatValue:0.#} segundo(s), tema específico {(data.FindProperty("themedFeedbackPrefab").objectReferenceValue == null ? "ausente" : "presente")}.");
            }
            report.AppendLine("\n## Componentes de interação relevantes\n");
            foreach (MonoBehaviour component in roots.SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true)).Where(x => x != null &&
                (x is BuildingPanel || x is CityManager || x is TaxCollectorNpc || x is TCCGame.Gameplay.Navigation.CityTravelNpc || x is TCCGame.Gameplay.Navigation.TavernScenePortal)))
                report.AppendLine($"- `{component.GetType().Name}` em `{PathOf(component.transform)}`.");
            Directory.CreateDirectory("Design/HUD");
            File.WriteAllText("Design/HUD/CentroHudInventory.md", report.ToString());

            theme = AssetDatabase.LoadAssetAtPath<MedievalHudTheme>(MedievalHudKit.ThemeAssetPath);
            GameObject root = new GameObject("Centro HUD Proposal Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            BuildCity(root.transform);
            BuildBuilding(root.transform, buildings.First(x => x.Definition.type == BuildingType.Barracks).Definition);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log("Centro inventory and isolated HUD proposal generated. Production scene untouched.");
        }

        private static void BuildCity(Transform root)
        {
            GameObject city = Group(root, "City Proposal");
            GameObject treasury = Panel(city.transform, "Treasury Widget", new Vector2(-670, 420), new Vector2(490, 160), theme.WoodPanel);
            Panel(treasury.transform, "Treasury Header", new Vector2(0, 51), new Vector2(470, 48), theme.MetalPanel);
            Text(treasury.transform, "Treasury Title", "IMPOSTOS DA CIDADE", new Vector2(18, 51), new Vector2(380, 36), 25, theme.Title);
            MedievalHudKit.AddIcon(treasury.transform, "Treasury Coin", theme.CoinIcon, new Vector2(-187, -5), new Vector2(44, 44));
            Text(treasury.transform, "Treasury Amount", "120 ouro disponíveis", new Vector2(20, -4), new Vector2(365, 42), 29, theme.GoldText);
            Text(treasury.transform, "Treasury Rate", "Mercado nível 3 • 20 ouro/min", new Vector2(0, -52), new Vector2(435, 32), 21, theme.Subtitle);
            GameObject gold = Panel(city.transform, "City Gold", new Vector2(735, 463), new Vector2(350, 78), theme.MetalPanel);
            MedievalHudKit.AddIcon(gold.transform, "Balance Coin", theme.CoinIcon, new Vector2(-120, 0), new Vector2(48, 48));
            Text(gold.transform, "Balance", "Ouro: 1 000", new Vector2(35, 0), new Vector2(235, 54), 30, theme.GoldText);
            GameObject prompt = Panel(city.transform, "City Prompt", new Vector2(0, -465), new Vector2(780, 78), theme.MetalPanel);
            Text(prompt.transform, "Prompt", "[E] COLETAR IMPOSTOS — 120 OURO", Vector2.zero, new Vector2(710, 58), 25, Color.white);
            GameObject toast = Panel(city.transform, "Tax Toast", new Vector2(-590, 275), new Vector2(650, 100), theme.MetalPanel);
            MedievalHudKit.AddIcon(toast.transform, "Toast Check", theme.CheckIcon, new Vector2(-272, 0), new Vector2(46, 46));
            Text(toast.transform, "Toast Message", "+120 ouro coletado\nSaldo atualizado: 1 120 ouro", new Vector2(34, 0), new Vector2(530, 78), 23, theme.Success);
            toast.SetActive(false);
        }

        private static void BuildBuilding(Transform root, BuildingDefinition definition)
        {
            GameObject modal = Group(root, "Building Proposal");
            MedievalHudKit.AddPanel(modal.transform, "Overlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, theme.ModalOverlay);
            GameObject board = Panel(modal.transform, "Building Board", Vector2.zero, new Vector2(1260, 840), theme.WoodPanel);
            Frame(board.transform, 1260, 840);
            Panel(board.transform, "Building Header", new Vector2(-25, 343), new Vector2(1080, 72), theme.MetalPanel);
            Text(board.transform, "Building Title", definition.displayName.ToUpperInvariant(), new Vector2(-25, 343), new Vector2(940, 52), 34, theme.Title);
            MedievalHudKit.AddButton(theme, board.transform, "Close Building", new Vector2(574, 343), new Vector2(52, 52), "X", Color.white, 24, theme.RedButton);
            Panel(board.transform, "Building Subtitle Plate", new Vector2(0, 272), new Vector2(1120, 42), theme.MetalPanel);
            Text(board.transform, "Building Subtitle", "Mais recrutas no próximo reestoque do Portão.", new Vector2(0, 272), new Vector2(1060, 30), 21, theme.Subtitle);
            GameObject picture = Panel(board.transform, "Building Illustration Panel", new Vector2(-380, 40), new Vector2(370, 370), theme.ParchmentPanel);
            MedievalHudKit.AddIcon(picture.transform, "Building Illustration", definition.levelSprites.FirstOrDefault(), new Vector2(0, 28), new Vector2(310, 250));
            Text(picture.transform, "Building Level", "NÍVEL 2 → 3", new Vector2(0, -140), new Vector2(320, 40), 28, theme.ParchmentText);
            GameObject paper = Panel(board.transform, "Building Comparison", new Vector2(205, 25), new Vector2(700, 400), theme.ParchmentPanel);
            Text(paper.transform, "Comparison Heading", "BENEFÍCIO DA MELHORIA", new Vector2(0, 158), new Vector2(630, 42), 27, theme.PriceText);
            Text(paper.transform, "Current Caption", "ATUAL", new Vector2(-160, 97), new Vector2(280, 32), 22, theme.PriceText);
            Text(paper.transform, "Next Caption", "PRÓXIMO NÍVEL", new Vector2(160, 97), new Vector2(280, 32), 22, theme.PriceText);
            Text(paper.transform, "Current Benefit", "16 guerreiros\npor reestoque", new Vector2(-160, 30), new Vector2(280, 90), 28, theme.ParchmentText);
            Text(paper.transform, "Next Benefit", "19 guerreiros\npor reestoque", new Vector2(160, 30), new Vector2(280, 90), 28, theme.ParchmentText);
            MedievalHudKit.AddIcon(paper.transform, "Cost Coin", theme.CoinIcon, new Vector2(-275, -112), new Vector2(46, 46));
            Text(paper.transform, "Upgrade Cost", "Custo: 225 ouro", new Vector2(30, -90), new Vector2(525, 40), 27, theme.PriceText);
            Text(paper.transform, "Cost Balance", "Seu saldo: 1 000 • Após: 775 ouro", new Vector2(30, -136), new Vector2(525, 36), 22, theme.ParchmentText);
            Panel(board.transform, "Building Feedback Plate", new Vector2(0, -229), new Vector2(1120, 52), theme.MetalPanel);
            Text(board.transform, "Building Feedback", "O novo estoque estará disponível no próximo reestoque (6h).", new Vector2(0, -229), new Vector2(1060, 38), 20, theme.Subtitle);
            MedievalHudKit.AddButton(theme, board.transform, "Back To City", new Vector2(-330, -325), new Vector2(330, 66), "VOLTAR À CIDADE", Color.white, 24, theme.BronzeButton);
            MedievalHudKit.AddButton(theme, board.transform, "Upgrade Building", new Vector2(250, -325), new Vector2(480, 66), "MELHORAR PARA NÍVEL 3", Color.white, 26, theme.GoldButton);
            Text(board.transform, "Building Help", "Esc fecha o painel • Custo descontado apenas ao confirmar", new Vector2(0, -382), new Vector2(1060, 28), 18, theme.Subtitle);
            modal.SetActive(false);
        }

        private static GameObject Group(Transform parent, string name)
        {
            GameObject group = new GameObject(name, typeof(RectTransform));
            group.transform.SetParent(parent, false);
            RectTransform rect = group.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return group;
        }
        private static GameObject Panel(Transform parent, string name, Vector2 position, Vector2 size, Sprite sprite)
        {
            var panel = MedievalHudKit.AddFixedPanel(parent, name, position, size, Color.white);
            MedievalHudKit.ApplyStyle(panel, sprite, Color.white);
            return panel;
        }
        private static TMP_Text Text(Transform parent, string name, string value, Vector2 position, Vector2 size, float fontSize, Color color)
        {
            var text = MedievalHudKit.AddText(theme, parent, name, value, position, size, fontSize, TextAlignmentOptions.Center, color);
            text.enableAutoSizing = true; text.fontSizeMin = Mathf.Min(18, fontSize); text.fontSizeMax = fontSize;
            return text;
        }
        private static void Frame(Transform parent, float width, float height)
        {
            MedievalHudKit.AddFramePiece(parent, "Frame Top", new Vector2(0, height / 2 - 14), new Vector2(width, 28), theme.MetalPanel);
            MedievalHudKit.AddFramePiece(parent, "Frame Bottom", new Vector2(0, -height / 2 + 14), new Vector2(width, 28), theme.MetalPanel);
            MedievalHudKit.AddFramePiece(parent, "Frame Left", new Vector2(-width / 2 + 14, 0), new Vector2(28, height), theme.MetalPanel);
            MedievalHudKit.AddFramePiece(parent, "Frame Right", new Vector2(width / 2 - 14, 0), new Vector2(28, height), theme.MetalPanel);
        }
        private static string PathOf(Transform target) => target.parent == null ? target.name : PathOf(target.parent) + "/" + target.name;
    }
}
#endif
