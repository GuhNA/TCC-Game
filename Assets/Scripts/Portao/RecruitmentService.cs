using System;
using UnityEngine;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using TCCGame.Gameplay.City;
using TCCGame.Gameplay.Economy;

namespace TCCGame.Gameplay.Recruitment
{
    public enum RecruitmentPurchaseStatus
    {
        Purchased,
        InvalidQuantity,
        OutOfStock,
        InsufficientMoney
    }

    public readonly struct RecruitmentPurchaseResult
    {
        public RecruitmentPurchaseStatus Status { get; }
        public int TotalCost { get; }
        public int Available { get; }

        public RecruitmentPurchaseResult(
            RecruitmentPurchaseStatus status,
            int totalCost,
            int available
        )
        {
            Status = status;
            TotalCost = totalCost;
            Available = available;
        }
    }

    public sealed class RecruitmentService
    {
        public const int BaseStock = 10;
        public const long RestockIntervalSeconds = 6L * 60L * 60L;

        private readonly GameSaveData state;
        private readonly EconomyService economy;
        private readonly BuildingService buildings;
        private readonly IClock clock;
        private readonly Action commit;

        public RecruitmentService(
            GameSaveData state,
            EconomyService economy,
            BuildingService buildings,
            IClock clock,
            Action commit
        )
        {
            this.state = state ??
                throw new ArgumentNullException(nameof(state));
            this.economy = economy ??
                throw new ArgumentNullException(nameof(economy));
            this.buildings = buildings ??
                throw new ArgumentNullException(nameof(buildings));
            this.clock = clock ??
                throw new ArgumentNullException(nameof(clock));
            this.commit = commit ??
                throw new ArgumentNullException(nameof(commit));

            InitializeIfNeeded();
        }

        public int GetAvailable(UnitType unitType)
        {
            return state.recruitment.GetAvailable(unitType);
        }

        public int GetOwned(UnitType unitType)
        {
            return state.army.Get(unitType);
        }

        public int GetUnitPrice(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Archer => 5,
                UnitType.Warrior => 3,
                UnitType.Mage => 8,
                _ => throw new ArgumentOutOfRangeException(nameof(unitType))
            };
        }

        public TimeSpan GetTimeUntilRestock()
        {
            long remainingSeconds = Math.Max(
                0L,
                state.recruitment.nextRestockUnixSeconds -
                clock.UnixSeconds
            );

            return TimeSpan.FromSeconds(remainingSeconds);
        }

        public bool RefreshStockIfDue()
        {
            if (clock.UnixSeconds <
                state.recruitment.nextRestockUnixSeconds)
            {
                return false;
            }

            foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
            {
                int buildingLevel = buildings.GetLevel(
                    GetRequiredBuilding(unitType)
                );

                state.recruitment.SetAvailable(
                    unitType,
                    CalculateCapacity(buildingLevel)
                );
            }

            state.recruitment.nextRestockUnixSeconds =
                clock.UnixSeconds + RestockIntervalSeconds;
            commit();

            return true;
        }

        public RecruitmentPurchaseResult TryPurchase(
            UnitType unitType,
            int quantity
        )
        {
            int available = GetAvailable(unitType);
            int owned = GetOwned(unitType);

            if (quantity <= 0)
            {
                return CreateResult(
                    RecruitmentPurchaseStatus.InvalidQuantity,
                    unitType,
                    0
                );
            }

            int totalCost;

            try
            {
                totalCost = checked(GetUnitPrice(unitType) * quantity);
            }
            catch (OverflowException)
            {
                return CreateResult(
                    RecruitmentPurchaseStatus.InvalidQuantity,
                    unitType,
                    0
                );
            }

            if (available < quantity)
            {
                return CreateResult(
                    RecruitmentPurchaseStatus.OutOfStock,
                    unitType,
                    totalCost
                );
            }

            if (!economy.CanAfford(totalCost))
            {
                return CreateResult(
                    RecruitmentPurchaseStatus.InsufficientMoney,
                    unitType,
                    totalCost
                );
            }

            int newOwned;

            try
            {
                newOwned = checked(owned + quantity);
            }
            catch (OverflowException)
            {
                return CreateResult(
                    RecruitmentPurchaseStatus.InvalidQuantity,
                    unitType,
                    totalCost
                );
            }

            int previousMoney = economy.Money;

            try
            {
                economy.DebitWithoutCommit(totalCost);
                state.recruitment.SetAvailable(
                    unitType,
                    available - quantity
                );
                state.army.Set(unitType, newOwned);
                commit();
            }
            catch
            {
                economy.RestoreMoney(previousMoney);
                state.recruitment.SetAvailable(unitType, available);
                state.army.Set(unitType, owned);
                throw;
            }

            economy.PublishChange();

            return CreateResult(
                RecruitmentPurchaseStatus.Purchased,
                unitType,
                totalCost
            );
        }

        public static int CalculateCapacity(int buildingLevel)
        {
            return Mathf.FloorToInt(
                BaseStock * (1f + 0.30f * Mathf.Max(0, buildingLevel))
            );
        }

        private void InitializeIfNeeded()
        {
            if (state.recruitment.initialized)
            {
                return;
            }

            state.recruitment.initialized = true;
            state.recruitment.availableArchers = BaseStock;
            state.recruitment.availableWarriors = BaseStock;
            state.recruitment.availableMages = BaseStock;
            state.recruitment.nextRestockUnixSeconds =
                clock.UnixSeconds + RestockIntervalSeconds;
            commit();
        }

        private RecruitmentPurchaseResult CreateResult(
            RecruitmentPurchaseStatus status,
            UnitType unitType,
            int totalCost
        )
        {
            return new RecruitmentPurchaseResult(
                status,
                totalCost,
                GetAvailable(unitType)
            );
        }

        private static BuildingType GetRequiredBuilding(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Archer => BuildingType.ArcherStand,
                UnitType.Warrior => BuildingType.Barracks,
                UnitType.Mage => BuildingType.MagicSchool,
                _ => throw new ArgumentOutOfRangeException(nameof(unitType))
            };
        }
    }
}
