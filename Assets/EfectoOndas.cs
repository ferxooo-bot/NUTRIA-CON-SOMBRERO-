using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EfectoOndas : MonoBehaviour
{
    public float velocidadCrecimiento = 3f;
    public float tiempoVida = 1f;
    public Color colorInicial = new Color(0.5f, 1f, 0.5f, 0.8f); // Verde semi-transparente
    public Color colorFinal = new Color(0.5f, 1f, 0.5f, 0f);     // Verde transparente
    
    private SpriteRenderer spriteRenderer;
    private float tiempoTranscurrido = 0f;
    public bool seguirJugador = true; // Activar si debe seguir al jugador
    public Transform objetivo; // Objeto a seguir (asignar al jugador)
    
    void Start()
    {
        // Obtener componente
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (seguirJugador && objetivo != null)
        {
            transform.position = objetivo.position;
        }
        
        // Configurar color inicial
        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorInicial;
        }
        
        // Comenzar con escala pequeña
        transform.localScale = Vector3.one * 0.1f;
        
        // Destruir después del tiempo de vida
        Destroy(gameObject, tiempoVida);
    }
    
    void Update()
    {
        // Incrementar escala
        transform.localScale += Vector3.one * velocidadCrecimiento * Time.deltaTime;
        
        // Transición de color
        tiempoTranscurrido += Time.deltaTime;
        float t = tiempoTranscurrido / tiempoVida;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(colorInicial, colorFinal, t);
        }
    }
}