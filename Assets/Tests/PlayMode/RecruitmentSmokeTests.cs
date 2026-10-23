using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Recruitment;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TCCGame.UI;
using TCCGame.Gameplay.Campaign;
using System.Collections.Generic;
using System.Reflection;
using TCCGame.Gameplay.Navigation;

namespace TCCGame.Tests.PlayMode
{
    public sealed class RecruitmentSmokeTests
    {
        private bool hadLegacySave;
        private string legacySave;
        private bool hadCurrentSave;
        private string currentSave;

        [SetUp]
        public void SetUp()
        {
            hadLegacySave = PlayerPrefs.HasKey(
                GameSaveRepository.LegacyCitySaveKey
            );
            legacySave = hadLegacySave
                ? PlayerPrefs.GetString(GameSaveRepository.LegacyCitySaveKey)
                : null;
            hadCurrentSave = PlayerPrefs.HasKey(GameSaveRepository.SaveKey);
            currentSave = hadCurrentSave
                ? PlayerPrefs.GetString(GameSaveRepository.SaveKey)
                : null;

            PlayerPrefs.DeleteKey(GameSaveRepository.LegacyCitySaveKey);
            PlayerPrefs.DeleteKey(GameSaveRepository.SaveKey);

            GameSaveData data = GameSaveData.CreateDefault();
            data.money = 1000;
            data.recruitment.initialized = true;
            data.recruitment.availableWarriors = 12;
            data.recruitment.availableArchers = 15;
            data.recruitment.availableMages = 8;
            data.recruitment.nextRestockUnixSeconds =
                System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;
            new GameSaveRepository().Save(data);
            GameSessionProvider.Reset();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() &&
                activeScene.isLoaded &&
                (activeScene.name == "Portão" || activeScene.name == "Centro"))
            {
                Scene cleanupScene = SceneManager.CreateScene(
                    "Recruitment smoke cleanup"
                );
                SceneManager.SetActiveScene(cleanupScene);
                AsyncOperation unload = SceneManager.UnloadSceneAsync(
                    activeScene
                );

                while (unload != null && !unload.isDone)
                {
                    yield return null;
                }
            }

            RestoreSave(
                GameSaveRepository.LegacyCitySaveKey,
                hadLegacySave,
                legacySave
            );
            RestoreSave(
                GameSaveRepository.SaveKey,
                hadCurrentSave,
                currentSave
            );
            PlayerPrefs.Save();
            GameSessionProvider.Reset();
        }

        [UnityTest]
        public IEnumerator RecruitmentMenu_CompletesItsCriticalFlow()
        {
            yield return SceneManager.LoadSceneAsync(
                "Portão",
                LoadSceneMode.Single
            );
            yield return null;

            NPCVenda vendor = Object.FindFirstObjectByType<NPCVenda>();
            PlayerInteractor interactor =
                Object.FindFirstObjectByType<PlayerInteractor>();

            Assert.That(vendor, Is.Not.Null);
            Assert.That(interactor, Is.Not.Null);
            GoldBalancePresenter goldHud = Object.FindFirstObjectByType<GoldBalancePresenter>();
            Assert.That(goldHud.PixelGoldText, Is.Not.Null);
            Assert.That(goldHud.PixelGoldText.text, Is.EqualTo("1 000"));
            Assert.That(Object.FindFirstObjectByType<GateHudView>(), Is.Not.Null);
            InteractionPromptView promptView = Object.FindFirstObjectByType<InteractionPromptView>(
                FindObjectsInactive.Include);
            Assert.That(promptView, Is.Not.Null);

            vendor.SendMessage(
                "abrir_popup",
                SendMessageOptions.RequireReceiver
            );

            RecruitmentRowView[] rows = Object.FindObjectsByType<
                RecruitmentRowView
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            Assert.That(rows, Has.Length.EqualTo(3));
            Assert.That(interactor.HasModalOpen, Is.True);
            Assert.That(vendor.popup.activeSelf, Is.True);
            yield return null;
            Assert.That(promptView.GetComponentInParent<CanvasGroup>().alpha, Is.Zero,
                "Interaction prompt must disappear while a modal is open.");

            RecruitmentRowView warrior = rows.Single(
                row => row.UnitType == UnitType.Warrior
            );
            Button confirm = warrior.GetComponentsInChildren<Button>(true)
                .Single(button => button.name == "Confirm Purchase");

            int price = GameSessionProvider.Current.Recruitment
                .GetUnitPrice(UnitType.Warrior);
            confirm.onClick.Invoke();
            yield return null;

            Assert.That(
                GameSessionProvider.Current.Economy.Money,
                Is.EqualTo(1000 - price)
            );
            Assert.That(goldHud.PixelGoldText.text, Is.EqualTo((1000 - price).ToString()));
            Assert.That(
                GameSessionProvider.Current.Recruitment
                    .GetAvailable(UnitType.Warrior),
                Is.EqualTo(11)
            );
            Assert.That(
                GameSessionProvider.Current.Recruitment
                    .GetOwned(UnitType.Warrior),
                Is.EqualTo(1)
            );

            TMP_Text feedback = vendor.popup
                .GetComponentsInChildren<TMP_Text>(true)
                .Single(text => text.name == "Feedback");
            Assert.That(feedback.text, Does.Contain("disponíveis: 11"));

            vendor.CloseModal();
            Assert.That(vendor.popup.activeSelf, Is.False);
            Assert.That(interactor.HasModalOpen, Is.False);
        }

        private static void RestoreSave(
            string key,
            bool existed,
            string value
        )
        {
            if (existed)
            {
                PlayerPrefs.SetString(key, value);
                return;
            }

            PlayerPrefs.DeleteKey(key);
        }

        [UnityTest]
        public IEnumerator PortaoModals_TransferAcknowledgeCancelAndTravel()
        {
            var repository = new GameSaveRepository();
            GameSaveData data = repository.Load();
            data.army.Set(UnitType.Warrior, 30);
            data.campaign.gates.Single(gate => gate.gateId == GateId.North).warriors = 20;
            data.campaign.scheduledWaves.Clear();
            data.campaign.scheduledWaves.Add(new WaveSaveData
            {
                sequence = 1, gateId = GateId.North, arrivalStrategicSeconds = 1,
                threatPower = 1, raiders = 1, status = WaveStatus.Resolved,
                battleReport = new BattleReportSaveData
                {
                    resolved = true, outcome = BattleOutcome.Repelled, effectiveDefense = 20,
                    defenseRatio = 20, initialGateHealth = 100, finalGateHealth = 100,
                    initialWarriors = 20
                }
            });
            data.campaign.nextWaveSequence = 2;
            data.navigation.currentArea = CityArea.Gate;
            data.navigation.activeGateId = GateId.North;
            repository.Save(data);
            GameSessionProvider.Reset();
            yield return SceneManager.LoadSceneAsync("Portão", LoadSceneMode.Single);
            yield return null;

            PlayerInteractor player = Object.FindFirstObjectByType<PlayerInteractor>();
            GarrisonManagementPanel.Show(player.gameObject);
            yield return null;
            GarrisonManagementPanel panel = Object.FindFirstObjectByType<GarrisonManagementPanel>();
            Assert.That(GarrisonManagementPanel.IsVisible, Is.True);
            Assert.That(player.HasModalOpen, Is.True);
            Assert.That(GameSessionProvider.Current.Campaign.IsPaused, Is.False);
            Button Control(string name) => panel.GetComponentsInChildren<Button>(true).Single(button => button.name == name);
            TMP_Text Read(string name) => panel.GetComponentsInChildren<TMP_Text>(true).Single(text => text.name == name);
            Control("Receive 10 Warrior").onClick.Invoke();
            Assert.That(GameSessionProvider.Current.TroopDeployment.GetReserveCount(UnitType.Warrior), Is.EqualTo(20));
            Assert.That(Read("Warrior Reserve").text, Is.EqualTo("20"));
            Control("Send 1 Warrior").onClick.Invoke();
            Assert.That(Read("Warrior Stationed").text, Is.EqualTo("19"));
            Assert.That(GameSessionProvider.Current.TroopDeployment.InTransit.Count, Is.EqualTo(2));
            Assert.That(Control("Send 1 Archer").interactable, Is.False);
            Assert.That(Control("Receive 10 Mage").interactable, Is.False);
            Control("Change Destination").onClick.Invoke();
            Assert.That(Read("Destination").text, Does.Contain("Portão"));
            Assert.That(Control("Acknowledge Report").gameObject.activeSelf, Is.True);
            Control("Acknowledge Report").onClick.Invoke();
            Assert.That(GameSessionProvider.Current.Campaign.GetLatestBattleReport(GateId.North).Value.ReportAcknowledged, Is.True);
            Assert.That(Control("Acknowledge Report").gameObject.activeSelf, Is.False);
            Control("Close").onClick.Invoke();
            Assert.That(player.HasModalOpen, Is.False);

            CityTravelNpc npc = Object.FindFirstObjectByType<CityTravelNpc>();
            TravelConversationPanel.Show(npc, player.gameObject);
            Assert.That(TravelConversationPanel.IsVisible, Is.True);
            Assert.That(player.HasModalOpen, Is.True);
            TravelConversationPanel travel = Object.FindFirstObjectByType<TravelConversationPanel>();
            Button TravelControl(string name) => travel.GetComponentsInChildren<Button>(true).Single(button => button.name == name);
            Assert.That(travel.GetComponentsInChildren<TMP_Text>(true).Single(text => text.name == "Travel Destination").text,
                Is.EqualTo("CENTRO DA CIDADE"));
            TravelControl("Cancel").onClick.Invoke();
            Assert.That(TravelConversationPanel.IsVisible, Is.False);
            Assert.That(player.HasModalOpen, Is.False);
            TravelConversationPanel.Show(npc, player.gameObject);
            TravelControl("Confirm").onClick.Invoke();
            Assert.That(TravelConversationPanel.IsVisible, Is.False);
            float deadline = Time.realtimeSinceStartup + 15f;
            while ((SceneTransitionController.IsTransitioning || SceneManager.GetActiveScene().name != "Centro") &&
                Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Centro"));
            Assert.That(SceneTransitionController.IsTransitioning, Is.False);
            Assert.That(GameSessionProvider.Current.Navigation.Current.IsAtGate, Is.False);
        }

        [UnityTest]
        public IEnumerator PortaoAlerts_ResumeAfterModalAndDismissFeedback()
        {
            yield return SceneManager.LoadSceneAsync("Portão", LoadSceneMode.Single);
            yield return null;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            GlobalThreatHud hud = Object.FindFirstObjectByType<GlobalThreatHud>();
            Assert.That(hud, Is.Not.Null);
            var queue = (Queue<WaveAlertMessage>)typeof(GlobalThreatHud)
                .GetField("alertQueue", flags).GetValue(hud);
            FieldInfo configField = typeof(GlobalThreatHud).GetField("config", flags);
            var originalConfig = (WaveAlertPresentationConfig)configField.GetValue(hud);
            var testConfig = Object.Instantiate(originalConfig);
            testConfig.bannerDurationSeconds = .5f;
            testConfig.enableAudio = false;
            configField.SetValue(hud, testConfig);
            try
            {
                queue.Clear();
                typeof(GlobalThreatHud).GetField("activeAlert", flags).SetValue(hud, null);
                queue.Enqueue(new WaveAlertMessage(WaveWarningStage.Announced,
                    GameSessionProvider.Current.Navigation.Current.ActiveGateId,
                    "Batedores: inimigos avançam para o Portão Norte."));
                yield return null;
                CanvasGroup group = hud.GetComponentInChildren<CanvasGroup>(true);
                Assert.That(group.alpha, Is.EqualTo(1f));
                Assert.That(hud.GetComponentsInChildren<TMP_Text>(true)
                    .Single(text => text.name == "Alert Message").text, Does.Contain("inimigos"));

                NPCVenda vendor = Object.FindFirstObjectByType<NPCVenda>();
                vendor.SendMessage("abrir_popup", SendMessageOptions.RequireReceiver);
                yield return new WaitForSecondsRealtime(.65f);
                Assert.That(group.alpha, Is.Zero);
                vendor.CloseModal();
                yield return null;
                Assert.That(group.alpha, Is.EqualTo(1f), "An interrupted alert must resume after closing the modal.");
                yield return new WaitForSecondsRealtime(.7f);
                Assert.That(group.alpha, Is.Zero, "The resumed alert must expire normally.");

                CaptainNpc captain = Object.FindFirstObjectByType<CaptainNpc>();
                Canvas marker = captain.GetComponentInChildren<Canvas>(true);
                Assert.That(marker.renderMode, Is.EqualTo(RenderMode.WorldSpace));
                SpriteRenderer sprite = captain.GetComponentInChildren<SpriteRenderer>();
                Assert.That(marker.transform.position.y, Is.GreaterThan(sprite.bounds.max.y));

                AlertPopup feedback = Object.FindFirstObjectByType<AlertPopup>();
                feedback.Show("Tropas insuficientes na reserva.");
                TMP_Text message = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None).Single(text => text.name == "Feedback Message");
                Assert.That(message.text, Does.Contain("Tropas insuficientes"));
                Canvas feedbackCanvas = message.GetComponentInParent<Canvas>();
                Assert.That(feedbackCanvas.gameObject.activeSelf, Is.True);
                feedbackCanvas.GetComponentInChildren<Button>(true).onClick.Invoke();
                Assert.That(feedbackCanvas.gameObject.activeSelf, Is.False);
                feedback.Show("Novo aviso.");
                float duration = (float)typeof(AlertPopup).GetField("visibleDurationSeconds", flags).GetValue(feedback);
                yield return new WaitForSeconds(duration + .15f);
                Assert.That(feedbackCanvas.gameObject.activeSelf, Is.False, "Legacy automatic dismissal must remain functional.");
            }
            finally
            {
                configField.SetValue(hud, originalConfig);
                Object.Destroy(testConfig);
            }
        }
    }
}
