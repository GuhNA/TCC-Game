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
            public MemoryRepository() { state.money = 100; }
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
            if (scene.IsValid() && scene.name == "Centro")
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

        private static Button Button(BuildingPanel panel, string name) => panel.GetComponentsInChildren<Button>(true).Single(x => x.name == name);
        private static TMP_Text Label(BuildingPanel panel, string name) => panel.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == name);
    }
}
