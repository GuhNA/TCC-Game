using System;
using System.Collections.Generic;
using UnityEngine;

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
public class CitySaveData
{
    public bool hasMoney;
    public int money;
    public List<BuildingSaveData> buildings = new();
    public TaxAccrualSaveData taxes = new();

    public int GetBuildingLevel(BuildingType type)
    {
        BuildingSaveData savedBuilding = buildings.Find(
            building => building.type == type
        );

        return savedBuilding == null ? 0 : savedBuilding.level;
    }
}

public static class CitySaveRepository
{
    private const string SaveKey = "tcc.city.save.v1";

    public static CitySaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            return new CitySaveData();
        }

        CitySaveData saveData = JsonUtility.FromJson<CitySaveData>(
            PlayerPrefs.GetString(SaveKey)
        );

        if (saveData == null)
        {
            return new CitySaveData();
        }

        saveData.buildings ??= new List<BuildingSaveData>();
        saveData.taxes ??= new TaxAccrualSaveData();

        return saveData;
    }

    public static void SaveMoney(int money)
    {
        CitySaveData saveData = Load();
        saveData.hasMoney = true;
        saveData.money = money;
        Save(saveData);
    }

    public static void SaveBuildingLevel(BuildingType type, int level)
    {
        CitySaveData saveData = Load();
        BuildingSaveData savedBuilding = saveData.buildings.Find(
            building => building.type == type
        );

        if (savedBuilding == null)
        {
            savedBuilding = new BuildingSaveData { type = type };
            saveData.buildings.Add(savedBuilding);
        }

        savedBuilding.level = level;
        Save(saveData);
    }

    public static void SaveTaxes(TaxAccrualSaveData taxes)
    {
        CitySaveData saveData = Load();
        saveData.taxes = taxes;
        Save(saveData);
    }

    private static void Save(CitySaveData saveData)
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }
}
