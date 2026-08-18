using System;
using System.Globalization;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Input;
using TCCGame.Gameplay.Recruitment;
using UnityEngine;
using UnityEngine.UI;

public class NPCVenda : MonoBehaviour
{
    public PopupAlerta alerta;
    public GameObject popup;
    public TMP_Text arqueiro_texto;
    public TMP_Text guerreiro_texto;
    public TMP_Text mago_texto;
    public TMP_Text tempo_texto;

    private bool aberto;
    private bool dentro;
    private UnitType? escolha;

    private RecruitmentService Recruitment =>
        GameSessionProvider.Current.Recruitment;

    private void Start()
    {
        aberto = false;
        dentro = false;

        if (popup != null)
        {
            popup.SetActive(false);
        }
    }

    private void Update()
    {
        if (aberto)
        {
            AtualizaTempo();
        }

        if (!dentro ||
            !GameInput.WasInteractPressedThisFrame())
        {
            return;
        }

        if (!aberto)
        {
            abrir_popup();
            return;
        }

        popup.SetActive(false);
        aberto = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<moviment>() != null)
        {
            dentro = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<moviment>() != null)
        {
            dentro = false;
        }
    }

    private void abrir_popup()
    {
        Recruitment.RefreshStockIfDue();
        atualizaTexto();
        AtualizaTempo();
        aberto = true;
        popup.SetActive(true);
    }

    public void vendaCalculo(int intValue)
    {
        if (!escolha.HasValue)
        {
            ExibirAlerta("Selecione um tipo de tropa.");
            return;
        }

        ProcessarCompra(escolha.Value, intValue);
    }

    public void compraEscolha(int value)
    {
        escolha = value switch
        {
            1 => UnitType.Archer,
            2 => UnitType.Warrior,
            3 => UnitType.Mage,
            _ => null
        };
    }

    public void porInput(InputField input)
    {
        if (input == null)
        {
            return;
        }

        if (!TryGetUnitType(input.name, out UnitType unitType) ||
            !int.TryParse(
                input.text?.Trim(),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int quantity
            ))
        {
            ExibirAlerta("Quantidade inválida");
            input.text = string.Empty;
            return;
        }

        ProcessarCompra(unitType, quantity);
        input.text = string.Empty;
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

    private static bool TryGetUnitType(
        string inputName,
        out UnitType unitType
    )
    {
        switch (inputName)
        {
            case "arqueiroInput":
                unitType = UnitType.Archer;
                return true;

            case "guerreiroInput":
                unitType = UnitType.Warrior;
                return true;

            case "magoInput":
                unitType = UnitType.Mage;
                return true;

            default:
                unitType = default;
                return false;
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
