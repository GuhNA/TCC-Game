using UnityEngine;
using UnityEngine.InputSystem;

public class TaxCollectorNpc : MonoBehaviour
{
    [SerializeField]
    private TaxAccrualService taxService;

    [SerializeField]
    private PopupAlerta alert;

    private bool playerNearby;

    private void Update()
    {
        if (!playerNearby ||
            Keyboard.current == null ||
            !Keyboard.current.eKey.wasPressedThisFrame)
        {
            return;
        }

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

        StartCoroutine(alert.attTexto(message));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<moviment>() != null)
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<moviment>() != null)
        {
            playerNearby = false;
        }
    }
}
