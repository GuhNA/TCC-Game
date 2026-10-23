using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TCCGame.Gameplay.Campaign;
using TCCGame.UI;
using UnityEditor;
using UnityEngine;

namespace TCCGame.Tests.Visual
{
    public sealed class PortaoHudStageTwoVisualScenario : IVisualReviewScenario
    {
        public string Id => "portao-hud-stage2";
        public string DisplayName => "Portão — alertas integrados";
        public string ScenePath => "Assets/Scenes/Portão.unity";
        public string ReferenceImagePath =>
            "Assets/Tests/Visual/Baselines/portao-hud/gameplay-1920x1080.png";
        public IReadOnlyList<VisualReviewCase> Cases { get; } = new[]
        {
            new VisualReviewCase("announced", "Aviso de onda", 1920, 1080),
            new VisualReviewCase("critical", "Aviso crítico compacto", 1366, 768),
            new VisualReviewCase("feedback", "Feedback do portão", 1920, 1080),
            new VisualReviewCase("feedback-compact", "Feedback compacto", 1366, 768)
        };

        public VisualReviewContext PrepareScene()
        {
            VisualReviewContext context = new PortaoHudStageOneVisualScenario().PrepareScene();
            CopyChildren("Assets/Resources/UI/WaveAlertCanvas.prefab", context.TargetRoot.transform);
            CopyChildren("Assets/Resources/UI/PortaoAlertFeedbackCanvas.prefab", context.TargetRoot.transform);

            // Use the production world-space prefab at the same sprite-relative anchor as runtime.
            var captain = UnityEngine.Object.FindFirstObjectByType<CaptainNpc>();
            var markerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/WaveAlertIndicator.prefab");
            var marker = UnityEngine.Object.Instantiate(markerPrefab);
            marker.name = "Review Captain Marker";
            marker.transform.localScale = Vector3.one * .01f;
            SpriteRenderer sprite = captain.GetComponentInChildren<SpriteRenderer>();
            marker.transform.position = new Vector3(sprite.bounds.center.x,
                sprite.bounds.max.y + .25f, captain.transform.position.z);
            return context;
        }

        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase)
        {
            new PortaoHudStageOneVisualScenario().ConfigureCase(context,
                new VisualReviewCase("normal", "Normal", reviewCase.Width, reviewCase.Height));
            bool feedback = reviewCase.Id.StartsWith("feedback", StringComparison.Ordinal);
            bool critical = reviewCase.Id == "critical";
            WaveAlertView view = context.TargetRoot.GetComponentInChildren<WaveAlertView>(true);
            view.gameObject.SetActive(!feedback);
            view.GetComponent<CanvasGroup>().alpha = 1f;
            view.Present(new WaveAlertMessage(critical ? WaveWarningStage.Critical : WaveWarningStage.Announced,
                default, critical
                    ? "O ataque ao Portão Norte é iminente. Reforce a guarnição imediatamente!"
                    : "Batedores: inimigos avançam para o Portão Norte."), new Color(.8f, .3f, .12f));
            Transform alert = context.TargetRoot.transform.Find("Alert Modal");
            alert.gameObject.SetActive(feedback);
            if (feedback)
                alert.GetComponentsInChildren<TMP_Text>(true).Single(text => text.name == "Feedback Message")
                    .text = "A reserva não possui soldados suficientes para essa formação.";
            GameObject marker = GameObject.Find("Review Captain Marker");
            if (marker != null) marker.GetComponent<Canvas>().enabled = !feedback;
        }

        private static void CopyChildren(string path, Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new InvalidOperationException(path);
            foreach (Transform child in prefab.transform)
            {
                GameObject copy = UnityEngine.Object.Instantiate(child.gameObject, parent, false);
                copy.name = child.name;
            }
        }
    }
}
