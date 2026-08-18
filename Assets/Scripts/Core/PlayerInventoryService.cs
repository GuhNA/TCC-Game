using System;
using System.Collections.Generic;
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
        private readonly GameSaveData state;
        private readonly EconomyService economy;
        private readonly Action commit;

        public IReadOnlyList<string> RecruitedAdventurerIds =>
            state.playerInventory.recruitedAdventurerIds.AsReadOnly();

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
        }

        public bool IsAdventurerRecruited(string adventurerId)
        {
            string normalizedId = NormalizeId(adventurerId);

            return normalizedId != null &&
                state.playerInventory.recruitedAdventurerIds.Contains(
                    normalizedId
                );
        }

        public AdventurerRecruitmentStatus TryRecruitAdventurer(
            string adventurerId,
            int price
        )
        {
            string normalizedId = NormalizeId(adventurerId);

            if (normalizedId == null || price < 0)
            {
                return AdventurerRecruitmentStatus.InvalidAdventurer;
            }

            if (IsAdventurerRecruited(normalizedId))
            {
                return AdventurerRecruitmentStatus.AlreadyRecruited;
            }

            if (!economy.CanAfford(price))
            {
                return AdventurerRecruitmentStatus.InsufficientMoney;
            }

            int previousMoney = economy.Money;

            try
            {
                economy.DebitWithoutCommit(price);
                state.playerInventory.recruitedAdventurerIds.Add(
                    normalizedId
                );
                commit();
            }
            catch
            {
                economy.RestoreMoney(previousMoney);
                state.playerInventory.recruitedAdventurerIds.Remove(
                    normalizedId
                );
                throw;
            }

            economy.PublishChange();
            return AdventurerRecruitmentStatus.Recruited;
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
    }
}
