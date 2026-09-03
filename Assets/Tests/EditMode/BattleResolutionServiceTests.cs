using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Tests.EditMode
{
    public sealed class BattleResolutionServiceTests
    {
        [Test]
        public void Resolution_IsAtomicAndIdempotent()
        {
            GameSaveData data = CreatePendingWave();
            data.campaign.gates[0].warriors = 20;
            var repository = new MemoryRepository(data);
            GameSession session = CreateSession(repository);
            int beforeMoney = session.Economy.Money;
            int eventCount = 0;
            session.Battles.BattleResolved += _ => eventCount++;

            bool first = session.Battles.TryResolvePending();
            bool second = session.Battles.TryResolvePending();
            BattleReportSnapshot report = session.Campaign
                .GetLatestBattleReport(GateId.North).Value;

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(repository.SaveCount, Is.EqualTo(1));
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(
                session.Economy.Money,
                Is.EqualTo(beforeMoney + report.Spoils)
            );
            Assert.That(
                session.Campaign.GetGate(GateId.North).Warriors,
                Is.EqualTo(report.SurvivingWarriors)
            );
            Assert.That(
                session.Campaign.Waves.CurrentWave.Value.Sequence,
                Is.EqualTo(2)
            );
        }

        [Test]
        public void Acknowledgement_PersistsAndClearsPendingReport()
        {
            var repository = new MemoryRepository(CreatePendingWave());
            GameSession session = CreateSession(repository);
            session.Battles.TryResolvePending();
            BattleReportSnapshot report = session.Campaign
                .GetLatestBattleReport(GateId.North, true).Value;

            bool acknowledged = session.Battles.AcknowledgeReport(
                report.WaveSequence
            );
            GameSession reloaded = CreateSession(repository);

            Assert.That(acknowledged, Is.True);
            Assert.That(repository.SaveCount, Is.EqualTo(2));
            Assert.That(
                reloaded.Campaign.GetLatestBattleReport(GateId.North, true),
                Is.Null
            );
            Assert.That(
                reloaded.Campaign.GetLatestBattleReport(GateId.North).Value
                    .ReportAcknowledged,
                Is.True
            );
        }

        [Test]
        public void Breach_EndsCampaignWithoutSchedulingNextWave()
        {
            GameSaveData data = CreatePendingWave();
            data.campaign.gates[0].currentHealth = 5;
            var repository = new MemoryRepository(data);
            GameSession session = CreateSession(repository);

            session.Battles.TryResolvePending();

            Assert.That(
                session.Campaign.Outcome,
                Is.EqualTo(CampaignOutcome.Defeat)
            );
            Assert.That(session.Campaign.GetGate(GateId.North).CurrentHealth,
                Is.Zero);
            Assert.That(session.Campaign.Waves.CurrentWave, Is.Null);
        }

        [Test]
        public void TenthVictory_EndsCampaignAndNeverCreatesWaveEleven()
        {
            GameSaveData data = CreatePendingWave(sequence: 10);
            data.campaign.gates[0].warriors = 100;
            var repository = new MemoryRepository(data);
            GameSession session = CreateSession(repository);

            session.Battles.TryResolvePending();

            Assert.That(
                session.Campaign.Outcome,
                Is.EqualTo(CampaignOutcome.Victory)
            );
            Assert.That(session.Campaign.Waves.CurrentWave, Is.Null);
            Assert.That(
                repository.Latest.campaign.scheduledWaves
                    .Any(wave => wave.sequence == 11),
                Is.False
            );
        }

        [Test]
        public void CommitFailure_RestoresAllBattleMutations()
        {
            GameSaveData data = CreatePendingWave();
            data.campaign.gates[0].warriors = 10;
            var repository = new MemoryRepository(data);
            GameSession session = CreateSession(repository);
            repository.ThrowOnSave = true;
            int money = session.Economy.Money;

            Assert.Throws<InvalidOperationException>(
                () => session.Battles.TryResolvePending()
            );

            Assert.That(session.Economy.Money, Is.EqualTo(money));
            Assert.That(session.Campaign.GetGate(GateId.North).Warriors,
                Is.EqualTo(10));
            Assert.That(session.Campaign.GetGate(GateId.North).CurrentHealth,
                Is.EqualTo(100));
            Assert.That(
                session.Campaign.Waves.CurrentWave.Value.Status,
                Is.EqualTo(WaveStatus.AwaitingResolution)
            );
            Assert.That(
                session.Campaign.GetLatestBattleReport(GateId.North),
                Is.Null
            );
        }

        [Test]
        public void RepeatedResolution_OneThousandCallsApplyOnlyOnce()
        {
            var repository = new MemoryRepository(CreatePendingWave());
            GameSession session = CreateSession(repository);
            int successes = 0;

            for (int index = 0; index < 1000; index++)
            {
                if (session.Battles.TryResolvePending())
                {
                    successes++;
                }
            }

            Assert.That(successes, Is.EqualTo(1));
            Assert.That(repository.SaveCount, Is.EqualTo(1));
            Assert.That(
                repository.Latest.campaign.scheduledWaves.Count(
                    wave => wave.battleReport != null &&
                        wave.battleReport.resolved
                ),
                Is.EqualTo(1)
            );
        }

        [Test]
        public void MoneyEvent_IsPublishedAfterCommittedFinalBalance()
        {
            var repository = new MemoryRepository(CreatePendingWave());
            GameSession session = CreateSession(repository);
            int publishedMoney = -1;
            int persistedMoneyWhenPublished = -1;
            session.Economy.MoneyChanged += money =>
            {
                publishedMoney = money;
                persistedMoneyWhenPublished = repository.Latest.money;
            };

            session.Battles.TryResolvePending();

            Assert.That(publishedMoney, Is.EqualTo(session.Economy.Money));
            Assert.That(persistedMoneyWhenPublished,
                Is.EqualTo(session.Economy.Money));
        }

        [Test]
        public void AcknowledgingTwice_CommitsOnlyFirstChange()
        {
            var repository = new MemoryRepository(CreatePendingWave());
            GameSession session = CreateSession(repository);
            session.Battles.TryResolvePending();
            int sequence = session.Campaign
                .GetLatestBattleReport(GateId.North).Value.WaveSequence;

            bool first = session.Battles.AcknowledgeReport(sequence);
            bool second = session.Battles.AcknowledgeReport(sequence);

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(repository.SaveCount, Is.EqualTo(2));
        }

        [Test]
        public void ObserverFailure_CannotDivergeMemoryFromCommittedSave()
        {
            var repository = new MemoryRepository(CreatePendingWave());
            GameSession session = CreateSession(repository);
            session.Battles.BattleResolved += _ =>
                throw new InvalidOperationException("Broken UI observer.");
            Exception observed = null;

            try
            {
                session.Battles.TryResolvePending();
            }
            catch (Exception exception)
            {
                observed = exception;
            }

            BattleReportSnapshot? memoryReport = session.Campaign
                .GetLatestBattleReport(GateId.North);
            GameSession reloaded = CreateSession(repository);
            BattleReportSnapshot? persistedReport = reloaded.Campaign
                .GetLatestBattleReport(GateId.North);

            var problems = new List<string>();
            if (observed != null)
            {
                problems.Add(
                    "Falha de apresentação escapou da transação de domínio."
                );
            }
            if (!memoryReport.HasValue)
            {
                problems.Add("A memória desfez um resultado que já foi salvo.");
            }
            if (!persistedReport.HasValue)
            {
                problems.Add("O resultado não permaneceu persistido.");
            }
            if (session.Economy.Money != reloaded.Economy.Money)
            {
                problems.Add("Saldo em memória divergiu do saldo persistido.");
            }

            Assert.That(problems, Is.Empty, string.Join(" ", problems));
        }

        [Test]
        public void TerminalCampaign_DoesNotAdvanceStrategicClock()
        {
            GameSaveData data = CreatePendingWave(sequence: 10);
            data.campaign.gates[0].warriors = 100;
            var repository = new MemoryRepository(data);
            GameSession session = CreateSession(repository);
            session.Battles.TryResolvePending();
            double stoppedAt = session.Campaign.ElapsedStrategicSeconds;

            session.Campaign.Advance(10000d);

            Assert.That(session.Campaign.Outcome,
                Is.EqualTo(CampaignOutcome.Victory));
            Assert.That(session.Campaign.ElapsedStrategicSeconds,
                Is.EqualTo(stoppedAt));
        }

        private static GameSession CreateSession(MemoryRepository repository) =>
            new(repository, new FixedClock());

        private static GameSaveData CreatePendingWave(int sequence = 1)
        {
            GameSaveData data = GameSaveData.CreateDefault();
            data.recruitment.initialized = true;
            data.recruitment.nextRestockUnixSeconds = 10000;
            CampaignSaveDataUtility.Normalize(data);
            data.campaign.scheduledWaves.Clear();
            data.campaign.scheduledWaves.Add(new WaveSaveData
            {
                sequence = sequence,
                gateId = GateId.North,
                arrivalStrategicSeconds = 0d,
                threatPower = 10,
                status = WaveStatus.AwaitingResolution,
                raiders = 10,
                warningStage = WaveWarningStage.Critical
            });
            data.campaign.nextWaveSequence = sequence + 1;
            return data;
        }

        private sealed class FixedClock : IClock
        {
            public long UnixSeconds => 1000;
        }

        private sealed class MemoryRepository : IGameSaveRepository
        {
            private GameSaveData data;

            public int SaveCount { get; private set; }
            public bool ThrowOnSave { get; set; }
            public GameSaveData Latest => data.Clone();

            public MemoryRepository(GameSaveData data)
            {
                this.data = data.Clone();
            }

            public GameSaveData Load() => data.Clone();

            public void Save(GameSaveData saveData)
            {
                if (ThrowOnSave)
                {
                    throw new InvalidOperationException("Simulated failure.");
                }

                data = saveData.Clone();
                SaveCount++;
            }
        }
    }
}
