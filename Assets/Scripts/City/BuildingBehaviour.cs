using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BuildingBehaviour : MonoBehaviour, IInteractable
{
    [SerializeField]
    private BuildingDefinition definition;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private int level;

    public bool CanInteract => enabled && definition != null;

    public BuildingDefinition Definition => definition;
    public int Level => level;

    public event Action<BuildingBehaviour> Selected;

    internal void Initialize(int initialLevel)
    {
        level = Mathf.Clamp(initialLevel, 0, definition.maximumLevel);
        RefreshView();
    }

    internal void RenderLevel(int newLevel)
    {
        level = Mathf.Clamp(newLevel, 0, definition.maximumLevel);
        RefreshView();
    }

    private void RefreshView()
    {
        if (definition == null || spriteRenderer == null)
        {
            return;
        }

        if (level == 0)
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
            level - 1,
            0,
            definition.levelSprites.Length - 1
        );

        spriteRenderer.sprite = definition.levelSprites[spriteIndex];
    }

    public void Interact(GameObject player)
    {
        Selected?.Invoke(this);
    }
}
