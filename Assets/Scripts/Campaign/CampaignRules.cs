using System;
using TCCGame.Core;
using TCCGame.Core.Persistence;

namespace TCCGame.Gameplay.Campaign
{
    public sealed class CampaignRules
    {
        public const int DefaultMaximumThreatPower = 100000;
        public const int MaximumWaves = 10;

        public int GateMaximumHealth { get; }
        public double FirstWavePreparationSeconds { get; }
        public double CheckpointIntervalSeconds { get; }
        public int MaximumThreatPower { get; }
        public int MinimumTransferSeconds { get; }
        public int MaximumTransferSeconds { get; }

        public double AnnouncedThresholdSeconds => 90d;
        public double ImminentThresholdSeconds => 30d;
        public double CriticalThresholdSeconds => 10d;

        public double WarriorPower => 1d;
        public double ArcherPower => 1.25d;
        public double MagePower => 2d;
        public double AdvantageMultiplier => 1.5d;
        public double MinimumCasualtyRate => 0.03d;
        public double MaximumCasualtyRate => 0.65d;
        public double CasualtyPressureThreshold => 0.75d;
        public double CasualtyPressureScale => 0.35d;
        public double GateDamageBase => 10d;
        public double GateDamageThreatScale => 0.8d;
        public double SpoilsThreatScale => 0.15d;

        public CampaignRules(
            int gateMaximumHealth = GateSaveData.DefaultHealth,
            double firstWavePreparationSeconds = 180d,
            double checkpointIntervalSeconds = 10d,
            int maximumThreatPower = DefaultMaximumThreatPower,
            int minimumTransferSeconds = 15,
            int maximumTransferSeconds = 25
        )
        {
            if (gateMaximumHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(gateMaximumHealth)
                );
            }

            if (!IsFiniteNonNegative(firstWavePreparationSeconds))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(firstWavePreparationSeconds)
                );
            }

            if (!IsFiniteNonNegative(checkpointIntervalSeconds) ||
                checkpointIntervalSeconds <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(checkpointIntervalSeconds)
                );
            }

            if (maximumThreatPower <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumThreatPower)
                );
            }

            if (minimumTransferSeconds < 0 ||
                maximumTransferSeconds < minimumTransferSeconds)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumTransferSeconds)
                );
            }

            GateMaximumHealth = gateMaximumHealth;
            FirstWavePreparationSeconds = firstWavePreparationSeconds;
            CheckpointIntervalSeconds = checkpointIntervalSeconds;
            MaximumThreatPower = maximumThreatPower;
            MinimumTransferSeconds = minimumTransferSeconds;
            MaximumTransferSeconds = maximumTransferSeconds;
        }

        public IntRange GetIntervalRange(int sequence)
        {
            ValidateSequence(sequence);

            if (sequence <= 3)
            {
                return new IntRange(150, 240);
            }

            if (sequence <= 7)
            {
                return new IntRange(100, 180);
            }

            return new IntRange(75, 140);
        }

        public int CalculateBaseThreatPower(int sequence)
        {
            ValidateSequence(sequence);

            double calculated = 10d * Math.Pow(1.22d, sequence - 1d);

            if (double.IsInfinity(calculated) ||
                calculated >= MaximumThreatPower)
            {
                return MaximumThreatPower;
            }

            return Math.Clamp(
                (int)Math.Round(
                    calculated,
                    MidpointRounding.AwayFromZero
                ),
                1,
                MaximumThreatPower
            );
        }

        public double CalculateMilitaryProgress(GameSaveData state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            double troopPower = GetArmyPower(state.army);

            if (state.campaign?.gates != null)
            {
                foreach (GateSaveData gate in state.campaign.gates)
                {
                    if (gate == null)
                    {
                        continue;
                    }

                    troopPower +=
                        Math.Max(0, gate.warriors) * WarriorPower +
                        Math.Max(0, gate.archers) * ArcherPower +
                        Math.Max(0, gate.mages) * MagePower;
                }
            }

            if (state.campaign?.troopTransfers != null)
            {
                foreach (TroopTransferSaveData transfer in
                         state.campaign.troopTransfers)
                {
                    if (transfer == null || transfer.quantity <= 0)
                    {
                        continue;
                    }

                    troopPower += transfer.quantity *
                        GetUnitPower(transfer.unitType);
                }
            }

            int militaryBuildingLevels =
                state.GetBuildingLevel(BuildingType.Barracks) +
                state.GetBuildingLevel(BuildingType.ArcherStand) +
                state.GetBuildingLevel(BuildingType.MagicSchool);

            return troopPower + 2d * Math.Max(0, militaryBuildingLevels);
        }

        public double CalculateProgressFactor(
            GameSaveData state,
            int sequence
        )
        {
            ValidateSequence(sequence);
            double expectedProgress = 20d + 4d * (sequence - 1d);
            double factor = 0.90d +
                0.10d * CalculateMilitaryProgress(state) /
                expectedProgress;

            return Math.Clamp(factor, 0.90d, 1.15d);
        }

        public int CalculateThreatPower(
            GameSaveData state,
            int sequence
        )
        {
            double calculated = CalculateBaseThreatPower(sequence) *
                CalculateProgressFactor(state, sequence);

            return Math.Clamp(
                (int)Math.Round(
                    calculated,
                    MidpointRounding.AwayFromZero
                ),
                1,
                MaximumThreatPower
            );
        }

        public int GetEnemyCost(EnemyType enemyType)
        {
            return enemyType switch
            {
                EnemyType.Raider => 1,
                EnemyType.Brute => 3,
                EnemyType.Shaman => 4,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(enemyType)
                )
            };
        }

        public int GetEnemyWeight(EnemyType enemyType)
        {
            return enemyType switch
            {
                EnemyType.Raider => 60,
                EnemyType.Brute => 25,
                EnemyType.Shaman => 15,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(enemyType)
                )
            };
        }

        public bool IsEnemyUnlocked(EnemyType enemyType, int sequence)
        {
            ValidateSequence(sequence);

            return enemyType switch
            {
                EnemyType.Raider => true,
                EnemyType.Brute => sequence >= 3,
                EnemyType.Shaman => sequence >= 5,
                _ => false
            };
        }

        public double GetUnitPower(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Warrior => WarriorPower,
                UnitType.Archer => ArcherPower,
                UnitType.Mage => MagePower,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(unitType)
                )
            };
        }

        public EnemyType GetFavoredEnemy(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Warrior => EnemyType.Raider,
                UnitType.Archer => EnemyType.Shaman,
                UnitType.Mage => EnemyType.Brute,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(unitType)
                )
            };
        }

        public double GetEffectivenessMultiplier(
            UnitType unitType,
            EnemyType enemyType
        )
        {
            if (!Enum.IsDefined(typeof(EnemyType), enemyType))
            {
                throw new ArgumentOutOfRangeException(nameof(enemyType));
            }

            return GetFavoredEnemy(unitType) == enemyType
                ? AdvantageMultiplier
                : 1d;
        }

        private double GetArmyPower(ArmySaveData army)
        {
            if (army == null)
            {
                return 0d;
            }

            return Math.Max(0, army.warriors) * WarriorPower +
                   Math.Max(0, army.archers) * ArcherPower +
                   Math.Max(0, army.mages) * MagePower;
        }

        private static void ValidateSequence(int sequence)
        {
            if (sequence <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sequence));
            }
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value >= 0d;
        }
    }
}
