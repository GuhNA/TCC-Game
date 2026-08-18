using System.Collections.Generic;
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

    private readonly Dictionary<BuildingType, BuildingState>
        buildingStates = new();

    private BuildingBehaviour selectedBuilding;
    private EconomyService economy;

    private void Awake()
    {
        economy = GameSessionProvider.Current.Economy;
        CreateInitialStates();
        InitializeBuildings();

        buildingPanel.UpgradeRequested += UpgradeSelectedBuilding;
        economy.MoneyChanged += HandleMoneyChanged;
    }

    private void OnDestroy()
    {
        buildingPanel.UpgradeRequested -= UpgradeSelectedBuilding;
        economy.MoneyChanged -= HandleMoneyChanged;

        foreach (BuildingBehaviour building in buildings)
        {
            building.Selected -= SelectBuilding;
        }
    }

    private void CreateInitialStates()
    {
        foreach (BuildingBehaviour building in buildings)
        {
            BuildingType type = building.Definition.type;

            if (buildingStates.ContainsKey(type))
            {
                Debug.LogError(
                    $"Building type duplicated: {type}"
                );

                continue;
            }

            buildingStates.Add(
                type,
                new BuildingState(
                    type,
                    Mathf.Clamp(
                        GameSessionProvider.Current.Buildings
                            .GetLevel(type),
                        0,
                        building.Definition.maximumLevel
                    )
                )
            );
        }
    }

    private void InitializeBuildings()
    {
        foreach (BuildingBehaviour building in buildings)
        {
            BuildingType type = building.Definition.type;

            building.Initialize(buildingStates[type]);
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

        BuildingDefinition definition =
            selectedBuilding.Definition;

        BuildingState state =
            selectedBuilding.State;

        if (state.level >= definition.maximumLevel)
        {
            return;
        }

        BuildingUpgradeStatus result =
            GameSessionProvider.Current.Buildings.TryUpgrade(definition);

        if (result != BuildingUpgradeStatus.Upgraded)
        {
            RefreshSelectedBuildingPanel();
            return;
        }

        selectedBuilding.IncreaseLevel();
        RefreshSelectedBuildingPanel();
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
            selectedBuilding.State.level
        );

        bool canAfford = economy.CanAfford(cost);

        buildingPanel.Show(
            selectedBuilding,
            canAfford
        );
    }

}
