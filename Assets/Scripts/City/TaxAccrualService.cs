using System;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Economy;
using UnityEngine;

public enum TaxCollectionStatus
{
    Collected,
    MarketNotBuilt,
    NothingAvailable
}

public readonly struct TaxCollectionResult
{
    public TaxCollectionStatus Status { get; }
    public int Amount { get; }
    public int TotalMoney { get; }

    public TaxCollectionResult(
        TaxCollectionStatus status,
        int amount,
        int totalMoney
    )
    {
        Status = status;
        Amount = amount;
        TotalMoney = totalMoney;
    }
}

public class TaxAccrualService : MonoBehaviour
{
    private const double MaximumAccrualSeconds = 24d * 60d * 60d;

    [SerializeField]
    private BuildingBehaviour localMarket;

    private TaxAccrualSaveData state;
    private EconomyService economy;

    private bool isInitialized;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        if (localMarket == null)
        {
            Debug.LogError(
                "TaxAccrualService requires a LocalMarket reference.",
                this
            );

            return;
        }

        GameSession session = GameSessionProvider.Current;
        economy = session.Economy;
        state = session.Taxes;

        long now = GetCurrentUnixSeconds();

        if (state == null || !state.initialized)
        {
            state = new TaxAccrualSaveData
            {
                initialized = true,
                lastCalculatedUnixSeconds = now,
                lastCollectedUnixSeconds = now
            };

            GameSessionProvider.Current.SaveTaxes(state);
        }

        localMarket.LevelChanged += HandleMarketLevelChanged;
        isInitialized = true;
    }

    internal TaxCollectionResult CollectTaxes()
    {
        if (!isInitialized || localMarket.State.level <= 0)
        {
            return new TaxCollectionResult(
                TaxCollectionStatus.MarketNotBuilt,
                0,
                economy?.Money ?? 0
            );
        }

        long now = GetCurrentUnixSeconds();
        AccrueUntil(now, localMarket.State.level);

        int amount = (int)Math.Floor(state.accruedTax + 0.0000001d);

        if (amount <= 0)
        {
            GameSessionProvider.Current.SaveTaxes(state);

            return new TaxCollectionResult(
                TaxCollectionStatus.NothingAvailable,
                0,
                economy.Money
            );
        }

        state.accruedTax -= amount;
        state.eligibleSecondsSinceCollection = 0d;
        state.lastCollectedUnixSeconds = now;

        economy.AddMoney(amount);

        return new TaxCollectionResult(
            TaxCollectionStatus.Collected,
            amount,
            economy.Money
        );
    }

    private void HandleMarketLevelChanged(
        BuildingBehaviour building,
        int previousLevel,
        int newLevel
    )
    {
        long now = GetCurrentUnixSeconds();
        AccrueUntil(now, previousLevel);

        if (previousLevel <= 0 && newLevel > 0)
        {
            state.lastCollectedUnixSeconds = now;
            state.eligibleSecondsSinceCollection = 0d;
            state.accruedTax = 0d;
        }

        GameSessionProvider.Current.SaveTaxes(state);
    }

    private void AccrueUntil(long now, int marketLevel)
    {
        if (now < state.lastCalculatedUnixSeconds)
        {
            state.lastCalculatedUnixSeconds = now;
            return;
        }

        double elapsedSeconds = now - state.lastCalculatedUnixSeconds;
        state.lastCalculatedUnixSeconds = now;

        int productionPerMinute = BuildingCalculator.GetProduction(
            localMarket.Definition,
            marketLevel
        );

        if (productionPerMinute <= 0)
        {
            return;
        }

        double eligibleSeconds = TaxCalculator.GetEligibleSeconds(
            elapsedSeconds,
            state.eligibleSecondsSinceCollection,
            MaximumAccrualSeconds
        );

        state.accruedTax += TaxCalculator.CalculateAccruedTax(
            productionPerMinute,
            eligibleSeconds
        );

        state.eligibleSecondsSinceCollection += eligibleSeconds;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCurrentAccrual();
        }
    }

    private void OnDestroy()
    {
        if (localMarket != null)
        {
            localMarket.LevelChanged -= HandleMarketLevelChanged;
        }

        SaveCurrentAccrual();
    }

    private void SaveCurrentAccrual()
    {
        if (!isInitialized || localMarket == null)
        {
            return;
        }

        AccrueUntil(GetCurrentUnixSeconds(), localMarket.State.level);
        GameSessionProvider.Current.SaveTaxes(state);
    }

    private static long GetCurrentUnixSeconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
