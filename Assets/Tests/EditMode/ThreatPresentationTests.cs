using NUnit.Framework;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Tests.EditMode
{
    public sealed class ThreatPresentationTests
    {
        [Test]
        public void Cards_RevealInformationByWarningStage()
        {
            GameSaveData state = CreateState(out WaveSaveData wave);
            CampaignRules rules = new();
            CampaignService campaign = new(state, rules, () => { });
            var presentation = new ThreatPresentationService(campaign, rules);

            GateThreatViewData observed = presentation.BuildGateCard(GateId.North);
            Assert.That(observed.State, Is.EqualTo(GateThreatState.Observed));
            Assert.That(observed.Threat, Is.Empty);
            Assert.That(observed.Countdown, Is.Empty);

            wave.warningStage = WaveWarningStage.Announced;
            GateThreatViewData announced = presentation.BuildGateCard(GateId.North);
            Assert.That(announced.State, Is.EqualTo(GateThreatState.Announced));
            Assert.That(announced.Threat, Is.EqualTo("Ameaça 12"));
            Assert.That(announced.Composition, Is.Empty);

            wave.warningStage = WaveWarningStage.Imminent;
            GateThreatViewData imminent = presentation.BuildGateCard(GateId.North);
            Assert.That(imminent.Composition, Is.Not.Empty);
            Assert.That(imminent.Outlook, Is.Not.Empty);
        }

        [Test]
        public void OtherGatesRemainSafeAndDueWaveHasNoNegativeCountdown()
        {
            GameSaveData state = CreateState(out WaveSaveData wave);
            CampaignRules rules = new();
            CampaignService campaign = new(state, rules, () => { });
            var presentation = new ThreatPresentationService(campaign, rules);

            Assert.That(
                presentation.BuildGateCard(GateId.West).State,
                Is.EqualTo(GateThreatState.Safe)
            );

            wave.status = WaveStatus.AwaitingResolution;
            wave.warningStage = WaveWarningStage.Critical;
            state.campaign.elapsedStrategicSeconds = 200d;
            GateThreatViewData due = presentation.BuildGateCard(GateId.North);

            Assert.That(due.State, Is.EqualTo(GateThreatState.AttackStarted));
            Assert.That(due.Countdown, Is.Empty);
            Assert.That(due.Status, Is.EqualTo("ATAQUE INICIADO"));
        }

        [Test]
        public void AlertText_IdentifiesGateAndSeverity()
        {
            GameSaveData state = CreateState(out WaveSaveData _);
            CampaignRules rules = new();
            CampaignService campaign = new(state, rules, () => { });
            var presentation = new ThreatPresentationService(campaign, rules);
            WaveSnapshot wave = campaign.Waves.CurrentWave.Value;

            WaveAlertMessage alert = presentation.BuildAlert(
                wave,
                WaveWarningStage.Critical
            );

            Assert.That(alert.Text, Does.Contain("CRÍTICO"));
            Assert.That(alert.Text, Does.Contain("Norte"));
        }

        private static GameSaveData CreateState(out WaveSaveData wave)
        {
            GameSaveData state = GameSaveData.CreateDefault();
            CampaignSaveDataUtility.Normalize(state);
            state.campaign.scheduledWaves.Clear();
            wave = new WaveSaveData
            {
                sequence = 1,
                gateId = GateId.North,
                arrivalStrategicSeconds = 180d,
                threatPower = 12,
                status = WaveStatus.Scheduled,
                raiders = 6,
                brutes = 2,
                warningStage = WaveWarningStage.None
            };
            state.campaign.scheduledWaves.Add(wave);
            state.campaign.nextWaveSequence = 2;
            return state;
        }
    }
}
