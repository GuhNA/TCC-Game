using System.Collections;
using System.Linq;
using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;
using UnityEngine;
using UnityEngine.TestTools;

namespace TCCGame.Tests.PlayMode
{
    public sealed class CampaignRuntimeIntegrationTests
    {
        private bool hadSave;
        private string previousSave;

        [SetUp]
        public void SetUp()
        {
            hadSave = PlayerPrefs.HasKey(GameSaveRepository.SaveKey);
            previousSave = hadSave
                ? PlayerPrefs.GetString(GameSaveRepository.SaveKey)
                : null;
            PlayerPrefs.DeleteKey(GameSaveRepository.SaveKey);
            PlayerPrefs.Save();
            GameSessionProvider.Reset();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GameSessionProvider.Reset();

            if (hadSave)
            {
                PlayerPrefs.SetString(
                    GameSaveRepository.SaveKey,
                    previousSave
                );
            }
            else
            {
                PlayerPrefs.DeleteKey(GameSaveRepository.SaveKey);
            }

            PlayerPrefs.Save();
            yield return null;
        }

        [UnityTest]
        public IEnumerator RuntimeBootstrap_CreatesOneDriverAndAdvancesTime()
        {
            yield return null;

            CampaignRuntimeDriver[] drivers = Object.FindObjectsByType<
                CampaignRuntimeDriver
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            CampaignService campaign = GameSessionProvider.Current.Campaign;
            double before = campaign.ElapsedStrategicSeconds;

            yield return null;
            yield return null;

            Assert.That(drivers, Has.Length.EqualTo(1));
            Assert.That(GlobalThreatHud.Exists, Is.True);
            Assert.That(
                campaign.ElapsedStrategicSeconds,
                Is.GreaterThan(before)
            );
            Assert.That(campaign.Waves.CurrentWave, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator DuplicateDriver_DestroysItself()
        {
            yield return null;
            new GameObject("Duplicate Campaign Runtime")
                .AddComponent<CampaignRuntimeDriver>();

            yield return null;

            CampaignRuntimeDriver[] drivers = Object.FindObjectsByType<
                CampaignRuntimeDriver
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            Assert.That(drivers.Count(), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ModalInteraction_BlocksPlayerWithoutPausingCampaignClock()
        {
            yield return null;
            CampaignService campaign = GameSessionProvider.Current.Campaign;
            var player = new GameObject("Campaign Modal Test Player");
            PlayerInteractor interactor =
                player.AddComponent<PlayerInteractor>();
            var owner = new object();

            interactor.AcquireModalLock(owner);
            double before = campaign.ElapsedStrategicSeconds;
            yield return null;
            yield return null;

            Assert.That(interactor.HasModalOpen, Is.True);
            Assert.That(campaign.IsPaused, Is.False);
            Assert.That(
                campaign.ElapsedStrategicSeconds,
                Is.GreaterThan(before)
            );

            interactor.ReleaseModalLock(owner);
            yield return null;

            Assert.That(campaign.IsPaused, Is.False);

            Object.Destroy(player);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DueArrival_JoinsAutomaticBattleExactlyOnce()
        {
            GameSaveData data = GameSaveData.CreateDefault();
            CampaignSaveDataUtility.Normalize(data);
            data.campaign.scheduledWaves.Clear();
            data.campaign.scheduledWaves.Add(new WaveSaveData
            {
                sequence = 1,
                gateId = GateId.North,
                arrivalStrategicSeconds = 0d,
                threatPower = 10,
                status = WaveStatus.AwaitingResolution,
                raiders = 10,
                warningStage = WaveWarningStage.Critical
            });
            data.campaign.nextWaveSequence = 2;
            data.campaign.troopTransfers.Add(new TroopTransferSaveData
            {
                sequence = 1,
                sourceKind = TroopLocationKind.Reserve,
                destinationKind = TroopLocationKind.Gate,
                destinationGateId = GateId.North,
                unitType = UnitType.Warrior,
                quantity = 5,
                departureStrategicSeconds = 0d,
                arrivalStrategicSeconds = 0d
            });
            data.campaign.nextTroopTransferSequence = 2;
            new GameSaveRepository().Save(data);
            GameSessionProvider.Reset();

            yield return null;
            yield return null;

            GameSession session = GameSessionProvider.Current;
            BattleReportSnapshot? report = session.Campaign
                .GetLatestBattleReport(GateId.North);
            Assert.That(report, Is.Not.Null);
            Assert.That(report.Value.InitialWarriors, Is.EqualTo(5));
            Assert.That(
                session.Campaign.GetGate(GateId.North).Warriors,
                Is.EqualTo(report.Value.SurvivingWarriors)
            );
            Assert.That(
                session.Campaign.GetLatestBattleReport(GateId.North).Value
                    .Spoils,
                Is.EqualTo(report.Value.Spoils)
            );
        }

        [UnityTest]
        public IEnumerator PendingBattle_WaitsUntilModalPauseEnds()
        {
            GameSaveData data = GameSaveData.CreateDefault();
            CampaignSaveDataUtility.Normalize(data);
            data.campaign.scheduledWaves.Clear();
            data.campaign.scheduledWaves.Add(new WaveSaveData
            {
                sequence = 1,
                gateId = GateId.East,
                arrivalStrategicSeconds = 0d,
                threatPower = 10,
                status = WaveStatus.AwaitingResolution,
                raiders = 10,
                warningStage = WaveWarningStage.Critical
            });
            data.campaign.nextWaveSequence = 2;
            new GameSaveRepository().Save(data);
            GameSessionProvider.Reset();
            CampaignService campaign = GameSessionProvider.Current.Campaign;
            var pauseOwner = new object();
            campaign.AcquirePause(pauseOwner);

            yield return null;
            yield return null;

            Assert.That(
                campaign.GetLatestBattleReport(GateId.East),
                Is.Null
            );
            Assert.That(
                campaign.Waves.CurrentWave.Value.Status,
                Is.EqualTo(WaveStatus.AwaitingResolution)
            );

            campaign.ReleasePause(pauseOwner);
            yield return null;

            Assert.That(
                campaign.GetLatestBattleReport(GateId.East),
                Is.Not.Null
            );
        }
    }
}
