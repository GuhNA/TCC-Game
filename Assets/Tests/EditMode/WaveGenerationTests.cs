using System.Linq;
using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Tests.EditMode
{
    public sealed class WaveGenerationTests
    {
        [Test]
        public void CampaignRandom_KnownSeedProducesStableSequence()
        {
            var random = new CampaignRandom(12345, 7);

            Assert.That(random.NextUInt(), Is.EqualTo(2018112477u));
            Assert.That(random.NextUInt(), Is.EqualTo(2155636804u));
            Assert.That(random.NextUInt(), Is.EqualTo(2749968702u));
            Assert.That(random.NextUInt(), Is.EqualTo(1008257273u));
        }

        [Test]
        public void SameStateAndSeed_ProduceIdenticalWave()
        {
            GameSaveData state = CreateState(seed: 9182);
            var generator = new WaveGenerator(new CampaignRules());

            WaveSaveData first = generator.Generate(state, 5, 100d);
            WaveSaveData second = generator.Generate(state, 5, 100d);

            AssertWavesEqual(first, second);
        }

        [Test]
        public void FirstWave_UsesExactPreparationTime()
        {
            GameSaveData state = CreateState(seed: 10);
            var generator = new WaveGenerator(new CampaignRules());

            WaveSaveData wave = generator.Generate(state, 1, 25d);

            Assert.That(wave.arrivalStrategicSeconds, Is.EqualTo(205d));
            Assert.That(wave.brutes, Is.Zero);
            Assert.That(wave.shamans, Is.Zero);
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(10)]
        public void Composition_ConsumesExactThreatBudget(int sequence)
        {
            CampaignRules rules = new CampaignRules();
            GameSaveData state = CreateState(seed: 200 + sequence);
            WaveSaveData wave = new WaveGenerator(rules).Generate(
                state,
                sequence,
                0d
            );
            int cost = wave.raiders * rules.GetEnemyCost(EnemyType.Raider) +
                wave.brutes * rules.GetEnemyCost(EnemyType.Brute) +
                wave.shamans * rules.GetEnemyCost(EnemyType.Shaman);

            Assert.That(cost, Is.EqualTo(wave.threatPower));

            if (sequence < 3)
            {
                Assert.That(wave.brutes, Is.Zero);
            }

            if (sequence < 5)
            {
                Assert.That(wave.shamans, Is.Zero);
            }
        }

        [Test]
        public void GateSelection_PreventsThirdConsecutiveAttack()
        {
            GameSaveData state = CreateState(seed: 44);
            state.campaign.scheduledWaves.Add(CreatePreviousWave(
                1,
                GateId.North
            ));
            state.campaign.scheduledWaves.Add(CreatePreviousWave(
                2,
                GateId.North
            ));

            WaveSaveData generated = new WaveGenerator(
                new CampaignRules()
            ).Generate(state, 3, 0d);

            Assert.That(generated.gateId, Is.Not.EqualTo(GateId.North));
        }

        [Test]
        public void ProgressFactor_IsBoundedAndWaveRemainsImmutable()
        {
            CampaignRules rules = new CampaignRules();
            GameSaveData state = CreateState(seed: 77);
            state.army.warriors = 100000;

            Assert.That(
                rules.CalculateProgressFactor(state, 1),
                Is.EqualTo(1.15d)
            );

            WaveSaveData wave = new WaveGenerator(rules).Generate(
                state,
                1,
                0d
            );
            int threat = wave.threatPower;
            int raiders = wave.raiders;
            state.army.warriors = 0;

            Assert.That(wave.threatPower, Is.EqualTo(threat));
            Assert.That(wave.raiders, Is.EqualTo(raiders));
        }

        [Test]
        public void VeryHighWave_ClampsThreatToSafeMaximum()
        {
            CampaignRules rules = new CampaignRules();
            GameSaveData state = CreateState(seed: 81);

            Assert.That(
                rules.CalculateBaseThreatPower(int.MaxValue),
                Is.EqualTo(CampaignRules.DefaultMaximumThreatPower)
            );
            Assert.That(
                rules.CalculateThreatPower(state, int.MaxValue),
                Is.LessThanOrEqualTo(
                    CampaignRules.DefaultMaximumThreatPower
                )
            );
        }

        [Test]
        public void UnitAffinities_MatchApprovedDesign()
        {
            CampaignRules rules = new CampaignRules();

            Assert.That(
                rules.GetFavoredEnemy(UnitType.Warrior),
                Is.EqualTo(EnemyType.Raider)
            );
            Assert.That(
                rules.GetFavoredEnemy(UnitType.Archer),
                Is.EqualTo(EnemyType.Shaman)
            );
            Assert.That(
                rules.GetFavoredEnemy(UnitType.Mage),
                Is.EqualTo(EnemyType.Brute)
            );
            Assert.That(rules.AdvantageMultiplier, Is.EqualTo(1.5d));
            Assert.That(
                rules.GetEffectivenessMultiplier(
                    UnitType.Warrior,
                    EnemyType.Raider
                ),
                Is.EqualTo(1.5d)
            );
            Assert.That(
                rules.GetEffectivenessMultiplier(
                    UnitType.Warrior,
                    EnemyType.Brute
                ),
                Is.EqualTo(1d)
            );
        }

        private static GameSaveData CreateState(int seed)
        {
            GameSaveData state = GameSaveData.CreateDefault();
            CampaignSaveDataUtility.Normalize(state);
            state.campaign.randomSeed = seed;
            state.campaign.scheduledWaves.Clear();
            return state;
        }

        private static WaveSaveData CreatePreviousWave(
            int sequence,
            GateId gateId
        )
        {
            return new WaveSaveData
            {
                sequence = sequence,
                gateId = gateId,
                arrivalStrategicSeconds = sequence,
                threatPower = 1,
                raiders = 1,
                status = WaveStatus.Resolved
            };
        }

        private static void AssertWavesEqual(
            WaveSaveData first,
            WaveSaveData second
        )
        {
            Assert.That(second.sequence, Is.EqualTo(first.sequence));
            Assert.That(second.gateId, Is.EqualTo(first.gateId));
            Assert.That(
                second.arrivalStrategicSeconds,
                Is.EqualTo(first.arrivalStrategicSeconds)
            );
            Assert.That(second.threatPower, Is.EqualTo(first.threatPower));
            Assert.That(second.raiders, Is.EqualTo(first.raiders));
            Assert.That(second.brutes, Is.EqualTo(first.brutes));
            Assert.That(second.shamans, Is.EqualTo(first.shamans));
        }
    }
}
