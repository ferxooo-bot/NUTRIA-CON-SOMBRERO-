using UnityEngine;

public class PuertaCueva : MonoBehaviour
{
    public GameObject paloIzquierdo;
    public GameObject paloDerecho;
    public float velocidad = 2f;
    public Vector3 offsetIzquierdo = new Vector3(-2f, 0, 0);
    public Vector3 offsetDerecho = new Vector3(2f, 0, 0);

    public int numeroPuerta = 0;
    public int puertaAnterior = -1;

    public Transform jugador; // <-- Asignar el jugador aquí en el Inspector
    public float distanciaMinima = 3f;

    private bool abrir = false;
    private Vector3 posicionFinalIzquierdo;
    private Vector3 posicionFinalDerecho;

    void Start()
    {
        posicionFinalIzquierdo = paloIzquierdo.transform.position + offsetIzquierdo;
        posicionFinalDerecho = paloDerecho.transform.position + offsetDerecho;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && PuedeAbrirse() && EsLaPuertaMasCercana())
        {
            abrir = true;
            ControladorPuertas.instancia.AbrirPuerta(numeroPuerta);
        }

        if (abrir)
        {
            paloIzquierdo.transform.position = Vector3.MoveTowards(
                paloIzquierdo.transform.position,
                posicionFinalIzquierdo,
                velocidad * Time.deltaTime
            );

            paloDerecho.transform.position = Vector3.MoveTowards(
                paloDerecho.transform.position,
                posicionFinalDerecho,
                velocidad * Time.deltaTime
            );
        }
    }

    bool PuedeAbrirse()
    {
        if (puertaAnterior < 0) return true;
        return ControladorPuertas.instancia.EstaPuertaAbierta(puertaAnterior);
    }

    bool EstaCercaDelJugador()
    {
        if (jugador == null) return false;
        float distancia = Vector3.Distance(transform.position, jugador.position);
        return distancia <= distanciaMinima;
    }

    bool EsLaPuertaMasCercana()
{
    PuertaCueva[] todasLasPuertas = FindObjectsOfType<PuertaCueva>();
    float miDistancia = Vector3.Distance(transform.position, jugador.position);

    foreach (var puerta in todasLasPuertas)
    {
        if (puerta == this) continue;

        float distanciaOtra = Vector3.Distance(puerta.transform.position, jugador.position);
        if (distanciaOtra < miDistancia)
        {
            return false; // Hay otra puerta más cerca
        }
    }

    return EstaCercaDelJugador();
}

}
