using TMPro;
using UnityEngine;

namespace TCCGame.UI
{
    public sealed class TravelHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text destination;
        [SerializeField] private TMP_Text healthLabel;
        [SerializeField] private TMP_Text healthValue;
        [SerializeField] private TMP_Text waveLabel;
        [SerializeField] private TMP_Text waveValue;

        public void Present(string destinationName, bool isGate, int health, int maximum,
            string status, string countdown, string threat)
        {
            destination.text = destinationName;
            healthLabel.text = isGate ? "Integridade do portão" : "Rota de retorno";
            healthValue.text = isGate ? $"{health} / {maximum}" : "Cidade";
            waveLabel.text = isGate ? status : "Destino da viagem";
            waveValue.text = isGate
                ? (string.IsNullOrEmpty(countdown) ? "Sem contagem" : countdown) : "Centro";
            if (isGate && !string.IsNullOrEmpty(threat)) waveLabel.text += $" • {threat}";
        }
    }
}
