using TCCGame.Core.Input;
using UnityEngine;

public class TaxCollectorNpc : MonoBehaviour, IInteractable
{
    [SerializeField]
    private TaxAccrualService taxService;

    [SerializeField]
    private AlertPopup alert;

    public bool CanInteract => enabled && taxService != null;

    public void Interact(GameObject player)
    {
        TaxCollectionResult result = taxService.CollectTaxes();
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
