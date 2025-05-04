using UnityEngine;
using TMPro;
using System.Collections;

public class PuertaCueva : MonoBehaviour
{
    public GameObject paloIzquierdo;
    public GameObject paloDerecho;
    public float velocidad = 2f;
    public Vector3 offsetIzquierdo = new Vector3(-2f, 0, 0);
    public Vector3 offsetDerecho = new Vector3(2f, 0, 0);

    public int numeroPuerta = 0;
    public int puertaAnterior = -1;

    public Transform jugador;
    public float distanciaMinima = 3f;

    public int basuraRequerida = 5;
    public TextMeshProUGUI textoContador;

    private bool abierta = false;
    private Vector3 posicionFinalIzquierdo;
    private Vector3 posicionFinalDerecho;
    private PlayerInventory inventario;

    void Start()
    {
        posicionFinalIzquierdo = paloIzquierdo.transform.position + offsetIzquierdo;
        posicionFinalDerecho = paloDerecho.transform.position + offsetDerecho;

        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            inventario = jugadorObj.GetComponent<PlayerInventory>();
        }
    }

    void Update()
    {
        if (abierta) return; // ✅ Ya está abierta, no hacer nada

        if (EstaCercaDelJugador())
        {
            MostrarMensajeFaltantes();

            if (Input.GetKeyDown(KeyCode.S) && PuedeAbrirse())
            {
                if (inventario != null && inventario.UsarBasuraExacta(basuraRequerida))
                {
                    AbrirPuerta();
                    ControladorPuertas.instancia.AbrirPuerta(numeroPuerta);
                }
                else
                {
                    MostrarMensajeFaltantes(); // muestra el mensaje aunque no tenga suficiente
                }
            }
        }
        else
        {
            // Oculta el mensaje si está lejos
            if (textoContador != null && !abierta)
            {
                textoContador.text = "";
            }
        }
    }

    void MostrarMensajeFaltantes()
    {
        if (textoContador == null || inventario == null) return;

        int falta = Mathf.Max(0, basuraRequerida - inventario.GetBasuraActual());
        if (falta > 0)
        {
            textoContador.text = "Faltan " + falta + " basuras para la puerta " + numeroPuerta;
        }
        else
        {
            textoContador.text = "Presiona S para abrir la puerta " + numeroPuerta;
        }
    }

    void AbrirPuerta()
    {
        abierta = true;
        StartCoroutine(MoverPuerta());

        if (textoContador != null)
            textoContador.text = "¡Puerta abierta!";
    }

    IEnumerator MoverPuerta()
    {
        float tiempo = 0f;
        Vector3 inicioIzquierdo = paloIzquierdo.transform.position;
        Vector3 inicioDerecho = paloDerecho.transform.position;

        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime * velocidad;
            paloIzquierdo.transform.position = Vector3.Lerp(inicioIzquierdo, posicionFinalIzquierdo, tiempo);
            paloDerecho.transform.position = Vector3.Lerp(inicioDerecho, posicionFinalDerecho, tiempo);
            yield return null;
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
        return Vector3.Distance(transform.position, jugador.position) <= distanciaMinima;
    }
}
