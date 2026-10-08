using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using TCCGame.Core;
using TCCGame.Gameplay.Navigation;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Controller for the single reusable exploration-combat scene. It reads the
    /// active mission from the save and the environment content from the catalog,
    /// then fills the world-space background and three prefab slots on each team.
    /// </summary>
    public sealed class ExplorationCombatSceneController : MonoBehaviour
    {
        private const float SecondsBetweenTurns = 0.65f;
        private const int MaximumTurns = 500;

        [Header("Environment content")]
        [SerializeField] private ExplorationEncounterCatalog encounterCatalog;
        [SerializeField] private Char_base[] adventurerCatalog;
        [SerializeField] private ExplorationBattleBackground battleBackground;

        [Header("Combatant slots (maximum three per team)")]
        [SerializeField] private ExplorationCombatantView[] adventurerViews;
        [SerializeField] private ExplorationCombatantView[] enemyViews;

        [Header("Interface")]
        [SerializeField] private TMP_Text environmentTitleText;
        [SerializeField] private TMP_Text roundText;
        [SerializeField] private TMP_Text combatLogText;
        [Tooltip("Optional Scroll View that contains Combat Log Text.")]
        [SerializeField] private ScrollRect combatLogScrollRect;
        [SerializeField] private TMP_Text detailsText;
        [SerializeField] private TMP_Text rewardsText;
        [SerializeField] private Button battleButton;
        [SerializeField] private TMP_Text battleButtonText;
        [SerializeField] private Button retreatButton;

        private readonly Dictionary<string, CombatVisualData> adventurerVisuals =
            new();
        private readonly Dictionary<string, CombatVisualData> enemyVisuals =
            new();
        private readonly StringBuilder combatHistory = new();
        private ExplorationMissionSaveData mission;
        private ExplorationEncounterDefinition encounter;
        private ExplorationTurnBattle battle;
        private Coroutine battleRoutine;
        private bool resolved;

        private void Awake()
        {
            battleButton?.onClick.AddListener(StartBattle);
            retreatButton?.onClick.AddListener(Retreat);
        }

        private void Start()
        {
            InitializeBattle();
        }

        private void OnDestroy()
        {
            battleButton?.onClick.RemoveListener(StartBattle);
            battleButton?.onClick.RemoveListener(ReturnToMap);
            retreatButton?.onClick.RemoveListener(Retreat);
        }

        public void InitializeBattle()
        {
            ExplorationService service =
                GameSessionProvider.Current.Exploration;
            mission = service.GetCurrentBattleMission();
            if (mission == null)
            {
                ShowFatalError(
                    "Nenhuma missão em batalha foi encontrada no save."
                );
                return;
            }

            encounterCatalog ??= Resources.Load<ExplorationEncounterCatalog>(
                ExplorationEncounterCatalog.ResourcesPath
            );
            if (encounterCatalog == null ||
                !encounterCatalog.TryGet(mission.environment, out encounter))
            {
                ShowFatalError(
                    "O destino " +
                    ExplorationProgression.GetDisplayName(mission.environment) +
                    " não possui um encontro configurado no EncounterCatalog."
                );
                return;
            }

            List<ExplorationEnemyDefinition> enemies =
                encounter.RollEnemyParty(unchecked(mission.id * 486187739));
            if (enemies.Count == 0)
            {
                ShowFatalError("Este encontro não possui inimigos configurados.");
                return;
            }

            List<ExplorationCombatantSaveData> party = mission.party
                .Where(member => member != null)
                .Take(ExplorationEncounterDefinition.MaximumTeamSize)
                .ToList();
            if (party.Count == 0)
            {
                ShowFatalError("A missão não possui aventureiros válidos.");
                return;
            }

            BuildVisualLookups(enemies);
            battle = new ExplorationTurnBattle(
                party.Select(CreateAdventurerCombatant),
                enemies.Select(CreateEnemyCombatant),
                mission.id
            );

            battleBackground?.SetSprite(encounter.Background);

            if (environmentTitleText != null)
            {
                environmentTitleText.text =
                    ExplorationProgression.GetDisplayName(mission.environment);
            }

            if (combatLogText != null)
            {
                combatHistory.Clear();
                AppendCombatLog(
                    "A equipe encontrou " + enemies.Count +
                    (enemies.Count == 1 ? " inimigo." : " inimigos.")
                );
            }

            if (rewardsText != null)
            {
                rewardsText.text = BuildPossibleRewardsText();
            }

            BindViews();
            RefreshViews();
            ShowDetails(battle.Adventurers[0]);
            SetBattleControlsReady();
        }

        public void StartBattle()
        {
            if (battle == null || resolved || battleRoutine != null)
            {
                return;
            }

            battleButton.interactable = false;
            battleRoutine = StartCoroutine(RunBattle());
        }

        public void Retreat()
        {
            if (mission == null || resolved)
            {
                return;
            }

            resolved = true;
            if (battleRoutine != null)
            {
                StopCoroutine(battleRoutine);
                battleRoutine = null;
            }

            GameSessionProvider.Current.Exploration.SetStatus(
                mission.id,
                ExplorationMissionStatus.Retreated
            );
            ReturnToMap();
        }

        public void ReturnToMap()
        {
            SceneTransitionController.TryReturnFromExplorationCombat();
        }

        private IEnumerator RunBattle()
        {
            int executedTurns = 0;
            while (!battle.IsFinished && executedTurns < MaximumTurns)
            {
                ExplorationTurnResult result = battle.ExecuteNextTurn();
                executedTurns++;
                WriteTurnLog(result);
                PlayTurnFeedback(result);
                PersistAdventurerHealth();
                RefreshViews();
                yield return new WaitForSecondsRealtime(SecondsBetweenTurns);
            }

            battleRoutine = null;
            if (!battle.IsFinished)
            {
                ShowFatalError(
                    "O combate excedeu o limite de segurança de turnos."
                );
                yield break;
            }

            ResolveBattle();
        }

        private void ResolveBattle()
        {
            resolved = true;
            bool victory = battle.Winner ==
                ExplorationCombatTeam.Adventurers;
            if (victory)
            {
                List<ExplorationRewardSaveData> rewards = RollRewards();
                GameSessionProvider.Current.Exploration.CompleteVictory(
                    mission.id,
                    rewards
                );
                AppendCombatLog("Vitória!");
                AppendCombatLog(
                    "A equipe iniciou o retorno. Tempo estimado: " +
                    ExplorationService.ReturnSeconds + " segundos."
                );
                if (rewardsText != null)
                {
                    rewardsText.text = FormatRewards(rewards);
                }
            }
            else
            {
                GameSessionProvider.Current.Exploration.SetStatus(
                    mission.id,
                    ExplorationMissionStatus.Defeated
                );
                if (combatLogText != null)
                {
                    AppendCombatLog("A equipe foi derrotada.");
                }
            }

            if (retreatButton != null)
            {
                retreatButton.gameObject.SetActive(false);
            }

            battleButton.onClick.RemoveListener(StartBattle);
            battleButton.onClick.AddListener(ReturnToMap);
            battleButton.interactable = true;
            if (battleButtonText != null)
            {
                battleButtonText.text = "Voltar ao mapa";
            }
        }

        private void BindViews()
        {
            BindTeamViews(
                adventurerViews,
                battle.Adventurers,
                adventurerVisuals
            );
            BindTeamViews(enemyViews, battle.Enemies, enemyVisuals);
        }

        private void BindTeamViews(
            ExplorationCombatantView[] views,
            IReadOnlyList<ExplorationBattleCombatant> combatants,
            IReadOnlyDictionary<string, CombatVisualData> visuals
        )
        {
            for (int index = 0; index < (views?.Length ?? 0); index++)
            {
                ExplorationCombatantView view = views[index];
                if (view == null)
                {
                    continue;
                }

                if (index >= combatants.Count)
                {
                    view.gameObject.SetActive(false);
                    continue;
                }

                ExplorationBattleCombatant combatant = combatants[index];
                visuals.TryGetValue(combatant.Id, out CombatVisualData visual);
                view.Bind(
                    combatant,
                    visual.Prefab,
                    visual.FallbackSprite,
                    visual.LocalPosition,
                    visual.LocalEulerAngles,
                    visual.LocalScale,
                    visual.FlipX,
                    ShowDetails
                );
            }
        }

        private void RefreshViews()
        {
            foreach (ExplorationCombatantView view in
                     (adventurerViews ?? Array.Empty<ExplorationCombatantView>())
                     .Concat(enemyViews ??
                         Array.Empty<ExplorationCombatantView>()))
            {
                view?.Refresh();
            }

            if (roundText != null)
            {
                roundText.text = "Rodada " + Math.Max(1, battle.Round);
            }
        }

        private void ShowDetails(ExplorationBattleCombatant combatant)
        {
            if (detailsText == null || combatant == null)
            {
                return;
            }

            string type = combatant.DamageType == DamageType.Magical
                ? "Mágico"
                : "Físico";
            detailsText.text =
                combatant.DisplayName + "\n" +
                "Vida: " + combatant.CurrentHealth + "/" +
                    combatant.MaximumHealth + "\n" +
                "Dano: " + combatant.MinimumDamage + "-" +
                    combatant.MaximumDamage + " (" + type + ")\n" +
                "Velocidade: " + combatant.Speed + "\n" +
                "Armadura: " + combatant.Armor + "\n" +
                "Res. mágica: " + combatant.MagicResistance;
        }

        private void WriteTurnLog(ExplorationTurnResult result)
        {
            if (combatLogText == null)
            {
                return;
            }

            AppendCombatLog(
                result.Attacker.DisplayName + " causou " +
                result.FinalDamage + " de dano em " +
                result.Target.DisplayName + "."
            );
            if (result.TargetDefeated)
            {
                AppendCombatLog(result.Target.DisplayName + " morreu.");
            }
        }

        private void AppendCombatLog(string message)
        {
            if (combatLogText == null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (combatHistory.Length > 0)
            {
                combatHistory.AppendLine();
            }

            combatHistory.Append(message.Trim());
            combatLogText.text = combatHistory.ToString();
            StartCoroutine(ScrollCombatLogToBottom());
        }

        private IEnumerator ScrollCombatLogToBottom()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            if (combatLogScrollRect != null)
            {
                combatLogScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private void PersistAdventurerHealth()
        {
            var health = battle.Adventurers.ToDictionary(
                unit => unit.Id,
                unit => unit.CurrentHealth
            );
            GameSessionProvider.Current.Exploration.UpdatePartyHealth(
                mission.id,
                health
            );
        }

        private List<ExplorationRewardSaveData> RollRewards()
        {
            var random = new System.Random(unchecked(mission.id * 397 + 17));
            var granted = new List<ExplorationRewardSaveData>();

            foreach (ExplorationRewardEntry entry in
                     encounter.PossibleRewards.Where(entry => entry != null))
            {
                int quantity = entry.Roll(random);
                if (quantity <= 0 || entry.Resource == null)
                {
                    continue;
                }

                granted.Add(new ExplorationRewardSaveData
                {
                    resourceId = entry.Resource.PersistenceId,
                    displayName = entry.Resource.DisplayName,
                    quantity = quantity
                });
            }

            return granted;
        }

        private static string FormatRewards(
            IReadOnlyCollection<ExplorationRewardSaveData> rewards
        )
        {
            return rewards == null || rewards.Count == 0
                ? "Nenhum recurso foi encontrado."
                : "Recursos no retorno: " + string.Join(", ", rewards.Select(
                    reward => reward.quantity + "x " + reward.displayName
                ));
        }

        private string BuildPossibleRewardsText()
        {
            string[] names = encounter.PossibleRewards
                .Where(entry => entry?.Resource != null)
                .Select(entry => entry.Resource.DisplayName)
                .Distinct()
                .ToArray();
            return names.Length == 0
                ? "Sem recursos configurados."
                : "Recursos possíveis: " + string.Join(", ", names);
        }

        private void BuildVisualLookups(
            IEnumerable<ExplorationEnemyDefinition> enemies
        )
        {
            adventurerVisuals.Clear();
            foreach (Char_base adventurer in adventurerCatalog ??
                     Array.Empty<Char_base>())
            {
                if (adventurer != null)
                {
                    adventurerVisuals[adventurer.PersistenceId] =
                        new CombatVisualData(
                            adventurer.combatPrefab,
                            adventurer.sprite,
                            adventurer.combatLocalPosition,
                            adventurer.combatLocalEulerAngles,
                            adventurer.combatLocalScale,
                            adventurer.flipCombatVisualX
                        );
                }
            }

            enemyVisuals.Clear();
            foreach (ExplorationEnemyDefinition enemy in enemies)
            {
                enemyVisuals[enemy.PersistenceId] = new CombatVisualData(
                    enemy.CombatPrefab,
                    enemy.Portrait,
                    enemy.CombatLocalPosition,
                    enemy.CombatLocalEulerAngles,
                    enemy.CombatLocalScale,
                    enemy.FlipCombatVisualX
                );
            }
        }

        private void PlayTurnFeedback(ExplorationTurnResult result)
        {
            ExplorationCombatantView attacker = FindView(result.Attacker);
            ExplorationCombatantView target = FindView(result.Target);
            attacker?.PlayAttackFeedback(target);
            target?.PlayHitFeedback();
        }

        private ExplorationCombatantView FindView(
            ExplorationBattleCombatant combatant
        )
        {
            return (adventurerViews ?? Array.Empty<ExplorationCombatantView>())
                .Concat(enemyViews ?? Array.Empty<ExplorationCombatantView>())
                .FirstOrDefault(view =>
                    view != null && ReferenceEquals(view.Combatant, combatant));
        }

        private readonly struct CombatVisualData
        {
            public CombatVisualData(
                GameObject prefab,
                Sprite fallbackSprite,
                Vector3 localPosition,
                Vector3 localEulerAngles,
                Vector3 localScale,
                bool flipX
            )
            {
                Prefab = prefab;
                FallbackSprite = fallbackSprite;
                LocalPosition = localPosition;
                LocalEulerAngles = localEulerAngles;
                LocalScale = localScale == Vector3.zero
                    ? Vector3.one
                    : localScale;
                FlipX = flipX;
            }

            public GameObject Prefab { get; }
            public Sprite FallbackSprite { get; }
            public Vector3 LocalPosition { get; }
            public Vector3 LocalEulerAngles { get; }
            public Vector3 LocalScale { get; }
            public bool FlipX { get; }
        }

        private static ExplorationBattleCombatant CreateAdventurerCombatant(
            ExplorationCombatantSaveData member
        )
        {
            return new ExplorationBattleCombatant(
                member.adventurerId,
                member.displayName,
                ExplorationCombatTeam.Adventurers,
                member.maxHealth,
                member.currentHealth,
                member.minimumDamage,
                member.maximumDamage,
                member.speed,
                member.armor,
                member.magicResistance,
                member.damageType
            );
        }

        private static ExplorationBattleCombatant CreateEnemyCombatant(
            ExplorationEnemyDefinition enemy
        )
        {
            return new ExplorationBattleCombatant(
                enemy.PersistenceId,
                enemy.DisplayName,
                ExplorationCombatTeam.Enemies,
                enemy.MaximumHealth,
                enemy.MaximumHealth,
                enemy.MinimumDamage,
                enemy.MaximumDamage,
                enemy.Speed,
                enemy.Armor,
                enemy.MagicResistance,
                enemy.DamageType
            );
        }

        private void SetBattleControlsReady()
        {
            battleButton.interactable = true;
            if (battleButtonText != null)
            {
                battleButtonText.text = "Batalhar";
            }

            if (retreatButton != null)
            {
                retreatButton.interactable = true;
            }
        }

        private void ShowFatalError(string message)
        {
            Debug.LogError(message, this);
            if (combatLogText != null)
            {
                combatHistory.Clear();
                AppendCombatLog(message);
            }

            if (battleButton != null)
            {
                battleButton.interactable = false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            adventurerCatalog = AssetDatabase.FindAssets("t:Char_base")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Char_base>)
                .Where(definition => definition != null)
                .OrderBy(definition => definition.name)
                .ToArray();

            if (adventurerViews != null &&
                adventurerViews.Length >
                ExplorationEncounterDefinition.MaximumTeamSize)
            {
                Array.Resize(
                    ref adventurerViews,
                    ExplorationEncounterDefinition.MaximumTeamSize
                );
            }

            if (enemyViews != null &&
                enemyViews.Length >
                ExplorationEncounterDefinition.MaximumTeamSize)
            {
                Array.Resize(
                    ref enemyViews,
                    ExplorationEncounterDefinition.MaximumTeamSize
                );
            }
        }
#endif
    }
}
