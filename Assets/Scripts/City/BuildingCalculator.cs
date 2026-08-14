using UnityEngine;

public static class BuildingCalculator
{
    public static int GetUpgradeCost(
        BuildingDefinition definition,
        int currentLevel
    )
    {
        return Mathf.RoundToInt(
            definition.baseUpgradeCost *
            Mathf.Pow(
                definition.upgradeCostMultiplier,
                currentLevel
            )
        );
    }

    public static int GetProduction(
        BuildingDefinition definition,
        int level
    )
    {
        if (level <= 0)
        {
            return 0;
        }

        return definition.baseProduction +
               definition.productionPerLevel * (level - 1);
    }
}
