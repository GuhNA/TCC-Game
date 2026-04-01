using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public string descricao;
    public short hp;
    public byte forca, destreza, inteligencia;

}
