using TCCGame.Core.Input;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class moviment : MonoBehaviour
{
    private Vector2 movementInput;
    private Rigidbody2D rig;
    private bool movementBlocked;

    public float velocity;

    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
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
}
