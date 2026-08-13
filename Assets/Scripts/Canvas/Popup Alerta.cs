using System.Collections;
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

    public IEnumerator attTexto(string texto){
        this.texto.text = texto;
        popup.SetActive(true);
        yield return new WaitForSeconds(1);
        popup.SetActive(false);
    } 
}
