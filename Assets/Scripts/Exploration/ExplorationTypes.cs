using System;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Every battle destination visible on the world map. Explicit numeric values
    /// protect existing save files because the first three environments already
    /// existed before the remaining destinations were introduced.
    /// Porto Azul is the player's hub and therefore is not a battle destination.
    /// </summary>
    public enum ExplorationEnvironment
    {
        ColinasDoVento = 0,
        FlorestaDasSombras = 1,
        CavernaDeGelo = 2,
        TorreAntiga = 3,
        VilaDoMar = 4,
        FlorestaEsmeralda = 5,
        MontanhasGelidas = 6,
        ReinoReal = 7,
        IlhaVerde = 8,
        CavernaSombria = 9,
        MontanhaDeFogo = 10,
        DesertoDosVentos = 11,
        Santuario = 12,
        IlhasCoral = 13
    }

    /// <summary>Persistent lifecycle of a party sent to an environment.</summary>
    public enum ExplorationMissionStatus
    {
        Travelling = 0,
        ReadyForBattle = 1,
        InBattle = 2,
        Won = 3,
        Retreated = 4,
        Defeated = 5,
        Returning = 6,
        Returned = 7
    }

    public enum ExplorationMissionKind
    {
        Capture = 0,
        Defense = 1
    }

    [Serializable]
    public sealed class ExplorationRewardSaveData
    {
        public string resourceId;
        public string displayName;
        public int quantity;
    }

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
        public ExplorationMissionKind kind;
        public ExplorationMissionStatus status;
        public long arrivalUnixSeconds;
        public bool arrivalAnnounced;
        public long returnUnixSeconds;
        public bool returnAnnounced;
        public bool rewardsClaimed;
        public bool reportAcknowledged;
        public System.Collections.Generic.List<ExplorationRewardSaveData>
            rewards = new();
        public System.Collections.Generic.List<ExplorationCombatantSaveData> party = new();
    }

    [Serializable]
    public sealed class ExplorationAreaSaveData
    {
        public ExplorationEnvironment environment;
        public bool captured;
        public int assignedWorkers;
        public long nextWorkerCollectionUnixSeconds;
        public System.Collections.Generic.List<ExplorationRewardSaveData>
            lastWorkerLoot = new();
        // Loot is intentionally kept outside of the inventory until the player
        // transports it from the tavern action board.
        public System.Collections.Generic.List<ExplorationRewardSaveData>
            pendingWorkerLoot = new();
        public long nextEnemyAttackUnixSeconds;
        public bool enemyAttackPending;
        public System.Collections.Generic.List<ExplorationCombatantSaveData>
            guardParty = new();
    }

    [Serializable]
    public sealed class ExplorationSaveData
    {
        public int nextMissionId = 1;
        public System.Collections.Generic.List<ExplorationMissionSaveData> missions = new();
        public System.Collections.Generic.List<ExplorationEnvironment>
            completedEnvironments = new();
        public System.Collections.Generic.List<ExplorationAreaSaveData> areas =
            new();
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
