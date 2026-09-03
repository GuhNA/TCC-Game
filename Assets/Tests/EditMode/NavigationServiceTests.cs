using NUnit.Framework;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;
using TCCGame.Gameplay.Navigation;

namespace TCCGame.Tests.EditMode
{
    public sealed class NavigationServiceTests
    {
        [Test]
        public void EnterGateAndReturnToCenter_PersistsTheEntryGate()
        {
            GameSaveData data = GameSaveData.CreateDefault();
            int commits = 0;
            var service = new NavigationService(data, () => commits++);

            service.EnterGate(GateId.West);

            Assert.That(service.Current.CurrentArea, Is.EqualTo(CityArea.Gate));
            Assert.That(service.Current.ActiveGateId, Is.EqualTo(GateId.West));
            Assert.That(commits, Is.EqualTo(1));

            service.ReturnToCenter();

            Assert.That(service.Current.CurrentArea, Is.EqualTo(CityArea.Center));
            Assert.That(service.Current.HasCenterEntryGate, Is.True);
            Assert.That(service.Current.CenterEntryGate, Is.EqualTo(GateId.West));
            Assert.That(data.navigation.centerEntryGate, Is.EqualTo(GateId.West));
            Assert.That(commits, Is.EqualTo(2));
        }

        [Test]
        public void ReenteringTheSameGate_DoesNotCommitTwice()
        {
            GameSaveData data = GameSaveData.CreateDefault();
            int commits = 0;
            var service = new NavigationService(data, () => commits++);

            service.EnterGate(GateId.North);
            service.EnterGate(GateId.North);

            Assert.That(commits, Is.EqualTo(1));
        }

        [Test]
        public void Formatter_UsesReadableWaveCountdowns()
        {
            Assert.That(
                GatePresentationFormatter.FormatRemainingTime(125d),
                Is.EqualTo("02:05")
            );
            Assert.That(
                GatePresentationFormatter.FormatRemainingTime(0d),
                Is.EqualTo("00:00")
            );
        }
    }
}
