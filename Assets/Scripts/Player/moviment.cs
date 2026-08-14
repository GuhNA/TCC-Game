using UnityEngine;
using UnityEngine.InputSystem;

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
        if (movementBlocked || Keyboard.current == null)
        {
            movementInput = Vector2.zero;
            return;
        }

        float axisX = Keyboard.current.aKey.isPressed ? -1 :
            Keyboard.current.dKey.isPressed ? 1 : 0;

        float axisY = Keyboard.current.sKey.isPressed ? -1 :
            Keyboard.current.wKey.isPressed ? 1 : 0;

        movementInput = new Vector2(axisX, axisY);
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
