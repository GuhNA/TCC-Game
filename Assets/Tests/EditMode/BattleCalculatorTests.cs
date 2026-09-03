using NUnit.Framework;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Tests.EditMode
{
    public sealed class BattleCalculatorTests
    {
        private readonly BattleCalculator calculator =
            new(new CampaignRules());

        [Test]
        public void FavoredComposition_ReducesCasualties()
        {
            GateStateSnapshot gate = CreateGate(100, 0, 0);
            BattleResolution favored = calculator.Calculate(
                gate,
                CreateWave(100, 100, 0, 0)
            );
            BattleResolution neutral = calculator.Calculate(
                gate,
                CreateWave(100, 0, 34, 0)
            );

            Assert.That(favored.EffectiveDefense, Is.GreaterThan(
                neutral.EffectiveDefense
            ));
            Assert.That(favored.LostWarriors, Is.LessThan(
                neutral.LostWarriors
            ));
        }

        [Test]
        public void SufficientDefense_ProtectsGate()
        {
            BattleResolution result = calculator.Calculate(
                CreateGate(20, 0, 0),
                CreateWave(10, 10, 0, 0)
            );

            Assert.That(result.GateDamage, Is.Zero);
            Assert.That(result.FinalGateHealth, Is.EqualTo(100));
            Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Repelled));
            Assert.That(result.Spoils, Is.EqualTo(1));
        }

        [Test]
        public void UndefendedLowHealthGate_IsBreachedWithoutSpoils()
        {
            GateStateSnapshot gate = new(
                GateId.North,
                5,
                0,
                0,
                0,
                0
            );
            BattleResolution result = calculator.Calculate(
                gate,
                CreateWave(10, 10, 0, 0)
            );

            Assert.That(result.GateDamage, Is.EqualTo(5));
            Assert.That(result.FinalGateHealth, Is.Zero);
            Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Breached));
            Assert.That(result.Spoils, Is.Zero);
        }

        [Test]
        public void LargestRemainder_DistributesExactLossTotal()
        {
            GateStateSnapshot gate = new(
                GateId.North,
                100,
                3,
                2,
                1,
                0
            );
            BattleResolution result = calculator.Calculate(
                gate,
                CreateWave(1000, 1000, 0, 0)
            );

            Assert.That(
                result.LostWarriors + result.LostArchers + result.LostMages,
                Is.EqualTo(4)
            );
            Assert.That(result.LostWarriors, Is.EqualTo(2));
            Assert.That(result.LostArchers, Is.EqualTo(1));
            Assert.That(result.LostMages, Is.EqualTo(1));
        }

        [Test]
        public void ExtremeValidCounts_DoNotOverflowOrWrap()
        {
            GateStateSnapshot gate = new(
                GateId.North,
                100,
                int.MaxValue,
                int.MaxValue,
                int.MaxValue,
                0
            );
            BattleResolution result = default;

            Assert.DoesNotThrow(() => result = calculator.Calculate(
                gate,
                CreateWave(100000, 100000, 0, 0)
            ));
            Assert.That(result.LostWarriors,
                Is.InRange(0, int.MaxValue));
            Assert.That(result.LostArchers,
                Is.InRange(0, int.MaxValue));
            Assert.That(result.LostMages,
                Is.InRange(0, int.MaxValue));
        }

        private static GateStateSnapshot CreateGate(
            int warriors,
            int archers,
            int mages
        ) => new(
            GateId.North,
            100,
            warriors,
            archers,
            mages,
            0
        );

        private static WaveSnapshot CreateWave(
            int threat,
            int raiders,
            int brutes,
            int shamans
        ) => new(
            1,
            GateId.North,
            0d,
            threat,
            WaveStatus.AwaitingResolution,
            raiders,
            brutes,
            shamans,
            WaveWarningStage.Critical
        );
    }
}
