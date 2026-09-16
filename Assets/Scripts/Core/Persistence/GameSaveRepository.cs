using System;
using System.Collections.Generic;
using UnityEngine;
using TCCGame.Gameplay.Exploration;

namespace TCCGame.Core.Persistence
{
    public interface IGameSaveRepository
    {
        GameSaveData Load();
        void Save(GameSaveData saveData);
    }

    [Serializable]
    internal sealed class LegacyCitySaveData
    {
        public bool hasMoney;
        public int money;
        public List<BuildingSaveData> buildings = new();
        public TaxAccrualSaveData taxes = new();
    }

    public sealed class GameSaveRepository : IGameSaveRepository
    {
        public const string SaveKey = "tcc.game.save.v2";
        public const string CorruptSaveKey = SaveKey + ".corrupt";
        public const string LegacyCitySaveKey = "tcc.city.save.v1";

        public GameSaveData Load()
        {
            bool hasCurrentSave = PlayerPrefs.HasKey(SaveKey);
            string rawCurrentSave = hasCurrentSave
                ? PlayerPrefs.GetString(SaveKey)
                : null;
            GameSaveData saveData = null;

            if (hasCurrentSave)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(rawCurrentSave))
                    {
                        throw new ArgumentException(
                            "The current save is empty."
                        );
                    }

                    saveData = JsonUtility.FromJson<GameSaveData>(
                        rawCurrentSave
                    );
                    if (saveData == null)
                    {
                        throw new ArgumentException(
                            "The current save could not be parsed."
                        );
                    }
                }
                catch (ArgumentException exception)
                {
                    return RecoverFromCorruptCurrentSave(
                        rawCurrentSave,
                        exception
                    );
                }
            }

            if (saveData != null)
            {
                int loadedVersion = saveData.version;
                Normalize(saveData);

                if (loadedVersion != GameSaveData.CurrentVersion)
                {
                    Save(saveData);
                }

                return saveData;
            }

            saveData = MigrateLegacyCitySave();
            Normalize(saveData);
            Save(saveData);

            return saveData;
        }

        private static GameSaveData RecoverFromCorruptCurrentSave(
            string rawSave,
            Exception exception
        )
        {
            if (!PlayerPrefs.HasKey(CorruptSaveKey))
            {
                PlayerPrefs.SetString(CorruptSaveKey, rawSave ?? string.Empty);
            }

            Debug.LogWarning(
                "The current save is invalid; a default save was created. " +
                exception.GetType().Name
            );
            GameSaveData recovered = GameSaveData.CreateDefault();
            Normalize(recovered);
            PlayerPrefs.SetString(
                SaveKey,
                JsonUtility.ToJson(recovered)
            );
            PlayerPrefs.Save();
            return recovered;
        }

        public void Save(GameSaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            Normalize(saveData);
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
            PlayerPrefs.Save();
        }

        private static GameSaveData LoadCurrentSave()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return null;
            }

            string json = PlayerPrefs.GetString(SaveKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonUtility.FromJson<GameSaveData>(json);
        }

        private static GameSaveData MigrateLegacyCitySave()
        {
            GameSaveData migrated = GameSaveData.CreateDefault();

            if (!PlayerPrefs.HasKey(LegacyCitySaveKey))
            {
                return migrated;
            }

            LegacyCitySaveData legacy;
            try
            {
                legacy = JsonUtility.FromJson<LegacyCitySaveData>(
                    PlayerPrefs.GetString(LegacyCitySaveKey)
                );
            }
            catch (ArgumentException exception)
            {
                Debug.LogWarning(
                    "The legacy save is invalid; using a default state. " +
                    exception.GetType().Name
                );
                return migrated;
            }

            if (legacy == null)
            {
                return migrated;
            }

            if (legacy.hasMoney)
            {
                migrated.money = legacy.money;
            }

            if (legacy.buildings != null)
            {
                foreach (BuildingSaveData building in legacy.buildings)
                {
                    if (building == null)
                    {
                        continue;
                    }

                    migrated.SetBuildingLevel(
                        building.type,
                        building.level
                    );
                }
            }

            if (legacy.taxes != null)
            {
                migrated.taxes = new TaxAccrualSaveData
                {
                    initialized = legacy.taxes.initialized,
                    lastCalculatedUnixSeconds =
                        legacy.taxes.lastCalculatedUnixSeconds,
                    lastCollectedUnixSeconds =
                        legacy.taxes.lastCollectedUnixSeconds,
                    accruedTax = legacy.taxes.accruedTax,
                    eligibleSecondsSinceCollection =
                        legacy.taxes.eligibleSecondsSinceCollection
                };
            }

            return migrated;
        }

        private static void Normalize(GameSaveData saveData)
        {
            saveData.version = GameSaveData.CurrentVersion;
            saveData.money = Mathf.Max(0, saveData.money);
            saveData.buildings ??= new List<BuildingSaveData>();
            saveData.taxes ??= new TaxAccrualSaveData();
            saveData.army ??= new ArmySaveData();
            saveData.recruitment ??= new RecruitmentSaveData();
            saveData.playerInventory ??= new PlayerInventorySaveData();
            saveData.exploration ??= new ExplorationSaveData();
            saveData.exploration.missions ??= new List<ExplorationMissionSaveData>();
            CampaignSaveDataUtility.Normalize(saveData);
            NavigationSaveDataUtility.Normalize(saveData);
            saveData.playerInventory.recruitedAdventurerIds ??=
                new List<string>();
            saveData.playerInventory.items ??=
                new List<InventoryItemSaveData>();

            var normalizedBuildings = new List<BuildingSaveData>();

            foreach (BuildingSaveData building in saveData.buildings)
            {
                if (building == null)
                {
                    continue;
                }

                BuildingSaveData existing = normalizedBuildings.Find(
                    candidate => candidate.type == building.type
                );

                if (existing == null)
                {
                    normalizedBuildings.Add(
                        new BuildingSaveData
                        {
                            type = building.type,
                            level = Mathf.Max(0, building.level)
                        }
                    );
                }
                else
                {
                    existing.level = Mathf.Max(0, building.level);
                }
            }

            saveData.buildings = normalizedBuildings;
            saveData.army.warriors = Mathf.Max(0, saveData.army.warriors);
            saveData.army.archers = Mathf.Max(0, saveData.army.archers);
            saveData.army.mages = Mathf.Max(0, saveData.army.mages);
            saveData.recruitment.availableWarriors = Mathf.Max(
                0,
                saveData.recruitment.availableWarriors
            );
            saveData.recruitment.availableArchers = Mathf.Max(
                0,
                saveData.recruitment.availableArchers
            );
            saveData.recruitment.availableMages = Mathf.Max(
                0,
                saveData.recruitment.availableMages
            );

            var normalizedAdventurerIds = new List<string>();

            foreach (string adventurerId in
                     saveData.playerInventory.recruitedAdventurerIds)
            {
                string normalizedId = adventurerId?.Trim();

                if (string.IsNullOrEmpty(normalizedId) ||
                    normalizedAdventurerIds.Contains(normalizedId))
                {
                    continue;
                }

                normalizedAdventurerIds.Add(normalizedId);
            }

            saveData.playerInventory.recruitedAdventurerIds =
                normalizedAdventurerIds;

            var normalizedItems = new List<InventoryItemSaveData>();

            foreach (InventoryItemSaveData item in
                     saveData.playerInventory.items)
            {
                string normalizedId = item?.itemId?.Trim();

                if (string.IsNullOrEmpty(normalizedId) ||
                    item.quantity <= 0)
                {
                    continue;
                }

                InventoryItemSaveData existing = normalizedItems.Find(
                    candidate => candidate.itemId == normalizedId
                );

                if (existing == null)
                {
                    normalizedItems.Add(
                        new InventoryItemSaveData
                        {
                            itemId = normalizedId,
                            quantity = item.quantity
                        }
                    );
                    continue;
                }

                existing.quantity = (int)Math.Min(
                    int.MaxValue,
                    (long)existing.quantity + item.quantity
                );
            }

            saveData.playerInventory.items = normalizedItems;
        }
    }
}
