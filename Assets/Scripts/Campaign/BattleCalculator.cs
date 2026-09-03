using System;
using System.Collections.Generic;
using TCCGame.Core;

namespace TCCGame.Gameplay.Campaign
{
    public sealed class BattleCalculator
    {
        private readonly CampaignRules rules;
        private readonly ThreatForecastService forecast;

        public BattleCalculator(CampaignRules rules)
        {
            this.rules = rules ??
                throw new ArgumentNullException(nameof(rules));
            forecast = new ThreatForecastService(rules);
        }

        public BattleResolution Calculate(
            GateStateSnapshot gate,
            WaveSnapshot wave
        )
        {
            ThreatForecast threatForecast = forecast.Calculate(gate, wave);
            double threat = Math.Max(1d, wave.ThreatPower);
            double pressure = threat /
                Math.Max(1d, threatForecast.EffectiveDefense);
            double casualtyRate = Math.Clamp(
                rules.MinimumCasualtyRate +
                Math.Max(
                    0d,
                    pressure - rules.CasualtyPressureThreshold
                ) * rules.CasualtyPressureScale,
                rules.MinimumCasualtyRate,
                rules.MaximumCasualtyRate
            );

            long totalSoldiers = (long)gate.Warriors +
                gate.Archers + gate.Mages;
            long totalLosses = totalSoldiers == 0
                ? 0
                : Math.Clamp(
                    Math.Max(
                        1L,
                        (long)Math.Round(
                            totalSoldiers * casualtyRate,
                            MidpointRounding.AwayFromZero
                        )
                    ),
                    0,
                    totalSoldiers
                );
            int[] losses = AllocateLosses(
                new long[] { gate.Warriors, gate.Archers, gate.Mages },
                totalLosses
            );

            int gateDamage = 0;
            if (threatForecast.DefenseRatio < 1d && gate.CurrentHealth > 0)
            {
                double rawDamage = (1d - threatForecast.DefenseRatio) *
                    (rules.GateDamageBase +
                     rules.GateDamageThreatScale * threat);
                gateDamage = Math.Clamp(
                    (int)Math.Ceiling(rawDamage),
                    1,
                    gate.CurrentHealth
                );
            }

            int finalHealth = gate.CurrentHealth - gateDamage;
            BattleOutcome outcome = finalHealth == 0
                ? BattleOutcome.Breached
                : BattleOutcome.Repelled;
            int spoils = outcome == BattleOutcome.Repelled
                ? Math.Max(
                    1,
                    (int)Math.Floor(threat * rules.SpoilsThreatScale)
                )
                : 0;

            return new BattleResolution(
                outcome,
                threatForecast.EffectiveDefense,
                threatForecast.DefenseRatio,
                gate.CurrentHealth,
                gateDamage,
                finalHealth,
                gate.Warriors,
                losses[0],
                gate.Archers,
                losses[1],
                gate.Mages,
                losses[2],
                spoils
            );
        }

        private static int[] AllocateLosses(long[] counts, long totalLosses)
        {
            var losses = new int[counts.Length];
            long total = 0;
            foreach (long count in counts)
            {
                total += count;
            }

            if (total == 0 || totalLosses == 0)
            {
                return losses;
            }

            var remainders = new List<(int Index, double Remainder)>();
            long allocated = 0;
            for (int index = 0; index < counts.Length; index++)
            {
                double exact = totalLosses * counts[index] / (double)total;
                losses[index] = (int)Math.Min(
                    counts[index],
                    (long)Math.Floor(exact)
                );
                allocated += losses[index];
                remainders.Add((index, exact - Math.Floor(exact)));
            }

            remainders.Sort((left, right) =>
            {
                int byRemainder = right.Remainder.CompareTo(left.Remainder);
                return byRemainder != 0
                    ? byRemainder
                    : left.Index.CompareTo(right.Index);
            });

            int cursor = 0;
            while (allocated < totalLosses)
            {
                int index = remainders[cursor % remainders.Count].Index;
                cursor++;
                if (losses[index] >= counts[index])
                {
                    continue;
                }

                losses[index]++;
                allocated++;
            }

            return losses;
        }
    }
}
