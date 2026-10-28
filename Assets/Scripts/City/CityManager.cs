using TCCGame.Core;
using TCCGame.Gameplay.City;
using TCCGame.Gameplay.Economy;
using UnityEngine;

public class CityManager : MonoBehaviour
{
    [SerializeField]
    private BuildingPanel buildingPanel;

    [SerializeField]
    private BuildingBehaviour[] buildings;

    private BuildingBehaviour selectedBuilding;
    private EconomyService economy;
    private BuildingService buildingService;
    private bool upgrading;

    private void Awake()
    {
        GameSession session = GameSessionProvider.Current;
        economy = session.Economy;
        buildingService = session.Buildings;
        InitializeBuildings();

        buildingPanel.UpgradeRequested += UpgradeSelectedBuilding;
        buildingPanel.Closed += HandlePanelClosed;
        economy.MoneyChanged += HandleMoneyChanged;
        buildingService.LevelChanged += HandleBuildingLevelChanged;
    }

    private void OnDestroy()
    {
        buildingPanel.UpgradeRequested -= UpgradeSelectedBuilding;
        buildingPanel.Closed -= HandlePanelClosed;
        economy.MoneyChanged -= HandleMoneyChanged;
        buildingService.LevelChanged -= HandleBuildingLevelChanged;

        foreach (BuildingBehaviour building in buildings)
        {
            building.Selected -= SelectBuilding;
        }
    }

    private void InitializeBuildings()
    {
        foreach (BuildingBehaviour building in buildings)
        {
            buildingService.RegisterDefinition(building.Definition);
            BuildingType type = building.Definition.type;

            building.Initialize(buildingService.GetLevel(type));
            building.Selected += SelectBuilding;
        }
    }

    private void SelectBuilding(BuildingBehaviour building)
    {
        selectedBuilding = building;
        RefreshSelectedBuildingPanel();
    }

    private void UpgradeSelectedBuilding()
    {
        if (selectedBuilding == null || upgrading || !buildingPanel.isActiveAndEnabled)
        {
            return;
        }

        BuildingDefinition definition = selectedBuilding.Definition;
        BuildingUpgradeStatus result;
        upgrading = true;
        try
        {
            result = buildingService.TryUpgrade(definition);
        }
        finally { upgrading = false; }
        RefreshSelectedBuildingPanel(result == BuildingUpgradeStatus.Upgraded
            ? $"{definition.displayName} no nível {selectedBuilding.Level}. Melhoria concluída."
            : null);
    }

    private void HandleBuildingLevelChanged(
        BuildingType type,
        int previousLevel,
        int newLevel
    )
    {
        foreach (BuildingBehaviour building in buildings)
        {
            if (building.Definition.type == type)
            {
                building.RenderLevel(newLevel);
                break;
            }
        }
        if (!upgrading && buildingPanel.isActiveAndEnabled) RefreshSelectedBuildingPanel();
    }

    private void HandleMoneyChanged(int newMoney)
    {
        if (!upgrading && buildingPanel.isActiveAndEnabled)
        {
            RefreshSelectedBuildingPanel();
        }
    }

    private void HandlePanelClosed() => selectedBuilding = null;

    private void RefreshSelectedBuildingPanel(string feedback = null)
    {
        if (selectedBuilding == null)
        {
            return;
        }

        bool canAfford = buildingService.CanAffordUpgrade(
            selectedBuilding.Definition
        );

        buildingPanel.Show(
            selectedBuilding,
            canAfford,
            feedback
        );
    }

}
