using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [Header("Item Configuration")]
    [SerializeField] private string itemType = "keys"; // Tipo de item: "keys", "food", "coins"
    [SerializeField] private int amount = 1;           // Cantidad que se añadirá al inventario
    [SerializeField] private bool destroyOnCollect = true; // Si el objeto debe destruirse al recogerlo
    
    [Header("Effects")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float floatAmplitude = 0.2f;  // Opcional: Para efecto de flotación
    [SerializeField] private float floatFrequency = 1.0f;  // Opcional: Para efecto de flotación
    
    [Header("Attraction Settings")]
    [SerializeField] private bool useAttractionEffect = true;  // Habilitar/deshabilitar la atracción
    [SerializeField] private float attractionRange = 3.0f;     // Distancia a la que comienza la atracción
    [SerializeField] private float attractionSpeed = 6.0f;     // Velocidad base de atracción
    [SerializeField] private float attractionAcceleration = 12.0f; // Aceleración de la atracción
    
    private Vector3 startPos;
    private AudioSource audioSource;
    private Transform playerTransform;
    private float currentSpeed = 0f;
    private bool isAttracting = false;
    
    void Start()
    {
        startPos = transform.position;
        
        // Añadir AudioSource si se ha proporcionado un sonido de colección
        if (collectSound != null && GetComponent<AudioSource>() == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = collectSound;
            audioSource.playOnAwake = false;
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        // Intentar encontrar al jugador al inicio
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    void Update()
    {
        // Si no estamos en modo de atracción, aplicar el efecto de flotación
        if (!isAttracting && floatAmplitude > 0)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        
        // Verificar si debemos empezar a atraer hacia el jugador
        if (useAttractionEffect && playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            // Si el jugador está dentro del rango de atracción
            if (distanceToPlayer <= attractionRange)
            {
                isAttracting = true;
                
                // Incrementar la velocidad actual con la aceleración
                currentSpeed += attractionAcceleration * Time.deltaTime;
                
                // Limitar la velocidad máxima basada en la distancia
                // Cuanto más cerca, más rápido (hasta un máximo)
                float maxSpeed = attractionSpeed * (1 + (attractionRange - distanceToPlayer) / attractionRange);
                currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
                
                // Calcular dirección hacia el jugador
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                
                // Mover el objeto hacia el jugador con la velocidad actual
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    playerTransform.position,
                    currentSpeed * Time.deltaTime
                );
                
                // Opcional: Añadir una pequeña rotación mientras se atrae
                transform.Rotate(0, 0, 360 * Time.deltaTime);
            }
            else
            {
                // Resetear si el jugador sale del rango
                isAttracting = false;
                currentSpeed = 0f;
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprobar si el objeto que colisiona es el jugador
        if (collision.CompareTag("Player"))
        {
            // Intentar obtener el componente InventoryPlayer del jugador
            InventoryPlayer playerInventory = collision.GetComponent<InventoryPlayer>();
            
            // Si el jugador tiene un inventario, añadir el ítem
            if (playerInventory != null)
            {
                // Añadir el ítem al inventario del jugador
                playerInventory.AddItem(itemType, amount);
                
                // Reproducir sonido si existe
                if (audioSource != null && collectSound != null)
                {
                    audioSource.Play();
                }
                
                // Si está configurado para destruirse después de recogerlo
                if (destroyOnCollect)
                {
                    // Si hay un sonido, esperar a que termine antes de destruir
                    if (audioSource != null && collectSound != null)
                    {
                        // Desactivar el renderer para que parezca que ya se recogió
                        Renderer renderer = GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.enabled = false;
                        }
                        
                        // Desactivar el collider para evitar activaciones múltiples
                        Collider2D collider = GetComponent<Collider2D>();
                        if (collider != null)
                        {
                            collider.enabled = false;
                        }
                        
                        // Destruir después de que el sonido termine
                        Destroy(gameObject, collectSound.length);
                    }
                    else
                    {
                        // Si no hay sonido, destruir inmediatamente
                        Destroy(gameObject);
                    }
                }
            }
            else
            {
                Debug.LogWarning("El jugador no tiene componente InventoryPlayer asignado.");
            }
        }
    }
    
    // Método auxiliar para visualizar el rango de atracción en el editor
    private void OnDrawGizmosSelected()
    {
        if (useAttractionEffect)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, attractionRange);
        }
    }
}