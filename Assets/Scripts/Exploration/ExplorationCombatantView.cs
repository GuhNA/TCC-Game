using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// World-space presentation for one combatant. The gameplay data remains in
    /// ExplorationTurnBattle; this component only owns the instantiated visual,
    /// name, health bar and click feedback.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ExplorationCombatantView : MonoBehaviour,
        IPointerClickHandler
    {
        [Header("World objects")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer fallbackRenderer;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Transform healthFill;
        [SerializeField, Min(0.1f)] private float fallbackTargetHeight = 3f;

        [Header("Feedback")]
        [SerializeField, Min(0f)] private float attackStepDistance = 0.25f;
        [SerializeField, Min(0.01f)] private float feedbackDuration = 0.12f;

        private ExplorationBattleCombatant combatant;
        private Action<ExplorationBattleCombatant> inspectRequested;
        private GameObject instantiatedVisual;
        private Vector3 healthFillInitialScale;
        private Coroutine feedbackRoutine;

        public ExplorationBattleCombatant Combatant => combatant;

        private void Awake()
        {
            if (healthFill != null)
            {
                healthFillInitialScale = healthFill.localScale;
            }
        }

        public void Bind(
            ExplorationBattleCombatant value,
            GameObject combatPrefab,
            Sprite fallbackSprite,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale,
            bool flipX,
            Action<ExplorationBattleCombatant> onInspectRequested
        )
        {
            ClearVisual();
            combatant = value;
            inspectRequested = onInspectRequested;
            gameObject.SetActive(value != null);
            if (value == null)
            {
                return;
            }

            if (nameText != null)
            {
                nameText.text = value.DisplayName;
            }

            Transform parent = visualRoot != null ? visualRoot : transform;
            if (combatPrefab != null)
            {
                instantiatedVisual = Instantiate(combatPrefab, parent);
                instantiatedVisual.name = combatPrefab.name + " (Combat Visual)";
                instantiatedVisual.transform.localPosition = localPosition;
                instantiatedVisual.transform.localRotation =
                    Quaternion.Euler(localEulerAngles);
                instantiatedVisual.transform.localScale = SafeScale(localScale);
                ApplyHorizontalFlip(instantiatedVisual.transform, flipX);
                if (fallbackRenderer != null)
                {
                    fallbackRenderer.enabled = false;
                }
            }
            else if (fallbackRenderer != null)
            {
                fallbackRenderer.enabled = fallbackSprite != null;
                fallbackRenderer.sprite = fallbackSprite;
                fallbackRenderer.transform.localPosition = localPosition;
                fallbackRenderer.transform.localRotation =
                    Quaternion.Euler(localEulerAngles);
                FitFallbackSprite(fallbackRenderer, SafeScale(localScale), flipX);
            }

            Refresh();
        }

        public void Refresh()
        {
            if (combatant == null)
            {
                return;
            }

            if (healthText != null)
            {
                healthText.text = combatant.CurrentHealth + "/" +
                    combatant.MaximumHealth;
            }

            float healthRatio = combatant.MaximumHealth <= 0
                ? 0f
                : Mathf.Clamp01(
                    combatant.CurrentHealth / (float)combatant.MaximumHealth
                );
            if (healthFill != null)
            {
                healthFill.localScale = new Vector3(
                    healthFillInitialScale.x * healthRatio,
                    healthFillInitialScale.y,
                    healthFillInitialScale.z
                );
            }

            SetVisualAlpha(combatant.IsAlive ? 1f : 0.32f);
        }

        public void PlayAttackFeedback(ExplorationCombatantView target)
        {
            Vector3 direction = target == null
                ? Vector3.right
                : (target.transform.position - transform.position).normalized;
            StartFeedback(direction * attackStepDistance, Color.white);
        }

        public void PlayHitFeedback()
        {
            StartFeedback(Vector3.zero, new Color(1f, 0.35f, 0.35f, 1f));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            inspectRequested?.Invoke(combatant);
        }

        private void StartFeedback(Vector3 offset, Color flashColor)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(FeedbackRoutine(offset, flashColor));
        }

        private IEnumerator FeedbackRoutine(Vector3 offset, Color flashColor)
        {
            Transform animatedRoot = visualRoot != null ? visualRoot : transform;
            Vector3 start = animatedRoot.localPosition;
            Color[] originalColors = CaptureRendererColors(animatedRoot);
            TintRenderers(animatedRoot, flashColor);
            animatedRoot.localPosition = start + offset;
            yield return new WaitForSecondsRealtime(feedbackDuration);
            animatedRoot.localPosition = start;
            RestoreRendererColors(animatedRoot, originalColors);
            feedbackRoutine = null;
        }

        private void ClearVisual()
        {
            if (instantiatedVisual != null)
            {
                Destroy(instantiatedVisual);
                instantiatedVisual = null;
            }
        }

        private void FitFallbackSprite(
            SpriteRenderer renderer,
            Vector3 requestedScale,
            bool flipX
        )
        {
            float spriteHeight = renderer.sprite == null
                ? 1f
                : Mathf.Max(0.01f, renderer.sprite.bounds.size.y);
            float fit = fallbackTargetHeight / spriteHeight;
            renderer.transform.localScale = new Vector3(
                requestedScale.x * fit * (flipX ? -1f : 1f),
                requestedScale.y * fit,
                requestedScale.z
            );
        }

        private static Vector3 SafeScale(Vector3 value)
        {
            return value == Vector3.zero ? Vector3.one : value;
        }

        private static void ApplyHorizontalFlip(Transform target, bool flipX)
        {
            if (!flipX)
            {
                return;
            }

            Vector3 scale = target.localScale;
            scale.x *= -1f;
            target.localScale = scale;
        }

        private void SetVisualAlpha(float alpha)
        {
            Transform root = visualRoot != null ? visualRoot : transform;
            foreach (SpriteRenderer renderer in
                     root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                Color color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }
        }

        private static Color[] CaptureRendererColors(Transform root)
        {
            SpriteRenderer[] renderers =
                root.GetComponentsInChildren<SpriteRenderer>(true);
            var colors = new Color[renderers.Length];
            for (int index = 0; index < renderers.Length; index++)
            {
                colors[index] = renderers[index].color;
            }

            return colors;
        }

        private static void TintRenderers(Transform root, Color tint)
        {
            foreach (SpriteRenderer renderer in
                     root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.color *= tint;
            }
        }

        private static void RestoreRendererColors(
            Transform root,
            Color[] colors
        )
        {
            SpriteRenderer[] renderers =
                root.GetComponentsInChildren<SpriteRenderer>(true);
            for (int index = 0;
                 index < renderers.Length && index < colors.Length;
                 index++)
            {
                renderers[index].color = colors[index];
            }
        }
    }
}
