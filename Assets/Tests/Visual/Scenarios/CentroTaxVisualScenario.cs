using System.Collections.Generic;
using System.Linq;
using TCCGame.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TCCGame.Tests.Visual
{
    public sealed class CentroTaxVisualScenario : IVisualReviewScenario
    {
        private TMP_Text goldLabel;
        public string Id => "centro-taxes";
        public string DisplayName => "Centro — impostos e interações integrados";
        public string ScenePath => "Assets/Scenes/Centro.unity";
        public string ReferenceImagePath => "Assets/Tests/Visual/Baselines/centro-proposals/tax-feedback-1366x768.png";
        public IReadOnlyList<VisualReviewCase> Cases { get; } = new[]
        {
            new VisualReviewCase("tax-feedback", "Coleta concluída", 1366, 768),
            new VisualReviewCase("city-hud", "Prévia contextual", 1920, 1080),
            new VisualReviewCase("tax-empty", "Sem impostos disponíveis", 1366, 768),
            new VisualReviewCase("tax-market-missing", "Mercado não construído", 1366, 768)
        };

        public VisualReviewContext PrepareScene()
        {
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)) canvas.enabled = false;
            // Capture actual production pieces together, without running services or touching a save.
            var tax = Object.FindFirstObjectByType<CityTaxHud>(FindObjectsInactive.Include);
            var root = tax.gameObject;
            var gold = Object.FindFirstObjectByType<GoldBalancePresenter>(FindObjectsInactive.Include);
            var goldCopy = Object.Instantiate(gold.PixelGoldText.transform.parent.gameObject, root.transform, false);
            goldCopy.SetActive(true);
            goldLabel = goldCopy.GetComponentInChildren<TMP_Text>(true);
            var prompt = Object.Instantiate(Resources.Load<GameObject>("UI/InteractionPrompt"));
            foreach (Transform child in prompt.transform.Cast<Transform>().ToArray()) child.SetParent(root.transform, false);
            Object.DestroyImmediate(prompt);
            return new VisualReviewContext(Object.FindFirstObjectByType<Camera>(), root.GetComponent<Canvas>(), root);
        }

        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase)
        {
            string id = reviewCase.Id;
            var view = context.TargetRoot.GetComponentInChildren<CityTaxHudView>(true);
            bool success = id == "tax-feedback";
            bool missing = id == "tax-market-missing";
            bool preview = id == "city-hud";
            view.Present(missing ? 0 : 3, preview ? 120 : 0, missing ? 0 : 20, success);
            view.PresentResult(new TaxCollectionResult(success ? TaxCollectionStatus.Collected : missing
                ? TaxCollectionStatus.MarketNotBuilt : TaxCollectionStatus.NothingAvailable, success ? 120 : 0, success ? 1120 : 1000));
            view.SetVisibility(true, !preview);
            goldLabel.text = success ? "1 120" : "1 000";
            context.TargetRoot.GetComponentInChildren<InteractionPromptView>(true).SetDescription(preview
                ? "Coletar impostos — 120 ouro" : "Consultar impostos");
            Canvas.ForceUpdateCanvases();
        }
    }
}
