using System;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Cidade", menuName = "Sistema/Cidade")]
public class Construcoes : ScriptableObject
{
    public string data_renova = DateTime.Now.ToString();
    public byte mercado_local, quartel_guerreiro, quartel_arqueiro, escola_mago = 0;
    public uint arqueirosDis, guerreirosDis, magosDis;
}
