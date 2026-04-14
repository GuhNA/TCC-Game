

using UnityEngine;

public class arqueiro : MonoBehaviour
{
    
    public Char_base status;

    void Start()
    {
        status.vivo = true;
        //inteligencia .1, destreza 1.5, força .5
        status.dano_max = (ushort)(4 + status.destreza*status.escalaDexMax
                                    + status.inteligencia*status.escalaIntMax 
                                    + status.forca*status.escalaForMax);
        status.dano_min = (ushort)(status.destreza*status.escalaDexMin
                                    + status.inteligencia*status.escalaIntMin 
                                    + status.forca*status.escalaForMin);
        status.calculaVida();
    }

   
}
