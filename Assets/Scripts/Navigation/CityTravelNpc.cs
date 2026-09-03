using TCCGame.Core;
using TCCGame.Core.Input;
using TCCGame.Gameplay.Campaign;
using UnityEngine;

namespace TCCGame.Gameplay.Navigation
{
    public sealed class CityTravelNpc : MonoBehaviour,
        IInteractable,
        IInteractionPromptProvider
    {
        [SerializeField]
        private TravelDestinationKind destinationKind;

        [SerializeField]
        private GateId gateId = GateId.North;

        [SerializeField]
        private string npcDisplayName = "Sentinela";

        public bool CanInteract => enabled &&
            !SceneTransitionController.IsTransitioning &&
            !TravelConversationPanel.IsVisible;

        public string InteractionPrompt => $"Conversar — {npcDisplayName}";

        public TravelDestination Destination
        {
            get
            {
                GateId destinationGate = destinationKind ==
                    TravelDestinationKind.Gate
                    ? gateId
                    : GameSessionProvider.Current.Navigation
                        .Current.ActiveGateId;
                return new TravelDestination(destinationKind, destinationGate);
            }
        }

        public string NpcDisplayName => npcDisplayName;

        public void Configure(TravelDestinationKind destination, GateId destinationGate, string displayName)
        {
            destinationKind = destination;
            gateId = destinationGate;
            npcDisplayName = displayName;
        }

        public void Interact(GameObject player)
        {
            if (!CanInteract)
            {
                return;
            }

            TravelConversationPanel.Show(this, player);
        }
    }
}
