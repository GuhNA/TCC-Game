using System;
using System.Globalization;
using TMPro;
using TCCGame.Core;
using UnityEngine;

public sealed class RecruitmentRowView : MonoBehaviour
{
    [SerializeField]
    private UnitType unitType;

    [SerializeField]
    private TMP_Text stockText;

    [SerializeField]
    private TMP_InputField quantityInput;

    public UnitType UnitType => unitType;

    public event Action<UnitType, int> PurchaseRequested;
    public event Action InputInvalid;

    public void Refresh(int available)
    {
        if (stockText != null)
        {
            stockText.text = $"{GetDisplayName()}: {available}";
        }
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
