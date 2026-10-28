using TCCGame.Core.Input;
using TCCGame.Core;
using TCCGame.UI;
using UnityEngine;

public class TaxCollectorNpc : MonoBehaviour, IInteractable, IInteractionPromptProvider
{
    [SerializeField]
    private TaxAccrualService taxService;

    [SerializeField]
    private AlertPopup alert;

    [SerializeField] private CityTaxHud hud;
    private int lastCollectionFrame = -1;

    public bool CanInteract => enabled && taxService != null;
    public string InteractionPrompt
    {
        get
        {
            int amount = GameSessionProvider.Current.Taxes.GetCollectibleAmount();
            return amount > 0 ? $"Coletar impostos — {CityTaxHudView.Money(amount)} ouro" : "Consultar impostos";
        }
    }

    public void Interact(GameObject player)
    {
        if (!CanInteract || UiMenuState.HasOpenMenu || lastCollectionFrame == Time.frameCount) return;
        lastCollectionFrame = Time.frameCount;
        TaxCollectionResult result = taxService.CollectTaxes();
        if (hud != null)
        {
            hud.ShowResult(result);
            return;
        }
        string message;

        switch (result.Status)
        {
            case TaxCollectionStatus.Collected:
                message =
                    $"Você coletou {result.Amount} moedas de impostos. " +
                    $"Total: {result.TotalMoney}.";
                break;

            case TaxCollectionStatus.MarketNotBuilt:
                message = "Construa o Mercado Local para gerar impostos.";
                break;

            default:
                message = "Ainda não há impostos disponíveis.";
                break;
        }

        alert?.Show(message);
    }
}
