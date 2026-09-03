using System;
using System.Collections.Generic;
using System;
using System.Linq;
using TCCGame.Core;
using TCCGame.Core.Input;
using UnityEngine;

[RequireComponent(typeof(moviment))]
public sealed class PlayerInteractor : MonoBehaviour
{
    private readonly Dictionary<IInteractable, int> candidates = new();
    private readonly HashSet<object> modalLockOwners = new();

    private moviment movement;

    public bool HasModalOpen => modalLockOwners.Count > 0;
    public IInteractable CurrentInteractable { get; private set; }

    public event Action<IInteractable> CurrentInteractableChanged;

    private void Awake()
    {
        movement = GetComponent<moviment>();
    }

    private void Update()
    {
        RefreshCurrentInteractable();

        if (HasModalOpen)
        {
            if (GameInput.WasInteractPressedThisFrame())
            {
                modalLockOwners
                    .OfType<IModalInteractionOwner>()
                    .FirstOrDefault()
                    ?.CloseModal();
            }

            return;
        }

        if (!GameInput.WasInteractPressedThisFrame())
        {
            return;
        }

        CurrentInteractable?.Interact(gameObject);
    }

    private void OnDisable()
    {
        modalLockOwners.Clear();
        SetCurrentInteractable(null);
        movement?.SetMovementBlocked(false);
    }

    public void AcquireModalLock(object owner)
    {
        if (owner == null || !modalLockOwners.Add(owner))
        {
            return;
        }

        movement.SetMovementBlocked(true);
    }

    public void ReleaseModalLock(object owner)
    {
        if (owner == null || !modalLockOwners.Remove(owner))
        {
            return;
        }

        movement.SetMovementBlocked(HasModalOpen);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        AddCandidates(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        RemoveCandidates(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        AddCandidates(collision.collider);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        RemoveCandidates(collision.collider);
    }

    private void AddCandidates(Collider2D collider)
    {
        foreach (IInteractable interactable in FindInteractables(collider))
        {
            candidates.TryGetValue(interactable, out int count);
            candidates[interactable] = count + 1;
        }

        RefreshCurrentInteractable();
    }

    private void RemoveCandidates(Collider2D collider)
    {
        foreach (IInteractable interactable in FindInteractables(collider))
        {
            if (!candidates.TryGetValue(interactable, out int count))
            {
                continue;
            }

            if (count > 1)
            {
                candidates[interactable] = count - 1;
                continue;
            }

            candidates.Remove(interactable);

            if (interactable is IInteractionExitAware exitAware)
            {
                exitAware.OnExitInteractionRange(gameObject);
            }
        }

        RefreshCurrentInteractable();
    }

    private void RefreshCurrentInteractable()
    {
        SetCurrentInteractable(GetCurrentInteractable());
    }

    private void SetCurrentInteractable(IInteractable interactable)
    {
        if (ReferenceEquals(CurrentInteractable, interactable))
        {
            return;
        }

        CurrentInteractable = interactable;
        CurrentInteractableChanged?.Invoke(CurrentInteractable);
    }

    private IInteractable GetCurrentInteractable()
    {
        return candidates.Keys
            .Where(candidate => candidate.CanInteract)
            .OrderBy(candidate => DistanceTo(candidate))
            .FirstOrDefault();
    }

    private float DistanceTo(IInteractable interactable)
    {
        Component component = interactable as Component;
        return component == null
            ? float.MaxValue
            : Vector2.Distance(transform.position, component.transform.position);
    }

    private static IEnumerable<IInteractable> FindInteractables(
        Collider2D collider
    )
    {
        return collider
            .GetComponentsInParent<MonoBehaviour>(true)
            .OfType<IInteractable>();
    }
}
