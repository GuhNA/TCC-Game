using UnityEngine;

public class Aventureiro : MonoBehaviour, IInteractable, IInteractionExitAware
{
    private const int DefaultPlayerSortingOrder = 1000;
    private const float HeightComparisonTolerance = 0.01f;

    public Char_base charBase;

    [Header("Ordenação em relação ao Player")]
    [Tooltip("Renderer cuja Order in Layer será ajustada pela altura. Se vazio, usa o SpriteRenderer deste objeto.")]
    [SerializeField] private SpriteRenderer sortingRenderer;

    [Tooltip("Order in Layer usada enquanto o Player ainda não foi encontrado.")]
    [SerializeField] private int fallbackPlayerSortingOrder =
        DefaultPlayerSortingOrder;

    public AdventurerRuntimeState RuntimeState { get; private set; }

    public AdventurerStats Stats => RuntimeState?.Stats ?? default;

    public bool IsBeingInspected { get; private set; }

    public bool CanInteract => enabled && charBase != null;

    private Transform playerTransform;
    private SpriteRenderer playerRenderer;

    private void Awake()
    {
        sortingRenderer ??= GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Configure(charBase);
        FindPlayer();
        RefreshSortingOrder();
    }

    private void LateUpdate()
    {
        // LateUpdate garante que a comparação seja feita depois do movimento
        // do Player no frame atual, evitando um quadro de atraso visual.
        if (playerTransform == null)
        {
            FindPlayer();
        }

        RefreshSortingOrder();
    }

    /// <summary>
    /// Reconfigura esta instância para representar outro aventureiro.
    /// O spawner da taverna usa este método ao sortear uma nova oferta; assim
    /// os atributos usados pela janela de recrutamento e o sprite visível são
    /// atualizados juntos, sem manter dados do aventureiro anterior.
    /// </summary>
    public void Configure(Char_base definition)
    {
        charBase = definition;
        if (definition == null)
        {
            RuntimeState = null;
            enabled = false;
            return;
        }

        enabled = true;
        RuntimeState = new AdventurerRuntimeState(
            AdventurerStatsCalculator.Calculate(definition)
        );

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && definition.sprite != null)
        {
            spriteRenderer.sprite = definition.sprite;
        }

        sortingRenderer ??= spriteRenderer;
    }

    public void Interact(GameObject player)
    {
        player.GetComponent<AdventurerRecruitmentController>()?.Open(this);
    }

    public void OnExitInteractionRange(GameObject player)
    {
        player.GetComponent<AdventurerRecruitmentController>()?.Close(this);
    }

    public void SetBeingInspected(bool value)
    {
        IsBeingInspected = value;
    }

    private void FindPlayer()
    {
        moviment playerMovement = FindFirstObjectByType<moviment>();
        if (playerMovement == null)
        {
            playerTransform = null;
            playerRenderer = null;
            return;
        }

        playerTransform = playerMovement.transform;
        playerRenderer = playerMovement.GetComponentInChildren<
            SpriteRenderer>();
    }

    private void RefreshSortingOrder()
    {
        if (sortingRenderer == null || playerTransform == null)
        {
            return;
        }

        int playerOrder = playerRenderer != null
            ? playerRenderer.sortingOrder
            : fallbackPlayerSortingOrder;
        // A profundidade visual deve ser decidida pela posição dos pés, não
        // pelo pivô do objeto (que pode estar no centro ou até na cabeça).
        float adventurerFeetY = GetFeetWorldY(
            sortingRenderer,
            transform
        );
        float playerFeetY = GetFeetWorldY(
            playerRenderer,
            playerTransform
        );
        float heightDifference = adventurerFeetY - playerFeetY;

        // Em um cenário 2D visto de cima, quem está mais abaixo precisa ser
        // desenhado por cima. Exemplo com Player em 1000:
        // aventureiro abaixo = 1001; acima = 999; mesma altura = 1000.
        if (heightDifference < -HeightComparisonTolerance)
        {
            sortingRenderer.sortingOrder = playerOrder + 1;
        }
        else if (heightDifference > HeightComparisonTolerance)
        {
            sortingRenderer.sortingOrder = playerOrder - 1;
        }
        else
        {
            sortingRenderer.sortingOrder = playerOrder;
        }
    }

    /// <summary>
    /// O limite inferior do SpriteRenderer representa os pés no mundo. Caso o
    /// renderer ainda não esteja disponível, o Transform permanece como fallback
    /// para não interromper a ordenação durante a inicialização da cena.
    /// </summary>
    private static float GetFeetWorldY(
        SpriteRenderer renderer,
        Transform fallbackTransform
    )
    {
        if (renderer != null && renderer.sprite != null)
        {
            return renderer.bounds.min.y;
        }

        return fallbackTransform != null
            ? fallbackTransform.position.y
            : 0f;
    }

}
