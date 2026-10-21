using System.Linq;
using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using TCCGame.Gameplay.City;
using TCCGame.Gameplay.Player;
using TCCGame.Gameplay.Recruitment;
using TCCGame.Gameplay.Exploration;
using UnityEngine;

namespace TCCGame.Tests.EditMode
{
    public class GameServicesTests
    {
        private BuildingDefinition buildingDefinition;

        [SetUp]
        public void SetUp()
        {
            buildingDefinition =
                ScriptableObject.CreateInstance<BuildingDefinition>();
            buildingDefinition.type = BuildingType.Barracks;
            buildingDefinition.maximumLevel = 10;
            buildingDefinition.baseUpgradeCost = 100;
            buildingDefinition.upgradeCostMultiplier = 1.5f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(buildingDefinition);
            GameSessionProvider.Reset();
        }

        [Test]
        public void BuildingUpgrade_DebitsAndPersistsAsOneTransaction()
        {
            GameSaveData data = CreateInitializedData();
            data.money = 100;
            var repository = new InMemoryGameSaveRepository(data);
            GameSession session = CreateSession(repository);
            BuildingType changedType = default;
            int previousLevel = -1;
            int newLevel = -1;
            session.Buildings.LevelChanged += (type, previous, current) =>
            {
                changedType = type;
                previousLevel = previous;
                newLevel = current;
            };

            BuildingUpgradeStatus result =
                session.Buildings.TryUpgrade(buildingDefinition);

            Assert.That(result, Is.EqualTo(
                BuildingUpgradeStatus.Upgraded
            ));
            Assert.That(session.Economy.Money, Is.Zero);
            Assert.That(
                session.Buildings.GetLevel(BuildingType.Barracks),
                Is.EqualTo(1)
            );
            Assert.That(repository.SaveCount, Is.EqualTo(1));
            Assert.That(changedType, Is.EqualTo(BuildingType.Barracks));
            Assert.That(previousLevel, Is.Zero);
            Assert.That(newLevel, Is.EqualTo(1));
        }

        [Test]
        public void FailedBuildingUpgrade_DoesNotMutateOrPersist()
        {
            GameSaveData data = CreateInitializedData();
            data.money = 99;
            var repository = new InMemoryGameSaveRepository(data);
            GameSession session = CreateSession(repository);

            BuildingUpgradeStatus result =
                session.Buildings.TryUpgrade(buildingDefinition);

            Assert.That(result, Is.EqualTo(
                BuildingUpgradeStatus.InsufficientMoney
            ));
            Assert.That(session.Economy.Money, Is.EqualTo(99));
            Assert.That(
                session.Buildings.GetLevel(BuildingType.Barracks),
                Is.Zero
            );
            Assert.That(repository.SaveCount, Is.Zero);
        }

        [Test]
        public void ExplorerBuildingUpgrade_ConsumesGoldAndMapResourcesAtomically()
        {
            ExplorationResourceDefinition wood =
                ScriptableObject.CreateInstance<
                    ExplorationResourceDefinition>();
            wood.name = "resource.wood";
            buildingDefinition.type = BuildingType.ExplorersLodge;
            buildingDefinition.levelUpgradeCosts = new[]
            {
                new BuildingUpgradeLevelCost
                {
                    goldCost = 100,
                    resources = new[]
                    {
                        new BuildingResourceRequirement
                        {
                            resource = wood,
                            quantity = 5
                        }
                    }
                }
            };
            GameSaveData data = CreateInitializedData();
            data.money = 100;
            data.playerInventory.items.Add(new InventoryItemSaveData
            {
                itemId = wood.PersistenceId,
                quantity = 5
            });
            var repository = new InMemoryGameSaveRepository(data);
            GameSession session = CreateSession(repository);

            BuildingUpgradeStatus result =
                session.Buildings.TryUpgrade(buildingDefinition);

            Assert.That(result, Is.EqualTo(BuildingUpgradeStatus.Upgraded));
            Assert.That(session.Economy.Money, Is.Zero);
            Assert.That(session.Inventory.GetItemQuantity(wood.PersistenceId),
                Is.Zero);
            Assert.That(session.Buildings.GetLevel(
                BuildingType.ExplorersLodge), Is.EqualTo(1));
            Assert.That(repository.SaveCount, Is.EqualTo(1));
            Object.DestroyImmediate(wood);
        }

        [Test]
        public void ExplorerBuildingUpgrade_WithMissingResourceChangesNothing()
        {
            ExplorationResourceDefinition wood =
                ScriptableObject.CreateInstance<
                    ExplorationResourceDefinition>();
            wood.name = "resource.wood";
            buildingDefinition.type = BuildingType.ExplorersLodge;
            buildingDefinition.levelUpgradeCosts = new[]
            {
                new BuildingUpgradeLevelCost
                {
                    goldCost = 100,
                    resources = new[]
                    {
                        new BuildingResourceRequirement
                        {
                            resource = wood,
                            quantity = 5
                        }
                    }
                }
            };
            GameSaveData data = CreateInitializedData();
            data.money = 100;
            var repository = new InMemoryGameSaveRepository(data);
            GameSession session = CreateSession(repository);

            BuildingUpgradeStatus result =
                session.Buildings.TryUpgrade(buildingDefinition);

            Assert.That(result,
                Is.EqualTo(BuildingUpgradeStatus.InsufficientResources));
            Assert.That(session.Economy.Money, Is.EqualTo(100));
            Assert.That(session.Buildings.GetLevel(
                BuildingType.ExplorersLodge), Is.Zero);
            Assert.That(repository.SaveCount, Is.Zero);
            Object.DestroyImmediate(wood);
        }

        [Test]
        public void ExplorerAssignments_CannotExceedExplorerBuildingLevel()
        {
            GameSaveData data = CreateInitializedData();
            data.SetBuildingLevel(BuildingType.ExplorersLodge, 2);
            data.exploration.areas.Add(new ExplorationAreaSaveData
            {
                environment = ExplorationEnvironment.ColinasDoVento,
                captured = true
            });
            data.exploration.areas.Add(new ExplorationAreaSaveData
            {
                environment = ExplorationEnvironment.FlorestaDasSombras,
                captured = true
            });
            var repository = new InMemoryGameSaveRepository(data);
            GameSession session = CreateSession(repository);

            bool first = session.Exploration.TrySetAssignedWorkers(
                ExplorationEnvironment.ColinasDoVento,
                2,
                out string firstError
            );
            bool second = session.Exploration.TrySetAssignedWorkers(
                ExplorationEnvironment.FlorestaDasSombras,
                1,
                out string secondError
            );

            Assert.That(first, Is.True, firstError);
            Assert.That(second, Is.False);
            Assert.That(secondError, Does.Contain("Construção de Exploradores"));
            Assert.That(session.Exploration.TotalExplorers, Is.EqualTo(2));
            Assert.That(session.Exploration.AvailableExplorers, Is.Zero);
        }

        [Test]
        public void RecruitmentPurchase_ChargesQuantityTimesUnitPrice()
        {
            GameSaveData data = CreateInitializedData();
            data.money = 100;
            data.recruitment.availableArchers = 20;
            var repository = new InMemoryGameSaveRepository(data);
            GameSession session = CreateSession(repository);

            RecruitmentPurchaseResult result =
                session.Recruitment.TryPurchase(UnitType.Archer, 10);

            Assert.That(result.Status, Is.EqualTo(
                RecruitmentPurchaseStatus.Purchased
            ));
            Assert.That(result.TotalCost, Is.EqualTo(50));
            Assert.That(session.Economy.Money, Is.EqualTo(50));
            Assert.That(
                session.Recruitment.GetAvailable(UnitType.Archer),
                Is.EqualTo(10)
            );
            Assert.That(
                session.Recruitment.GetOwned(UnitType.Archer),
                Is.EqualTo(10)
            );
            Assert.That(repository.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void RecruitmentPurchase_WithInsufficientMoneyIsAtomic()
        {
            GameSaveData data = CreateInitializedData();
            data.money = 0;
            data.recruitment.availableMages = 5;
            var repository = new InMemoryGameSaveRepository(data);
            GameSession session = CreateSession(repository);

            RecruitmentPurchaseResult result =
                session.Recruitment.TryPurchase(UnitType.Mage, 1);

            Assert.That(result.Status, Is.EqualTo(
                RecruitmentPurchaseStatus.InsufficientMoney
            ));
            Assert.That(session.Economy.Money, Is.Zero);
            Assert.That(
                session.Recruitment.GetAvailable(UnitType.Mage),
                Is.EqualTo(5)
            );
            Assert.That(
                session.Recruitment.GetOwned(UnitType.Mage),
                Is.Zero
            );
            Assert.That(repository.SaveCount, Is.Zero);
        }

        [Test]
        public void AdventurerRecruitment_DebitsPersistsAndRejectsDuplicate()
        {
            GameSaveData data = CreateInitializedData();
            data.money = 200;
            var repository = new InMemoryGameSaveRepository(data);
            GameSession session = CreateSession(repository);

            AdventurerRecruitmentStatus first =
                session.Inventory.TryRecruitAdventurer(
                    "adventurer.archer",
                    150
                );
            AdventurerRecruitmentStatus duplicate =
                session.Inventory.TryRecruitAdventurer(
                    "adventurer.archer",
                    150
                );

            Assert.That(first, Is.EqualTo(
                AdventurerRecruitmentStatus.Recruited
            ));
            Assert.That(duplicate, Is.EqualTo(
                AdventurerRecruitmentStatus.AlreadyRecruited
            ));
            Assert.That(session.Economy.Money, Is.EqualTo(50));
            Assert.That(
                session.Inventory.IsAdventurerRecruited(
                    "adventurer.archer"
                ),
                Is.True
            );
            Assert.That(repository.SaveCount, Is.EqualTo(1));

            GameSession reloaded = CreateSession(repository);

            Assert.That(
                reloaded.Inventory.IsAdventurerRecruited(
                    "adventurer.archer"
                ),
                Is.True
            );
            Assert.That(reloaded.Economy.Money, Is.EqualTo(50));
        }

        [Test]
        public void AdventurerRecruitment_WithInsufficientMoneyIsAtomic()
        {
            GameSaveData data = CreateInitializedData();
            data.money = 149;
            var repository = new InMemoryGameSaveRepository(data);
            GameSession session = CreateSession(repository);

            AdventurerRecruitmentStatus result =
                session.Inventory.TryRecruitAdventurer(
                    "adventurer.archer",
                    150
                );

            Assert.That(result, Is.EqualTo(
                AdventurerRecruitmentStatus.InsufficientMoney
            ));
            Assert.That(session.Economy.Money, Is.EqualTo(149));
            Assert.That(
                session.Inventory.RecruitedAdventurerIds,
                Is.Empty
            );
            Assert.That(repository.SaveCount, Is.Zero);
        }

        [Test]
        public void InventoryItems_AreAggregatedAndPersisted()
        {
            GameSaveData data = CreateInitializedData();
            var repository = new InMemoryGameSaveRepository(data);
            GameSession session = CreateSession(repository);

            bool firstAdded = session.Inventory.TryAddItem(
                "item.health-potion",
                2
            );
            bool secondAdded = session.Inventory.TryAddItem(
                "item.health-potion"
            );

            Assert.That(firstAdded, Is.True);
            Assert.That(secondAdded, Is.True);
            Assert.That(
                session.Inventory.GetItemQuantity("item.health-potion"),
                Is.EqualTo(3)
            );
            Assert.That(repository.SaveCount, Is.EqualTo(2));

            GameSession reloaded = CreateSession(repository);

            Assert.That(
                reloaded.Inventory.GetItemQuantity("item.health-potion"),
                Is.EqualTo(3)
            );
        }

        [TestCase(0, 10)]
        [TestCase(1, 13)]
        [TestCase(2, 16)]
        [TestCase(3, 19)]
        public void RestockCapacity_DependsOnlyOnBuildingLevel(
            int buildingLevel,
            int expectedCapacity
        )
        {
            Assert.That(
                RecruitmentService.CalculateCapacity(buildingLevel),
                Is.EqualTo(expectedCapacity)
            );
        }

        [Test]
        public void Restock_UsesUtcClockAndDoesNotCompoundPreviousStock()
        {
            GameSaveData data = CreateInitializedData();
            data.SetBuildingLevel(BuildingType.ArcherStand, 2);
            data.recruitment.availableArchers = 500;
            data.recruitment.nextRestockUnixSeconds = 1000;
            var repository = new InMemoryGameSaveRepository(data);
            var clock = new FakeClock { UnixSeconds = 1000 };
            var session = new GameSession(repository, clock);

            bool refreshed = session.Recruitment.RefreshStockIfDue();

            Assert.That(refreshed, Is.True);
            Assert.That(
                session.Recruitment.GetAvailable(UnitType.Archer),
                Is.EqualTo(16)
            );
            Assert.That(
                session.Recruitment.GetTimeUntilRestock(),
                Is.EqualTo(System.TimeSpan.FromHours(6))
            );
            Assert.That(repository.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void TavernWave_StartsOnEntryAndOnlyRefreshesAfterExpiry()
        {
            GameSaveData data = CreateInitializedData();
            var repository = new InMemoryGameSaveRepository(data);
            var clock = new FakeClock { UnixSeconds = 100 };
            var session = new GameSession(repository, clock);
            string[] candidates =
            {
                "adventurer.archer",
                "adventurer.mage",
                "adventurer.bandit",
                "adventurer.warrior"
            };

            var firstWave = session.TavernRecruitment.EnterTavern(
                candidates,
                2,
                120
            ).ToArray();
            clock.UnixSeconds = 150;
            var sameWave = session.TavernRecruitment.EnterTavern(
                candidates,
                2,
                120
            ).ToArray();

            Assert.That(firstWave.Length, Is.EqualTo(2));
            CollectionAssert.AreEqual(firstWave, sameWave);
            Assert.That(session.TavernRecruitment.RemainingSeconds,
                Is.EqualTo(70));
            Assert.That(repository.SaveCount, Is.EqualTo(1));

            clock.UnixSeconds = 220;
            Assert.That(session.TavernRecruitment.IsCurrentWaveExpired,
                Is.True);

            // Esta chamada representa uma nova entrada na cena da Taverna.
            var refreshedWave = session.TavernRecruitment.EnterTavern(
                candidates,
                2,
                120
            );
            Assert.That(refreshedWave.Count, Is.EqualTo(2));
            Assert.That(session.TavernRecruitment.RemainingSeconds,
                Is.EqualTo(120));
            Assert.That(repository.SaveCount, Is.EqualTo(2));
        }

        private static GameSession CreateSession(
            InMemoryGameSaveRepository repository
        )
        {
            return new GameSession(
                repository,
                new FakeClock { UnixSeconds = 1000 }
            );
        }

        private static GameSaveData CreateInitializedData()
        {
            GameSaveData data = GameSaveData.CreateDefault();
            data.recruitment.initialized = true;
            data.recruitment.nextRestockUnixSeconds =
                1000 + RecruitmentService.RestockIntervalSeconds;
            return data;
        }

        private sealed class FakeClock : IClock
        {
            public long UnixSeconds { get; set; }
        }

        private sealed class InMemoryGameSaveRepository
            : IGameSaveRepository
        {
            private GameSaveData data;

            public int SaveCount { get; private set; }

            public InMemoryGameSaveRepository(GameSaveData data)
            {
                this.data = data.Clone();
            }

            public GameSaveData Load()
            {
                return data.Clone();
            }

            public void Save(GameSaveData saveData)
            {
                data = saveData.Clone();
                SaveCount++;
            }
        }
    }
}
