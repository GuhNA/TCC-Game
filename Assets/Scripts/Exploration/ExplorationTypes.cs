using System;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>Initial locations available from the exploration map.</summary>
    public enum ExplorationEnvironment { Forest, Ruins, Cavern }

    /// <summary>Persistent lifecycle of a party sent to an environment.</summary>
    public enum ExplorationMissionStatus { Travelling, ReadyForBattle, InBattle, Won, Retreated }

    [Serializable]
    public sealed class ExplorationCombatantSaveData
    {
        public string adventurerId;
        public string displayName;
        public int maxHealth;
        public int currentHealth;
        public int minimumDamage;
        public int maximumDamage;
        public int speed;
        public int armor;
        public int magicResistance;
        public DamageType damageType;
    }

    [Serializable]
    public sealed class ExplorationMissionSaveData
    {
        public int id;
        public ExplorationEnvironment environment;
        public ExplorationMissionStatus status;
        public long arrivalUnixSeconds;
        public bool arrivalAnnounced;
        public System.Collections.Generic.List<ExplorationCombatantSaveData> party = new();
    }

    [Serializable]
    public sealed class ExplorationSaveData
    {
        public int nextMissionId = 1;
        public System.Collections.Generic.List<ExplorationMissionSaveData> missions = new();
    }

    /// <summary>
    /// Reusable combat math. Damage after defense is always at least one.
    /// Formula: finalDamage = max(1, rolledDamage - relevantDefense).
    /// Physical attacks use armor; magical attacks use magic resistance.
    /// </summary>
    public static class ExplorationCombatCalculator
    {
        public static int CalculateDamage(
            int rolledDamage,
            DamageType damageType,
            int armor,
            int magicResistance
        )
        {
            int defense = damageType == DamageType.Magical
                ? magicResistance
                : armor;
            return Math.Max(1, rolledDamage - Math.Max(0, defense));
        }
    }
}
