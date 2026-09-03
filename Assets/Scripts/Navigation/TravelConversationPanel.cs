using TMPro;
using TCCGame.Core;
using TCCGame.Core.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TCCGame.Gameplay.Navigation
{
    /// <summary>
    /// Conversa curta usada pelos NPCs de deslocamento. Mantem o jogador no mundo e
    /// deixa explicita a decisao antes de trocar de cena.
    /// </summary>
    [DefaultExecutionOrder(-7900)]
    public sealed class TravelConversationPanel : MonoBehaviour
    {
        private static TravelConversationPanel instance;

        private GameObject panelRoot;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI bodyText;
        private Button confirmButton;
        private CityTravelNpc sourceNpc;
        private PlayerInteractor lockedInteractor;
        private bool visible;

        [SerializeField]
        private GameObject interfacePrefab;

        public static bool IsVisible => instance != null && instance.visible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInstance();
        }

        public static void Show(CityTravelNpc npc, GameObject player)
        {
            if (npc == null || player == null || !EnsureInstance())
            {
                return;
            }

            instance.Open(npc, player);
        }

        private static bool EnsureInstance()
        {
            if (instance != null)
            {
                return true;
            }

            instance = FindFirstObjectByType<TravelConversationPanel>();
            if (instance != null)
            {
                return true;
            }

            GameObject panelObject = new GameObject("Travel Conversation Panel");
            instance = panelObject.AddComponent<TravelConversationPanel>();
            DontDestroyOnLoad(panelObject);
            return true;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInterface();
        }

        private void Update()
        {
            if (!visible)
            {
                return;
            }

            if (GameInput.WasCancelPressedThisFrame())
            {
                Close();
                return;
            }

            if (GameInput.WasSubmitPressedThisFrame())
            {
                ConfirmTravel();
            }
        }

        private void OnDestroy()
        {
            UiMenuState.SetOpen(this, false);

            if (instance == this)
            {
                instance = null;
            }

            ReleasePlayerLock();
        }

        private void Open(CityTravelNpc npc, GameObject player)
        {
            if (visible || SceneTransitionController.IsTransitioning)
            {
                return;
            }

            lockedInteractor = player.GetComponent<PlayerInteractor>();
            if (lockedInteractor == null)
            {
                return;
            }

            sourceNpc = npc;
            TravelDestination destination = npc.Destination;

            titleText.text = npc.NpcDisplayName.ToUpperInvariant();
            if (destination.Kind == TravelDestinationKind.Gate)
            {
                string gateName = GateDisplayNames.Get(destination.GateId);
                bodyText.text = $"Posso levá-lo ao Portão {gateName}.\n\n" +
                                GatePresentationFormatter.BuildGateDetails(GameSessionProvider.Current.Campaign, destination.GateId);
                SetButtonLabel(confirmButton, $"Ir ao Portão {gateName}");
            }
            else
            {
                bodyText.text = "A estrada até o centro está livre. Da cidade você poderá redistribuir a guarda e consultar os demais portões.";
                SetButtonLabel(confirmButton, "Voltar ao Centro");
            }

            visible = true;
            panelRoot.SetActive(true);
            UiMenuState.SetOpen(this, true);
            lockedInteractor.AcquireModalLock(this);
            EventSystem.current?.SetSelectedGameObject(confirmButton.gameObject);
        }

        private void ConfirmTravel()
        {
            if (!visible || sourceNpc == null)
            {
                return;
            }

            TravelDestination destination = sourceNpc.Destination;
            Close();

            if (destination.Kind == TravelDestinationKind.Gate)
            {
                SceneTransitionController.TryTravelToGate(destination.GateId);
            }
            else
            {
                SceneTransitionController.TryReturnToCenter();
            }
        }

        private void Close()
        {
            if (!visible)
            {
                return;
            }

            visible = false;
            sourceNpc = null;
            UiMenuState.SetOpen(this, false);
            panelRoot.SetActive(false);
            ReleasePlayerLock();
        }

        private void ReleasePlayerLock()
        {
            if (lockedInteractor != null)
            {
                lockedInteractor.ReleaseModalLock(this);
                lockedInteractor = null;
            }
        }

        private void LoadInterface()
        {
            interfacePrefab ??= Resources.Load<GameObject>(
                "UI/TravelConversationCanvas"
            );
            if (interfacePrefab == null)
            {
                Debug.LogError(
                    "TravelConversationPanel requires " +
                    "Resources/UI/TravelConversationCanvas.prefab."
                );
                enabled = false;
                return;
            }

            GameObject instance = Instantiate(interfacePrefab, transform);
            panelRoot = FindChild(instance, "Travel Dialog");
            titleText = FindChild(instance, "Title")
                ?.GetComponent<TextMeshProUGUI>();
            bodyText = FindChild(instance, "Body")
                ?.GetComponent<TextMeshProUGUI>();
            confirmButton = FindChild(instance, "Confirm")
                ?.GetComponent<Button>();
            Button cancelButton = FindChild(instance, "Cancel")
                ?.GetComponent<Button>();
            if (panelRoot == null || titleText == null || bodyText == null ||
                confirmButton == null || cancelButton == null)
            {
                Debug.LogError("TravelConversationCanvas prefab is incomplete.");
                enabled = false;
                return;
            }

            confirmButton.onClick.AddListener(ConfirmTravel);
            cancelButton.onClick.AddListener(Close);
            panelRoot.SetActive(false);
        }

        private static GameObject FindChild(GameObject root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static void SetButtonLabel(Button button, string text)
        {
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = text;
            }
        }
    }
}
