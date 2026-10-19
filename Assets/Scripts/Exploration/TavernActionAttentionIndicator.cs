using TCCGame.Core;
using UnityEngine;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Drives the tavern's attention icon. Attach it to the empty parent that
    /// contains AttentionExclamation, not to the animated child itself.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TavernActionAttentionIndicator : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private GameObject attentionVisual;
        [SerializeField] private Animator attentionAnimator;

        [Header("Exploration loot")]
        [SerializeField] private ExplorationEncounterCatalog encounterCatalog;
        [Tooltip("Time in seconds for workers to generate another loot batch. Default: 5 minutes.")]
        [SerializeField, Min(1)] private int explorerLootIntervalSeconds =
            ExplorationService.ExplorerCollectionSeconds;

        [Header("Attention playback")]
        [Tooltip("While an action is pending, replay the one-shot alert at this interval.")]
        [SerializeField, Min(0.25f)] private float replayIntervalSeconds = 2f;

        private ExplorationService service;
        private bool isVisible;
        private float nextReplayTime;

        private void Reset()
        {
            attentionAnimator = GetComponentInChildren<Animator>(true);
            attentionVisual = attentionAnimator != null
                ? attentionAnimator.gameObject
                : null;
            encounterCatalog = Resources.Load<ExplorationEncounterCatalog>(
                ExplorationEncounterCatalog.ResourcesPath
            );
        }

        private void Awake()
        {
            ResolveReferences();
            SetVisible(false, true);
        }

        private void Update()
        {
            service ??= GameSessionProvider.Current.Exploration;
            service.SetExplorerCollectionIntervalSeconds(
                explorerLootIntervalSeconds
            );

            // The component is present in the tavern scene, so it keeps these
            // time-based states moving even with the action panel closed.
            service.RefreshArrivals();
            service.RefreshExplorerCollections(encounterCatalog);
            SetVisible(service.HasTavernActionPending, false);
        }

        private void SetVisible(bool shouldShow, bool force)
        {
            if (attentionVisual == null)
            {
                return;
            }

            if (!shouldShow)
            {
                if (force || isVisible)
                {
                    attentionVisual.SetActive(false);
                    isVisible = false;
                }
                return;
            }

            if (force || !isVisible || Time.unscaledTime >= nextReplayTime)
            {
                attentionVisual.SetActive(false);
                attentionVisual.SetActive(true);
                attentionAnimator ??= attentionVisual.GetComponent<Animator>();
                if (attentionAnimator != null)
                {
                    attentionAnimator.Rebind();
                    attentionAnimator.Play("AttentionLoop", 0, 0f);
                }

                nextReplayTime = Time.unscaledTime + replayIntervalSeconds;
                isVisible = true;
            }
        }

        private void ResolveReferences()
        {
            if (attentionVisual == null)
            {
                attentionAnimator = GetComponentInChildren<Animator>(true);
                attentionVisual = attentionAnimator != null
                    ? attentionAnimator.gameObject
                    : null;
            }
            else if (attentionAnimator == null)
            {
                attentionAnimator = attentionVisual.GetComponent<Animator>();
            }

            encounterCatalog ??= Resources.Load<ExplorationEncounterCatalog>(
                ExplorationEncounterCatalog.ResourcesPath
            );
        }

        private void OnValidate()
        {
            explorerLootIntervalSeconds = Mathf.Max(1, explorerLootIntervalSeconds);
            replayIntervalSeconds = Mathf.Max(0.25f, replayIntervalSeconds);
        }

        public void Configure(
            GameObject visual,
            ExplorationEncounterCatalog catalog
        )
        {
            attentionVisual = visual;
            attentionAnimator = visual != null
                ? visual.GetComponent<Animator>()
                : null;
            encounterCatalog = catalog;
        }
    }
}
