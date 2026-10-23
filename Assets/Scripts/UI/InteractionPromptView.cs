using TMPro;
using UnityEngine;

namespace TCCGame.UI
{
    public sealed class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private TMP_Text description;

        public void SetDescription(string value) => description.text = value;
    }
}
