using System;
using System.Globalization;
using UnityEngine;
using System.Reflection;

[System.Serializable]
[CreateAssetMenu(fileName = "Cidade", menuName = "Sistema/Cidade")]
public class Construcoes : ScriptableObject
{

    public string data_renova = new DateTime().ToString();
    public byte quartel_guerreiro, quartel_arqueiro, escola_mago;
    public uint arqueirosDis, guerreirosDis, magosDis;
}
