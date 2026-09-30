using System;
using System.Collections.Generic;
using System.Linq;
using TCCGame.Core.Persistence;
using TCCGame.Core.Time;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Owns saved exploration missions. A mission stores a battle-ready snapshot
    /// of each selected adventurer, so later equipment edits cannot corrupt an
    /// expedition already travelling.
    /// </summary>
    public sealed class ExplorationService
    {
        public const int MinimumPartySize = 1;
        public const int TravelSeconds = 5;
        private readonly GameSaveData state;
        private readonly IClock clock;
        private readonly Action commit;

        public event Action<ExplorationMissionSaveData> MissionArrived;

        public ExplorationService(GameSaveData state, IClock clock, Action commit)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.commit = commit ?? throw new ArgumentNullException(nameof(commit));
            state.exploration ??= new ExplorationSaveData();
            state.exploration.missions ??= new List<ExplorationMissionSaveData>();
            state.exploration.completedEnvironments ??=
                new List<ExplorationEnvironment>();
        }

        public IReadOnlyList<ExplorationMissionSaveData> Missions => state.exploration.missions.AsReadOnly();

        public IReadOnlyList<ExplorationEnvironment> CompletedEnvironments =>
            state.exploration.completedEnvironments.AsReadOnly();

        public bool IsEnvironmentUnlocked(ExplorationEnvironment environment)
        {
            return ExplorationProgression.IsUnlocked(
                environment,
                state.exploration.completedEnvironments
            );
        }

        public bool TrySendParty(ExplorationEnvironment environment, IEnumerable<Char_base> party, out string error)
        {
            if (!Enum.IsDefined(typeof(ExplorationEnvironment), environment) ||
                !IsEnvironmentUnlocked(environment))
            {
                error = "Conclua as regiões anteriores para liberar este destino.";
                return false;
            }

            List<Char_base> selected = party?.Where(item => item != null).Distinct().ToList();
            if (selected == null || selected.Count < MinimumPartySize) { error = "Selecione ao menos um aventureiro."; return false; }
            if (selected.Any(item => !state.playerInventory.recruitedAdventurerIds.Contains(item.PersistenceId))) { error = "O grupo possui um aventureiro não contratado."; return false; }
            if (state.exploration.missions.Any(m => m.status == ExplorationMissionStatus.Travelling || m.status == ExplorationMissionStatus.ReadyForBattle || m.status == ExplorationMissionStatus.InBattle)) { error = "Já existe uma expedição em andamento."; return false; }

            var mission = new ExplorationMissionSaveData { id = state.exploration.nextMissionId++, environment = environment, status = ExplorationMissionStatus.Travelling, arrivalUnixSeconds = clock.UnixSeconds + TravelSeconds };
            foreach (Char_base adventurer in selected) mission.party.Add(CreateSnapshot(adventurer));
            state.exploration.missions.Add(mission);
            commit(); error = null; return true;
        }

        public bool RefreshArrivals()
        {
            bool changed = false;
            foreach (ExplorationMissionSaveData mission in state.exploration.missions.Where(m => m.status == ExplorationMissionStatus.Travelling && clock.UnixSeconds >= m.arrivalUnixSeconds))
            {
                mission.status = ExplorationMissionStatus.ReadyForBattle;
                changed = true;
                if (!mission.arrivalAnnounced) { mission.arrivalAnnounced = true; MissionArrived?.Invoke(mission); }
            }
            if (changed) commit();
            return changed;
        }

        public ExplorationMissionSaveData GetMission(int id) => state.exploration.missions.Find(m => m.id == id);
        public void SetStatus(int id, ExplorationMissionStatus status)
        {
            ExplorationMissionSaveData mission = GetMission(id);
            if (mission == null)
            {
                return;
            }

            mission.status = status;
            if (status == ExplorationMissionStatus.Won &&
                !state.exploration.completedEnvironments.Contains(
                    mission.environment
                ))
            {
                state.exploration.completedEnvironments.Add(
                    mission.environment
                );
            }

            commit();
        }

        private static ExplorationCombatantSaveData CreateSnapshot(Char_base adventurer)
        {
            AdventurerStats stats = AdventurerStatsCalculator.Calculate(adventurer);
            return new ExplorationCombatantSaveData { adventurerId = adventurer.PersistenceId, displayName = adventurer.name, maxHealth = stats.MaxHealth, currentHealth = stats.MaxHealth, minimumDamage = stats.MinimumDamage, maximumDamage = stats.MaximumDamage, speed = stats.Speed, armor = adventurer.armadura, magicResistance = adventurer.resistência_magica, damageType = stats.DamageType };
        }
    }
}
