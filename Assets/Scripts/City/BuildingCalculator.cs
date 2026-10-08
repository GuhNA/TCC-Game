using UnityEngine;

public static class BuildingCalculator
{
    public static int GetUpgradeCost(
        BuildingDefinition definition,
        int currentLevel
    )
    {
        BuildingUpgradeLevelCost configured = GetConfiguredLevelCost(
            definition,
            currentLevel
        );
        if (configured != null && configured.goldCost >= 0)
        {
            return configured.goldCost;
        }

        return Mathf.RoundToInt(
            definition.baseUpgradeCost *
            Mathf.Pow(
                definition.upgradeCostMultiplier,
                currentLevel
            )
        );
    }

    public static BuildingResourceRequirement[] GetResourceRequirements(
        BuildingDefinition definition,
        int currentLevel
    )
    {
        BuildingUpgradeLevelCost configured = GetConfiguredLevelCost(
            definition,
            currentLevel
        );
        return configured?.resources ??
            System.Array.Empty<BuildingResourceRequirement>();
    }

    private static BuildingUpgradeLevelCost GetConfiguredLevelCost(
        BuildingDefinition definition,
        int currentLevel
    )
    {
        if (definition == null || currentLevel < 0 ||
            definition.levelUpgradeCosts == null ||
            currentLevel >= definition.levelUpgradeCosts.Length)
        {
            return null;
        }

        return definition.levelUpgradeCosts[currentLevel];
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
