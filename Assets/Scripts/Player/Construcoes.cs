using System;
using System.Globalization;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "Cidade", menuName = "Sistema/Cidade")]
public class Construcoes : ScriptableObject
{

    public DateTime data_renova = DateTime.ParseExact("07/30/2026 22:23:01", "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
    public byte quartel_guerreiro, quartel_arqueiro, escola_mago = 0;
}
