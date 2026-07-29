using UnityEngine;
using UnityEngine.UI;

public class AtualizaOuro : MonoBehaviour
{
    public Text ouro;
    public Inventario inventario;
    void Start()
    {
        ouro.text = "Ouro: "+inventario.moedas;
    }

}
