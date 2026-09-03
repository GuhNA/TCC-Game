using System;
using System.Linq;
using NUnit.Framework;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Tests.EditMode
{
    public sealed class CampaignServiceTests
    {
        [Test]
        public void NewCampaign_HasFourHealthyGatesAndFirstWave()
        {
            CampaignService service = CreateService(out _, out _);

            Assert.That(service.Gates.Count, Is.EqualTo(4));
            Assert.That(
                service.Gates
                    .Select(gate => gate.GateId)
                    .Distinct()
                    .Count(),
                Is.EqualTo(4)
            );
            Assert.That(
                service.Gates.All(gate =>
                    gate.CurrentHealth == GateSaveData.DefaultHealth),
                Is.True
            );
            Assert.That(service.Waves.CurrentWave, Is.Not.Null);
            Assert.That(
                service.Waves.CurrentWave.Value.ArrivalStrategicSeconds,
                Is.EqualTo(180d)
            );
        }

        [Test]
        public void Advance_RejectsInvalidDelta()
        {
            CampaignService service = CreateService(out _, out _);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => service.Advance(-1d)
            );
            Assert.Throws<ArgumentOutOfRangeException>(
                () => service.Advance(double.NaN)
            );
            Assert.Throws<ArgumentOutOfRangeException>(
                () => service.Advance(double.PositiveInfinity)
            );
        }

        [Test]
        public void MultiplePauseOwners_MustAllReleaseBeforeTimeAdvances()
        {
            CampaignService service = CreateService(out _, out _);
            var first = new object();
            var second = new object();

            Assert.That(service.AcquirePause(first), Is.True);
            Assert.That(service.AcquirePause(first), Is.False);
            Assert.That(service.AcquirePause(second), Is.True);
            service.Advance(5d);

            Assert.That(service.ElapsedStrategicSeconds, Is.Zero);
            Assert.That(service.ReleasePause(new object()), Is.False);
            Assert.That(service.ReleasePause(first), Is.True);
            service.Advance(5d);
            Assert.That(service.ElapsedStrategicSeconds, Is.Zero);

            Assert.That(service.ReleasePause(second), Is.True);
            service.Advance(5d);
            Assert.That(service.ElapsedStrategicSeconds, Is.EqualTo(5d));
        }

        [Test]
        public void Advance_CommitsDirtyStateAtConfiguredInterval()
        {
            CampaignService service = CreateService(
                out InMemoryRepository repository,
                out _,
                new CampaignRules(checkpointIntervalSeconds: 10d)
            );

            service.Advance(9d);
            Assert.That(repository.SaveCount, Is.Zero);

            service.Advance(1d);
            Assert.That(repository.SaveCount, Is.EqualTo(1));
            Assert.That(
                repository.Load().campaign.elapsedStrategicSeconds,
                Is.EqualTo(10d)
            );
        }

        [Test]
        public void Checkpoint_PersistsOnlyWhenStateIsDirty()
        {
            CampaignService service = CreateService(
                out InMemoryRepository repository,
                out _
            );

            service.Checkpoint();
            service.Checkpoint();

            Assert.That(repository.SaveCount, Is.EqualTo(1));

            service.Advance(1d);
            service.Checkpoint();
            Assert.That(repository.SaveCount, Is.EqualTo(2));
        }

        private static CampaignService CreateService(
            out InMemoryRepository repository,
            out GameSaveData data,
            CampaignRules rules = null
        )
        {
            data = GameSaveData.CreateDefault();
            data.campaign.randomSeed = 12345;
            repository = new InMemoryRepository(data);
            InMemoryRepository capturedRepository = repository;
            GameSaveData runtimeData = repository.Load();
            return new CampaignService(
                runtimeData,
                rules ?? new CampaignRules(),
                () => capturedRepository.Save(runtimeData)
            );
        }

        private sealed class InMemoryRepository : IGameSaveRepository
        {
            private GameSaveData data;

            public int SaveCount { get; private set; }

            public InMemoryRepository(GameSaveData data)
            {
                this.data = data.Clone();
            }

            public GameSaveData Load() => data.Clone();

            public void Save(GameSaveData saveData)
            {
                data = saveData.Clone();
                SaveCount++;
            }
        }
    }
}
