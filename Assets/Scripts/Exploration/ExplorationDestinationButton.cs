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
        private bool canStartNewMission;
        private bool counterAttackPending;
        private bool hasReport;
        private ExplorationMissionStatus? activeMissionStatus;

        public ExplorationEnvironment Environment => environment;

        public void Initialize(
            ExplorationMapController owner,
            bool unlocked,
            bool completed,
            ExplorationMissionStatus? missionStatus,
            bool canSendNewParty,
            bool underCounterAttack,
            bool reportAvailable
        )
        {
            controller = owner;
            activeMissionStatus = missionStatus;
            canStartNewMission = canSendNewParty;
            counterAttackPending = underCounterAttack;
            hasReport = reportAvailable;
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
                bool readyForBattle = activeMissionStatus ==
                    ExplorationMissionStatus.ReadyForBattle;
                bool canResumeBattle = activeMissionStatus ==
                    ExplorationMissionStatus.InBattle;
                bool canOpenCapturedArea = completed &&
                    (hasReport || counterAttackPending);
                bool canInspectReturn = activeMissionStatus ==
                    ExplorationMissionStatus.Returning;
                sendButton.interactable = interactionEnabled && unlocked &&
                    (readyForBattle || canResumeBattle || canInspectReturn ||
                     canOpenCapturedArea ||
                     (!completed && activeMissionStatus == null &&
                      canStartNewMission));
            }

            if (buttonText != null)
            {
                buttonText.text = GetButtonText(unlocked, completed);
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
            if (activeMissionStatus ==
                    ExplorationMissionStatus.ReadyForBattle ||
                activeMissionStatus == ExplorationMissionStatus.InBattle)
            {
                controller?.EnterBattle((int)environment);
                return;
            }

            if (activeMissionStatus == ExplorationMissionStatus.Returning ||
                (isCompleted && !counterAttackPending))
            {
                controller?.OpenAreaReport((int)environment);
                return;
            }

            controller?.OpenPartySelection((int)environment);
        }

        private string GetButtonText(bool unlocked, bool completed)
        {
            if (!unlocked)
            {
                return "Bloqueado";
            }

            return activeMissionStatus switch
            {
                ExplorationMissionStatus.Travelling => "A caminho...",
                ExplorationMissionStatus.ReadyForBattle => "Batalhar",
                ExplorationMissionStatus.InBattle => "Retomar batalha",
                ExplorationMissionStatus.Returning => "Retornando...",
                _ => counterAttackPending
                    ? "Defender"
                    : completed
                        ? hasReport ? "Relatório" : "Administrar"
                        : canStartNewMission ? "Enviar" : "Expedição ativa"
            };
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
