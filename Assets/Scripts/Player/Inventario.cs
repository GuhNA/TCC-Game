using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NovoInventario", menuName = "Sistema/Inventario")]
public class Inventario : ScriptableObject
{
    
    //taverna membro
    public List<Char_base> aventureiros;

    //monetizacao
    public ushort moedas;

    //exercito
    public byte soldados, arqueiros, magos;

    //itens gerais
    public List<Item> items;
}
