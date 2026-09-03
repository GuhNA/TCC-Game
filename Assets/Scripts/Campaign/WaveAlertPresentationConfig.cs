using UnityEngine;

namespace TCCGame.Gameplay.Campaign
{
    [CreateAssetMenu(
        fileName = "WaveAlertPresentationConfig",
        menuName = "TCC Game/Campaign/Wave Alert Presentation"
    )]
    public sealed class WaveAlertPresentationConfig : ScriptableObject
    {
        [Min(0.5f)] public float bannerDurationSeconds = 3f;
        [Range(0f, 1f)] public float audioVolume = 0.2f;
        public bool enableAudio = true;
        public bool enablePulse = true;
        public Color safeColor = new(0.10f, 0.18f, 0.22f, 0.92f);
        public Color observedColor = new(0.10f, 0.26f, 0.42f, 0.94f);
        public Color announcedColor = new(0.52f, 0.40f, 0.08f, 0.96f);
        public Color imminentColor = new(0.70f, 0.27f, 0.06f, 0.97f);
        public Color criticalColor = new(0.62f, 0.08f, 0.08f, 0.98f);

        public static WaveAlertPresentationConfig LoadOrCreateDefault()
        {
            WaveAlertPresentationConfig configured =
                Resources.Load<WaveAlertPresentationConfig>(
                    nameof(WaveAlertPresentationConfig)
                );
            if (configured != null)
            {
                return configured;
            }

            WaveAlertPresentationConfig runtime =
                CreateInstance<WaveAlertPresentationConfig>();
            runtime.hideFlags = HideFlags.HideAndDontSave;
            return runtime;
        }

        private void OnValidate()
        {
            bannerDurationSeconds = Mathf.Max(0.5f, bannerDurationSeconds);
            audioVolume = Mathf.Clamp01(audioVolume);
        }
    }
}
