using TCCGame.Core;
using UnityEngine;

namespace TCCGame.Gameplay.Campaign
{
    [DefaultExecutionOrder(-10000)]
    public sealed class CampaignRuntimeDriver : MonoBehaviour
    {
        private static CampaignRuntimeDriver instance;

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

            var driverObject = new GameObject("Campaign Runtime");
            driverObject.AddComponent<CampaignRuntimeDriver>();
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
        }

        private void Update()
        {
            GameSession session = GameSessionProvider.Current;
            if (session.Campaign.AdvanceClock(Time.unscaledDeltaTime))
            {
                session.TroopDeployment.ResolveArrivals();
                session.Campaign.ProcessWavesAndCheckpoint();
            }

            if (!session.Campaign.IsPaused)
            {
                session.Battles.TryResolvePending();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                GameSessionProvider.Current.Campaign.Checkpoint();
            }
        }

        private void OnApplicationQuit()
        {
            GameSessionProvider.Current.Campaign.Checkpoint();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
