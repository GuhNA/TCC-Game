#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TCCGame.Editor
{
    public static class PortaoHudInventory
    {
        private const string ScenePath = "Assets/Scenes/Portão.unity";
        private const string ReportPath = "Design/HUD/PortaoHudInventory.md";

        private static readonly string[] RuntimeUiPrefabs =
        {
            "Assets/Resources/UI/RecruitmentCanvas.prefab",
            "Assets/Prefabs/UI/GoldBalanceHUD.prefab",
            "Assets/Resources/UI/GateContextCanvas.prefab",
            "Assets/Resources/UI/InteractionPrompt.prefab",
            "Assets/Resources/UI/WaveAlertCanvas.prefab",
            "Assets/Resources/UI/WaveAlertIndicator.prefab",
            "Assets/Resources/UI/TravelConversationCanvas.prefab",
            "Assets/Resources/UI/GarrisonManagementCanvas.prefab",
            "Assets/Resources/UI/SceneFadeCanvas.prefab"
        };

        [MenuItem("TCC Game/UI/Generate Portao HUD Inventory")]
        public static void Generate()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            StringBuilder report = new StringBuilder();
            report.AppendLine("# Inventário de HUDs — mapa Portão");
            report.AppendLine();
            report.AppendLine("Gerado automaticamente a partir da cena e dos prefabs de runtime.");
            report.AppendLine();
            report.AppendLine("## Canvases presentes na cena");
            report.AppendLine();
            report.AppendLine("| Canvas | Caminho | Ativo | Ordem | Textos | Imagens | Botões |");
            report.AppendLine("|---|---|---:|---:|---:|---:|---:|");

            Canvas[] canvases = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Canvas>(true))
                .OrderBy(canvas => canvas.sortingOrder)
                .ThenBy(canvas => HierarchyPath(canvas.transform))
                .ToArray();
            foreach (Canvas canvas in canvases)
            {
                GameObject root = canvas.gameObject;
                report.AppendLine($"| {root.name} | `{HierarchyPath(root.transform)}` | " +
                    $"{root.activeInHierarchy} | {canvas.sortingOrder} | " +
                    $"{CountTexts(root)} | " +
                    $"{root.GetComponentsInChildren<Image>(true).Length} | " +
                    $"{root.GetComponentsInChildren<Button>(true).Length} |");
            }

            report.AppendLine();
            report.AppendLine("## Prefabs relacionados ao Portão");
            report.AppendLine();
            report.AppendLine("| Interface | Asset | Canvas | Textos | Imagens | Botões |");
            report.AppendLine("|---|---|---:|---:|---:|---:|");
            foreach (string path in RuntimeUiPrefabs)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    report.AppendLine($"| Ausente | `{path}` | — | — | — | — |");
                    continue;
                }

                report.AppendLine($"| {prefab.name} | `{path}` | " +
                    $"{prefab.GetComponentsInChildren<Canvas>(true).Length} | " +
                    $"{CountTexts(prefab)} | " +
                    $"{prefab.GetComponentsInChildren<Image>(true).Length} | " +
                    $"{prefab.GetComponentsInChildren<Button>(true).Length} |");
            }

            report.AppendLine();
            report.AppendLine("## Critério de prototipação");
            report.AppendLine();
            report.AppendLine("O protótipo deve representar HUD persistente, avisos, prompts e modais " +
                "sem alterar a lógica de jogo. Recrutamento permanece como baseline visual.");

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, report.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"Portao HUD inventory generated: {ReportPath}");
        }

        private static int CountTexts(GameObject root) =>
            root.GetComponentsInChildren<TMP_Text>(true).Length +
            root.GetComponentsInChildren<Text>(true).Length;

        private static string HierarchyPath(Transform target)
        {
            string path = target.name;
            while (target.parent != null)
            {
                target = target.parent;
                path = target.name + "/" + path;
            }
            return path;
        }
    }
}
#endif
