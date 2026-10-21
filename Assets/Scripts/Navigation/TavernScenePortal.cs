using TCCGame.Core;
using TCCGame.Core.Input;
using UnityEngine;

namespace TCCGame.Gameplay.Navigation
{
    /// <summary>
    /// Interação exclusiva da Taverna. No Centro, a zona da porta entra na
    /// cena Taverna; dentro dela, a zona exit retorna ao Centro. O prédio ainda
    /// mantém sua interação separada de melhoria fora da região da porta.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class TavernScenePortal : MonoBehaviour,
        IInteractable,
        IInteractionPromptProvider
    {
        [SerializeField] private bool exitsTavern;
        [SerializeField] private bool requireBuiltTavern = true;

        public bool CanInteract => enabled &&
            !SceneTransitionController.IsTransitioning &&
            (exitsTavern || !requireBuiltTavern ||
             GameSessionProvider.Current.Buildings.GetLevel(
                 BuildingType.Tavern) > 0);

        public string InteractionPrompt => exitsTavern
            ? "Sair para o Centro"
            : "Entrar na Taverna";

        public void Interact(GameObject player)
        {
            if (!CanInteract)
            {
                return;
            }

            bool started = exitsTavern
                ? SceneTransitionController.TryExitTavern()
                : SceneTransitionController.TryEnterTavern();
            if (!started)
            {
                Debug.LogWarning(
                    "Não foi possível iniciar a transição da Taverna.",
                    this
                );
            }
        }
    }
}
