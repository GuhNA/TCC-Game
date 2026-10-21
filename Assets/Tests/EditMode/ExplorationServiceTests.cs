using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using TCCGame.Gameplay.Exploration;
using UnityEngine;

namespace TCCGame.Tests.EditMode
{
    public sealed class ExplorationServiceTests
    {
        [Test]
        public void Victory_HoldsRewardsUntilPartyReturns()
        {
            var state = GameSaveData.CreateDefault();
            var clock = new FakeClock { UnixSeconds = 100 };
            var mission = new ExplorationMissionSaveData
            {
                id = 1,
                environment = ExplorationEnvironment.ColinasDoVento,
                status = ExplorationMissionStatus.InBattle
            };
            state.exploration.missions.Add(mission);
            var service = new ExplorationService(state, clock, () => { });

            service.CompleteVictory(1, new[]
            {
                new ExplorationRewardSaveData
                {
                    resourceId = "resource.wood.light",
                    displayName = "Madeira leve",
                    quantity = 3
                }
            });

            Assert.That(mission.status,
                Is.EqualTo(ExplorationMissionStatus.Returning));
            Assert.That(state.playerInventory.items, Is.Empty);

            clock.UnixSeconds += ExplorationService.ReturnSeconds;
            service.RefreshArrivals();

            Assert.That(mission.status,
                Is.EqualTo(ExplorationMissionStatus.Returned));
            Assert.That(state.playerInventory.items[0].quantity, Is.EqualTo(3));
        }

        [Test]
        public void CapturedArea_WithGuardCreatesDefenseWhenAttackIsDue()
        {
            var state = GameSaveData.CreateDefault();
            var clock = new FakeClock { UnixSeconds = 100 };
            state.exploration.missions.Add(new ExplorationMissionSaveData
            {
                id = 1,
                environment = ExplorationEnvironment.ColinasDoVento,
                status = ExplorationMissionStatus.InBattle
            });
            var service = new ExplorationService(state, clock, () => { });
            service.CompleteVictory(1, null);
            clock.UnixSeconds += ExplorationService.ReturnSeconds;
            service.RefreshArrivals();

            Char_base guard = ScriptableObject.CreateInstance<Char_base>();
            guard.name = "guard.test";
            guard.vida = 20;
            guard.dano_min = 2;
            guard.dano_max = 4;
            state.playerInventory.recruitedAdventurerIds.Add(
                guard.PersistenceId
            );
            Assert.That(service.TryAssignGuardParty(
                ExplorationEnvironment.ColinasDoVento,
                new[] { guard },
                out string error), Is.True, error);

            clock.UnixSeconds +=
                ExplorationService.EnemyCounterAttackSeconds;
            service.RefreshArrivals();

            ExplorationMissionSaveData defense = service.GetActiveMission();
            Assert.That(defense, Is.Not.Null);
            Assert.That(defense.kind,
                Is.EqualTo(ExplorationMissionKind.Defense));
            Assert.That(defense.status,
                Is.EqualTo(ExplorationMissionStatus.ReadyForBattle));
            Object.DestroyImmediate(guard);
        }

        [Test]
        public void UndefendedArea_LosesExplorersAndMustBeRecaptured()
        {
            var state = GameSaveData.CreateDefault();
            var clock = new FakeClock { UnixSeconds = 100 };
            state.SetBuildingLevel(BuildingType.ExplorersLodge, 2);
            state.exploration.completedEnvironments.Add(
                ExplorationEnvironment.ColinasDoVento
            );
            state.exploration.areas.Add(new ExplorationAreaSaveData
            {
                environment = ExplorationEnvironment.ColinasDoVento,
                captured = true,
                assignedWorkers = 2,
                nextEnemyAttackUnixSeconds = 100
            });
            var service = new ExplorationService(state, clock, () => { });

            service.RefreshArrivals();

            ExplorationAreaSaveData area = service.GetArea(
                ExplorationEnvironment.ColinasDoVento
            );
            Assert.That(area.captured, Is.False);
            Assert.That(area.assignedWorkers, Is.Zero);
            Assert.That(service.LostExplorers, Is.EqualTo(2));
            Assert.That(service.TotalExplorers, Is.Zero);
            CollectionAssert.DoesNotContain(
                service.CompletedEnvironments,
                ExplorationEnvironment.ColinasDoVento
            );
            Assert.That(service.GetLatestMissionForEnvironment(
                    ExplorationEnvironment.ColinasDoVento).status,
                Is.EqualTo(ExplorationMissionStatus.Defeated));
        }

        private sealed class FakeClock : IClock
        {
            public long UnixSeconds { get; set; }
        }
    }
}
