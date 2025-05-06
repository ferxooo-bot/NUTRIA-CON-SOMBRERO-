using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class FishMovement : MonoBehaviour
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

    [Header("Comportamiento de Exploración")]
    public float wallAvoidanceWeight = 2.0f;    // Peso para evitar paredes
    public float centerAttractionWeight = 1.0f; // Peso para atraer hacia el centro
    public float wallDetectionDistance = 1.5f;  // Distancia para detectar paredes
    public float explorationModeProbability = 0.3f; // Probabilidad de entrar en modo exploración
    public float explorationDuration = 5f;      // Duración del modo exploración
    public float randomDirectionWeight = 0.8f;  // Peso para dirección aleatoria en exploración
    public Transform tankCenter;                // Centro del tanque/pecera (opcional)

    [Header("Comportamiento")]
    public float avoidObstacleWeight = 3.0f; // Peso para evitar obstáculos
    public float alignmentWeight = 1.5f;     // Peso para alinearse con la dirección actual
    public float schoolingWeight = 1.0f;     // Peso para cardumen
    public float schoolingRadius = 2.0f;     // Radio de detección de otros peces

    // Variables privadas
    private Vector2 smoothedDirection;
    private Vector2 directionVelocity; // Para SmoothDamp
    private float currentSpeed;
    private float colliderRadius;
    private float randomSeed;
    private float wiggleTimer;
    private bool isExploring = false;
    private float explorationTimer = 0f;
    private Vector2 explorationTarget;
    private float nextBehaviorChange;
    private Vector2 tankCenterPos;
    
    // Propiedad pública para la dirección actual (con setter privado)
    public Vector2 CurrentDirection { get; private set; } = Vector2.right;

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
        smoothedDirection = CurrentDirection;
        wiggleTimer = randomSeed; // Comienza con un valor aleatorio para variedad
        nextBehaviorChange = Time.time + Random.Range(3f, 8f);
        
        // Determinar centro de la pecera
        if (tankCenter != null)
            tankCenterPos = tankCenter.position;
        else
            tankCenterPos = Vector2.zero; // Valor predeterminado si no se asigna
            
        // Inicializar un objetivo aleatorio
        SetRandomExplorationTarget();
    }

    void Update()
    {
        // Actualizar temporizadores y estados
        UpdateBehaviorState();
        
        // Calcular direcciones libres y sus puntuaciones
        List<(Vector2 dir, float score)> directionScores = EvaluateDirections();
        
        // Obtener la mejor dirección según las puntuaciones
        Vector2 desiredDirection = CalculateDesiredDirection(directionScores);
        
        // Aplicar comportamiento de exploración si está activo
        if (isExploring)
        {
            Vector2 explorationDirection = CalculateExplorationDirection();
            desiredDirection = Vector2.Lerp(desiredDirection, explorationDirection, randomDirectionWeight);
        }
        
        // Detectar y responder a otros peces (comportamiento de cardumen)
        Vector2 schoolingDirection = CalculateSchoolingInfluence();
        
        // Calcular dirección para evitar paredes cercanas
        Vector2 wallAvoidanceDirection = CalculateWallAvoidance();
        
        // Combinar las diferentes influencias con sus pesos
        if (wallAvoidanceDirection != Vector2.zero)
        {
            // Si hay paredes cerca, priorizarlas
            desiredDirection = Vector2.Lerp(desiredDirection, wallAvoidanceDirection, wallAvoidanceWeight * 0.5f);
        }
        
        // Combinar con comportamiento de cardumen
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
        CurrentDirection = smoothedDirection;
    }

    private void UpdateBehaviorState()
    {
        // Actualizar temporizador de exploración si está activo
        if (isExploring)
        {
            explorationTimer -= Time.deltaTime;
            if (explorationTimer <= 0f)
            {
                isExploring = false;
                nextBehaviorChange = Time.time + Random.Range(4f, 10f);
            }
        }
        // Decidir si entrar en modo exploración
        else if (Time.time >= nextBehaviorChange)
        {
            if (Random.value < explorationModeProbability)
            {
                isExploring = true;
                explorationTimer = explorationDuration * Random.Range(0.7f, 1.3f); // Variedad en duración
                SetRandomExplorationTarget();
            }
            nextBehaviorChange = Time.time + Random.Range(3f, 8f);
        }
    }

    private void SetRandomExplorationTarget()
    {
        // Calcular un objetivo aleatorio hacia el centro de la pecera
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(0.5f, 3f);
        explorationTarget = tankCenterPos + new Vector2(
            Mathf.Cos(randomAngle) * randomDistance,
            Mathf.Sin(randomAngle) * randomDistance
        );
    }

    private Vector2 CalculateExplorationDirection()
    {
        // Dirección hacia el objetivo de exploración
        Vector2 dirToTarget = (explorationTarget - (Vector2)transform.position).normalized;
        
        // Añadir un poco de variación
        float variationAngle = Mathf.Sin(Time.time * 0.5f) * 20f;
        return RotateVector(dirToTarget, variationAngle);
    }

    private Vector2 CalculateWallAvoidance()
    {
        Vector2 avoidanceVector = Vector2.zero;
        int wallCount = 0;
        
        // Lanzar más rayos para detectar paredes
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f;
            Vector2 dir = RotateVector(Vector2.right, angle).normalized;
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, wallDetectionDistance, obstacleLayer);
            if (hit.collider != null)
            {
                // Calcular dirección de escape según distancia
                float weight = 1f - (hit.distance / wallDetectionDistance);
                avoidanceVector -= dir * weight;
                wallCount++;
                
                // Visualización del rayo de detección de pared
                Debug.DrawLine(transform.position, hit.point, Color.yellow);
            }
        }
        
        // Si detectamos paredes, calcular vector promedio de evasión
        if (wallCount > 0)
        {
            avoidanceVector /= wallCount;
            
            // A veces, añade una componente hacia el centro para evitar quedarse en esquinas
            if (Random.value < 0.4f)
            {
                Vector2 centerDirection = ((Vector2)tankCenterPos - (Vector2)transform.position).normalized;
                avoidanceVector = Vector2.Lerp(avoidanceVector, centerDirection, centerAttractionWeight);
            }
            
            return avoidanceVector.normalized;
        }
        
        return Vector2.zero; // No hay paredes cerca
    }

    private List<(Vector2 dir, float score)> EvaluateDirections()
    {
        List<(Vector2 dir, float score)> scoredDirections = new List<(Vector2 dir, float score)>();
        float halfSpread = raySpreadAngle * 0.5f;

        for (int i = 0; i < rayCount; i++)
        {
            float angleOffset = -halfSpread + (raySpreadAngle / (rayCount - 1)) * i;
            Vector2 dir = RotateVector(CurrentDirection, angleOffset).normalized;

            Vector2 origin = (Vector2)transform.position + dir * colliderRadius;
            
            // Visualizar rayos en el editor
            Debug.DrawRay(origin, dir * rayDistance, Color.red);

            // Detectar obstáculos
            RaycastHit2D hit = Physics2D.CircleCast(origin, sensorRadius, dir, rayDistance, obstacleLayer);
            
            if (hit.collider == null)
            {
                // Calcular puntuación basada en varios factores
                float alignment = Vector2.Dot(CurrentDirection, dir); // Preferencia por seguir adelante
                float distanceFromCenter = Mathf.Abs(angleOffset) / halfSpread; // Preferencia por el centro
                
                // Factor de exploración
                float explorationBonus = 0f;
                if (isExploring)
                {
                    Vector2 targetDir = (explorationTarget - (Vector2)transform.position).normalized;
                    float targetAlignment = Vector2.Dot(dir, targetDir);
                    explorationBonus = targetAlignment * 0.5f;
                }
                
                // Añadir una pequeña variación aleatoria
                float randomVariation = Random.Range(0.9f, 1.1f);
                
                // Combinar factores en una puntuación final
                float score = (alignment * 1.2f) * (1.0f - distanceFromCenter * 0.5f) * randomVariation + explorationBonus;
                
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
            // A veces elegir una dirección subóptima pero viable para más variedad
            int index = 0;
            if (directions.Count > 1 && Random.value < 0.2f)
            {
                index = Random.Range(0, Mathf.Min(3, directions.Count));
            }
            return directions[index].dir;
        }
        else
        {
            // Emergencia: invertir dirección si no hay opciones seguras
            Vector2 escapeDir = -CurrentDirection;
            
            // Intentar encontrar una dirección de escape lateral si la inversión tampoco funciona
            RaycastHit2D forwardHit = Physics2D.CircleCast(
                (Vector2)transform.position + escapeDir * colliderRadius, 
                sensorRadius, escapeDir, rayDistance, obstacleLayer);
            
            if (forwardHit.collider != null)
            {
                // Probar dirección perpendicular o hacia el centro
                if (Random.value < 0.5f)
                    escapeDir = RotateVector(escapeDir, 90f);
                else
                    escapeDir = ((Vector2)tankCenterPos - (Vector2)transform.position).normalized;
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
            
            FishMovement otherFish = fish.GetComponent<FishMovement>();
            if (otherFish != null)
            {
                // Ahora accedemos a la propiedad pública CurrentDirection
                alignmentVector += otherFish.CurrentDirection;
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
        
        return CurrentDirection; // Si no hay otros peces, mantener dirección actual
    }

    private void UpdateSpeed(bool pathIsClear)
    {
        float targetSpeed;
        
        // Variar velocidad según comportamiento
        if (isExploring)
            targetSpeed = maxSpeed * Random.Range(0.8f, 1.0f); // Velocidad alta en exploración
        else if (!pathIsClear)
            targetSpeed = minSpeed; // Más lento cerca de obstáculos
        else
            targetSpeed = Random.Range(minSpeed * 1.2f, maxSpeed * 0.9f); // Velocidad media normal
        
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
        
        // Más ondulación durante la exploración
        if (isExploring)
            return (primaryWave + secondaryWave) * 1.2f;
        else
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
        
        // Mostrar radio de detección de paredes
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, wallDetectionDistance);
        
        // Mostrar objetivo de exploración si está en modo exploración
        if (Application.isPlaying && isExploring)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(explorationTarget, 0.1f);
            Gizmos.DrawLine(transform.position, explorationTarget);
        }
    }
}