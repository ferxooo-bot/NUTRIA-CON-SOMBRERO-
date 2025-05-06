using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class Fish2Movement : MonoBehaviour
{
    [Header("Detección de Obstáculos")]
    public int rayCount = 7;
    public float raySpreadAngle = 90f;
    public float rayDistance = 0.5f;
    public float sensorRadius = 0.1f;
    public LayerMask obstacleLayer;
    public LayerMask fishLayer; // Capa para detectar otros peces

    [Header("Movimiento y Giro")]
    public float maxSpeed = 2f;
    public float minSpeed = 0.8f;  // Velocidad mínima
    public float acceleration = 0.5f; // Aceleración
    public float rotationSpeed = 180f;
    public float directionSmoothTime = 0.3f; // Tiempo de suavizado para cambios de dirección

    [Header("Movimiento Orgánico")]
    public float baseWiggleFrequency = 2f;   // Frecuencia base de ondulación
    public float wiggleAmplitude = 5f;       // Amplitud de la ondulación
    public float speedWiggleInfluence = 1f;  // Cuánto la velocidad afecta a la ondulación

    [Header("Comportamiento")]
    public float avoidObstacleWeight = 3.0f; // Peso para evitar obstáculos
    public float alignmentWeight = 1.5f;     // Peso para alinearse con la dirección actual
    public float schoolingWeight = 1.0f;     // Peso para cardumen
    public float schoolingRadius = 2.0f;     // Radio de detección de otros peces

    // Variables privadas
    private Vector2 currentDirection = Vector2.right;
    private Vector2 smoothedDirection;
    private Vector2 directionVelocity; // Para SmoothDamp
    private float currentSpeed;
    private float colliderRadius;
    private float randomSeed;
    private float wiggleTimer;

    void Start()
    {
        // Inicializar el radio del collider
        Collider2D col = GetComponent<Collider2D>();
        if (col is CircleCollider2D cc)
            colliderRadius = cc.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
        else if (col is BoxCollider2D bc)
            colliderRadius = (Mathf.Max(bc.size.x, bc.size.y) * 0.5f) * Mathf.Max(transform.localScale.x, transform.localScale.y);
        else
            colliderRadius = sensorRadius;

        // Valores iniciales
        randomSeed = Random.Range(0f, 1000f);
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        smoothedDirection = currentDirection;
        wiggleTimer = randomSeed; // Comienza con un valor aleatorio para variedad
    }

    void Update()
    {
        // Calcular direcciones libres y sus puntuaciones
        List<(Vector2 dir, float score)> directionScores = EvaluateDirections();
        
        // Obtener la mejor dirección según las puntuaciones
        Vector2 desiredDirection = CalculateDesiredDirection(directionScores);
        
        // Detectar y responder a otros peces (comportamiento de cardumen)
        Vector2 schoolingDirection = CalculateSchoolingInfluence();
        
        // Combinar las diferentes influencias con sus pesos
        desiredDirection = (desiredDirection * avoidObstacleWeight + schoolingDirection * schoolingWeight).normalized;
        
        // Suavizar el cambio de dirección usando SmoothDamp
        smoothedDirection = Vector2.SmoothDamp(
            smoothedDirection, 
            desiredDirection, 
            ref directionVelocity, 
            directionSmoothTime);

        // Actualizar velocidad con aceleración/desaceleración natural
        UpdateSpeed(directionScores.Count > 0);
        
        // Calcular el ángulo con ondulación orgánica
        float baseAngle = Mathf.Atan2(smoothedDirection.y, smoothedDirection.x) * Mathf.Rad2Deg;
        float wiggleAngle = CalculateOrganicWiggle();
        float finalAngle = baseAngle + wiggleAngle;

        // Aplicar rotación y movimiento
        transform.rotation = Quaternion.Euler(0f, 0f, finalAngle);
        transform.Translate(smoothedDirection * currentSpeed * Time.deltaTime, Space.World);
        
        // Actualizar la dirección actual
        currentDirection = smoothedDirection;
    }

    private List<(Vector2 dir, float score)> EvaluateDirections()
    {
        List<(Vector2 dir, float score)> scoredDirections = new List<(Vector2 dir, float score)>();
        float halfSpread = raySpreadAngle * 0.5f;

        for (int i = 0; i < rayCount; i++)
        {
            float angleOffset = -halfSpread + (raySpreadAngle / (rayCount - 1)) * i;
            Vector2 dir = RotateVector(currentDirection, angleOffset).normalized;

            Vector2 origin = (Vector2)transform.position + dir * colliderRadius;
            
            // Visualizar rayos en el editor
            Debug.DrawRay(origin, dir * rayDistance, Color.red);

            // Detectar obstáculos
            RaycastHit2D hit = Physics2D.CircleCast(origin, sensorRadius, dir, rayDistance, obstacleLayer);
            
            if (hit.collider == null)
            {
                // Calcular puntuación basada en varios factores
                float alignment = Vector2.Dot(currentDirection, dir); // Preferencia por seguir adelante
                float distanceFromCenter = Mathf.Abs(angleOffset) / halfSpread; // Preferencia por el centro
                
                // Añadir una pequeña variación aleatoria
                float randomVariation = Random.Range(0.9f, 1.1f);
                
                // Combinar factores en una puntuación final
                float score = (alignment * 1.2f) * (1.0f - distanceFromCenter * 0.5f) * randomVariation;
                
                scoredDirections.Add((dir, score));
            }
        }

        // Ordenar por puntuación (mayor a menor)
        scoredDirections.Sort((a, b) => b.score.CompareTo(a.score));
        return scoredDirections;
    }

    private Vector2 CalculateDesiredDirection(List<(Vector2 dir, float score)> directions)
    {
        if (directions.Count > 0)
        {
            // Usar la dirección mejor puntuada
            return directions[0].dir;
        }
        else
        {
            // Emergencia: invertir dirección si no hay opciones seguras
            Vector2 escapeDir = -currentDirection;
            
            // Intentar encontrar una dirección de escape lateral si la inversión tampoco funciona
            RaycastHit2D forwardHit = Physics2D.CircleCast(
                (Vector2)transform.position + escapeDir * colliderRadius, 
                sensorRadius, escapeDir, rayDistance, obstacleLayer);
            
            if (forwardHit.collider != null)
            {
                // Probar dirección perpendicular
                escapeDir = RotateVector(escapeDir, 90f);
            }
            
            return escapeDir;
        }
    }

    private Vector2 CalculateSchoolingInfluence()
    {
        Vector2 alignmentVector = Vector2.zero;
        int fishCount = 0;

        // Detectar otros peces en el radio de cardumen
        Collider2D[] nearbyFish = Physics2D.OverlapCircleAll(transform.position, schoolingRadius, fishLayer);
        
        foreach (Collider2D fish in nearbyFish)
        {
            if (fish.gameObject == gameObject) continue; // Ignorarse a sí mismo
            
            Fish2Movement otherFish = fish.GetComponent<Fish2Movement>();
            if (otherFish != null)
            {
                alignmentVector += otherFish.currentDirection;
                fishCount++;
            }
        }

        // Calcular dirección promedio del cardumen
        if (fishCount > 0)
        {
            alignmentVector /= fishCount;
            alignmentVector.Normalize();
            return alignmentVector;
        }
        
        return currentDirection; // Si no hay otros peces, mantener dirección actual
    }

    private void UpdateSpeed(bool pathIsClear)
    {
        // Acelerar si hay camino libre, desacelerar ante obstáculos
        float targetSpeed = pathIsClear ? maxSpeed : minSpeed;
        
        // Suavizar cambios de velocidad
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
    }

    private float CalculateOrganicWiggle()
    {
        // Incrementar el temporizador
        wiggleTimer += Time.deltaTime * (baseWiggleFrequency + currentSpeed * speedWiggleInfluence);
        
        // Usar una combinación de funciones sinusoidales para un movimiento más natural
        float primaryWave = Mathf.Sin(wiggleTimer) * wiggleAmplitude;
        float secondaryWave = Mathf.Sin(wiggleTimer * 2.7f) * (wiggleAmplitude * 0.3f);
        
        return primaryWave + secondaryWave;
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
            v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad)
        );
    }

    // Para visualizar el radio de cardumen en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, schoolingRadius);
    }
}