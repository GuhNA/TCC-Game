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
        public const int MaximumPartySize =
            ExplorationEncounterDefinition.MaximumTeamSize;
        public const int TravelSeconds = 5;
        public const int ReturnSeconds = 5;
        public const int EnemyCounterAttackSeconds = 300;
        // Default production interval. A scene component can override this per
        // session through SetExplorerCollectionIntervalSeconds.
        public const int ExplorerCollectionSeconds = 300;
        private readonly GameSaveData state;
        private readonly IClock clock;
        private readonly Action commit;
        private int explorerCollectionIntervalSeconds = ExplorerCollectionSeconds;

        public event Action<ExplorationMissionSaveData> MissionArrived;
        public event Action<ExplorationMissionSaveData> MissionReturned;

        public ExplorationService(GameSaveData state, IClock clock, Action commit)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.commit = commit ?? throw new ArgumentNullException(nameof(commit));
            state.exploration ??= new ExplorationSaveData();
            state.playerInventory ??= new PlayerInventorySaveData();
            state.playerInventory.items ??= new List<InventoryItemSaveData>();
            state.exploration.missions ??= new List<ExplorationMissionSaveData>();
            state.exploration.completedEnvironments ??=
                new List<ExplorationEnvironment>();
            state.exploration.areas ??= new List<ExplorationAreaSaveData>();
            foreach (ExplorationMissionSaveData mission in
                     state.exploration.missions.Where(mission => mission != null))
            {
                mission.party ??= new List<ExplorationCombatantSaveData>();
                mission.rewards ??= new List<ExplorationRewardSaveData>();
            }

            foreach (ExplorationAreaSaveData area in
                     state.exploration.areas.Where(area => area != null))
            {
                area.guardParty ??= new List<ExplorationCombatantSaveData>();
                area.lastWorkerLoot ??=
                    new List<ExplorationRewardSaveData>();
                area.pendingWorkerLoot ??=
                    new List<ExplorationRewardSaveData>();
            }

            foreach (ExplorationEnvironment completed in
                     state.exploration.completedEnvironments)
            {
                ExplorationAreaSaveData area = GetOrCreateArea(completed);
                area.captured = true;
                if (area.nextEnemyAttackUnixSeconds <= 0 &&
                    !area.enemyAttackPending)
                {
                    area.nextEnemyAttackUnixSeconds =
                        clock.UnixSeconds + EnemyCounterAttackSeconds;
                }
            }
        }

        public IReadOnlyList<ExplorationMissionSaveData> Missions => state.exploration.missions.AsReadOnly();

        public IReadOnlyList<ExplorationEnvironment> CompletedEnvironments =>
            state.exploration.completedEnvironments.AsReadOnly();

        public int TotalExplorers => Math.Max(
            0,
            state.GetBuildingLevel(BuildingType.ExplorersLodge)
        );

        public int AssignedExplorers => state.exploration.areas
            .Where(area => area != null && area.captured)
            .Sum(area => Math.Max(0, area.assignedWorkers));

        public int AvailableExplorers => Math.Max(
            0,
            TotalExplorers - AssignedExplorers
        );

        public int ExplorerCollectionIntervalSeconds =>
            explorerCollectionIntervalSeconds;

        /// <summary>
        /// True whenever the tavern action board has work waiting for the
        /// player: an arrived party / battle, an unread return report, or
        /// explorer resources still waiting for transport.
        /// </summary>
        public bool HasTavernActionPending =>
            state.exploration.missions.Any(mission => mission != null &&
                mission.status == ExplorationMissionStatus.ReadyForBattle) ||
            state.exploration.missions.Any(mission => mission != null &&
                mission.status == ExplorationMissionStatus.Returned &&
                !mission.reportAcknowledged) ||
            state.exploration.areas.Any(area => area != null &&
                area.pendingWorkerLoot != null &&
                area.pendingWorkerLoot.Any(reward => reward != null &&
                    reward.quantity > 0));

        public void SetExplorerCollectionIntervalSeconds(int seconds)
        {
            explorerCollectionIntervalSeconds = Math.Max(1, seconds);
        }

        public ExplorationMissionSaveData GetActiveMission()
        {
            return state.exploration.missions.LastOrDefault(IsActive);
        }

        public ExplorationMissionSaveData GetActiveMissionForEnvironment(
            ExplorationEnvironment environment
        )
        {
            return state.exploration.missions.LastOrDefault(mission =>
                mission != null &&
                mission.environment == environment &&
                IsActive(mission));
        }

        public ExplorationMissionSaveData GetLatestMissionForEnvironment(
            ExplorationEnvironment environment
        )
        {
            return state.exploration.missions.LastOrDefault(mission =>
                mission != null && mission.environment == environment);
        }

        public ExplorationAreaSaveData GetArea(
            ExplorationEnvironment environment
        )
        {
            return state.exploration.areas.LastOrDefault(area =>
                area != null && area.environment == environment);
        }

        public ExplorationMissionSaveData GetCurrentBattleMission()
        {
            return state.exploration.missions.LastOrDefault(mission =>
                mission != null &&
                mission.status == ExplorationMissionStatus.InBattle);
        }

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

            ExplorationAreaSaveData targetArea = GetArea(environment);
            bool isDefense = targetArea != null &&
                targetArea.captured && targetArea.enemyAttackPending;
            if (state.exploration.completedEnvironments.Contains(environment) &&
                !isDefense)
            {
                error = "A região já foi capturada. Abra seu relatório para administrá-la.";
                return false;
            }

            List<Char_base> selected = party?.Where(item => item != null).Distinct().ToList();
            if (selected == null || selected.Count < MinimumPartySize) { error = "Selecione ao menos um aventureiro."; return false; }
            if (selected.Count > MaximumPartySize) { error = "O grupo pode ter no máximo três aventureiros."; return false; }
            if (selected.Any(item => !state.playerInventory.recruitedAdventurerIds.Contains(item.PersistenceId))) { error = "O grupo possui um aventureiro não contratado."; return false; }
            if (selected.Any(item => IsAdventurerBusy(item.PersistenceId)))
            {
                error = "O grupo possui um aventureiro que já está em missão ou na guarda.";
                return false;
            }
            if (state.exploration.missions.Any(IsActive)) { error = "Já existe uma expedição em andamento."; return false; }

            var mission = new ExplorationMissionSaveData { id = state.exploration.nextMissionId++, environment = environment, kind = isDefense ? ExplorationMissionKind.Defense : ExplorationMissionKind.Capture, status = ExplorationMissionStatus.Travelling, arrivalUnixSeconds = clock.UnixSeconds + TravelSeconds };
            foreach (Char_base adventurer in selected) mission.party.Add(CreateSnapshot(adventurer));
            state.exploration.missions.Add(mission);
            commit(); error = null; return true;
        }

        public bool RefreshArrivals()
        {
            bool changed = false;
            foreach (ExplorationMissionSaveData legacyMission in
                     state.exploration.missions.Where(mission =>
                         mission != null &&
                         mission.status == ExplorationMissionStatus.Won))
            {
                legacyMission.status = ExplorationMissionStatus.Returning;
                legacyMission.returnUnixSeconds =
                    clock.UnixSeconds + ReturnSeconds;
                ExplorationAreaSaveData legacyArea = GetOrCreateArea(
                    legacyMission.environment
                );
                legacyArea.captured = true;
                if (legacyArea.nextEnemyAttackUnixSeconds <= 0)
                {
                    legacyArea.nextEnemyAttackUnixSeconds =
                        clock.UnixSeconds + EnemyCounterAttackSeconds;
                }
                changed = true;
            }

            foreach (ExplorationMissionSaveData mission in state.exploration.missions.Where(m => m.status == ExplorationMissionStatus.Travelling && clock.UnixSeconds >= m.arrivalUnixSeconds))
            {
                mission.status = ExplorationMissionStatus.ReadyForBattle;
                changed = true;
                if (!mission.arrivalAnnounced) { mission.arrivalAnnounced = true; MissionArrived?.Invoke(mission); }
            }

            foreach (ExplorationMissionSaveData mission in
                     state.exploration.missions.Where(mission =>
                         mission != null &&
                         mission.status == ExplorationMissionStatus.Returning &&
                         clock.UnixSeconds >= mission.returnUnixSeconds))
            {
                mission.status = ExplorationMissionStatus.Returned;
                ClaimMissionRewards(mission);
                changed = true;
                if (!mission.returnAnnounced)
                {
                    mission.returnAnnounced = true;
                    MissionReturned?.Invoke(mission);
                }
            }
            changed |= RefreshAreaThreats();
            if (changed) commit();
            return changed;
        }

        public ExplorationMissionSaveData GetMission(int id) => state.exploration.missions.Find(m => m.id == id);

        public bool TryBeginBattle(
            int missionId,
            out ExplorationMissionSaveData mission,
            out string error
        )
        {
            RefreshArrivals();
            mission = GetMission(missionId);
            if (mission == null)
            {
                error = "A expedição não foi encontrada.";
                return false;
            }

            if (mission.status == ExplorationMissionStatus.InBattle)
            {
                error = null;
                return true;
            }

            if (mission.status != ExplorationMissionStatus.ReadyForBattle)
            {
                error = "A equipe ainda não está pronta para esta batalha.";
                return false;
            }

            mission.status = ExplorationMissionStatus.InBattle;
            commit();
            error = null;
            return true;
        }

        public void UpdatePartyHealth(
            int missionId,
            IReadOnlyDictionary<string, int> currentHealthByAdventurer
        )
        {
            ExplorationMissionSaveData mission = GetMission(missionId);
            if (mission == null || currentHealthByAdventurer == null)
            {
                return;
            }

            bool changed = false;
            foreach (ExplorationCombatantSaveData member in mission.party)
            {
                if (member == null ||
                    !currentHealthByAdventurer.TryGetValue(
                        member.adventurerId,
                        out int health
                    ))
                {
                    continue;
                }

                int normalized = Math.Clamp(health, 0, member.maxHealth);
                if (member.currentHealth == normalized)
                {
                    continue;
                }

                member.currentHealth = normalized;
                changed = true;
            }

            if (changed)
            {
                commit();
            }
        }

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

            if ((status == ExplorationMissionStatus.Defeated ||
                 status == ExplorationMissionStatus.Retreated) &&
                mission.kind == ExplorationMissionKind.Defense)
            {
                ExplorationAreaSaveData area = GetArea(mission.environment);
                if (area != null)
                {
                    area.captured = false;
                    area.enemyAttackPending = false;
                    area.assignedWorkers = 0;
                    area.nextWorkerCollectionUnixSeconds = 0;
                    area.guardParty.Clear();
                }
                state.exploration.completedEnvironments.Remove(
                    mission.environment
                );
            }

            commit();
        }

        public void CompleteVictory(
            int missionId,
            IEnumerable<ExplorationRewardSaveData> rewards
        )
        {
            ExplorationMissionSaveData mission = GetMission(missionId);
            if (mission == null)
            {
                return;
            }

            mission.rewards = rewards?
                .Where(reward => reward != null && reward.quantity > 0 &&
                    !string.IsNullOrWhiteSpace(reward.resourceId))
                .Select(reward => new ExplorationRewardSaveData
                {
                    resourceId = reward.resourceId.Trim(),
                    displayName = reward.displayName?.Trim(),
                    quantity = reward.quantity
                })
                .ToList() ?? new List<ExplorationRewardSaveData>();
            mission.status = ExplorationMissionStatus.Returning;
            mission.returnUnixSeconds = clock.UnixSeconds + ReturnSeconds;
            mission.returnAnnounced = false;
            mission.reportAcknowledged = false;
            mission.rewardsClaimed = false;

            if (!state.exploration.completedEnvironments.Contains(
                    mission.environment))
            {
                state.exploration.completedEnvironments.Add(
                    mission.environment
                );
            }

            ExplorationAreaSaveData area = GetOrCreateArea(
                mission.environment
            );
            area.captured = true;
            area.enemyAttackPending = false;
            area.nextEnemyAttackUnixSeconds =
                clock.UnixSeconds + EnemyCounterAttackSeconds;
            commit();
        }

        public bool AcknowledgeReport(int missionId)
        {
            ExplorationMissionSaveData mission = GetMission(missionId);
            if (mission == null || mission.reportAcknowledged)
            {
                return false;
            }

            mission.reportAcknowledged = true;
            commit();
            return true;
        }

        public bool TryAssignGuardParty(
            ExplorationEnvironment environment,
            IEnumerable<Char_base> party,
            out string error
        )
        {
            ExplorationAreaSaveData area = GetArea(environment);
            if (area == null || !area.captured)
            {
                error = "Capture a região antes de posicionar uma guarda.";
                return false;
            }

            List<Char_base> selected = party?
                .Where(item => item != null)
                .Distinct()
                .ToList() ?? new List<Char_base>();
            if (selected.Count < MinimumPartySize ||
                selected.Count > MaximumPartySize)
            {
                error = "A guarda deve ter entre um e três aventureiros.";
                return false;
            }

            if (selected.Any(item =>
                    !state.playerInventory.recruitedAdventurerIds.Contains(
                        item.PersistenceId)))
            {
                error = "A guarda possui um aventureiro não contratado.";
                return false;
            }

            if (selected.Any(item => IsAdventurerBusy(
                    item.PersistenceId,
                    environment)))
            {
                error = "Um dos aventureiros selecionados já está ocupado.";
                return false;
            }

            area.guardParty = selected.Select(CreateSnapshot).ToList();
            commit();
            error = null;
            return true;
        }

        public bool IsAdventurerBusy(
            string adventurerId,
            ExplorationEnvironment? ignoredGuardArea = null
        )
        {
            if (string.IsNullOrWhiteSpace(adventurerId))
            {
                return false;
            }

            bool inMission = state.exploration.missions.Any(mission =>
                IsActive(mission) && mission.party.Any(member =>
                    member != null && member.adventurerId == adventurerId));
            if (inMission)
            {
                return true;
            }

            return state.exploration.areas.Any(area =>
                area != null && area.environment != ignoredGuardArea &&
                area.guardParty.Any(member =>
                    member != null && member.adventurerId == adventurerId));
        }

        public bool TrySetAssignedWorkers(
            ExplorationEnvironment environment,
            int workerCount,
            out string error
        )
        {
            ExplorationAreaSaveData area = GetArea(environment);
            if (area == null || !area.captured)
            {
                error = "Capture a região antes de enviar trabalhadores.";
                return false;
            }

            if (workerCount < 0)
            {
                error = "A quantidade de trabalhadores não pode ser negativa.";
                return false;
            }

            int assignedOutsideThisArea = AssignedExplorers -
                Math.Max(0, area.assignedWorkers);
            if (assignedOutsideThisArea + workerCount > TotalExplorers)
            {
                error = "Melhore a Construção de Exploradores para liberar " +
                    "mais exploradores. Disponíveis: " + AvailableExplorers +
                    ".";
                return false;
            }

            area.assignedWorkers = workerCount;
            area.nextWorkerCollectionUnixSeconds = workerCount > 0
                ? area.nextWorkerCollectionUnixSeconds > clock.UnixSeconds
                    ? area.nextWorkerCollectionUnixSeconds
                    : clock.UnixSeconds + explorerCollectionIntervalSeconds
                : 0;
            commit();
            error = null;
            return true;
        }

        public bool RefreshExplorerCollections(
            ExplorationEncounterCatalog encounterCatalog
        )
        {
            if (encounterCatalog == null)
            {
                return false;
            }

            bool changed = false;
            foreach (ExplorationAreaSaveData area in
                     state.exploration.areas.Where(area =>
                         area != null && area.captured &&
                         !area.enemyAttackPending && area.assignedWorkers > 0))
            {
                if (area.nextWorkerCollectionUnixSeconds <= 0)
                {
                    area.nextWorkerCollectionUnixSeconds =
                        clock.UnixSeconds + explorerCollectionIntervalSeconds;
                    changed = true;
                    continue;
                }

                if (!encounterCatalog.TryGet(
                        area.environment,
                        out ExplorationEncounterDefinition encounter))
                {
                    continue;
                }

                int processedCycles = 0;
                while (clock.UnixSeconds >=
                           area.nextWorkerCollectionUnixSeconds &&
                       processedCycles < 100)
                {
                    area.lastWorkerLoot = RollExplorerLoot(
                        area,
                        encounter,
                        area.nextWorkerCollectionUnixSeconds
                    );
                    AddPendingWorkerLoot(area, area.lastWorkerLoot);

                    area.nextWorkerCollectionUnixSeconds +=
                        explorerCollectionIntervalSeconds;
                    processedCycles++;
                    changed = true;
                }
            }

            if (changed)
            {
                commit();
            }

            return changed;
        }

        /// <summary>
        /// Moves every pending worker reward for one captured area into the
        /// player's inventory. The tavern UI should call this from its
        /// "Transportar recursos" action.
        /// </summary>
        public bool TryTransportWorkerLoot(
            ExplorationEnvironment environment,
            out string error
        )
        {
            ExplorationAreaSaveData area = GetArea(environment);
            if (area == null || area.pendingWorkerLoot == null ||
                !area.pendingWorkerLoot.Any(reward => reward != null &&
                    reward.quantity > 0))
            {
                error = "Não há recursos aguardando transporte nesta região.";
                return false;
            }

            foreach (ExplorationRewardSaveData reward in area.pendingWorkerLoot)
            {
                if (reward != null)
                {
                    AddRewardToInventory(reward.resourceId, reward.quantity);
                }
            }

            area.pendingWorkerLoot.Clear();
            commit();
            error = null;
            return true;
        }

        private bool RefreshAreaThreats()
        {
            bool changed = false;
            foreach (ExplorationAreaSaveData area in
                     state.exploration.areas.Where(area =>
                         area != null && area.captured &&
                         !area.enemyAttackPending &&
                         area.nextEnemyAttackUnixSeconds > 0 &&
                         clock.UnixSeconds >= area.nextEnemyAttackUnixSeconds))
            {
                area.enemyAttackPending = true;
                area.nextEnemyAttackUnixSeconds = 0;
                area.nextWorkerCollectionUnixSeconds = 0;
                changed = true;

            }

            ExplorationAreaSaveData guardedArea = state.exploration.areas
                .FirstOrDefault(area => area != null && area.captured &&
                    area.enemyAttackPending && area.guardParty.Count > 0 &&
                    !state.exploration.missions.Any(mission =>
                        IsActive(mission) &&
                        mission.environment == area.environment));
            if (guardedArea != null &&
                !state.exploration.missions.Any(IsActive))
            {
                state.exploration.missions.Add(
                    new ExplorationMissionSaveData
                    {
                        id = state.exploration.nextMissionId++,
                        environment = guardedArea.environment,
                        kind = ExplorationMissionKind.Defense,
                        status = ExplorationMissionStatus.ReadyForBattle,
                        arrivalUnixSeconds = clock.UnixSeconds,
                        arrivalAnnounced = true,
                        party = guardedArea.guardParty
                            .Select(CloneCombatant)
                            .ToList()
                    }
                );
                changed = true;
            }

            return changed;
        }

        private static ExplorationCombatantSaveData CloneCombatant(
            ExplorationCombatantSaveData source
        )
        {
            return new ExplorationCombatantSaveData
            {
                adventurerId = source.adventurerId,
                displayName = source.displayName,
                maxHealth = source.maxHealth,
                currentHealth = source.maxHealth,
                minimumDamage = source.minimumDamage,
                maximumDamage = source.maximumDamage,
                speed = source.speed,
                armor = source.armor,
                magicResistance = source.magicResistance,
                damageType = source.damageType
            };
        }

        private static bool IsActive(ExplorationMissionSaveData mission)
        {
            return mission != null &&
                (mission.status == ExplorationMissionStatus.Travelling ||
                 mission.status == ExplorationMissionStatus.ReadyForBattle ||
                 mission.status == ExplorationMissionStatus.InBattle ||
                 mission.status == ExplorationMissionStatus.Returning);
        }

        private ExplorationAreaSaveData GetOrCreateArea(
            ExplorationEnvironment environment
        )
        {
            ExplorationAreaSaveData area = GetArea(environment);
            if (area != null)
            {
                return area;
            }

            area = new ExplorationAreaSaveData
            {
                environment = environment
            };
            state.exploration.areas.Add(area);
            return area;
        }

        private void ClaimMissionRewards(ExplorationMissionSaveData mission)
        {
            if (mission.rewardsClaimed)
            {
                return;
            }

            state.playerInventory.items ??= new List<InventoryItemSaveData>();
            foreach (ExplorationRewardSaveData reward in mission.rewards ??
                     new List<ExplorationRewardSaveData>())
            {
                if (reward == null || reward.quantity <= 0 ||
                    string.IsNullOrWhiteSpace(reward.resourceId))
                {
                    continue;
                }

                AddRewardToInventory(reward.resourceId, reward.quantity);
            }

            mission.rewardsClaimed = true;
        }

        private List<ExplorationRewardSaveData> RollExplorerLoot(
            ExplorationAreaSaveData area,
            ExplorationEncounterDefinition encounter,
            long cycleTimestamp
        )
        {
            int seed = unchecked(
                ((int)area.environment + 1) * 486187739 ^
                cycleTimestamp.GetHashCode()
            );
            var random = new Random(seed);
            var rewards = new Dictionary<string, ExplorationRewardSaveData>();
            for (int explorer = 0;
                 explorer < area.assignedWorkers;
                 explorer++)
            {
                foreach (ExplorationRewardEntry entry in
                         encounter.PossibleRewards.Where(entry => entry != null))
                {
                    int quantity = entry.Roll(random);
                    if (quantity <= 0 || entry.Resource == null)
                    {
                        continue;
                    }

                    string id = entry.Resource.PersistenceId;
                    if (!rewards.TryGetValue(id, out var reward))
                    {
                        reward = new ExplorationRewardSaveData
                        {
                            resourceId = id,
                            displayName = entry.Resource.DisplayName
                        };
                        rewards.Add(id, reward);
                    }

                    reward.quantity = checked(reward.quantity + quantity);
                }
            }

            return rewards.Values.ToList();
        }

        private static void AddPendingWorkerLoot(
            ExplorationAreaSaveData area,
            IEnumerable<ExplorationRewardSaveData> collectedLoot
        )
        {
            area.pendingWorkerLoot ??= new List<ExplorationRewardSaveData>();
            foreach (ExplorationRewardSaveData reward in collectedLoot ??
                     Enumerable.Empty<ExplorationRewardSaveData>())
            {
                if (reward == null || reward.quantity <= 0 ||
                    string.IsNullOrWhiteSpace(reward.resourceId))
                {
                    continue;
                }

                ExplorationRewardSaveData pending = area.pendingWorkerLoot.Find(
                    candidate => candidate != null &&
                        candidate.resourceId == reward.resourceId
                );
                if (pending == null)
                {
                    area.pendingWorkerLoot.Add(new ExplorationRewardSaveData
                    {
                        resourceId = reward.resourceId,
                        displayName = reward.displayName,
                        quantity = reward.quantity
                    });
                }
                else
                {
                    pending.quantity = checked(pending.quantity + reward.quantity);
                }
            }
        }

        private void AddRewardToInventory(string resourceId, int quantity)
        {
            if (quantity <= 0 || string.IsNullOrWhiteSpace(resourceId))
            {
                return;
            }

            string id = resourceId.Trim();
            InventoryItemSaveData item = state.playerInventory.items.Find(
                candidate => candidate != null && candidate.itemId == id
            );
            if (item == null)
            {
                item = new InventoryItemSaveData { itemId = id };
                state.playerInventory.items.Add(item);
            }

            item.quantity = checked(item.quantity + quantity);
        }

        private static ExplorationCombatantSaveData CreateSnapshot(Char_base adventurer)
        {
            AdventurerStats stats = AdventurerStatsCalculator.Calculate(adventurer);
            return new ExplorationCombatantSaveData { adventurerId = adventurer.PersistenceId, displayName = adventurer.name, maxHealth = stats.MaxHealth, currentHealth = stats.MaxHealth, minimumDamage = stats.MinimumDamage, maximumDamage = stats.MaximumDamage, speed = stats.Speed, armor = adventurer.armadura, magicResistance = adventurer.resistência_magica, damageType = stats.DamageType };
        }
    }
}
