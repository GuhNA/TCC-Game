using System.Collections.Generic;
using UnityEngine;

public class CityManager : MonoBehaviour
{
    [SerializeField]
    private EconomyManager economyManager;

    [SerializeField]
    private BuildingPanel buildingPanel;

    [SerializeField]
    private BuildingBehaviour[] buildings;

    private readonly Dictionary<BuildingType, BuildingState>
        buildingStates = new();

    private BuildingBehaviour selectedBuilding;

    private void Awake()
    {
        CreateInitialStates();
        InitializeBuildings();

        buildingPanel.UpgradeRequested += UpgradeSelectedBuilding;
        economyManager.MoneyChanged += HandleMoneyChanged;
    }

    private void OnDestroy()
    {
        buildingPanel.UpgradeRequested -= UpgradeSelectedBuilding;
        economyManager.MoneyChanged -= HandleMoneyChanged;

        foreach (BuildingBehaviour building in buildings)
        {
            building.Selected -= SelectBuilding;
            building.LevelChanged -= HandleBuildingLevelChanged;
        }
    }

    private void CreateInitialStates()
    {
        CitySaveData saveData = CitySaveRepository.Load();

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
                        saveData.GetBuildingLevel(type),
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
            building.LevelChanged += HandleBuildingLevelChanged;
        }
    }

    private void HandleBuildingLevelChanged(
        BuildingBehaviour building,
        int previousLevel,
        int newLevel
    )
    {
        CitySaveRepository.SaveBuildingLevel(
            building.Definition.type,
            newLevel
        );
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

        int cost = BuildingCalculator.GetUpgradeCost(
            definition,
            state.level
        );

        if (!economyManager.TrySpend(cost))
        {
            RefreshSelectedBuildingPanel();
            return;
        }

        selectedBuilding.IncreaseLevel();
        RefreshSelectedBuildingPanel();

        RecalculateCityProduction();
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

        bool canAfford = economyManager.CanAfford(cost);

        buildingPanel.Show(
            selectedBuilding,
            canAfford
        );
    }

    private void RecalculateCityProduction()
    {
        foreach (BuildingBehaviour building in buildings)
        {
            int production = BuildingCalculator.GetProduction(
                building.Definition,
                building.State.level
            );

            Debug.Log(
                $"{building.Definition.displayName}: {production}"
            );
        }
    }
}
