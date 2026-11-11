using System;
using NUnit.Framework;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using UnityEditor;
using UnityEngine;

namespace TCCGame.Tests.EditMode
{
    public sealed class OfflineResumeTests
    {
        private const long StartUnixSeconds = 1_700_000_000;
        private const int OneDay = 24 * 60 * 60;
        private const int InitialBalance = 1000;

        private IsolatedGameSave save;
        private BuildingDefinition market;

        [SetUp]
        public void SetUp()
        {
            market = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(
                "Assets/Scriptable/Buildings/LocalMarket.asset"
            );
            Assert.That(market, Is.Not.Null);
            save = new IsolatedGameSave();

            GameSaveData initial = GameSaveData.CreateDefault();
            initial.money = InitialBalance;
            initial.SetBuildingLevel(BuildingType.LocalMarket, 3);
            save.OpenRepository().Save(initial);

            GameSession firstSession = OpenSession(StartUnixSeconds);
            Assert.That(firstSession.Buildings.GetProduction(BuildingType.LocalMarket), Is.EqualTo(20));
            firstSession.Taxes.SaveCurrentAccrual();
        }

        [TearDown]
        public void TearDown()
        {
            save?.Dispose();
        }

        [TestCase(0, 0)]
        [TestCase(360, 120)]
        [TestCase(OneDay - 1, 28799)]
        [TestCase(OneDay, 28800)]
        [TestCase(2 * OneDay, 28800)]
        public void ReopeningCalculatesTaxesForTheEligibleInterval(int secondsAway, int expected)
        {
            GameSession reopened = OpenSession(StartUnixSeconds + secondsAway);
            int preview = reopened.Taxes.GetCollectibleAmount();
            int collected = reopened.Taxes.CollectTaxes().Amount;
            int balance = reopened.Economy.Money;

            Report($"absence_seconds={secondsAway} expected_gold={expected} observed_preview={preview} observed_collected={collected} expected_balance={InitialBalance + expected} observed_balance={balance}");
            Assert.That(preview, Is.EqualTo(expected));
            Assert.That(collected, Is.EqualTo(expected));
            Assert.That(balance, Is.EqualTo(InitialBalance + expected));
        }

        [Test]
        public void ReopeningUsesOnlyTheRemainingTaxBudget()
        {
            long savedAt = StartUnixSeconds + OneDay - 30 * 60;
            GameSession beforeClosing = OpenSession(savedAt);
            beforeClosing.Taxes.SaveCurrentAccrual();
            GameSaveData saved = save.OpenRepository().Load();

            GameSession reopened = OpenSession(savedAt + 2 * 60 * 60);
            int total = reopened.Taxes.GetCollectibleAmount();
            double added = total - saved.taxes.accruedTax;

            Report($"absence_seconds=7200 saved_eligible_seconds={saved.taxes.eligibleSecondsSinceCollection} saved_accrued_gold={saved.taxes.accruedTax} expected_added_gold=600 observed_added_gold={added} observed_total_gold={total}");
            Assert.That(saved.taxes.eligibleSecondsSinceCollection, Is.EqualTo(OneDay - 30 * 60));
            Assert.That(saved.taxes.accruedTax, Is.EqualTo(28200));
            Assert.That(added, Is.EqualTo(600));
            Assert.That(total, Is.EqualTo(28800));
        }

        [Test]
        public void CollectedTaxesPersistAcrossReopeningWithoutASecondCredit()
        {
            long collectionTime = StartUnixSeconds + 360;
            GameSession collectionSession = OpenSession(collectionTime);
            int collected = collectionSession.Taxes.CollectTaxes().Amount;

            GameSession reopened = OpenSession(collectionTime);
            int balanceBeforeRetry = reopened.Economy.Money;
            int available = reopened.Taxes.GetCollectibleAmount();
            int collectedAgain = reopened.Taxes.CollectTaxes().Amount;
            int finalBalance = OpenSession(collectionTime).Economy.Money;

            Report($"expected_first_credit=120 observed_first_credit={collected} expected_saved_balance=1120 observed_saved_balance={balanceBeforeRetry} expected_second_credit=0 observed_available={available} observed_second_credit={collectedAgain} observed_final_balance={finalBalance}");
            Assert.That(collected, Is.EqualTo(120));
            Assert.That(balanceBeforeRetry, Is.EqualTo(1120));
            Assert.That(available, Is.Zero);
            Assert.That(collectedAgain, Is.Zero);
            Assert.That(finalBalance, Is.EqualTo(1120));
        }

        [Test]
        public void ReopeningPreservesCampaignTimeUntilItAdvancesInTheNewSession()
        {
            GameSession beforeClosing = OpenSession(StartUnixSeconds);
            beforeClosing.Campaign.Advance(47.25d);
            beforeClosing.Campaign.Checkpoint();

            GameSession reopened = OpenSession(StartUnixSeconds + 2 * OneDay);
            double elapsed = reopened.Campaign.ElapsedStrategicSeconds;
            var wave = reopened.Campaign.GetWave(1);
            Assert.That(wave.HasValue, Is.True);
            double remaining = wave.Value.ArrivalStrategicSeconds - elapsed;
            reopened.Campaign.Advance(5d);
            double advanced = reopened.Campaign.ElapsedStrategicSeconds;

            Report($"absence_seconds=172800 expected_elapsed=47.25 observed_elapsed={elapsed} expected_remaining=132.75 observed_remaining={remaining} expected_after_advance=52.25 observed_after_advance={advanced}");
            Assert.That(elapsed, Is.EqualTo(47.25d));
            Assert.That(remaining, Is.EqualTo(132.75d));
            Assert.That(advanced, Is.EqualTo(52.25d));
        }

        private GameSession OpenSession(long unixSeconds)
        {
            var session = new GameSession(save.OpenRepository(), new FixedClock(unixSeconds));
            session.Buildings.RegisterDefinition(market);
            return session;
        }

        private static void Report(FormattableString observation)
        {
            Debug.Log("TCC_OFFLINE " + FormattableString.Invariant(observation));
        }

        private sealed class FixedClock : IClock
        {
            public FixedClock(long unixSeconds)
            {
                UnixSeconds = unixSeconds;
            }

            public long UnixSeconds { get; }
        }
    }
}
