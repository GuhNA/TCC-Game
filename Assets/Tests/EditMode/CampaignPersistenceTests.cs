using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;
using TCCGame.Gameplay.Navigation;
using UnityEngine;

namespace TCCGame.Tests.EditMode
{
    public sealed class CampaignPersistenceTests
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
        }

        [TearDown]
        public void TearDown()
        {
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
        }

        [Test]
        public void DefaultSave_ClonesCampaignData()
        {
            GameSaveData original = GameSaveData.CreateDefault();
            CampaignSaveDataUtility.Normalize(original);
            original.campaign.elapsedStrategicSeconds = 42.5d;
            original.campaign.gates[0].warriors = 3;

            GameSaveData clone = original.Clone();

            Assert.That(clone.campaign, Is.Not.Null);
            Assert.That(
                clone.campaign.elapsedStrategicSeconds,
                Is.EqualTo(42.5d)
            );
            Assert.That(clone.campaign.gates[0].warriors, Is.EqualTo(3));
            Assert.That(clone.campaign.gates.Count, Is.EqualTo(4));
        }

        [Test]
        public void CampaignState_RoundTripsThroughPlayerPrefs()
        {
            GameSaveData data = GameSaveData.CreateDefault();
            CampaignSaveDataUtility.Normalize(data);
            data.campaign.elapsedStrategicSeconds = 47.25d;
            data.campaign.gates[2].currentHealth = 63;
            data.campaign.gates[2].archers = 9;
            data.campaign.scheduledWaves.Add(new WaveSaveData
            {
                sequence = 1,
                gateId = GateId.South,
                arrivalStrategicSeconds = 180d,
                threatPower = 10,
                status = WaveStatus.Scheduled,
                raiders = 10,
                warningStage = WaveWarningStage.Announced
            });
            data.campaign.nextWaveSequence = 2;
            data.campaign.troopTransfers.Add(new TroopTransferSaveData
            {
                sequence = 1,
                sourceKind = TroopLocationKind.Reserve,
                destinationKind = TroopLocationKind.Gate,
                destinationGateId = GateId.East,
                unitType = UnitType.Warrior,
                quantity = 4,
                departureStrategicSeconds = 47.25d,
                arrivalStrategicSeconds = 67.25d
            });
            data.campaign.nextTroopTransferSequence = 2;

            var repository = new GameSaveRepository();
            repository.Save(data);
            GameSaveData loaded = repository.Load();

            Assert.That(
                loaded.campaign.elapsedStrategicSeconds,
                Is.EqualTo(47.25d)
            );
            Assert.That(loaded.campaign.gates[2].currentHealth, Is.EqualTo(63));
            Assert.That(loaded.campaign.gates[2].archers, Is.EqualTo(9));
            Assert.That(
                loaded.campaign.scheduledWaves.Count,
                Is.EqualTo(1)
            );
            Assert.That(
                loaded.campaign.scheduledWaves[0].warningStage,
                Is.EqualTo(WaveWarningStage.Announced)
            );
            Assert.That(loaded.campaign.troopTransfers, Has.Count.EqualTo(1));
            Assert.That(loaded.campaign.troopTransfers[0].quantity, Is.EqualTo(4));
            Assert.That(
                loaded.campaign.troopTransfers[0].destinationGateId,
                Is.EqualTo(GateId.East)
            );
        }

        [Test]
        public void VersionThreeSave_MigratesWithoutLosingExistingState()
        {
            const string json =
                "{\"version\":3,\"money\":321," +
                "\"buildings\":[{\"type\":2,\"level\":4}]," +
                "\"army\":{\"warriors\":7,\"archers\":5," +
                "\"mages\":2}," +
                "\"recruitment\":{\"initialized\":true," +
                "\"availableWarriors\":8,\"availableArchers\":9," +
                "\"availableMages\":10}," +
                "\"playerInventory\":{\"recruitedAdventurerIds\":" +
                "[\"adventurer.archer\"],\"items\":[]}}";
            PlayerPrefs.SetString(GameSaveRepository.SaveKey, json);

            GameSaveData loaded = new GameSaveRepository().Load();

            Assert.That(loaded.version, Is.EqualTo(7));
            Assert.That(loaded.money, Is.EqualTo(321));
            Assert.That(
                loaded.GetBuildingLevel(BuildingType.Barracks),
                Is.EqualTo(4)
            );
            Assert.That(loaded.army.warriors, Is.EqualTo(7));
            Assert.That(loaded.army.archers, Is.EqualTo(5));
            Assert.That(loaded.army.mages, Is.EqualTo(2));
            Assert.That(
                loaded.playerInventory.recruitedAdventurerIds,
                Does.Contain("adventurer.archer")
            );
            AssertFourDefaultGates(loaded.campaign.gates);
            Assert.That(loaded.navigation, Is.Not.Null);
            Assert.That(loaded.navigation.currentArea, Is.EqualTo(CityArea.Center));
        }

        [Test]
        public void VersionSixPendingWave_MigratesToUnresolvedV7Report()
        {
            GameSaveData data = GameSaveData.CreateDefault();
            CampaignSaveDataUtility.Normalize(data);
            data.version = 6;
            data.campaign.scheduledWaves.Add(new WaveSaveData
            {
                sequence = 1,
                gateId = GateId.West,
                arrivalStrategicSeconds = 12d,
                threatPower = 10,
                status = WaveStatus.AwaitingResolution,
                raiders = 10,
                warningStage = WaveWarningStage.Critical,
                battleReport = null
            });
            data.campaign.nextWaveSequence = 2;
            PlayerPrefs.SetString(
                GameSaveRepository.SaveKey,
                JsonUtility.ToJson(data)
            );

            GameSaveData loaded = new GameSaveRepository().Load();

            Assert.That(loaded.version, Is.EqualTo(7));
            Assert.That(loaded.campaign.scheduledWaves, Has.Count.EqualTo(1));
            WaveSaveData wave = loaded.campaign.scheduledWaves[0];
            Assert.That(wave.status, Is.EqualTo(WaveStatus.AwaitingResolution));
            Assert.That(wave.battleReport, Is.Not.Null);
            Assert.That(wave.battleReport.resolved, Is.False);
            Assert.That(loaded.campaign.outcome,
                Is.EqualTo(CampaignOutcome.Ongoing));
        }

        [Test]
        public void CorruptCampaign_NormalizesGatesAndWaveList()
        {
            GameSaveData data = GameSaveData.CreateDefault();
            data.campaign.elapsedStrategicSeconds = -5d;
            data.campaign.randomSeed = 0;
            data.campaign.nextWaveSequence = 1;
            data.campaign.gates = new List<GateSaveData>
            {
                new()
                {
                    gateId = GateId.North,
                    currentHealth = -1,
                    warriors = -2
                },
                new()
                {
                    gateId = GateId.North,
                    currentHealth = 70,
                    warriors = 4
                }
            };
            data.campaign.scheduledWaves = new List<WaveSaveData>
            {
                null,
                new()
                {
                    sequence = -1,
                    threatPower = 0
                },
                new()
                {
                    sequence = 3,
                    gateId = GateId.East,
                    arrivalStrategicSeconds = 50d,
                    threatPower = 10,
                    raiders = 10,
                    status = WaveStatus.Scheduled
                }
            };

            new GameSaveRepository().Save(data);
            GameSaveData loaded = new GameSaveRepository().Load();

            Assert.That(
                loaded.campaign.elapsedStrategicSeconds,
                Is.Zero
            );
            Assert.That(loaded.campaign.randomSeed, Is.Not.Zero);
            Assert.That(loaded.campaign.gates.Count, Is.EqualTo(4));
            Assert.That(
                loaded.campaign.gates
                    .Select(gate => gate.gateId)
                    .Distinct()
                    .Count(),
                Is.EqualTo(4)
            );
            GateSaveData north = loaded.campaign.gates.Single(
                gate => gate.gateId == GateId.North
            );
            Assert.That(north.currentHealth, Is.EqualTo(70));
            Assert.That(north.warriors, Is.EqualTo(4));
            Assert.That(
                loaded.campaign.scheduledWaves.Count,
                Is.EqualTo(1)
            );
            Assert.That(loaded.campaign.nextWaveSequence, Is.EqualTo(4));
        }

        private static void AssertFourDefaultGates(
            IReadOnlyCollection<GateSaveData> gates
        )
        {
            Assert.That(gates.Count, Is.EqualTo(4));
            Assert.That(
                gates.Select(gate => gate.gateId).Distinct().Count(),
                Is.EqualTo(4)
            );
            Assert.That(
                gates.All(gate =>
                    gate.currentHealth == GateSaveData.DefaultHealth &&
                    gate.warriors == 0 &&
                    gate.archers == 0 &&
                    gate.mages == 0),
                Is.True
            );
        }
    }
}
