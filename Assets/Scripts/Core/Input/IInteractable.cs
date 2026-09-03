using UnityEngine;

public interface IInteractable
{
    bool CanInteract { get; }

    void Interact(GameObject player);
}

public interface IInteractionPromptProvider
{
    string InteractionPrompt { get; }
}

public interface IInteractionExitAware
{
    void OnExitInteractionRange(GameObject player);
}

public interface IModalInteractionOwner
{
    void CloseModal();
}
