using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; 

public class ProcuraInput : MonoBehaviour
{
    public void procuraInput()
    {
        Transform atual = transform.parent;

        while (atual != null)
        {
            Transform texto = atual.transform.Find("Input");
            if(texto != null)
            {
                Debug.Log("Encontrei");
            }
            atual = atual.parent;
        }

        Debug.Log("Nenhum Input encontrado.");
    }
}
