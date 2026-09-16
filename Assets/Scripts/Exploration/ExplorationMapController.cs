using System.Collections.Generic;
using TCCGame.Core;
using UnityEngine;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Attach this component to the world object the player uses to open the map.
    /// Button callbacks use SelectEnvironment, ToggleAdventurer and SendParty.
    /// Canvas layout remains fully owned by the scene through the Inspector fields.
    /// </summary>
    public sealed class ExplorationMapController : MonoBehaviour, IInteractable, IModalInteractionOwner
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private AlertPopup alert;
        [SerializeField] private Char_base[] availableAdventurers;

        private readonly HashSet<Char_base> selectedParty = new();
        private PlayerInteractor interactor;
        private ExplorationEnvironment selectedEnvironment;

        public bool CanInteract => enabled && popupRoot != null;

        private void Update()
        {
            GameSessionProvider.Current.Exploration.RefreshArrivals();
        }

        public void Interact(GameObject player)
        {
            interactor = player == null ? null : player.GetComponent<PlayerInteractor>();
            popupRoot.SetActive(true);
            interactor?.AcquireModalLock(this);
        }

        public void SelectEnvironment(int environment)
        {
            if (!System.Enum.IsDefined(typeof(ExplorationEnvironment), environment)) return;
            selectedEnvironment = (ExplorationEnvironment)environment;
        }

        public void ToggleAdventurer(int rosterIndex)
        {
            if (availableAdventurers == null || rosterIndex < 0 ||
                rosterIndex >= availableAdventurers.Length) return;
            Char_base adventurer = availableAdventurers[rosterIndex];
            if (adventurer == null) return;
            if (!selectedParty.Add(adventurer)) selectedParty.Remove(adventurer);
        }

        public void SendParty()
        {
            if (GameSessionProvider.Current.Exploration.TrySendParty(selectedEnvironment, selectedParty, out string error))
            {
                alert?.Show("Expedição enviada. Chegada em 5 segundos.");
                selectedParty.Clear();
                CloseModal();
                return;
            }
            alert?.Show(error);
        }

        public void CloseModal()
        {
            popupRoot?.SetActive(false);
            interactor?.ReleaseModalLock(this);
            interactor = null;
        }
    }
}
