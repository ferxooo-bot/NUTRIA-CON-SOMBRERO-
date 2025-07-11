using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class pregunta : MonoBehaviour
{
    
    [SerializeField] public string doorId; 
    [SerializeField] public GameObject canvasPregunta;
    [SerializeField] public TextMeshProUGUI textoPregunta;
    [SerializeField] public TextMeshProUGUI[] textosRespuestas;
    [SerializeField] public string preguntaT;
    [SerializeField] public string re1;
    [SerializeField] public string re2;
    [SerializeField] public string re3;
    [SerializeField] public string re4;
    
    [SerializeField] public GameObject puerta;
    [SerializeField] public int repuestaC = 1; 
    [SerializeField] private float rangoDeteccion = 7f;
    private GameObject jugador;
    [SerializeField] public AudioSource rCorrecta;
    [SerializeField] public AudioSource rIncorrecta;
    [SerializeField] public AudioSource sonidoA;
    [SerializeField] public int intentosRestantes = 2;
    
    [SerializeField] public GameObject mensajeCorrecto;
    [SerializeField] public GameObject mensajeInCorrecto; 

    [SerializeField] public DoorController doorController;     
    
    private Aviso aviso;
    
    [SerializeField] public FatherMoment1 fatherMoment1Script;
    
    void Start()
    {
        // Corregido: Asignar al campo de la clase, no crear una variable local
        doorController = puerta.GetComponent<DoorController>();
        aviso = GetComponent<Aviso>();
        canvasPregunta.SetActive(false);
        mensajeCorrecto.SetActive(false);
        mensajeInCorrecto.SetActive(false);
        
        jugador = GameObject.FindGameObjectWithTag("Player");
        
        // Si fatherMoment1Script no ha sido asignado en el inspector, intentamos encontrarlo
        if (fatherMoment1Script == null)
        {
            fatherMoment1Script = FindObjectOfType<FatherMoment1>();
        }

        // Verificar que todos los componentes necesarios estén asignados
        if (puerta == null)
        {
            Debug.LogError("La puerta no está asignada en el inspector para " + gameObject.name);
        }
        
        if (doorController == null && puerta != null)
        {
            Debug.LogWarning("No se encontró DoorController en la puerta. Verificar que la puerta tenga este componente: " + puerta.name);
        }
    }

    void Update()
    {
        float distancia = Vector2.Distance(transform.position, jugador.transform.position);

        if (distancia < rangoDeteccion)
        {
            if (Input.GetKeyDown(KeyCode.E) && intentosRestantes > 0)
            {
                MostrarPregunta();
                sonidoA.Play();
            }
            if (canvasPregunta.activeSelf) {
                if (Input.GetKeyDown(KeyCode.Alpha1)) { revisarR(0); }
                if (Input.GetKeyDown(KeyCode.Alpha2)) { revisarR(1); }
                if (Input.GetKeyDown(KeyCode.Alpha3)) { revisarR(2); }
                if (Input.GetKeyDown(KeyCode.Alpha4)) { revisarR(3); }
                if (Input.GetKeyDown(KeyCode.Escape)) { cerrar(); }                                         
            }
        }
    }

    void MostrarPregunta()
    {
        canvasPregunta.SetActive(true);
        if (fatherMoment1Script != null)
        {
            fatherMoment1Script.puedeMoverse = false;
        }
        textoPregunta.text = preguntaT;
        textosRespuestas[0].text = re1;
        textosRespuestas[1].text = re2;
        textosRespuestas[2].text = re3;
        textosRespuestas[3].text = re4;
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
        
        // Aseguramos que se abra la puerta y luego se destruya
        if (doorController != null)
        {
            Debug.Log("Llamando a AddNewDoorOpen para la puerta: " + puerta.name);
            doorController.AddNewDoorOpen();
        }
        else
        {
            Debug.LogWarning("DoorController es null, no se puede llamar AddNewDoorOpen");
        }
        
        // Aseguramos que la puerta se destruya
        if (puerta != null)
        {
            Debug.Log("Destruyendo la puerta: " + puerta.name);
            Destroy(puerta);
        }
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
        if (fatherMoment1Script != null)
        {
            fatherMoment1Script.puedeMoverse = true;
        }
    }

    void DesactivarScript()
    {
        cerrar();
        this.enabled = false;
        if (fatherMoment1Script != null)
        {
            fatherMoment1Script.puedeMoverse = true;
        }
        if (aviso != null)
        {
            aviso.OcultarUI();
            aviso.enabled = false;
        }
    }
}