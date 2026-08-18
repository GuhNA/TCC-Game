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

            int chooseCalls = 0;
            int calculateCalls = 0;
            int inputCalls = 0;

            foreach (Button button in ComponentsInScene<Button>(scene))
            {
                int chooseIndex = -1;
                int calculateIndex = -1;

                for (int index = 0;
                     index < button.onClick.GetPersistentEventCount();
                     index++)
                {
                    if (button.onClick.GetPersistentTarget(index) != vendor)
                    {
                        continue;
                    }

                    switch (button.onClick.GetPersistentMethodName(index))
                    {
                        case "compraEscolha":
                            chooseCalls++;
                            chooseIndex = index;
                            break;

                        case "vendaCalculo":
                            calculateCalls++;
                            calculateIndex = index;
                            break;

                        case "porInput":
                            inputCalls++;
                            break;
                    }
                }

                if (chooseIndex >= 0 && calculateIndex >= 0)
                {
                    Assert.That(
                        chooseIndex,
                        Is.LessThan(calculateIndex),
                        $"Button {button.name} must select the unit " +
                        "before calculating the purchase."
                    );
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

            foreach (Text legacyText in
                     vendor.popup.GetComponentsInChildren<Text>(true))
            {
                Assert.That(
                    legacyText.GetComponentInParent<InputField>(),
                    Is.Not.Null,
                    $"{legacyText.name} should use TextMesh Pro unless " +
                    "it belongs to a legacy InputField."
                );
            }

            foreach (TMP_Text saleText in
                     vendor.popup.GetComponentsInChildren<TMP_Text>(true))
            {
                Assert.That(saleText.font, Is.Not.Null);
                Assert.That(saleText.fontSharedMaterial, Is.Not.Null);
                Assert.That(saleText.color.a, Is.GreaterThan(0.99f));
                Assert.That(saleText.raycastTarget, Is.False);
            }
            AssertGoldHudWiring(goldPresenter);
            Assert.That(chooseCalls, Is.EqualTo(9));
            Assert.That(calculateCalls, Is.EqualTo(9));
            Assert.That(inputCalls, Is.EqualTo(3));
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
    }
}
