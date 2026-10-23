using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TCCGame.Tests.Visual
{
    public sealed class PortaoHudPrototypeVisualScenario : IVisualReviewScenario
    {
        private static readonly IReadOnlyList<VisualReviewCase> ReviewCases =
            new List<VisualReviewCase>
            {
                new("gameplay", "HUD durante o jogo", 1920, 1080),
                new("gameplay-compact", "HUD compacto", 1366, 768),
                new("travel-modal", "Modal de viagem", 1920, 1080),
                new("garrison-modal", "Gestão da guarnição", 1920, 1080),
                new("alert-modal", "Alerta do portão", 1920, 1080)
            };

        public string Id => "portao-hud";
        public string DisplayName => "Conjunto de HUDs do mapa Portão";
        public string ScenePath => "Assets/Scenes/Portão.unity";
        public string ReferenceImagePath =>
            "Design/HUD/RecruitmentHudVisualDirection.png";
        public IReadOnlyList<VisualReviewCase> Cases => ReviewCases;

        public VisualReviewContext PrepareScene()
        {
            GameObject prefab = Resources.Load<GameObject>(
                "UI/Prototypes/PortaoHudPrototypeCanvas");
            Camera camera = UnityEngine.Object.FindFirstObjectByType<Camera>(
                FindObjectsInactive.Include);
            if (prefab == null || camera == null)
                throw new InvalidOperationException(
                    "Portao HUD prototype prefab or scene camera was not found.");

            Canvas[] existing = UnityEngine.Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas canvas in existing) canvas.enabled = false;

            GameObject root = UnityEngine.Object.Instantiate(prefab);
            root.name = prefab.name;
            Canvas target = root.GetComponent<Canvas>();
            camera.gameObject.SetActive(true);
            camera.enabled = true;
            return new VisualReviewContext(camera, target, root);
        }

        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase)
        {
            SetActive(context.TargetRoot, "Gameplay HUD",
                reviewCase.Id.StartsWith("gameplay", StringComparison.Ordinal));
            SetActive(context.TargetRoot, "Travel Modal", reviewCase.Id == "travel-modal");
            SetActive(context.TargetRoot, "Garrison Modal", reviewCase.Id == "garrison-modal");
            SetActive(context.TargetRoot, "Alert Modal", reviewCase.Id == "alert-modal");
            context.TargetRoot.SetActive(true);
            Canvas.ForceUpdateCanvases();
        }

        private static void SetActive(GameObject root, string name, bool active)
        {
            Transform target = root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(item => item.name == name);
            if (target == null)
                throw new InvalidOperationException($"Prototype group not found: {name}");
            target.gameObject.SetActive(active);
        }
    }

}
