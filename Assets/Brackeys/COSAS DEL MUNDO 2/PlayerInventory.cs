using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int basuraRecolectada = 0;

    public void RecogerBasura()
    {
        basuraRecolectada++;
    }

    public bool UsarBasuraExacta(int cantidad)
{
    if (basuraRecolectada >= cantidad)
    {
        // Solo usa lo necesario, pero luego reinicia el conteo
        basuraRecolectada = 0;
        return true;
    }
    return false;
}


    public int GetBasuraActual()
    {
        return basuraRecolectada;
    }
}
