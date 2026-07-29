using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NovoInventario", menuName = "Sistema/Inventario")]
public class Inventario : ScriptableObject
{
    
    //taverna membro
    public List<Char_base> aventureiros;

    //monetizacao
    public uint moedas;

    //exercito
    public uint guerreiro, arqueiro, mago;

    //itens gerais
    public List<Item> items;
}
