using System;
using TCCGame.Core;

namespace TCCGame.Gameplay.Campaign
{
    public enum BattleOutlook
    {
        Unavailable,
        Favorable,
        Risky,
        Unfavorable
    }

    public readonly struct ThreatForecast
    {
        public double BaseDefense { get; }
        public double EffectiveDefense { get; }
        public double DefenseRatio { get; }
        public BattleOutlook Outlook { get; }
        public string Explanation { get; }

        public ThreatForecast(
            double baseDefense,
            double effectiveDefense,
            double defenseRatio,
            BattleOutlook outlook,
            string explanation
        )
        {
            BaseDefense = baseDefense;
            EffectiveDefense = effectiveDefense;
            DefenseRatio = defenseRatio;
            Outlook = outlook;
            Explanation = explanation ?? string.Empty;
        }
    }

    public sealed class ThreatForecastService
    {
        public const double FavorableThreshold = 1.15d;
        public const double RiskyThreshold = 0.85d;

        private readonly CampaignRules rules;

        public ThreatForecastService(CampaignRules rules)
        {
            this.rules = rules ??
                throw new ArgumentNullException(nameof(rules));
        }

        public double CalculateBaseDefense(GateStateSnapshot gate)
        {
            return gate.Warriors * rules.WarriorPower +
                   gate.Archers * rules.ArcherPower +
                   gate.Mages * rules.MagePower;
        }

        public ThreatForecast Calculate(
            GateStateSnapshot gate,
            WaveSnapshot wave
        )
        {
            double baseDefense = CalculateBaseDefense(gate);
            double raiderThreat = wave.Raiders *
                rules.GetEnemyCost(EnemyType.Raider);
            double bruteThreat = wave.Brutes *
                rules.GetEnemyCost(EnemyType.Brute);
            double shamanThreat = wave.Shamans *
                rules.GetEnemyCost(EnemyType.Shaman);
            double composedThreat = raiderThreat + bruteThreat + shamanThreat;

            if (wave.ThreatPower <= 0 || composedThreat <= 0d)
            {
                return new ThreatForecast(
                    baseDefense,
                    baseDefense,
                    0d,
                    BattleOutlook.Unavailable,
                    "Composição inimiga indisponível."
                );
            }

            double bonus = rules.AdvantageMultiplier - 1d;
            double effectiveDefense =
                gate.Warriors * rules.WarriorPower *
                    (1d + bonus * raiderThreat / composedThreat) +
                gate.Archers * rules.ArcherPower *
                    (1d + bonus * shamanThreat / composedThreat) +
                gate.Mages * rules.MagePower *
                    (1d + bonus * bruteThreat / composedThreat);
            double ratio = effectiveDefense / wave.ThreatPower;

            return new ThreatForecast(
                baseDefense,
                effectiveDefense,
                ratio,
                Classify(ratio),
                BuildAdvantageExplanation(gate, wave)
            );
        }

        public static BattleOutlook Classify(double ratio)
        {
            if (double.IsNaN(ratio) || double.IsInfinity(ratio) || ratio < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(ratio));
            }

            if (ratio >= FavorableThreshold)
            {
                return BattleOutlook.Favorable;
            }

            return ratio >= RiskyThreshold
                ? BattleOutlook.Risky
                : BattleOutlook.Unfavorable;
        }

        public static string BuildAdvantageExplanation(
            GateStateSnapshot gate,
            WaveSnapshot wave
        )
        {
            var advantages = new System.Collections.Generic.List<string>();
            if (gate.Warriors > 0 && wave.Raiders > 0)
            {
                advantages.Add("Guerreiros contra Saqueadores");
            }

            if (gate.Archers > 0 && wave.Shamans > 0)
            {
                advantages.Add("Arqueiros contra Xamãs");
            }

            if (gate.Mages > 0 && wave.Brutes > 0)
            {
                advantages.Add("Magos contra Brutos");
            }

            return advantages.Count == 0
                ? "A guarnição não explora uma vantagem de composição."
                : $"Vantagens: {string.Join(", ", advantages)}.";
        }
    }
}
