using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Central lookup used by the map and combat scene. Store this asset under
    /// Resources/Exploration/EncounterCatalog so the combat scene can load it.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EncounterCatalog",
        menuName = "TCC/Exploration/Encounter Catalog"
    )]
    public sealed class ExplorationEncounterCatalog : ScriptableObject
    {
        public const string ResourcesPath = "Exploration/EncounterCatalog";

        [SerializeField] private ExplorationEncounterDefinition[] encounters =
            Array.Empty<ExplorationEncounterDefinition>();

        public IReadOnlyList<ExplorationEncounterDefinition> Encounters =>
            encounters;

        public bool TryGet(
            ExplorationEnvironment environment,
            out ExplorationEncounterDefinition encounter
        )
        {
            encounter = encounters?.FirstOrDefault(candidate =>
                candidate != null && candidate.Environment == environment);
            return encounter != null;
        }

        private void OnValidate()
        {
            encounters = (encounters ??
                    Array.Empty<ExplorationEncounterDefinition>())
                .Where(encounter => encounter != null)
                .GroupBy(encounter => encounter.Environment)
                .Select(group => group.First())
                .OrderBy(encounter => (int)encounter.Environment)
                .ToArray();
        }
    }
}
