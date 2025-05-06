using UnityEngine;
using TMPro;



public class JaulaAnimal : MonoBehaviour
{
    public Sprite spriteJaulaRota; // Imagen de jaula rota
private SpriteRenderer spriteRenderer;
    public int llavesNecesarias = 3;
    public TextMeshProUGUI textoLlavesFaltantes;
    public Transform jugador;
    public float distanciaMinima = 3f;
    public GameObject animal;

    private PlayerInventory inventario;
    private bool liberado = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            inventario = jugadorObj.GetComponent<PlayerInventory>();
            jugador = jugadorObj.transform;
        }
    }

    void Update()
    {
        if (liberado || jugador == null || inventario == null) return;

        float distancia = Vector3.Distance(jugador.position, transform.position);
        if (distancia <= distanciaMinima)
        {
            int faltan = Mathf.Max(0, llavesNecesarias - inventario.GetLlavesActuales());
            textoLlavesFaltantes.text = "Faltan " + faltan + " llaves";

            if (Input.GetKeyDown(KeyCode.S))
            {
                if (inventario.UsarLlaves(llavesNecesarias))
                {
                    liberado = true;
                    textoLlavesFaltantes.text = "¡Animal liberado!";
                    if (animal != null)
                    {
                        animal.GetComponent<MovimientoAnimal>()?.ActivarMovimiento();
                    }
                    spriteRenderer.sprite = spriteJaulaRota;
this.enabled = false; // Desactiva este script para que ya no interactúe

                }
                else
                {
                    textoLlavesFaltantes.text = "Faltan " + faltan + " llaves";
                }
            }
        }
        else
        {
            textoLlavesFaltantes.text = "";
        }
    }
}
