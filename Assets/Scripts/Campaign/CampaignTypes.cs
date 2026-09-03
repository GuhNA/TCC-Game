using TCCGame.Core;

namespace TCCGame.Gameplay.Campaign
{
    public readonly struct IntRange
    {
        public int Minimum { get; }
        public int Maximum { get; }

        public IntRange(int minimum, int maximum)
        {
            if (minimum < 0 || maximum < minimum)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(minimum)
                );
            }

            Minimum = minimum;
            Maximum = maximum;
        }
    }

    public enum GateId
    {
        North,
        East,
        South,
        West
    }

    public enum EnemyType
    {
        Raider,
        Brute,
        Shaman
    }

    public enum WaveStatus
    {
        Scheduled,
        AwaitingResolution,
        Resolved
    }

    public enum WaveWarningStage
    {
        None,
        Announced,
        Imminent,
        Critical
    }

    public enum BattleOutcome
    {
        None,
        Repelled,
        Breached
    }

    public enum CampaignOutcome
    {
        Ongoing,
        Victory,
        Defeat
    }

    public enum TroopLocationKind
    {
        Reserve,
        Gate
    }

    public enum TroopDispatchStatus
    {
        Dispatched,
        InvalidQuantity,
        InvalidRoute,
        InsufficientTroops,
        CampaignEnded
    }

    public readonly struct TroopLocation
    {
        public TroopLocationKind Kind { get; }
        public GateId GateId { get; }

        private TroopLocation(TroopLocationKind kind, GateId gateId)
        {
            Kind = kind;
            GateId = gateId;
        }

        public static TroopLocation Reserve =>
            new(TroopLocationKind.Reserve, GateId.North);

        public static TroopLocation Gate(GateId gateId) =>
            new(TroopLocationKind.Gate, gateId);

        public bool Equals(TroopLocation other) =>
            Kind == other.Kind &&
            (Kind == TroopLocationKind.Reserve || GateId == other.GateId);
    }

    public readonly struct TroopTransferSnapshot
    {
        public int Sequence { get; }
        public TroopLocation Source { get; }
        public TroopLocation Destination { get; }
        public UnitType UnitType { get; }
        public int Quantity { get; }
        public double DepartureStrategicSeconds { get; }
        public double ArrivalStrategicSeconds { get; }

        public TroopTransferSnapshot(
            int sequence,
            TroopLocation source,
            TroopLocation destination,
            UnitType unitType,
            int quantity,
            double departureStrategicSeconds,
            double arrivalStrategicSeconds
        )
        {
            Sequence = sequence;
            Source = source;
            Destination = destination;
            UnitType = unitType;
            Quantity = quantity;
            DepartureStrategicSeconds = departureStrategicSeconds;
            ArrivalStrategicSeconds = arrivalStrategicSeconds;
        }
    }

    public readonly struct TroopDispatchResult
    {
        public TroopDispatchStatus Status { get; }
        public TroopTransferSnapshot? Transfer { get; }

        public bool Succeeded => Status == TroopDispatchStatus.Dispatched;

        public TroopDispatchResult(
            TroopDispatchStatus status,
            TroopTransferSnapshot? transfer = null
        )
        {
            Status = status;
            Transfer = transfer;
        }
    }

    public readonly struct GateStateSnapshot
    {
        public GateId GateId { get; }
        public int CurrentHealth { get; }
        public int Warriors { get; }
        public int Archers { get; }
        public int Mages { get; }
        public int LastResolvedWaveSequence { get; }

        public GateStateSnapshot(
            GateId gateId,
            int currentHealth,
            int warriors,
            int archers,
            int mages,
            int lastResolvedWaveSequence
        )
        {
            GateId = gateId;
            CurrentHealth = currentHealth;
            Warriors = warriors;
            Archers = archers;
            Mages = mages;
            LastResolvedWaveSequence = lastResolvedWaveSequence;
        }

        public int Get(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Warrior => Warriors,
                UnitType.Archer => Archers,
                UnitType.Mage => Mages,
                _ => 0
            };
        }
    }

    public readonly struct WaveSnapshot
    {
        public int Sequence { get; }
        public GateId GateId { get; }
        public double ArrivalStrategicSeconds { get; }
        public int ThreatPower { get; }
        public WaveStatus Status { get; }
        public int Raiders { get; }
        public int Brutes { get; }
        public int Shamans { get; }
        public WaveWarningStage WarningStage { get; }

        public WaveSnapshot(
            int sequence,
            GateId gateId,
            double arrivalStrategicSeconds,
            int threatPower,
            WaveStatus status,
            int raiders,
            int brutes,
            int shamans,
            WaveWarningStage warningStage
        )
        {
            Sequence = sequence;
            GateId = gateId;
            ArrivalStrategicSeconds = arrivalStrategicSeconds;
            ThreatPower = threatPower;
            Status = status;
            Raiders = raiders;
            Brutes = brutes;
            Shamans = shamans;
            WarningStage = warningStage;
        }

        public int GetEnemyCount(EnemyType enemyType)
        {
            return enemyType switch
            {
                EnemyType.Raider => Raiders,
                EnemyType.Brute => Brutes,
                EnemyType.Shaman => Shamans,
                _ => 0
            };
        }
    }

    public readonly struct BattleResolution
    {
        public BattleOutcome Outcome { get; }
        public double EffectiveDefense { get; }
        public double DefenseRatio { get; }
        public int InitialGateHealth { get; }
        public int GateDamage { get; }
        public int FinalGateHealth { get; }
        public int InitialWarriors { get; }
        public int LostWarriors { get; }
        public int InitialArchers { get; }
        public int LostArchers { get; }
        public int InitialMages { get; }
        public int LostMages { get; }
        public int Spoils { get; }

        public BattleResolution(
            BattleOutcome outcome,
            double effectiveDefense,
            double defenseRatio,
            int initialGateHealth,
            int gateDamage,
            int finalGateHealth,
            int initialWarriors,
            int lostWarriors,
            int initialArchers,
            int lostArchers,
            int initialMages,
            int lostMages,
            int spoils
        )
        {
            Outcome = outcome;
            EffectiveDefense = effectiveDefense;
            DefenseRatio = defenseRatio;
            InitialGateHealth = initialGateHealth;
            GateDamage = gateDamage;
            FinalGateHealth = finalGateHealth;
            InitialWarriors = initialWarriors;
            LostWarriors = lostWarriors;
            InitialArchers = initialArchers;
            LostArchers = lostArchers;
            InitialMages = initialMages;
            LostMages = lostMages;
            Spoils = spoils;
        }
    }

    public readonly struct BattleReportSnapshot
    {
        public int WaveSequence { get; }
        public GateId GateId { get; }
        public BattleOutcome Outcome { get; }
        public double EffectiveDefense { get; }
        public double DefenseRatio { get; }
        public int ThreatPower { get; }
        public int InitialGateHealth { get; }
        public int GateDamage { get; }
        public int FinalGateHealth { get; }
        public int InitialWarriors { get; }
        public int LostWarriors { get; }
        public int InitialArchers { get; }
        public int LostArchers { get; }
        public int InitialMages { get; }
        public int LostMages { get; }
        public int Spoils { get; }
        public bool ReportAcknowledged { get; }

        public int SurvivingWarriors => InitialWarriors - LostWarriors;
        public int SurvivingArchers => InitialArchers - LostArchers;
        public int SurvivingMages => InitialMages - LostMages;
        public long TotalLosses =>
            (long)LostWarriors + LostArchers + LostMages;

        public BattleReportSnapshot(
            int waveSequence,
            GateId gateId,
            BattleOutcome outcome,
            double effectiveDefense,
            double defenseRatio,
            int threatPower,
            int initialGateHealth,
            int gateDamage,
            int finalGateHealth,
            int initialWarriors,
            int lostWarriors,
            int initialArchers,
            int lostArchers,
            int initialMages,
            int lostMages,
            int spoils,
            bool reportAcknowledged
        )
        {
            WaveSequence = waveSequence;
            GateId = gateId;
            Outcome = outcome;
            EffectiveDefense = effectiveDefense;
            DefenseRatio = defenseRatio;
            ThreatPower = threatPower;
            InitialGateHealth = initialGateHealth;
            GateDamage = gateDamage;
            FinalGateHealth = finalGateHealth;
            InitialWarriors = initialWarriors;
            LostWarriors = lostWarriors;
            InitialArchers = initialArchers;
            LostArchers = lostArchers;
            InitialMages = initialMages;
            LostMages = lostMages;
            Spoils = spoils;
            ReportAcknowledged = reportAcknowledged;
        }
    }
}
