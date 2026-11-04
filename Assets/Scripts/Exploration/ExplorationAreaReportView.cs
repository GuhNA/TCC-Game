using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>Displays the live state of the selected area in the authored report.</summary>
    public sealed class ExplorationAreaReportView : MonoBehaviour
    {
        [Serializable]
        private sealed class GuardSlot
        {
            public GameObject root;
            public Image portrait;
            public TMP_Text nameText;
            public TMP_Text healthText;
        }

        private sealed class RewardRow
        {
            public string resourceId;
            public long maximumPerExplorer;
            public ExplorationRewardItemView view;
        }

        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private TMP_Text locationText;
        [SerializeField] private TMP_Text explorerCountText;
        [SerializeField] private ScrollRect rewardScroll;
        [SerializeField] private ExplorationRewardItemView itemPrefab;
        [SerializeField] private GuardSlot[] guardSlots = Array.Empty<GuardSlot>();
        [SerializeField] private GameObject attackOn;
        [SerializeField] private GameObject noAttack;
        [SerializeField] private TMP_Text attackTimeText;
        [SerializeField] private Button assignButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Button transportButton;
        [SerializeField] private Button guardButton;
        [SerializeField] private Button backButton;

        private readonly List<RewardRow> rows = new();
        private readonly Dictionary<string, Char_base> definitions = new();
        private ExplorationMapController controller;
        private ExplorationService service;
        private ExplorationEnvironment environment;
        private float nextRefresh;

        public bool IsVisible => gameObject.activeInHierarchy;

        public void Initialize(ExplorationMapController owner)
        {
            controller = owner;
            foreach (TMP_Text label in GetComponentsInChildren<TMP_Text>(true))
            {
                if (font != null)
                {
                    label.font = font;
                    label.fontSharedMaterial = font.material;
                }
            }
        }

        private void OnEnable()
        {
            assignButton?.onClick.AddListener(AssignExplorer);
            removeButton?.onClick.AddListener(RemoveExplorer);
            transportButton?.onClick.AddListener(TransportLoot);
            guardButton?.onClick.AddListener(SelectGuards);
            backButton?.onClick.AddListener(Back);
        }

        private void OnDisable()
        {
            assignButton?.onClick.RemoveListener(AssignExplorer);
            removeButton?.onClick.RemoveListener(RemoveExplorer);
            transportButton?.onClick.RemoveListener(TransportLoot);
            guardButton?.onClick.RemoveListener(SelectGuards);
            backButton?.onClick.RemoveListener(Back);
        }

        public void Show(
            ExplorationService exploration,
            ExplorationEnvironment selectedEnvironment,
            ExplorationEncounterCatalog encounters,
            IEnumerable<Char_base> adventurers)
        {
            bool resetScroll = !IsVisible || environment != selectedEnvironment;
            service = exploration;
            environment = selectedEnvironment;
            definitions.Clear();
            foreach (Char_base definition in adventurers ?? Array.Empty<Char_base>())
            {
                if (definition != null)
                {
                    definitions[definition.PersistenceId] = definition;
                }
            }

            locationText.text = ExplorationProgression.GetDisplayName(environment);
            RebuildRewards(encounters);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rewardScroll.content);
            if (resetScroll)
            {
                rewardScroll.StopMovement();
                rewardScroll.verticalNormalizedPosition = 1f;
            }
        }

        public void Hide() => gameObject.SetActive(false);

        private void LateUpdate()
        {
            if (service != null && Time.unscaledTime >= nextRefresh)
            {
                Refresh();
                nextRefresh = Time.unscaledTime + 0.2f;
            }
        }

        private void RebuildRewards(ExplorationEncounterCatalog encounters)
        {
            foreach (RewardRow row in rows)
            {
                row.view.gameObject.SetActive(false);
                Destroy(row.view.gameObject);
            }
            rows.Clear();

            if (encounters == null || !encounters.TryGet(environment, out var encounter))
            {
                return;
            }

            foreach (var group in (encounter.PossibleRewards ??
                         Array.Empty<ExplorationRewardEntry>())
                     .Where(entry => entry?.Resource != null)
                     .GroupBy(entry => entry.Resource.PersistenceId))
            {
                ExplorationRewardItemView item = Instantiate(itemPrefab, rewardScroll.content);
                item.Bind(group.First().Resource, font);
                rows.Add(new RewardRow
                {
                    resourceId = group.Key,
                    maximumPerExplorer = group.Where(entry => entry.DropChance > 0f)
                        .Sum(entry => (long)Math.Max(1,
                            Math.Max(entry.MinimumQuantity, entry.MaximumQuantity))),
                    view = item
                });
            }
        }

        public void Refresh()
        {
            if (service == null)
            {
                return;
            }

            ExplorationAreaSaveData area = service.GetArea(environment);
            int workers = area != null && area.captured
                ? Math.Max(0, area.assignedWorkers) : 0;
            explorerCountText.text = $"{workers} / {service.AvailableExplorers}";
            foreach (RewardRow row in rows)
            {
                long collected = area?.pendingWorkerLoot?
                    .Where(reward => reward != null && reward.resourceId == row.resourceId)
                    .Sum(reward => (long)Math.Max(0, reward.quantity)) ?? 0;
                row.view.SetQuantities(collected, row.maximumPerExplorer * workers);
            }

            var guards = area != null && area.captured
                ? area.guardParty?.Where(member => member != null).ToArray()
                : null;
            for (int index = 0; index < guardSlots.Length; index++)
            {
                GuardSlot slot = guardSlots[index];
                bool occupied = guards != null && index < guards.Length;
                slot.root.SetActive(occupied);
                if (!occupied)
                {
                    continue;
                }

                ExplorationCombatantSaveData member = guards[index];
                string definitionId = string.IsNullOrWhiteSpace(member.definitionId)
                    ? member.adventurerId : member.definitionId;
                Char_base definition = null;
                if (!string.IsNullOrEmpty(definitionId))
                {
                    definitions.TryGetValue(definitionId, out definition);
                }
                slot.portrait.sprite = definition != null ? definition.sprite : null;
                slot.portrait.enabled = slot.portrait.sprite != null;
                slot.portrait.preserveAspect = true;
                slot.nameText.text = string.IsNullOrWhiteSpace(member.displayName)
                    ? definition != null ? definition.name : "Aventureiro"
                    : member.displayName;
                slot.healthText.text = $"{Math.Max(0, member.currentHealth)}/{member.maxHealth}";
            }

            bool threatened = area != null && area.captured &&
                (area.enemyAttackPending || area.nextEnemyAttackUnixSeconds > 0);
            attackOn.SetActive(threatened);
            noAttack.SetActive(!threatened);
            long remaining = service.GetSecondsUntilEnemyAttack(environment);
            attackTimeText.text = $"{remaining / 60:00}:{remaining % 60:00}";

            bool captured = area?.captured == true;
            assignButton.interactable = captured && service.AvailableExplorers > 0;
            removeButton.interactable = captured && workers > 0;
            transportButton.interactable = area?.pendingWorkerLoot?.Any(
                reward => reward != null && reward.quantity > 0) == true;
            guardButton.interactable = captured &&
                service.GetActiveMissionForEnvironment(environment) == null;
        }

        private void AssignExplorer() => controller?.AssignExplorer();
        private void RemoveExplorer() => controller?.RemoveExplorer();
        private void TransportLoot() => controller?.TransportExplorerLoot();
        private void SelectGuards() => controller?.BeginGuardSelection();
        private void Back() => controller?.ClosePartySelection();
    }
}
