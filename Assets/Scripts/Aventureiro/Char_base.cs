using System;
using System.Collections.Generic;
using UnityEngine;
public enum Classes{ guerreiro, arqueiro, ladrão, mago}


[System.Serializable]

[CreateAssetMenu(fileName = "aventureiro", menuName = "Personagem")]
public class Char_base : ScriptableObject
{
    public Sprite sprite;
    [Header("Status base")]
    public ushort vida;
    public byte resistência_magica, armadura, destreza, inteligencia, forca, mana,
    ganho_mana;
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
