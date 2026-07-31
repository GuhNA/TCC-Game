using System;
using UnityEngine;

public class DefenseStatus : MonoBehaviour
{

    Inventario inventario;
    int arqueiros;
    int guerreiros;
    int magos;

    DateTime defenseTime;
    void Start()
    {
        //atualizar o tempo inicial
        defenseTime = DateTime.Now.AddSeconds(3);
    }

    void Update()
    {
        //if(DateTime.Now.CompareTo(defenseTime) == 1) Debug.Log("battle time!");
        //Acontece batalha, atualiza a data nova.
    }
}
