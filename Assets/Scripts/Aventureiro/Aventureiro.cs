using UnityEngine;

public class Aventureiro : MonoBehaviour, IInteractable, IInteractionExitAware
{
    public Char_base charBase;

    public AdventurerRuntimeState RuntimeState { get; private set; }

    public AdventurerStats Stats => RuntimeState?.Stats ?? default;

    public bool CanInteract => enabled && charBase != null;

    private void Start()
    {
        if (charBase == null)
        {
            enabled = false;
            return;
        }

        RuntimeState = new AdventurerRuntimeState(
            AdventurerStatsCalculator.Calculate(charBase)
        );
    }

    public void Interact(GameObject player)
    {
        player.GetComponent<AdventurerRecruitmentController>()?.Open(this);
    }

    public void OnExitInteractionRange(GameObject player)
    {
        player.GetComponent<AdventurerRecruitmentController>()?.Close(this);
    }

}
