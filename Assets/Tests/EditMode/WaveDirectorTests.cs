using System.Collections.Generic;
using NUnit.Framework;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Tests.EditMode
{
    public sealed class WaveDirectorTests
    {
        [Test]
        public void WarningsAndArrival_FireOnceInOrder()
        {
            CampaignService service = CreateService(out SaveCounter saves);
            var stages = new List<WaveWarningStage>();
            int dueCount = 0;
            service.Waves.WaveWarningChanged +=
                (_, stage) => stages.Add(stage);
            service.Waves.WaveBecameDue += _ => dueCount++;

            service.Advance(90d);
            service.Advance(60d);
            service.Advance(20d);
            service.Advance(10d);
            service.Advance(1d);

            Assert.That(stages, Is.EqualTo(new[]
            {
                WaveWarningStage.Announced,
                WaveWarningStage.Imminent,
                WaveWarningStage.Critical
            }));
            Assert.That(dueCount, Is.EqualTo(1));
            Assert.That(
                service.Waves.CurrentWave.Value.Status,
                Is.EqualTo(WaveStatus.AwaitingResolution)
            );
            Assert.That(saves.Count, Is.EqualTo(4));
        }

        [Test]
        public void LargeTimeJump_EmitsAllStagesThenDueWithOneCommit()
        {
            CampaignService service = CreateService(out SaveCounter saves);
            var stages = new List<WaveWarningStage>();
            int dueCount = 0;
            service.Waves.WaveWarningChanged +=
                (_, stage) => stages.Add(stage);
            service.Waves.WaveBecameDue += _ => dueCount++;

            service.Advance(180d);

            Assert.That(stages, Is.EqualTo(new[]
            {
                WaveWarningStage.Announced,
                WaveWarningStage.Imminent,
                WaveWarningStage.Critical
            }));
            Assert.That(dueCount, Is.EqualTo(1));
            Assert.That(saves.Count, Is.EqualTo(1));
        }

        [Test]
        public void PausedClock_DoesNotApproachWave()
        {
            CampaignService service = CreateService(out _);
            var owner = new object();
            service.AcquirePause(owner);

            service.Advance(180d);

            Assert.That(service.ElapsedStrategicSeconds, Is.Zero);
            Assert.That(
                service.Waves.CurrentWave.Value.Status,
                Is.EqualTo(WaveStatus.Scheduled)
            );
        }

        [Test]
        public void ReloadedWarningStage_IsNotPublishedAgain()
        {
            GameSaveData state = GameSaveData.CreateDefault();
            state.campaign.randomSeed = 9981;
            var saves = new SaveCounter();
            CampaignService firstSession = new CampaignService(
                state,
                new CampaignRules(checkpointIntervalSeconds: 1000d),
                () => saves.Save(state)
            );
            firstSession.Advance(90d);

            CampaignService reloadedSession = new CampaignService(
                state,
                new CampaignRules(checkpointIntervalSeconds: 1000d),
                () => saves.Save(state)
            );
            var stages = new List<WaveWarningStage>();
            reloadedSession.Waves.WaveWarningChanged +=
                (_, stage) => stages.Add(stage);

            reloadedSession.Advance(60d);

            Assert.That(stages, Is.EqualTo(new[]
            {
                WaveWarningStage.Imminent
            }));
        }

        [Test]
        public void AwaitingWave_IsNotReplacedByTimeAdvancing()
        {
            CampaignService service = CreateService(out _);
            service.Advance(180d);

            service.Advance(1000d);

            Assert.That(service.Waves.CurrentWave.Value.Sequence, Is.EqualTo(1));
            Assert.That(
                service.Waves.CurrentWave.Value.Status,
                Is.EqualTo(WaveStatus.AwaitingResolution)
            );
        }

        [Test]
        public void ResolveCurrentWave_RecordsGateAndSchedulesNext()
        {
            CampaignService service = CreateService(out SaveCounter saves);
            service.Advance(180d);
            WaveSnapshot first = service.Waves.CurrentWave.Value;
            int scheduledCount = 0;
            service.Waves.WaveScheduled += _ => scheduledCount++;

            bool resolved = service.MarkWaveResolved(first.Sequence);
            WaveSnapshot next = service.Waves.CurrentWave.Value;

            Assert.That(resolved, Is.True);
            Assert.That(next.Sequence, Is.EqualTo(2));
            Assert.That(
                next.ArrivalStrategicSeconds,
                Is.InRange(330d, 420d)
            );
            Assert.That(
                service.GetGate(first.GateId)
                    .LastResolvedWaveSequence,
                Is.EqualTo(1)
            );
            Assert.That(scheduledCount, Is.EqualTo(1));
            Assert.That(saves.Count, Is.EqualTo(2));
        }

        private static CampaignService CreateService(out SaveCounter saves)
        {
            GameSaveData state = GameSaveData.CreateDefault();
            state.campaign.randomSeed = 9981;
            saves = new SaveCounter();
            SaveCounter captured = saves;
            return new CampaignService(
                state,
                new CampaignRules(checkpointIntervalSeconds: 1000d),
                () => captured.Save(state)
            );
        }

        private sealed class SaveCounter
        {
            public int Count { get; private set; }

            public void Save(GameSaveData state)
            {
                state.Clone();
                Count++;
            }
        }
    }
}
