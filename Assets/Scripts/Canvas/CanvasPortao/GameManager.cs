using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Text ouro;
    public Inventario inventario;
    void Start()
    {
        ouro.text = "Ouro: "+inventario.moedas;
    }

    public void atualizaOuro()
    {
        ouro.text = "Ouro: "+inventario.moedas;
    }
}
