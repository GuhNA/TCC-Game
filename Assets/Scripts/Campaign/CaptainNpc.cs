using TCCGame.Core.Input;
using TCCGame.Core;
using TCCGame.Gameplay.Navigation;
using TMPro;
using UnityEngine;

namespace TCCGame.Gameplay.Campaign
{
    public sealed class CaptainNpc : MonoBehaviour,
        IInteractable,
        IInteractionPromptProvider
    {
        [SerializeField]
        private string npcDisplayName = "Capitão da Guarda";

        [SerializeField]
        private GameObject alertIndicatorPrefab;

        private GameObject alertIndicator;
        private TextMeshProUGUI alertText;
        private float refreshAt;

        private void Awake()
        {
            LoadAlertIndicator();
        }

        private void Update()
        {
            if (Time.unscaledTime < refreshAt)
            {
                return;
            }

            refreshAt = Time.unscaledTime + 0.25f;
            RefreshAlertIndicator();
        }

        private void OnDestroy()
        {
            if (alertIndicator != null)
            {
                Destroy(alertIndicator);
            }
        }

        public bool CanInteract => enabled &&
            !GarrisonManagementPanel.IsVisible;

        public string InteractionPrompt =>
            $"Conversar — {npcDisplayName}";

        public void Interact(GameObject player)
        {
            if (CanInteract)
            {
                GarrisonManagementPanel.Show(player);
            }
        }

        private void LoadAlertIndicator()
        {
            alertIndicatorPrefab ??= Resources.Load<GameObject>(
                "UI/WaveAlertIndicator"
            );
            if (alertIndicatorPrefab == null)
            {
                Debug.LogError(
                    "CaptainNpc requires Resources/UI/WaveAlertIndicator.prefab."
                );
                return;
            }

            alertIndicator = Instantiate(alertIndicatorPrefab, transform);
            alertIndicator.name = "Wave Alert Indicator";
            SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                // Keep the world-space marker above the sprite regardless of the NPC scale.
                Vector3 parentScale = transform.lossyScale;
                alertIndicator.transform.localScale = new Vector3(
                    .01f / Mathf.Max(.001f, Mathf.Abs(parentScale.x)),
                    .01f / Mathf.Max(.001f, Mathf.Abs(parentScale.y)), 1f);
                alertIndicator.transform.position = new Vector3(renderer.bounds.center.x,
                    renderer.bounds.max.y + .25f, transform.position.z);
            }
            alertText = alertIndicator.GetComponentInChildren<TextMeshProUGUI>(
                true
            );
            if (alertText == null)
            {
                Debug.LogError(
                    "WaveAlertIndicator prefab must contain TMP text."
                );
                return;
            }
            alertIndicator.SetActive(false);
        }

        private void RefreshAlertIndicator()
        {
            if (alertText == null || GameSessionProvider.Current == null)
            {
                return;
            }

            NavigationSnapshot navigation = GameSessionProvider.Current
                .Navigation.Current;
            if (!navigation.IsAtGate)
            {
                alertIndicator.SetActive(false);
                return;
            }

            GateThreatViewData threat = GameSessionProvider.Current.Threats
                .BuildGateCard(navigation.ActiveGateId);
            bool hasAlert = !UiMenuState.HasOpenMenu &&
                (threat.State >= GateThreatState.Announced ||
                !string.IsNullOrEmpty(threat.ReportNotice));
            alertText.color = threat.State >= GateThreatState.Critical
                ? new Color(1f, 0.18f, 0.12f, 1f)
                : new Color(1f, 0.82f, 0.12f, 1f);
            alertIndicator.SetActive(hasAlert);
        }
    }
}
