using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class moviment : MonoBehaviour
{

    Vector2 move;
    Rigidbody2D rig;
    float axisX;
    float axisY;
    public float velocity;


    
    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
    }



    void Update()
    {
        axisX = Keyboard.current.aKey.isPressed ? -1 : 
                Keyboard.current.dKey.isPressed ? 1 : 0;

        axisY = Keyboard.current.sKey.isPressed ? -1 : 
                Keyboard.current.wKey.isPressed ? 1 : 0;

        move = new Vector2(axisX * velocity * Time.deltaTime, axisY * velocity * Time.deltaTime);
        rig.position += new Vector2(move.x, move.y);

    }

}
