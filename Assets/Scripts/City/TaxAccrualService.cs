using System;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using TCCGame.Gameplay.City;
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

public sealed class TaxService
{
    private const double MaximumAccrualSeconds = 24d * 60d * 60d;

    private readonly GameSaveData state;
    private readonly EconomyService economy;
    private readonly BuildingService buildings;
    private readonly IClock clock;
    private readonly Action commit;

    public TaxService(
        GameSaveData state,
        EconomyService economy,
        BuildingService buildings,
        IClock clock,
        Action commit
    )
    {
        this.state = state ?? throw new ArgumentNullException(nameof(state));
        this.economy = economy ??
            throw new ArgumentNullException(nameof(economy));
        this.buildings = buildings ??
            throw new ArgumentNullException(nameof(buildings));
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
        this.commit = commit ?? throw new ArgumentNullException(nameof(commit));

        InitializeIfNeeded();
        buildings.LevelChanged += HandleBuildingLevelChanged;
    }

    public TaxCollectionResult CollectTaxes()
    {
        int marketLevel = GetMarketLevel();

        if (marketLevel <= 0)
        {
            return new TaxCollectionResult(
                TaxCollectionStatus.MarketNotBuilt,
                0,
                economy.Money
            );
        }

        AccrueUntil(clock.UnixSeconds, marketLevel);

        int amount = (int)Math.Floor(state.taxes.accruedTax + 0.0000001d);

        if (amount <= 0)
        {
            commit();
            return new TaxCollectionResult(
                TaxCollectionStatus.NothingAvailable,
                0,
                economy.Money
            );
        }

        state.taxes.accruedTax -= amount;
        state.taxes.eligibleSecondsSinceCollection = 0d;
        state.taxes.lastCollectedUnixSeconds = clock.UnixSeconds;
        economy.AddMoney(amount);

        return new TaxCollectionResult(
            TaxCollectionStatus.Collected,
            amount,
            economy.Money
        );
    }

    public void SaveCurrentAccrual()
    {
        AccrueUntil(clock.UnixSeconds, GetMarketLevel());
        commit();
    }

    private void InitializeIfNeeded()
    {
        if (state.taxes.initialized)
        {
            return;
        }

        long now = clock.UnixSeconds;
        state.taxes.initialized = true;
        state.taxes.lastCalculatedUnixSeconds = now;
        state.taxes.lastCollectedUnixSeconds = now;
    }

    private void HandleBuildingLevelChanged(
        BuildingType type,
        int previousLevel,
        int newLevel
    )
    {
        if (type != BuildingType.LocalMarket)
        {
            return;
        }

        long now = clock.UnixSeconds;
        AccrueUntil(now, previousLevel);

        if (previousLevel <= 0 && newLevel > 0)
        {
            state.taxes.lastCollectedUnixSeconds = now;
            state.taxes.eligibleSecondsSinceCollection = 0d;
            state.taxes.accruedTax = 0d;
        }

        commit();
    }

    private void AccrueUntil(long now, int marketLevel)
    {
        if (now < state.taxes.lastCalculatedUnixSeconds)
        {
            state.taxes.lastCalculatedUnixSeconds = now;
            return;
        }

        double elapsedSeconds = now - state.taxes.lastCalculatedUnixSeconds;
        state.taxes.lastCalculatedUnixSeconds = now;

        int productionPerMinute = GetProduction(marketLevel);

        if (productionPerMinute <= 0)
        {
            return;
        }

        double eligibleSeconds = TaxCalculator.GetEligibleSeconds(
            elapsedSeconds,
            state.taxes.eligibleSecondsSinceCollection,
            MaximumAccrualSeconds
        );

        state.taxes.accruedTax += TaxCalculator.CalculateAccruedTax(
            productionPerMinute,
            eligibleSeconds
        );
        state.taxes.eligibleSecondsSinceCollection += eligibleSeconds;
    }

    private int GetMarketLevel()
    {
        return buildings.GetLevel(BuildingType.LocalMarket);
    }

    private int GetProduction(int marketLevel)
    {
        return marketLevel <= 0
            ? 0
            : buildings.GetProduction(BuildingType.LocalMarket, marketLevel);
    }
}

public class TaxAccrualService : MonoBehaviour
{
    [SerializeField]
    private BuildingBehaviour localMarket;

    private TaxService service;

    private void Start()
    {
        if (localMarket == null ||
            localMarket.Definition == null ||
            localMarket.Definition.type != BuildingType.LocalMarket)
        {
            Debug.LogError(
                "TaxAccrualService requires a LocalMarket reference.",
                this
            );
            return;
        }

        service = GameSessionProvider.Current.Taxes;
    }

    internal TaxCollectionResult CollectTaxes()
    {
        return service == null
            ? new TaxCollectionResult(
                TaxCollectionStatus.MarketNotBuilt,
                0,
                GameSessionProvider.Current.Economy.Money
            )
            : service.CollectTaxes();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            service?.SaveCurrentAccrual();
        }
    }

    private void OnDestroy()
    {
        service?.SaveCurrentAccrual();
    }
}
