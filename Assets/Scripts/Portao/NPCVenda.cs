using System;
using System.Globalization;
using System.Linq;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Input;
using TCCGame.Gameplay.Recruitment;
using UnityEngine;

public class NPCVenda : MonoBehaviour, IInteractable
{
    public PopupAlerta alerta;
    public GameObject popup;
    public TMP_Text arqueiro_texto;
    public TMP_Text guerreiro_texto;
    public TMP_Text mago_texto;
    public TMP_Text tempo_texto;

    private RecruitmentRowView[] rows;

    private bool aberto;

    public bool CanInteract => popup != null;

    private RecruitmentService Recruitment =>
        GameSessionProvider.Current.Recruitment;

    private void Start()
    {
        aberto = false;
        rows = popup == null
            ? System.Array.Empty<RecruitmentRowView>()
            : popup.GetComponentsInChildren<RecruitmentRowView>(true);

        foreach (RecruitmentRowView row in rows)
        {
            row.PurchaseRequested += HandlePurchaseRequested;
            row.InputInvalid += HandleInvalidInput;
        }

        if (popup != null)
        {
            popup.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (rows == null)
        {
            return;
        }

        foreach (RecruitmentRowView row in rows)
        {
            row.PurchaseRequested -= HandlePurchaseRequested;
            row.InputInvalid -= HandleInvalidInput;
        }
    }

    private void Update()
    {
        if (aberto)
        {
            AtualizaTempo();
        }

    }

    public void Interact(GameObject player)
    {
        if (!aberto)
        {
            abrir_popup();
            return;
        }

        popup.SetActive(false);
        aberto = false;
    }

    private void abrir_popup()
    {
        Recruitment.RefreshStockIfDue();
        atualizaTexto();
        AtualizaTempo();
        aberto = true;
        popup.SetActive(true);
    }

    private void HandlePurchaseRequested(UnitType unitType, int quantity)
    {
        Purchase(unitType, quantity);
    }

    private void HandleInvalidInput()
    {
        ExibirAlerta("Quantidade inválida");
    }

    public void Purchase(UnitType unitType, int quantity)
    {
        ProcessarCompra(unitType, quantity);
    }

    private void ProcessarCompra(UnitType unitType, int quantity)
    {
        RecruitmentPurchaseResult result =
            Recruitment.TryPurchase(unitType, quantity);

        switch (result.Status)
        {
            case RecruitmentPurchaseStatus.Purchased:
                ExibirAlerta(
                    $"Compra realizada, {GetPluralName(unitType)} " +
                    $"disponíveis: {result.Available}"
                );
                break;

            case RecruitmentPurchaseStatus.OutOfStock:
                ExibirAlerta(
                    $"Quantidade indisponível de {GetSingularName(unitType)}"
                );
                break;

            case RecruitmentPurchaseStatus.InsufficientMoney:
                ExibirAlerta("Dinheiro insuficiente");
                break;

            default:
                ExibirAlerta("Quantidade inválida");
                break;
        }

        atualizaTexto();
    }

    private void atualizaTexto()
    {
        arqueiro_texto.text =
            $"Arqueiros: {Recruitment.GetAvailable(UnitType.Archer)}";
        guerreiro_texto.text =
            $"Guerreiros: {Recruitment.GetAvailable(UnitType.Warrior)}";
        mago_texto.text =
            $"Magos: {Recruitment.GetAvailable(UnitType.Mage)}";

        foreach (RecruitmentRowView row in rows ?? Enumerable.Empty<RecruitmentRowView>())
        {
            row.Refresh(Recruitment.GetAvailable(row.UnitType));
        }
    }

    private void AtualizaTempo()
    {
        if (tempo_texto == null)
        {
            return;
        }

        TimeSpan remaining = Recruitment.GetTimeUntilRestock();
        int totalHours = Mathf.FloorToInt((float)remaining.TotalHours);

        tempo_texto.text = string.Format(
            CultureInfo.InvariantCulture,
            "Tempo: {0:00}:{1:00}:{2:00}",
            totalHours,
            remaining.Minutes,
            remaining.Seconds
        );
    }

    private void ExibirAlerta(string message)
    {
        if (alerta != null)
        {
            StartCoroutine(alerta.attTexto(message));
        }
    }

    private static string GetSingularName(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Archer => "arqueiro",
            UnitType.Warrior => "guerreiro",
            UnitType.Mage => "mago",
            _ => "tropa"
        };
    }

    private static string GetPluralName(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Archer => "arqueiros",
            UnitType.Warrior => "guerreiros",
            UnitType.Mage => "magos",
            _ => "tropas"
        };
    }
}
