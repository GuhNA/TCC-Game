using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Input;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using TCCGame.UI;
using TCCGame.Gameplay.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TCCGame.Tests.PlayMode
{
    public sealed class CentroTaxSmokeTests
    {
        private static readonly FieldInfo Current = typeof(GameSessionProvider).GetField("current", BindingFlags.Static | BindingFlags.NonPublic);
        private object previous;
        private MemoryRepository repository;
        private Clock clock;
        private sealed class Clock : IClock { public long UnixSeconds { get; set; } = 1000000; }
        private sealed class MemoryRepository : IGameSaveRepository
        {
            public GameSaveData Data = GameSaveData.CreateDefault();
            public int Saves;
            public GameSaveData Load() => Data.Clone();
            public void Save(GameSaveData data) { Data = data.Clone(); Saves++; }
        }

        [SetUp]
        public void SetUp()
        {
            previous = Current.GetValue(null);
            clock = new Clock();
            repository = new MemoryRepository();
            repository.Data.SetBuildingLevel(BuildingType.LocalMarket, 3);
            repository.Data.SetBuildingLevel(BuildingType.Tavern, 1);
            repository.Data.taxes.initialized = true;
            repository.Data.taxes.lastCalculatedUnixSeconds = clock.UnixSeconds - 360;
            repository.Data.taxes.lastCollectedUnixSeconds = clock.UnixSeconds - 360;
            Current.SetValue(null, new GameSession(repository, clock));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name == "Centro")
            {
                SceneManager.SetActiveScene(SceneManager.CreateScene("Centro tax cleanup"));
                yield return SceneManager.UnloadSceneAsync(scene);
            }
            Current.SetValue(null, previous);
        }

        [UnityTest]
        public IEnumerator Taxes_PreviewCollectPauseExpireAndExplainEmptyResults()
        {
            yield return SceneManager.LoadSceneAsync("Centro", LoadSceneMode.Single);
            yield return null;
            var session = GameSessionProvider.Current;
            var collector = Object.FindFirstObjectByType<TaxCollectorNpc>();
            var player = Object.FindFirstObjectByType<PlayerInteractor>();
            var hud = Object.FindFirstObjectByType<CityTaxHud>();
            var view = hud.GetComponentInChildren<CityTaxHudView>(true);
            GameObject widget = view.transform.Find("Treasury Widget").gameObject;
            GameObject toast = view.transform.Find("Tax Toast").gameObject;
            Assert.That(widget.activeSelf, Is.False, "Tax preview stays contextual.");
            int saves = repository.Saves;
            for (int i = 0; i < 20; i++) Assert.That(session.Taxes.GetCollectibleAmount(), Is.EqualTo(120));
            Assert.That(repository.Saves, Is.EqualTo(saves), "A preview cannot save or collect taxes.");
            Assert.That(session.Economy.Money, Is.EqualTo(1000));
            Assert.That(collector.InteractionPrompt, Does.Contain("120 ouro"));
            var tavern = Object.FindObjectsByType<BuildingBehaviour>(FindObjectsSortMode.None).Single(x => x.Definition.type == BuildingType.Tavern);
            Assert.That(tavern.InteractionPrompt, Is.EqualTo("Gerenciar Taverna"));
            Assert.That(Object.FindFirstObjectByType<TavernScenePortal>().InteractionPrompt, Is.EqualTo("Entrar na Taverna"));

            player.transform.position = collector.transform.position;
            player.SendMessage("OnTriggerEnter2D", collector.GetComponentInChildren<Collider2D>());
            yield return null;
            yield return null;
            Assert.That(widget.activeSelf, Is.True);
            collector.Interact(player.gameObject);
            collector.Interact(player.gameObject);
            yield return null;
            Assert.That(session.Economy.Money, Is.EqualTo(1120));
            Assert.That(session.Taxes.GetCollectibleAmount(), Is.Zero);
            Assert.That(Label(view, "Toast Message").text, Does.Contain("+120 ouro"));
            Assert.That(Object.FindFirstObjectByType<GoldBalancePresenter>().PixelGoldText.text, Is.EqualTo("1 120"));
            var gold = Object.FindFirstObjectByType<GoldBalancePresenter>();
            Assert.That(gold.transform.parent, Is.Null, "Gold uses its own correctly scaled root canvas.");
            Assert.That(gold.transform.localScale.x, Is.GreaterThan(0), "Unity scales the root canvas to the current viewport.");
            Assert.That(gold.GetComponent<Canvas>().isRootCanvas, Is.True);
            Assert.That(gold.transform.Cast<Transform>().Count(x => x.gameObject.activeSelf), Is.EqualTo(1), "Only the bound gold visual is displayed.");
            Assert.That(toast.activeSelf, Is.True);
            Assert.That(player.HasModalOpen || UiMenuState.HasOpenMenu, Is.False, "Collecting does not block the city.");

            tavern.Interact(player.gameObject);
            collector.Interact(player.gameObject);
            Assert.That(session.Economy.Money, Is.EqualTo(1120));
            yield return null;
            Assert.That(toast.activeSelf || widget.activeSelf, Is.False, "Tax UI waits behind an open menu.");
            yield return new WaitForSecondsRealtime(3.7f);
            Object.FindFirstObjectByType<BuildingPanel>().Hide();
            yield return null;
            Assert.That(toast.activeSelf, Is.True, "Reading time must pause while the modal is open.");
            yield return new WaitForSecondsRealtime(3.7f);
            Assert.That(toast.activeSelf, Is.False);

            collector.Interact(player.gameObject);
            yield return null;
            Assert.That(Label(view, "Toast Message").text, Does.Contain("Ainda não há impostos"));
            Assert.That(session.Economy.Money, Is.EqualTo(1120));
            // Simulate a missing market without writing to the user's save.
            var state = (GameSaveData)typeof(GameSession).GetProperty("State", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
            state.SetBuildingLevel(BuildingType.LocalMarket, 0);
            collector.Interact(player.gameObject);
            yield return null;
            Assert.That(Label(view, "Toast Message").text, Does.Contain("Construa o Mercado Local"));
            Assert.That(session.Economy.Money, Is.EqualTo(1120));
            state.SetBuildingLevel(BuildingType.LocalMarket, 3);
            clock.UnixSeconds -= 100;
            Assert.That(session.Taxes.GetCollectibleAmount(), Is.Zero, "Clock rollback cannot project negative accrual.");
            clock.UnixSeconds += 100 + 2 * 24 * 3600;
            saves = repository.Saves;
            Assert.That(session.Taxes.GetCollectibleAmount(), Is.EqualTo(28800), "Preview uses the same 24-hour budget as collection.");
            Assert.That(repository.Saves, Is.EqualTo(saves));
            Assert.That(session.Taxes.CollectTaxes().Amount, Is.EqualTo(28800));
        }

        private static TMP_Text Label(Component root, string name) => root.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == name);
    }
}
