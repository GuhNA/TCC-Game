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

    private void Awake()
    {
        GameSession session = GameSessionProvider.Current;
        economy = session.Economy;
        buildingService = session.Buildings;
        InitializeBuildings();

        buildingPanel.UpgradeRequested += UpgradeSelectedBuilding;
        economy.MoneyChanged += HandleMoneyChanged;
        buildingService.LevelChanged += HandleBuildingLevelChanged;
    }

    private void OnDestroy()
    {
        buildingPanel.UpgradeRequested -= UpgradeSelectedBuilding;
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
        if (selectedBuilding == null)
        {
            return;
        }

        BuildingDefinition definition = selectedBuilding.Definition;
        BuildingUpgradeStatus result = buildingService.TryUpgrade(definition);

        if (result != BuildingUpgradeStatus.Upgraded)
        {
            RefreshSelectedBuildingPanel();
            return;
        }
        RefreshSelectedBuildingPanel();
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
    }

    private void HandleMoneyChanged(int newMoney)
    {
        if (buildingPanel.isActiveAndEnabled)
        {
            RefreshSelectedBuildingPanel();
        }
    }

    private void RefreshSelectedBuildingPanel()
    {
        if (selectedBuilding == null)
        {
            return;
        }

        int cost = BuildingCalculator.GetUpgradeCost(
            selectedBuilding.Definition,
            selectedBuilding.Level
        );

        bool canAfford = economy.CanAfford(cost);

        buildingPanel.Show(
            selectedBuilding,
            canAfford
        );
    }

}
