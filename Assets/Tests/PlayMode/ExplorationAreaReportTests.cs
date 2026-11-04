using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;
using TCCGame.Gameplay.Exploration;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TCCGame.Tests.PlayMode
{
    public sealed class ExplorationAreaReportTests
    {
        private sealed class Clock : IClock { public long UnixSeconds { get; set; } = 1000; }
        private sealed class Repository : IGameSaveRepository
        {
            public GameSaveData Data = GameSaveData.CreateDefault();
            public GameSaveData Load() => Data;
            public void Save(GameSaveData data) => Data = data;
        }

        private GameSession previousSession;
        private static readonly FieldInfo CurrentSession = typeof(GameSessionProvider)
            .GetField("current", BindingFlags.Static | BindingFlags.NonPublic);

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Scene scene = SceneManager.GetSceneByName("Taverna");
            if (scene.IsValid() && scene.isLoaded)
            {
                SceneManager.SetActiveScene(SceneManager.CreateScene("Report test cleanup"));
                yield return SceneManager.UnloadSceneAsync(scene);
            }
            CurrentSession.SetValue(null, previousSession);
        }

        [UnityTest]
        public IEnumerator AuthoredReport_DisplaysLiveAreaStateAndPreservesNavigation()
        {
            previousSession = CurrentSession.GetValue(null) as GameSession;
            var repository = new Repository();
            var clock = new Clock();
            var state = repository.Data;
            state.SetBuildingLevel(BuildingType.ExplorersLodge, 5);
            var area = new ExplorationAreaSaveData
            {
                environment = ExplorationEnvironment.ColinasDoVento,
                captured = true,
                assignedWorkers = 2,
                nextEnemyAttackUnixSeconds = 1125,
                nextWorkerCollectionUnixSeconds = 1300
            };
            state.exploration.areas.Add(area);
            state.exploration.areas.Add(new ExplorationAreaSaveData
            {
                environment = ExplorationEnvironment.FlorestaDasSombras,
                captured = true,
                assignedWorkers = 1
            });
            var session = new GameSession(repository, clock);
            CurrentSession.SetValue(null, session);
            yield return SceneManager.LoadSceneAsync("Taverna");
            yield return null;

            var controller = Object.FindFirstObjectByType<ExplorationMapController>();
            var view = Object.FindFirstObjectByType<ExplorationAreaReportView>(FindObjectsInactive.Include);
            Assert.That(view, Is.Not.Null, "Taverna must serialize the report view.");
            Assert.That(view.gameObject.activeSelf, Is.False);
            var catalog = Resources.Load<ExplorationEncounterCatalog>(ExplorationEncounterCatalog.ResourcesPath);
            Assert.That(catalog.TryGet(area.environment, out var encounter), Is.True);
            var resource = encounter.PossibleRewards.First(entry => entry.Resource != null).Resource;
            area.pendingWorkerLoot.Add(new ExplorationRewardSaveData
            {
                resourceId = resource.PersistenceId, quantity = 7, displayName = resource.DisplayName
            });
            Char_base[] definitions = Get<Char_base[]>(controller, "adventurerCatalog");
            Assert.That(definitions.Length, Is.GreaterThanOrEqualTo(3));
            var guards = definitions.Take(3).Select((definition, index) => new ExplorationCombatantSaveData
            {
                adventurerId = definition.PersistenceId,
                definitionId = definition.PersistenceId,
                displayName = "Guarda " + (index + 1),
                currentHealth = 37 + index,
                maxHealth = 100
            }).ToArray();
            area.guardParty.AddRange(guards.Take(2));
            controller.Interact(null);
            controller.OpenAreaReport((int)area.environment);
            yield return null;

            Assert.That(view.IsVisible, Is.True);
            Assert.That(Get<TMP_Text>(view, "locationText").text, Is.EqualTo("Colinas do Vento"));
            Assert.That(Get<TMP_Text>(view, "explorerCountText").text, Is.EqualTo("2 / 2"));
            Assert.That(Get<TMP_Text>(view, "attackTimeText").text, Is.EqualTo("02:05"));
            Assert.That(Get<GameObject>(view, "attackOn").activeSelf, Is.True);
            Assert.That(Get<GameObject>(view, "noAttack").activeSelf, Is.False);
            Transform guardRoot = view.GetComponentsInChildren<Transform>(true)
                .Single(child => child.name == "Aventureiros");
            Transform[] slots = Enumerable.Range(0, guardRoot.childCount)
                .Select(guardRoot.GetChild).Where(child => child.name.StartsWith("Aventureiro")).ToArray();
            Assert.That(slots.Select(slot => slot.gameObject.activeSelf), Is.EqualTo(new[] { true, true, false }));
            Assert.That(slots[0].Find("Nome").GetComponent<TMP_Text>().text, Is.EqualTo("Guarda 1"));
            Assert.That(slots[0].Find("Vida").GetComponent<TMP_Text>().text, Is.EqualTo("37/100"));
            Assert.That(slots[0].Find("Image").GetComponent<Image>().sprite, Is.SameAs(definitions[0].sprite));
            var rewardRows = view.GetComponentsInChildren<ExplorationRewardItemView>();
            Assert.That(rewardRows.Length, Is.EqualTo(encounter.PossibleRewards
                .Where(entry => entry.Resource != null).Select(entry => entry.Resource.PersistenceId).Distinct().Count()));
            var row = rewardRows.Single(item => Get<TMP_Text>(item, "nameText").text == resource.DisplayName);
            long maximum = encounter.PossibleRewards.Where(entry => entry.Resource == resource && entry.DropChance > 0)
                .Sum(entry => (long)entry.MaximumQuantity);
            Assert.That(Get<TMP_Text>(row, "quantityText").text, Is.EqualTo($"7 / {maximum * 2}"));
            Assert.That(Get<Image>(row, "icon").sprite, Is.SameAs(resource.Icon));
            Assert.That(view.GetComponentsInChildren<TMP_Text>(true).All(label =>
                label.font == Get<TMP_FontAsset>(view, "font")), Is.True);
            Capture(view);

            Get<Button>(view, "assignButton").onClick.Invoke();
            yield return null;
            Assert.That(area.assignedWorkers, Is.EqualTo(3));
            Assert.That(Get<TMP_Text>(view, "explorerCountText").text, Is.EqualTo("3 / 1"));
            Get<Button>(view, "removeButton").onClick.Invoke();
            yield return null;
            Assert.That(area.assignedWorkers, Is.EqualTo(2));
            Get<Button>(view, "transportButton").onClick.Invoke();
            yield return null;
            Assert.That(area.pendingWorkerLoot, Is.Empty);
            Assert.That(Get<Button>(view, "transportButton").interactable, Is.False);
            row = view.GetComponentsInChildren<ExplorationRewardItemView>()
                .Single(item => Get<TMP_Text>(item, "nameText").text == resource.DisplayName);
            Assert.That(Get<TMP_Text>(row, "quantityText").text, Is.EqualTo($"0 / {maximum * 2}"));

            for (int count = 0; count <= 3; count++)
            {
                area.guardParty.Clear();
                area.guardParty.AddRange(guards.Take(count));
                view.Refresh();
                Assert.That(slots.Count(slot => slot.gameObject.activeSelf), Is.EqualTo(count));
            }
            clock.UnixSeconds++;
            view.Refresh();
            Assert.That(Get<TMP_Text>(view, "attackTimeText").text, Is.EqualTo("02:04"));
            area.enemyAttackPending = true;
            area.nextEnemyAttackUnixSeconds = 0;
            view.Refresh();
            Assert.That(Get<TMP_Text>(view, "attackTimeText").text, Is.EqualTo("00:00"));
            Assert.That(Get<GameObject>(view, "attackOn").activeSelf, Is.True);
            area.enemyAttackPending = false;
            view.Refresh();
            Assert.That(Get<GameObject>(view, "noAttack").activeSelf, Is.True);
            Assert.That(Get<GameObject>(view, "attackOn").activeSelf, Is.False);

            Get<Button>(view, "guardButton").onClick.Invoke();
            Assert.That(view.gameObject.activeSelf, Is.False);
            Assert.That(Get<GameObject>(controller, "partySelectionPopupRoot").activeSelf, Is.True);
            controller.ClosePartySelection();
            yield return null;
            Assert.That(view.IsVisible, Is.True, "Canceling guard selection must return to the report.");
            controller.OpenAreaReport((int)ExplorationEnvironment.FlorestaDasSombras);
            yield return null;
            Assert.That(Get<TMP_Text>(view, "locationText").text, Is.EqualTo("Floresta das Sombras"));
            Assert.That(Get<TMP_Text>(view, "explorerCountText").text, Is.EqualTo("1 / 2"));
            Assert.That(slots.All(slot => !slot.gameObject.activeSelf), Is.True);

            // A long reward table must increase the scroll content, not overlap rows.
            var longEncounter = ScriptableObject.Instantiate(encounter);
            var longCatalog = ScriptableObject.CreateInstance<ExplorationEncounterCatalog>();
            var resources = Enumerable.Range(0, 8).Select(index =>
            {
                var definition = ScriptableObject.Instantiate(resource);
                Set(definition, "persistenceId", "report.test." + index);
                Set(definition, "displayName", "Recurso " + index);
                return definition;
            }).ToArray();
            var entries = resources.Select(definition =>
            {
                var entry = new ExplorationRewardEntry();
                Set(entry, "resource", definition);
                return entry;
            }).ToArray();
            Set(longEncounter, "possibleRewards", entries);
            Set(longCatalog, "encounters", new[] { longEncounter });
            try
            {
                view.Show(session.Exploration, area.environment, longCatalog, definitions);
                yield return null;
                Canvas.ForceUpdateCanvases();
                ScrollRect scroll = Get<ScrollRect>(view, "rewardScroll");
                Assert.That(view.GetComponentsInChildren<ExplorationRewardItemView>().Length, Is.EqualTo(8));
                Assert.That(scroll.content.rect.height, Is.GreaterThan(scroll.viewport.rect.height));
                scroll.verticalNormalizedPosition = 0;
                Canvas.ForceUpdateCanvases();
                Assert.That(scroll.content.anchoredPosition.y, Is.GreaterThan(0));
            }
            finally
            {
                Object.Destroy(longCatalog);
                Object.Destroy(longEncounter);
                foreach (var definition in resources) Object.Destroy(definition);
            }
            Get<Button>(view, "backButton").onClick.Invoke();
            Assert.That(view.gameObject.activeSelf, Is.False);
            Assert.That(Get<GameObject>(controller, "environmentSelectionRoot").activeSelf, Is.True);
            controller.CloseModal();
        }

        private static T Get<T>(object owner, string field) => (T)owner.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);

        private static void Set(object owner, string field, object value) => owner.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);

        private static void Capture(ExplorationAreaReportView view)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                return;
            Canvas canvas = view.GetComponentInParent<Canvas>().rootCanvas;
            Camera camera = Camera.main;
            var mode = canvas.renderMode;
            var worldCamera = canvas.worldCamera;
            float planeDistance = canvas.planeDistance;
            int sortingOrder = canvas.sortingOrder;
            var target = new RenderTexture(1280, 720, 24);
            var previous = RenderTexture.active;
            var previousTarget = camera.targetTexture;
            var capture = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
                canvas.sortingOrder = 5000;
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                capture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                capture.Apply();
                string directory = Path.GetFullPath("Logs/AreaReportValidation");
                Directory.CreateDirectory(directory);
                File.WriteAllBytes(Path.Combine(directory, "Report.png"), capture.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode = mode;
                canvas.worldCamera = worldCamera;
                canvas.planeDistance = planeDistance;
                canvas.sortingOrder = sortingOrder;
                camera.targetTexture = previousTarget;
                RenderTexture.active = previous;
                target.Release();
                Object.Destroy(target);
                Object.Destroy(capture);
            }
        }
    }
}
