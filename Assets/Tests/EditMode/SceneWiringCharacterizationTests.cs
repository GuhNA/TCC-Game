using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TCCGame.Tests.EditMode
{
    public class SceneWiringCharacterizationTests
    {
        private SceneSetup[] previousSceneSetup;

        [SetUp]
        public void SetUp()
        {
            previousSceneSetup = EditorSceneManager.GetSceneManagerSetup();
        }

        [TearDown]
        public void TearDown()
        {
            if (previousSceneSetup != null &&
                previousSceneSetup.Any(setup => setup.isLoaded))
            {
                EditorSceneManager.RestoreSceneManagerSetup(
                    previousSceneSetup
                );
                return;
            }

            EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single
            );
        }

        [Test]
        public void CentroScene_HasFourUniqueBuildingsAndCoreReferences()
        {
            Scene scene = EditorSceneManager.OpenScene(
                "Assets/Scenes/Centro.unity",
                OpenSceneMode.Single
            );

            CityManager cityManager = ComponentsInScene<CityManager>(scene)
                .Single();
            SerializedObject serializedManager =
                new SerializedObject(cityManager);

            SerializedProperty panel =
                serializedManager.FindProperty("buildingPanel");
            SerializedProperty buildings =
                serializedManager.FindProperty("buildings");

            var buildingTypes = new HashSet<BuildingType>();

            for (int index = 0; index < buildings.arraySize; index++)
            {
                var building = (BuildingBehaviour)buildings
                    .GetArrayElementAtIndex(index)
                    .objectReferenceValue;

                Assert.That(building, Is.Not.Null);
                Assert.That(building.Definition, Is.Not.Null);
                buildingTypes.Add(building.Definition.type);
            }

            TaxAccrualService taxService =
                ComponentsInScene<TaxAccrualService>(scene).Single();
            SerializedObject serializedTax = new SerializedObject(taxService);
            GoldBalancePresenter goldPresenter =
                ComponentsInScene<GoldBalancePresenter>(scene).Single();

            Assert.That(panel.objectReferenceValue, Is.Not.Null);
            Assert.That(buildings.arraySize, Is.EqualTo(4));
            Assert.That(buildingTypes.Count, Is.EqualTo(4));
            Assert.That(
                serializedTax.FindProperty("localMarket")
                    .objectReferenceValue,
                Is.Not.Null
            );
            Assert.That(
                ComponentsInScene<moviment>(scene).Count(),
                Is.EqualTo(1)
            );
            AssertGoldHudWiring(goldPresenter);
        }

        [Test]
        public void PortaoScene_PreservesVendorReferencesAndButtonContracts()
        {
            Scene scene = EditorSceneManager.OpenScene(
                "Assets/Scenes/Portão.unity",
                OpenSceneMode.Single
            );

            NPCVenda vendor = ComponentsInScene<NPCVenda>(scene).Single();
            GoldBalancePresenter goldPresenter =
                ComponentsInScene<GoldBalancePresenter>(scene).Single();

            RecruitmentRowView[] rows =
                vendor.popup.GetComponentsInChildren<RecruitmentRowView>(true);
            Assert.That(rows, Has.Length.EqualTo(3));

            int rowButtonCalls = 0;
            foreach (Button button in
                     vendor.popup.GetComponentsInChildren<Button>(true))
            {
                for (int index = 0;
                     index < button.onClick.GetPersistentEventCount();
                     index++)
                {
                    if (button.onClick.GetPersistentTarget(index)
                        is RecruitmentRowView)
                    {
                        rowButtonCalls++;
                        Assert.That(
                            new[] {
                                nameof(RecruitmentRowView.PurchaseOne),
                                nameof(RecruitmentRowView.PurchaseTen),
                                nameof(RecruitmentRowView.PurchaseHundred),
                                nameof(RecruitmentRowView.SubmitInput)
                            },
                            Does.Contain(
                                button.onClick.GetPersistentMethodName(index)
                            )
                        );
                    }
                }
            }

            Assert.That(vendor.alerta, Is.Not.Null);
            Assert.That(vendor.popup, Is.Not.Null);
            Assert.That(vendor.arqueiro_texto, Is.Not.Null);
            Assert.That(vendor.guerreiro_texto, Is.Not.Null);
            Assert.That(vendor.mago_texto, Is.Not.Null);
            Assert.That(vendor.tempo_texto, Is.Not.Null);
            AssertSaleText(vendor.arqueiro_texto);
            AssertSaleText(vendor.guerreiro_texto);
            AssertSaleText(vendor.mago_texto);
            AssertSaleText(vendor.tempo_texto);

            Assert.That(
                vendor.popup.GetComponentsInChildren<Text>(true),
                Is.Empty
            );
            Assert.That(
                vendor.popup.GetComponentsInChildren<InputField>(true),
                Is.Empty
            );

            foreach (TMP_InputField input in
                     vendor.popup.GetComponentsInChildren<TMP_InputField>(true))
            {
                TMP_Text inputText = input.textComponent as TMP_Text;
                Assert.That(inputText, Is.Not.Null);
                AssertRenderableText(inputText, requireOpaque: false);
                TMP_Text placeholder = input.placeholder as TMP_Text;
                if (placeholder != null)
                {
                    AssertRenderableText(placeholder, requireOpaque: false);
                }
            }

            foreach (TMP_Text saleText in
                     vendor.popup.GetComponentsInChildren<TMP_Text>(true))
            {
                AssertRenderableText(saleText, requireOpaque: false);
            }
            AssertGoldHudWiring(goldPresenter);
            Assert.That(rowButtonCalls, Is.EqualTo(12));
            Assert.That(
                ComponentsInScene<PlayerInteractor>(scene).Count(),
                Is.EqualTo(1)
            );
            Assert.That(
                ComponentsInScene<moviment>(scene).Count(),
                Is.EqualTo(1)
            );
        }

        [Test]
        public void ProjectInputActions_DefinesSharedMovementAndInteraction()
        {
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<
                InputActionAsset
            >("Assets/InputSystem_Actions.inputactions");

            Assert.That(actions, Is.Not.Null);

            InputAction move = actions.FindAction(
                "Player/Move",
                throwIfNotFound: false
            );
            InputAction interact = actions.FindAction(
                "Player/Interact",
                throwIfNotFound: false
            );

            Assert.That(move, Is.Not.Null);
            Assert.That(interact, Is.Not.Null);
            Assert.That(
                interact.bindings.Any(
                    binding => binding.path == "<Keyboard>/e"
                ),
                Is.True
            );
        }

        private static IEnumerable<T> ComponentsInScene<T>(Scene scene)
            where T : Component
        {
            return scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true));
        }

        private static void AssertGoldHudWiring(
            GoldBalancePresenter presenter
        )
        {
            var serializedPresenter = new SerializedObject(presenter);
            var goldText = (Text)serializedPresenter
                .FindProperty("goldText")
                .objectReferenceValue;
            GoldBalancePresenter prefabSource = PrefabUtility
                .GetCorrespondingObjectFromSource(presenter);

            Assert.That(goldText, Is.Not.Null);
            Assert.That(goldText.canvas, Is.Not.Null);
            Assert.That(goldText.raycastTarget, Is.False);
            Assert.That(prefabSource, Is.Not.Null);
            Assert.That(
                AssetDatabase.GetAssetPath(prefabSource),
                Is.EqualTo("Assets/Prefabs/UI/GoldBalanceHUD.prefab")
            );
        }

        private static void AssertSaleText(TMP_Text text)
        {
            Assert.That(text, Is.Not.Null);
            Assert.That(text.font, Is.Not.Null);
            Assert.That(text.fontSharedMaterial, Is.Not.Null);
            Assert.That(text.color, Is.EqualTo(Color.white));
            Assert.That(text.raycastTarget, Is.False);
        }

        private static void AssertRenderableText(
            TMP_Text text,
            bool requireOpaque = true
        )
        {
            Assert.That(text, Is.Not.Null);
            Assert.That(text.font, Is.Not.Null);
            Assert.That(text.fontSharedMaterial, Is.Not.Null);
            if (requireOpaque)
            {
                Assert.That(text.color.a, Is.GreaterThan(0.99f));
            }
            else
            {
                Assert.That(text.color.a, Is.GreaterThan(0f));
            }
            Assert.That(text.raycastTarget, Is.False);
        }
    }
}
