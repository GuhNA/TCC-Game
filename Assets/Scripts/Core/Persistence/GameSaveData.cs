using System;
using System.Collections.Generic;
using TCCGame.Core;
using TCCGame.Gameplay.Exploration;
using UnityEngine;

namespace TCCGame.Core.Persistence
{
    [Serializable]
    public class BuildingSaveData
    {
        public BuildingType type;
        public int level;
    }

    [Serializable]
    public class TaxAccrualSaveData
    {
        public bool initialized;
        public long lastCalculatedUnixSeconds;
        public long lastCollectedUnixSeconds;
        public double accruedTax;
        public double eligibleSecondsSinceCollection;
    }

    [Serializable]
    public class ArmySaveData
    {
        public int warriors;
        public int archers;
        public int mages;

        public int Get(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Archer => archers,
                UnitType.Warrior => warriors,
                UnitType.Mage => mages,
                _ => 0
            };
        }

        public void Set(UnitType unitType, int value)
        {
            switch (unitType)
            {
                case UnitType.Archer:
                    archers = value;
                    break;

                case UnitType.Warrior:
                    warriors = value;
                    break;

                case UnitType.Mage:
                    mages = value;
                    break;
            }
        }
    }

    [Serializable]
    public class RecruitmentSaveData
    {
        public bool initialized;
        public long nextRestockUnixSeconds;
        public int availableWarriors = 10;
        public int availableArchers = 10;
        public int availableMages = 10;

        public int GetAvailable(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Archer => availableArchers,
                UnitType.Warrior => availableWarriors,
                UnitType.Mage => availableMages,
                _ => 0
            };
        }

        public void SetAvailable(UnitType unitType, int value)
        {
            switch (unitType)
            {
                case UnitType.Archer:
                    availableArchers = value;
                    break;

                case UnitType.Warrior:
                    availableWarriors = value;
                    break;

                case UnitType.Mage:
                    availableMages = value;
                    break;
            }
        }
    }

    [Serializable]
    public class InventoryItemSaveData
    {
        public string itemId;
        public int quantity;
    }

    [Serializable]
    public class PlayerInventorySaveData
    {
        public List<string> recruitedAdventurerIds = new();
        public List<InventoryItemSaveData> items = new();
    }

    [Serializable]
    public class GameSaveData
    {
        public const int CurrentVersion = 8;
        public const int DefaultMoney = 1000;

        public int version = CurrentVersion;
        public int money = DefaultMoney;
        public List<BuildingSaveData> buildings = new();
        public TaxAccrualSaveData taxes = new();
        public ArmySaveData army = new();
        public RecruitmentSaveData recruitment = new();
        public PlayerInventorySaveData playerInventory = new();
        public CampaignSaveData campaign = new();
        public NavigationSaveData navigation = new();
        public ExplorationSaveData exploration = new();

        public int GetBuildingLevel(BuildingType type)
        {
            BuildingSaveData savedBuilding = buildings.Find(
                building => building != null && building.type == type
            );

            return savedBuilding == null ? 0 : savedBuilding.level;
        }

        public void SetBuildingLevel(BuildingType type, int level)
        {
            BuildingSaveData savedBuilding = buildings.Find(
                building => building != null && building.type == type
            );

            if (savedBuilding == null)
            {
                savedBuilding = new BuildingSaveData { type = type };
                buildings.Add(savedBuilding);
            }

            savedBuilding.level = level;
        }

        public static GameSaveData CreateDefault()
        {
            return new GameSaveData();
        }

        public GameSaveData Clone()
        {
            return JsonUtility.FromJson<GameSaveData>(
                JsonUtility.ToJson(this)
            );
        }
    }
}
