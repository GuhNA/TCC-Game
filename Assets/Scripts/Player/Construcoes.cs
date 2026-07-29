using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "Cidade", menuName = "Sistema/Cidade")]
public class Construcoes : ScriptableObject
{
    public byte quartel_guerreiro, quartel_arqueiro, escola_mago = 0;
}
