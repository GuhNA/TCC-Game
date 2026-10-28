using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.UI
{
    public sealed class CityTaxHudView : MonoBehaviour
    {
        [SerializeField] private GameObject widget;
        [SerializeField] private GameObject toast;
        [SerializeField] private TMP_Text amount;
        [SerializeField] private TMP_Text rate;
        [SerializeField] private TMP_Text message;
        [SerializeField] private Image statusIcon;
        [SerializeField] private MedievalHudTheme theme;

        public void Present(int level, int available, int production, bool collected = false)
        {
            amount.text = collected && available == 0 ? "Impostos coletados" : $"{Money(available)} ouro disponíveis";
            rate.text = level <= 0 ? "Construa o Mercado Local" : $"Mercado nível {level} • {Money(production)} ouro/min";
        }

        public void PresentResult(TaxCollectionResult result)
        {
            bool success = result.Status == TaxCollectionStatus.Collected;
            message.text = success ? $"+{Money(result.Amount)} ouro coletado\nSaldo atualizado: {Money(result.TotalMoney)} ouro"
                : result.Status == TaxCollectionStatus.MarketNotBuilt ? "Construa o Mercado Local\npara gerar impostos."
                : "Ainda não há impostos disponíveis.\nRetorne após acumular mais ouro.";
            message.color = success ? theme.Success : theme.Subtitle;
            statusIcon.sprite = success ? theme.CheckIcon : theme.HourglassIcon;
        }

        public void SetVisibility(bool showWidget, bool showToast)
        {
            widget.SetActive(showWidget);
            toast.SetActive(showToast);
        }

        public static string Money(int value) => value.ToString("N0", CultureInfo.InvariantCulture).Replace(',', ' ');
    }
}
