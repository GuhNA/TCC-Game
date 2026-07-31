using System;
using UnityEngine;
using UnityEngine.UI;

public class NPCVenda : MonoBehaviour
{
    //Supor valor de mago, soldado e arqueiro
    //Valor de compra
    //Arqueiro = 5 
    //Mago = 8
    //Guerreiro = 4
    public GameManager game;
    TimeSpan tempo_atualiza;
    bool aberto, dentro;
    public PopupAlerta alerta;
    public GameObject popup;
    public Text arqueiro_texto, guerreiro_texto, mago_texto;
    public Inventario inventario;
    public Construcoes cidade;
    uint arqueiro, guerreiro, mago = 10;
    byte arqueiro_value = 5, guerreiro_value = 3, mago_value = 8;

    int escolha = 0;

    void Start()
    {
        aberto = false;
        dentro = false;
        popup.SetActive(false);
    }

    void Update()
    {
        tempo_atualiza = cidade.data_renova.AddHours(6).Subtract(DateTime.Now);
        //Range para pegar a string [..8] = pega os primeiros 8 caracteres. [8..] = pega apos 8 caracteres.
        //print(tempo_atualiza.ToString()[..8]);
        
        if(dentro)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                
                if(!aberto) abrir_popup();
                else {popup.SetActive(false); aberto = false;}
            }
        }
    }
    void OnTriggerEnter2D()
    {  
        dentro = true;
    }

    void OnTriggerExit2D()
    {
        dentro = false;
    }
    void abrir_popup()
    {
        aberto = true;
        
        //Verifica se passou de 6h de CD
        //Necessario login para voltar a atualizar  NON E INTERESSANTE
        if(DateTime.Compare(cidade.data_renova.AddHours(6),DateTime.Now) == 1)
        {
            cidade.data_renova = DateTime.Now;
            atualizaDados();
            atualizaTexto();
        }
        popup.SetActive(true);
    }

    public void vendaCalculo(int intValue)
    { 
        uint value = (uint)intValue;
        switch (escolha)
        {
            case 1:
                if(value <= arqueiro)
                {
                    arqueiro -=value;
                    inventario.arqueiro += value;
                    inventario.moedas -= arqueiro_value;
                    StartCoroutine(alerta.attTexto("Compra realizada, arqueiros disponíveis: "+arqueiro));
                    atualizaTexto();
                }
                else  StartCoroutine(alerta.attTexto("Quantidade indisponível de arqueiro"));
                break;
            case 2:
                if(value <= guerreiro)
                {
                    guerreiro -=value;
                    inventario.guerreiro += value;
                    inventario.moedas -= guerreiro_value;
                    StartCoroutine(alerta.attTexto("Compra realizada, guerreiros disponíveis: "+guerreiro));
                    atualizaTexto();
                }
                else  StartCoroutine(alerta.attTexto("Quantidade indisponível de guerreiro"));
            break;

            case 3:
                if(value <= mago)
                {
                    mago -=value;
                    inventario.mago += value;
                    inventario.moedas -= mago_value;
                    StartCoroutine(alerta.attTexto("Compra realizada, magos disponíveis: "+mago));
                    atualizaTexto();
                }
                else  StartCoroutine(alerta.attTexto("Quantidade indisponível de mago"));
                break;
        }

    }
    public void compraEscolha(int value)
    {
        escolha = value;
    }
    void atualizaTexto()
    {
        arqueiro_texto.text = "Arqueiros: "+arqueiro;
        guerreiro_texto.text = "Guerreiros: "+guerreiro;
        mago_texto.text = "Magos: "+mago;
        game.atualizaOuro();

    }

    void atualizaDados()
    {
        //reset a cada 6h
        arqueiro = (uint)((cidade.quartel_arqueiro*1.3f*arqueiro > 0) ? cidade.quartel_arqueiro*1.3f*arqueiro :  10);
        guerreiro = (uint)((cidade.quartel_guerreiro*1.3f*guerreiro > 0)? cidade.quartel_guerreiro*1.3f*guerreiro : 10);
        mago = (uint)((cidade.escola_mago*1.3f*mago > 0)? cidade.escola_mago*1.3f*mago : 10);
    }

}
