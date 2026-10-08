using System;
using System.Collections.Generic;
using UnityEngine;
public enum Classes{ guerreiro, arqueiro, ladrão, mago}

/// <summary>
/// Defines which defense reduces an adventurer's attack during exploration battles.
/// </summary>
public enum DamageType
{
    Physical,
    Magical
}


[System.Serializable]

[CreateAssetMenu(fileName = "aventureiro", menuName = "Personagem")]
public class Char_base : ScriptableObject
{
    [SerializeField]
    private string persistenceId;

    public string PersistenceId => string.IsNullOrWhiteSpace(persistenceId)
        ? name
        : persistenceId;

    public Sprite sprite;

    [Space(10)][Header("Visual no combate de exploração")]
    [Tooltip("Prefab 2D instanciado na cena de combate. Se ficar vazio, o campo Sprite será usado como alternativa.")]
    public GameObject combatPrefab;

    [Tooltip("Ajuste fino da posição do prefab dentro do ponto reservado ao combatente.")]
    public Vector3 combatLocalPosition;

    [Tooltip("Rotação local aplicada somente ao visual instanciado no combate.")]
    public Vector3 combatLocalEulerAngles;

    [Tooltip("Escala local aplicada ao prefab. Use (1,1,1) para manter a escala original dele.")]
    public Vector3 combatLocalScale = Vector3.one;

    [Tooltip("Espelha o visual horizontalmente sem alterar o prefab original.")]
    public bool flipCombatVisualX;

    [Header("Status base")]
    public ushort vida;
    public byte resistência_magica, armadura, destreza, inteligencia, forca, mana,
    ganho_mana;

    [Tooltip("Determina a ordem dos turnos: valores maiores atacam primeiro.")]
    public byte velocidade = 10;

    [Tooltip("Dano físico é reduzido por armadura; dano mágico, por resistência mágica.")]
    public DamageType tipoDano = DamageType.Physical;
    [Space(10)][Header("Classe")]
    public Classes classe;
    public List<Tracos> traços;
    public bool distancia;
    //Dano tem de ser um calculo, baseado em classe
    //public short dano_min, dano_max, vida;

    [Space(10)][Header("Outros")]
    public ushort preco;


    [Space(10)][Header("Itens")]
    public Item mao;
    public Item torso, acessorio;

    [Space(10)][Header("Dano")]
    public ushort dano_min;
    public ushort dano_max;
    public bool vivo;

    [Space(10)][Header("Escaladores")]
    public float escalaDexMax;
    public float escalaIntMax, escalaForMax, escalaDexMin, escalaIntMin, escalaForMin;
    public void calculaVida()
    {
        if(mao != null){if(mao.hp > 0) vida += mao.hp;}
        if(torso != null){if(torso.hp > 0) vida += torso.hp;}
        if(acessorio != null){ if(acessorio.hp > 0) vida += acessorio.hp;}
    }
    public void morrer()
    {
        if (vida <= 0) vivo = false;
    }

}
