using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class BuildingBehaviour : MonoBehaviour
{
    [SerializeField]
    private BuildingDefinition definition;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private BuildingState state;
    private bool playerInContact;

    public BuildingDefinition Definition => definition;
    public BuildingState State => state;

    public event Action<BuildingBehaviour> Selected;
    public event Action<BuildingBehaviour, int, int> LevelChanged;

    public void Initialize(BuildingState buildingState)
    {
        state = buildingState;
        RefreshView();
    }

    public void IncreaseLevel()
    {
        if (state == null)
        {
            throw new InvalidOperationException(
                "Building has not been initialized."
            );
        }

        if (state.level >= definition.maximumLevel)
        {
            return;
        }

        int previousLevel = state.level;
        state.level++;
        RefreshView();
        LevelChanged?.Invoke(this, previousLevel, state.level);
    }

    public void RefreshView()
    {
        if (state == null || spriteRenderer == null)
        {
            return;
        }

        if (state.level == 0)
        {
            spriteRenderer.sprite = definition.unbuiltSprite;
            return;
        }

        if (definition.levelSprites == null ||
            definition.levelSprites.Length == 0)
        {
            return;
        }

        int spriteIndex = Mathf.Clamp(
            state.level - 1,
            0,
            definition.levelSprites.Length - 1
        );

        spriteRenderer.sprite = definition.levelSprites[spriteIndex];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponentInParent<moviment>() == null)
        {
            return;
        }

        playerInContact = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponentInParent<moviment>() == null)
        {
            return;
        }

        playerInContact = false;
    }

    private void Update()
    {
        if (playerInContact &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Selected?.Invoke(this);
        }
    }
}
