using System;
using System.Collections.Generic;
using System.Linq;
using TCCGame.Core.Persistence;

namespace TCCGame.Gameplay.Campaign
{
    public sealed class CampaignService
    {
        private readonly GameSaveData state;
        private readonly CampaignRules rules;
        private readonly Action commit;
        private readonly HashSet<object> pauseOwners = new();

        private bool dirty;
        private double lastCheckpointStrategicSeconds;

        public double ElapsedStrategicSeconds =>
            state.campaign.elapsedStrategicSeconds;
        public bool IsPaused => pauseOwners.Count > 0;
        public CampaignOutcome Outcome => state.campaign.outcome;
        public WaveDirectorService Waves { get; }

        public event Action<bool> PauseChanged;

        public CampaignService(
            GameSaveData state,
            CampaignRules rules,
            Action commit
        )
        {
            this.state = state ??
                throw new ArgumentNullException(nameof(state));
            this.rules = rules ??
                throw new ArgumentNullException(nameof(rules));
            this.commit = commit ??
                throw new ArgumentNullException(nameof(commit));

            dirty = CampaignSaveDataUtility.Normalize(state);
            Waves = new WaveDirectorService(
                state,
                rules,
                PersistSemanticChange
            );
            dirty |= Waves.EnsureWaveScheduled();
            lastCheckpointStrategicSeconds = ElapsedStrategicSeconds;
        }

        public IReadOnlyList<GateStateSnapshot> Gates =>
            state.campaign.gates
                .Select(gate => gate.ToSnapshot())
                .ToArray();

        public GateStateSnapshot GetGate(GateId gateId)
        {
            GateSaveData gate = state.campaign.gates.Find(
                candidate => candidate.gateId == gateId
            );

            if (gate == null)
            {
                throw new ArgumentOutOfRangeException(nameof(gateId));
            }

            return gate.ToSnapshot();
        }

        public BattleReportSnapshot? GetLatestBattleReport(
            GateId gateId,
            bool onlyUnacknowledged = false
        )
        {
            WaveSaveData wave = state.campaign.scheduledWaves
                .Where(candidate => candidate != null &&
                    candidate.gateId == gateId &&
                    candidate.battleReport != null &&
                    candidate.battleReport.resolved &&
                    (!onlyUnacknowledged ||
                     !candidate.battleReport.reportAcknowledged))
                .OrderByDescending(candidate => candidate.sequence)
                .FirstOrDefault();
            return wave == null
                ? null
                : wave.battleReport.ToSnapshot(wave);
        }

        public WaveSnapshot? GetWave(int sequence)
        {
            WaveSaveData wave = state.campaign.scheduledWaves.Find(
                candidate => candidate != null &&
                    candidate.sequence == sequence
            );
            return wave == null ? null : wave.ToSnapshot();
        }

        public void Advance(double deltaSeconds)
        {
            if (!AdvanceClock(deltaSeconds))
            {
                return;
            }

            ProcessWavesAndCheckpoint();
        }

        internal bool AdvanceClock(double deltaSeconds)
        {
            if (double.IsNaN(deltaSeconds) ||
                double.IsInfinity(deltaSeconds) ||
                deltaSeconds < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            if (deltaSeconds == 0d || IsPaused ||
                Outcome != CampaignOutcome.Ongoing)
            {
                return false;
            }

            state.campaign.elapsedStrategicSeconds += deltaSeconds;
            dirty = true;
            return true;
        }

        internal void ProcessWavesAndCheckpoint()
        {
            Waves.Advance();

            if (dirty &&
                ElapsedStrategicSeconds -
                lastCheckpointStrategicSeconds >=
                rules.CheckpointIntervalSeconds)
            {
                CommitCheckpoint();
            }
        }

        public bool AcquirePause(object owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (!pauseOwners.Add(owner))
            {
                return false;
            }

            if (pauseOwners.Count == 1)
            {
                PauseChanged?.Invoke(true);
            }

            return true;
        }

        public bool ReleasePause(object owner)
        {
            if (owner == null || !pauseOwners.Remove(owner))
            {
                return false;
            }

            if (pauseOwners.Count == 0)
            {
                PauseChanged?.Invoke(false);
            }

            return true;
        }

        public bool MarkWaveResolved(int sequence)
        {
            return Waves.MarkResolved(sequence);
        }

        public void Checkpoint()
        {
            if (dirty)
            {
                CommitCheckpoint();
            }
        }

        private void PersistSemanticChange()
        {
            dirty = true;
            CommitCheckpoint();
        }

        private void CommitCheckpoint()
        {
            commit();
            dirty = false;
            lastCheckpointStrategicSeconds = ElapsedStrategicSeconds;
        }
    }
}
