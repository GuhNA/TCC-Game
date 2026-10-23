using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.UI
{
    public sealed class GateHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text health;
        [SerializeField] private TMP_Text wave;
        [SerializeField] private Image healthFill;
        [SerializeField] private Color healthyColor = new Color(.42f, .70f, .24f);
        [SerializeField] private Color criticalColor = new Color(.85f, .22f, .12f);

        public void Refresh(string gateName, int currentHealth, int maximumHealth,
            string status, string countdown, string threat)
        {
            title.text = gateName;
            health.text = $"{currentHealth} / {maximumHealth}";
            float ratio = maximumHealth > 0 ? Mathf.Clamp01((float)currentHealth / maximumHealth) : 0f;
            // A plain-color Image has no sprite, so Unity ignores Image.fillAmount.
            // Scale and offset the solid bar to keep its left edge fixed instead.
            RectTransform fillRect = healthFill.rectTransform;
            fillRect.localScale = new Vector3(ratio, 1f, 1f);
            fillRect.anchoredPosition = new Vector2(
                -fillRect.sizeDelta.x * (1f - ratio) * .5f, 0f);
            healthFill.color = ratio <= .3f ? criticalColor : healthyColor;
            health.color = ratio <= .3f ? criticalColor : healthyColor;
            wave.text = string.IsNullOrEmpty(countdown) ? status : $"{status}  {countdown}";
            // Preserve strategic information from the original HUD, even when no wave banner is visible.
            if (!string.IsNullOrEmpty(threat)) wave.text += $" • {threat}";
        }
    }
}
