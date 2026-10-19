using UnityEngine;

/// <summary>
/// Escolhe a animação do Player usando a velocidade real do Rigidbody2D.
///
/// Estados esperados no Animator Controller:
/// - idle_up, idle_right e idle_down;
/// - walk_up, walk_right e walk_down.
///
/// Não são necessários estados exclusivos para a esquerda: idle_right e
/// walk_right são reutilizados com SpriteRenderer.flipX ativado.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class PlayerAnimationController : MonoBehaviour
{
    private const string BaseLayer = "Base Layer.";
    private static readonly int IdleUp = Animator.StringToHash(
        BaseLayer + "idle_up"
    );
    private static readonly int IdleRight = Animator.StringToHash(
        BaseLayer + "idle_right"
    );
    private static readonly int IdleDown = Animator.StringToHash(
        BaseLayer + "idle_down"
    );
    private static readonly int WalkUp = Animator.StringToHash(
        BaseLayer + "walk_up"
    );
    private static readonly int WalkRight = Animator.StringToHash(
        BaseLayer + "walk_right"
    );
    private static readonly int WalkDown = Animator.StringToHash(
        BaseLayer + "walk_down"
    );

    [Header("Referências (preenchidas automaticamente)")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Configuração")]
    [Tooltip("Velocidade mínima para considerar que o Player está andando.")]
    [SerializeField, Min(0f)] private float movementThreshold = 0.01f;
    [Tooltip("Direção usada antes do primeiro movimento. O padrão é para baixo.")]
    [SerializeField] private Vector2 initialFacingDirection = Vector2.down;
    [Tooltip("Suavização curta ao trocar entre os estados do Animator.")]
    [SerializeField, Min(0f)] private float transitionDuration = 0.05f;

    private Vector2 lastFacingDirection;
    private int currentStateHash;
    private int missingStateWarningHash;

    private void Awake()
    {
        ResolveReferences();
        lastFacingDirection = ToCardinalDirection(
            initialFacingDirection == Vector2.zero
                ? Vector2.down
                : initialFacingDirection
        );

        if (animator != null)
        {
            // As animações apenas trocam sprites; nunca devem movimentar o
            // GameObject através de Root Motion.
            animator.applyRootMotion = false;
        }
    }

    private void OnEnable()
    {
        currentStateHash = 0;
    }

    private void LateUpdate()
    {
        if (animator == null || body == null || spriteRenderer == null)
        {
            return;
        }

        Vector2 currentVelocity = body.linearVelocity;
        bool isMoving = currentVelocity.sqrMagnitude >
            movementThreshold * movementThreshold;

        // A direção só é substituída enquanto existe movimento. Quando o Player
        // para, esse valor permanece e seleciona o idle da última direção olhada.
        if (isMoving)
        {
            lastFacingDirection = ToCardinalDirection(currentVelocity);
        }

        bool isHorizontal = Mathf.Abs(lastFacingDirection.x) > 0.5f;

        // Esquerda usa os mesmos clipes da direita, apenas espelhados.
        // Ao olhar para cima/baixo o flip é removido para não inverter esses clipes.
        spriteRenderer.flipX = isHorizontal && lastFacingDirection.x < 0f;

        int targetState = SelectState(isMoving, lastFacingDirection);
        PlayStateIfChanged(targetState);
    }

    private int SelectState(bool isMoving, Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0.5f)
        {
            return isMoving ? WalkRight : IdleRight;
        }

        if (direction.y > 0f)
        {
            return isMoving ? WalkUp : IdleUp;
        }

        return isMoving ? WalkDown : IdleDown;
    }

    private void PlayStateIfChanged(int targetStateHash)
    {
        if (targetStateHash == currentStateHash)
        {
            return;
        }

        if (!animator.HasState(0, targetStateHash))
        {
            if (missingStateWarningHash != targetStateHash)
            {
                Debug.LogWarning(
                    "O Animator do Player não contém um dos estados esperados: " +
                    "idle_up, idle_right, idle_down, walk_up, walk_right ou " +
                    "walk_down.",
                    this
                );
                missingStateWarningHash = targetStateHash;
            }

            return;
        }

        animator.CrossFadeInFixedTime(
            targetStateHash,
            transitionDuration,
            0
        );
        currentStateHash = targetStateHash;
        missingStateWarningHash = 0;
    }

    /// <summary>
    /// Converte uma velocidade diagonal em uma das quatro direções principais.
    /// O eixo com maior intensidade determina qual animação será apresentada.
    /// </summary>
    private static Vector2 ToCardinalDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x < 0f ? Vector2.left : Vector2.right;
        }

        return direction.y < 0f ? Vector2.down : Vector2.up;
    }

    private void ResolveReferences()
    {
        animator ??= GetComponent<Animator>();
        body ??= GetComponent<Rigidbody2D>();
        spriteRenderer ??= GetComponent<SpriteRenderer>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ResolveReferences();
        movementThreshold = Mathf.Max(0f, movementThreshold);
        transitionDuration = Mathf.Max(0f, transitionDuration);
    }
#endif
}
