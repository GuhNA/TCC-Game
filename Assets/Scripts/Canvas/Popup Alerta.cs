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
        popup.SetActive(true);
        this.texto.text = texto;
        yield return new WaitForSeconds(1);
        popup.SetActive(false);
    } 
}
