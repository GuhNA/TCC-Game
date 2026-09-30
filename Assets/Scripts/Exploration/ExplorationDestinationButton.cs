using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Connects one map button to one destination. The controller controls whether
    /// it is interactable according to the persistent exploration progression.
    /// </summary>
    public sealed class ExplorationDestinationButton : MonoBehaviour
    {
        [SerializeField] private ExplorationEnvironment environment;
        [SerializeField] private Button sendButton;
        [SerializeField] private TMP_Text buttonText;
        [Tooltip("Optional normal/completed visual. Defaults to the Send Button object.")]
        [SerializeField] private GameObject availableStateRoot;
        [Tooltip("May be a child already in the scene or a prefab asset. Prefab assets are instantiated automatically.")]
        [FormerlySerializedAs("lockedIndicator")]
        [SerializeField] private GameObject lockedStatePrefabOrInstance;
        [Tooltip("Parent used when Locked State is a prefab. Defaults to the normal button's parent.")]
        [SerializeField] private Transform lockedStateParent;

        private ExplorationMapController controller;
        private GameObject lockedStateInstance;
        private bool ownsLockedStateInstance;
        private bool interactionEnabled = true;
        private bool isUnlocked;
        private bool isCompleted;

        public ExplorationEnvironment Environment => environment;

        public void Initialize(
            ExplorationMapController owner,
            bool unlocked,
            bool completed
        )
        {
            controller = owner;
            sendButton ??= GetComponent<Button>();
            sendButton?.onClick.RemoveListener(OpenSelection);
            sendButton?.onClick.AddListener(OpenSelection);
            Refresh(unlocked, completed);
        }

        public void Refresh(bool unlocked, bool completed)
        {
            isUnlocked = unlocked;
            isCompleted = completed;
            GameObject availableRoot = availableStateRoot != null
                ? availableStateRoot
                : sendButton != null ? sendButton.gameObject : gameObject;
            GameObject lockedRoot = GetOrCreateLockedState();

            // A locked destination swaps the normal button for its locked visual.
            // A completed destination keeps the normal visual, but disables it.
            availableRoot.SetActive(unlocked);
            lockedRoot?.SetActive(!unlocked);

            if (sendButton != null)
            {
                sendButton.interactable =
                    interactionEnabled && unlocked && !completed;
            }

            if (buttonText != null)
            {
                buttonText.text = completed
                    ? "Concluído"
                    : unlocked ? "Enviar" : "Bloqueado";
            }
        }

        /// <summary>
        /// Temporarily blocks this destination while another map panel is open.
        /// Refreshing preserves the permanent unlocked/completed rules.
        /// </summary>
        public void SetInteractionEnabled(bool value)
        {
            interactionEnabled = value;
            Refresh(isUnlocked, isCompleted);
        }

        private void OnDestroy()
        {
            sendButton?.onClick.RemoveListener(OpenSelection);

            if (ownsLockedStateInstance && lockedStateInstance != null)
            {
                Destroy(lockedStateInstance);
            }
        }

        private void OpenSelection()
        {
            controller?.OpenPartySelection((int)environment);
        }

        private GameObject GetOrCreateLockedState()
        {
            if (lockedStateInstance != null)
            {
                return lockedStateInstance;
            }

            if (lockedStatePrefabOrInstance == null)
            {
                return null;
            }

            // Scene objects can be toggled directly. Prefab assets do not belong
            // to a loaded scene and must first be instantiated.
            if (lockedStatePrefabOrInstance.scene.IsValid())
            {
                lockedStateInstance = lockedStatePrefabOrInstance;
                return lockedStateInstance;
            }

            Transform parent = lockedStateParent;
            if (parent == null && sendButton != null)
            {
                parent = sendButton.transform.parent;
            }

            if (parent == null)
            {
                parent = transform.parent;
            }

            lockedStateInstance = Instantiate(
                lockedStatePrefabOrInstance,
                parent,
                false
            );
            lockedStateInstance.name =
                lockedStatePrefabOrInstance.name + " (Locked)";
            ownsLockedStateInstance = true;
            return lockedStateInstance;
        }
    }
}
