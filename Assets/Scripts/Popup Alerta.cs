using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PopupAlerta : MonoBehaviour
{
    public GameObject popup;
    public Text texto;

    void Start()
    {
        popup.SetActive(false);
    }
    public async Task AttTexto(string texto)
    {
        popup.SetActive(true);
        this.texto.text = texto;
        await Task.Delay(1000);
        popup.SetActive(false);
    }
}
