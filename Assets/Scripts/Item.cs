using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public string descricao;
    public ushort hp;
    public byte forca, destreza, inteligencia;

}
