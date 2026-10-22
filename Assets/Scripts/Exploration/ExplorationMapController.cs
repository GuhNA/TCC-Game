using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Input;
using TCCGame.Gameplay.Navigation;
using TCCGame.Gameplay.Player;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Coordinates the world map and the party-selection popup. The catalog stores
    /// every Char_base definition, but the visible roster is rebuilt by matching
    /// PersistenceId values against the player's recruited IDs from the save.
    /// </summary>
    public sealed class ExplorationMapController : MonoBehaviour,
        IInteractable,
        IModalInteractionOwner
    {
        [Header("Map popup")]
        [FormerlySerializedAs("popupRoot")]
        [SerializeField] private GameObject mapPopupRoot;
        [Tooltip("Closes the complete map and returns control to the player.")]
        [SerializeField] private Button closeMapButton;
        [Tooltip("Container named SelectEnviroment. Hidden while the party popup is open.")]
        [SerializeField] private GameObject environmentSelectionRoot;
        [SerializeField] private ExplorationDestinationButton[] destinationButtons;

        [Header("Party-selection popup")]
        [SerializeField] private GameObject partySelectionPopupRoot;
        [SerializeField] private RectTransform partyListContent;
        [SerializeField] private AdventurerSelectionCardView cardPrefab;
        [SerializeField] private TMP_Text selectedDestinationText;
        [SerializeField] private TMP_Text emptyRosterMessage;
        [SerializeField] private Button confirmPartyButton;
        [SerializeField] private TMP_Text confirmPartyButtonText;
        [SerializeField] private Button backButton;

        [Header("Captured-area management")]
        [Tooltip("Optional. If empty, buttons are cloned from Confirm at runtime.")]
        [SerializeField] private Button assignExplorerButton;
        [SerializeField] private Button removeExplorerButton;
        [SerializeField] private Button transportLootButton;
        [SerializeField] private bool createExplorerButtonsAtRuntime = true;

        [Header("Adventurer definitions")]
        [Tooltip("Complete catalog. Only recruited entries are shown at runtime.")]
        [FormerlySerializedAs("availableAdventurers")]
        [SerializeField] private Char_base[] adventurerCatalog;
        [SerializeField] private ExplorationEncounterCatalog encounterCatalog;
        [SerializeField] private bool autoPopulateCatalogInEditor = true;

        [Header("Rules and feedback")]
        [SerializeField, Range(
            ExplorationService.MinimumPartySize,
            ExplorationService.MaximumPartySize
        )]
        private int maximumPartySize = ExplorationService.MaximumPartySize;
        [SerializeField] private AlertPopup alert;

        private readonly HashSet<RecruitedAdventurer> selectedParty = new();
        private readonly Dictionary<RecruitedAdventurer, AdventurerSelectionCardView>
            cards = new();
        private readonly List<AdventurerSelectionCardView> spawnedCards = new();
        private List<RecruitedAdventurer> recruitedAdventurers = new();
        private PlayerInteractor interactor;
        private ExplorationEnvironment selectedEnvironment;
        private PanelMode panelMode;
        private ExplorationMissionSaveData visibleReport;

        private enum PanelMode
        {
            Mission,
            Report,
            Guard
        }

        public bool CanInteract => enabled && mapPopupRoot != null;
        public IReadOnlyList<RecruitedAdventurer> RecruitedAdventurers =>
            recruitedAdventurers.AsReadOnly();

        private void Awake()
        {
            partySelectionPopupRoot?.SetActive(false);
            if (confirmPartyButtonText == null && confirmPartyButton != null)
            {
                confirmPartyButtonText = confirmPartyButton
                    .GetComponentInChildren<TMP_Text>(true);
            }
            EnsureExplorerButtons();
            SetExplorerButtonsVisible(false);
            encounterCatalog ??= Resources.Load<ExplorationEncounterCatalog>(
                ExplorationEncounterCatalog.ResourcesPath
            );
        }

        private void OnEnable()
        {
            closeMapButton?.onClick.AddListener(CloseModal);
            confirmPartyButton?.onClick.AddListener(SendParty);
            backButton?.onClick.AddListener(ClosePartySelection);
            assignExplorerButton?.onClick.AddListener(AssignExplorer);
            removeExplorerButton?.onClick.AddListener(RemoveExplorer);
            transportLootButton?.onClick.AddListener(TransportExplorerLoot);
        }

        private void OnDisable()
        {
            closeMapButton?.onClick.RemoveListener(CloseModal);
            confirmPartyButton?.onClick.RemoveListener(SendParty);
            backButton?.onClick.RemoveListener(ClosePartySelection);
            assignExplorerButton?.onClick.RemoveListener(AssignExplorer);
            removeExplorerButton?.onClick.RemoveListener(RemoveExplorer);
            transportLootButton?.onClick.RemoveListener(TransportExplorerLoot);
            CloseModal();
        }

        private void Update()
        {
            ExplorationService service =
                GameSessionProvider.Current.Exploration;
            bool changed = service.RefreshArrivals();
            changed |= service.RefreshExplorerCollections(encounterCatalog);
            if (changed &&
                mapPopupRoot != null && mapPopupRoot.activeInHierarchy)
            {
                RefreshDestinationButtons();
                if (panelMode == PanelMode.Report &&
                    partySelectionPopupRoot != null &&
                    partySelectionPopupRoot.activeInHierarchy)
                {
                    OpenAreaReport((int)selectedEnvironment);
                }
            }
        }

        public void Interact(GameObject player)
        {
            if (mapPopupRoot == null)
            {
                return;
            }

            interactor = player == null
                ? null
                : player.GetComponent<PlayerInteractor>();
            mapPopupRoot.SetActive(true);
            partySelectionPopupRoot?.SetActive(false);
            SetEnvironmentSelectionEnabled(true);
            selectedParty.Clear();
            RefreshDestinationButtons();
            interactor?.AcquireModalLock(this);
            UiMenuState.SetOpen(this, true);
        }

        /// <summary>
        /// Callback for an Enviar button. The argument is the integer value of its
        /// ExplorationEnvironment. ExplorationDestinationButton configures this
        /// automatically; it can also be selected in a regular Button OnClick.
        /// </summary>
        public void OpenPartySelection(int environmentValue)
        {
            if (!Enum.IsDefined(
                    typeof(ExplorationEnvironment),
                    environmentValue
                ))
            {
                alert?.Show("Destino de exploração inválido.");
                return;
            }

            ExplorationEnvironment environment =
                (ExplorationEnvironment)environmentValue;
            if (!GameSessionProvider.Current.Exploration
                    .IsEnvironmentUnlocked(environment))
            {
                alert?.Show(
                    "Conclua as regiões anteriores para liberar este destino."
                );
                return;
            }

            if (GameSessionProvider.Current.Exploration.GetActiveMission() !=
                null)
            {
                alert?.Show(
                    "Conclua ou recue da expedição atual antes de enviar outra."
                );
                return;
            }

            selectedEnvironment = environment;
            panelMode = PanelMode.Mission;
            SetExplorerButtonsVisible(false);
            selectedParty.Clear();
            BuildRecruitedRoster();

            if (selectedDestinationText != null)
            {
                selectedDestinationText.text =
                    ExplorationProgression.GetDisplayName(environment);
            }

            partySelectionPopupRoot?.SetActive(true);
            SetEnvironmentSelectionEnabled(false);
            if (confirmPartyButton != null)
            {
                confirmPartyButton.gameObject.SetActive(true);
            }
            UpdateConfirmButton();
        }

        public void OpenAreaReport(int environmentValue)
        {
            if (!Enum.IsDefined(
                    typeof(ExplorationEnvironment),
                    environmentValue))
            {
                return;
            }

            selectedEnvironment = (ExplorationEnvironment)environmentValue;
            panelMode = PanelMode.Report;
            selectedParty.Clear();
            visibleReport = GameSessionProvider.Current.Exploration
                .GetLatestMissionForEnvironment(selectedEnvironment);
            BuildReportCards(visibleReport);

            if (selectedDestinationText != null)
            {
                selectedDestinationText.text =
                    "Relatório — " + ExplorationProgression.GetDisplayName(
                        selectedEnvironment
                    );
            }

            if (emptyRosterMessage != null)
            {
                emptyRosterMessage.gameObject.SetActive(true);
                emptyRosterMessage.text = BuildReportSummary(visibleReport) +
                    "\n" + BuildExplorerSummary();
            }

            if (visibleReport != null &&
                visibleReport.status == ExplorationMissionStatus.Returned)
            {
                GameSessionProvider.Current.Exploration.AcknowledgeReport(
                    visibleReport.id
                );
            }

            partySelectionPopupRoot?.SetActive(true);
            SetEnvironmentSelectionEnabled(false);
            SetConfirmButtonLabel("Definir guarda");
            if (confirmPartyButton != null)
            {
                confirmPartyButton.gameObject.SetActive(true);
                confirmPartyButton.interactable = true;
            }
            SetExplorerButtonsVisible(true);
            UpdateExplorerButtons();
        }

        // Compatibility with Inspector events from the original controller.
        public void SelectEnvironment(int environmentValue)
        {
            OpenPartySelection(environmentValue);
        }

        /// <summary>
        /// Called by a destination whose travelling party has arrived. The same
        /// map button changes from Enviar to Batalhar, then loads the reusable
        /// combat scene with the mission saved as InBattle.
        /// </summary>
        public void EnterBattle(int environmentValue)
        {
            if (!Enum.IsDefined(
                    typeof(ExplorationEnvironment),
                    environmentValue
                ))
            {
                alert?.Show("Destino de batalha inválido.");
                return;
            }

            ExplorationEnvironment environment =
                (ExplorationEnvironment)environmentValue;
            ExplorationService service =
                GameSessionProvider.Current.Exploration;
            service.RefreshArrivals();
            ExplorationMissionSaveData mission =
                service.GetActiveMissionForEnvironment(environment);

            if (mission == null ||
                (mission.status != ExplorationMissionStatus.ReadyForBattle &&
                 mission.status != ExplorationMissionStatus.InBattle))
            {
                alert?.Show("A equipe ainda não chegou a este destino.");
                RefreshDestinationButtons();
                return;
            }

            if (!SceneTransitionController.TryEnterExplorationCombat(
                    mission.id,
                    out string error
                ))
            {
                alert?.Show(error);
                return;
            }

            CloseModal();
        }

        /// <summary>
        /// Compatibility callback for manually configured buttons. The index now
        /// refers to the filtered recruited roster, not the complete catalog.
        /// </summary>
        public void ToggleAdventurer(int rosterIndex)
        {
            if (rosterIndex < 0 ||
                rosterIndex >= recruitedAdventurers.Count)
            {
                return;
            }

            ToggleAdventurer(recruitedAdventurers[rosterIndex]);
        }

        public void SendParty()
        {
            if (panelMode == PanelMode.Report)
            {
                BeginGuardSelection();
                return;
            }

            if (selectedParty.Count < ExplorationService.MinimumPartySize)
            {
                alert?.Show(
                    "Selecione ao menos " +
                    ExplorationService.MinimumPartySize +
                    " aventureiro."
                );
                UpdateConfirmButton();
                return;
            }

            if (panelMode == PanelMode.Guard)
            {
                if (GameSessionProvider.Current.Exploration.TryAssignGuardParty(
                        selectedEnvironment,
                        selectedParty,
                        out string guardError))
                {
                    alert?.Show("Equipe de guarda posicionada.");
                    OpenAreaReport((int)selectedEnvironment);
                    return;
                }

                alert?.Show(guardError);
                return;
            }

            if (GameSessionProvider.Current.Exploration.TrySendParty(
                    selectedEnvironment,
                    selectedParty,
                    out string error
                ))
            {
                alert?.Show(
                    "Expedição enviada para " +
                    ExplorationProgression.GetDisplayName(
                        selectedEnvironment
                    ) + ". Chegada em " +
                    ExplorationService.TravelSeconds + " segundos."
                );
                selectedParty.Clear();
                CloseModal();
                return;
            }

            alert?.Show(error);
        }

        public void ClosePartySelection()
        {
            selectedParty.Clear();
            visibleReport = null;
            SetExplorerButtonsVisible(false);
            partySelectionPopupRoot?.SetActive(false);
            SetEnvironmentSelectionEnabled(true);
            UpdateConfirmButton();
        }

        public void CloseModal()
        {
            mapPopupRoot?.SetActive(false);
            partySelectionPopupRoot?.SetActive(false);
            selectedParty.Clear();
            visibleReport = null;
            panelMode = PanelMode.Mission;
            SetExplorerButtonsVisible(false);
            SetEnvironmentSelectionEnabled(true);
            UiMenuState.SetOpen(this, false);
            interactor?.ReleaseModalLock(this);
            interactor = null;
        }

        private void BuildRecruitedRoster()
        {
            ClearCards();
            recruitedAdventurers = GameSessionProvider.Current.Inventory
                .ResolveAdventurers(adventurerCatalog)
                .Where(adventurer =>
                    !GameSessionProvider.Current.Exploration.IsAdventurerBusy(
                        adventurer.InstanceId,
                        panelMode == PanelMode.Guard
                            ? selectedEnvironment
                            : null
                    ))
                .OrderBy(adventurer => adventurer.DisplayName)
                .ToList();

            emptyRosterMessage?.gameObject.SetActive(
                recruitedAdventurers.Count == 0
            );

            if (partyListContent == null || cardPrefab == null)
            {
                if (recruitedAdventurers.Count > 0)
                {
                    Debug.LogError(
                        "ExplorationMapController requires Party List Content " +
                        "and Card Prefab to render recruited adventurers.",
                        this
                    );
                }

                return;
            }

            foreach (RecruitedAdventurer adventurer in recruitedAdventurers)
            {
                AdventurerSelectionCardView card = Instantiate(
                    cardPrefab,
                    partyListContent
                );
                card.Bind(
                    adventurer.Definition,
                    adventurer.DisplayName,
                    false,
                    () => ToggleAdventurer(adventurer)
                );
                cards.Add(adventurer, card);
                spawnedCards.Add(card);
            }
        }

        private void BeginGuardSelection()
        {
            panelMode = PanelMode.Guard;
            SetExplorerButtonsVisible(false);
            selectedParty.Clear();
            BuildRecruitedRoster();
            if (selectedDestinationText != null)
            {
                selectedDestinationText.text =
                    "Guarda — " + ExplorationProgression.GetDisplayName(
                        selectedEnvironment
                    );
            }

            SetConfirmButtonLabel("Confirmar guarda");
            UpdateConfirmButton();
        }

        private void BuildReportCards(ExplorationMissionSaveData report)
        {
            ClearCards();
            if (partyListContent == null || cardPrefab == null || report == null)
            {
                return;
            }

            var catalog = (adventurerCatalog ?? Array.Empty<Char_base>())
                .Where(item => item != null)
                .GroupBy(item => item.PersistenceId)
                .ToDictionary(group => group.Key, group => group.First());
            foreach (ExplorationCombatantSaveData member in report.party ??
                     new List<ExplorationCombatantSaveData>())
            {
                if (member == null)
                {
                    continue;
                }

                string definitionId = string.IsNullOrWhiteSpace(
                    member.definitionId)
                        ? member.adventurerId
                        : member.definitionId;
                catalog.TryGetValue(definitionId, out Char_base definition);
                AdventurerSelectionCardView card = Instantiate(
                    cardPrefab,
                    partyListContent
                );
                card.BindReport(definition, member);
                spawnedCards.Add(card);
            }
        }

        private static string BuildReportSummary(
            ExplorationMissionSaveData report
        )
        {
            if (report == null)
            {
                return "Região capturada. Ainda não existe relatório salvo.";
            }

            string status = report.status switch
            {
                ExplorationMissionStatus.Returning =>
                    "A equipe está retornando com os recursos.",
                ExplorationMissionStatus.Returned =>
                    "A equipe retornou à cidade.",
                ExplorationMissionStatus.Defeated =>
                    "A equipe foi derrotada.",
                _ => "Operação em andamento."
            };
            string rewards = report.rewards == null ||
                report.rewards.Count == 0
                    ? "Recursos: nenhum."
                    : "Recursos: " + string.Join(", ", report.rewards.Select(
                        reward => reward.quantity + "x " + reward.displayName
                    ));
            return status + "\n" + rewards;
        }

        private void SetConfirmButtonLabel(string value)
        {
            if (confirmPartyButtonText != null)
            {
                confirmPartyButtonText.text = value;
            }
        }

        public void AssignExplorer()
        {
            ExplorationService service =
                GameSessionProvider.Current.Exploration;
            ExplorationAreaSaveData area = service.GetArea(selectedEnvironment);
            int current = area?.assignedWorkers ?? 0;
            if (!service.TrySetAssignedWorkers(
                    selectedEnvironment,
                    current + 1,
                    out string error))
            {
                alert?.Show(error);
                return;
            }

            alert?.Show("Explorador enviado para a região.");
            OpenAreaReport((int)selectedEnvironment);
        }

        public void RemoveExplorer()
        {
            ExplorationService service =
                GameSessionProvider.Current.Exploration;
            ExplorationAreaSaveData area = service.GetArea(selectedEnvironment);
            int current = area?.assignedWorkers ?? 0;
            if (!service.TrySetAssignedWorkers(
                    selectedEnvironment,
                    Mathf.Max(0, current - 1),
                    out string error))
            {
                alert?.Show(error);
                return;
            }

            alert?.Show("Explorador retirado da região.");
            OpenAreaReport((int)selectedEnvironment);
        }

        public void TransportExplorerLoot()
        {
            ExplorationService service =
                GameSessionProvider.Current.Exploration;
            if (!service.TryTransportWorkerLoot(selectedEnvironment, out string error))
            {
                alert?.Show(error);
                return;
            }

            alert?.Show("Recursos transportados para o inventário.");
            OpenAreaReport((int)selectedEnvironment);
        }

        private string BuildExplorerSummary()
        {
            ExplorationService service =
                GameSessionProvider.Current.Exploration;
            ExplorationAreaSaveData area = service.GetArea(selectedEnvironment);
            int assignedHere = area?.assignedWorkers ?? 0;
            string lastLoot = area?.lastWorkerLoot == null ||
                area.lastWorkerLoot.Count == 0
                    ? "Última coleta: nenhuma."
                    : "Última coleta: " + string.Join(", ",
                        area.lastWorkerLoot.Select(reward =>
                            reward.quantity + "x " + reward.displayName));
            string pendingLoot = area?.pendingWorkerLoot == null ||
                area.pendingWorkerLoot.Count == 0
                    ? "Aguardando transporte: nenhum."
                    : "Aguardando transporte: " + string.Join(", ",
                        area.pendingWorkerLoot.Where(reward =>
                                reward != null && reward.quantity > 0)
                            .Select(reward => reward.quantity + "x " +
                                reward.displayName));
            return "Exploradores nesta região: " + assignedHere +
                " | Disponíveis: " + service.AvailableExplorers +
                "/" + service.TotalExplorers +
                " | Perdidos: " + service.LostExplorers + "\n" +
                lastLoot + "\n" +
                pendingLoot;
        }

        private void EnsureExplorerButtons()
        {
            if (!createExplorerButtonsAtRuntime || confirmPartyButton == null)
            {
                return;
            }

            assignExplorerButton ??= CloneManagementButton(
                "Assign Explorer",
                "+ Explorador",
                -0.55f
            );
            removeExplorerButton ??= CloneManagementButton(
                "Remove Explorer",
                "- Explorador",
                0.55f
            );
            transportLootButton ??= CloneManagementButton(
                "Transport Explorer Loot",
                "Transportar",
                0f
            );
        }

        private Button CloneManagementButton(
            string objectName,
            string label,
            float horizontalMultiplier
        )
        {
            GameObject clone = Instantiate(
                confirmPartyButton.gameObject,
                confirmPartyButton.transform.parent,
                false
            );
            clone.name = objectName;
            Button button = clone.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            TMP_Text text = clone.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
            }

            if (clone.transform is RectTransform cloneRect &&
                confirmPartyButton.transform is RectTransform sourceRect)
            {
                float width = sourceRect.rect.width > 1f
                    ? sourceRect.rect.width
                    : Mathf.Max(180f, sourceRect.sizeDelta.x);
                cloneRect.anchoredPosition = sourceRect.anchoredPosition +
                    new Vector2(width * horizontalMultiplier,
                        sourceRect.rect.height + 12f);
            }

            return button;
        }

        private void SetExplorerButtonsVisible(bool visible)
        {
            assignExplorerButton?.gameObject.SetActive(visible);
            removeExplorerButton?.gameObject.SetActive(visible);
            transportLootButton?.gameObject.SetActive(visible);
        }

        private void UpdateExplorerButtons()
        {
            ExplorationService service =
                GameSessionProvider.Current.Exploration;
            ExplorationAreaSaveData area = service.GetArea(selectedEnvironment);
            int assignedHere = area?.assignedWorkers ?? 0;
            if (assignExplorerButton != null)
            {
                assignExplorerButton.interactable =
                    service.AvailableExplorers > 0;
            }
            if (removeExplorerButton != null)
            {
                removeExplorerButton.interactable = assignedHere > 0;
            }
            if (transportLootButton != null)
            {
                transportLootButton.interactable = area?.pendingWorkerLoot != null &&
                    area.pendingWorkerLoot.Any(reward =>
                        reward != null && reward.quantity > 0);
            }
        }

        private void ToggleAdventurer(RecruitedAdventurer adventurer)
        {
            bool selected;
            if (selectedParty.Contains(adventurer))
            {
                selectedParty.Remove(adventurer);
                selected = false;
            }
            else
            {
                if (selectedParty.Count >= maximumPartySize)
                {
                    alert?.Show(
                        "O grupo pode ter no máximo " +
                        maximumPartySize + " aventureiros."
                    );
                    return;
                }

                selectedParty.Add(adventurer);
                selected = true;
            }

            if (cards.TryGetValue(adventurer, out var card))
            {
                card.SetSelected(selected);
            }

            UpdateConfirmButton();
        }

        private void RefreshDestinationButtons()
        {
            ExplorationService service =
                GameSessionProvider.Current.Exploration;
            service.RefreshArrivals();
            ExplorationMissionSaveData activeMission =
                service.GetActiveMission();
            var completed = new HashSet<ExplorationEnvironment>(
                service.CompletedEnvironments
            );

            foreach (ExplorationDestinationButton destination in
                     destinationButtons ??
                     Array.Empty<ExplorationDestinationButton>())
            {
                if (destination == null)
                {
                    continue;
                }

                ExplorationEnvironment environment = destination.Environment;
                ExplorationMissionSaveData environmentMission =
                    service.GetActiveMissionForEnvironment(environment);
                ExplorationAreaSaveData area = service.GetArea(environment);
                destination.Initialize(
                    this,
                    service.IsEnvironmentUnlocked(environment),
                    completed.Contains(environment),
                    environmentMission?.status,
                    activeMission == null,
                    area?.enemyAttackPending ?? false,
                    service.GetLatestMissionForEnvironment(environment) != null
                );
            }
        }

        private void UpdateConfirmButton()
        {
            if (confirmPartyButton != null)
            {
                confirmPartyButton.interactable =
                    selectedParty.Count >= ExplorationService.MinimumPartySize &&
                    selectedParty.Count <= maximumPartySize;
            }

            if (panelMode == PanelMode.Mission)
            {
                SetConfirmButtonLabel("Enviar");
            }
        }

        private void SetEnvironmentSelectionEnabled(bool enabled)
        {
            environmentSelectionRoot?.SetActive(enabled);

            foreach (ExplorationDestinationButton destination in
                     destinationButtons ??
                     Array.Empty<ExplorationDestinationButton>())
            {
                destination?.SetInteractionEnabled(enabled);
            }
        }

        private void ClearCards()
        {
            foreach (AdventurerSelectionCardView card in spawnedCards)
            {
                if (card != null)
                {
                    card.gameObject.SetActive(false);
                    Destroy(card.gameObject);
                }
            }

            cards.Clear();
            spawnedCards.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maximumPartySize = Mathf.Max(
                ExplorationService.MinimumPartySize,
                maximumPartySize
            );
            maximumPartySize = Mathf.Min(
                ExplorationService.MaximumPartySize,
                maximumPartySize
            );

            if (!autoPopulateCatalogInEditor)
            {
                return;
            }

            adventurerCatalog = AssetDatabase.FindAssets("t:Char_base")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Char_base>)
                .Where(definition => definition != null)
                .OrderBy(definition => definition.name)
                .ToArray();
        }
#endif
    }
}
