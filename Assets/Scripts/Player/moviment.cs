using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class moviment : MonoBehaviour
{
    bool ativaPopup;
    public GameObject popup_aventureiro;
    AventureiroPopup popup;
    Vector2 move;
    Rigidbody2D rig;
    float axisX;
    float axisY;
    [SerializeField] float velocity;


    void Awake()
    {
        popup = FindAnyObjectByType<AventureiroPopup>();
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

    void OnTriggerEnter2D(Collider2D collision)
    {   
  
        if(collision)
        {
            Debug.Log(collision.name);
            //pegando dados do npc
            Aventureiro aventureiro = collision.gameObject.GetComponentInParent<Aventureiro>();
            //abrindo/fechando popup

            //Pesquisar amanha este input nao esta funcionando
            if(Input.GetKeyDown(KeyCode.E))
            {
                ativaPopup = !ativaPopup;
                Debug.Log(ativaPopup);
            }
            //atualizando os dados da popup
            if(ativaPopup) attDados(aventureiro);
            else popup_aventureiro.SetActive(false);
        }
    }

    private void OnCollisionExit2D(Collision2D other) 
    {
        //removendo dados pegos do aventureiro
        popup.vida.text = "";
        popup.dex.text = "";
        popup.inte.text = "";
        popup.foto.sprite = null;
    }

    void attDados(Aventureiro aventureiro)
    {
        popup.vida.text = aventureiro.status.vida.ToString();
        popup.dex.text = aventureiro.status.destreza.ToString();
        popup.inte.text = aventureiro.status.inteligencia.ToString();
        popup.forca.text = aventureiro.status.forca.ToString();

        popup.foto.sprite = aventureiro.status.sprite;
        popup_aventureiro.SetActive(true);
    }
}
