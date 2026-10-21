using System;
using System.Collections.Generic;
using System.Linq;

namespace TCCGame.Gameplay.Exploration
{
    public enum ExplorationCombatTeam
    {
        Adventurers,
        Enemies
    }

    public sealed class ExplorationBattleCombatant
    {
        public string Id { get; }
        public string DisplayName { get; }
        public ExplorationCombatTeam Team { get; }
        public int MaximumHealth { get; }
        public int CurrentHealth { get; private set; }
        public int MinimumDamage { get; }
        public int MaximumDamage { get; }
        public int Speed { get; }
        public int Armor { get; }
        public int MagicResistance { get; }
        public DamageType DamageType { get; }
        public bool IsAlive => CurrentHealth > 0;

        public ExplorationBattleCombatant(
            string id,
            string displayName,
            ExplorationCombatTeam team,
            int maximumHealth,
            int currentHealth,
            int minimumDamage,
            int maximumDamage,
            int speed,
            int armor,
            int magicResistance,
            DamageType damageType
        )
        {
            Id = id ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? Id
                : displayName;
            Team = team;
            MaximumHealth = Math.Max(1, maximumHealth);
            CurrentHealth = Math.Clamp(
                currentHealth,
                0,
                MaximumHealth
            );
            MinimumDamage = Math.Max(1, minimumDamage);
            MaximumDamage = Math.Max(MinimumDamage, maximumDamage);
            Speed = Math.Max(0, speed);
            Armor = Math.Max(0, armor);
            MagicResistance = Math.Max(0, magicResistance);
            DamageType = damageType;
        }

        public void ApplyDamage(int amount)
        {
            CurrentHealth = Math.Max(0, CurrentHealth - Math.Max(0, amount));
        }
    }

    public readonly struct ExplorationTurnResult
    {
        public ExplorationBattleCombatant Attacker { get; }
        public ExplorationBattleCombatant Target { get; }
        public int RolledDamage { get; }
        public int FinalDamage { get; }
        public bool TargetDefeated { get; }
        public int Round { get; }

        public ExplorationTurnResult(
            ExplorationBattleCombatant attacker,
            ExplorationBattleCombatant target,
            int rolledDamage,
            int finalDamage,
            bool targetDefeated,
            int round
        )
        {
            Attacker = attacker;
            Target = target;
            RolledDamage = rolledDamage;
            FinalDamage = finalDamage;
            TargetDefeated = targetDefeated;
            Round = round;
        }
    }

    /// <summary>
    /// Motor determinístico por iniciativa. Cada pulso soma a velocidade à
    /// barra do combatente; ao alcançar 100 ele age e gasta 100 pontos. Assim,
    /// ao longo do combate, velocidade 20 produz duas ações para cada ação de
    /// velocidade 10. Adventurers always hit the leftmost living enemy; enemies
    /// always hit the rightmost living adventurer.
    /// </summary>
    public sealed class ExplorationTurnBattle
    {
        private readonly List<ExplorationBattleCombatant> adventurers;
        private readonly List<ExplorationBattleCombatant> enemies;
        private readonly System.Random random;
        private const int InitiativeThreshold = 100;
        private readonly Dictionary<ExplorationBattleCombatant, int>
            initiative = new();
        private readonly HashSet<ExplorationBattleCombatant>
            actedInCurrentRound = new();

        public IReadOnlyList<ExplorationBattleCombatant> Adventurers =>
            adventurers;
        public IReadOnlyList<ExplorationBattleCombatant> Enemies => enemies;
        public int Round { get; private set; }
        public bool IsFinished => !adventurers.Any(unit => unit.IsAlive) ||
            !enemies.Any(unit => unit.IsAlive);
        public ExplorationCombatTeam? Winner => !IsFinished
            ? null
            : adventurers.Any(unit => unit.IsAlive)
                ? ExplorationCombatTeam.Adventurers
                : ExplorationCombatTeam.Enemies;

        public ExplorationTurnBattle(
            IEnumerable<ExplorationBattleCombatant> adventurerParty,
            IEnumerable<ExplorationBattleCombatant> enemyParty,
            int randomSeed
        )
        {
            adventurers = ValidateParty(
                adventurerParty,
                ExplorationCombatTeam.Adventurers,
                nameof(adventurerParty)
            );
            enemies = ValidateParty(
                enemyParty,
                ExplorationCombatTeam.Enemies,
                nameof(enemyParty)
            );
            random = new System.Random(randomSeed);
            foreach (ExplorationBattleCombatant combatant in
                     adventurers.Concat(enemies))
            {
                initiative[combatant] = 0;
            }
            Round = 1;
        }

        public ExplorationTurnResult ExecuteNextTurn()
        {
            if (IsFinished)
            {
                throw new InvalidOperationException("The battle is finished.");
            }

            ExplorationBattleCombatant attacker = GetNextAttacker();
            ExplorationBattleCombatant target = attacker.Team ==
                ExplorationCombatTeam.Adventurers
                // Enemy slots are stored from left to right.
                ? enemies.First(unit => unit.IsAlive)
                // Allied slots are stored from left to right, so the last
                // living entry is the rightmost member of the formation.
                : adventurers.Last(unit => unit.IsAlive);
            int rolledDamage = random.Next(
                attacker.MinimumDamage,
                attacker.MaximumDamage + 1
            );
            int finalDamage = ExplorationCombatCalculator.CalculateDamage(
                rolledDamage,
                attacker.DamageType,
                target.Armor,
                target.MagicResistance
            );
            target.ApplyDamage(finalDamage);

            return new ExplorationTurnResult(
                attacker,
                target,
                rolledDamage,
                finalDamage,
                !target.IsAlive,
                Round
            );
        }

        private ExplorationBattleCombatant GetNextAttacker()
        {
            if (AllLivingCombatantsActed())
            {
                actedInCurrentRound.Clear();
                Round++;
            }

            List<ExplorationBattleCombatant> living = adventurers
                .Concat(enemies)
                .Where(unit => unit.IsAlive)
                .ToList();
            while (living.All(unit =>
                       initiative[unit] < InitiativeThreshold))
            {
                foreach (ExplorationBattleCombatant unit in living)
                {
                    // Velocidade zero ainda recebe o mínimo de um ponto para
                    // impedir que uma unidade válida fique travada para sempre.
                    initiative[unit] += Math.Max(1, unit.Speed);
                }
            }

            ExplorationBattleCombatant attacker = living
                .Where(unit => initiative[unit] >= InitiativeThreshold)
                .OrderByDescending(unit => initiative[unit])
                .ThenByDescending(unit => unit.Speed)
                .ThenBy(unit => unit.Team)
                .ThenBy(unit => unit.DisplayName)
                .First();
            initiative[attacker] -= InitiativeThreshold;
            actedInCurrentRound.Add(attacker);
            return attacker;
        }

        private bool AllLivingCombatantsActed()
        {
            return adventurers.Concat(enemies)
                .Where(unit => unit.IsAlive)
                .All(unit => actedInCurrentRound.Contains(unit));
        }

        private static List<ExplorationBattleCombatant> ValidateParty(
            IEnumerable<ExplorationBattleCombatant> party,
            ExplorationCombatTeam expectedTeam,
            string parameterName
        )
        {
            List<ExplorationBattleCombatant> result = party?
                .Where(unit => unit != null)
                .ToList() ?? new List<ExplorationBattleCombatant>();

            if (result.Count < 1 ||
                result.Count > ExplorationEncounterDefinition.MaximumTeamSize)
            {
                throw new ArgumentException(
                    "A combat team must contain between one and three units.",
                    parameterName
                );
            }

            if (result.Any(unit => unit.Team != expectedTeam))
            {
                throw new ArgumentException(
                    "A combatant belongs to the wrong team.",
                    parameterName
                );
            }

            return result;
        }
    }
}
