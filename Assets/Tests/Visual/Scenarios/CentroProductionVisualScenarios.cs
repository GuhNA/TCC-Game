using System.Collections.Generic;
using System.Linq;
using TCCGame.Gameplay.Exploration;
using TCCGame.Prototypes;
using TCCGame.UI;
using UnityEngine;

namespace TCCGame.Tests.Visual
{
    public sealed class CentroProductionVisualScenario : IVisualReviewScenario
    {
        private readonly CentroTaxVisualScenario hud = new CentroTaxVisualScenario();
        public string Id => "centro-production";
        public string DisplayName => "Centro — layout integrado";
        public string ScenePath => "Assets/Scenes/Centro.unity";
        public string ReferenceImagePath => "Artifacts/VisualTests/centro-static-background/captures/overview-1920x1440.png";
        public IReadOnlyList<VisualReviewCase> Cases { get; } = new[]
        {
            new VisualReviewCase("overview", "Composição final do Centro", 1920, 1440),
            new VisualReviewCase("tavern-management", "Taverna e placa no Centro final", 1366, 768)
        };

        public VisualReviewContext PrepareScene()
        {
            var context = hud.PrepareScene();
            foreach (var canvas in GameObject.Find("Centro Layout").GetComponentsInChildren<Canvas>())
                if (canvas.renderMode == RenderMode.WorldSpace) canvas.enabled = true;
            return context;
        }

        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase)
        {
            foreach (Transform child in context.TargetRoot.transform) child.gameObject.SetActive(true);
            hud.ConfigureCase(context, new VisualReviewCase("city-hud", reviewCase.DisplayName, reviewCase.Width, reviewCase.Height));
            bool overview = reviewCase.Id == "overview";
            var tavern = Object.FindObjectsByType<BuildingBehaviour>(FindObjectsSortMode.None)
                .Single(x => x.Definition.type == BuildingType.Tavern);
            Vector2 position = tavern.GetComponent<BoxCollider2D>().bounds.center;
            ProductionWorldCapture.PlaceCamera(context.Camera,
                overview ? new Vector2(0, 3) : position + new Vector2(-2.4f, 1.6f),
                overview ? 20.5f : 4.99f);
            if (!overview)
                Object.FindFirstObjectByType<PlayerInteractor>().transform.position = position + new Vector2(.55f, .30f);
            foreach (var view in Object.FindObjectsByType<WorldBuildingView>(FindObjectsSortMode.None))
                view.Refresh();
            foreach (var actor in Object.FindObjectsByType<WorldPrototypeDepthSort>(FindObjectsSortMode.None))
                actor.Refresh();
            context.TargetRoot.GetComponentInChildren<InteractionPromptView>(true).SetDescription(tavern.InteractionPrompt);
            if (overview)
                foreach (Transform child in context.TargetRoot.transform) child.gameObject.SetActive(false);
            else
                context.TargetRoot.GetComponentInChildren<CityTaxHudView>(true).SetVisibility(false, false);
        }
    }

    public sealed class TavernProductionVisualScenario : IVisualReviewScenario
    {
        public string Id => "centro-tavern-production";
        public string DisplayName => "Taverna — interior final";
        public string ScenePath => "Assets/Scenes/Taverna.unity";
        public string ReferenceImagePath => "Artifacts/VisualTests/centro-tavern-collisions/captures/floor-1600x1200.png";
        public IReadOnlyList<VisualReviewCase> Cases { get; } = new[]
        {
            new VisualReviewCase("interior", "Interior, quadro de exploração e saída", 1600, 1200)
        };

        public VisualReviewContext PrepareScene()
        {
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                canvas.enabled = false;
            var root = new GameObject("Tavern production review", typeof(Canvas));
            return new VisualReviewContext(Camera.main, root.GetComponent<Canvas>(), root);
        }

        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase)
        {
            var floor = GameObject.Find("taverna_0").GetComponent<SpriteRenderer>();
            var board = Object.FindFirstObjectByType<ExplorationMapController>();
            var player = Object.FindFirstObjectByType<PlayerInteractor>();
            player.transform.position = board.GetComponent<BoxCollider2D>().bounds.center + Vector3.down * .5f;
            ProductionWorldCapture.PlaceCamera(context.Camera, floor.bounds.center, 6.7f);
        }
    }

    internal static class ProductionWorldCapture
    {
        internal static void PlaceCamera(Camera camera, Vector2 center, float zoom)
        {
            foreach (var script in camera.GetComponents<MonoBehaviour>())
                if (script != null && script.GetType().Namespace == "Unity.Cinemachine") script.enabled = false;
            camera.transform.position = new Vector3(center.x, center.y, camera.transform.position.z);
            camera.orthographicSize = zoom;
        }
    }
}
