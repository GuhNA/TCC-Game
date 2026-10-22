using System;
using System.Collections.Generic;
using System.Linq;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Economy;

namespace TCCGame.Gameplay.Player
{
    public enum AdventurerRecruitmentStatus
    {
        Recruited,
        InvalidAdventurer,
        AlreadyRecruited,
        InsufficientMoney
    }

    public sealed class PlayerInventoryService
    {
        public const int MaximumAdventurerNameLength = 24;

        private readonly GameSaveData state;
        private readonly EconomyService economy;
        private readonly Action commit;

        public IReadOnlyList<string> RecruitedAdventurerIds =>
            state.playerInventory.recruitedAdventurerIds.AsReadOnly();

        public IReadOnlyList<RecruitedAdventurerSaveData>
            RecruitedAdventurers =>
                state.playerInventory.recruitedAdventurers.AsReadOnly();

        public PlayerInventoryService(
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
            state.playerInventory.recruitedAdventurerIds ??=
                new List<string>();
            state.playerInventory.recruitedAdventurers ??=
                new List<RecruitedAdventurerSaveData>();
            state.playerInventory.items ??= new List<InventoryItemSaveData>();
            EnsureAdventurerRecordsFromLegacyIds();
        }

        public bool IsAdventurerRecruited(string adventurerId)
        {
            string normalizedId = NormalizeId(adventurerId);

            return normalizedId != null &&
                state.playerInventory.recruitedAdventurers.Any(record =>
                    record != null && record.definitionId == normalizedId);
        }

        public AdventurerRecruitmentStatus TryRecruitAdventurer(
            string adventurerId,
            int price
        )
        {
            return TryRecruitAdventurer(
                adventurerId,
                GetFallbackClassName(adventurerId),
                null,
                price,
                out _
            );
        }

        public AdventurerRecruitmentStatus TryRecruitAdventurer(
            string adventurerId,
            string classDisplayName,
            string requestedName,
            int price,
            out RecruitedAdventurerSaveData recruitedAdventurer
        )
        {
            recruitedAdventurer = null;
            string normalizedId = NormalizeId(adventurerId);

            if (normalizedId == null || price < 0)
            {
                return AdventurerRecruitmentStatus.InvalidAdventurer;
            }

            if (!economy.CanAfford(price))
            {
                return AdventurerRecruitmentStatus.InsufficientMoney;
            }

            int recruitmentNumber = GetNextRecruitmentNumber(normalizedId);
            string defaultName = BuildDefaultName(
                classDisplayName,
                recruitmentNumber
            );
            string displayName = NormalizeDisplayName(requestedName) ??
                defaultName;
            string instanceId = BuildInstanceId(
                normalizedId,
                recruitmentNumber
            );
            while (state.playerInventory.recruitedAdventurers.Any(record =>
                       record != null && record.instanceId == instanceId))
            {
                recruitmentNumber++;
                instanceId = BuildInstanceId(
                    normalizedId,
                    recruitmentNumber
                );
                defaultName = BuildDefaultName(
                    classDisplayName,
                    recruitmentNumber
                );
                if (NormalizeDisplayName(requestedName) == null)
                {
                    displayName = defaultName;
                }
            }

            var record = new RecruitedAdventurerSaveData
            {
                instanceId = instanceId,
                definitionId = normalizedId,
                displayName = displayName,
                recruitmentNumber = recruitmentNumber
            };

            int previousMoney = economy.Money;

            try
            {
                economy.DebitWithoutCommit(price);
                state.playerInventory.recruitedAdventurerIds.Add(
                    normalizedId
                );
                state.playerInventory.recruitedAdventurers.Add(record);
                commit();
            }
            catch
            {
                economy.RestoreMoney(previousMoney);
                int lastLegacyIndex = state.playerInventory
                    .recruitedAdventurerIds.LastIndexOf(normalizedId);
                if (lastLegacyIndex >= 0)
                {
                    state.playerInventory.recruitedAdventurerIds.RemoveAt(
                        lastLegacyIndex
                    );
                }
                state.playerInventory.recruitedAdventurers.Remove(record);
                throw;
            }

            economy.PublishChange();
            recruitedAdventurer = record;
            return AdventurerRecruitmentStatus.Recruited;
        }

        public string GetNextDefaultAdventurerName(
            string adventurerId,
            string classDisplayName
        )
        {
            string normalizedId = NormalizeId(adventurerId);
            return normalizedId == null
                ? NormalizeDisplayName(classDisplayName) ?? "Aventureiro"
                : BuildDefaultName(
                    classDisplayName,
                    GetNextRecruitmentNumber(normalizedId)
                );
        }

        public bool TryRenameAdventurer(
            string instanceId,
            string newName,
            out string error
        )
        {
            string normalizedInstanceId = NormalizeId(instanceId);
            RecruitedAdventurerSaveData record =
                state.playerInventory.recruitedAdventurers.FirstOrDefault(
                    candidate => candidate != null &&
                        candidate.instanceId == normalizedInstanceId
                );
            if (record == null)
            {
                error = "Aventureiro não encontrado.";
                return false;
            }

            string normalizedName = NormalizeDisplayName(newName);
            if (normalizedName == null)
            {
                error = "Informe um nome para o aventureiro.";
                return false;
            }

            record.displayName = normalizedName;
            commit();
            error = null;
            return true;
        }

        public IReadOnlyList<RecruitedAdventurer> ResolveAdventurers(
            IEnumerable<Char_base> catalog
        )
        {
            Dictionary<string, Char_base> definitions = (catalog ??
                    Array.Empty<Char_base>())
                .Where(definition => definition != null)
                .GroupBy(definition => definition.PersistenceId)
                .ToDictionary(group => group.Key, group => group.First());

            return state.playerInventory.recruitedAdventurers
                .Where(record => record != null &&
                    definitions.ContainsKey(record.definitionId))
                .Select(record => new RecruitedAdventurer(
                    record,
                    definitions[record.definitionId]
                ))
                .ToList();
        }

        public int GetItemQuantity(string itemId)
        {
            string normalizedId = NormalizeId(itemId);

            if (normalizedId == null)
            {
                return 0;
            }

            InventoryItemSaveData item =
                state.playerInventory.items.Find(
                    candidate => candidate.itemId == normalizedId
                );

            return item?.quantity ?? 0;
        }

        public bool TryAddItem(string itemId, int quantity = 1)
        {
            string normalizedId = NormalizeId(itemId);

            if (normalizedId == null || quantity <= 0)
            {
                return false;
            }

            InventoryItemSaveData item =
                state.playerInventory.items.Find(
                    candidate => candidate.itemId == normalizedId
                );
            int previousQuantity = item?.quantity ?? 0;
            int newQuantity;

            try
            {
                newQuantity = checked(previousQuantity + quantity);
            }
            catch (OverflowException)
            {
                return false;
            }

            bool created = item == null;

            if (created)
            {
                item = new InventoryItemSaveData
                {
                    itemId = normalizedId
                };
                state.playerInventory.items.Add(item);
            }

            try
            {
                item.quantity = newQuantity;
                commit();
            }
            catch
            {
                if (created)
                {
                    state.playerInventory.items.Remove(item);
                }
                else
                {
                    item.quantity = previousQuantity;
                }

                throw;
            }

            return true;
        }

        private static string NormalizeId(string id)
        {
            string normalizedId = id?.Trim();
            return string.IsNullOrEmpty(normalizedId) ? null : normalizedId;
        }

        private void EnsureAdventurerRecordsFromLegacyIds()
        {
            foreach (IGrouping<string, string> group in
                     state.playerInventory.recruitedAdventurerIds
                         .Select(NormalizeId)
                         .Where(id => id != null)
                         .GroupBy(id => id))
            {
                int existing = state.playerInventory.recruitedAdventurers
                    .Count(record => record != null &&
                        record.definitionId == group.Key);
                for (int index = existing; index < group.Count(); index++)
                {
                    int number = GetNextRecruitmentNumber(group.Key);
                    state.playerInventory.recruitedAdventurers.Add(
                        new RecruitedAdventurerSaveData
                        {
                            instanceId = BuildInstanceId(group.Key, number),
                            definitionId = group.Key,
                            displayName = string.Empty,
                            recruitmentNumber = number
                        }
                    );
                }
            }

            state.playerInventory.recruitedAdventurerIds =
                state.playerInventory.recruitedAdventurers
                    .Where(record => record != null &&
                        NormalizeId(record.definitionId) != null)
                    .Select(record => record.definitionId.Trim())
                    .ToList();
        }

        private int GetNextRecruitmentNumber(string definitionId)
        {
            return state.playerInventory.recruitedAdventurers
                .Where(record => record != null &&
                    record.definitionId == definitionId)
                .Select(record => Math.Max(1, record.recruitmentNumber))
                .DefaultIfEmpty(0)
                .Max() + 1;
        }

        private static string BuildDefaultName(
            string classDisplayName,
            int recruitmentNumber
        )
        {
            string baseName = NormalizeDisplayName(classDisplayName) ??
                "Aventureiro";
            return recruitmentNumber <= 1
                ? baseName
                : baseName + " " + recruitmentNumber;
        }

        private static string BuildInstanceId(
            string definitionId,
            int recruitmentNumber
        )
        {
            return definitionId + ".recruit." + recruitmentNumber;
        }

        private static string NormalizeDisplayName(string value)
        {
            string normalized = value?.Trim();
            if (string.IsNullOrEmpty(normalized))
            {
                return null;
            }

            return normalized.Length <= MaximumAdventurerNameLength
                ? normalized
                : normalized.Substring(0, MaximumAdventurerNameLength);
        }

        private static string GetFallbackClassName(string adventurerId)
        {
            string normalized = NormalizeId(adventurerId);
            if (normalized == null)
            {
                return "Aventureiro";
            }

            int separator = normalized.LastIndexOf('.');
            string rawName = separator >= 0
                ? normalized.Substring(separator + 1)
                : normalized;
            return string.IsNullOrWhiteSpace(rawName)
                ? "Aventureiro"
                : char.ToUpperInvariant(rawName[0]) + rawName.Substring(1);
        }
    }

    public sealed class RecruitedAdventurer
    {
        public RecruitedAdventurerSaveData SaveData { get; }
        public Char_base Definition { get; }
        public string InstanceId => SaveData.instanceId;
        public string DefinitionId => SaveData.definitionId;
        public string DisplayName => string.IsNullOrWhiteSpace(
            SaveData.displayName)
                ? Definition.name
                : SaveData.displayName;

        public RecruitedAdventurer(
            RecruitedAdventurerSaveData saveData,
            Char_base definition
        )
        {
            SaveData = saveData ??
                throw new ArgumentNullException(nameof(saveData));
            Definition = definition ??
                throw new ArgumentNullException(nameof(definition));
        }
    }
}
