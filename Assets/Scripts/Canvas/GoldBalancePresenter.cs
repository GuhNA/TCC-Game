using TCCGame.Core;
using TCCGame.Gameplay.Economy;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public sealed class GoldBalancePresenter : MonoBehaviour
{
    [SerializeField]
    private Text goldText;

    [SerializeField] private TMP_Text pixelGoldText;

    private EconomyService economy;

    public Text GoldText => goldText;
    public TMP_Text PixelGoldText => pixelGoldText;

    private void OnEnable()
    {
        economy = GameSessionProvider.Current.Economy;
        economy.MoneyChanged += Refresh;
        Refresh(economy.Money);
    }

    private void OnDisable()
    {
        if (economy != null)
        {
            economy.MoneyChanged -= Refresh;
        }
    }

    private void Refresh(int money)
    {
        if (goldText != null)
        {
            goldText.text = $"Ouro: {money}";
        }
        if (pixelGoldText != null)
            pixelGoldText.text = money.ToString("N0", CultureInfo.InvariantCulture).Replace(',', ' ');
    }
}
