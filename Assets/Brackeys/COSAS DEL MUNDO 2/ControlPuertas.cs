using UnityEngine;

public class ControladorPuertas : MonoBehaviour
{
    public static ControladorPuertas instancia;
    public bool[] puertasAbiertas;

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        puertasAbiertas = new bool[10]; // Hasta 10 puertas (ajustable)
    }

    public bool EstaPuertaAbierta(int numero)
    {
        return puertasAbiertas[numero];
    }

    public void AbrirPuerta(int numero)
    {
        puertasAbiertas[numero] = true;
    }
}
