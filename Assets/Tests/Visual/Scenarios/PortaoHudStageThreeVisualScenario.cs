using System.Collections.Generic;
using System.Linq;
using TMPro;
using TCCGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Tests.Visual
{
    public sealed class PortaoHudStageThreeVisualScenario : IVisualReviewScenario
    {
        public string Id => "portao-hud-stage3";
        public string DisplayName => "Portão — viagem e guarnição integradas";
        public string ScenePath => "Assets/Scenes/Portão.unity";
        public string ReferenceImagePath => "Assets/Tests/Visual/Baselines/portao-hud/garrison-modal-1920x1080.png";
        public IReadOnlyList<VisualReviewCase> Cases { get; } = new[]
        {
            new VisualReviewCase("travel-center", "Retorno ao Centro", 1920, 1080),
            new VisualReviewCase("travel-gate", "Viagem ao portão", 1920, 1080),
            new VisualReviewCase("travel-compact", "Viagem compacta", 1366, 768),
            new VisualReviewCase("garrison", "Gestão da guarnição", 1920, 1080),
            new VisualReviewCase("garrison-empty", "Sem tropas e relatório", 1366, 768),
            new VisualReviewCase("garrison-long", "Relatório e trânsito extensos", 1366, 768)
        };

        public VisualReviewContext PrepareScene()
        {
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                canvas.enabled = false;
            GameObject root = new GameObject("Modal Production Review", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            foreach (string name in new[] { "TravelConversationCanvas", "GarrisonManagementCanvas" })
                foreach (Transform child in Resources.Load<GameObject>("UI/" + name).transform)
                {
                    GameObject copy = Object.Instantiate(child.gameObject, root.transform, false);
                    copy.name = child.name;
                }
            return new VisualReviewContext(Object.FindFirstObjectByType<Camera>(), root.GetComponent<Canvas>(), root);
        }

        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase)
        {
            bool travel = reviewCase.Id.StartsWith("travel");
            Transform travelRoot = context.TargetRoot.transform.Find("Travel Dialog");
            Transform garrisonRoot = context.TargetRoot.transform.Find("Garrison Dialog");
            travelRoot.gameObject.SetActive(travel);
            garrisonRoot.gameObject.SetActive(!travel);
            if (travel)
            {
                bool center = reviewCase.Id == "travel-center";
                travelRoot.GetComponent<TravelHudView>().Present(center ? "CENTRO DA CIDADE" : "PORTÃO NORTE",
                    !center, 96, 100, "ANUNCIADO", "01:25", "Ameaça 42");
                Text(travelRoot, "Title").text = "CONDUTOR DA CIDADE";
                Text(travelRoot, "Body").text = center
                    ? "Na cidade você pode redistribuir a guarda e consultar os demais portões."
                    : "Confirme a viagem para este portão.";
                Button(travelRoot, "Confirm").GetComponentInChildren<TMP_Text>().text = center ? "Voltar ao Centro" : "Ir ao Portão Norte";
            }
            else
            {
                bool empty = reviewCase.Id == "garrison-empty";
                bool extensive = reviewCase.Id == "garrison-long";
                GarrisonHudView view = garrisonRoot.GetComponent<GarrisonHudView>();
                view.SetStrategicState(96, 100, "Ameaça 42", "01:25");
                for (int i = 0; i < 3; i++) view.SetUnit(i, empty ? 0 : 18 - i * 6, empty ? 0 : 50 - i * 5, true);
                Text(garrisonRoot, "Status").text = empty ? "Nenhuma tropa disponível na origem." : "10 guerreiros em marcha.";
                Text(garrisonRoot, "Destination").text = "Destino dos envios: Portão Sul";
                Text(garrisonRoot, "Battle Report").text = empty ? "Nenhum relatório de batalha neste portão."
                    : "ONDA 3 — ONDA REPELIDA\nDefesa 48 / Ameaça 42\nG 20→18 (-2) • A 12→11 (-1) • M 6→6 (-0)\nPortão 100→96 (-4)\nEspólio +8 ouro" +
                    (extensive ? "\nVantagem da linha de frente: guerreiros resistem aos invasores.\nOs arqueiros mantiveram a defesa a distância.\nConsulte o resultado antes de redistribuir os sobreviventes." : "");
                Text(garrisonRoot, "Summary").text = empty ? "EM TRÂNSITO  nenhum"
                    : "EM TRÂNSITO  10 G Norte → Sul (18,7s)" +
                    (extensive ? "\n10 A Reserva → Norte (12,3s)\n5 M Norte → Leste (24,1s)\n10 G Reserva → Norte (27,8s)" : "");
                Button(garrisonRoot, "Acknowledge Report").gameObject.SetActive(!empty);
                foreach (ScrollRect scroll in garrisonRoot.GetComponentsInChildren<ScrollRect>(true)) scroll.verticalNormalizedPosition = 1;
            }
            Canvas.ForceUpdateCanvases();
        }

        private static TMP_Text Text(Transform root, string name) => root.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == name);
        private static Button Button(Transform root, string name) => root.GetComponentsInChildren<Button>(true).Single(x => x.name == name);
    }
}
