using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Player;
using TCCGame.Gameplay.Recruitment;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TCCGame.Tests.PlayMode
{
    public class LegacyFlowCharacterizationTests
    {
        private readonly List<VendorHarness> vendorHarnesses = new();
        private readonly List<GameObject> otherObjects = new();

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

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (VendorHarness harness in vendorHarnesses)
            {
                harness.Dispose();
            }

            foreach (GameObject gameObject in otherObjects)
            {
                if (gameObject != null)
                {
                    UnityEngine.Object.Destroy(gameObject);
                }
            }

            vendorHarnesses.Clear();
            otherObjects.Clear();
            yield return null;

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

        [UnityTest]
        public IEnumerator AdventurerPurchase_UsesPanelAndPersistentInventory()
        {
            ConfigureGame(data => data.money = 200);

            var player = new GameObject("Adventurer buyer");
            otherObjects.Add(player);
            player.SetActive(false);
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<moviment>();
            PlayerInteractor interactor =
                player.AddComponent<PlayerInteractor>();
            AdventurerRecruitmentController controller =
                player.AddComponent<AdventurerRecruitmentController>();

            var panelObject = new GameObject("Adventurer recruitment panel");
            otherObjects.Add(panelObject);
            panelObject.SetActive(false);
            AdventurerRecruitmentPanel panel =
                panelObject.AddComponent<AdventurerRecruitmentPanel>();
            Button purchaseButton = new GameObject("Purchase button")
                .AddComponent<Button>();
            purchaseButton.transform.SetParent(panelObject.transform);
            SetPrivateField(panel, "panelRoot", panelObject);
            SetPrivateField(panel, "purchaseButton", purchaseButton);
            SetPrivateField(controller, "panel", panel);
            player.SetActive(true);

            var npcObject = new GameObject("Recruitable adventurer");
            otherObjects.Add(npcObject);
            npcObject.SetActive(false);
            Aventureiro adventurer = npcObject.AddComponent<Aventureiro>();
            Char_base definition =
                ScriptableObject.CreateInstance<Char_base>();
            definition.name = "adventurer.test";
            definition.preco = 150;
            adventurer.charBase = definition;
            npcObject.SetActive(true);
            yield return null;

            controller.Open(adventurer);
            Assert.That(interactor.HasModalOpen, Is.True);
            purchaseButton.onClick.Invoke();

            controller.Open(adventurer);
            purchaseButton.onClick.Invoke();

            Assert.That(Session.Economy.Money, Is.EqualTo(50));
            Assert.That(interactor.HasModalOpen, Is.False);
            Assert.That(
                Session.Inventory.IsAdventurerRecruited(
                    "adventurer.test"
                ),
                Is.True
            );

            GameSaveData persisted = new GameSaveRepository().Load();
            Assert.That(
                persisted.playerInventory.recruitedAdventurerIds,
                Is.EquivalentTo(new[] { "adventurer.test" })
            );

            UnityEngine.Object.Destroy(definition);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Vendor_QuantityOnePreservesPricesAndUpdatesView()
        {
            ConfigureGame(data =>
            {
                data.money = 100;
                data.recruitment.availableArchers = 5;
            });
            VendorHarness harness = CreateVendorHarness();

            harness.Vendor.Purchase(UnitType.Archer, 1);

            Assert.That(Session.Economy.Money, Is.EqualTo(95));
            Assert.That(
                Session.Recruitment.GetOwned(UnitType.Archer),
                Is.EqualTo(1)
            );
            Assert.That(
                Session.Recruitment.GetAvailable(UnitType.Archer),
                Is.EqualTo(4)
            );
            Assert.That(harness.GoldText.text, Is.EqualTo("Ouro: 95"));
            Assert.That(
                harness.ArcherStockText.text,
                Is.EqualTo("Arqueiros: 4")
            );

            yield return null;
        }

        [UnityTest]
        public IEnumerator Vendor_OutOfStockLeavesTransactionUntouched()
        {
            ConfigureGame(data =>
            {
                data.money = 100;
                data.recruitment.availableWarriors = 0;
            });
            VendorHarness harness = CreateVendorHarness();

            harness.Vendor.Purchase(UnitType.Warrior, 1);

            Assert.That(Session.Economy.Money, Is.EqualTo(100));
            Assert.That(
                Session.Recruitment.GetOwned(UnitType.Warrior),
                Is.Zero
            );
            Assert.That(
                Session.Recruitment.GetAvailable(UnitType.Warrior),
                Is.Zero
            );

            yield return null;
        }

        [UnityTest]
        public IEnumerator Vendor_MultipleUnitsChargeTheTotalPrice()
        {
            ConfigureGame(data =>
            {
                data.money = 100;
                data.recruitment.availableArchers = 20;
            });
            VendorHarness harness = CreateVendorHarness();

            harness.Vendor.Purchase(UnitType.Archer, 10);

            Assert.That(Session.Economy.Money, Is.EqualTo(50));
            Assert.That(
                Session.Recruitment.GetOwned(UnitType.Archer),
                Is.EqualTo(10)
            );
            Assert.That(
                Session.Recruitment.GetAvailable(UnitType.Archer),
                Is.EqualTo(10)
            );

            yield return null;
        }

        [UnityTest]
        public IEnumerator Vendor_InsufficientMoneyRejectsAtomically()
        {
            ConfigureGame(data =>
            {
                data.money = 0;
                data.recruitment.availableMages = 5;
            });
            VendorHarness harness = CreateVendorHarness();

            harness.Vendor.Purchase(UnitType.Mage, 1);

            Assert.That(Session.Economy.Money, Is.Zero);
            Assert.That(
                Session.Recruitment.GetOwned(UnitType.Mage),
                Is.Zero
            );
            Assert.That(
                Session.Recruitment.GetAvailable(UnitType.Mage),
                Is.EqualTo(5)
            );

            yield return null;
        }

        [UnityTest]
        public IEnumerator Vendor_ZeroQuantityIsRejected()
        {
            ConfigureGame(data =>
            {
                data.money = 100;
                data.recruitment.availableArchers = 5;
            });
            VendorHarness harness = CreateVendorHarness();

            harness.Vendor.Purchase(UnitType.Archer, 0);

            Assert.That(Session.Economy.Money, Is.EqualTo(100));
            Assert.That(
                Session.Recruitment.GetOwned(UnitType.Archer),
                Is.Zero
            );
            Assert.That(
                Session.Recruitment.GetAvailable(UnitType.Archer),
                Is.EqualTo(5)
            );

            yield return null;
        }

        [UnityTest]
        public IEnumerator Vendor_EmptyInputIsRejectedWithoutException()
        {
            ConfigureGame(data => data.money = 100);
            VendorHarness harness = CreateVendorHarness();
            RecruitmentRowView row = harness.CreateInputRow(
                UnitType.Mage,
                string.Empty
            );

            Assert.DoesNotThrow(row.SubmitInput);
            Assert.That(
                Session.Recruitment.GetOwned(UnitType.Mage),
                Is.Zero
            );

            yield return null;
        }

        [UnityTest]
        public IEnumerator Vendor_PartiallyNumericInputIsRejectedWithoutException()
        {
            ConfigureGame(data => data.money = 100);
            VendorHarness harness = CreateVendorHarness();
            RecruitmentRowView row = harness.CreateInputRow(
                UnitType.Archer,
                "1 inválido"
            );

            Assert.DoesNotThrow(row.SubmitInput);
            Assert.That(
                Session.Recruitment.GetOwned(UnitType.Archer),
                Is.Zero
            );

            yield return null;
        }

        [UnityTest]
        public IEnumerator Vendor_RestockUsesOnlyBaseStockAndBuildingLevel()
        {
            ConfigureGame(data =>
            {
                data.SetBuildingLevel(BuildingType.ArcherStand, 2);
                data.recruitment.availableArchers = 500;
                data.recruitment.nextRestockUnixSeconds = 0;
            });
            VendorHarness harness = CreateVendorHarness();

            InvokePrivateMethod(harness.Vendor, "abrir_popup");

            Assert.That(
                Session.Recruitment.GetAvailable(UnitType.Archer),
                Is.EqualTo(16)
            );

            yield return null;
        }

        private static GameSession Session => GameSessionProvider.Current;

        private static void ConfigureGame(Action<GameSaveData> configure)
        {
            GameSaveData data = GameSaveData.CreateDefault();
            data.recruitment.initialized = true;
            data.recruitment.nextRestockUnixSeconds =
                DateTimeOffset.UtcNow.ToUnixTimeSeconds() +
                RecruitmentService.RestockIntervalSeconds;
            configure(data);

            new GameSaveRepository().Save(data);
            GameSessionProvider.Reset();
        }

        private VendorHarness CreateVendorHarness()
        {
            var harness = new VendorHarness();
            vendorHarnesses.Add(harness);
            return harness;
        }

        private static void InvokePrivateMethod(
            NPCVenda vendor,
            string methodName
        )
        {
            MethodInfo method = typeof(NPCVenda).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.That(method, Is.Not.Null);
            method.Invoke(vendor, null);
        }

        private static void SetPrivateField<T>(
            object target,
            string fieldName,
            T value
        )
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        private sealed class VendorHarness
        {
            private readonly GameObject root;

            public NPCVenda Vendor { get; }
            public Text GoldText { get; }
            public TMP_Text ArcherStockText { get; }

            public VendorHarness()
            {
                root = new GameObject("Vendor test harness");

                GoldText = CreateText("OuroValue");
                ArcherStockText = CreateTmpText("ArcherStock");
                TMP_Text warriorStockText =
                    CreateTmpText("WarriorStock");
                TMP_Text mageStockText = CreateTmpText("MageStock");
                TMP_Text timeText = CreateTmpText("Time");

                GoldBalancePresenter goldPresenter =
                    CreateObject("Gold Balance Presenter")
                        .AddComponent<GoldBalancePresenter>();
                SetPrivateField(goldPresenter, "goldText", GoldText);

                var popupObject = CreateObject("Popup");
                var alert = CreateObject("Alert").AddComponent<AlertPopup>();
                SetPrivateField(alert, "popupRoot", popupObject);
                SetPrivateField(alert, "messageText", CreateText("AlertText"));

                Vendor = CreateObject("Vendor").AddComponent<NPCVenda>();
                Vendor.alerta = alert;
                Vendor.popup = CreateObject("VendorPopup");
                Vendor.arqueiro_texto = ArcherStockText;
                Vendor.guerreiro_texto = warriorStockText;
                Vendor.mago_texto = mageStockText;
                Vendor.tempo_texto = timeText;
            }

            public RecruitmentRowView CreateInputRow(
                UnitType unitType,
                string value
            )
            {
                GameObject rowObject = CreateObject($"{unitType}Row");
                RecruitmentRowView row =
                    rowObject.AddComponent<RecruitmentRowView>();
                SetPrivateField(row, "unitType", unitType);

                GameObject inputObject = new GameObject("Input");
                inputObject.transform.SetParent(rowObject.transform);
                var input = inputObject.AddComponent<TMP_InputField>();
                input.text = value;
                SetPrivateField(row, "quantityInput", input);
                return row;
            }

            public void Dispose()
            {
                if (Vendor != null)
                {
                    Vendor.StopAllCoroutines();
                }

                UnityEngine.Object.Destroy(root);
            }

            private GameObject CreateObject(string objectName)
            {
                var gameObject = new GameObject(objectName);
                gameObject.transform.SetParent(root.transform);
                return gameObject;
            }

            private Text CreateText(string objectName)
            {
                GameObject textObject = CreateObject(objectName);
                textObject.AddComponent<CanvasRenderer>();
                return textObject.AddComponent<Text>();
            }

            private TMP_Text CreateTmpText(string objectName)
            {
                GameObject textObject = CreateObject(objectName);
                textObject.AddComponent<CanvasRenderer>();
                return textObject.AddComponent<TextMeshProUGUI>();
            }
        }
    }
}
