using Unity.VisualScripting;
using UnityEngine;

public class Aviso : MonoBehaviour
{
        [SerializeField] private GameObject promptUI;
        [SerializeField] private float rangoDeteccion = 2f;
        private GameObject jugador;
        
    
    void Start()
    {
        if (promptUI == null) 
        {
            promptUI.SetActive(false);
        }
        
        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player");
        }
    }

    // Update is called once per frame
    void Update()
    {
        float distancia = Vector2.Distance(transform.position, jugador.transform.position);
        if (distancia < rangoDeteccion)
        {
            MostrarUI();
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                MostrarMenu();
            }
        }
        else
        {
            OcultarUI();
        }
    }
    
    void MostrarUI()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
            Debug.Log("Mostrando UI");
        }
    }
    void OcultarUI()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
            Debug.Log("Ocultando UI");
        }
        
    }
    void MostrarMenu()
    {
        
        Debug.Log("¡Presionaste E cerca del objeto!");
    }
}
