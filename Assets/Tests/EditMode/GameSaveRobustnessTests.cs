using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;
using UnityEngine;

namespace TCCGame.Tests.EditMode
{
    public sealed class GameSaveRobustnessTests
    {
        private bool hadSave;
        private string previousSave;
        private bool hadLegacySave;
        private string previousLegacySave;
        private bool hadCorruptSave;
        private string previousCorruptSave;

        [SetUp]
        public void SetUp()
        {
            hadSave = PlayerPrefs.HasKey(GameSaveRepository.SaveKey);
            previousSave = hadSave
                ? PlayerPrefs.GetString(GameSaveRepository.SaveKey)
                : null;
            hadLegacySave = PlayerPrefs.HasKey(
                GameSaveRepository.LegacyCitySaveKey
            );
            previousLegacySave = hadLegacySave
                ? PlayerPrefs.GetString(GameSaveRepository.LegacyCitySaveKey)
                : null;
            hadCorruptSave = PlayerPrefs.HasKey(GameSaveRepository.CorruptSaveKey);
            previousCorruptSave = hadCorruptSave
                ? PlayerPrefs.GetString(GameSaveRepository.CorruptSaveKey)
                : null;
            PlayerPrefs.DeleteKey(GameSaveRepository.SaveKey);
            PlayerPrefs.DeleteKey(GameSaveRepository.LegacyCitySaveKey);
            PlayerPrefs.DeleteKey(GameSaveRepository.CorruptSaveKey);
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            RestoreKey(
                GameSaveRepository.SaveKey,
                hadSave,
                previousSave
            );
            RestoreKey(
                GameSaveRepository.LegacyCitySaveKey,
                hadLegacySave,
                previousLegacySave
            );
            RestoreKey(
                GameSaveRepository.CorruptSaveKey,
                hadCorruptSave,
                previousCorruptSave
            );
            PlayerPrefs.Save();
        }

        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void EarlierSchemaVersions_MigrateToV7(int version)
        {
            string json = $"{{\"version\":{version},\"money\":432}}";
            PlayerPrefs.SetString(GameSaveRepository.SaveKey, json);

            GameSaveData loaded = new GameSaveRepository().Load();

            Assert.That(loaded.version, Is.EqualTo(7));
            Assert.That(loaded.money, Is.EqualTo(432));
            Assert.That(loaded.campaign.gates, Has.Count.EqualTo(4));
            Assert.That(loaded.navigation, Is.Not.Null);
        }

        [Test]
        public void BattleReport_RoundTripsAllConsequences()
        {
            GameSaveData data = CreateResolvedData();
            var repository = new GameSaveRepository();

            repository.Save(data);
            GameSaveData loaded = repository.Load();
            WaveSaveData wave = loaded.campaign.scheduledWaves.Single();

            Assert.That(wave.status, Is.EqualTo(WaveStatus.Resolved));
            Assert.That(wave.battleReport.resolved, Is.True);
            Assert.That(wave.battleReport.outcome,
                Is.EqualTo(BattleOutcome.Repelled));
            Assert.That(wave.battleReport.effectiveDefense,
                Is.EqualTo(12.5d));
            Assert.That(wave.battleReport.initialWarriors, Is.EqualTo(10));
            Assert.That(wave.battleReport.lostWarriors, Is.EqualTo(2));
            Assert.That(wave.battleReport.gateDamage, Is.EqualTo(7));
            Assert.That(wave.battleReport.finalGateHealth, Is.EqualTo(93));
            Assert.That(wave.battleReport.spoils, Is.EqualTo(3));
            Assert.That(wave.battleReport.reportAcknowledged, Is.False);
        }

        [Test]
        public void InvalidCampaignValues_AreNormalizedWithoutInvalidEnums()
        {
            GameSaveData data = GameSaveData.CreateDefault();
            data.money = -10;
            data.army.warriors = -1;
            data.campaign.outcome = (CampaignOutcome)999;
            data.navigation.activeGateId = (GateId)999;
            data.campaign.gates = null;
            data.campaign.troopTransfers = null;
            data.campaign.scheduledWaves = null;

            var repository = new GameSaveRepository();
            repository.Save(data);
            GameSaveData loaded = repository.Load();

            Assert.That(loaded.money, Is.Zero);
            Assert.That(loaded.army.warriors, Is.Zero);
            Assert.That(loaded.campaign.outcome,
                Is.EqualTo(CampaignOutcome.Ongoing));
            Assert.That(loaded.campaign.gates, Has.Count.EqualTo(4));
            Assert.That(loaded.campaign.troopTransfers, Is.Not.Null);
            Assert.That(loaded.campaign.scheduledWaves, Is.Not.Null);
            Assert.That(Enum.IsDefined(
                typeof(GateId),
                loaded.navigation.activeGateId
            ), Is.True);
        }

        [TestCase("{")]
        [TestCase("not-json")]
        [TestCase("{\"version\":7,\"campaign\":")]
        public void MalformedCurrentSave_DoesNotPreventGameStartup(string json)
        {
            PlayerPrefs.SetString(GameSaveRepository.SaveKey, json);
            GameSaveData loaded = null;
            Exception observed = null;

            try
            {
                loaded = new GameSaveRepository().Load();
            }
            catch (Exception exception)
            {
                observed = exception;
            }

            var problems = new List<string>();
            if (observed != null)
            {
                problems.Add(
                    "Um save inválido impediu o jogo de iniciar: " +
                    observed.GetType().Name
                );
            }
            if (loaded == null)
            {
                problems.Add("Nenhum estado de recuperação foi carregado.");
            }
            else if (loaded.version != GameSaveData.CurrentVersion)
            {
                problems.Add("O estado recuperado não está na versão atual.");
            }

            Assert.That(problems, Is.Empty, string.Join(" ", problems));
            Assert.That(
                PlayerPrefs.GetString(GameSaveRepository.CorruptSaveKey),
                Is.EqualTo(json)
            );
        }

        [Test]
        public void SaveOperations_DoNotCreateAParallelCurrentKey()
        {
            new GameSaveRepository().Save(GameSaveData.CreateDefault());

            Assert.That(PlayerPrefs.HasKey(GameSaveRepository.SaveKey),
                Is.True);
            Assert.That(GameSaveRepository.SaveKey,
                Is.EqualTo("tcc.game.save.v2"));
        }

        private static GameSaveData CreateResolvedData()
        {
            GameSaveData data = GameSaveData.CreateDefault();
            CampaignSaveDataUtility.Normalize(data);
            data.campaign.scheduledWaves.Add(new WaveSaveData
            {
                sequence = 1,
                gateId = GateId.North,
                arrivalStrategicSeconds = 180d,
                threatPower = 20,
                status = WaveStatus.Resolved,
                raiders = 20,
                warningStage = WaveWarningStage.Critical,
                battleReport = new BattleReportSaveData
                {
                    resolved = true,
                    outcome = BattleOutcome.Repelled,
                    effectiveDefense = 12.5d,
                    defenseRatio = 0.625d,
                    initialGateHealth = 100,
                    gateDamage = 7,
                    finalGateHealth = 93,
                    initialWarriors = 10,
                    lostWarriors = 2,
                    spoils = 3,
                    reportAcknowledged = false
                }
            });
            data.campaign.nextWaveSequence = 2;
            return data;
        }

        private static void RestoreKey(
            string key,
            bool existed,
            string value
        )
        {
            if (existed)
            {
                PlayerPrefs.SetString(key, value);
            }
            else
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
    }
}
