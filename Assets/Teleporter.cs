using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform destino; // El punto de destino donde se teletransportará el jugador
    [SerializeField] private float rangoDeteccion = 2f; // Radio para detectar al jugador
    [SerializeField] private KeyCode teclaTeleport = KeyCode.E; // Tecla para activar el teletransporte (opcional)
    [SerializeField] private bool requiereTecla = true; // ¿Se necesita presionar una tecla?
    [SerializeField] private float tiempoEspera = 0.5f; // Tiempo antes de poder volver a teletransportarse
    
    [Header("Efectos")]
    [SerializeField] private bool usarEfectoVisual = true;
    [SerializeField] private GameObject efectoTeletransporte; // Prefab de un efecto visual (opcional)
    
    private bool puedeUsarse = true;
    private GameObject jugador;
    [Header("Interfaz de Usuario")]
    [SerializeField] private GameObject promptUI; // Arrastra el Canvas desde el editor


    private AudioSource audioSource;
    private bool playerInRange = false;
    
    
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (promptUI != null)
        {
            promptUI.SetActive(false); // Ocultar UI al inicio
        }
        
        // Buscar al jugador si no se ha asignado
        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player");
        }
        
        // Verificar que el destino esté configurado
        if (destino == null)
        {
            Debug.LogError("¡Por favor asigna un punto de destino al teleportador!");
            enabled = false; // Desactivar el script si no hay destino
        }
        
        // Verificar que el destino no sea el mismo objeto del teleportador
        if (destino == transform)
        {
            Debug.LogError("¡El destino no puede ser el mismo objeto teleportador!");
            enabled = false; // Desactivar el script si el destino es inválido
        }
        
        
    }
    
    void Update()
    {
        if (!puedeUsarse || destino == null || jugador == null) return;
        
        // Calcular distancia entre el jugador y el teleportador
        float distancia = Vector2.Distance(transform.position, jugador.transform.position);
        
        // Verificar si el jugador está en rango
        if (distancia <= rangoDeteccion)
        {
            // Interfaz de presione E
            playerInRange = true;
            MostrarUI();
            
            // Si requiere tecla, verificar si se ha presionado
            if (!requiereTecla || (requiereTecla && Input.GetKeyDown(teclaTeleport)))
            {
                StartCoroutine(TeletransportarJugador());
                audioSource.Play();
            }
            
        } else
        {
            playerInRange = false;
            OcultarUI();
        }
    }


    void MostrarUI()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);

        }
    }

    void OcultarUI()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);

        }
    }
    
    IEnumerator TeletransportarJugador()
    {
        puedeUsarse = false;
        
        GameObject efecto = Instantiate(
            efectoTeletransporte, 
            jugador.transform.position, 
            Quaternion.identity, 
            jugador.transform // ← Esto hace que sea hijo del jugador
        );
        
        // Configurar seguimiento
        EfectoOndas efectoScript = efecto.GetComponent<EfectoOndas>();
        if (efectoScript != null)
        {
            efectoScript.seguirJugador = true;
            efectoScript.objetivo = jugador.transform;
        }
        // Efecto visual antes de teletransporte (opcional)
        Instantiate(efectoTeletransporte, jugador.transform.position, Quaternion.identity);
        Debug.Log("[DEBUG] Efecto instanciado en: " + jugador.transform.position);
        
        // Pequeña pausa para el efecto
        yield return new WaitForSeconds(0.1f);
        
        // Teletransportar al jugador
        Vector3 nuevaPosicion = destino.position;
        jugador.transform.position = nuevaPosicion;
        
        // Debug para verificar después del teletransporte
        Debug.Log("Posición del jugador después del teletransporte: " + jugador.transform.position);
        
        // Tiempo de espera antes de poder usar nuevamente
        yield return new WaitForSeconds(tiempoEspera);
        
        puedeUsarse = true;
    }
    
    // Dibujar el rango de detección en el editor (solo visible en el editor)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        
        if (destino != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, destino.position);
            Gizmos.DrawWireSphere(destino.position, 0.5f);
        }
    }
    
}