using TCCGame.Core;
using TCCGame.Gameplay.Player;
using UnityEngine;

[RequireComponent(typeof(moviment))]
public class AdmAventureiro : MonoBehaviour
{
    moviment movimento;
    bool ativaPopup;
    private PlayerInteractor interactor;
    public GameObject popup_aventureiro;
    public AventureiroPopup popup;
    public Aventureiro aventureiro;
    public PopupAlerta alerta;

    private void Awake()
    {
        movimento = GetComponent<moviment>();
        interactor = GetComponent<PlayerInteractor>();
    }

    private void OnDisable()
    {
        interactor?.CloseAllModals();

        if (movimento != null)
        {
            movimento.SetMovementBlocked(false);
        }
    }

    public void Open(Aventureiro target)
    {
        if (ativaPopup && aventureiro == target)
        {
            Close(target);
            return;
        }

        if (ativaPopup)
        {
            ClearPopup();
        }

        aventureiro = target;
        ativaPopup = aventureiro != null && popup != null;

        if (ativaPopup)
        {
            attDados(aventureiro);
            interactor?.SetModalOpen(true);
        }
    }

    public void Close(Aventureiro target)
    {
        if (aventureiro != target)
        {
            return;
        }

        ClearPopup();
    }

    private void ClearPopup()
    {
        if (popup != null)
        {
            //removendo dados pegos do aventureiro
            popup.vida.text = "";
            popup.dex.text = "";
            popup.inte.text = "";
            popup.forca.text = "";
            popup.foto.sprite = null;
            popup_aventureiro?.SetActive(false);
        }
        else
        {
            popup_aventureiro?.SetActive(false);
        }

        ativaPopup = false;
        aventureiro = null;
        interactor?.SetModalOpen(false);
    }   
    void attDados(Aventureiro aventureiro)
    {
        popup.vida.text = aventureiro.Stats.MaxHealth.ToString();
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

        ClearPopup();

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
