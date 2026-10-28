using System;
using System.Globalization;
using System.Linq;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Input;
using TCCGame.Gameplay.Recruitment;
using TCCGame.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingPanel : MonoBehaviour, IModalInteractionOwner
{
    [SerializeField] private TMP_Text buildingNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text productionText;
    [SerializeField] private TMP_Text nextCaptionText;
    [SerializeField] private TMP_Text nextBenefitText;
    [SerializeField] private TMP_Text upgradeCostText;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text helpText;
    [SerializeField] private TMP_Text upgradeButtonText;
    [SerializeField] private Image illustration;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private MedievalHudTheme theme;

    private PlayerInteractor lockedInteractor;
    private moviment fallbackMovement;
    private GameObject previousSelection;
    private bool visible;
    private int lastUpgradeFrame = -1;

    public event Action UpgradeRequested;
    public event Action Closed;

    private void Awake()
    {
        upgradeButton.onClick.AddListener(RequestUpgrade);
        backButton.onClick.AddListener(Hide);
        closeButton.onClick.AddListener(Hide);
    }

    private void Update()
    {
        if (visible && GameInput.WasCancelPressedThisFrame()) Hide();
    }

    private void RequestUpgrade()
    {
        if (!visible || !upgradeButton.interactable || lastUpgradeFrame == Time.frameCount) return;
        lastUpgradeFrame = Time.frameCount;
        UpgradeRequested?.Invoke();
    }

    internal void Show(BuildingBehaviour building, bool canAfford, string feedback = null)
    {
        if (!visible && UiMenuState.HasOpenMenu) return;
        var session = GameSessionProvider.Current;
        string resources = string.Join(", ", BuildingCalculator
            .GetResourceRequirements(building.Definition, building.Level)
            .Where(requirement => requirement != null)
            .Select(requirement =>
                $"{session.Buildings.GetResourceQuantity(requirement.ResourceId)}/" +
                $"{requirement.quantity} {requirement.DisplayName}"));
        Present(building.Definition, building.Level, session.Economy.Money, canAfford, resources, feedback);
        if (visible)
        {
            EnsureUsableSelection();
            return;
        }

        previousSelection = EventSystem.current?.currentSelectedGameObject;
        gameObject.SetActive(true);
        transform.localScale = Vector3.one;
        visible = true;
        UiMenuState.SetOpen(this, true);
        lockedInteractor = FindFirstObjectByType<PlayerInteractor>();
        if (lockedInteractor != null) lockedInteractor.AcquireModalLock(this);
        else
        {
            fallbackMovement = FindFirstObjectByType<moviment>();
            fallbackMovement?.SetMovementBlocked(true);
        }
        SelectDefault();
    }

    /// <summary>Renders supplied values, including visual review without accessing a save.</summary>
    public void Present(BuildingDefinition definition, int currentLevel, int money,
        bool canAfford, string resources = "", string feedback = null)
    {
        bool maximum = currentLevel >= definition.maximumLevel;
        int next = Mathf.Min(currentLevel + 1, definition.maximumLevel);
        int cost = BuildingCalculator.GetUpgradeCost(definition, currentLevel);
        buildingNameText.text = definition.displayName.ToUpperInvariant();
        descriptionText.text = definition.type == BuildingType.LocalMarket
            ? "Mais impostos acumulados por minuto enquanto o Mercado estiver construído."
            : definition.type == BuildingType.Tavern
                ? "Gerencie o edifício aqui; use a porta para entrar na Taverna."
                : "Mais recrutas disponíveis no próximo reestoque do Portão.";
        levelText.text = maximum ? $"NÍVEL {currentLevel}/{definition.maximumLevel}"
            : currentLevel == 0 ? "PRÉVIA • NÍVEL 0 → 1" : $"NÍVEL {currentLevel} → {next}";
        productionText.text = Benefit(definition, currentLevel);
        nextCaptionText.text = maximum ? "CONCLUÍDO" : "PRÓXIMO NÍVEL";
        nextBenefitText.text = maximum ? "Limite de melhoria\natingido" : Benefit(definition, next);
        upgradeCostText.text = maximum ? "Nenhum novo custo" : $"Custo: {cost} ouro";
        balanceText.text = maximum ? "Todos os níveis deste edifício concluídos."
            : money < cost ? $"Seu saldo: {Money(money)} • Faltam {Money(cost - money)} ouro"
                : $"Seu saldo: {Money(money)} • Após: {Money(money - cost)} ouro";
        if (!maximum && !string.IsNullOrEmpty(resources)) balanceText.text += "\n" + resources;
        feedbackText.text = feedback ?? (maximum
            ? "Edifício no nível máximo. Seu benefício atual continua ativo."
            : money < cost ? "Ouro insuficiente. Colete impostos ou retorne com mais recursos."
            : !canAfford ? "Recursos insuficientes. Confira os materiais exigidos para melhorar."
            : definition.type == BuildingType.LocalMarket ? "O novo rendimento passa a valer após a melhoria."
            : definition.type == BuildingType.Tavern ? "A nova capacidade será usada na próxima onda de aventureiros."
            : "A melhoria altera o próximo reestoque (6h); não concede tropas imediatamente.");
        illustration.sprite = definition.levelSprites != null && definition.levelSprites.Length > 0
            ? definition.levelSprites[Mathf.Clamp(currentLevel - 1, 0, definition.levelSprites.Length - 1)]
            : definition.unbuiltSprite;
        illustration.enabled = illustration.sprite != null;
        upgradeButton.gameObject.SetActive(!maximum);
        upgradeButton.interactable = !maximum && canAfford;
        upgradeButtonText.text = currentLevel == 0 ? $"CONSTRUIR POR {cost} OURO" : $"MELHORAR PARA NÍVEL {next}";
        upgradeButtonText.color = canAfford ? theme.GoldButtonLabel : new Color(1f, .91f, .7f);
        backButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(maximum ? 0 : -330, -325);
        helpText.text = maximum ? "Esc fecha o painel • Benefício mantido no nível máximo"
            : "Esc fecha o painel • Custo descontado apenas ao confirmar";
    }

    private static string Benefit(BuildingDefinition definition, int level)
    {
        if (definition.type == BuildingType.LocalMarket)
            return $"{BuildingCalculator.GetProduction(definition, level)} ouro\npor minuto";
        if (definition.type == BuildingType.Tavern)
            return $"{(level <= 0 ? 0 : level + 1)} aventureiros\npor onda";
        if (definition.type == BuildingType.ExplorersLodge)
            return $"{level} exploradores\ndisponíveis";
        string unit = definition.type == BuildingType.ArcherStand ? "arqueiros"
            : definition.type == BuildingType.MagicSchool ? "magos" : "guerreiros";
        return $"{RecruitmentService.CalculateCapacity(level)} {unit}\npor reestoque";
    }

    private static string Money(int amount) => amount.ToString("N0", CultureInfo.InvariantCulture).Replace(',', ' ');

    private void EnsureUsableSelection()
    {
        GameObject selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null || !selected.activeInHierarchy ||
            (selected.TryGetComponent(out Selectable control) && !control.IsInteractable())) SelectDefault();
    }

    private void SelectDefault() => EventSystem.current?.SetSelectedGameObject(
        upgradeButton.gameObject.activeSelf && upgradeButton.interactable ? upgradeButton.gameObject : backButton.gameObject);

    public void CloseModal() => Hide();

    public void Hide() => gameObject.SetActive(false);

    private void OnDisable()
    {
        UiMenuState.SetOpen(this, false);
        lockedInteractor?.ReleaseModalLock(this);
        lockedInteractor = null;
        fallbackMovement?.SetMovementBlocked(false);
        fallbackMovement = null;
        if (!visible) return;
        visible = false;
        EventSystem.current?.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy
            ? previousSelection : null);
        previousSelection = null;
        Closed?.Invoke();
    }
}
