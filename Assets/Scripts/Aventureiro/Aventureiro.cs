using UnityEngine;

public class Aventureiro : MonoBehaviour, IInteractable, IInteractionExitAware
{
    public Char_base charBase;

    public AdventurerRuntimeState RuntimeState { get; private set; }

    public AdventurerStats Stats => RuntimeState?.Stats ?? default;

    public bool IsBeingInspected { get; private set; }

    public bool CanInteract => enabled && charBase != null;

    private void Start()
    {
        Configure(charBase);
    }

    /// <summary>
    /// Reconfigura esta instância para representar outro aventureiro.
    /// O spawner da taverna usa este método ao sortear uma nova oferta; assim
    /// os atributos usados pela janela de recrutamento e o sprite visível são
    /// atualizados juntos, sem manter dados do aventureiro anterior.
    /// </summary>
    public void Configure(Char_base definition)
    {
        charBase = definition;
        if (definition == null)
        {
            RuntimeState = null;
            enabled = false;
            return;
        }

        enabled = true;
        RuntimeState = new AdventurerRuntimeState(
            AdventurerStatsCalculator.Calculate(definition)
        );

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && definition.sprite != null)
        {
            spriteRenderer.sprite = definition.sprite;
        }
    }

    public void Interact(GameObject player)
    {
        player.GetComponent<AdventurerRecruitmentController>()?.Open(this);
    }

    public void OnExitInteractionRange(GameObject player)
    {
        player.GetComponent<AdventurerRecruitmentController>()?.Close(this);
    }

    public void SetBeingInspected(bool value)
    {
        IsBeingInspected = value;
    }

}
