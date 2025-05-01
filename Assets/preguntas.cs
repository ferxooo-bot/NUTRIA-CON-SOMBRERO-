using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class pregunta : MonoBehaviour
{
    [SerializeField] public GameObject canvasPregunta;
    [SerializeField] public TextMeshProUGUI textoPregunta;
    [SerializeField] public TextMeshProUGUI[] textosRespuestas;
    [SerializeField] public GameObject puerta;
    [SerializeField] public int repuestaC; // Ahora se asigna desde el Inspector
    [SerializeField] private float rangoDeteccion = 7f;
    private GameObject jugador;
    [SerializeField] public AudioSource rCorrecta;
    [SerializeField] public AudioSource rIncorrecta;
    [SerializeField] public AudioSource sonidoA;
    [SerializeField] public int intentosRestantes = 2;
    
    [SerializeField] public GameObject mensajeCorrecto;
    [SerializeField] public GameObject mensajeInCorrecto; 
    
    public FatherMoment1 fatherMoment1Script;

    void Start()
    {
        canvasPregunta.SetActive(false);
        mensajeCorrecto.SetActive(false);
        mensajeInCorrecto.SetActive(false);
        
        jugador = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        float distancia = Vector2.Distance(transform.position, jugador.transform.position);

        if (distancia < rangoDeteccion)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                
                MostrarPregunta();
                sonidoA.Play();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) { revisarR(0); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { revisarR(1); }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { revisarR(2); }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { revisarR(3); }
            if (Input.GetKeyDown(KeyCode.Escape)) { cerrar(); }
        }
    }

    void MostrarPregunta()
    {
        
        canvasPregunta.SetActive(true);
        fatherMoment1Script.puedeMoverse = false; 
        textoPregunta.text = "¿Cuál de las siguientes acciones ayuda a cuidar el medio ambiente?";
        textosRespuestas[0].text = "1. Dejar luces encendidas todo el día";
        textosRespuestas[1].text = "2. Reciclar y reutilizar materiales";
        textosRespuestas[2].text = "3. Usar plástico de un solo uso";
        textosRespuestas[3].text = "4. Tirar basura en la calle";
    }

    void revisarR(int index)
    {
        if (intentosRestantes > 0)
        {
            if (index == repuestaC)
            {
                intentosRestantes--;
                StartCoroutine(EsperarYCerrarC());
            }
            else if (index <= 3 && index >= 0)
            {
                intentosRestantes--;
                if (intentosRestantes > 0)
                {
                    StartCoroutine(EsperarI());
                }
                else
                {
                    StartCoroutine(EsperarIYDesactivar());
                }
            }
        }
        else
        {
            DesactivarScript();
        }
    }
    
    IEnumerator EsperarYCerrarC()
    {
        cerrar();
        mensajeCorrecto.SetActive(true);
        rCorrecta.Play();
        
        yield return new WaitForSeconds(3f);
        
        mensajeCorrecto.SetActive(false);
        Destroy(puerta);        
    }
    IEnumerator EsperarI()
    {
        cerrar();
        mensajeInCorrecto.SetActive(true);
        rIncorrecta.Play();
        
        yield return new WaitForSeconds(3f);
        
        mensajeInCorrecto.SetActive(false);
    }

    IEnumerator EsperarIYDesactivar()
    {
        cerrar();
        mensajeInCorrecto.SetActive(true);
        rIncorrecta.Play();
        yield return new WaitForSeconds(3f);
        mensajeInCorrecto.SetActive(false);
        DesactivarScript();
    }

    void cerrar()
    {
        canvasPregunta.SetActive(false);
        fatherMoment1Script.puedeMoverse = true; 
    }

    void DesactivarScript()
    {
        canvasPregunta.SetActive(false);
        this.enabled = false;
    }
}
