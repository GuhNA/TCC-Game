using TCCGame.Core;
using UnityEngine;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Optional scene bridge for the arrival notification. Place it beside an
    /// existing AlertPopup; it keeps checking travel even while the map is closed.
    /// </summary>
    public sealed class ExplorationArrivalAlert : MonoBehaviour
    {
        [SerializeField] private AlertPopup alert;
        private ExplorationService service;

        private void OnEnable()
        {
            service = GameSessionProvider.Current.Exploration;
            service.MissionArrived += ShowArrival;
            service.RefreshArrivals();
        }

        private void Update() => service?.RefreshArrivals();

        private void OnDisable()
        {
            if (service != null) service.MissionArrived -= ShowArrival;
        }

        private void ShowArrival(ExplorationMissionSaveData mission)
        {
            alert?.Show($"O grupo chegou a {mission.environment}. Abra o mapa para iniciar a batalha.");
        }
    }
}
