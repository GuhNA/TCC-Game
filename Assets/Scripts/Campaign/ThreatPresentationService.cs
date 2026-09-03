using System;
using TCCGame.Gameplay.Navigation;

namespace TCCGame.Gameplay.Campaign
{
    public enum GateThreatState
    {
        Safe,
        Observed,
        Announced,
        Imminent,
        Critical,
        AttackStarted,
        Victory,
        Defeat
    }

    public readonly struct GateThreatViewData
    {
        public GateId GateId { get; }
        public GateThreatState State { get; }
        public string Direction { get; }
        public string Status { get; }
        public string Countdown { get; }
        public string Threat { get; }
        public string Defense { get; }
        public string Composition { get; }
        public string Outlook { get; }
        public string Explanation { get; }
        public string ReportNotice { get; }

        public GateThreatViewData(
            GateId gateId,
            GateThreatState state,
            string direction,
            string status,
            string countdown,
            string threat,
            string defense,
            string composition,
            string outlook,
            string explanation,
            string reportNotice = ""
        )
        {
            GateId = gateId;
            State = state;
            Direction = direction;
            Status = status;
            Countdown = countdown;
            Threat = threat;
            Defense = defense;
            Composition = composition;
            Outlook = outlook;
            Explanation = explanation;
            ReportNotice = reportNotice;
        }
    }

    public readonly struct WaveAlertMessage
    {
        public WaveWarningStage Stage { get; }
        public GateId GateId { get; }
        public string Text { get; }

        public WaveAlertMessage(
            WaveWarningStage stage,
            GateId gateId,
            string text
        )
        {
            Stage = stage;
            GateId = gateId;
            Text = text;
        }
    }

    public sealed class ThreatPresentationService
    {
        private readonly CampaignService campaign;
        private readonly ThreatForecastService forecast;

        public ThreatPresentationService(
            CampaignService campaign,
            CampaignRules rules
        )
        {
            this.campaign = campaign ??
                throw new ArgumentNullException(nameof(campaign));
            forecast = new ThreatForecastService(rules);
        }

        public GateThreatViewData BuildGateCard(GateId gateId)
        {
            GateStateSnapshot gate = campaign.GetGate(gateId);
            WaveSnapshot? current = campaign.Waves.CurrentWave;
            double baseDefense = forecast.CalculateBaseDefense(gate);
            string defense = $"Def. {baseDefense:0.#}";
            BattleReportSnapshot? pending = campaign.GetLatestBattleReport(
                gateId,
                onlyUnacknowledged: true
            );
            string reportNotice = pending.HasValue
                ? $"RELATÓRIO DA ONDA {pending.Value.WaveSequence}"
                : string.Empty;

            if (campaign.Outcome != CampaignOutcome.Ongoing)
            {
                bool defeat = campaign.Outcome == CampaignOutcome.Defeat;
                string status = defeat && gate.CurrentHealth == 0
                    ? "PORTÃO ROMPIDO"
                    : defeat
                        ? "CIDADE DERROTADA"
                        : "CAMPANHA VENCIDA";
                return new GateThreatViewData(
                    gateId,
                    defeat ? GateThreatState.Defeat : GateThreatState.Victory,
                    GateDisplayNames.GetUppercase(gateId),
                    status,
                    string.Empty,
                    string.Empty,
                    defense,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    reportNotice
                );
            }

            if (!current.HasValue || current.Value.GateId != gateId)
            {
                return new GateThreatViewData(
                    gateId,
                    GateThreatState.Safe,
                    GateDisplayNames.GetUppercase(gateId),
                    "SEGURO",
                    string.Empty,
                    string.Empty,
                    defense,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    reportNotice
                );
            }

            WaveSnapshot wave = current.Value;
            GateThreatState state = GetState(wave);
            bool revealThreat = state >= GateThreatState.Announced;
            bool revealComposition = state >= GateThreatState.Imminent;
            ThreatForecast result = forecast.Calculate(gate, wave);
            string countdown = wave.Status == WaveStatus.Scheduled
                ? GatePresentationFormatter.FormatRemainingTime(
                    Math.Max(
                        0d,
                        wave.ArrivalStrategicSeconds -
                        campaign.ElapsedStrategicSeconds
                    )
                )
                : string.Empty;

            return new GateThreatViewData(
                gateId,
                state,
                GateDisplayNames.GetUppercase(gateId),
                GetStatus(state),
                revealThreat ? countdown : string.Empty,
                revealThreat ? $"Ameaça {wave.ThreatPower}" : string.Empty,
                defense,
                revealComposition
                    ? $"S {wave.Raiders} · B {wave.Brutes} · X {wave.Shamans}"
                    : string.Empty,
                revealThreat ? GetOutlook(result.Outlook) : string.Empty,
                revealComposition ? result.Explanation : string.Empty,
                reportNotice
            );
        }

        public WaveAlertMessage BuildAlert(
            WaveSnapshot wave,
            WaveWarningStage stage
        )
        {
            string gate = GateDisplayNames.Get(wave.GateId);
            string text = stage switch
            {
                WaveWarningStage.Announced =>
                    $"Batedor: inimigos avançam para o Portão {gate}.",
                WaveWarningStage.Imminent =>
                    $"Ataque iminente no Portão {gate} — prepare a guarnição.",
                WaveWarningStage.Critical =>
                    $"ALERTA CRÍTICO — Portão {gate}!",
                _ => $"Movimento inimigo próximo ao Portão {gate}."
            };
            return new WaveAlertMessage(stage, wave.GateId, text);
        }

        public WaveAlertMessage BuildAttackStartedAlert(WaveSnapshot wave)
        {
            return new WaveAlertMessage(
                WaveWarningStage.Critical,
                wave.GateId,
                $"ATAQUE INICIADO — Portão {GateDisplayNames.Get(wave.GateId)}!"
            );
        }

        public WaveAlertMessage BuildBattleResolvedAlert(
            BattleReportSnapshot report
        )
        {
            string gate = GateDisplayNames.Get(report.GateId);
            string text = report.Outcome == BattleOutcome.Breached
                ? $"PORTÃO {gate.ToUpperInvariant()} ROMPIDO — a cidade foi derrotada."
                : $"Portão {gate}: onda repelida. " +
                  $"Baixas {report.TotalLosses}, portão -{report.GateDamage}, " +
                  $"espólio +{report.Spoils} ouro.";
            return new WaveAlertMessage(
                report.Outcome == BattleOutcome.Breached
                    ? WaveWarningStage.Critical
                    : WaveWarningStage.Announced,
                report.GateId,
                text
            );
        }

        private static GateThreatState GetState(WaveSnapshot wave)
        {
            if (wave.Status == WaveStatus.AwaitingResolution)
            {
                return GateThreatState.AttackStarted;
            }

            return wave.WarningStage switch
            {
                WaveWarningStage.Announced => GateThreatState.Announced,
                WaveWarningStage.Imminent => GateThreatState.Imminent,
                WaveWarningStage.Critical => GateThreatState.Critical,
                _ => GateThreatState.Observed
            };
        }

        private static string GetStatus(GateThreatState state)
        {
            return state switch
            {
                GateThreatState.Safe => "SEGURO",
                GateThreatState.Observed => "SINAIS DISTANTES",
                GateThreatState.Announced => "ANUNCIADO",
                GateThreatState.Imminent => "IMINENTE",
                GateThreatState.Critical => "CRÍTICO",
                GateThreatState.AttackStarted => "ATAQUE INICIADO",
                _ => string.Empty
            };
        }

        private static string GetOutlook(BattleOutlook outlook)
        {
            return outlook switch
            {
                BattleOutlook.Favorable => "FAVORÁVEL",
                BattleOutlook.Risky => "ARRISCADO",
                BattleOutlook.Unfavorable => "DESFAVORÁVEL",
                _ => "SEM PROGNÓSTICO"
            };
        }
    }
}
