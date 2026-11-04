using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Gameplay.Exploration
{
    public sealed class ExplorationRewardItemView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text quantityText;

        public void Bind(ExplorationResourceDefinition resource, TMP_FontAsset font)
        {
            icon.sprite = resource.Icon;
            icon.enabled = resource.Icon != null;
            icon.preserveAspect = true;
            nameText.text = resource.DisplayName;
            foreach (TMP_Text label in GetComponentsInChildren<TMP_Text>(true))
            {
                if (font != null)
                {
                    label.font = font;
                    label.fontSharedMaterial = font.material;
                }
            }
        }

        public void SetQuantities(long available, long totalPerCycle)
        {
            quantityText.text = $"{available} / {totalPerCycle}";
        }
    }
}
