using System;
using System.Collections.Generic;
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
        [SerializeField] private ExplorationEnemyDefinition[] enemies =
            Array.Empty<ExplorationEnemyDefinition>();
        [SerializeField] private ExplorationRewardEntry[] possibleRewards =
            Array.Empty<ExplorationRewardEntry>();

        public ExplorationEnvironment Environment => environment;
        public Sprite Background => background;
        public int MinimumEnemyCount => minimumEnemyCount;
        public int MaximumEnemyCount => maximumEnemyCount;
        public bool AllowRepeatedEnemies => allowRepeatedEnemies;
        public IReadOnlyList<ExplorationEnemyDefinition> Enemies => enemies;
        public IReadOnlyList<ExplorationRewardEntry> PossibleRewards =>
            possibleRewards;

        /// <summary>
        /// Builds one party from the configured enemy pool. Sampling with
        /// replacement is deliberate: a pool containing only Goblin can produce
        /// Goblin/Goblin/Goblin when Allow Repeated Enemies is enabled.
        /// </summary>
        public List<ExplorationEnemyDefinition> RollEnemyParty(int seed)
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

            var random = new System.Random(seed);
            int allowedMaximum = allowRepeatedEnemies
                ? MaximumTeamSize
                : Math.Min(MaximumTeamSize, pool.Count);
            int minimum = Math.Clamp(minimumEnemyCount, 1, allowedMaximum);
            int maximum = Math.Clamp(maximumEnemyCount, minimum, allowedMaximum);
            int count = random.Next(minimum, maximum + 1);
            var result = new List<ExplorationEnemyDefinition>(count);

            if (allowRepeatedEnemies)
            {
                for (int index = 0; index < count; index++)
                {
                    result.Add(pool[random.Next(0, pool.Count)]);
                }
            }
            else
            {
                while (result.Count < count)
                {
                    int index = random.Next(0, pool.Count);
                    result.Add(pool[index]);
                    pool.RemoveAt(index);
                }
            }

            return result;
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

            possibleRewards ??= Array.Empty<ExplorationRewardEntry>();
        }
    }
}
