using System;
using System.Collections.Generic;
using System.Linq;
using TCCGame.Core.Persistence;

namespace TCCGame.Gameplay.Campaign
{
    public sealed class WaveGenerator
    {
        private readonly CampaignRules rules;

        public WaveGenerator(CampaignRules rules)
        {
            this.rules = rules ??
                throw new ArgumentNullException(nameof(rules));
        }

        public WaveSaveData Generate(
            GameSaveData state,
            int sequence,
            double scheduledFromStrategicSeconds
        )
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (sequence <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sequence));
            }

            if (double.IsNaN(scheduledFromStrategicSeconds) ||
                double.IsInfinity(scheduledFromStrategicSeconds) ||
                scheduledFromStrategicSeconds < 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(scheduledFromStrategicSeconds)
                );
            }

            CampaignRandom random = new CampaignRandom(
                state.campaign.randomSeed,
                sequence
            );
            double delay = GetDelay(sequence, random);
            GateId gateId = SelectGate(state.campaign, random);
            int threatPower = rules.CalculateThreatPower(state, sequence);

            var wave = new WaveSaveData
            {
                sequence = sequence,
                gateId = gateId,
                arrivalStrategicSeconds =
                    scheduledFromStrategicSeconds + delay,
                threatPower = threatPower,
                status = WaveStatus.Scheduled,
                warningStage = WaveWarningStage.None
            };

            FillComposition(wave, random);
            return wave;
        }

        private double GetDelay(int sequence, CampaignRandom random)
        {
            if (sequence == 1)
            {
                return rules.FirstWavePreparationSeconds;
            }

            IntRange interval = rules.GetIntervalRange(sequence);
            return random.NextInt(
                interval.Minimum,
                checked(interval.Maximum + 1)
            );
        }

        private static GateId SelectGate(
            CampaignSaveData campaign,
            CampaignRandom random
        )
        {
            var candidates = Enum.GetValues(typeof(GateId))
                .Cast<GateId>()
                .ToList();
            List<WaveSaveData> previous = campaign.scheduledWaves
                .Where(wave => wave != null)
                .OrderByDescending(wave => wave.sequence)
                .Take(2)
                .ToList();

            if (previous.Count == 2 &&
                previous[0].gateId == previous[1].gateId)
            {
                candidates.Remove(previous[0].gateId);
            }

            return candidates[random.NextInt(0, candidates.Count)];
        }

        private void FillComposition(
            WaveSaveData wave,
            CampaignRandom random
        )
        {
            int remainingBudget = wave.threatPower;

            while (remainingBudget > 0)
            {
                EnemyType enemyType = SelectEnemy(
                    wave.sequence,
                    remainingBudget,
                    random
                );

                switch (enemyType)
                {
                    case EnemyType.Raider:
                        wave.raiders++;
                        break;

                    case EnemyType.Brute:
                        wave.brutes++;
                        break;

                    case EnemyType.Shaman:
                        wave.shamans++;
                        break;
                }

                remainingBudget -= rules.GetEnemyCost(enemyType);
            }
        }

        private EnemyType SelectEnemy(
            int sequence,
            int remainingBudget,
            CampaignRandom random
        )
        {
            var candidates = new List<EnemyType>();
            int totalWeight = 0;

            foreach (EnemyType enemyType in
                     Enum.GetValues(typeof(EnemyType)))
            {
                if (!rules.IsEnemyUnlocked(enemyType, sequence) ||
                    rules.GetEnemyCost(enemyType) > remainingBudget)
                {
                    continue;
                }

                candidates.Add(enemyType);
                totalWeight += rules.GetEnemyWeight(enemyType);
            }

            int roll = random.NextInt(0, totalWeight);

            foreach (EnemyType candidate in candidates)
            {
                int weight = rules.GetEnemyWeight(candidate);

                if (roll < weight)
                {
                    return candidate;
                }

                roll -= weight;
            }

            return EnemyType.Raider;
        }
    }
}
