using System;
using System.Collections.Generic;
using System.Linq;
using TCCGame.Core.Persistence;
using TCCGame.Gameplay.Economy;

namespace TCCGame.Gameplay.Campaign
{
    public sealed class BattleResolutionService
    {
        private readonly GameSaveData state;
        private readonly WaveDirectorService waves;
        private readonly EconomyService economy;
        private readonly Action commit;
        private readonly BattleCalculator calculator;

        public event Action<BattleReportSnapshot> BattleResolved;
        public event Action BattleReportsChanged;

        public BattleResolutionService(
            GameSaveData state,
            CampaignRules rules,
            WaveDirectorService waves,
            EconomyService economy,
            Action commit
        )
        {
            this.state = state ??
                throw new ArgumentNullException(nameof(state));
            this.waves = waves ??
                throw new ArgumentNullException(nameof(waves));
            this.economy = economy ??
                throw new ArgumentNullException(nameof(economy));
            this.commit = commit ??
                throw new ArgumentNullException(nameof(commit));
            calculator = new BattleCalculator(rules);
        }

        public bool TryResolvePending()
        {
            WaveSaveData wave = state.campaign.scheduledWaves
                .Where(candidate => candidate != null &&
                    candidate.status == WaveStatus.AwaitingResolution)
                .OrderBy(candidate => candidate.sequence)
                .FirstOrDefault();
            if (wave == null ||
                wave.battleReport != null && wave.battleReport.resolved)
            {
                return false;
            }

            GateSaveData gate = state.campaign.gates.Find(
                candidate => candidate != null &&
                    candidate.gateId == wave.gateId
            );
            if (gate == null)
            {
                return false;
            }

            BattleResolution result = calculator.Calculate(
                gate.ToSnapshot(),
                wave.ToSnapshot()
            );
            int previousMoney = state.money;
            CampaignOutcome previousOutcome = state.campaign.outcome;
            int previousNextSequence = state.campaign.nextWaveSequence;
            List<WaveSaveData> previousWaves = state.campaign.scheduledWaves
                .Select(CloneWave)
                .ToList();
            GateSaveData previousGate = CloneGate(gate);

            BattleReportSnapshot report;
            WaveResolutionChange waveChange;
            try
            {
                ApplyResult(gate, wave, result);
                state.money = checked(state.money + result.Spoils);
                state.campaign.outcome = GetCampaignOutcome(
                    wave.sequence,
                    result.Outcome
                );

                if (!waves.PrepareResolution(
                    wave.sequence,
                    out waveChange
                ))
                {
                    throw new InvalidOperationException(
                        "The pending wave could not be completed."
                    );
                }

                report = wave.battleReport.ToSnapshot(wave);
                commit();
            }
            catch
            {
                state.money = previousMoney;
                state.campaign.outcome = previousOutcome;
                state.campaign.nextWaveSequence = previousNextSequence;
                state.campaign.scheduledWaves = previousWaves;
                RestoreGate(gate.gateId, previousGate);
                throw;
            }

            // The transaction ends at commit. Presentation observers cannot
            // roll back an already persisted battle.
            economy.PublishChange();
            waves.PublishResolution(waveChange);
            PublishSafely(BattleResolved, report);
            PublishSafely(BattleReportsChanged);
            return true;
        }

        public bool AcknowledgeReport(int waveSequence)
        {
            WaveSaveData wave = state.campaign.scheduledWaves.Find(
                candidate => candidate != null &&
                    candidate.sequence == waveSequence &&
                    candidate.battleReport != null &&
                    candidate.battleReport.resolved
            );
            if (wave == null || wave.battleReport.reportAcknowledged)
            {
                return false;
            }

            wave.battleReport.reportAcknowledged = true;
            try
            {
                commit();
            }
            catch
            {
                wave.battleReport.reportAcknowledged = false;
                throw;
            }

            PublishSafely(BattleReportsChanged);
            return true;
        }

        private static void PublishSafely<T>(
            Action<T> handlers,
            T value
        )
        {
            if (handlers == null)
            {
                return;
            }

            foreach (Action<T> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(value);
                }
                catch (Exception)
                {
                    // Presentation observers are isolated from the committed transaction.
                }
            }
        }

        private static void PublishSafely(Action handlers)
        {
            if (handlers == null)
            {
                return;
            }

            foreach (Action handler in handlers.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception)
                {
                    // Presentation observers are isolated from the committed transaction.
                }
            }
        }

        private CampaignOutcome GetCampaignOutcome(
            int sequence,
            BattleOutcome outcome
        )
        {
            if (outcome == BattleOutcome.Breached)
            {
                return CampaignOutcome.Defeat;
            }

            return sequence >= CampaignRules.MaximumWaves
                ? CampaignOutcome.Victory
                : CampaignOutcome.Ongoing;
        }

        private static void ApplyResult(
            GateSaveData gate,
            WaveSaveData wave,
            BattleResolution result
        )
        {
            gate.currentHealth = result.FinalGateHealth;
            gate.warriors = result.InitialWarriors - result.LostWarriors;
            gate.archers = result.InitialArchers - result.LostArchers;
            gate.mages = result.InitialMages - result.LostMages;
            wave.battleReport = new BattleReportSaveData
            {
                resolved = true,
                outcome = result.Outcome,
                effectiveDefense = result.EffectiveDefense,
                defenseRatio = result.DefenseRatio,
                initialGateHealth = result.InitialGateHealth,
                gateDamage = result.GateDamage,
                finalGateHealth = result.FinalGateHealth,
                initialWarriors = result.InitialWarriors,
                lostWarriors = result.LostWarriors,
                initialArchers = result.InitialArchers,
                lostArchers = result.LostArchers,
                initialMages = result.InitialMages,
                lostMages = result.LostMages,
                spoils = result.Spoils,
                reportAcknowledged = false
            };
        }

        private static WaveSaveData CloneWave(WaveSaveData source)
        {
            return new WaveSaveData
            {
                sequence = source.sequence,
                gateId = source.gateId,
                arrivalStrategicSeconds = source.arrivalStrategicSeconds,
                threatPower = source.threatPower,
                status = source.status,
                raiders = source.raiders,
                brutes = source.brutes,
                shamans = source.shamans,
                warningStage = source.warningStage,
                battleReport = CloneReport(source.battleReport)
            };
        }

        private static BattleReportSaveData CloneReport(
            BattleReportSaveData source
        )
        {
            if (source == null)
            {
                return new BattleReportSaveData();
            }

            return new BattleReportSaveData
            {
                resolved = source.resolved,
                outcome = source.outcome,
                effectiveDefense = source.effectiveDefense,
                defenseRatio = source.defenseRatio,
                initialGateHealth = source.initialGateHealth,
                gateDamage = source.gateDamage,
                finalGateHealth = source.finalGateHealth,
                initialWarriors = source.initialWarriors,
                lostWarriors = source.lostWarriors,
                initialArchers = source.initialArchers,
                lostArchers = source.lostArchers,
                initialMages = source.initialMages,
                lostMages = source.lostMages,
                spoils = source.spoils,
                reportAcknowledged = source.reportAcknowledged
            };
        }

        private static GateSaveData CloneGate(GateSaveData source)
        {
            return new GateSaveData
            {
                gateId = source.gateId,
                currentHealth = source.currentHealth,
                warriors = source.warriors,
                archers = source.archers,
                mages = source.mages,
                lastResolvedWaveSequence = source.lastResolvedWaveSequence
            };
        }

        private void RestoreGate(GateId gateId, GateSaveData source)
        {
            GateSaveData target = state.campaign.gates.Find(
                candidate => candidate.gateId == gateId
            );
            if (target == null)
            {
                state.campaign.gates.Add(source);
                return;
            }

            target.currentHealth = source.currentHealth;
            target.warriors = source.warriors;
            target.archers = source.archers;
            target.mages = source.mages;
            target.lastResolvedWaveSequence =
                source.lastResolvedWaveSequence;
        }
    }
}
