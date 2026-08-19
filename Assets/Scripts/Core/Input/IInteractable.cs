using UnityEngine;

public interface IInteractable
{
    bool CanInteract { get; }

    void Interact(GameObject player);
}

public interface IInteractionExitAware
{
    void OnExitInteractionRange(GameObject player);
}
