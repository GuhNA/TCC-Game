using System;
using System.Collections.Generic;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Economy;

namespace TCCGame.Gameplay.City
{
    public enum BuildingUpgradeStatus
    {
        Upgraded,
        MaximumLevel,
        InsufficientMoney
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

            int previousMoney = economy.Money;
            int newLevel = previousLevel + 1;

            try
            {
                economy.DebitWithoutCommit(cost);
                state.SetBuildingLevel(definition.type, newLevel);
                commit();
            }
            catch
            {
                economy.RestoreMoney(previousMoney);
                state.SetBuildingLevel(definition.type, previousLevel);
                throw;
            }

            economy.PublishChange();
            LevelChanged?.Invoke(definition.type, previousLevel, newLevel);
            return BuildingUpgradeStatus.Upgraded;
        }
    }
}
