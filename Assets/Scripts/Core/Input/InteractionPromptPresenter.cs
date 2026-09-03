using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TCCGame.Core.Input
{
    [DefaultExecutionOrder(-8000)]
    public sealed class InteractionPromptPresenter : MonoBehaviour
    {
        private static InteractionPromptPresenter instance;

        private PlayerInteractor interactor;
        private CanvasGroup canvasGroup;
        private TMP_Text promptText;

        [SerializeField]
        private GameObject promptPrefab;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration
        )]
        private static void ResetStatics()
        {
            instance = null;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad
        )]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            var promptObject = new GameObject("Interaction Prompt");
            promptObject.AddComponent<InteractionPromptPresenter>();
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
            LoadUiPrefab();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            AttachToPlayer();
        }

        private void Update()
        {
            if (interactor == null)
            {
                AttachToPlayer();
                return;
            }

            UpdatePrompt(interactor.CurrentInteractable);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;

            if (interactor != null)
            {
                interactor.CurrentInteractableChanged -= UpdatePrompt;
            }

            if (instance == this)
            {
                instance = null;
            }
        }

        private void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            AttachToPlayer();
        }

        private void AttachToPlayer()
        {
            PlayerInteractor nextInteractor = Object.FindFirstObjectByType<
                PlayerInteractor
            >();

            if (nextInteractor == interactor)
            {
                return;
            }

            if (interactor != null)
            {
                interactor.CurrentInteractableChanged -= UpdatePrompt;
            }

            interactor = nextInteractor;

            if (interactor != null)
            {
                interactor.CurrentInteractableChanged += UpdatePrompt;
                UpdatePrompt(interactor.CurrentInteractable);
                return;
            }

            SetVisible(false);
        }

        private void UpdatePrompt(IInteractable interactable)
        {
            if (interactor == null || interactor.HasModalOpen ||
                UiMenuState.HasOpenMenu ||
                interactable == null || !interactable.CanInteract)
            {
                SetVisible(false);
                return;
            }

            string description = interactable is IInteractionPromptProvider
                provider
                ? provider.InteractionPrompt
                : "Interagir";

            promptText.text = $"[E] {description}";
            SetVisible(true);
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private void LoadUiPrefab()
        {
            if (promptPrefab == null)
            {
                promptPrefab = Resources.Load<GameObject>(
                    "UI/InteractionPrompt"
                );
            }

            if (promptPrefab == null)
            {
                Debug.LogError(
                    "InteractionPromptPresenter requires " +
                    "Resources/UI/InteractionPrompt.prefab."
                );
                return;
            }

            GameObject instance = Instantiate(promptPrefab, transform);
            instance.name = "Interaction Prompt Canvas";
            canvasGroup = instance.GetComponentInChildren<CanvasGroup>(true);
            promptText = instance.GetComponentInChildren<TMP_Text>(true);
            if (canvasGroup == null || promptText == null)
            {
                Debug.LogError(
                    "InteractionPrompt prefab must contain CanvasGroup and TMP text."
                );
            }

            SetVisible(false);
        }
    }
}
