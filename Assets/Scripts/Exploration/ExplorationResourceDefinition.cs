using UnityEngine;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Resource stored in the player's inventory after an exploration victory.
    /// The same identifier can later be consumed by crafting or city upgrades.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ExplorationResource",
        menuName = "TCC/Exploration/Resource"
    )]
    public sealed class ExplorationResourceDefinition : ScriptableObject
    {
        [SerializeField] private string persistenceId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;

        public string PersistenceId => string.IsNullOrWhiteSpace(persistenceId)
            ? name
            : persistenceId.Trim();
        public string DisplayName => string.IsNullOrWhiteSpace(displayName)
            ? name
            : displayName.Trim();
        public string Description => description;
        public Sprite Icon => icon;
    }
}
