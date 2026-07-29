using UnityEngine;
using UnityEngine.UI;

public class NPCVenda : MonoBehaviour
{
    PopupAlerta alerta;
    bool aberto, dentro;
    public GameObject popup;
    public Text arqueiro_texto, guerreiro_texto, mago_texto;
    public Inventario inventario;
    public Construcoes cidade;
    uint arqueiro, guerreiro, mago = 10;
    int escolha = 0;

    void Start()
    {
        aberto = false;
        dentro = false;
        popup.SetActive(false);
    }

    void Update()
    {
        if(dentro)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Entrei if");
                if(!aberto) abrir_popup();
                else {popup.SetActive(false); aberto = false;}
            }
        }
        print(aberto);
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
        atualizaDados();
        popup.SetActive(true);
    }

    public async void vendaCalculo(int intValue)
    { 
        uint value = (uint)intValue;
        switch (escolha)
        {
            case 1:
                if(value < arqueiro)
                {
                    arqueiro -=value;
                    inventario.arqueiro += value;
                    await alerta.AttTexto("Compra realizada, arqueiros disponíveis: "+arqueiro);
                    atualizaDados();
                }
                else await alerta.AttTexto("Quantidade indisponível de arqueiro");
                break;
            case 2:
                if(value < guerreiro)
                {
                    guerreiro -=value;
                    inventario.guerreiro += value;
                    await alerta.AttTexto("Compra realizada, guerreiros disponíveis: "+guerreiro);
                    atualizaDados();
                }
                else await alerta.AttTexto("Quantidade indisponível de guerreiro");
            break;

            case 3:
                if(value < mago)
                {
                    mago -=value;
                    inventario.mago += value;
                    await alerta.AttTexto("Compra realizada, magos disponíveis: "+mago);
                    atualizaDados();
                }
                else await alerta.AttTexto("Quantidade indisponível de mago");
                break;
        }

    }
    public void compraEscolha(int value)
    {
        escolha = value;
    }
    void atualizaDados()
    {
        arqueiro = (uint)((cidade.quartel_arqueiro*1.3f*arqueiro > 0) ? cidade.quartel_arqueiro*1.3f*arqueiro :  10);
        guerreiro = (uint)((cidade.quartel_guerreiro*1.3f*guerreiro > 0)? cidade.quartel_guerreiro*1.3f*guerreiro : 10);
        mago = (uint)((cidade.escola_mago*1.3f*mago > 0)? cidade.escola_mago*1.3f*mago : 10);
        arqueiro_texto.text = "Arqueiros: "+arqueiro;
        guerreiro_texto.text = "Guerreiros: "+guerreiro;
        mago_texto.text = "Magos: "+mago;
    }
}
