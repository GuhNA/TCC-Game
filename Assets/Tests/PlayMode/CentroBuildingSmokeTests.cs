using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Input;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using TCCGame.Gameplay.Campaign;
using TCCGame.Gameplay.Exploration;
using TCCGame.Gameplay.Navigation;
using TCCGame.Prototypes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TCCGame.Tests.PlayMode
{
    public sealed class CentroBuildingSmokeTests
    {
        private static readonly FieldInfo SessionField = typeof(GameSessionProvider)
            .GetField("current", BindingFlags.Static | BindingFlags.NonPublic);
        private object previousSession;
        private Keyboard keyboard;
        private bool addedKeyboard;
        private InputAction cancel;
        private bool cancelWasEnabled;
        private InputSettings.BackgroundBehavior previousBackground;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
#endif

        private sealed class MemoryRepository : IGameSaveRepository
        {
            private GameSaveData state = GameSaveData.CreateDefault();
            public MemoryRepository(GameSaveData saved = null)
            {
                if (saved != null) state = saved.Clone();
                else state.money = 100;
            }
            public GameSaveData Load() => state.Clone();
            public void Save(GameSaveData data) => state = data.Clone();
        }

        [SetUp]
        public void SetUp()
        {
            previousSession = SessionField.GetValue(null);
            // Tests transact only against this repository, never the user's PlayerPrefs.
            SessionField.SetValue(null, new GameSession(new MemoryRepository(), new SystemClock()));
            previousBackground = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            previousEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.IsValid() && (scene.name == SceneCatalog.Center ||
                scene.name == SceneCatalog.Gate || scene.name == SceneCatalog.Tavern))
            {
                SceneManager.SetActiveScene(SceneManager.CreateScene("Centro smoke cleanup"));
                yield return SceneManager.UnloadSceneAsync(scene);
            }
            if (cancel != null && !cancelWasEnabled) cancel.Disable();
            if (addedKeyboard) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = previousBackground;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInput;
#endif
            SessionField.SetValue(null, previousSession);
        }

        [UnityTest]
        public IEnumerator Buildings_ConstructUpgradeRejectAndCloseUsingProductionScene()
        {
            yield return SceneManager.LoadSceneAsync("Centro", LoadSceneMode.Single);
            yield return null;
            BuildingPanel panel = Object.FindFirstObjectByType<BuildingPanel>(FindObjectsInactive.Include);
            var player = Object.FindFirstObjectByType<PlayerInteractor>();
            var buildings = Object.FindObjectsByType<BuildingBehaviour>(FindObjectsSortMode.None);
            Assert.That(buildings, Has.Length.EqualTo(6));
            Assert.That(panel.gameObject.activeSelf, Is.False);
            BuildingBehaviour barracks = buildings.Single(x => x.Definition.type == BuildingType.Barracks);
            barracks.Interact(player.gameObject);
            yield return null;
            Assert.That(panel.isActiveAndEnabled, Is.True);
            Assert.That(player.HasModalOpen && UiMenuState.HasOpenMenu, Is.True);
            Button upgrade = Button(panel, "Upgrade Building");
            Assert.That(Label(panel, "Building Level").text, Does.Contain("NÍVEL 0"));
            Canvas.ForceUpdateCanvases();
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, upgrade.transform.position)
            };
            var hits = new List<RaycastResult>();
            panel.GetComponentInParent<GraphicRaycaster>().Raycast(pointer, hits);
            Assert.That(hits.First().gameObject, Is.EqualTo(upgrade.gameObject), "The primary action must receive pointer input.");

            upgrade.onClick.Invoke();
            upgrade.onClick.Invoke(); // Duplicate dispatch in the same frame must not buy twice.
            yield return null;
            var session = GameSessionProvider.Current;
            Assert.That(barracks.Level, Is.EqualTo(1));
            Assert.That(session.Economy.Money, Is.Zero);
            Assert.That(upgrade.interactable, Is.False);
            Assert.That(Label(panel, "Cost Balance").text, Does.Contain("Faltam 150"));
            upgrade.onClick.Invoke();
            Assert.That(barracks.Level, Is.EqualTo(1));
            session.Economy.AddMoney(100000);
            yield return null;
            Assert.That(upgrade.interactable, Is.True);

            // Ouro sozinho não constrói o alojamento: também são necessários recursos do mapa.
            BuildingBehaviour lodge = buildings.Single(x => x.Definition.type == BuildingType.ExplorersLodge);
            Assert.That(lodge.name, Is.EqualTo("ExplorationBuild"));
            Assert.That(lodge.GetComponent<Collider2D>().enabled, Is.True);
            panel.Hide();
            lodge.Interact(player.gameObject);
            yield return null;
            Assert.That(upgrade.interactable, Is.False);
            Assert.That(Label(panel, "Building Subtitle").text, Does.Contain("Cada nível concede 1 explorador"));
            Assert.That(Label(panel, "Building Feedback").text, Does.Contain("Recursos insuficientes"));

            foreach (BuildingUpgradeLevelCost levelCost in lodge.Definition.levelUpgradeCosts)
                foreach (BuildingResourceRequirement requirement in levelCost.resources)
                    Assert.That(session.Inventory.TryAddItem(requirement.ResourceId, requirement.quantity), Is.True);

            foreach (BuildingBehaviour building in buildings)
            {
                panel.Hide();
                Assert.That(player.HasModalOpen || UiMenuState.HasOpenMenu, Is.False);
                building.Interact(player.gameObject);
                yield return null;
                Assert.That(Label(panel, "Building Title").text, Is.EqualTo(building.Definition.displayName.ToUpperInvariant()));
                while (building.Level < 2)
                {
                    int before = session.Economy.Money;
                    int level = building.Level;
                    int cost = BuildingCalculator.GetUpgradeCost(building.Definition, level);
                    int explorerCapacity = session.Exploration.AvailableExplorers;
                    var materials = BuildingCalculator.GetResourceRequirements(building.Definition, level)
                        .GroupBy(x => x.ResourceId)
                        .ToDictionary(x => x.Key, x => new { Before = session.Buildings.GetResourceQuantity(x.Key), Cost = x.Sum(r => r.quantity) });
                    upgrade.onClick.Invoke();
                    upgrade.onClick.Invoke();
                    yield return null;
                    Assert.That(building.Level, Is.EqualTo(level + 1));
                    Assert.That(session.Buildings.GetLevel(building.Definition.type), Is.EqualTo(level + 1));
                    Assert.That(session.Economy.Money, Is.EqualTo(before - cost));
                    foreach (var material in materials)
                        Assert.That(session.Buildings.GetResourceQuantity(material.Key), Is.EqualTo(material.Value.Before - material.Value.Cost));
                    if (building.Definition.type == BuildingType.ExplorersLodge)
                        Assert.That(session.Exploration.AvailableExplorers, Is.EqualTo(explorerCapacity + 1));
                    Assert.That(Label(panel, "Building Feedback").text, Does.Contain("Melhoria concluída"));
                }
                Assert.That(Label(panel, "Current Benefit").text, Does.Contain(building.Definition.type == BuildingType.Tavern
                    ? "3 aventureiros" : building.Definition.type == BuildingType.LocalMarket ? "15 ouro"
                    : building.Definition.type == BuildingType.ExplorersLodge ? "2 exploradores" : "16 "));
                Button(panel, "Close Building").onClick.Invoke();
                Assert.That(player.HasModalOpen, Is.False);
            }

            barracks.Interact(player.gameObject);
            while (barracks.Level < barracks.Definition.maximumLevel)
            {
                upgrade.onClick.Invoke();
                yield return null;
            }
            Assert.That(upgrade.gameObject.activeSelf, Is.False);
            Assert.That(Label(panel, "Next Caption").text, Is.EqualTo("CONCLUÍDO"));
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(Button(panel, "Back To City").gameObject));
            Button(panel, "Back To City").onClick.Invoke();
            session.Economy.AddMoney(1);
            yield return null;
            Assert.That(panel.gameObject.activeSelf, Is.False, "A balance update must not reopen a closed modal.");

            barracks.Interact(player.gameObject);
            cancel = InputSystem.actions.FindAction("UI/Cancel", true);
            cancelWasEnabled = cancel.enabled;
            cancel.Enable();
            keyboard = InputSystem.AddDevice<Keyboard>();
            addedKeyboard = true;
            InputSystem.EnableDevice(keyboard);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
            // Process the synthetic input in the same frame as the modal's handler.
            InputSystem.Update();
            Assert.That(GameInput.WasCancelPressedThisFrame(), Is.True,
                $"Escape must reach UI/Cancel. Keyboard enabled: {keyboard.enabled}, pressed: {keyboard.escapeKey.isPressed}, controls: {string.Join(", ", cancel.controls.Select(x => x.path))}");
            panel.SendMessage("Update", SendMessageOptions.RequireReceiver);
            Assert.That(panel.gameObject.activeSelf, Is.False, "Esc closes the building modal.");
            Assert.That(player.HasModalOpen || UiMenuState.HasOpenMenu, Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ApprovedLayout_ConnectsTavernServicesAndAllFourGateSpawns()
        {
            var saved = GameSaveData.CreateDefault();
            saved.money = 731;
            saved.SetBuildingLevel(BuildingType.Tavern, 1);
            saved.SetBuildingLevel(BuildingType.ExplorersLodge, 2);
            var repository = new MemoryRepository(saved);
            var session = new GameSession(repository, new SystemClock());
            SessionField.SetValue(null, session);

            yield return SceneManager.LoadSceneAsync(SceneCatalog.Center, LoadSceneMode.Single);
            yield return null;
            Assert.That(GameObject.Find("Centro Static Background"), Is.Not.Null);
            Assert.That(GameObject.Find("Tavern Management Sign"), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<WorldBuildingView>(FindObjectsSortMode.None).Length, Is.EqualTo(6));
            var entrance = Object.FindFirstObjectByType<TavernScenePortal>();
            var management = Object.FindObjectsByType<BuildingBehaviour>(FindObjectsSortMode.None)
                .Single(x => x.Definition.type == BuildingType.Tavern);
            yield return StandIn(management, management);
            yield return StandIn(entrance, entrance);
            entrance.Interact(Object.FindFirstObjectByType<PlayerInteractor>().gameObject);
            yield return WaitForScene(SceneCatalog.Tavern);

            AssertAtSpawn("CenterDefaultSpawn");
            Assert.That(GameObject.Find("Tavern floor perimeter").GetComponent<EdgeCollider2D>().enabled, Is.True);
            Assert.That(Object.FindFirstObjectByType<TavernRecruitableSpawner>().enabled, Is.True);
            Assert.That(Object.FindFirstObjectByType<ExplorationAreaReportView>(FindObjectsInactive.Include), Is.Not.Null);
            var recruitables = Object.FindObjectsByType<Aventureiro>(FindObjectsSortMode.None);
            Assert.That(recruitables, Is.Not.Empty);
            foreach (var recruitable in recruitables)
                AssertNpcZones(recruitable);
            var board = Object.FindFirstObjectByType<ExplorationMapController>();
            yield return StandIn(board, board);
            var player = Object.FindFirstObjectByType<PlayerInteractor>();
            board.Interact(player.gameObject);
            Assert.That(player.HasModalOpen, Is.True);
            board.CloseModal();

            var exit = Object.FindFirstObjectByType<TavernScenePortal>();
            yield return StandIn(exit, exit);
            exit.Interact(player.gameObject);
            yield return WaitForScene(SceneCatalog.Center);
            AssertAtSpawn(PlayerSpawnController.TavernReturnAnchor);
            Assert.That(Object.FindFirstObjectByType<PlayerInteractor>().CurrentInteractable, Is.Not.TypeOf<TavernScenePortal>());
            AssertNpcZones(Object.FindFirstObjectByType<TaxCollectorNpc>());

            foreach (GateId gate in System.Enum.GetValues(typeof(GateId)))
            {
                var sentinel = Object.FindObjectsByType<CityTravelNpc>(FindObjectsSortMode.None)
                    .Single(x => x.Destination.GateId == gate);
                yield return StandIn(sentinel, sentinel, checkNpcRange: true);
                ConfirmTravel(sentinel);
                yield return WaitForScene(SceneCatalog.Gate);
                AssertAtSpawn("GateEntranceSpawn");
                Assert.That(session.Navigation.Current.ActiveGateId, Is.EqualTo(gate));

                var guide = Object.FindFirstObjectByType<CityTravelNpc>();
                AssertNpcZones(Object.FindFirstObjectByType<CaptainNpc>());
                AssertNpcZones(Object.FindFirstObjectByType<NPCVenda>());
                yield return StandIn(guide, guide, checkNpcRange: true);
                ConfirmTravel(guide);
                yield return WaitForScene(SceneCatalog.Center);
                AssertAtSpawn($"CenterFrom{gate}Spawn");
            }

            Assert.That(repository.Load().money, Is.EqualTo(saved.money));
            Assert.That(session.Buildings.GetLevel(BuildingType.ExplorersLodge), Is.EqualTo(2));
        }

        private static CircleCollider2D AssertNpcZones(Component npc)
        {
            var solid = npc.GetComponents<CircleCollider2D>().Single(c => c.enabled && !c.isTrigger);
            var zone = npc.GetComponentsInChildren<Collider2D>().Single(c => c.enabled && c.isTrigger);
            Assert.That(solid.bounds.extents.x, Is.InRange(.21f, .28f), npc.name);
            Assert.That(zone.bounds.size.x, Is.GreaterThanOrEqualTo(2.6f), npc.name);
            Assert.That(zone.bounds.Contains(solid.bounds.center), Is.True, npc.name);
            return solid;
        }

        private static IEnumerator StandIn(Component target, IInteractable interaction, bool checkNpcRange = false)
        {
            var player = Object.FindFirstObjectByType<PlayerInteractor>();
            var body = player.GetComponent<Rigidbody2D>();
            var feet = player.GetComponent<Collider2D>();
            var zone = target.GetComponents<Collider2D>().Single(c => c.enabled && c.isTrigger);
            var solid = checkNpcRange ? AssertNpcZones(target) : null;
            Vector2 contactPoint = zone.bounds.center;
            if (solid != null) contactPoint += Vector2.up * .78f;
            Vector2 offset = (Vector2)feet.bounds.center - body.position;
            body.position = contactPoint - offset;
            body.linearVelocity = Vector2.zero;
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(player.CurrentInteractable, Is.SameAs(interaction), target.name);
            if (solid != null) Assert.That(feet.Distance(solid).isOverlapped, Is.False, target.name);
        }

        private static void ConfirmTravel(CityTravelNpc guide)
        {
            guide.Interact(Object.FindFirstObjectByType<PlayerInteractor>().gameObject);
            Assert.That(TravelConversationPanel.IsVisible, Is.True);
            var panel = Object.FindFirstObjectByType<TravelConversationPanel>();
            panel.GetComponentsInChildren<Button>(true).Single(x => x.name == "Confirm").onClick.Invoke();
        }

        private static IEnumerator WaitForScene(string scene)
        {
            float deadline = Time.realtimeSinceStartup + 15f;
            while ((SceneTransitionController.IsTransitioning || SceneManager.GetActiveScene().name != scene)
                && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(scene));
            Assert.That(SceneTransitionController.IsTransitioning, Is.False);
        }

        private static void AssertAtSpawn(string name)
        {
            Vector2 position = Object.FindFirstObjectByType<PlayerInteractor>().GetComponent<Rigidbody2D>().position;
            Assert.That(Vector2.Distance(position, GameObject.Find(name).transform.position), Is.LessThan(.20f), name);
        }

        private static Button Button(BuildingPanel panel, string name) => panel.GetComponentsInChildren<Button>(true).Single(x => x.name == name);
        private static TMP_Text Label(BuildingPanel panel, string name) => panel.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == name);
    }
}
