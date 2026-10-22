using System;
using System.Collections.Generic;
using System.Linq;
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
            saveData.tavernRecruitmentWave ??=
                new TavernRecruitmentWaveSaveData();
            saveData.tavernRecruitmentWave.adventurerIds ??=
                new List<string>();
            saveData.tavernRecruitmentWave.adventurerIds =
                saveData.tavernRecruitmentWave.adventurerIds
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim())
                    .ToList();
            saveData.tavernRecruitmentWave.sequence = Mathf.Max(
                0,
                saveData.tavernRecruitmentWave.sequence
            );
            saveData.tavernRecruitmentWave.expiresUnixSeconds = Math.Max(
                0,
                saveData.tavernRecruitmentWave.expiresUnixSeconds
            );
            saveData.exploration ??= new ExplorationSaveData();
            saveData.exploration.missions ??= new List<ExplorationMissionSaveData>();
            saveData.exploration.areas ??= new List<ExplorationAreaSaveData>();
            saveData.exploration.lostExplorers = Mathf.Max(
                0,
                saveData.exploration.lostExplorers
            );
            foreach (ExplorationMissionSaveData mission in
                     saveData.exploration.missions.Where(mission => mission != null))
            {
                mission.party ??= new List<ExplorationCombatantSaveData>();
                mission.rewards ??= new List<ExplorationRewardSaveData>();
            }
            foreach (ExplorationAreaSaveData area in
                     saveData.exploration.areas.Where(area => area != null))
            {
                area.guardParty ??= new List<ExplorationCombatantSaveData>();
                area.lastWorkerLoot ??=
                    new List<ExplorationRewardSaveData>();
                area.pendingWorkerLoot ??=
                    new List<ExplorationRewardSaveData>();
                area.assignedWorkers = Mathf.Max(0, area.assignedWorkers);
            }
            saveData.exploration.completedEnvironments ??=
                new List<ExplorationEnvironment>();
            saveData.exploration.completedEnvironments = saveData.exploration
                .completedEnvironments
                .Where(environment => Enum.IsDefined(
                    typeof(ExplorationEnvironment),
                    environment
                ))
                .Distinct()
                .ToList();
            CampaignSaveDataUtility.Normalize(saveData);
            NavigationSaveDataUtility.Normalize(saveData);
            saveData.playerInventory.recruitedAdventurerIds ??=
                new List<string>();
            saveData.playerInventory.recruitedAdventurers ??=
                new List<RecruitedAdventurerSaveData>();
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

            NormalizeRecruitedAdventurers(saveData);

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

        private static void NormalizeRecruitedAdventurers(
            GameSaveData saveData
        )
        {
            PlayerInventorySaveData inventory = saveData.playerInventory;
            List<string> legacyIds = inventory.recruitedAdventurerIds
                .Select(id => id?.Trim())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();
            var normalized = new List<RecruitedAdventurerSaveData>();
            var usedInstanceIds = new HashSet<string>();
            var nextNumberByDefinition = new Dictionary<string, int>();

            foreach (RecruitedAdventurerSaveData record in
                     inventory.recruitedAdventurers)
            {
                string definitionId = record?.definitionId?.Trim();
                if (string.IsNullOrEmpty(definitionId))
                {
                    continue;
                }

                int nextNumber = nextNumberByDefinition.TryGetValue(
                    definitionId,
                    out int knownNext
                ) ? knownNext : 1;
                int recruitmentNumber = Math.Max(
                    nextNumber,
                    record.recruitmentNumber
                );
                string instanceId = record.instanceId?.Trim();
                if (string.IsNullOrEmpty(instanceId) ||
                    !usedInstanceIds.Add(instanceId))
                {
                    instanceId = BuildAdventurerInstanceId(
                        definitionId,
                        recruitmentNumber
                    );
                    while (!usedInstanceIds.Add(instanceId))
                    {
                        recruitmentNumber++;
                        instanceId = BuildAdventurerInstanceId(
                            definitionId,
                            recruitmentNumber
                        );
                    }
                }

                normalized.Add(new RecruitedAdventurerSaveData
                {
                    instanceId = instanceId,
                    definitionId = definitionId,
                    displayName = record.displayName?.Trim() ?? string.Empty,
                    recruitmentNumber = recruitmentNumber
                });
                nextNumberByDefinition[definitionId] =
                    recruitmentNumber + 1;
            }

            // Cada ID dos saves antigos representava uma contratação. Criamos
            // uma instância correspondente sem apagar os registros novos.
            foreach (IGrouping<string, string> legacyGroup in
                     legacyIds.GroupBy(id => id))
            {
                int existingCount = normalized.Count(record =>
                    record.definitionId == legacyGroup.Key);
                int missingCount = legacyGroup.Count() - existingCount;
                for (int index = 0; index < missingCount; index++)
                {
                    int number = nextNumberByDefinition.TryGetValue(
                        legacyGroup.Key,
                        out int next
                    ) ? next : 1;
                    string instanceId = BuildAdventurerInstanceId(
                        legacyGroup.Key,
                        number
                    );
                    while (!usedInstanceIds.Add(instanceId))
                    {
                        number++;
                        instanceId = BuildAdventurerInstanceId(
                            legacyGroup.Key,
                            number
                        );
                    }

                    normalized.Add(new RecruitedAdventurerSaveData
                    {
                        instanceId = instanceId,
                        definitionId = legacyGroup.Key,
                        displayName = string.Empty,
                        recruitmentNumber = number
                    });
                    nextNumberByDefinition[legacyGroup.Key] = number + 1;
                }
            }

            inventory.recruitedAdventurers = normalized;
            inventory.recruitedAdventurerIds = normalized
                .Select(record => record.definitionId)
                .ToList();

            foreach (ExplorationMissionSaveData mission in
                     saveData.exploration.missions)
            {
                NormalizeCombatantReferences(mission?.party, normalized);
            }

            foreach (ExplorationAreaSaveData area in
                     saveData.exploration.areas)
            {
                NormalizeCombatantReferences(area?.guardParty, normalized);
            }
        }

        private static void NormalizeCombatantReferences(
            IEnumerable<ExplorationCombatantSaveData> combatants,
            IReadOnlyList<RecruitedAdventurerSaveData> adventurers
        )
        {
            if (combatants == null)
            {
                return;
            }

            foreach (ExplorationCombatantSaveData combatant in combatants)
            {
                if (combatant == null)
                {
                    continue;
                }

                RecruitedAdventurerSaveData byInstance = adventurers
                    .FirstOrDefault(record =>
                        record.instanceId == combatant.adventurerId);
                if (byInstance != null)
                {
                    combatant.definitionId = byInstance.definitionId;
                    continue;
                }

                string legacyDefinitionId = string.IsNullOrWhiteSpace(
                    combatant.definitionId)
                        ? combatant.adventurerId?.Trim()
                        : combatant.definitionId.Trim();
                RecruitedAdventurerSaveData migrated = adventurers
                    .FirstOrDefault(record =>
                        record.definitionId == legacyDefinitionId);
                if (migrated == null)
                {
                    combatant.definitionId = legacyDefinitionId;
                    continue;
                }

                combatant.adventurerId = migrated.instanceId;
                combatant.definitionId = migrated.definitionId;
                if (string.IsNullOrWhiteSpace(combatant.displayName))
                {
                    combatant.displayName = migrated.displayName;
                }
            }
        }

        private static string BuildAdventurerInstanceId(
            string definitionId,
            int recruitmentNumber
        )
        {
            return definitionId + ".recruit." + recruitmentNumber;
        }
    }
}
