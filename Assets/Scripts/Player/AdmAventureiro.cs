using UnityEngine;

public class AdmAventureiro : MonoBehaviour
{
    public Inventario inventario;
    moviment movimento;
    bool aventureiro_proximo;
    bool ativaPopup;
    public GameObject popup_aventureiro;
    public AventureiroPopup popup;
    public Aventureiro aventureiro;

    private void Awake() {
        movimento = GetComponent<moviment>();
    }
    private void Update() 
    {
        if(aventureiro_proximo)
        {
            if(Input.GetKeyDown(KeyCode.E)) {
                ativaPopup = !ativaPopup;
            }
        }
        if(ativaPopup) 
        {
            attDados(aventureiro);
            movimento.velocity = 0;
        }
        else
        {
            popup_aventureiro.SetActive(false);
            movimento.velocity = 10;
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {   
  
        if(collision.gameObject.GetComponentInParent<Aventureiro>())
        {
            //pegando dados do npc
            aventureiro = collision.gameObject.GetComponentInParent<Aventureiro>();
            //abrindo/fechando popup
            aventureiro_proximo = true;
        }
    }

   private void OnTriggerExit2D(Collider2D other) 
   {
        
    //removendo dados pegos do aventureiro
    popup.vida.text = "";
    popup.dex.text = "";
    popup.inte.text = "";
    popup.forca.text = "";
    popup.foto.sprite = null;
    aventureiro_proximo = false;

    }
    void attDados(Aventureiro aventureiro)
    {
        popup.vida.text = aventureiro.status.vida.ToString();
        popup.dex.text = aventureiro.status.destreza.ToString();
        popup.inte.text = aventureiro.status.inteligencia.ToString();
        popup.forca.text = aventureiro.status.forca.ToString();
        popup.comprar.text = aventureiro.status.preco.ToString();
        popup.foto.sprite = aventureiro.status.sprite;
        popup_aventureiro.SetActive(true);
    }

    public void comprarPersonagem()
    {
        if(inventario.moedas >= aventureiro.status.preco) 
        {
            inventario.moedas -= aventureiro.status.preco;
            inventario.aventureiros.Add(aventureiro);
        }
        
    }
}