using UnityEngine;

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

    [Header("Produção")]
    [Min(0)]
    public int baseProduction;

    [Min(0)]
    public int productionPerLevel;

    [Header("Visual")]
    public Sprite unbuiltSprite;
    public Sprite[] levelSprites;
}
