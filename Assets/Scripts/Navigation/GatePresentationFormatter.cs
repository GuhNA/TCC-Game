using System;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Campaign;

namespace TCCGame.Gameplay.Navigation
{
    public static class GatePresentationFormatter
    {
        public static string BuildGateDetails(
            CampaignService campaign,
            GateId gateId
        )
        {
            if (campaign == null)
            {
                throw new ArgumentNullException(nameof(campaign));
            }

            GateStateSnapshot gate = campaign.GetGate(gateId);
            string waveLine = BuildWaveLine(campaign, gateId);

            return $"Portão {GateDisplayNames.Get(gateId)}\n" +
                $"Integridade: {gate.CurrentHealth}/" +
                $"{GateSaveData.DefaultHealth}\n" +
                waveLine;
        }

        public static string BuildWaveLine(
            CampaignService campaign,
            GateId gateId
        )
        {
            WaveSnapshot? currentWave = campaign.Waves.CurrentWave;

            if (!currentWave.HasValue ||
                currentWave.Value.GateId != gateId)
            {
                return "Nenhum ataque anunciado para este portão.";
            }

            WaveSnapshot wave = currentWave.Value;

            if (wave.Status == WaveStatus.AwaitingResolution)
            {
                return $"Ataque aguardando defesa — Ameaça {wave.ThreatPower}";
            }

            double remaining = Math.Max(
                0d,
                wave.ArrivalStrategicSeconds -
                campaign.ElapsedStrategicSeconds
            );

            return $"Ataque previsto: {FormatRemainingTime(remaining)}\n" +
                $"Ameaça estimada: {wave.ThreatPower}";
        }

        public static string FormatRemainingTime(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds))
            {
                throw new ArgumentOutOfRangeException(nameof(seconds));
            }

            int totalSeconds = Math.Max(0, (int)Math.Ceiling(seconds));
            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;
            return $"{minutes:00}:{remainingSeconds:00}";
        }
    }
}
