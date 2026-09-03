using TMPro;
using TCCGame.Core;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Gameplay.Navigation
{
    public sealed class GateContextPresenter : MonoBehaviour
    {
        private TMP_Text contextText;
        private double nextRefreshAt;

        [SerializeField]
        private GameObject contextPrefab;

        private void Awake()
        {
            LoadUi();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            if (Time.unscaledTimeAsDouble < nextRefreshAt)
            {
                return;
            }

            Refresh();
        }

        private void Refresh()
        {
            nextRefreshAt = Time.unscaledTimeAsDouble + 0.25d;
            NavigationSnapshot navigation =
                GameSessionProvider.Current.Navigation.Current;
            string gateName = GateDisplayNames.GetUppercase(
                navigation.ActiveGateId
            );
            GateThreatViewData threat = GameSessionProvider.Current.Threats
                .BuildGateCard(navigation.ActiveGateId);
            GateStateSnapshot gate = GameSessionProvider.Current.Campaign
                .GetGate(navigation.ActiveGateId);
            contextText.text = $"PORTÃO {gateName}\n" +
                $"Integridade: {gate.CurrentHealth}/" +
                $"{GateSaveData.DefaultHealth}\n" +
                $"{threat.Status}" +
                (string.IsNullOrEmpty(threat.Countdown)
                    ? string.Empty
                    : $" · {threat.Countdown}") +
                (string.IsNullOrEmpty(threat.Threat)
                    ? string.Empty
                    : $" · {threat.Threat}");
        }

        private void LoadUi()
        {
            contextPrefab ??= Resources.Load<GameObject>("UI/GateContextCanvas");
            if (contextPrefab == null)
            {
                Debug.LogError(
                    "GateContextPresenter requires Resources/UI/GateContextCanvas.prefab."
                );
                enabled = false;
                return;
            }

            GameObject instance = Instantiate(contextPrefab, transform);
            contextText = instance.GetComponentInChildren<TMP_Text>(true);
            if (contextText == null)
            {
                Debug.LogError("GateContextCanvas prefab must contain TMP text.");
                enabled = false;
            }
        }
    }
}
