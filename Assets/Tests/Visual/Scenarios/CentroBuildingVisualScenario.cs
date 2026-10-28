using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace TCCGame.Tests.Visual
{
    public sealed class CentroBuildingVisualScenario : IVisualReviewScenario
    {
        public string Id => "centro-buildings";
        public string DisplayName => "Centro — edifícios integrados";
        public string ScenePath => "Assets/Scenes/Centro.unity";
        public string ReferenceImagePath => "Assets/Tests/Visual/Baselines/centro-proposals/building-1920x1080.png";
        public IReadOnlyList<VisualReviewCase> Cases { get; } = new CentroHudProposalVisualScenario().Cases
            .Where(x => x.Id != "city-hud" && x.Id != "tax-feedback").ToArray();

        public VisualReviewContext PrepareScene()
        {
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)) canvas.enabled = false;
            BuildingPanel panel = Object.FindFirstObjectByType<BuildingPanel>(FindObjectsInactive.Include);
            Canvas target = panel.GetComponentInParent<Canvas>();
            panel.gameObject.SetActive(true);
            return new VisualReviewContext(Object.FindFirstObjectByType<Camera>(), target, target.gameObject);
        }

        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase)
        {
            string id = reviewCase.Id;
            string asset = id == "market" ? "LocalMarket" : id == "tavern" ? "Tavern"
                : id == "archers" ? "ArcherStand" : id == "mages" ? "MagicSchool" : "Barracks";
            var definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>($"Assets/Scriptable/Buildings/{asset}.asset");
            int level = id == "maximum" ? definition.maximumLevel : id == "unbuilt" ? 0 : 2;
            int bank = id == "insufficient" ? 100 : 1000;
            context.TargetRoot.GetComponentInChildren<BuildingPanel>(true).Present(definition, level, bank,
                level < definition.maximumLevel && bank >= BuildingCalculator.GetUpgradeCost(definition, level));
            Canvas.ForceUpdateCanvases();
        }
    }
}
