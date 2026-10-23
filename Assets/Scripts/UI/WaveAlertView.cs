using TMPro;
using TCCGame.Gameplay.Campaign;
using UnityEngine;

namespace TCCGame.UI
{
    public sealed class WaveAlertView : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text message;

        public void Present(WaveAlertMessage alert, Color emphasis)
        {
            title.text = alert.Stage switch
            {
                WaveWarningStage.Announced => "AVISO DOS BATEDORES",
                WaveWarningStage.Imminent => "INIMIGOS SE APROXIMAM",
                _ => "ATENÇÃO NO PORTÃO"
            };
            // Preserve the dark metal surface and readable contrast at every severity.
            title.color = Color.Lerp(emphasis, new Color(1f, .82f, .33f), .65f);
            message.text = alert.Text;
        }
    }
}
