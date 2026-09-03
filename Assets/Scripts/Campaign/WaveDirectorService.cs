using System;
using System.Collections.Generic;
using System.Linq;
using TCCGame.Core.Persistence;

namespace TCCGame.Gameplay.Campaign
{
    internal readonly struct WaveResolutionChange
    {
        public WaveSnapshot? ScheduledWave { get; }
        public IReadOnlyList<WaveSnapshot> WarningSnapshots { get; }

        public WaveResolutionChange(
            WaveSnapshot? scheduledWave,
            IReadOnlyList<WaveSnapshot> warningSnapshots
        )
        {
            ScheduledWave = scheduledWave;
            WarningSnapshots = warningSnapshots;
        }
    }

    public sealed class WaveDirectorService
    {
        private readonly GameSaveData state;
        private readonly CampaignRules rules;
        private readonly WaveGenerator generator;
        private readonly Action persistSemanticChange;

        public event Action<WaveSnapshot> WaveScheduled;
        public event Action<WaveSnapshot, WaveWarningStage>
            WaveWarningChanged;
        public event Action<WaveSnapshot> WaveBecameDue;

        public WaveDirectorService(
            GameSaveData state,
            CampaignRules rules,
            Action persistSemanticChange
        )
        {
            this.state = state ??
                throw new ArgumentNullException(nameof(state));
            this.rules = rules ??
                throw new ArgumentNullException(nameof(rules));
            this.persistSemanticChange = persistSemanticChange ??
                throw new ArgumentNullException(
                    nameof(persistSemanticChange)
                );
            generator = new WaveGenerator(rules);
        }

        public WaveSnapshot? CurrentWave
        {
            get
            {
                WaveSaveData wave = FindCurrentWave();
                return wave == null ? null : wave.ToSnapshot();
            }
        }

        public bool EnsureWaveScheduled()
        {
            if (FindCurrentWave() != null || !CanScheduleNextWave())
            {
                return false;
            }

            ScheduleNextWave(publishEvents: false);
            return true;
        }

        internal void Advance()
        {
            WaveSaveData wave = FindCurrentWave();

            if (wave == null)
            {
                if (CanScheduleNextWave())
                {
                    ScheduleNextWave(publishEvents: true);
                }

                return;
            }

            if (wave.status != WaveStatus.Scheduled)
            {
                return;
            }

            double remaining = wave.arrivalStrategicSeconds -
                state.campaign.elapsedStrategicSeconds;
            WaveWarningStage targetStage = GetTargetWarningStage(remaining);
            List<WaveSnapshot> warningSnapshots =
                AdvanceWarningStages(wave, targetStage);
            bool becameDue = remaining <= 0d;

            if (becameDue)
            {
                wave.status = WaveStatus.AwaitingResolution;
            }

            if (warningSnapshots.Count == 0 && !becameDue)
            {
                return;
            }

            persistSemanticChange();

            foreach (WaveSnapshot snapshot in warningSnapshots)
            {
                WaveWarningChanged?.Invoke(
                    snapshot,
                    snapshot.WarningStage
                );
            }

            if (becameDue)
            {
                WaveBecameDue?.Invoke(wave.ToSnapshot());
            }
        }

        public bool MarkResolved(int sequence)
        {
            if (!PrepareResolution(sequence, out WaveResolutionChange change))
            {
                return false;
            }

            persistSemanticChange();
            PublishResolution(change);
            return true;
        }

        internal bool PrepareResolution(
            int sequence,
            out WaveResolutionChange change
        )
        {
            change = default;
            WaveSaveData wave = state.campaign.scheduledWaves.Find(
                candidate => candidate != null &&
                    candidate.sequence == sequence &&
                    candidate.status == WaveStatus.AwaitingResolution
            );

            WaveSaveData queuedWave = state.campaign.scheduledWaves
                .Where(candidate => candidate != null &&
                    candidate != wave &&
                    candidate.status != WaveStatus.Resolved)
                .OrderBy(candidate => candidate.sequence)
                .FirstOrDefault();

            if (wave == null)
            {
                return false;
            }

            wave.status = WaveStatus.Resolved;
            GateSaveData gate = state.campaign.gates.Find(
                candidate => candidate != null &&
                    candidate.gateId == wave.gateId
            );

            if (gate != null)
            {
                gate.lastResolvedWaveSequence = Math.Max(
                    gate.lastResolvedWaveSequence,
                    sequence
                );
            }

            bool scheduledNewWave = queuedWave == null &&
                CanScheduleNextWave();
            WaveSaveData nextWave = scheduledNewWave
                ? ScheduleNextWave(publishEvents: false)
                : queuedWave;
            List<WaveSnapshot> warningSnapshots = nextWave == null
                ? new List<WaveSnapshot>()
                : AdvanceWarningStages(
                    nextWave,
                    GetTargetWarningStage(
                        nextWave.arrivalStrategicSeconds -
                        state.campaign.elapsedStrategicSeconds
                    )
                );
            change = new WaveResolutionChange(
                scheduledNewWave ? nextWave.ToSnapshot() : null,
                warningSnapshots
            );
            return true;
        }

        internal void PublishResolution(WaveResolutionChange change)
        {
            if (change.ScheduledWave.HasValue)
            {
                PublishSafely(WaveScheduled, change.ScheduledWave.Value);
            }

            foreach (WaveSnapshot snapshot in change.WarningSnapshots)
            {
                PublishSafely(WaveWarningChanged,
                    snapshot,
                    snapshot.WarningStage
                );
            }

        }

        private static void PublishSafely(
            Action<WaveSnapshot> handlers,
            WaveSnapshot value
        )
        {
            if (handlers == null)
            {
                return;
            }

            foreach (Action<WaveSnapshot> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(value);
                }
                catch (Exception)
                {
                    // Presentation observers must not interrupt campaign state updates.
                }
            }
        }

        private static void PublishSafely(
            Action<WaveSnapshot, WaveWarningStage> handlers,
            WaveSnapshot wave,
            WaveWarningStage stage
        )
        {
            if (handlers == null)
            {
                return;
            }

            foreach (Action<WaveSnapshot, WaveWarningStage> handler in
                     handlers.GetInvocationList())
            {
                try
                {
                    handler(wave, stage);
                }
                catch (Exception)
                {
                    // Presentation observers must not interrupt campaign state updates.
                }
            }
        }

        private WaveSaveData ScheduleNextWave(bool publishEvents)
        {
            int sequence = state.campaign.nextWaveSequence;

            if (sequence <= 0 || sequence == int.MaxValue)
            {
                throw new InvalidOperationException(
                    "The campaign cannot schedule another wave."
                );
            }

            WaveSaveData wave = generator.Generate(
                state,
                sequence,
                state.campaign.elapsedStrategicSeconds
            );
            state.campaign.scheduledWaves.Add(wave);
            state.campaign.nextWaveSequence = checked(sequence + 1);

            if (publishEvents)
            {
                WaveWarningStage target = GetTargetWarningStage(
                    wave.arrivalStrategicSeconds -
                    state.campaign.elapsedStrategicSeconds
                );
                List<WaveSnapshot> warnings = AdvanceWarningStages(
                    wave,
                    target
                );
                persistSemanticChange();
                WaveScheduled?.Invoke(wave.ToSnapshot());

                foreach (WaveSnapshot snapshot in warnings)
                {
                    WaveWarningChanged?.Invoke(
                        snapshot,
                        snapshot.WarningStage
                    );
                }
            }

            return wave;
        }

        private bool CanScheduleNextWave()
        {
            int sequence = state.campaign.nextWaveSequence;
            return state.campaign.outcome == CampaignOutcome.Ongoing &&
                   sequence > 0 &&
                   sequence <= CampaignRules.MaximumWaves;
        }

        private WaveSaveData FindCurrentWave()
        {
            return state.campaign.scheduledWaves
                .Where(wave => wave != null &&
                    wave.status != WaveStatus.Resolved)
                .OrderBy(wave => wave.sequence)
                .FirstOrDefault();
        }

        private WaveWarningStage GetTargetWarningStage(double remaining)
        {
            if (remaining <= rules.CriticalThresholdSeconds)
            {
                return WaveWarningStage.Critical;
            }

            if (remaining <= rules.ImminentThresholdSeconds)
            {
                return WaveWarningStage.Imminent;
            }

            if (remaining <= rules.AnnouncedThresholdSeconds)
            {
                return WaveWarningStage.Announced;
            }

            return WaveWarningStage.None;
        }

        private static List<WaveSnapshot> AdvanceWarningStages(
            WaveSaveData wave,
            WaveWarningStage target
        )
        {
            var snapshots = new List<WaveSnapshot>();

            for (int value = (int)wave.warningStage + 1;
                 value <= (int)target;
                 value++)
            {
                wave.warningStage = (WaveWarningStage)value;
                snapshots.Add(wave.ToSnapshot());
            }

            return snapshots;
        }
    }
}
