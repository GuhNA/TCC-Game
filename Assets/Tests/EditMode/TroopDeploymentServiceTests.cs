using System.Linq;
using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Tests.EditMode
{
    public sealed class TroopDeploymentServiceTests
    {
        [Test]
        public void Dispatch_RemovesFromOriginAndCreditsOnlyOnArrival()
        {
            GameSaveData state = CreateState();
            state.army.warriors = 12;
            CampaignRules rules = new(
                checkpointIntervalSeconds: 1000d,
                minimumTransferSeconds: 15,
                maximumTransferSeconds: 15
            );
            CampaignService campaign = new(state, rules, () => { });
            TroopDeploymentService deployment = new(state, rules, () => { });

            TroopDispatchResult result = deployment.Dispatch(
                TroopLocation.Reserve,
                TroopLocation.Gate(GateId.East),
                UnitType.Warrior,
                5
            );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(deployment.GetReserveCount(UnitType.Warrior), Is.EqualTo(7));
            Assert.That(deployment.GetGateCount(GateId.East, UnitType.Warrior), Is.Zero);
            Assert.That(deployment.InTransit.Single().Quantity, Is.EqualTo(5));

            campaign.Advance(14d);
            Assert.That(deployment.ResolveArrivals(), Is.False);
            campaign.Advance(1d);
            Assert.That(deployment.ResolveArrivals(), Is.True);

            Assert.That(deployment.GetGateCount(GateId.East, UnitType.Warrior), Is.EqualTo(5));
            Assert.That(deployment.InTransit, Is.Empty);
        }

        [Test]
        public void GateToGateTransfer_ConservesEveryTroop()
        {
            GameSaveData state = CreateState();
            state.campaign.gates.Single(gate => gate.gateId == GateId.North)
                .archers = 8;
            CampaignRules rules = new(
                checkpointIntervalSeconds: 1000d,
                minimumTransferSeconds: 20,
                maximumTransferSeconds: 20
            );
            CampaignService campaign = new(state, rules, () => { });
            TroopDeploymentService deployment = new(state, rules, () => { });

            deployment.Dispatch(
                TroopLocation.Gate(GateId.North),
                TroopLocation.Gate(GateId.South),
                UnitType.Archer,
                6
            );

            int totalDuringTravel = deployment.GetGateCount(GateId.North, UnitType.Archer) +
                deployment.GetGateCount(GateId.South, UnitType.Archer) +
                deployment.InTransit.Sum(transfer => transfer.Quantity);
            Assert.That(totalDuringTravel, Is.EqualTo(8));

            campaign.Advance(20d);
            deployment.ResolveArrivals();
            int totalAfterArrival = deployment.GetGateCount(GateId.North, UnitType.Archer) +
                deployment.GetGateCount(GateId.South, UnitType.Archer);
            Assert.That(totalAfterArrival, Is.EqualTo(8));
        }

        [Test]
        public void InvalidOrUnaffordableDispatch_DoesNotMutateState()
        {
            GameSaveData state = CreateState();
            state.army.mages = 2;
            CampaignRules rules = new();
            TroopDeploymentService deployment = new(state, rules, () => { });

            TroopDispatchResult insufficient = deployment.Dispatch(
                TroopLocation.Reserve,
                TroopLocation.Gate(GateId.West),
                UnitType.Mage,
                3
            );
            TroopDispatchResult sameRoute = deployment.Dispatch(
                TroopLocation.Reserve,
                TroopLocation.Reserve,
                UnitType.Mage,
                1
            );

            Assert.That(insufficient.Status, Is.EqualTo(TroopDispatchStatus.InsufficientTroops));
            Assert.That(sameRoute.Status, Is.EqualTo(TroopDispatchStatus.InvalidRoute));
            Assert.That(deployment.GetReserveCount(UnitType.Mage), Is.EqualTo(2));
            Assert.That(deployment.InTransit, Is.Empty);
        }

        [Test]
        public void InTransitTroops_StillCountTowardMilitaryProgress()
        {
            GameSaveData state = CreateState();
            state.army.warriors = 10;
            CampaignRules rules = new();
            TroopDeploymentService deployment = new(state, rules, () => { });
            double before = rules.CalculateMilitaryProgress(state);

            deployment.Dispatch(
                TroopLocation.Reserve,
                TroopLocation.Gate(GateId.North),
                UnitType.Warrior,
                10
            );

            Assert.That(rules.CalculateMilitaryProgress(state), Is.EqualTo(before));
        }

        [Test]
        public void EndedCampaign_RejectsNewDispatches()
        {
            GameSaveData state = CreateState();
            state.army.warriors = 3;
            state.campaign.gates[0].currentHealth = 0;
            state.campaign.outcome = CampaignOutcome.Defeat;
            TroopDeploymentService deployment = new(
                state,
                new CampaignRules(),
                () => { }
            );

            TroopDispatchResult result = deployment.Dispatch(
                TroopLocation.Reserve,
                TroopLocation.Gate(GateId.North),
                UnitType.Warrior,
                1
            );

            Assert.That(
                result.Status,
                Is.EqualTo(TroopDispatchStatus.CampaignEnded)
            );
            Assert.That(deployment.GetReserveCount(UnitType.Warrior),
                Is.EqualTo(3));
            Assert.That(deployment.InTransit, Is.Empty);
        }

        private static GameSaveData CreateState()
        {
            GameSaveData state = GameSaveData.CreateDefault();
            CampaignSaveDataUtility.Normalize(state);
            return state;
        }
    }
}
