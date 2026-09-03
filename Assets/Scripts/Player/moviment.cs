using TCCGame.Core.Input;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class moviment : MonoBehaviour
{
    private Vector2 movementInput;
    private Rigidbody2D rig;
    private Collider2D playerCollider;
    private bool movementBlocked;

    public float velocity;

    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
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
}
