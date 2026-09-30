using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Input;
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
        [SerializeField] private Button backButton;

        [Header("Adventurer definitions")]
        [Tooltip("Complete catalog. Only recruited entries are shown at runtime.")]
        [FormerlySerializedAs("availableAdventurers")]
        [SerializeField] private Char_base[] adventurerCatalog;
        [SerializeField] private bool autoPopulateCatalogInEditor = true;

        [Header("Rules and feedback")]
        [SerializeField, Min(1)] private int maximumPartySize = 6;
        [SerializeField] private AlertPopup alert;

        private readonly HashSet<Char_base> selectedParty = new();
        private readonly Dictionary<Char_base, AdventurerSelectionCardView>
            cards = new();
        private List<Char_base> recruitedAdventurers = new();
        private PlayerInteractor interactor;
        private ExplorationEnvironment selectedEnvironment;

        public bool CanInteract => enabled && mapPopupRoot != null;
        public IReadOnlyList<Char_base> RecruitedAdventurers =>
            recruitedAdventurers.AsReadOnly();

        private void Awake()
        {
            partySelectionPopupRoot?.SetActive(false);
        }

        private void OnEnable()
        {
            closeMapButton?.onClick.AddListener(CloseModal);
            confirmPartyButton?.onClick.AddListener(SendParty);
            backButton?.onClick.AddListener(ClosePartySelection);
        }

        private void OnDisable()
        {
            closeMapButton?.onClick.RemoveListener(CloseModal);
            confirmPartyButton?.onClick.RemoveListener(SendParty);
            backButton?.onClick.RemoveListener(ClosePartySelection);
            CloseModal();
        }

        private void Update()
        {
            GameSessionProvider.Current.Exploration.RefreshArrivals();
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

            selectedEnvironment = environment;
            selectedParty.Clear();
            BuildRecruitedRoster();

            if (selectedDestinationText != null)
            {
                selectedDestinationText.text =
                    ExplorationProgression.GetDisplayName(environment);
            }

            partySelectionPopupRoot?.SetActive(true);
            SetEnvironmentSelectionEnabled(false);
            UpdateConfirmButton();
        }

        // Compatibility with Inspector events from the original controller.
        public void SelectEnvironment(int environmentValue)
        {
            OpenPartySelection(environmentValue);
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
            partySelectionPopupRoot?.SetActive(false);
            SetEnvironmentSelectionEnabled(true);
            UpdateConfirmButton();
        }

        public void CloseModal()
        {
            mapPopupRoot?.SetActive(false);
            partySelectionPopupRoot?.SetActive(false);
            selectedParty.Clear();
            SetEnvironmentSelectionEnabled(true);
            UiMenuState.SetOpen(this, false);
            interactor?.ReleaseModalLock(this);
            interactor = null;
        }

        private void BuildRecruitedRoster()
        {
            ClearCards();
            var recruitedIds = new HashSet<string>(
                GameSessionProvider.Current.Inventory
                    .RecruitedAdventurerIds
            );

            recruitedAdventurers = (adventurerCatalog ??
                    Array.Empty<Char_base>())
                .Where(definition => definition != null)
                .GroupBy(definition => definition.PersistenceId)
                .Select(group => group.First())
                .Where(definition =>
                    recruitedIds.Contains(definition.PersistenceId))
                .OrderBy(definition => definition.name)
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

            foreach (Char_base adventurer in recruitedAdventurers)
            {
                AdventurerSelectionCardView card = Instantiate(
                    cardPrefab,
                    partyListContent
                );
                card.Bind(adventurer, false, ToggleAdventurer);
                cards.Add(adventurer, card);
            }
        }

        private void ToggleAdventurer(Char_base adventurer)
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
                destination.Initialize(
                    this,
                    service.IsEnvironmentUnlocked(environment),
                    completed.Contains(environment)
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
            foreach (AdventurerSelectionCardView card in cards.Values)
            {
                if (card != null)
                {
                    card.gameObject.SetActive(false);
                    Destroy(card.gameObject);
                }
            }

            cards.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maximumPartySize = Mathf.Max(
                ExplorationService.MinimumPartySize,
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
