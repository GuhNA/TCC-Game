using TCCGame.Core;
using TCCGame.Core.Input;
using TCCGame.Gameplay.Player;
using UnityEngine;

[RequireComponent(typeof(moviment))]
public class AdmAventureiro : MonoBehaviour
{
    moviment movimento;
    bool aventureiro_proximo;
    bool ativaPopup;
    public GameObject popup_aventureiro;
    public AventureiroPopup popup;
    public Aventureiro aventureiro;
    public PopupAlerta alerta;

    private void Awake()
    {
        movimento = GetComponent<moviment>();
    }

    private void OnDisable()
    {
        if (movimento != null)
        {
            movimento.SetMovementBlocked(false);
        }
    }

    private void Update() 
    {
        if(aventureiro_proximo)
        {
            if(GameInput.WasInteractPressedThisFrame())
            {
                ativaPopup = !ativaPopup;
            }
        }
        if(ativaPopup) 
        {
            if (aventureiro != null && popup != null)
            {
                attDados(aventureiro);
                movimento.SetMovementBlocked(true);
            }
            else
            {
                ativaPopup = false;
                movimento.SetMovementBlocked(false);
            }
        }
        else
        {
            if (popup_aventureiro != null)
            {
                popup_aventureiro.SetActive(false);
            }

            movimento.SetMovementBlocked(false);
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
        Aventureiro exitingAdventurer =
            other.GetComponentInParent<Aventureiro>();

        if (exitingAdventurer == null ||
            exitingAdventurer != aventureiro)
        {
            return;
        }

        if (popup != null)
        {
            //removendo dados pegos do aventureiro
            popup.vida.text = "";
            popup.dex.text = "";
            popup.inte.text = "";
            popup.forca.text = "";
            popup.foto.sprite = null;
        }

        aventureiro_proximo = false;
        ativaPopup = false;
        aventureiro = null;
        movimento.SetMovementBlocked(false);
    }   
    void attDados(Aventureiro aventureiro)
    {
        popup.vida.text = aventureiro.charBase.vida.ToString();
        popup.dex.text = aventureiro.charBase.destreza.ToString();
        popup.inte.text = aventureiro.charBase.inteligencia.ToString();
        popup.forca.text = aventureiro.charBase.forca.ToString();
        popup.comprar.text = aventureiro.charBase.preco.ToString();
        popup.foto.sprite = aventureiro.charBase.sprite;
        if (popup_aventureiro != null)
        {
            popup_aventureiro.SetActive(true);
        }
    }

    public void ComprarPersonagem()
    {
        if (aventureiro == null || aventureiro.charBase == null)
        {
            ativaPopup = false;
            ShowAlert("Aventureiro inválido");
            return;
        }

        AdventurerRecruitmentStatus result =
            GameSessionProvider.Current.Inventory.TryRecruitAdventurer(
                aventureiro.charBase.PersistenceId,
                aventureiro.charBase.preco
            );

        ativaPopup = false;

        switch (result)
        {
            case AdventurerRecruitmentStatus.Recruited:
                ShowAlert("Mercenário contratado");
                break;

            case AdventurerRecruitmentStatus.AlreadyRecruited:
                ShowAlert("Personagem já contratado!!!");
                break;

            case AdventurerRecruitmentStatus.InsufficientMoney:
                ShowAlert("Dinheiro insuficiente");
                break;

            default:
                ShowAlert("Aventureiro inválido");
                break;
        }
    }

    private void ShowAlert(string message)
    {
        if (alerta != null)
        {
            StartCoroutine(alerta.attTexto(message));
        }
    }
}
