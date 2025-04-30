using UnityEngine;

public class EnemyHunting : MonoBehaviour
{
    [Header("Configuración de Visión")]
    [SerializeField] private float rangoVision = 3f;
    [SerializeField] private float anguloVision = 60f;
    [SerializeField] private LayerMask targetMask; // Capa del jugador
    [SerializeField] private LayerMask obstacleMask; // Capa de obstáculos

    [Header("Configuración de Persecución")]
    [SerializeField] private float velocidadPersecucion = 5f;
    
    [Header("Configuración de Ataque")]
    [SerializeField] private int contactoDano = 3; // Daño por contacto
    [SerializeField] private float contactoKnockback = 5f; // Fuerza de retroceso
    [SerializeField] private float tiempoEntreDanoContacto = 1f; // Tiempo entre daños
    private float ultimoTiempoDano; // Temporizador interno
    
    
    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    [Header("Configuración de Persecución fuera de visión")]
    [SerializeField] private float tiempoPerdidaVision = 2f; // ← Tiempo que debe estar fuera de visión para cancelar persecución

    private float tiempoFueraDeVision = 0f;
   
    private Vector2 posicionInicial;
    private bool persiguiendo = false;
    
    private bool mirandoDerecha = true;

    // NUEVO: Temporizador para cambiar la dirección de la mirada
    private float tiempoCambioMirada = 0f;
    private float intervaloCambioMirada = 4f;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        
        posicionInicial = transform.position;
    }

   private void Update()
{
    if (jugador == null) return;

    if (persiguiendo)
    {
        GestionarPersecucion();
    }
    else
    {
        GestionarBusqueda(); // Asegura que la búsqueda se reanude siempre
    }
}


    private void GestionarBusqueda()
    {
        tiempoCambioMirada += Time.deltaTime;

        if (tiempoCambioMirada >= intervaloCambioMirada)
        {
            Flip();
            tiempoCambioMirada = 0f;
        }

        if (JugadorEnVision())
        {
            IniciarPersecucion();
        }
    }

    private bool JugadorEnVision()
    {
        float distancia = Vector2.Distance(transform.position, jugador.position);
        if (distancia > rangoVision) return false;

        // 👇 Nueva lógica: filtra por altura
        if (Mathf.Abs(jugador.position.y - transform.position.y) > 1f) return false;

        // 👇 Nueva lógica: solo ve al frente
        bool jugadorADerecha = jugador.position.x > transform.position.x;
        if (jugadorADerecha != mirandoDerecha) return false;

        // 👁 Revisión de obstáculos en línea recta
        Vector2 direccionAlJugador = (jugador.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionAlJugador, distancia, obstacleMask);

        return hit.collider == null;
    }

   private void GestionarPersecucion()
{
    bool jugadorEnVisionAhora = JugadorEnVision();

    if (jugadorEnVisionAhora)
    {
        tiempoFueraDeVision = 0f; // Sigue persiguiendo sin problemas
    }
    else
    {
        tiempoFueraDeVision += Time.deltaTime;

        if (tiempoFueraDeVision >= tiempoPerdidaVision)
        {
            VolverBase();
            return;
        }
    }

    Perseguir();
}

    private void IniciarPersecucion()
    {
        persiguiendo = true;
        tiempoFueraDeVision = 0f;
        animator.SetBool("Persiguiendo", true);
    }

    private void Perseguir()
    {
        Vector2 direccion = (jugador.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direccion.x * velocidadPersecucion, rb.linearVelocity.y);
        GestionarOrientacion(direccion);
    }


    private void VolverBase()
    {
        Vector2 direccion = (posicionInicial - (Vector2)transform.position).normalized;

        if (Vector2.Distance(transform.position, posicionInicial) > 1f)
        {
            rb.linearVelocity = direccion * velocidadPersecucion;
            GestionarOrientacion(direccion);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            persiguiendo = false;
            animator.SetBool("Persiguiendo", false);

            // Reinicia el temporizador de cambio de mirada al volver a base
            tiempoCambioMirada = 0f;
        }
    }

    private void GestionarOrientacion(Vector2 direccionMovimiento)
    {
        bool debeMirarDerecha = direccionMovimiento.x > 0;
        
        if (debeMirarDerecha != mirandoDerecha)
        {
            Flip();
        }
    }

    private void Flip()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time - ultimoTiempoDano >= tiempoEntreDanoContacto)
            {
                AplicarDanoContacto(collision.transform);
                ultimoTiempoDano = Time.time;
            }
        }
    }

    private void AplicarDanoContacto(Transform jugador)
    {
        // Calcular dirección del knockback
        Vector2 direccionKnockback = (jugador.position - transform.position).normalized;
        Vector2 knockback = direccionKnockback * contactoKnockback;
    
        // Aplicar daño
        FatherMoment1 playerController = jugador.GetComponent<FatherMoment1>();
        if (playerController != null)
        {
            playerController.TakeDamage(contactoDano, knockback);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoVision);

        Vector3 direccionMirada = mirandoDerecha ? Vector3.right : Vector3.left;
        Vector3 visionA = Quaternion.Euler(0, 0, anguloVision / 2) * direccionMirada;
        Vector3 visionB = Quaternion.Euler(0, 0, -anguloVision / 2) * direccionMirada;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, visionA * rangoVision);
        Gizmos.DrawRay(transform.position, visionB * rangoVision);
    }
}
