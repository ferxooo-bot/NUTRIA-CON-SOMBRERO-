using UnityEngine;
using System.Collections.Generic;

public class ControladorPuertas : MonoBehaviour
{
    public static ControladorPuertas instancia; // Singleton

    private HashSet<int> puertasAbiertas = new HashSet<int>();

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject); // Evita duplicados si ya existe otro
        }
    }

    public void AbrirPuerta(int numeroPuerta)
    {
        if (!puertasAbiertas.Contains(numeroPuerta))
        {
            puertasAbiertas.Add(numeroPuerta);
        }
    }

    public bool EstaPuertaAbierta(int numeroPuerta)
    {
        return puertasAbiertas.Contains(numeroPuerta);
    }
}
