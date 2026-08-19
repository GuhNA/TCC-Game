using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text buildingNameText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private TMP_Text levelText;

    [SerializeField]
    private TMP_Text productionText;

    [SerializeField]
    private TMP_Text upgradeCostText;

    [SerializeField]
    private TMP_Text upgradeButtonText;

    [SerializeField]
    private Button upgradeButton;

    private moviment playerMovement;

    public event Action UpgradeRequested;

    private void Awake()
    {
        playerMovement = FindFirstObjectByType<moviment>();

        upgradeButton.onClick.AddListener(
            () => UpgradeRequested?.Invoke()
        );

        gameObject.SetActive(false);
    }

    internal void Show(
        BuildingBehaviour building,
        bool canAfford
    )
    {
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<moviment>();
        }

        BuildingDefinition definition = building.Definition;
        int currentLevel = building.Level;

        buildingNameText.text = definition.displayName;
        descriptionText.text = definition.description;

        levelText.text =
            $"Nível: {currentLevel}/{definition.maximumLevel}";

        int production = BuildingCalculator.GetProduction(
            definition,
            currentLevel
        );

        productionText.text = $"Produção: {production}";

        bool isMaximumLevel =
            currentLevel >= definition.maximumLevel;

        if (isMaximumLevel)
        {
            upgradeCostText.text = "Nível máximo";
            upgradeButton.interactable = false;
            upgradeButtonText.text = "Máximo";
        }
        else
        {
            int cost = BuildingCalculator.GetUpgradeCost(
                definition,
                currentLevel
            );

            upgradeCostText.text = $"Custo: {cost}";
            upgradeButton.interactable = canAfford;

            upgradeButtonText.text =
                currentLevel == 0
                    ? "Construir"
                    : "Melhorar";
        }

        gameObject.SetActive(true);
        playerMovement?.SetMovementBlocked(true);
    }

    public void Hide()
    {
        playerMovement?.SetMovementBlocked(false);
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        playerMovement?.SetMovementBlocked(false);
    }
}
