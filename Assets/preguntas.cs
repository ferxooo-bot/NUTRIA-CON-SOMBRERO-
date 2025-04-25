using TMPro;
using UnityEngine;

public class pregunta : MonoBehaviour
{
    public GameObject canvasPregunta;
    public TextMeshProUGUI textoPregunta;
    public TextMeshProUGUI[] textosRespuestas;
    private AudioSource audioSource;
    public GameObject puerta;
    private int repuestaC = 1;
    [SerializeField] private float rangoDeteccion = 7f;
    private GameObject jugador;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        canvasPregunta.SetActive(false);
        
        // Asegurarte de obtener el jugador
        jugador = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // Calcular distancia
        float distancia = Vector2.Distance(transform.position, jugador.transform.position);

        if (distancia < rangoDeteccion)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                MostrarPregunta();
                audioSource.Play();
            }

            if (Input.GetKeyDown(KeyCode.A)) { revisarR(0); }
            if (Input.GetKeyDown(KeyCode.B)) { revisarR(1); }
            if (Input.GetKeyDown(KeyCode.C)) { revisarR(2); }
            if (Input.GetKeyDown(KeyCode.D)) { revisarR(3); }
        }
    }

    void MostrarPregunta()
    {
        canvasPregunta.SetActive(true);
        textoPregunta.text = "¿Cuál de las siguientes acciones ayuda a cuidar el medio ambiente?";
        textosRespuestas[0].text = "A. Dejar luces encendidas todo el día";
        textosRespuestas[1].text = "B. Reciclar y reutilizar materiales";
        textosRespuestas[2].text = "C. Usar plástico de un solo uso";
        textosRespuestas[3].text = "D. Tirar basura en la calle";
    }

    void revisarR(int index)
    {
        if (index == repuestaC)
        {
            Destroy(puerta);
            cerrar();
        }
    }

    void cerrar()
    {
        canvasPregunta.SetActive(false);
    }
}