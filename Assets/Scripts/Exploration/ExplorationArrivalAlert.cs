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
            service.MissionReturned += ShowReturn;
            service.RefreshArrivals();
        }

        private void Update() => service?.RefreshArrivals();

        private void OnDisable()
        {
            if (service != null)
            {
                service.MissionArrived -= ShowArrival;
                service.MissionReturned -= ShowReturn;
            }
        }

        private void ShowArrival(ExplorationMissionSaveData mission)
        {
            string environmentName =
                ExplorationProgression.GetDisplayName(mission.environment);
            alert?.Show(
                $"O grupo chegou a {environmentName}. " +
                "Abra o mapa para iniciar a batalha."
            );
        }

        private void ShowReturn(ExplorationMissionSaveData mission)
        {
            string environmentName =
                ExplorationProgression.GetDisplayName(mission.environment);
            alert?.Show(
                $"A equipe retornou de {environmentName} com os recursos. " +
                "Abra o mapa e selecione a região para ver o relatório."
            );
        }
    }
}
