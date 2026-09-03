using System;
using System.Collections.Generic;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Core.Persistence
{
    [Serializable]
    public sealed class GateSaveData
    {
        public const int DefaultHealth = 100;

        public GateId gateId;
        public int currentHealth = DefaultHealth;
        public int warriors;
        public int archers;
        public int mages;
        public int lastResolvedWaveSequence;

        public GateStateSnapshot ToSnapshot()
        {
            return new GateStateSnapshot(
                gateId,
                currentHealth,
                warriors,
                archers,
                mages,
                lastResolvedWaveSequence
            );
        }

        public int Get(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Warrior => warriors,
                UnitType.Archer => archers,
                UnitType.Mage => mages,
                _ => 0
            };
        }

        public void Set(UnitType unitType, int value)
        {
            switch (unitType)
            {
                case UnitType.Warrior:
                    warriors = value;
                    break;
                case UnitType.Archer:
                    archers = value;
                    break;
                case UnitType.Mage:
                    mages = value;
                    break;
            }
        }
    }

    [Serializable]
    public sealed class BattleReportSaveData
    {
        public bool resolved;
        public BattleOutcome outcome;
        public double effectiveDefense;
        public double defenseRatio;
        public int initialGateHealth;
        public int gateDamage;
        public int finalGateHealth;
        public int initialWarriors;
        public int lostWarriors;
        public int initialArchers;
        public int lostArchers;
        public int initialMages;
        public int lostMages;
        public int spoils;
        public bool reportAcknowledged;

        public BattleReportSnapshot ToSnapshot(WaveSaveData wave)
        {
            return new BattleReportSnapshot(
                wave.sequence,
                wave.gateId,
                outcome,
                effectiveDefense,
                defenseRatio,
                wave.threatPower,
                initialGateHealth,
                gateDamage,
                finalGateHealth,
                initialWarriors,
                lostWarriors,
                initialArchers,
                lostArchers,
                initialMages,
                lostMages,
                spoils,
                reportAcknowledged
            );
        }
    }

    [Serializable]
    public sealed class WaveSaveData
    {
        public int sequence;
        public GateId gateId;
        public double arrivalStrategicSeconds;
        public int threatPower;
        public WaveStatus status;
        public int raiders;
        public int brutes;
        public int shamans;
        public WaveWarningStage warningStage;
        public BattleReportSaveData battleReport = new();

        public WaveSnapshot ToSnapshot()
        {
            return new WaveSnapshot(
                sequence,
                gateId,
                arrivalStrategicSeconds,
                threatPower,
                status,
                raiders,
                brutes,
                shamans,
                warningStage
            );
        }
    }

    [Serializable]
    public sealed class TroopTransferSaveData
    {
        public int sequence;
        public TroopLocationKind sourceKind;
        public GateId sourceGateId;
        public TroopLocationKind destinationKind;
        public GateId destinationGateId;
        public UnitType unitType;
        public int quantity;
        public double departureStrategicSeconds;
        public double arrivalStrategicSeconds;

        public TroopTransferSnapshot ToSnapshot()
        {
            TroopLocation source = sourceKind == TroopLocationKind.Reserve
                ? TroopLocation.Reserve
                : TroopLocation.Gate(sourceGateId);
            TroopLocation destination = destinationKind ==
                TroopLocationKind.Reserve
                ? TroopLocation.Reserve
                : TroopLocation.Gate(destinationGateId);

            return new TroopTransferSnapshot(
                sequence,
                source,
                destination,
                unitType,
                quantity,
                departureStrategicSeconds,
                arrivalStrategicSeconds
            );
        }
    }

    [Serializable]
    public sealed class CampaignSaveData
    {
        public bool initialized = true;
        public double elapsedStrategicSeconds;
        public int randomSeed = CampaignSaveDataUtility.CreateSeed();
        public int nextWaveSequence = 1;
        public int nextTroopTransferSequence = 1;
        public CampaignOutcome outcome;
        public List<GateSaveData> gates = new();
        public List<WaveSaveData> scheduledWaves = new();
        public List<TroopTransferSaveData> troopTransfers = new();
    }

    public static class CampaignSaveDataUtility
    {
        public static int CreateSeed()
        {
            int seed = Guid.NewGuid().GetHashCode();
            return seed == 0 ? 1 : seed;
        }

        public static bool Normalize(GameSaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            bool changed = false;

            if (saveData.campaign == null)
            {
                saveData.campaign = new CampaignSaveData();
                changed = true;
            }

            CampaignSaveData campaign = saveData.campaign;

            if (!campaign.initialized)
            {
                campaign.initialized = true;
                changed = true;
            }

            if (!IsFinite(campaign.elapsedStrategicSeconds) ||
                campaign.elapsedStrategicSeconds < 0d)
            {
                campaign.elapsedStrategicSeconds = 0d;
                changed = true;
            }

            if (campaign.randomSeed == 0)
            {
                campaign.randomSeed = CreateSeed();
                changed = true;
            }

            if (!Enum.IsDefined(typeof(CampaignOutcome), campaign.outcome))
            {
                campaign.outcome = CampaignOutcome.Ongoing;
                changed = true;
            }

            campaign.gates ??= new List<GateSaveData>();
            campaign.scheduledWaves ??= new List<WaveSaveData>();
            campaign.troopTransfers ??= new List<TroopTransferSaveData>();

            var normalizedGates = new List<GateSaveData>();

            foreach (GateId gateId in Enum.GetValues(typeof(GateId)))
            {
                GateSaveData source = null;

                foreach (GateSaveData candidate in campaign.gates)
                {
                    if (candidate != null && candidate.gateId == gateId)
                    {
                        source = candidate;
                    }
                }

                var normalized = new GateSaveData { gateId = gateId };

                if (source != null)
                {
                    normalized.currentHealth = Math.Clamp(
                        source.currentHealth,
                        0,
                        GateSaveData.DefaultHealth
                    );
                    normalized.warriors = Math.Max(0, source.warriors);
                    normalized.archers = Math.Max(0, source.archers);
                    normalized.mages = Math.Max(0, source.mages);
                    normalized.lastResolvedWaveSequence = Math.Max(
                        0,
                        source.lastResolvedWaveSequence
                    );
                }

                normalizedGates.Add(normalized);
            }

            if (!GateListsEqual(campaign.gates, normalizedGates))
            {
                campaign.gates = normalizedGates;
                changed = true;
            }

            var normalizedWaves = new List<WaveSaveData>();

            foreach (WaveSaveData wave in campaign.scheduledWaves)
            {
                if (!IsValid(wave))
                {
                    changed = true;
                    continue;
                }

                WaveSaveData existing = normalizedWaves.Find(
                    candidate => candidate.sequence == wave.sequence
                );

                if (existing != null)
                {
                    normalizedWaves.Remove(existing);
                    changed = true;
                }

                normalizedWaves.Add(CopyAndClamp(wave));
            }

            normalizedWaves.Sort(
                (left, right) => left.sequence.CompareTo(right.sequence)
            );

            if (!WaveListsEqual(campaign.scheduledWaves, normalizedWaves))
            {
                campaign.scheduledWaves = normalizedWaves;
                changed = true;
            }

            bool hasBreachedGate = normalizedGates.Exists(
                gate => gate.currentHealth == 0
            );
            bool hasFinalVictory = normalizedWaves.Exists(
                wave => wave.sequence == CampaignRules.MaximumWaves &&
                    wave.status == WaveStatus.Resolved &&
                    wave.battleReport != null &&
                    wave.battleReport.resolved &&
                    wave.battleReport.outcome == BattleOutcome.Repelled
            );
            CampaignOutcome normalizedOutcome = hasBreachedGate
                ? CampaignOutcome.Defeat
                : hasFinalVictory
                    ? CampaignOutcome.Victory
                    : CampaignOutcome.Ongoing;
            if (campaign.outcome != normalizedOutcome)
            {
                campaign.outcome = normalizedOutcome;
                changed = true;
            }

            int minimumNextSequence = 1;

            foreach (WaveSaveData wave in campaign.scheduledWaves)
            {
                int nextSequence = wave.sequence == int.MaxValue
                    ? int.MaxValue
                    : wave.sequence + 1;
                minimumNextSequence = Math.Max(
                    minimumNextSequence,
                    nextSequence
                );
            }

            if (campaign.nextWaveSequence < minimumNextSequence)
            {
                campaign.nextWaveSequence = minimumNextSequence;
                changed = true;
            }

            var normalizedTransfers = new List<TroopTransferSaveData>();

            foreach (TroopTransferSaveData transfer in campaign.troopTransfers)
            {
                if (!IsValid(transfer))
                {
                    changed = true;
                    continue;
                }

                TroopTransferSaveData existing = normalizedTransfers.Find(
                    candidate => candidate.sequence == transfer.sequence
                );
                if (existing != null)
                {
                    normalizedTransfers.Remove(existing);
                    changed = true;
                }

                normalizedTransfers.Add(Copy(transfer));
            }

            normalizedTransfers.Sort(
                (left, right) => left.sequence.CompareTo(right.sequence)
            );
            if (!TransferListsEqual(campaign.troopTransfers, normalizedTransfers))
            {
                campaign.troopTransfers = normalizedTransfers;
                changed = true;
            }

            int minimumNextTransferSequence = 1;
            foreach (TroopTransferSaveData transfer in campaign.troopTransfers)
            {
                int nextSequence = transfer.sequence == int.MaxValue
                    ? int.MaxValue
                    : transfer.sequence + 1;
                minimumNextTransferSequence = Math.Max(
                    minimumNextTransferSequence,
                    nextSequence
                );
            }

            if (campaign.nextTroopTransferSequence < minimumNextTransferSequence)
            {
                campaign.nextTroopTransferSequence = minimumNextTransferSequence;
                changed = true;
            }

            return changed;
        }

        private static bool IsValid(TroopTransferSaveData transfer)
        {
            if (transfer == null || transfer.sequence <= 0 ||
                !Enum.IsDefined(typeof(TroopLocationKind), transfer.sourceKind) ||
                !Enum.IsDefined(typeof(TroopLocationKind), transfer.destinationKind) ||
                !Enum.IsDefined(typeof(UnitType), transfer.unitType) ||
                transfer.quantity <= 0 ||
                !IsFinite(transfer.departureStrategicSeconds) ||
                !IsFinite(transfer.arrivalStrategicSeconds) ||
                transfer.departureStrategicSeconds < 0d ||
                transfer.arrivalStrategicSeconds < transfer.departureStrategicSeconds)
            {
                return false;
            }

            if (transfer.sourceKind == TroopLocationKind.Gate &&
                !Enum.IsDefined(typeof(GateId), transfer.sourceGateId))
            {
                return false;
            }

            if (transfer.destinationKind == TroopLocationKind.Gate &&
                !Enum.IsDefined(typeof(GateId), transfer.destinationGateId))
            {
                return false;
            }

            return transfer.sourceKind != transfer.destinationKind ||
                   transfer.sourceKind != TroopLocationKind.Reserve &&
                   transfer.sourceGateId != transfer.destinationGateId;
        }

        private static TroopTransferSaveData Copy(TroopTransferSaveData source)
        {
            return new TroopTransferSaveData
            {
                sequence = source.sequence,
                sourceKind = source.sourceKind,
                sourceGateId = source.sourceGateId,
                destinationKind = source.destinationKind,
                destinationGateId = source.destinationGateId,
                unitType = source.unitType,
                quantity = source.quantity,
                departureStrategicSeconds = source.departureStrategicSeconds,
                arrivalStrategicSeconds = source.arrivalStrategicSeconds
            };
        }

        private static bool IsValid(WaveSaveData wave)
        {
            return wave != null &&
                   wave.sequence > 0 &&
                   Enum.IsDefined(typeof(GateId), wave.gateId) &&
                   IsFinite(wave.arrivalStrategicSeconds) &&
                   wave.arrivalStrategicSeconds >= 0d &&
                   wave.threatPower > 0 &&
                   Enum.IsDefined(typeof(WaveStatus), wave.status) &&
                   Enum.IsDefined(
                       typeof(WaveWarningStage),
                       wave.warningStage
                   ) &&
                   (wave.raiders > 0 ||
                    wave.brutes > 0 ||
                    wave.shamans > 0);
        }

        private static WaveSaveData CopyAndClamp(WaveSaveData source)
        {
            BattleReportSaveData report = NormalizeReport(source);
            return new WaveSaveData
            {
                sequence = source.sequence,
                gateId = source.gateId,
                arrivalStrategicSeconds = source.arrivalStrategicSeconds,
                threatPower = source.threatPower,
                status = report.resolved
                    ? WaveStatus.Resolved
                    : source.status,
                raiders = Math.Max(0, source.raiders),
                brutes = Math.Max(0, source.brutes),
                shamans = Math.Max(0, source.shamans),
                warningStage = source.warningStage,
                battleReport = report
            };
        }

        private static BattleReportSaveData NormalizeReport(
            WaveSaveData source
        )
        {
            BattleReportSaveData report = source.battleReport;
            if (report == null || !report.resolved)
            {
                if (source.status == WaveStatus.Resolved)
                {
                    return new BattleReportSaveData
                    {
                        resolved = true,
                        outcome = BattleOutcome.None,
                        reportAcknowledged = true
                    };
                }

                return new BattleReportSaveData();
            }

            BattleOutcome outcome = Enum.IsDefined(
                typeof(BattleOutcome),
                report.outcome
            ) ? report.outcome : BattleOutcome.None;
            int initialWarriors = Math.Max(0, report.initialWarriors);
            int initialArchers = Math.Max(0, report.initialArchers);
            int initialMages = Math.Max(0, report.initialMages);
            int initialGateHealth = Math.Clamp(
                report.initialGateHealth,
                0,
                GateSaveData.DefaultHealth
            );
            int gateDamage = Math.Clamp(
                report.gateDamage,
                0,
                initialGateHealth
            );

            return new BattleReportSaveData
            {
                resolved = true,
                outcome = outcome,
                effectiveDefense = IsFinite(report.effectiveDefense)
                    ? Math.Max(0d, report.effectiveDefense)
                    : 0d,
                defenseRatio = IsFinite(report.defenseRatio)
                    ? Math.Max(0d, report.defenseRatio)
                    : 0d,
                initialGateHealth = initialGateHealth,
                gateDamage = gateDamage,
                finalGateHealth = initialGateHealth - gateDamage,
                initialWarriors = initialWarriors,
                lostWarriors = Math.Clamp(
                    report.lostWarriors,
                    0,
                    initialWarriors
                ),
                initialArchers = initialArchers,
                lostArchers = Math.Clamp(
                    report.lostArchers,
                    0,
                    initialArchers
                ),
                initialMages = initialMages,
                lostMages = Math.Clamp(
                    report.lostMages,
                    0,
                    initialMages
                ),
                spoils = Math.Max(0, report.spoils),
                reportAcknowledged = report.reportAcknowledged
            };
        }

        private static bool GateListsEqual(
            List<GateSaveData> left,
            List<GateSaveData> right
        )
        {
            if (left == null || left.Count != right.Count)
            {
                return false;
            }

            for (int index = 0; index < left.Count; index++)
            {
                GateSaveData a = left[index];
                GateSaveData b = right[index];

                if (a == null ||
                    a.gateId != b.gateId ||
                    a.currentHealth != b.currentHealth ||
                    a.warriors != b.warriors ||
                    a.archers != b.archers ||
                    a.mages != b.mages ||
                    a.lastResolvedWaveSequence !=
                        b.lastResolvedWaveSequence)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool WaveListsEqual(
            List<WaveSaveData> left,
            List<WaveSaveData> right
        )
        {
            if (left == null || left.Count != right.Count)
            {
                return false;
            }

            for (int index = 0; index < left.Count; index++)
            {
                WaveSaveData a = left[index];
                WaveSaveData b = right[index];

                if (a == null ||
                    a.sequence != b.sequence ||
                    a.gateId != b.gateId ||
                    a.arrivalStrategicSeconds !=
                        b.arrivalStrategicSeconds ||
                    a.threatPower != b.threatPower ||
                    a.status != b.status ||
                    a.raiders != b.raiders ||
                    a.brutes != b.brutes ||
                    a.shamans != b.shamans ||
                    a.warningStage != b.warningStage ||
                    !ReportsEqual(a.battleReport, b.battleReport))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ReportsEqual(
            BattleReportSaveData a,
            BattleReportSaveData b
        )
        {
            if (a == null || b == null)
            {
                return a == b;
            }

            return a.resolved == b.resolved &&
                   a.outcome == b.outcome &&
                   a.effectiveDefense == b.effectiveDefense &&
                   a.defenseRatio == b.defenseRatio &&
                   a.initialGateHealth == b.initialGateHealth &&
                   a.gateDamage == b.gateDamage &&
                   a.finalGateHealth == b.finalGateHealth &&
                   a.initialWarriors == b.initialWarriors &&
                   a.lostWarriors == b.lostWarriors &&
                   a.initialArchers == b.initialArchers &&
                   a.lostArchers == b.lostArchers &&
                   a.initialMages == b.initialMages &&
                   a.lostMages == b.lostMages &&
                   a.spoils == b.spoils &&
                   a.reportAcknowledged == b.reportAcknowledged;
        }

        private static bool TransferListsEqual(
            List<TroopTransferSaveData> left,
            List<TroopTransferSaveData> right
        )
        {
            if (left == null || left.Count != right.Count)
            {
                return false;
            }

            for (int index = 0; index < left.Count; index++)
            {
                TroopTransferSaveData a = left[index];
                TroopTransferSaveData b = right[index];
                if (a == null || a.sequence != b.sequence ||
                    a.sourceKind != b.sourceKind ||
                    a.sourceGateId != b.sourceGateId ||
                    a.destinationKind != b.destinationKind ||
                    a.destinationGateId != b.destinationGateId ||
                    a.unitType != b.unitType || a.quantity != b.quantity ||
                    a.departureStrategicSeconds != b.departureStrategicSeconds ||
                    a.arrivalStrategicSeconds != b.arrivalStrategicSeconds)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
