using NUnit.Framework;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Tests.EditMode
{
    public sealed class ThreatForecastTests
    {
        private readonly ThreatForecastService forecast =
            new(new CampaignRules());

        [Test]
        public void WarriorBonus_AppliesAgainstRaidersButNotBrutes()
        {
            GateStateSnapshot gate = new(
                GateId.North,
                100,
                10,
                0,
                0,
                0
            );
            WaveSnapshot raiders = CreateWave(
                threat: 10,
                raiders: 10,
                brutes: 0,
                shamans: 0
            );
            WaveSnapshot brutes = CreateWave(
                threat: 9,
                raiders: 0,
                brutes: 3,
                shamans: 0
            );

            Assert.That(
                forecast.Calculate(gate, raiders).EffectiveDefense,
                Is.EqualTo(15d).Within(0.001d)
            );
            Assert.That(
                forecast.Calculate(gate, brutes).EffectiveDefense,
                Is.EqualTo(10d).Within(0.001d)
            );
        }

        [Test]
        public void ArcherAndMageBonuses_MatchTheirFavoredEnemies()
        {
            GateStateSnapshot gate = new(
                GateId.East,
                100,
                0,
                4,
                3,
                0
            );
            WaveSnapshot wave = CreateWave(
                threat: 20,
                raiders: 0,
                brutes: 4,
                shamans: 2
            );
            ThreatForecast result = forecast.Calculate(gate, wave);

            Assert.That(result.BaseDefense, Is.EqualTo(11d));
            Assert.That(result.EffectiveDefense, Is.GreaterThan(11d));
            Assert.That(result.Explanation, Does.Contain("Arqueiros"));
        }

        [TestCase(1.15d, BattleOutlook.Favorable)]
        [TestCase(0.85d, BattleOutlook.Risky)]
        [TestCase(0.849d, BattleOutlook.Unfavorable)]
        public void Classification_UsesDocumentedBoundaries(
            double ratio,
            BattleOutlook expected
        )
        {
            Assert.That(ThreatForecastService.Classify(ratio), Is.EqualTo(expected));
        }

        private static WaveSnapshot CreateWave(
            int threat,
            int raiders,
            int brutes,
            int shamans
        )
        {
            return new WaveSnapshot(
                1,
                GateId.North,
                90d,
                threat,
                WaveStatus.Scheduled,
                raiders,
                brutes,
                shamans,
                WaveWarningStage.Announced
            );
        }
    }
}
