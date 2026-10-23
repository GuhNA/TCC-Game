using System;
using System.Collections.Generic;
using TMPro;
using TCCGame.Core;
using TCCGame.Core.Input;
using UnityEngine;
using UnityEngine.UI;
using TCCGame.UI;

namespace TCCGame.Gameplay.Campaign
{
    [DefaultExecutionOrder(-7600)]
    public sealed class GlobalThreatHud : MonoBehaviour
    {
        private static GlobalThreatHud instance;

        private readonly Queue<WaveAlertMessage> alertQueue = new();
        private WaveAlertPresentationConfig config;
        private CanvasGroup bannerGroup;
        private Image bannerBackground;
        private TextMeshProUGUI bannerText;
        private WaveAlertView themedView;
        private AudioSource audioSource;
        [SerializeField]
        private GameObject alertPrefab;
        private WaveAlertMessage? activeAlert;
        private float alertRemaining;
        private bool subscribed;

        public static bool Exists => instance != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            GameObject hudObject = new GameObject("Global Threat HUD");
            hudObject.AddComponent<GlobalThreatHud>();
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
            config = WaveAlertPresentationConfig.LoadOrCreateDefault();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = config.audioVolume;
            LoadInterface();
        }

        private void Start()
        {
            Subscribe();
        }

        private void Update()
        {
            Subscribe();

            UpdateBanner();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Subscribe()
        {
            if (subscribed)
            {
                return;
            }

            WaveDirectorService waves =
                GameSessionProvider.Current.Campaign.Waves;
            waves.WaveWarningChanged += HandleWarningChanged;
            waves.WaveBecameDue += HandleWaveBecameDue;
            GameSessionProvider.Current.Battles.BattleResolved +=
                HandleBattleResolved;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            WaveDirectorService waves =
                GameSessionProvider.Current.Campaign.Waves;
            waves.WaveWarningChanged -= HandleWarningChanged;
            waves.WaveBecameDue -= HandleWaveBecameDue;
            GameSessionProvider.Current.Battles.BattleResolved -=
                HandleBattleResolved;
            subscribed = false;
        }

        private void HandleWarningChanged(
            WaveSnapshot wave,
            WaveWarningStage stage
        )
        {
            alertQueue.Enqueue(
                GameSessionProvider.Current.Threats.BuildAlert(wave, stage)
            );
        }

        private void HandleWaveBecameDue(WaveSnapshot wave)
        {
            alertQueue.Enqueue(
                GameSessionProvider.Current.Threats
                    .BuildAttackStartedAlert(wave)
            );
        }

        private void HandleBattleResolved(BattleReportSnapshot report)
        {
            alertQueue.Enqueue(
                GameSessionProvider.Current.Threats
                    .BuildBattleResolvedAlert(report)
            );
        }

        private void UpdateBanner()
        {
            if (bannerGroup == null) return;
            if (UiMenuState.HasOpenMenu)
            {
                bannerGroup.alpha = 0f;
                return;
            }

            if (!activeAlert.HasValue)
            {
                if (alertQueue.Count == 0)
                {
                    bannerGroup.alpha = 0f;
                    return;
                }

                ShowNextAlert();
            }

            // A modal pauses the active alert; restore it when the menu closes.
            bannerGroup.alpha = 1f;

            alertRemaining -= Time.unscaledDeltaTime;
            if (alertRemaining > 0f)
            {
                return;
            }

            activeAlert = null;
            bannerGroup.alpha = 0f;
        }

        private void ShowNextAlert()
        {
            WaveAlertMessage alert = alertQueue.Dequeue();
            activeAlert = alert;
            alertRemaining = config.bannerDurationSeconds;
            if (themedView != null) themedView.Present(alert, GetAlertColor(alert.Stage));
            else
            {
                bannerText.text = alert.Text;
                bannerBackground.color = GetAlertColor(alert.Stage);
            }
            bannerGroup.alpha = 1f;

            if (config.enableAudio && config.audioVolume > 0f)
            {
                audioSource.PlayOneShot(
                    CreateAlertTone(alert.Stage),
                    config.audioVolume
                );
            }
        }

        private void LoadInterface()
        {
            alertPrefab ??= Resources.Load<GameObject>("UI/WaveAlertCanvas");
            if (alertPrefab == null)
            {
                Debug.LogError(
                    "GlobalThreatHud requires Resources/UI/WaveAlertCanvas.prefab."
                );
                enabled = false;
                return;
            }

            GameObject instance = Instantiate(alertPrefab, transform);
            themedView = instance.GetComponentInChildren<WaveAlertView>(true);
            bannerGroup = instance.GetComponentInChildren<CanvasGroup>(true);
            bannerBackground = instance.GetComponentInChildren<Image>(true);
            bannerText = instance.GetComponentInChildren<TextMeshProUGUI>(true);
            if (bannerGroup == null || bannerBackground == null ||
                bannerText == null)
            {
                Debug.LogError(
                    "WaveAlertCanvas prefab must contain CanvasGroup, Image and TMP text."
                );
                enabled = false;
                return;
            }

            bannerGroup.alpha = 0f;
            bannerGroup.interactable = false;
            bannerGroup.blocksRaycasts = false;
            bannerBackground.raycastTarget = false;
        }

        private Color GetAlertColor(WaveWarningStage stage)
        {
            return stage switch
            {
                WaveWarningStage.Announced => config.announcedColor,
                WaveWarningStage.Imminent => config.imminentColor,
                _ => config.criticalColor
            };
        }

        private static AudioClip CreateAlertTone(WaveWarningStage stage)
        {
            int sampleRate = 22050;
            float duration = stage == WaveWarningStage.Critical ? 0.28f : 0.18f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float frequency = stage switch
            {
                WaveWarningStage.Announced => 520f,
                WaveWarningStage.Imminent => 660f,
                _ => 820f
            };
            float[] samples = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                float progress = index / (float)sampleCount;
                float envelope = 1f - progress;
                samples[index] = Mathf.Sin(
                    2f * Mathf.PI * frequency * index / sampleRate
                ) * envelope * 0.22f;
            }

            AudioClip clip = AudioClip.Create(
                $"Wave Alert {stage}",
                sampleCount,
                1,
                sampleRate,
                false
            );
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
