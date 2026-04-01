using Unity.VisualScripting;
using UnityEditor.Timeline;
using UnityEngine;

[System.Serializable]
public class Char_base
{
    public short vida;
    public Item mao, torso, acessorio;
    public short dano_min, dano_max;
    public bool vivo;
    
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
