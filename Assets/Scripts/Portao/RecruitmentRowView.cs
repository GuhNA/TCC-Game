using System;
using System.Globalization;
using TMPro;
using TCCGame.Core;
using UnityEngine;
using UnityEngine.UI;

public sealed class RecruitmentRowView : MonoBehaviour
{
    [SerializeField]
    private UnitType unitType;

    [SerializeField]
    private TMP_Text stockText;

    [SerializeField]
    private TMP_Text ownedText;

    [SerializeField]
    private TMP_Text priceText;

    [SerializeField]
    private TMP_Text quantityText;

    [SerializeField]
    private TMP_Text totalCostText;

    [SerializeField]
    private TMP_Text statusText;

    [SerializeField]
    private TMP_Text actionButtonText;

    [SerializeField]
    private Button decreaseButton;

    [SerializeField]
    private Button increaseButton;

    [SerializeField]
    private Button purchaseButton;

    [SerializeField]
    private TMP_InputField quantityInput;

    private int available;
    private int unitPrice;
    private int currentMoney;
    private int selectedQuantity = 1;

    public UnitType UnitType => unitType;
    public TMP_Text StockText => stockText;
    public int SelectedQuantity => selectedQuantity;

    public event Action<UnitType, int> PurchaseRequested;
    public event Action InputInvalid;

    public void Refresh(int available)
    {
        this.available = Mathf.Max(0, available);
        if (stockText != null)
        {
            stockText.text = $"{GetDisplayName()}: {available}";
        }
    }

    public void Refresh(int available, int owned, int unitPrice, int money)
    {
        this.available = Mathf.Max(0, available);
        this.unitPrice = Mathf.Max(0, unitPrice);
        currentMoney = Mathf.Max(0, money);
        selectedQuantity = Mathf.Clamp(
            selectedQuantity,
            this.available > 0 ? 1 : 0,
            this.available
        );

        if (stockText != null)
        {
            stockText.text = $"Estoque: {this.available}";
        }

        if (ownedText != null)
        {
            ownedText.text = $"Na reserva: {Mathf.Max(0, owned)}";
        }

        if (priceText != null)
        {
            priceText.text = $"Unidade: {this.unitPrice} ouro";
        }

        RefreshSelection();
    }

    public void DecreaseQuantity()
    {
        SetQuantity(selectedQuantity - 1);
    }

    public void IncreaseQuantity()
    {
        SetQuantity(selectedQuantity + 1);
    }

    public void SelectOne()
    {
        SetQuantity(1);
    }

    public void SelectTen()
    {
        SetQuantity(10);
    }

    public void SelectFive()
    {
        SetQuantity(5);
    }

    public void SelectMaximum()
    {
        SetQuantity(available);
    }

    public void ConfirmPurchase()
    {
        if (selectedQuantity <= 0 || selectedQuantity > available)
        {
            InputInvalid?.Invoke();
            return;
        }

        Purchase(selectedQuantity);
    }

    public void ResetSelection()
    {
        selectedQuantity = available > 0 ? 1 : 0;
        RefreshSelection();
    }

    public void PurchaseOne() => Purchase(1);
    public void PurchaseTen() => Purchase(10);
    public void PurchaseHundred() => Purchase(100);

    public void SubmitInput()
    {
        string raw = quantityInput?.text?.Trim();

        if (!int.TryParse(
                raw,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int quantity
            ))
        {
            InputInvalid?.Invoke();
            ClearInput();
            return;
        }

        Purchase(quantity);
        ClearInput();
    }

    private void Purchase(int quantity)
    {
        PurchaseRequested?.Invoke(unitType, quantity);
    }

    private void SetQuantity(int quantity)
    {
        selectedQuantity = available <= 0
            ? 0
            : Mathf.Clamp(quantity, 1, available);
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        int totalCost;
        try
        {
            totalCost = checked(unitPrice * selectedQuantity);
        }
        catch (OverflowException)
        {
            totalCost = int.MaxValue;
        }

        if (quantityText != null)
        {
            quantityText.text = selectedQuantity.ToString(
                CultureInfo.InvariantCulture
            );
        }

        if (totalCostText != null)
        {
            totalCostText.text = $"Total: {totalCost} ouro";
        }

        if (actionButtonText != null)
        {
            actionButtonText.text = selectedQuantity > 0
                ? $"CONTRATAR {selectedQuantity}"
                : "INDISPONÍVEL";
        }

        bool valid = selectedQuantity > 0 &&
            selectedQuantity <= available &&
            totalCost <= currentMoney;

        if (purchaseButton != null)
        {
            purchaseButton.interactable = valid;
        }

        if (decreaseButton != null)
        {
            decreaseButton.interactable = selectedQuantity > 1;
        }

        if (increaseButton != null)
        {
            increaseButton.interactable = selectedQuantity < available;
        }

        if (statusText != null)
        {
            statusText.text = available <= 0
                ? "Estoque esgotado"
                : totalCost > currentMoney
                    ? "Ouro insuficiente"
                    : "Pronto para contratar";
            statusText.color = valid
                ? new Color(0.18f, 0.46f, 0.16f, 1f)
                : new Color(0.67f, 0.20f, 0.13f, 1f);
        }
    }

    private void ClearInput()
    {
        if (quantityInput != null)
        {
            quantityInput.text = string.Empty;
        }
    }

    private string GetDisplayName()
    {
        return unitType switch
        {
            UnitType.Archer => "Arqueiros",
            UnitType.Warrior => "Guerreiros",
            UnitType.Mage => "Magos",
            _ => "Tropas"
        };
    }
}
