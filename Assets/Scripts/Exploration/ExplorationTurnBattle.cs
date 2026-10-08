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
    /// Deterministic turn engine. At the beginning of every round all living
    /// combatants are ordered by speed (highest first). Adventurers always hit
    /// the leftmost living enemy; enemies always hit the rightmost living
    /// adventurer. This preserves the visual front-line formation.
    /// </summary>
    public sealed class ExplorationTurnBattle
    {
        private readonly List<ExplorationBattleCombatant> adventurers;
        private readonly List<ExplorationBattleCombatant> enemies;
        private readonly System.Random random;
        private Queue<ExplorationBattleCombatant> turnQueue = new();

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
            while (true)
            {
                if (turnQueue.Count == 0)
                {
                    Round++;
                    turnQueue = new Queue<ExplorationBattleCombatant>(
                        adventurers.Concat(enemies)
                            .Where(unit => unit.IsAlive)
                            .OrderByDescending(unit => unit.Speed)
                            .ThenBy(unit => unit.Team)
                            .ThenBy(unit => unit.DisplayName)
                    );
                }

                ExplorationBattleCombatant attacker = turnQueue.Dequeue();
                if (attacker.IsAlive)
                {
                    return attacker;
                }
            }
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
