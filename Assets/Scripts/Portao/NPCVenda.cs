using System;
using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;

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
    public Text arqueiro_texto, guerreiro_texto, mago_texto, tempo_texto;
    public Inventario inventario;
    public Construcoes cidade;
    byte arqueiro_value = 5, guerreiro_value = 3, mago_value = 8;
    
    DateTime data_renova;
    int escolha = 0;

    void Start()
    {
        aberto = false;
        dentro = false;
        popup.SetActive(false);
        data_renova = DateTime.Parse(cidade.data_renova);
        print(data_renova);
    }

    void Update()
    {
        if (aberto)
        {
            tempo_atualiza = data_renova.AddHours(6).Subtract(DateTime.Now);
            tempo_texto.text = "Tempo: "+ tempo_atualiza.ToString()[..8];
        }        

        if(dentro)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {

                if (!aberto) abrir_popup();
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
        atualizaTexto();
        aberto = true;
        //Verifica se passou de 6h de CD
        //Necessario login para voltar a atualizar  NON E INTERESSANTE
        if(DateTime.Compare(DateTime.Now,data_renova.AddHours(6)) == 1)
        {
            cidade.data_renova = DateTime.Now.ToString();
            data_renova = DateTime.Now;
            atualizaDados();
            atualizaTexto();
        }
        popup.SetActive(true);
    }

    public void vendaCalculo(int intValue)
    { 
        uint value = (uint)intValue;
        //Botão arqueiro = 1, guerreiro = 2, mago = 3;
        switch (escolha)
        {
            case 1:
                if(value <= cidade.arqueirosDis)
                {
                    cidade.arqueirosDis -=value;
                    inventario.arqueiro += value;
                    inventario.moedas -= arqueiro_value;
                    StartCoroutine(alerta.attTexto("Compra realizada, arqueiros disponíveis: "+cidade.arqueirosDis));
                    atualizaTexto();
                }
                else  StartCoroutine(alerta.attTexto("Quantidade indisponível de arqueiro"));
                break;
            case 2:
                if(value <= cidade.guerreirosDis)
                {
                    cidade.guerreirosDis -=value;
                    inventario.guerreiro += value;
                    inventario.moedas -= guerreiro_value;
                    StartCoroutine(alerta.attTexto("Compra realizada, guerreiros disponíveis: "+cidade.guerreirosDis));
                    atualizaTexto();
                }
                else  StartCoroutine(alerta.attTexto("Quantidade indisponível de guerreiro"));
            break;

            case 3:
                if(value <= cidade.magosDis)
                {
                    cidade.magosDis -=value;
                    inventario.mago += value;
                    inventario.moedas -= mago_value;
                    StartCoroutine(alerta.attTexto("Compra realizada, magos disponíveis: "+cidade.magosDis));
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
        arqueiro_texto.text = "Arqueiros: "+cidade.arqueirosDis;
        guerreiro_texto.text = "Guerreiros: "+cidade.guerreirosDis;
        mago_texto.text = "Magos: "+cidade.magosDis;
        game.atualizaOuro();

    }

    void atualizaDados()
    {
        //reset a cada 6h
        cidade.arqueirosDis = (uint)((cidade.quartel_arqueiro*1.3f*cidade.arqueirosDis > 0) ? cidade.quartel_arqueiro*1.3f*cidade.arqueirosDis :  10);
        cidade.guerreirosDis = (uint)((cidade.quartel_guerreiro*1.3f*cidade.guerreirosDis > 0)? cidade.quartel_guerreiro*1.3f*cidade.guerreirosDis : 10);
        cidade.magosDis = (uint)((cidade.escola_mago*1.3f*cidade.magosDis > 0)? cidade.escola_mago*1.3f*cidade.magosDis : 10);
    }

    public void porInput(InputField input)
    {
        if(input.name == "arqueiroInput")
        {
            if(Regex.IsMatch(input.text , @"\d+"))
            {
                if(uint.Parse(input.text) <= cidade.arqueirosDis)
                {
                    cidade.arqueirosDis -=uint.Parse(input.text);
                    inventario.arqueiro += uint.Parse(input.text);
                    inventario.moedas -= arqueiro_value;
                    StartCoroutine(alerta.attTexto("Compra realizada, arqueiros disponíveis: "+cidade.arqueirosDis));
                    atualizaTexto();
                }
                else  StartCoroutine(alerta.attTexto("Quantidade indisponível de arqueiro"));
            }
        }
        else if(input.name == "guerreiroInput")
        {
            if(Regex.IsMatch(input.text , @"\d+"))
            {
                if(uint.Parse(input.text) <= cidade.guerreirosDis)
                {
                    cidade.guerreirosDis -=uint.Parse(input.text);
                    inventario.guerreiro += uint.Parse(input.text);
                    inventario.moedas -= guerreiro_value;
                    StartCoroutine(alerta.attTexto("Compra realizada, guerreiros disponíveis: "+cidade.guerreirosDis));
                    atualizaTexto();
                }
                else  StartCoroutine(alerta.attTexto("Quantidade indisponível de guerreiro"));
            }
        }
        else if(input.name == "magoInput")
        {
            if(uint.Parse(input.text) <= cidade.magosDis)
            {
                cidade.magosDis -=uint.Parse(input.text);;
                inventario.mago += uint.Parse(input.text);;
                inventario.moedas -= mago_value;
                StartCoroutine(alerta.attTexto("Compra realizada, magos disponíveis: "+cidade.magosDis));
                atualizaTexto();
            }
            else  StartCoroutine(alerta.attTexto("Quantidade indisponível de mago"));
        }
        input.text = "";
    }
}
