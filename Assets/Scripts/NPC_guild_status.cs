using System;
using System.Collections.Generic;
using UnityEngine;

public enum Classes{guerreiro, arqueiro, mago, ladrao}

[CreateAssetMenu(fileName = "Aventureiro", menuName = "Aventureiro")]
public class NPC_guild_status : ScriptableObject
{
    public Sprite sprite;
    [Header("Status base")]
    public short vida;
    public byte resistência_magica, armadura, destreza, inteligencia, forca, mana,
    ganho_mana;
    [Space(10)][Header("Classe")]
    public Classes classe;
    public List<string> traços;
    public bool distancia;
    //Dano tem de ser um calculo, baseado em classe
    //public short dano_min, dano_max, vida; 
    [Space(10)][Header("escaladores")]
    public float escalaDexMax;
    public float escalaIntMax, escalaForMax, escalaDexMin, escalaIntMin, escalaForMin;

}
