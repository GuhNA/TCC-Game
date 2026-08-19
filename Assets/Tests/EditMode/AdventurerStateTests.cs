using NUnit.Framework;
using UnityEngine;

public sealed class AdventurerStateTests
{
    [Test]
    public void Calculate_DoesNotMutateTheDefinitionAsset()
    {
        Char_base definition = CreateDefinition();
        definition.vida = 40;
        definition.dano_min = 7;
        definition.dano_max = 11;
        definition.vivo = false;

        AdventurerStats stats = AdventurerStatsCalculator.Calculate(
            definition
        );

        Assert.That(stats.MaxHealth, Is.EqualTo(40));
        Assert.That(definition.vida, Is.EqualTo(40));
        Assert.That(definition.dano_min, Is.EqualTo(7));
        Assert.That(definition.dano_max, Is.EqualTo(11));
        Assert.That(definition.vivo, Is.False);

        Object.DestroyImmediate(definition);
    }

    [Test]
    public void RuntimeStates_FromTheSameDefinitionAreIndependent()
    {
        Char_base definition = CreateDefinition();
        definition.vida = 40;
        AdventurerStats stats = AdventurerStatsCalculator.Calculate(
            definition
        );

        var first = new AdventurerRuntimeState(stats);
        var second = new AdventurerRuntimeState(stats);

        first.ApplyDamage(10);

        Assert.That(first.CurrentHealth, Is.EqualTo(30));
        Assert.That(second.CurrentHealth, Is.EqualTo(40));
        Assert.That(first.IsAlive, Is.True);
        Assert.That(second.IsAlive, Is.True);

        Object.DestroyImmediate(definition);
    }

    [Test]
    public void Calculate_AppliesEquipmentHealthOnlyToTheRuntimeResult()
    {
        Char_base definition = CreateDefinition();
        definition.vida = 40;
        Item item = ScriptableObject.CreateInstance<Item>();
        item.hp = 5;
        definition.mao = item;

        AdventurerStats first = AdventurerStatsCalculator.Calculate(
            definition
        );
        AdventurerStats second = AdventurerStatsCalculator.Calculate(
            definition
        );

        Assert.That(first.MaxHealth, Is.EqualTo(45));
        Assert.That(second.MaxHealth, Is.EqualTo(45));
        Assert.That(definition.vida, Is.EqualTo(40));

        Object.DestroyImmediate(item);
        Object.DestroyImmediate(definition);
    }

    private static Char_base CreateDefinition()
    {
        return ScriptableObject.CreateInstance<Char_base>();
    }
}
