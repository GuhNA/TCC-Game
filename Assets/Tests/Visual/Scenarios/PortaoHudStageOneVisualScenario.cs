using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TCCGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Tests.Visual
{
    /// <summary>Captures the shipped prefabs, rather than the presentation prototype.</summary>
    public sealed class PortaoHudStageOneVisualScenario : IVisualReviewScenario
    {
        public string Id => "portao-hud-stage1";
        public string DisplayName => "Portão — HUD base integrado";
        public string ScenePath => "Assets/Scenes/Portão.unity";
        public string ReferenceImagePath =>
            "Artifacts/VisualTests/portao-hud/captures/gameplay-1920x1080.png";
        public IReadOnlyList<VisualReviewCase> Cases { get; } = new[]
        {
            new VisualReviewCase("normal", "HUD base", 1920, 1080),
            new VisualReviewCase("compact", "HUD base compacto", 1366, 768),
            new VisualReviewCase("critical", "Integridade crítica e saldo alto", 1366, 768)
        };

        public VisualReviewContext PrepareScene()
        {
            foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)) canvas.enabled = false;
            GameObject root = new GameObject("Production HUD Capture", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler));
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            foreach (string path in new[]
            {
                "Assets/Resources/UI/GateContextCanvas.prefab",
                "Assets/Prefabs/UI/GoldBalanceHUD.prefab",
                "Assets/Resources/UI/InteractionPrompt.prefab"
            })
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) throw new InvalidOperationException(path);
                foreach (Transform child in prefab.transform)
                {
                    GameObject copy = UnityEngine.Object.Instantiate(child.gameObject, root.transform, false);
                    copy.name = child.name;
                }
            }
            return new VisualReviewContext(UnityEngine.Object.FindFirstObjectByType<Camera>(),
                root.GetComponent<Canvas>(), root);
        }

        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase)
        {
            bool critical = reviewCase.Id == "critical";
            context.TargetRoot.GetComponentInChildren<GateHudView>().Refresh(
                "PORTÃO NORTE", critical ? 22 : 100, 100,
                critical ? "ATAQUE IMINENTE" : "ANUNCIADO", "01:25", "Ameaça 42");
            context.TargetRoot.GetComponentInChildren<InteractionPromptView>().SetDescription(
                critical ? "Inspecionar o relatório do capitão" : "Falar com o capitão do portão");
            TMP_Text gold = context.TargetRoot.transform.Find("Gold Balance")
                .GetComponentInChildren<TMP_Text>();
            gold.text = critical ? "2 147 483 647" : "145 319";
            Canvas.ForceUpdateCanvases();
        }
    }
}
