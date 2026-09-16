using System;

public readonly struct AdventurerStats
{
    public ushort MaxHealth { get; }
    public ushort MinimumDamage { get; }
    public ushort MaximumDamage { get; }
    public byte Speed { get; }
    public DamageType DamageType { get; }

    public AdventurerStats(
        ushort maxHealth,
        ushort minimumDamage,
        ushort maximumDamage,
        byte speed,
        DamageType damageType
    )
    {
        MaxHealth = maxHealth;
        MinimumDamage = minimumDamage;
        MaximumDamage = maximumDamage;
        Speed = speed;
        DamageType = damageType;
    }
}

public static class AdventurerStatsCalculator
{
    public static AdventurerStats Calculate(Char_base definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        int maxHealth = definition.vida + GetEquipmentHealth(definition);
        int minimumDamage = CalculateDamage(
            definition.destreza,
            definition.inteligencia,
            definition.forca,
            definition.escalaDexMin,
            definition.escalaIntMin,
            definition.escalaForMin
        );
        int maximumDamage = 4 + CalculateDamage(
            definition.destreza,
            definition.inteligencia,
            definition.forca,
            definition.escalaDexMax,
            definition.escalaIntMax,
            definition.escalaForMax
        );

        return new AdventurerStats(
            ToUShort(maxHealth),
            ToUShort(minimumDamage),
            ToUShort(maximumDamage),
            definition.velocidade,
            definition.tipoDano
        );
    }

    private static int GetEquipmentHealth(Char_base definition)
    {
        int health = 0;

        if (definition.mao != null)
        {
            health += definition.mao.hp;
        }

        if (definition.torso != null)
        {
            health += definition.torso.hp;
        }

        if (definition.acessorio != null)
        {
            health += definition.acessorio.hp;
        }

        return health;
    }

    private static int CalculateDamage(
        byte dexterity,
        byte intelligence,
        byte strength,
        float dexterityScale,
        float intelligenceScale,
        float strengthScale
    )
    {
        return (int)(
            dexterity * dexterityScale +
            intelligence * intelligenceScale +
            strength * strengthScale
        );
    }

    private static ushort ToUShort(int value)
    {
        return (ushort)Math.Clamp(value, 0, ushort.MaxValue);
    }
}

public sealed class AdventurerRuntimeState
{
    public AdventurerStats Stats { get; }
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    public AdventurerRuntimeState(AdventurerStats stats)
    {
        Stats = stats;
        CurrentHealth = stats.MaxHealth;
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentHealth = Math.Max(0, CurrentHealth - amount);
    }
}
