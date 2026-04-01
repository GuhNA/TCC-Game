

using UnityEngine;

public class arqueiro : MonoBehaviour
{
    
    public Char_base char_Base;
    public NPC_guild_status status;
    void Start()
    {
        char_Base.vida = status.vida;
        char_Base.vivo = true;
        //inteligencia .1, destreza 1.5, força .5
        char_Base.dano_max = (short)(4 + status.destreza*status.escalaDexMax
                                    + status.inteligencia*status.escalaIntMax 
                                    + status.forca*status.escalaForMax);
        char_Base.dano_min = (short)(status.destreza*status.escalaDexMin
                                    + status.inteligencia*status.escalaIntMin 
                                    + status.forca*status.escalaForMin);
        char_Base.calculaVida();
    }

   
}
