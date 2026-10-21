using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TCCGame.Gameplay.Exploration
{
    [Serializable]
    public sealed class ExplorationRewardEntry
    {
        [SerializeField] private ExplorationResourceDefinition resource;
        [SerializeField, Range(0f, 1f)] private float dropChance = 1f;
        [SerializeField, Min(1)] private int minimumQuantity = 1;
        [SerializeField, Min(1)] private int maximumQuantity = 1;

        public ExplorationResourceDefinition Resource => resource;
        public float DropChance => dropChance;
        public int MinimumQuantity => minimumQuantity;
        public int MaximumQuantity => maximumQuantity;

        public int Roll(System.Random random)
        {
            if (resource == null || random == null ||
                random.NextDouble() > dropChance)
            {
                return 0;
            }

            int minimum = Math.Max(1, minimumQuantity);
            int maximum = Math.Max(minimum, maximumQuantity);
            return random.Next(minimum, maximum + 1);
        }
    }

    /// <summary>
    /// Complete visual and gameplay configuration for one map location.
    /// One reusable combat scene reads this asset and changes its background,
    /// enemy party and reward table according to the mission environment.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ExplorationEncounter",
        menuName = "TCC/Exploration/Encounter"
    )]
    public sealed class ExplorationEncounterDefinition : ScriptableObject
    {
        public const int MaximumTeamSize = 3;

        [SerializeField] private ExplorationEnvironment environment;
        [SerializeField] private Sprite background;
        [Header("Enemy party generation")]
        [Tooltip("Minimum number of enemies rolled from the pool below.")]
        [SerializeField, Range(1, MaximumTeamSize)]
        private int minimumEnemyCount = 1;
        [Tooltip("Maximum number of enemies rolled from the pool below.")]
        [SerializeField, Range(1, MaximumTeamSize)]
        private int maximumEnemyCount = MaximumTeamSize;
        [Tooltip("Allows the same enemy definition to be rolled more than once.")]
        [SerializeField] private bool allowRepeatedEnemies = true;
        [Tooltip("Poder total mínimo da equipe inimiga neste local.")]
        [SerializeField, Min(1)] private int minimumEnemyBattlePower = 10;
        [Tooltip("Poder total máximo da equipe inimiga neste local.")]
        [SerializeField, Min(1)] private int maximumEnemyBattlePower = 30;
        [Tooltip("Redução do poder inimigo nas batalhas de defesa. 0.4 significa 40%.")]
        [SerializeField, Range(0f, 1f)] private float defensePowerReduction = 0.4f;
        [SerializeField] private ExplorationEnemyDefinition[] enemies =
            Array.Empty<ExplorationEnemyDefinition>();
        [SerializeField] private ExplorationRewardEntry[] possibleRewards =
            Array.Empty<ExplorationRewardEntry>();

        public ExplorationEnvironment Environment => environment;
        public Sprite Background => background;
        public int MinimumEnemyCount => minimumEnemyCount;
        public int MaximumEnemyCount => maximumEnemyCount;
        public bool AllowRepeatedEnemies => allowRepeatedEnemies;
        public int MinimumEnemyBattlePower => minimumEnemyBattlePower;
        public int MaximumEnemyBattlePower => maximumEnemyBattlePower;
        public float DefensePowerReduction => defensePowerReduction;
        public IReadOnlyList<ExplorationEnemyDefinition> Enemies => enemies;
        public IReadOnlyList<ExplorationRewardEntry> PossibleRewards =>
            possibleRewards;

        /// <summary>
        /// Monta todas as combinações possíveis de até três inimigos e sorteia
        /// somente entre as que respeitam a quantidade e a faixa de poder.
        /// Repetições são combinações válidas quando habilitadas no Inspector.
        /// </summary>
        public List<ExplorationEnemyDefinition> RollEnemyParty(
            int seed,
            float powerMultiplier = 1f
        )
        {
            var pool = new List<ExplorationEnemyDefinition>();
            foreach (ExplorationEnemyDefinition enemy in enemies ??
                     Array.Empty<ExplorationEnemyDefinition>())
            {
                if (enemy != null)
                {
                    pool.Add(enemy);
                }
            }

            if (pool.Count == 0)
            {
                return new List<ExplorationEnemyDefinition>();
            }

            int allowedMaximum = allowRepeatedEnemies
                ? MaximumTeamSize
                : Math.Min(MaximumTeamSize, pool.Count);
            int minimum = Math.Clamp(minimumEnemyCount, 1, allowedMaximum);
            int maximum = Math.Clamp(maximumEnemyCount, minimum, allowedMaximum);
            float normalizedMultiplier = Mathf.Max(0.01f, powerMultiplier);
            int targetMinimumPower = Math.Max(
                1,
                Mathf.RoundToInt(minimumEnemyBattlePower *
                    normalizedMultiplier)
            );
            int targetMaximumPower = Math.Max(
                targetMinimumPower,
                Mathf.RoundToInt(maximumEnemyBattlePower *
                    normalizedMultiplier)
            );

            var everyParty = new List<List<ExplorationEnemyDefinition>>();
            BuildPartyCombinations(
                pool,
                0,
                maximum,
                new List<ExplorationEnemyDefinition>(),
                everyParty
            );

            List<List<ExplorationEnemyDefinition>> validParties = everyParty
                .Where(party => party.Count >= minimum &&
                    PartyPower(party) >= targetMinimumPower &&
                    PartyPower(party) <= targetMaximumPower)
                .ToList();
            var random = new System.Random(seed);

            if (validParties.Count == 0)
            {
                // Uma configuração impossível não deve bloquear o carregamento
                // da batalha. Escolhemos a combinação mais próxima e avisamos o
                // DEV para que ajuste os poderes no Inspector.
                validParties = everyParty
                    .Where(party => party.Count >= minimum)
                    .OrderBy(party => DistanceFromPowerRange(
                        PartyPower(party),
                        targetMinimumPower,
                        targetMaximumPower
                    ))
                    .Take(1)
                    .ToList();
                Debug.LogWarning(
                    $"{name}: nenhuma equipe inimiga atende ao poder " +
                    $"{targetMinimumPower}-{targetMaximumPower}. " +
                    "A combinação mais próxima será usada.",
                    this
                );
            }

            if (validParties.Count == 0)
            {
                return new List<ExplorationEnemyDefinition>();
            }

            var result = new List<ExplorationEnemyDefinition>(
                validParties[random.Next(validParties.Count)]
            );
            // A combinação define quem participa; a ordem visual também pode
            // variar sem alterar o poder final da equipe.
            for (int index = result.Count - 1; index > 0; index--)
            {
                int other = random.Next(index + 1);
                (result[index], result[other]) =
                    (result[other], result[index]);
            }

            return result;
        }

        private void BuildPartyCombinations(
            IReadOnlyList<ExplorationEnemyDefinition> pool,
            int firstAllowedIndex,
            int maximumCount,
            List<ExplorationEnemyDefinition> current,
            List<List<ExplorationEnemyDefinition>> result
        )
        {
            if (current.Count > 0)
            {
                result.Add(new List<ExplorationEnemyDefinition>(current));
            }

            if (current.Count >= maximumCount)
            {
                return;
            }

            for (int index = firstAllowedIndex; index < pool.Count; index++)
            {
                current.Add(pool[index]);
                BuildPartyCombinations(
                    pool,
                    allowRepeatedEnemies ? index : index + 1,
                    maximumCount,
                    current,
                    result
                );
                current.RemoveAt(current.Count - 1);
            }
        }

        private static int PartyPower(
            IEnumerable<ExplorationEnemyDefinition> party
        )
        {
            return party.Sum(enemy => Math.Max(1, enemy.BattlePower));
        }

        private static int DistanceFromPowerRange(
            int power,
            int minimum,
            int maximum
        )
        {
            if (power < minimum)
            {
                return minimum - power;
            }

            return power > maximum ? power - maximum : 0;
        }

        private void OnValidate()
        {
            enemies ??= Array.Empty<ExplorationEnemyDefinition>();
            minimumEnemyCount = Mathf.Clamp(
                minimumEnemyCount,
                1,
                MaximumTeamSize
            );
            maximumEnemyCount = Mathf.Clamp(
                maximumEnemyCount,
                minimumEnemyCount,
                MaximumTeamSize
            );
            minimumEnemyBattlePower = Mathf.Max(1, minimumEnemyBattlePower);
            maximumEnemyBattlePower = Mathf.Max(
                minimumEnemyBattlePower,
                maximumEnemyBattlePower
            );
            defensePowerReduction = Mathf.Clamp01(defensePowerReduction);

            possibleRewards ??= Array.Empty<ExplorationRewardEntry>();
        }
    }
}
