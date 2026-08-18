using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using UnityEngine;

namespace TCCGame.Tests.EditMode
{
    public class BuildingCalculatorCharacterizationTests
    {
        private BuildingDefinition definition;

        [SetUp]
        public void SetUp()
        {
            definition = ScriptableObject.CreateInstance<BuildingDefinition>();
            definition.baseUpgradeCost = 100;
            definition.upgradeCostMultiplier = 1.5f;
            definition.baseProduction = 10;
            definition.productionPerLevel = 5;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(definition);
        }

        [TestCase(0, 100)]
        [TestCase(1, 150)]
        [TestCase(2, 225)]
        [TestCase(3, 338)]
        public void UpgradeCost_PreservesCurrentProgression(
            int currentLevel,
            int expectedCost
        )
        {
            Assert.That(
                BuildingCalculator.GetUpgradeCost(definition, currentLevel),
                Is.EqualTo(expectedCost)
            );
        }

        [TestCase(0, 0)]
        [TestCase(1, 10)]
        [TestCase(2, 15)]
        [TestCase(3, 20)]
        public void Production_PreservesCurrentProgression(
            int level,
            int expectedProduction
        )
        {
            Assert.That(
                BuildingCalculator.GetProduction(definition, level),
                Is.EqualTo(expectedProduction)
            );
        }
    }

    public class TaxCalculatorCharacterizationTests
    {
        [TestCase(10, 30d, 5d)]
        [TestCase(10, 60d, 10d)]
        [TestCase(15, 120d, 30d)]
        [TestCase(0, 120d, 0d)]
        public void AccruedTax_PreservesPerMinuteCalculation(
            int productionPerMinute,
            double elapsedSeconds,
            double expectedTax
        )
        {
            Assert.That(
                TaxCalculator.CalculateAccruedTax(
                    productionPerMinute,
                    elapsedSeconds
                ),
                Is.EqualTo(expectedTax).Within(0.000001d)
            );
        }

        [Test]
        public void EligibleSeconds_StopsAtCurrentTwentyFourHourBudget()
        {
            const double day = 24d * 60d * 60d;

            Assert.That(
                TaxCalculator.GetEligibleSeconds(
                    elapsedSeconds: 120d,
                    alreadyUsedSeconds: day - 30d,
                    maximumSeconds: day
                ),
                Is.EqualTo(30d)
            );
        }

        [Test]
        public void EligibleSeconds_DoesNotReprocessConsumedBudget()
        {
            const double day = 24d * 60d * 60d;

            Assert.That(
                TaxCalculator.GetEligibleSeconds(
                    elapsedSeconds: 60d,
                    alreadyUsedSeconds: day,
                    maximumSeconds: day
                ),
                Is.Zero
            );
        }
    }

    public class GameSaveRepositoryCharacterizationTests
    {
        private bool hadLegacySave;
        private string legacySave;
        private bool hadVersionTwoSave;
        private string versionTwoSave;

        [SetUp]
        public void SetUp()
        {
            hadLegacySave = PlayerPrefs.HasKey(
                GameSaveRepository.LegacyCitySaveKey
            );
            legacySave = hadLegacySave
                ? PlayerPrefs.GetString(
                    GameSaveRepository.LegacyCitySaveKey
                )
                : null;
            hadVersionTwoSave = PlayerPrefs.HasKey(
                GameSaveRepository.SaveKey
            );
            versionTwoSave = hadVersionTwoSave
                ? PlayerPrefs.GetString(GameSaveRepository.SaveKey)
                : null;

            PlayerPrefs.DeleteKey(GameSaveRepository.LegacyCitySaveKey);
            PlayerPrefs.DeleteKey(GameSaveRepository.SaveKey);
            PlayerPrefs.Save();
            GameSessionProvider.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            GameSessionProvider.Reset();

            if (hadLegacySave)
            {
                PlayerPrefs.SetString(
                    GameSaveRepository.LegacyCitySaveKey,
                    legacySave
                );
            }
            else
            {
                PlayerPrefs.DeleteKey(
                    GameSaveRepository.LegacyCitySaveKey
                );
            }

            if (hadVersionTwoSave)
            {
                PlayerPrefs.SetString(
                    GameSaveRepository.SaveKey,
                    versionTwoSave
                );
            }
            else
            {
                PlayerPrefs.DeleteKey(GameSaveRepository.SaveKey);
            }

            PlayerPrefs.Save();
            GameSessionProvider.Reset();
        }

        [Test]
        public void MissingSave_LoadsConfiguredDefaults()
        {
            GameSaveData loaded = new GameSaveRepository().Load();

            Assert.That(loaded, Is.Not.Null);
            Assert.That(
                loaded.version,
                Is.EqualTo(GameSaveData.CurrentVersion)
            );
            Assert.That(loaded.money, Is.EqualTo(1000));
            Assert.That(loaded.buildings, Is.Not.Null.And.Empty);
            Assert.That(loaded.taxes, Is.Not.Null);
            Assert.That(loaded.playerInventory, Is.Not.Null);
            Assert.That(
                loaded.playerInventory.recruitedAdventurerIds,
                Is.Not.Null.And.Empty
            );
            Assert.That(
                loaded.playerInventory.items,
                Is.Not.Null.And.Empty
            );
            Assert.That(PlayerPrefs.HasKey(GameSaveRepository.SaveKey), Is.True);
        }

        [Test]
        public void LegacyCitySave_IsMigratedOnceAndVersionTwoWinsAfterwards()
        {
            var legacy = new LegacyCitySaveFixture
            {
                hasMoney = true,
                money = 640,
                buildings = new System.Collections.Generic.List<BuildingSaveData>
                {
                    new BuildingSaveData
                    {
                        type = BuildingType.Barracks,
                        level = 4
                    }
                },
                taxes = new TaxAccrualSaveData
                {
                    initialized = true,
                    accruedTax = 9.5d
                }
            };

            PlayerPrefs.SetString(
                GameSaveRepository.LegacyCitySaveKey,
                JsonUtility.ToJson(legacy)
            );

            var repository = new GameSaveRepository();
            GameSaveData migrated = repository.Load();

            legacy.money = 1;
            PlayerPrefs.SetString(
                GameSaveRepository.LegacyCitySaveKey,
                JsonUtility.ToJson(legacy)
            );
            GameSaveData loadedAgain = repository.Load();

            Assert.That(migrated.money, Is.EqualTo(640));
            Assert.That(
                migrated.GetBuildingLevel(BuildingType.Barracks),
                Is.EqualTo(4)
            );
            Assert.That(migrated.taxes.accruedTax, Is.EqualTo(9.5d));
            Assert.That(loadedAgain.money, Is.EqualTo(640));
        }

        [Test]
        public void VersionTwoSave_IsUpgradedWithoutLosingState()
        {
            PlayerPrefs.SetString(
                GameSaveRepository.SaveKey,
                "{\"version\":2,\"money\":321}"
            );

            GameSaveData loaded = new GameSaveRepository().Load();

            Assert.That(
                loaded.version,
                Is.EqualTo(GameSaveData.CurrentVersion)
            );
            Assert.That(loaded.money, Is.EqualTo(321));
            Assert.That(loaded.playerInventory, Is.Not.Null);
            Assert.That(
                loaded.playerInventory.recruitedAdventurerIds,
                Is.Empty
            );
            Assert.That(loaded.playerInventory.items, Is.Empty);

            GameSaveData persisted = JsonUtility.FromJson<GameSaveData>(
                PlayerPrefs.GetString(GameSaveRepository.SaveKey)
            );
            Assert.That(
                persisted.version,
                Is.EqualTo(GameSaveData.CurrentVersion)
            );
        }

        [System.Serializable]
        private sealed class LegacyCitySaveFixture
        {
            public bool hasMoney;
            public int money;
            public System.Collections.Generic.List<BuildingSaveData>
                buildings = new();
            public TaxAccrualSaveData taxes = new();
        }
    }
}
