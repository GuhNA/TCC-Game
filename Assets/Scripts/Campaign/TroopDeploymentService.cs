using System;
using System.Collections.Generic;
using System.Linq;
using TCCGame.Core;
using TCCGame.Core.Persistence;

namespace TCCGame.Gameplay.Campaign
{
    public sealed class TroopDeploymentService
    {
        private readonly GameSaveData state;
        private readonly CampaignRules rules;
        private readonly Action commit;

        public event Action Changed;

        public IReadOnlyList<TroopTransferSnapshot> InTransit =>
            state.campaign.troopTransfers
                .Select(transfer => transfer.ToSnapshot())
                .ToArray();

        public TroopDeploymentService(
            GameSaveData state,
            CampaignRules rules,
            Action commit
        )
        {
            this.state = state ??
                throw new ArgumentNullException(nameof(state));
            this.rules = rules ??
                throw new ArgumentNullException(nameof(rules));
            this.commit = commit ??
                throw new ArgumentNullException(nameof(commit));

            CampaignSaveDataUtility.Normalize(state);
            ResolveArrivals();
        }

        public int GetReserveCount(UnitType unitType)
        {
            ValidateUnitType(unitType);
            return state.army.Get(unitType);
        }

        public int GetGateCount(GateId gateId, UnitType unitType)
        {
            ValidateGate(gateId);
            ValidateUnitType(unitType);
            return GetGate(gateId).Get(unitType);
        }

        public TroopDispatchResult Dispatch(
            TroopLocation source,
            TroopLocation destination,
            UnitType unitType,
            int quantity
        )
        {
            if (state.campaign.outcome != CampaignOutcome.Ongoing)
            {
                return new TroopDispatchResult(
                    TroopDispatchStatus.CampaignEnded
                );
            }

            if (quantity <= 0)
            {
                return new TroopDispatchResult(
                    TroopDispatchStatus.InvalidQuantity
                );
            }

            if (!IsValid(source) || !IsValid(destination) ||
                source.Equals(destination) ||
                state.campaign.nextTroopTransferSequence == int.MaxValue)
            {
                return new TroopDispatchResult(
                    TroopDispatchStatus.InvalidRoute
                );
            }

            ValidateUnitType(unitType);
            int available = GetCount(source, unitType);
            if (quantity > available)
            {
                return new TroopDispatchResult(
                    TroopDispatchStatus.InsufficientTroops
                );
            }

            int sequence = state.campaign.nextTroopTransferSequence++;
            var random = new CampaignRandom(
                state.campaign.randomSeed ^ unchecked((int)0x51ED270Bu),
                sequence
            );
            int duration = random.NextInt(
                rules.MinimumTransferSeconds,
                rules.MaximumTransferSeconds + 1
            );
            double departure = state.campaign.elapsedStrategicSeconds;

            SetCount(source, unitType, available - quantity);
            var transfer = new TroopTransferSaveData
            {
                sequence = sequence,
                sourceKind = source.Kind,
                sourceGateId = source.GateId,
                destinationKind = destination.Kind,
                destinationGateId = destination.GateId,
                unitType = unitType,
                quantity = quantity,
                departureStrategicSeconds = departure,
                arrivalStrategicSeconds = departure + duration
            };
            state.campaign.troopTransfers.Add(transfer);
            commit();
            Changed?.Invoke();

            return new TroopDispatchResult(
                TroopDispatchStatus.Dispatched,
                transfer.ToSnapshot()
            );
        }

        public bool ResolveArrivals()
        {
            double elapsed = state.campaign.elapsedStrategicSeconds;
            List<TroopTransferSaveData> arrived =
                state.campaign.troopTransfers
                    .Where(transfer =>
                        transfer.arrivalStrategicSeconds <= elapsed)
                    .ToList();

            if (arrived.Count == 0)
            {
                return false;
            }

            foreach (TroopTransferSaveData transfer in arrived)
            {
                TroopLocation destination =
                    transfer.destinationKind == TroopLocationKind.Reserve
                        ? TroopLocation.Reserve
                        : TroopLocation.Gate(transfer.destinationGateId);
                int current = GetCount(destination, transfer.unitType);
                SetCount(
                    destination,
                    transfer.unitType,
                    (int)Math.Min(int.MaxValue, (long)current + transfer.quantity)
                );
                state.campaign.troopTransfers.Remove(transfer);
            }

            commit();
            Changed?.Invoke();
            return true;
        }

        private int GetCount(TroopLocation location, UnitType unitType)
        {
            return location.Kind == TroopLocationKind.Reserve
                ? state.army.Get(unitType)
                : GetGate(location.GateId).Get(unitType);
        }

        private void SetCount(
            TroopLocation location,
            UnitType unitType,
            int value
        )
        {
            if (location.Kind == TroopLocationKind.Reserve)
            {
                state.army.Set(unitType, value);
                return;
            }

            GetGate(location.GateId).Set(unitType, value);
        }

        private GateSaveData GetGate(GateId gateId)
        {
            return state.campaign.gates.Find(gate => gate.gateId == gateId);
        }

        private static bool IsValid(TroopLocation location)
        {
            return Enum.IsDefined(typeof(TroopLocationKind), location.Kind) &&
                   (location.Kind == TroopLocationKind.Reserve ||
                    Enum.IsDefined(typeof(GateId), location.GateId));
        }

        private static void ValidateGate(GateId gateId)
        {
            if (!Enum.IsDefined(typeof(GateId), gateId))
            {
                throw new ArgumentOutOfRangeException(nameof(gateId));
            }
        }

        private static void ValidateUnitType(UnitType unitType)
        {
            if (!Enum.IsDefined(typeof(UnitType), unitType))
            {
                throw new ArgumentOutOfRangeException(nameof(unitType));
            }
        }
    }
}
