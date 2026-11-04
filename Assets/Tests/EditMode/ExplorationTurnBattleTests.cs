using System;
using System.Reflection;
using NUnit.Framework;
using TCCGame.Gameplay.Exploration;
using UnityEngine;

namespace TCCGame.Tests.EditMode
{
    public sealed class ExplorationTurnBattleTests
    {
        [Test]
        public void FasterCombatantActsFirst()
        {
            ExplorationBattleCombatant hero = Create(
                "hero",
                ExplorationCombatTeam.Adventurers,
                speed: 12
            );
            ExplorationBattleCombatant enemy = Create(
                "enemy",
                ExplorationCombatTeam.Enemies,
                speed: 4
            );
            var battle = new ExplorationTurnBattle(
                new[] { hero },
                new[] { enemy },
                1
            );

            ExplorationTurnResult turn = battle.ExecuteNextTurn();

            Assert.That(turn.Attacker, Is.SameAs(hero));
            Assert.That(turn.Round, Is.EqualTo(1));
        }

        [Test]
        public void DoubleSpeedActsTwiceBeforeSlowerCombatant()
        {
            ExplorationBattleCombatant fast = Create(
                "fast",
                ExplorationCombatTeam.Adventurers,
                speed: 20
            );
            ExplorationBattleCombatant slow = Create(
                "slow",
                ExplorationCombatTeam.Enemies,
                speed: 10
            );
            var battle = new ExplorationTurnBattle(
                new[] { fast },
                new[] { slow },
                1
            );

            ExplorationTurnResult first = battle.ExecuteNextTurn();
            ExplorationTurnResult second = battle.ExecuteNextTurn();
            ExplorationTurnResult third = battle.ExecuteNextTurn();

            Assert.That(first.Attacker, Is.SameAs(fast));
            Assert.That(second.Attacker, Is.SameAs(fast));
            Assert.That(third.Attacker, Is.SameAs(slow));
        }

        [Test]
        public void RelevantDefenseReducesDamageButNeverBelowOne()
        {
            var hero = new ExplorationBattleCombatant(
                "hero",
                "Hero",
                ExplorationCombatTeam.Adventurers,
                20,
                20,
                5,
                5,
                10,
                0,
                0,
                DamageType.Physical
            );
            var enemy = new ExplorationBattleCombatant(
                "enemy",
                "Enemy",
                ExplorationCombatTeam.Enemies,
                20,
                20,
                1,
                1,
                1,
                100,
                0,
                DamageType.Physical
            );
            var battle = new ExplorationTurnBattle(
                new[] { hero },
                new[] { enemy },
                1
            );

            ExplorationTurnResult turn = battle.ExecuteNextTurn();

            Assert.That(turn.FinalDamage, Is.EqualTo(1));
            Assert.That(enemy.CurrentHealth, Is.EqualTo(19));
        }

        [Test]
        public void MoreThanThreeCombatantsIsRejected()
        {
            ExplorationBattleCombatant[] heroes =
            {
                Create("1", ExplorationCombatTeam.Adventurers, 1),
                Create("2", ExplorationCombatTeam.Adventurers, 1),
                Create("3", ExplorationCombatTeam.Adventurers, 1),
                Create("4", ExplorationCombatTeam.Adventurers, 1)
            };

            Assert.Throws<ArgumentException>(() =>
                new ExplorationTurnBattle(
                    heroes,
                    new[]
                    {
                        Create("enemy", ExplorationCombatTeam.Enemies, 1)
                    },
                    1
                ));
        }

        [Test]
        public void AdventurerAlwaysTargetsLeftmostLivingEnemy()
        {
            ExplorationBattleCombatant hero = Create(
                "hero",
                ExplorationCombatTeam.Adventurers,
                20
            );
            ExplorationBattleCombatant leftEnemy = Create(
                "left",
                ExplorationCombatTeam.Enemies,
                1
            );
            ExplorationBattleCombatant rightEnemy = Create(
                "right",
                ExplorationCombatTeam.Enemies,
                1
            );
            var battle = new ExplorationTurnBattle(
                new[] { hero },
                new[] { leftEnemy, rightEnemy },
                5
            );

            ExplorationTurnResult turn = battle.ExecuteNextTurn();

            Assert.That(turn.Target, Is.SameAs(leftEnemy));
        }

        [Test]
        public void EnemyAlwaysTargetsRightmostLivingAdventurer()
        {
            ExplorationBattleCombatant leftHero = Create(
                "left",
                ExplorationCombatTeam.Adventurers,
                1
            );
            ExplorationBattleCombatant rightHero = Create(
                "right",
                ExplorationCombatTeam.Adventurers,
                1
            );
            ExplorationBattleCombatant enemy = Create(
                "enemy",
                ExplorationCombatTeam.Enemies,
                20
            );
            var battle = new ExplorationTurnBattle(
                new[] { leftHero, rightHero },
                new[] { enemy },
                5
            );

            ExplorationTurnResult turn = battle.ExecuteNextTurn();

            Assert.That(turn.Target, Is.SameAs(rightHero));
        }

        [Test]
        public void EncounterRollsRepeatedEnemiesInsidePowerRange()
        {
            var enemy = ScriptableObject.CreateInstance<
                ExplorationEnemyDefinition>();
            SetPrivateField(enemy, "battlePower", 10);
            var encounter = ScriptableObject.CreateInstance<
                ExplorationEncounterDefinition>();
            SetPrivateField(encounter, "minimumEnemyCount", 1);
            SetPrivateField(encounter, "maximumEnemyCount", 3);
            SetPrivateField(encounter, "allowRepeatedEnemies", true);
            SetPrivateField(encounter, "minimumEnemyBattlePower", 20);
            SetPrivateField(encounter, "maximumEnemyBattlePower", 20);
            SetPrivateField(
                encounter,
                "enemies",
                new[] { enemy }
            );

            var party = encounter.RollEnemyParty(123);

            Assert.That(party.Count, Is.EqualTo(2));
            Assert.That(party[0], Is.SameAs(enemy));
            Assert.That(party[1], Is.SameAs(enemy));
            UnityEngine.Object.DestroyImmediate(encounter);
            UnityEngine.Object.DestroyImmediate(enemy);
        }

        [TestCase(ExplorationCombatTeam.Adventurers)]
        [TestCase(ExplorationCombatTeam.Enemies)]
        public void Formation_OrdersByCombinedDefenseThenMaximumHealth(ExplorationCombatTeam team)
        {
            var armored = CreateDefender("armored", team, 10, 0, 60);
            var resistant = CreateDefender("resistant", team, 2, 9, 30);
            var healthier = CreateDefender("healthier", team, 6, 4, 100);
            // Vida atual não substitui o atributo de vida máxima no desempate.
            healthier.ApplyDamage(95);
            var input = new[] { healthier, resistant, armored };
            var other = Create("other", team == ExplorationCombatTeam.Adventurers
                ? ExplorationCombatTeam.Enemies : ExplorationCombatTeam.Adventurers, 1);
            var battle = new ExplorationTurnBattle(
                team == ExplorationCombatTeam.Adventurers ? input : new[] { other },
                team == ExplorationCombatTeam.Enemies ? input : new[] { other }, 1);

            CollectionAssert.AreEqual(team == ExplorationCombatTeam.Adventurers
                ? new[] { armored, healthier, resistant }
                : new[] { resistant, healthier, armored },
                team == ExplorationCombatTeam.Adventurers ? battle.Adventurers : battle.Enemies);
            CollectionAssert.AreEqual(new[] { healthier, resistant, armored }, input,
                "A formação não deve modificar a lista original da missão.");
        }

        [TestCase(ExplorationCombatTeam.Adventurers)]
        [TestCase(ExplorationCombatTeam.Enemies)]
        public void Formation_FullTiesPreserveInputOrder(ExplorationCombatTeam team)
        {
            var first = CreateDefender("z", team, 4, 6, 100);
            var second = CreateDefender("a", team, 8, 2, 100);
            var other = Create("other", team == ExplorationCombatTeam.Adventurers
                ? ExplorationCombatTeam.Enemies : ExplorationCombatTeam.Adventurers, 1);
            var battle = new ExplorationTurnBattle(
                team == ExplorationCombatTeam.Adventurers ? new[] { first, second } : new[] { other },
                team == ExplorationCombatTeam.Enemies ? new[] { first, second } : new[] { other }, 1);
            CollectionAssert.AreEqual(new[] { first, second },
                team == ExplorationCombatTeam.Adventurers ? battle.Adventurers : battle.Enemies);
        }

        [TestCase(ExplorationCombatTeam.Adventurers)]
        [TestCase(ExplorationCombatTeam.Enemies)]
        public void Attacks_FollowDefensiveFormationAndSkipFallenFrontliner(ExplorationCombatTeam targetTeam)
        {
            var back = CreateDefender("back", targetTeam, 0, 0, 100);
            var middle = CreateDefender("middle", targetTeam, 2, 3, 100);
            var front = CreateDefender("front", targetTeam, 3, 2, 200);
            var attackerTeam = targetTeam == ExplorationCombatTeam.Adventurers
                ? ExplorationCombatTeam.Enemies : ExplorationCombatTeam.Adventurers;
            var attacker = Create("fast", attackerTeam, 100);
            var defenders = new[] { front, back, middle };
            var battle = new ExplorationTurnBattle(
                targetTeam == ExplorationCombatTeam.Adventurers ? defenders : new[] { attacker },
                targetTeam == ExplorationCombatTeam.Enemies ? defenders : new[] { attacker }, 1);
            ExplorationTurnResult first = battle.ExecuteNextTurn();
            Assert.That(first.Attacker, Is.SameAs(attacker));
            Assert.That(first.Target, Is.SameAs(front));
            front.ApplyDamage(150);
            Assert.That(battle.ExecuteNextTurn().Target, Is.SameAs(front),
                "Dano recebido não deve mudar a formação.");
            front.ApplyDamage(200);
            Assert.That(battle.ExecuteNextTurn().Target, Is.SameAs(middle));
        }

        private static ExplorationBattleCombatant CreateDefender(string id,
            ExplorationCombatTeam team, int armor, int resistance, int health)
        {
            return new ExplorationBattleCombatant(id, id, team, health, health,
                1, 1, 1, armor, resistance, DamageType.Physical);
        }

        private static void SetPrivateField<T>(
            object target,
            string fieldName,
            T value
        )
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static ExplorationBattleCombatant Create(
            string id,
            ExplorationCombatTeam team,
            int speed
        )
        {
            return new ExplorationBattleCombatant(
                id,
                id,
                team,
                20,
                20,
                3,
                3,
                speed,
                0,
                0,
                DamageType.Physical
            );
        }
    }
}
