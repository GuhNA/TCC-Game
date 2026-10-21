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
