using UnityEngine;

public class ContadorObjetos : MonoBehaviour
{
    public static ContadorObjetos Instance;

    public int cantidadPescado = 0;
    public int cantidadBotellaPlastico = 0;
    public int cantidadBotellaVidrio = 0;
    public int cantidadLata = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AumentarPescado()
    {
        cantidadPescado++;
        Debug.Log("Pescados recogidos: " + cantidadPescado);
    }

    public void AumentarBotellaPlastico()
    {
        cantidadBotellaPlastico++;
        Debug.Log("Botellas de plástico recogidas: " + cantidadBotellaPlastico);
    }

    public void AumentarBotellaVidrio()
    {
        cantidadBotellaVidrio++;
        Debug.Log("Botellas de vidrio recogidas: " + cantidadBotellaVidrio);
    }

    public void AumentarLata()
    {
        cantidadLata++;
        Debug.Log("Latas recogidas: " + cantidadLata);
    }
}
