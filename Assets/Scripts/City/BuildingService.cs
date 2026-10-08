using System;
using System.Collections.Generic;
using System.Linq;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Economy;

namespace TCCGame.Gameplay.City
{
    public enum BuildingUpgradeStatus
    {
        Upgraded,
        MaximumLevel,
        InsufficientMoney,
        InsufficientResources
    }

    public sealed class BuildingService
    {
        private readonly GameSaveData state;
        private readonly EconomyService economy;
        private readonly Action commit;
        private readonly Dictionary<BuildingType, BuildingDefinition>
            definitions = new();

        public event Action<BuildingType, int, int> LevelChanged;

        public BuildingService(
            GameSaveData state,
            EconomyService economy,
            Action commit
        )
        {
            this.state = state ??
                throw new ArgumentNullException(nameof(state));
            this.economy = economy ??
                throw new ArgumentNullException(nameof(economy));
            this.commit = commit ??
                throw new ArgumentNullException(nameof(commit));
            state.playerInventory ??= new PlayerInventorySaveData();
            state.playerInventory.items ??= new List<InventoryItemSaveData>();
        }

        public int GetLevel(BuildingType type)
        {
            return state.GetBuildingLevel(type);
        }

        public void RegisterDefinition(BuildingDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (definitions.TryGetValue(definition.type, out BuildingDefinition existing) &&
                existing != definition)
            {
                throw new InvalidOperationException(
                    $"Building type duplicated: {definition.type}"
                );
            }

            definitions[definition.type] = definition;
        }

        public int GetProduction(BuildingType type)
        {
            return GetProduction(type, GetLevel(type));
        }

        public int GetProduction(BuildingType type, int level)
        {
            return definitions.TryGetValue(
                type,
                out BuildingDefinition definition
            )
                ? BuildingCalculator.GetProduction(definition, level)
                : 0;
        }

        public bool CanAffordUpgrade(BuildingDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            int currentLevel = GetLevel(definition.type);
            return currentLevel < definition.maximumLevel &&
                economy.CanAfford(BuildingCalculator.GetUpgradeCost(
                    definition,
                    currentLevel
                )) && HasRequiredResources(definition, currentLevel);
        }

        public int GetResourceQuantity(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                return 0;
            }

            InventoryItemSaveData item = state.playerInventory.items.Find(
                candidate => candidate != null &&
                    candidate.itemId == resourceId.Trim()
            );
            return item?.quantity ?? 0;
        }

        public BuildingUpgradeStatus TryUpgrade(
            BuildingDefinition definition
        )
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            int previousLevel = GetLevel(definition.type);

            if (previousLevel >= definition.maximumLevel)
            {
                return BuildingUpgradeStatus.MaximumLevel;
            }

            int cost = BuildingCalculator.GetUpgradeCost(
                definition,
                previousLevel
            );

            if (!economy.CanAfford(cost))
            {
                return BuildingUpgradeStatus.InsufficientMoney;
            }

            if (!HasRequiredResources(definition, previousLevel))
            {
                return BuildingUpgradeStatus.InsufficientResources;
            }

            int previousMoney = economy.Money;
            int newLevel = previousLevel + 1;
            Dictionary<InventoryItemSaveData, int> previousQuantities =
                CaptureResourceQuantities(definition, previousLevel);

            try
            {
                economy.DebitWithoutCommit(cost);
                ConsumeResources(definition, previousLevel);
                state.SetBuildingLevel(definition.type, newLevel);
                commit();
            }
            catch
            {
                economy.RestoreMoney(previousMoney);
                foreach (KeyValuePair<InventoryItemSaveData, int> pair in
                         previousQuantities)
                {
                    pair.Key.quantity = pair.Value;
                }
                state.SetBuildingLevel(definition.type, previousLevel);
                throw;
            }

            economy.PublishChange();
            LevelChanged?.Invoke(definition.type, previousLevel, newLevel);
            return BuildingUpgradeStatus.Upgraded;
        }

        private bool HasRequiredResources(
            BuildingDefinition definition,
            int currentLevel
        )
        {
            foreach (IGrouping<string, BuildingResourceRequirement> group in
                     ValidRequirements(definition, currentLevel)
                         .GroupBy(requirement => requirement.ResourceId))
            {
                int required = group.Sum(requirement =>
                    Math.Max(1, requirement.quantity));
                if (GetResourceQuantity(group.Key) < required)
                {
                    return false;
                }
            }

            return BuildingCalculator.GetResourceRequirements(
                    definition,
                    currentLevel
                )
                .All(requirement => requirement?.resource != null);
        }

        private Dictionary<InventoryItemSaveData, int>
            CaptureResourceQuantities(
                BuildingDefinition definition,
                int currentLevel
            )
        {
            var result = new Dictionary<InventoryItemSaveData, int>();
            foreach (BuildingResourceRequirement requirement in
                     ValidRequirements(definition, currentLevel))
            {
                InventoryItemSaveData item = state.playerInventory.items.Find(
                    candidate => candidate != null &&
                        candidate.itemId == requirement.ResourceId
                );
                if (item != null && !result.ContainsKey(item))
                {
                    result[item] = item.quantity;
                }
            }

            return result;
        }

        private void ConsumeResources(
            BuildingDefinition definition,
            int currentLevel
        )
        {
            foreach (IGrouping<string, BuildingResourceRequirement> group in
                     ValidRequirements(definition, currentLevel)
                         .GroupBy(requirement => requirement.ResourceId))
            {
                int quantity = group.Sum(requirement =>
                    Math.Max(1, requirement.quantity));
                InventoryItemSaveData item = state.playerInventory.items.Find(
                    candidate => candidate != null &&
                        candidate.itemId == group.Key
                );
                item.quantity -= quantity;
            }
        }

        private static IEnumerable<BuildingResourceRequirement>
            ValidRequirements(
                BuildingDefinition definition,
                int currentLevel
            )
        {
            return BuildingCalculator.GetResourceRequirements(
                    definition,
                    currentLevel
                )
                .Where(requirement => requirement?.resource != null);
        }
    }
}
