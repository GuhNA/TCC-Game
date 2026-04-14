using UnityEngine;

public class Aventureiro : MonoBehaviour
{
    
    public Char_base charBase;
    Sprite spriteChar;


    void Start()
    {
        spriteChar = charBase.sprite;
        charBase.vivo = true;
        //inteligencia .1, destreza 1.5, força .5
        charBase.dano_max = (ushort)(4 + charBase.destreza*charBase.escalaDexMax
                                    + charBase.inteligencia*charBase.escalaIntMax 
                                    + charBase.forca*charBase.escalaForMax);
        charBase.dano_min = (ushort)(charBase.destreza*charBase.escalaDexMin
                                    + charBase.inteligencia*charBase.escalaIntMin 
                                    + charBase.forca*charBase.escalaForMin);
        charBase.calculaVida();
    }

}
