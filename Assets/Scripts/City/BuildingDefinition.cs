using System;
using TCCGame.Gameplay.Exploration;
using UnityEngine;

[Serializable]
public sealed class BuildingResourceRequirement
{
    public ExplorationResourceDefinition resource;
    [Min(1)] public int quantity = 1;

    public string ResourceId => resource == null
        ? string.Empty
        : resource.PersistenceId;
    public string DisplayName => resource == null
        ? "Recurso não configurado"
        : resource.DisplayName;
}

[Serializable]
public sealed class BuildingUpgradeLevelCost
{
    [Tooltip("Use -1 para manter o cálculo Base Cost x Multiplier.")]
    [Min(-1)] public int goldCost = -1;
    public BuildingResourceRequirement[] resources =
        Array.Empty<BuildingResourceRequirement>();
}

[CreateAssetMenu(
    fileName = "BuildingDefinition",
    menuName = "Sistema/City/Building Definition"
)]
public class BuildingDefinition : ScriptableObject
{
    [Header("Identificação")]
    public BuildingType type;
    public string displayName;

    [TextArea]
    public string description;

    [Header("Evolução")]
    [Min(1)]
    public int maximumLevel = 10;

    [Min(0)]
    public int baseUpgradeCost = 100;

    [Min(1f)]
    public float upgradeCostMultiplier = 1.5f;

    [Tooltip("Custo explícito por nível-alvo. O elemento 0 constrói o nível 1.")]
    public BuildingUpgradeLevelCost[] levelUpgradeCosts =
        Array.Empty<BuildingUpgradeLevelCost>();

    [Header("Produção")]
    [Min(0)]
    public int baseProduction;

    [Min(0)]
    public int productionPerLevel;

    [Header("Visual")]
    public Sprite unbuiltSprite;
    public Sprite[] levelSprites;
}
