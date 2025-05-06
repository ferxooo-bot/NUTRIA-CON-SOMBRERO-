using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class ImprovedFishMovement : MonoBehaviour
{
    [Header("Detección de Obstáculos")]
    public int rayCount = 7;
    public float raySpreadAngle = 90f;
    public float rayDistance = 0.5f;
    public float sensorRadius = 0.1f;
    public LayerMask obstacleLayer;

    [Header("Movimiento y Giro")]
    public float speed = 2f;
    public float rotationSpeed = 180f;

    [Header("Movimiento Aleatorio Sutil")]
    public float randomWiggleStrength = 5f;  // Máximo de variación angular (grados)
    public float wiggleSpeed = 0.3f;         // Qué tan rápido cambia la variación

    [Header("Comportamiento de Cardumen")]
    public bool enableFlocking = true;       // Activar/desactivar comportamiento de cardumen
    public float detectionRadius = 2.0f;     // Distancia para detectar otros peces
    public LayerMask fishLayer;              // Capa donde están los peces
    [Range(0, 2)] public float cohesionWeight = 1.0f;      // Fuerza hacia el centro del grupo
    [Range(0, 2)] public float separationWeight = 1.2f;    // Fuerza para evitar otros peces
    [Range(0, 2)] public float alignmentWeight = 0.8f;     // Fuerza para alinearse con otros
    public float minSeparationDistance = 0.5f;             // Distancia mínima entre peces
    [Range(0, 1)] public float flockingInfluence = 0.5f;   // Influencia del cardumen vs navegación individual

    private Vector2 currentDirection;
    private float colliderRadius;
    private float randomSeed;
    private List<GameObject> nearbyFish = new List<GameObject>();

    void Start()
    {
        // Inicializa la dirección del pez (hacia la derecha)
        currentDirection = Vector2.right;
        
        // Obtiene el radio del collider para las detecciones
        Collider2D col = GetComponent<Collider2D>();
        if (col is CircleCollider2D cc)
            colliderRadius = cc.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
        else if (col is BoxCollider2D bc)
            colliderRadius = (Mathf.Max(bc.size.x, bc.size.y) * 0.5f) * Mathf.Max(transform.localScale.x, transform.localScale.y);
        else
            colliderRadius = sensorRadius;

        // Cada pez tiene su propio offset para movimiento aleatorio
        randomSeed = Random.Range(0f, 1000f);
        
        // Asigna una rotación inicial aleatoria
        float randomAngle = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0, 0, randomAngle);
        currentDirection = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );
    }

    void Update()
    {
        // PASO 1: Calcular comportamiento individual basado en obstáculos y wiggle
        Vector2 obstacleAvoidanceDirection = CalculateObstacleAvoidance();
        
        // PASO 2: Si el cardumen está activado, calcula su influencia
        Vector2 flockingDirection = Vector2.zero;
        if (enableFlocking)
        {
            flockingDirection = CalculateFlockingBehavior();
        }

        // PASO 3: Combinar ambos comportamientos
        Vector2 desiredDirection = Vector2.zero;
        
        if (obstacleAvoidanceDirection != Vector2.zero)
        {
            // Si hay obstáculos cerca, prioriza evitarlos
            desiredDirection = obstacleAvoidanceDirection;
            
            // Agrega algo de influencia del cardumen si está activo
            if (enableFlocking && flockingDirection != Vector2.zero)
            {
                desiredDirection = Vector2.Lerp(desiredDirection, flockingDirection, flockingInfluence * 0.5f);
            }
        }
        else if (flockingDirection != Vector2.zero)
        {
            // Si no hay obstáculos pero hay cardumen, sigue al cardumen
            desiredDirection = Vector2.Lerp(currentDirection, flockingDirection, flockingInfluence);
        }
        else
        {
            // Si no hay ni obstáculos ni cardumen, sigue en la dirección actual
            desiredDirection = currentDirection;
        }
        
        // PASO 4: Aplica movimiento aleatorio (wiggle)
        float curAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg;
        
        // Variación sutil con Perlin Noise
        float noise = Mathf.PerlinNoise(Time.time * wiggleSpeed, randomSeed);
        float wiggleAngle = (noise - 0.5f) * 2f * randomWiggleStrength;
        targetAngle += wiggleAngle;
        
        // PASO 5: Girar suavemente hacia la dirección deseada
        float newAngle = Mathf.MoveTowardsAngle(curAngle, targetAngle, rotationSpeed * Time.deltaTime);
        currentDirection = new Vector2(
            Mathf.Cos(newAngle * Mathf.Deg2Rad),
            Mathf.Sin(newAngle * Mathf.Deg2Rad)
        ).normalized;
        
        // PASO 6: Actualizar rotación y posición
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

    private Vector2 CalculateObstacleAvoidance()
    {
        List<(Vector2 dir, float score)> freeDirs = new List<(Vector2 dir, float score)>();
        float halfSpread = raySpreadAngle * 0.5f;
        
        for (int i = 0; i < rayCount; i++)
        {
            float angleOffset = -halfSpread + (raySpreadAngle / (rayCount - 1)) * i;
            Vector2 dir = RotateVector(currentDirection, angleOffset).normalized;
            
            Vector2 origin = (Vector2)transform.position + dir * colliderRadius;
            Debug.DrawRay(origin, dir * rayDistance, Color.red);
            
            RaycastHit2D hit = Physics2D.CircleCast(origin, sensorRadius, dir, rayDistance, obstacleLayer);
            if (hit.collider == null)
            {
                float alignment = Vector2.Dot(currentDirection, dir);
                float randomness = Random.Range(0.85f, 1.15f);
                float score = alignment * randomness;
                freeDirs.Add((dir, score));
            }
        }
        
        if (freeDirs.Count > 0)
        {
            freeDirs.Sort((a, b) => b.score.CompareTo(a.score));
            return freeDirs[0].dir;
        }
        else if (Physics2D.CircleCast(transform.position, sensorRadius, currentDirection, rayDistance, obstacleLayer))
        {
            // Si estamos atrapados, retroceder
            return -currentDirection;
        }
        
        return currentDirection; // Mantener dirección actual si no hay problemas
    }
    
    private Vector2 CalculateFlockingBehavior()
    {
        // Encontrar peces cercanos
        nearbyFish.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, fishLayer);
        
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                nearbyFish.Add(col.gameObject);
            }
        }
        
        if (nearbyFish.Count == 0)
            return Vector2.zero;
        
        // 1. Cohesión: moverse hacia el centro del grupo
        Vector2 cohesionVector = Vector2.zero;
        Vector2 centerOfMass = Vector2.zero;
        foreach (GameObject fish in nearbyFish)
        {
            centerOfMass += (Vector2)fish.transform.position;
        }
        centerOfMass /= nearbyFish.Count;
        cohesionVector = ((Vector2)centerOfMass - (Vector2)transform.position).normalized;
        
        // 2. Separación: mantener distancia
        Vector2 separationVector = Vector2.zero;
        int tooCloseCount = 0;
        foreach (GameObject fish in nearbyFish)
        {
            float distance = Vector2.Distance(fish.transform.position, transform.position);
            if (distance < minSeparationDistance)
            {
                Vector2 awayFromFish = ((Vector2)transform.position - (Vector2)fish.transform.position).normalized;
                awayFromFish /= Mathf.Max(0.1f, distance);  // Más fuerza cuanto más cerca
                separationVector += awayFromFish;
                tooCloseCount++;
            }
        }
        if (tooCloseCount > 0)
        {
            separationVector /= tooCloseCount;
        }
        
        // 3. Alineación: nadar en la misma dirección
        Vector2 alignmentVector = Vector2.zero;
        foreach (GameObject fish in nearbyFish)
        {
            // Inferimos la dirección basada en la rotación del pez
            float angle = fish.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            alignmentVector += direction;
        }
        alignmentVector = alignmentVector.normalized;
        
        // Combinar los tres comportamientos
        Vector2 flockingDirection = (cohesionVector * cohesionWeight +
                                    separationVector * separationWeight +
                                    alignmentVector * alignmentWeight).normalized;
                                    
        // Debug - visualización
        if (flockingDirection != Vector2.zero)
        {
            Debug.DrawRay(transform.position, flockingDirection * 0.5f, Color.yellow);
        }
        
        return flockingDirection;
    }
    
    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
            v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad)
        );
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualizar el radio de detección de cardumen
        if (enableFlocking)
        {
            Gizmos.color = new Color(0, 1, 1, 0.2f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // Visualizar el radio de separación
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, minSeparationDistance);
        }
        
        // Visualizar sensores de obstáculos
        if (Application.isPlaying)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.2f);
            float halfSpread = raySpreadAngle * 0.5f;
            for (int i = 0; i < rayCount; i++)
            {
                float angleOffset = -halfSpread + (raySpreadAngle / (rayCount - 1)) * i;
                Vector2 dir = RotateVector(currentDirection, angleOffset).normalized;
                Gizmos.DrawRay((Vector2)transform.position + dir * colliderRadius, dir * rayDistance);
            }
        }
    }
}