using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public TextMeshProUGUI llavesTexto; // Asignar desde el Inspector (TextMeshPro)
    public int llaves = 0;
    public int basuraRecolectada = 0;

    public void RecogerBasura()
    {
        basuraRecolectada++;
    }

    public bool UsarBasuraExacta(int cantidad)
    {
        if (basuraRecolectada >= cantidad)
        {
            basuraRecolectada = 0;
            return true;
        }
        return false;
    }

    public int GetBasuraActual()
    {
        return basuraRecolectada;
    }

    public void RecogerLlave()
    {
        llaves++;
        ActualizarTexto();
    }

    public bool UsarLlaves(int cantidad)
    {
        if (llaves >= cantidad)
        {
            llaves -= cantidad;
            ActualizarTexto();
            return true;
        }
        return false;
    }

    public int GetLlavesActuales()
    {
        return llaves;
    }

    void ActualizarTexto()
    {
        if (llavesTexto != null)
            llavesTexto.text = "Llaves: " + llaves;
    }
}
