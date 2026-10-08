using UnityEngine;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Enemy template shared by every encounter that uses this creature.
    /// Keeping combat numbers in an asset allows balancing without code changes.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ExplorationEnemy",
        menuName = "TCC/Exploration/Enemy"
    )]
    public sealed class ExplorationEnemyDefinition : ScriptableObject
    {
        [SerializeField] private string persistenceId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite portrait;

        [Header("Battle visual")]
        [Tooltip("2D prefab instantiated in the combat world. Portrait is used as fallback.")]
        [SerializeField] private GameObject combatPrefab;
        [SerializeField] private Vector3 combatLocalPosition;
        [SerializeField] private Vector3 combatLocalEulerAngles;
        [SerializeField] private Vector3 combatLocalScale = Vector3.one;
        [SerializeField] private bool flipCombatVisualX;

        [Header("Combat stats")]
        [SerializeField, Min(1)] private int maximumHealth = 20;
        [SerializeField, Min(1)] private int minimumDamage = 2;
        [SerializeField, Min(1)] private int maximumDamage = 5;
        [SerializeField, Min(0)] private int speed = 5;
        [SerializeField, Min(0)] private int armor;
        [SerializeField, Min(0)] private int magicResistance;
        [SerializeField] private DamageType damageType = DamageType.Physical;

        public string PersistenceId => string.IsNullOrWhiteSpace(persistenceId)
            ? name
            : persistenceId.Trim();
        public string DisplayName => string.IsNullOrWhiteSpace(displayName)
            ? name
            : displayName.Trim();
        public Sprite Portrait => portrait;
        public GameObject CombatPrefab => combatPrefab;
        public Vector3 CombatLocalPosition => combatLocalPosition;
        public Vector3 CombatLocalEulerAngles => combatLocalEulerAngles;
        public Vector3 CombatLocalScale => combatLocalScale;
        public bool FlipCombatVisualX => flipCombatVisualX;
        public int MaximumHealth => maximumHealth;
        public int MinimumDamage => minimumDamage;
        public int MaximumDamage => maximumDamage;
        public int Speed => speed;
        public int Armor => armor;
        public int MagicResistance => magicResistance;
        public DamageType DamageType => damageType;

        private void OnValidate()
        {
            maximumHealth = Mathf.Max(1, maximumHealth);
            minimumDamage = Mathf.Max(1, minimumDamage);
            maximumDamage = Mathf.Max(minimumDamage, maximumDamage);
            speed = Mathf.Max(0, speed);
            armor = Mathf.Max(0, armor);
            magicResistance = Mathf.Max(0, magicResistance);
            if (combatLocalScale == Vector3.zero)
            {
                combatLocalScale = Vector3.one;
            }
        }
    }
}
