using TCCGame.Core;
using TCCGame.Gameplay.Economy;
using UnityEngine;
using UnityEngine.UI;

public sealed class GoldBalancePresenter : MonoBehaviour
{
    [SerializeField]
    private Text goldText;

    private EconomyService economy;

    public Text GoldText => goldText;

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
    }
}
