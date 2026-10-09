using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        [Tooltip("Sprite colorido chamado Full, exibido sobre o Empty.")]
        [SerializeField] private SpriteRenderer healthFullRenderer;
        [Tooltip("Sprite cinza chamado Empty, exibido abaixo do Full.")]
        [SerializeField] private SpriteRenderer healthEmptyRenderer;
        [Tooltip("Compatibilidade com a barra antiga criada por escala.")]
        [SerializeField] private Transform healthFill;
        [Tooltip(
            "Marcado: a parte cinza aparece da esquerda para a direita."
        )]
        [SerializeField] private bool drainHealthFromLeftToRight = true;
        [SerializeField, Min(0.1f)] private float fallbackTargetHeight = 3f;

        [Header("Feedback")]
        [SerializeField, Min(0f)] private float attackStepDistance = 0.25f;
        [SerializeField, Min(0.01f)] private float feedbackDuration = 0.12f;

        private ExplorationBattleCombatant combatant;
        private Action<ExplorationBattleCombatant> inspectRequested;
        private GameObject instantiatedVisual;
        private Vector3 healthFillInitialScale;
        private Sprite healthFullInitialSprite;
        private Canvas healthBarCanvas;
        private Image healthEmptyImage;
        private Image healthFullImage;
        private Coroutine feedbackRoutine;

        public ExplorationBattleCombatant Combatant => combatant;

        private void Awake()
        {
            ResolveHealthBarReferences();
            CacheHealthBarInitialState();
            CreateCanvasHealthBar();
        }

        /// <summary>
        /// Localiza automaticamente a nova estrutura Health Bar/Health/Full/Empty.
        /// Assim cada um dos seis slots não precisa ser ligado manualmente no
        /// Inspector sempre que o visual da barra for trocado.
        /// </summary>
        private void ResolveHealthBarReferences()
        {
            Transform health = FindDescendant("Health");
            Transform full = FindDescendant("Full");
            Transform empty = FindDescendant("Empty");

            if (healthText == null && health != null)
            {
                healthText = health.GetComponent<TMP_Text>();
            }

            if (healthFullRenderer == null && full != null)
            {
                healthFullRenderer = full.GetComponent<SpriteRenderer>();
            }

            if (healthEmptyRenderer == null && empty != null)
            {
                healthEmptyRenderer = empty.GetComponent<SpriteRenderer>();
            }

            // Mantém projetos/cenas antigos funcionando. Quando existe o novo
            // sprite Full, ele passa a ser o preenchimento oficial da barra.
            if (healthFullRenderer != null)
            {
                healthFill = healthFullRenderer.transform;
            }

            // Garante que a parte cheia seja desenhada sobre a imagem cinza.
            if (healthFullRenderer != null && healthEmptyRenderer != null &&
                healthFullRenderer.sortingLayerID ==
                healthEmptyRenderer.sortingLayerID)
            {
                healthFullRenderer.sortingOrder = Mathf.Max(
                    healthFullRenderer.sortingOrder,
                    healthEmptyRenderer.sortingOrder + 1
                );
            }
        }

        private void CacheHealthBarInitialState()
        {
            if (healthFill != null)
            {
                healthFillInitialScale = healthFill.localScale;
            }

            if (healthFullRenderer != null)
            {
                healthFullInitialSprite = healthFullRenderer.sprite;
                healthFullRenderer.maskInteraction =
                    SpriteMaskInteraction.None;
            }
        }

        /// <summary>
        /// Constrói uma pequena Canvas em World Space exatamente sobre as barras
        /// existentes. O tamanho em pixels é convertido para unidades do mundo
        /// usando Pixels Per Unit, preservando posição, rotação e escala atuais.
        /// bars_0 vira o fundo cinza e bars_2 vira uma Image Filled por cima.
        /// </summary>
        private void CreateCanvasHealthBar()
        {
            if (healthBarCanvas != null ||
                healthFullRenderer == null ||
                healthEmptyRenderer == null ||
                healthFullInitialSprite == null ||
                healthEmptyRenderer.sprite == null)
            {
                return;
            }

            Sprite emptySprite = healthEmptyRenderer.sprite;
            Transform source = healthEmptyRenderer.transform;
            float pixelsPerUnit = Mathf.Max(
                1f,
                emptySprite.pixelsPerUnit
            );

            var canvasObject = new GameObject(
                "Health Bar Canvas (Runtime)",
                typeof(RectTransform),
                typeof(Canvas)
            );
            RectTransform canvasRect =
                canvasObject.GetComponent<RectTransform>();
            canvasRect.SetParent(source.parent, false);
            canvasRect.localPosition = source.localPosition;
            canvasRect.localRotation = source.localRotation;
            canvasRect.localScale = new Vector3(
                source.localScale.x / pixelsPerUnit,
                source.localScale.y / pixelsPerUnit,
                1f
            );
            canvasRect.sizeDelta = new Vector2(
                emptySprite.rect.width,
                emptySprite.rect.height
            );
            canvasRect.pivot = new Vector2(0.5f, 0.5f);

            healthBarCanvas = canvasObject.GetComponent<Canvas>();
            healthBarCanvas.renderMode = RenderMode.WorldSpace;
            healthBarCanvas.overrideSorting = true;
            healthBarCanvas.sortingLayerID =
                healthEmptyRenderer.sortingLayerID;
            healthBarCanvas.sortingOrder = Mathf.Max(
                healthEmptyRenderer.sortingOrder,
                healthFullRenderer.sortingOrder
            );
            healthBarCanvas.pixelPerfect = false;

            healthEmptyImage = CreateHealthImage(
                canvasRect,
                "Empty (bars_0)",
                emptySprite,
                healthEmptyRenderer.color
            );
            healthFullImage = CreateHealthImage(
                canvasRect,
                "Full (bars_2)",
                healthFullInitialSprite,
                healthFullRenderer.color
            );
            healthFullImage.type = Image.Type.Filled;
            healthFullImage.fillMethod = Image.FillMethod.Horizontal;
            healthFullImage.fillOrigin = drainHealthFromLeftToRight
                ? (int)Image.OriginHorizontal.Right
                : (int)Image.OriginHorizontal.Left;
            healthFullImage.fillAmount = 1f;

            // Os SpriteRenderers continuam como referências para os assets e a
            // posição, mas deixam de ser desenhados. Apenas as Images da Canvas
            // passam a representar visualmente a barra.
            healthEmptyRenderer.enabled = false;
            healthFullRenderer.enabled = false;

            // Mantém o texto numérico acima da nova Canvas sem alterar sua
            // posição, tamanho, fonte ou qualquer outra propriedade visual.
            Renderer healthTextRenderer = healthText == null
                ? null
                : healthText.GetComponent<Renderer>();
            if (healthTextRenderer != null)
            {
                healthTextRenderer.sortingLayerID =
                    healthBarCanvas.sortingLayerID;
                healthTextRenderer.sortingOrder = Mathf.Max(
                    healthTextRenderer.sortingOrder,
                    healthBarCanvas.sortingOrder + 1
                );
            }
        }

        private static Image CreateHealthImage(
            RectTransform parent,
            string objectName,
            Sprite sprite,
            Color color
        )
        {
            var imageObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = false;
            return image;
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
            UpdateHealthBar(healthRatio);

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

        /// <summary>
        /// Atualiza diretamente o Fill Amount da bars_2. A bars_0 permanece
        /// inteira abaixo dela e aparece gradualmente conforme a vida diminui.
        /// </summary>
        private void UpdateHealthBar(float healthRatio)
        {
            healthRatio = Mathf.Clamp01(healthRatio);
            if (healthFullImage != null && healthEmptyImage != null)
            {
                healthEmptyImage.enabled = true;
                healthFullImage.enabled = healthRatio > 0.0001f;
                healthFullImage.fillOrigin = drainHealthFromLeftToRight
                    ? (int)Image.OriginHorizontal.Right
                    : (int)Image.OriginHorizontal.Left;
                healthFullImage.fillAmount = healthRatio;
                return;
            }

            // Compatibilidade com uma cena antiga que não tenha os dois sprites
            // necessários para criar a Canvas.
            if (healthFill != null)
            {
                Vector3 scaled = healthFillInitialScale;
                scaled.x *= healthRatio;
                healthFill.localScale = scaled;
            }
        }

        private Transform FindDescendant(string objectName)
        {
            foreach (Transform descendant in
                     GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(
                    descendant.name,
                    objectName,
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    return descendant;
                }
            }

            return null;
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveHealthBarReferences();
        }
#endif
    }
}
