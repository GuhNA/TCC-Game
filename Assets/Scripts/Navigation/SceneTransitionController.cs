using System.Collections;
using TCCGame.Core;
using TCCGame.Gameplay.Campaign;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TCCGame.Gameplay.Navigation
{
    [DefaultExecutionOrder(-9000)]
    public sealed class SceneTransitionController : MonoBehaviour
    {
        private const float FadeDurationSeconds = 0.2f;

        private static SceneTransitionController instance;

        private Image fadeImage;
        private CampaignService pausedCampaign;
        private bool isTransitioning;
        private bool initialRestoreChecked;
        private string explorationReturnScene =
            SceneCatalog.DefaultExplorationReturn;

        [SerializeField]
        private GameObject fadePrefab;

        public static bool IsTransitioning =>
            instance != null && instance.isTransitioning;

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

            var controllerObject = new GameObject("Navigation Runtime");
            controllerObject.AddComponent<SceneTransitionController>();
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
            LoadFadeOverlay();
        }

        private void Start()
        {
            TryRestoreSavedLocation();
        }

        private void OnDestroy()
        {
            ReleaseCampaignPause();

            if (instance == this)
            {
                instance = null;
            }
        }

        public static bool TryTravelToGate(GateId gateId)
        {
            if (instance == null || !instance.enabled ||
                instance.fadeImage == null || instance.isTransitioning)
            {
                return false;
            }

            return instance.BeginTravelToGate(gateId);
        }

        public static bool TryReturnToCenter()
        {
            if (instance == null || !instance.enabled ||
                instance.fadeImage == null || instance.isTransitioning)
            {
                return false;
            }

            return instance.BeginReturnToCenter();
        }

        public static bool TryEnterExplorationCombat(
            int missionId,
            out string error
        )
        {
            if (instance == null || !instance.enabled ||
                instance.fadeImage == null || instance.isTransitioning)
            {
                error = "A transição de cena ainda não está disponível.";
                return false;
            }

            if (!Application.CanStreamedLevelBeLoaded(
                    SceneCatalog.ExplorationCombat
                ))
            {
                error =
                    "Adicione a cena ExplorationCombat ao Build Settings.";
                return false;
            }

            return instance.BeginExplorationCombat(missionId, out error);
        }

        public static bool TryReturnFromExplorationCombat()
        {
            if (instance == null || !instance.enabled ||
                instance.fadeImage == null || instance.isTransitioning)
            {
                return false;
            }

            instance.StartCoroutine(instance.LoadSceneRoutine(
                instance.explorationReturnScene,
                true
            ));
            return true;
        }

        private bool BeginTravelToGate(GateId gateId)
        {
            GameSession session = GameSessionProvider.Current;
            session.Navigation.EnterGate(gateId);
            StartCoroutine(LoadSceneRoutine(SceneCatalog.Gate, true));
            return true;
        }

        private bool BeginReturnToCenter()
        {
            GameSession session = GameSessionProvider.Current;

            if (!session.Navigation.ReturnToCenter())
            {
                return false;
            }

            StartCoroutine(LoadSceneRoutine(SceneCatalog.Center, true));
            return true;
        }

        private bool BeginExplorationCombat(
            int missionId,
            out string error
        )
        {
            if (!GameSessionProvider.Current.Exploration.TryBeginBattle(
                    missionId,
                    out _,
                    out error
                ))
            {
                return false;
            }

            explorationReturnScene = SceneManager.GetActiveScene().name;
            StartCoroutine(LoadSceneRoutine(
                SceneCatalog.ExplorationCombat,
                false
            ));
            return true;
        }

        private void TryRestoreSavedLocation()
        {
            if (initialRestoreChecked)
            {
                return;
            }

            initialRestoreChecked = true;
            NavigationSnapshot navigation =
                GameSessionProvider.Current.Navigation.Current;

            if (navigation.CurrentArea != CityArea.Gate ||
                SceneManager.GetActiveScene().name == SceneCatalog.Gate)
            {
                return;
            }

            SetFade(1f);
            StartCoroutine(LoadSceneRoutine(SceneCatalog.Gate, true));
        }

        private IEnumerator LoadSceneRoutine(
            string sceneName,
            bool placePlayerAfterLoad
        )
        {
            if (isTransitioning)
            {
                yield break;
            }

            isTransitioning = true;
            AcquireCampaignPause();

            yield return FadeTo(1f);
            GameSessionProvider.Current.Campaign.Checkpoint();

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Single
            );

            while (loadOperation != null && !loadOperation.isDone)
            {
                yield return null;
            }

            yield return null;
            if (placePlayerAfterLoad)
            {
                PlayerSpawnController.PlacePlayerInActiveScene();
            }
            yield return FadeTo(0f);

            ReleaseCampaignPause();
            isTransitioning = false;
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            float initialAlpha = fadeImage.color.a;
            float elapsed = 0f;

            while (elapsed < FadeDurationSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(
                    elapsed / FadeDurationSeconds
                );
                SetFade(Mathf.Lerp(initialAlpha, targetAlpha, progress));
                yield return null;
            }

            SetFade(targetAlpha);
        }

        private void AcquireCampaignPause()
        {
            pausedCampaign = GameSessionProvider.Current.Campaign;
            pausedCampaign.AcquirePause(this);
        }

        private void ReleaseCampaignPause()
        {
            if (pausedCampaign == null)
            {
                return;
            }

            pausedCampaign.ReleasePause(this);
            pausedCampaign = null;
        }

        private void LoadFadeOverlay()
        {
            fadePrefab ??= Resources.Load<GameObject>("UI/SceneFadeCanvas");
            if (fadePrefab == null)
            {
                Debug.LogError(
                    "SceneTransitionController requires " +
                    "Resources/UI/SceneFadeCanvas.prefab."
                );
                enabled = false;
                return;
            }

            GameObject instance = Instantiate(fadePrefab, transform);
            foreach (Image image in instance.GetComponentsInChildren<Image>(true))
            {
                if (image.name == "Fade Image")
                {
                    fadeImage = image;
                    break;
                }
            }

            if (fadeImage == null)
            {
                Debug.LogError("SceneFadeCanvas prefab requires Fade Image.");
                enabled = false;
                return;
            }

            SetFade(0f);
        }

        private void SetFade(float alpha)
        {
            Color color = fadeImage.color;
            color.a = Mathf.Clamp01(alpha);
            fadeImage.color = color;
            fadeImage.raycastTarget = color.a > 0.01f;
        }
    }
}
