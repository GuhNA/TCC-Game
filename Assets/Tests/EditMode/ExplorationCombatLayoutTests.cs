using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using TCCGame.Gameplay.Exploration;
using UnityEditor;
using UnityEngine;

namespace TCCGame.Tests.EditMode
{
    public sealed class ExplorationCombatLayoutTests
    {
        private readonly List<Object> cleanup = new();
        private Sprite sprite;
        private TMP_FontAsset font;

        [SetUp]
        public void SetUp()
        {
            var texture = new Texture2D(16, 16);
            cleanup.Add(texture);
            sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), Vector2.one * 0.5f, 16);
            cleanup.Add(sprite);
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/Fonts/PixelifySans/PixelifySans SDF.asset");
            Assert.That(font, Is.Not.Null);
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = cleanup.Count - 1; index >= 0; index--)
                if (cleanup[index] != null) Object.DestroyImmediate(cleanup[index]);
            cleanup.Clear();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SmallCharacter_ReservesWholeHealthBarEvenAtZeroHealth(bool canvasBar)
        {
            var view = CreateSlot(0, 0.3f, 3.5f, "I", canvasBar);
            Assert.That(view.TryGetSlotWorldBounds(out var full), Is.True);
            Assert.That(full.size.x, Is.EqualTo(3.5f).Within(0.001f));
            view.Combatant.ApplyDamage(100);
            view.Refresh();
            Assert.That(view.TryGetSlotWorldBounds(out var empty), Is.True);
            Assert.That(empty.min.x, Is.EqualTo(full.min.x).Within(0.001f));
            Assert.That(empty.max.x, Is.EqualTo(full.max.x).Within(0.001f));
        }

        [Test]
        public void LegacyFillOnlyBar_KeepsItsOriginalWidthAfterDamage()
        {
            var view = CreateSlot(0, 0.3f, 3.5f, "I", false);
            Set(view, "healthEmptyRenderer", null);
            view.Combatant.ApplyDamage(100);
            view.Refresh();
            Assert.That(Get<Transform>(view, "healthFill").localScale.x, Is.Zero);
            Assert.That(view.TryGetSlotWorldBounds(out var bounds), Is.True);
            Assert.That(bounds.size.x, Is.EqualTo(3.5f).Within(0.001f));
        }

        [Test]
        public void WideCharacter_UsesSpriteInsteadOfSmallerBarAndName()
        {
            var view = CreateSlot(0, 6f, 2f, "I", true);
            Assert.That(view.TryGetSlotWorldBounds(out var bounds), Is.True);
            Assert.That(bounds.size.x, Is.EqualTo(6f).Within(0.001f));
        }

        [Test]
        public void LongName_IsMeasuredImmediatelyWithoutUsingItsTextContainerWidth()
        {
            var view = CreateSlot(0, 0.3f, 1f, "Goblin Ligeiro Com Nome Muito Comprido", true);
            TMP_Text label = Get<TMP_Text>(view, "nameText");
            label.fontSize = 15;
            label.text = "Nome atualizado antes de renderizar o primeiro frame";
            Assert.That(view.TryGetSlotWorldBounds(out var bounds), Is.True);
            Bounds renderedName = label.GetComponent<Renderer>().bounds;
            Assert.That(renderedName.size.x, Is.GreaterThan(1f));
            Assert.That(bounds.min.x, Is.EqualTo(renderedName.min.x).Within(0.001f));
            Assert.That(bounds.max.x, Is.EqualTo(renderedName.max.x).Within(0.001f));
            Assert.That(bounds.size.x, Is.LessThan(label.rectTransform.rect.width));
        }

        [TestCase(0f, false)]
        [TestCase(0.25f, true)]
        [TestCase(0.8f, true)]
        public void MixedTeam_PreservesConfiguredGapAndTeamCenter(float spacing, bool reverseInput)
        {
            var views = new[]
            {
                CreateSlot(-3, 0.3f, 3.5f, "I", true),
                CreateSlot(0, 5f, 2f, "I", true),
                CreateSlot(3, 0.3f, 1f, "Goblin Ligeiro Com Nome Comprido", true)
            };
            var label = Get<TMP_Text>(views[2], "nameText");
            label.fontSize = 15;
            label.transform.localPosition = new Vector3(0.4f, 2, 0);
            views[1].transform.localScale = new Vector3(1.4f, 0.8f, 1f);
            var controllerObject = new GameObject("Layout controller");
            cleanup.Add(controllerObject);
            var controller = controllerObject.AddComponent<ExplorationCombatSceneController>();
            Set(controller, "combatantSlotSpacing", spacing);
            var input = reverseInput ? views.Reverse().ToArray() : views;
            Invoke(controller, "ArrangeTeamByVisualSize", (object)input);

            Assert.That(views.Average(view => view.transform.position.x), Is.EqualTo(0f).Within(0.001f));
            for (int index = 1; index < views.Length; index++)
            {
                Bounds left = IndependentBounds(views[index - 1]);
                Bounds right = IndependentBounds(views[index]);
                Assert.That(right.min.x - left.max.x, Is.EqualTo(spacing).Within(0.001f));
            }
            Vector3[] positions = views.Select(view => view.transform.position).ToArray();
            Invoke(controller, "ArrangeTeamByVisualSize", (object)input);
            for (int index = 0; index < views.Length; index++)
                Assert.That(Vector3.Distance(positions[index], views[index].transform.position), Is.LessThan(0.001f));
        }

        [TestCase(ExplorationCombatTeam.Adventurers, 2)]
        [TestCase(ExplorationCombatTeam.Adventurers, 3)]
        [TestCase(ExplorationCombatTeam.Enemies, 2)]
        [TestCase(ExplorationCombatTeam.Enemies, 3)]
        public void Formation_BindsToSpatialSlotOrderEvenWithShuffledInspectorReferences(
            ExplorationCombatTeam team, int count)
        {
            var views = new[]
            {
                CreateSlot(-3, 0.3f, 3f, "I", true),
                CreateSlot(0, 0.3f, 3f, "I", true),
                CreateSlot(3, 0.3f, 3f, "I", true)
            };
            var front = new ExplorationBattleCombatant("front", "Front", team,
                100, 100, 1, 1, 1, 8, 2, DamageType.Physical);
            var rear = new ExplorationBattleCombatant("rear", "Rear", team,
                200, 200, 1, 1, 1, 1, 1, DamageType.Physical);
            var middle = new ExplorationBattleCombatant("middle", "Middle", team,
                50, 50, 1, 1, 1, 2, 8, DamageType.Physical);
            var other = new ExplorationBattleCombatant("other", "Other",
                team == ExplorationCombatTeam.Adventurers ? ExplorationCombatTeam.Enemies
                    : ExplorationCombatTeam.Adventurers, 10, 10, 1, 1, 1, 0, 0, DamageType.Physical);
            var input = new[] { front, rear, middle }.Take(count).ToArray();
            var battle = new ExplorationTurnBattle(
                team == ExplorationCombatTeam.Adventurers ? input : new[] { other },
                team == ExplorationCombatTeam.Enemies ? input : new[] { other }, 1);
            var controllerObject = new GameObject("Formation controller");
            cleanup.Add(controllerObject);
            var controller = controllerObject.AddComponent<ExplorationCombatSceneController>();
            var shuffledViews = new[] { views[2], null, views[0], views[1] };
            var combatants = team == ExplorationCombatTeam.Adventurers ? battle.Adventurers : battle.Enemies;
            Invoke(controller, "BindTeamViews", shuffledViews, combatants,
                Get<object>(controller, "enemyVisuals"));
            Invoke(controller, "ArrangeTeamByVisualSize", (object)shuffledViews);

            var activeViews = views.Where(view => view.gameObject.activeSelf)
                .OrderBy(view => view.transform.position.x).ToArray();
            Assert.That(activeViews.Length, Is.EqualTo(count));
            CollectionAssert.AreEqual(combatants, activeViews.Select(view => view.Combatant).ToArray());
            Assert.That(team == ExplorationCombatTeam.Adventurers
                ? activeViews.Last().Combatant : activeViews.First().Combatant, Is.SameAs(front));
        }

        private ExplorationCombatantView CreateSlot(float x, float bodyWidth, float barWidth,
            string name, bool canvasBar)
        {
            var root = new GameObject("Slot");
            cleanup.Add(root);
            root.transform.position = new Vector3(x, 0, 0);
            SpriteRenderer body = CreateSprite(root.transform, "Visual", bodyWidth);
            var bar = new GameObject("Health Bar");
            bar.transform.SetParent(root.transform, false);
            SpriteRenderer empty = CreateSprite(bar.transform, "Empty", barWidth);
            SpriteRenderer full = CreateSprite(bar.transform, "Full", barWidth);
            var nameObject = new GameObject("Name", typeof(RectTransform), typeof(TextMeshPro));
            nameObject.transform.SetParent(root.transform, false);
            TMP_Text label = nameObject.GetComponent<TMP_Text>();
            label.font = font;
            label.fontSize = 1;
            label.enableAutoSizing = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta = new Vector2(100, 2);
            var view = root.AddComponent<ExplorationCombatantView>();
            Set(view, "visualRoot", body.transform);
            Set(view, "fallbackRenderer", body);
            Set(view, "nameText", label);
            Set(view, "healthEmptyRenderer", empty);
            Set(view, "healthFullRenderer", full);
            Set(view, "healthFill", full.transform);
            Invoke(view, "CacheHealthBarInitialState");
            if (canvasBar) Invoke(view, "CreateCanvasHealthBar");
            view.Bind(new ExplorationBattleCombatant("test", name, ExplorationCombatTeam.Enemies,
                    100, 100, 1, 1, 1, 0, 0, DamageType.Physical),
                null, sprite, Vector3.zero, Vector3.zero, null);
            return view;
        }

        private SpriteRenderer CreateSprite(Transform parent, string name, float width)
        {
            var child = new GameObject(name, typeof(SpriteRenderer));
            child.transform.SetParent(parent, false);
            child.transform.localScale = new Vector3(width, 0.2f, 1);
            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            return renderer;
        }

        private static Bounds IndependentBounds(ExplorationCombatantView view)
        {
            Bounds bounds = Get<SpriteRenderer>(view, "fallbackRenderer").bounds;
            bounds.Encapsulate(Get<SpriteRenderer>(view, "healthEmptyRenderer").bounds);
            bounds.Encapsulate(Get<TMP_Text>(view, "nameText").GetComponent<Renderer>().bounds);
            return bounds;
        }

        private static T Get<T>(object owner, string field) => (T)owner.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
        private static void Set(object owner, string field, object value) => owner.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
        private static void Invoke(object owner, string method, params object[] arguments) => owner.GetType()
            .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, arguments);
    }
}
