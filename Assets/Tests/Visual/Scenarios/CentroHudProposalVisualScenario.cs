using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using TMPro;
using TCCGame.Gameplay.Recruitment;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Tests.Visual
{
    public sealed class CentroHudProposalVisualScenario : IVisualReviewScenario
    {
        public string Id => "centro-proposals";
        public string DisplayName => "Centro — propostas de UX/UI";
        public string ScenePath => "Assets/Scenes/Centro.unity";
        public string ReferenceImagePath => "Design/HUD/CentroLegacyBuilding.png";
        public IReadOnlyList<VisualReviewCase> Cases { get; } = new[]
        {
            new VisualReviewCase("building", "Quartel — melhoria", 1920, 1080),
            new VisualReviewCase("building-compact", "Melhoria compacta", 1366, 768),
            new VisualReviewCase("unbuilt", "Construção inicial", 1920, 1080),
            new VisualReviewCase("insufficient", "Ouro insuficiente", 1366, 768),
            new VisualReviewCase("maximum", "Nível máximo", 1366, 768),
            new VisualReviewCase("market", "Mercado — impostos por minuto", 1920, 1080),
            new VisualReviewCase("tavern", "Taverna — aventureiros por onda", 1920, 1080),
            new VisualReviewCase("archers", "Estande de Arqueiros", 1920, 1080),
            new VisualReviewCase("mages", "Escola de Magia", 1920, 1080),
            new VisualReviewCase("city-hud", "Impostos e interação", 1920, 1080),
            new VisualReviewCase("tax-feedback", "Coleta — feedback compacto", 1366, 768)
        };

        public VisualReviewContext PrepareScene()
        {
            foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)) canvas.enabled = false;
            var prefab = Resources.Load<GameObject>("UI/Prototypes/CentroHudProposalCanvas");
            if (prefab == null) throw new InvalidOperationException("Centro proposal prefab is missing.");
            GameObject root = UnityEngine.Object.Instantiate(prefab);
            Camera camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            return new VisualReviewContext(camera, root.GetComponent<Canvas>(), root);
        }

        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase)
        {
            string id = reviewCase.Id;
            bool city = id == "city-hud" || id == "tax-feedback";
            Transform cityRoot = context.TargetRoot.transform.Find("City Proposal");
            Transform buildingRoot = context.TargetRoot.transform.Find("Building Proposal");
            cityRoot.gameObject.SetActive(city); buildingRoot.gameObject.SetActive(!city);
            cityRoot.Find("Tax Toast").gameObject.SetActive(id == "tax-feedback");
            Text(context, "Balance").text = id == "tax-feedback" ? "Ouro: 1 120" : "Ouro: 1 000";
            Text(context, "Treasury Amount").text = id == "tax-feedback" ? "Impostos coletados" : "120 ouro disponíveis";
            Text(context, "Prompt").text = id == "tax-feedback" ? "[E] CONSULTAR IMPOSTOS" : "[E] COLETAR IMPOSTOS — 120 OURO";
            if (city) { Canvas.ForceUpdateCanvases(); return; }

            string asset = id == "market" ? "LocalMarket" : id == "tavern" ? "Tavern" : id == "archers" ? "ArcherStand" : id == "mages" ? "MagicSchool" : "Barracks";
            BuildingDefinition d = AssetDatabase.LoadAssetAtPath<BuildingDefinition>($"Assets/Scriptable/Buildings/{asset}.asset");
            bool max = id == "maximum";
            int level = max ? d.maximumLevel : id == "unbuilt" ? 0 : 2;
            int next = Mathf.Min(level + 1, d.maximumLevel);
            int bank = id == "insufficient" ? 100 : 1000;
            int cost = BuildingCalculator.GetUpgradeCost(d, level);
            Text(context, "Building Title").text = d.displayName.ToUpperInvariant();
            Text(context, "Building Subtitle").text = d.type == BuildingType.LocalMarket
                ? "Mais impostos acumulados por minuto enquanto o Mercado estiver construído."
                : d.type == BuildingType.Tavern ? "Gerencie o edifício aqui; use a porta para entrar na Taverna."
                : "Mais recrutas disponíveis no próximo reestoque do Portão.";
            Text(context, "Building Level").text = max ? $"NÍVEL {level}/{d.maximumLevel}" : $"NÍVEL {level} → {next}";
            Text(context, "Current Caption").text = "ATUAL";
            Text(context, "Next Caption").text = max ? "CONCLUÍDO" : "PRÓXIMO NÍVEL";
            Text(context, "Current Benefit").text = Benefit(d, level);
            Text(context, "Next Benefit").text = max ? "Limite de melhoria\natingido" : Benefit(d, next);
            Text(context, "Upgrade Cost").text = max ? "Nenhum novo custo" : $"Custo: {cost} ouro";
            Text(context, "Cost Balance").text = max ? "Todos os níveis deste edifício concluídos."
                : bank < cost ? $"Seu saldo: {bank} • Faltam {cost - bank} ouro" : $"Seu saldo: {Money(bank)} • Após: {Money(bank - cost)} ouro";
            Text(context, "Building Feedback").text = max ? "Edifício no nível máximo. Seu benefício atual continua ativo."
                : bank < cost ? "Ouro insuficiente. Colete impostos ou retorne com mais recursos."
                : d.type == BuildingType.LocalMarket ? "O novo rendimento passa a valer após a melhoria."
                : d.type == BuildingType.Tavern ? "A nova capacidade será usada na próxima onda de aventureiros."
                : "A melhoria altera o próximo reestoque (6h); não concede tropas imediatamente.";
            Image illustration = context.TargetRoot.GetComponentsInChildren<Image>(true).Single(x => x.name == "Building Illustration");
            // Preview the completed building instead of the scene's placeholder marker.
            illustration.sprite = d.levelSprites[Mathf.Clamp(level - 1, 0, d.levelSprites.Length - 1)];
            if (level == 0) Text(context, "Building Level").text = "PRÉVIA • NÍVEL 0 → 1";
            Button button = context.TargetRoot.GetComponentsInChildren<Button>(true).Single(x => x.name == "Upgrade Building");
            button.gameObject.SetActive(!max); button.interactable = bank >= cost;
            TMP_Text buttonLabel = button.GetComponentInChildren<TMP_Text>(true);
            buttonLabel.text = level == 0 ? $"CONSTRUIR POR {cost} OURO" : $"MELHORAR PARA NÍVEL {next}";
            buttonLabel.color = bank < cost ? new Color(1f, .91f, .7f) : new Color(.2f, .11f, .04f);
            var back = context.TargetRoot.GetComponentsInChildren<Button>(true).Single(x => x.name == "Back To City");
            back.GetComponent<RectTransform>().anchoredPosition = new Vector2(max ? 0 : -330, -325);
            Text(context, "Building Help").text = max ? "Esc fecha o painel • Benefício mantido no nível máximo"
                : "Esc fecha o painel • Custo descontado apenas ao confirmar";
            Canvas.ForceUpdateCanvases();
        }

        private static string Benefit(BuildingDefinition d, int level)
        {
            if (d.type == BuildingType.LocalMarket) return $"{BuildingCalculator.GetProduction(d, level)} ouro\npor minuto";
            if (d.type == BuildingType.Tavern) return $"{(level == 0 ? 0 : level + 1)} aventureiros\npor onda";
            string unit = d.type == BuildingType.ArcherStand ? "arqueiros" : d.type == BuildingType.MagicSchool ? "magos" : "guerreiros";
            return $"{RecruitmentService.CalculateCapacity(level)} {unit}\npor reestoque";
        }
        private static TMP_Text Text(VisualReviewContext context, string name) => context.TargetRoot.GetComponentsInChildren<TMP_Text>(true).Single(text => text.name == name);
        private static string Money(int value) => value.ToString("N0", CultureInfo.InvariantCulture).Replace(',', ' ');
    }
}
