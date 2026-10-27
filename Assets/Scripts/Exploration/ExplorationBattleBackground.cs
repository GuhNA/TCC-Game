using UnityEngine;

namespace TCCGame.Gameplay.Exploration
{
    /// <summary>
    /// Fits an encounter background to an orthographic camera without stretching
    /// it. Some image can be cropped at the edges when aspect ratios differ.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ExplorationBattleBackground : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Camera targetCamera;

        private void Awake()
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            targetCamera ??= Camera.main;
            ApplyBackgroundSortingOrder();
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            ApplyBackgroundSortingOrder();
            spriteRenderer.sprite = sprite;
            spriteRenderer.enabled = sprite != null;
            FitToCamera();
        }

        public void FitToCamera()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            targetCamera ??= Camera.main;
            if (targetCamera == null || !targetCamera.orthographic)
            {
                return;
            }

            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            float cameraHeight = targetCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * targetCamera.aspect;
            float scale = Mathf.Max(
                cameraWidth / Mathf.Max(0.01f, spriteSize.x),
                cameraHeight / Mathf.Max(0.01f, spriteSize.y)
            );
            transform.localScale = new Vector3(scale, scale, 1f);
            Vector3 cameraPosition = targetCamera.transform.position;
            transform.position = new Vector3(
                cameraPosition.x,
                cameraPosition.y,
                transform.position.z
            );
        }

        private void ApplyBackgroundSortingOrder()
        {
            if (spriteRenderer != null)
            {
                // Os combatentes usam 1, 2 e 3. O cenário permanece sempre atrás.
                spriteRenderer.sortingOrder = 0;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            ApplyBackgroundSortingOrder();
        }
#endif
    }
}
