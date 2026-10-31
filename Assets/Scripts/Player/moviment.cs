using System.Collections.Generic;
using TCCGame.Core.Input;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class moviment : MonoBehaviour
{
    private const float FeetComparisonTolerance = 0.01f;

    private Vector2 movementInput;
    private Rigidbody2D rig;
    private Collider2D playerCollider;
    private bool movementBlocked;

    [Header("Ordenação dos NPCs")]
    [Tooltip(
        "Todo personagem ativo com esta tag terá a ordem comparada com os " +
        "pés do Player. O SpriteRenderer pode estar no objeto ou em um filho."
    )]
    [SerializeField] private string sortableNpcTag = "NPC";

    [Tooltip("Intervalo para localizar NPCs novos ou instanciados na cena.")]
    [SerializeField, Min(0.1f)] private float npcRefreshInterval = 0.5f;

    private readonly List<SpriteRenderer> sortableNpcRenderers = new();
    private SpriteRenderer playerSortingRenderer;
    private float nextNpcRefreshTime;
    private bool invalidNpcTagReported;

    public float velocity;

    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        playerSortingRenderer = FindPrimaryRenderer(gameObject);
    }

    void Update()
    {
        if (movementBlocked)
        {
            movementInput = Vector2.zero;
            return;
        }

        movementInput = GameInput.ReadMovement();
    }

    void FixedUpdate()
    {
        rig.linearVelocity = movementBlocked
            ? Vector2.zero
            : movementInput * velocity;
    }

    void LateUpdate()
    {
        ClampToCameraViewport();
        RefreshNpcCacheWhenNeeded();
        RefreshNpcSortingOrders();
    }

    public void SetMovementBlocked(bool blocked)
    {
        movementBlocked = blocked;

        if (!blocked)
        {
            return;
        }

        movementInput = Vector2.zero;

        if (rig != null)
        {
            rig.linearVelocity = Vector2.zero;
        }
    }

    private void ClampToCameraViewport()
    {
        Camera activeCamera = Camera.main;

        if (rig == null || activeCamera == null || !activeCamera.orthographic)
        {
            return;
        }

        Vector2 extent = playerCollider == null
            ? new Vector2(0.2f, 0.2f)
            : playerCollider.bounds.extents;
        float halfHeight = activeCamera.orthographicSize;
        float halfWidth = halfHeight * activeCamera.aspect;
        Vector3 cameraPosition = activeCamera.transform.position;
        Vector2 current = rig.position;

        float minimumX = cameraPosition.x - halfWidth + extent.x;
        float maximumX = cameraPosition.x + halfWidth - extent.x;
        float minimumY = cameraPosition.y - halfHeight + extent.y;
        float maximumY = cameraPosition.y + halfHeight - extent.y;

        Vector2 clamped = new Vector2(
            Mathf.Clamp(current.x, minimumX, maximumX),
            Mathf.Clamp(current.y, minimumY, maximumY)
        );

        if (clamped == current)
        {
            return;
        }

        rig.position = clamped;
        rig.linearVelocity = new Vector2(
            clamped.x == current.x ? rig.linearVelocity.x : 0f,
            clamped.y == current.y ? rig.linearVelocity.y : 0f
        );
    }

    private void RefreshNpcCacheWhenNeeded()
    {
        if (Time.unscaledTime < nextNpcRefreshTime)
        {
            return;
        }

        nextNpcRefreshTime = Time.unscaledTime +
            Mathf.Max(0.1f, npcRefreshInterval);
        sortableNpcRenderers.Clear();

        if (string.IsNullOrWhiteSpace(sortableNpcTag))
        {
            return;
        }

        GameObject[] taggedCharacters;
        try
        {
            taggedCharacters = GameObject.FindGameObjectsWithTag(
                sortableNpcTag
            );
            invalidNpcTagReported = false;
        }
        catch (UnityException exception)
        {
            if (!invalidNpcTagReported)
            {
                Debug.LogError(
                    "A tag de ordenação de NPCs não está cadastrada: " +
                    sortableNpcTag + ". " + exception.Message,
                    this
                );
                invalidNpcTagReported = true;
            }
            return;
        }

        foreach (GameObject character in taggedCharacters)
        {
            if (character == null || character == gameObject)
            {
                continue;
            }

            SpriteRenderer renderer = FindPrimaryRenderer(character);
            if (renderer != null)
            {
                sortableNpcRenderers.Add(renderer);
            }
        }
    }

    private void RefreshNpcSortingOrders()
    {
        playerSortingRenderer ??= FindPrimaryRenderer(gameObject);
        if (playerSortingRenderer == null ||
            playerSortingRenderer.sprite == null)
        {
            return;
        }

        float playerFeetY = playerSortingRenderer.bounds.min.y;
        int playerOrder = playerSortingRenderer.sortingOrder;

        for (int index = sortableNpcRenderers.Count - 1;
             index >= 0;
             index--)
        {
            SpriteRenderer npcRenderer = sortableNpcRenderers[index];
            if (npcRenderer == null)
            {
                sortableNpcRenderers.RemoveAt(index);
                continue;
            }

            if (!npcRenderer.gameObject.activeInHierarchy ||
                npcRenderer.sprite == null)
            {
                continue;
            }

            float heightDifference = npcRenderer.bounds.min.y - playerFeetY;
            if (heightDifference < -FeetComparisonTolerance)
            {
                npcRenderer.sortingOrder = playerOrder + 1;
            }
            else if (heightDifference > FeetComparisonTolerance)
            {
                npcRenderer.sortingOrder = playerOrder - 1;
            }
            else
            {
                npcRenderer.sortingOrder = playerOrder;
            }
        }
    }

    /// <summary>
    /// Prefere o renderer do objeto raiz. Em NPCs compostos, usa como alternativa
    /// o maior sprite encontrado nos filhos, normalmente o corpo do personagem.
    /// </summary>
    private static SpriteRenderer FindPrimaryRenderer(GameObject character)
    {
        if (character == null)
        {
            return null;
        }

        SpriteRenderer rootRenderer = character.GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            return rootRenderer;
        }

        SpriteRenderer selected = null;
        float selectedArea = -1f;
        foreach (SpriteRenderer renderer in
                 character.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer == null || renderer.sprite == null)
            {
                continue;
            }

            Vector2 size = renderer.sprite.bounds.size;
            float area = Mathf.Abs(
                size.x * renderer.transform.lossyScale.x *
                size.y * renderer.transform.lossyScale.y
            );
            if (area > selectedArea)
            {
                selected = renderer;
                selectedArea = area;
            }
        }

        return selected;
    }
}
