using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class SchoolingFish : ComplexFishMovement
{
    [Header("Configuración de Cardumen")]
    [SerializeField] private SchoolState.SchoolSettings schoolSettings;
    
    [Space]
    [Tooltip("Probabilidad por segundo de intentar unirse a un cardumen cuando está vagando")]
    [Range(0f, 1f)]
    [SerializeField] private float joinSchoolChance = 0.05f;
    
    [Tooltip("Distancia máxima a la que puede detectar un cardumen para unirse")]
    [SerializeField] private float schoolDetectionRadius = 8f;
    
    [Tooltip("Tiempo mínimo que debe pasar después de huir antes de poder unirse a un cardumen")]
    [SerializeField] private float timeBeforeRegrouping = 5f;
    
    // Variables de control interno
    private float timeSinceLastFlee = 0f;
    private float schoolCheckTimer = 0f;
    
    // Referencia de estado
    private SchoolState schoolState;
    
    // Referencia al depredador actual (para reemplazar el acceso a currentThreat de la clase padre)
    private Transform currentPredator;

    protected override void Start()
    {
        // Asegurarnos que la capa de peces esté configurada si no se hizo en el editor
        if (schoolSettings.fishLayerMask == 0)
        {
            schoolSettings.fishLayerMask = LayerMask.GetMask("Fish");
        }
        
        // Iniciar el temporizador
        timeSinceLastFlee = timeBeforeRegrouping + 1f; // Permitir unirse a cardumen al inicio
        
        base.Start();
    }

    protected override void InitializeStates()
    {
        // Llamar a la inicialización de ComplexFishMovement primero
        base.InitializeStates();
        
        // Crear el estado de cardumen
        schoolState = new SchoolState(this, schoolSettings);
        
        // Añadir el estado de cardumen al diccionario
        availableStates[typeof(SchoolState)] = schoolState;
    }

    protected override void Update()
    {
        // Actualizar temporizadores
        timeSinceLastFlee += Time.deltaTime;
        schoolCheckTimer += Time.deltaTime;
        
        // Detectar cardumenes cercanos si estamos en estado wandering
        if (currentState is WanderState && 
            timeSinceLastFlee > timeBeforeRegrouping && 
            schoolCheckTimer > 1f) // Comprobar cada segundo
        {
            schoolCheckTimer = 0f;
            TryDetectAndJoinSchool();
        }
        
        // Continuar con la actualización normal
        base.Update();
    }

    private void TryDetectAndJoinSchool()
    {
        // Verificar probabilidad aleatoria para ver si intentamos unirse a un cardumen
        if (Random.value > joinSchoolChance)
            return;
            
        // Buscar otros peces cercanos que puedan estar en un cardumen
        Collider2D[] nearbyFish = Physics2D.OverlapCircleAll(
            transform.position, 
            schoolDetectionRadius, 
            schoolSettings.fishLayerMask
        );
        
        int schoolingFishCount = 0;
        
        // Contar cuántos peces están en modo cardumen
        foreach (var fish in nearbyFish)
        {
            if (fish.gameObject != gameObject) // No contarse a sí mismo
            {
                SchoolingFish otherFish = fish.GetComponent<SchoolingFish>();
                if (otherFish != null && otherFish.IsInSchoolState())
                {
                    schoolingFishCount++;
                }
            }
        }
        
        // Si hay suficientes peces en cardumen cerca, unirse
        if (schoolingFishCount >= 2)
        {
            TransitionToState(typeof(SchoolState));
        }
    }

    // Sobrescribir el método de detección de amenazas para mantener nuestra propia referencia
    private void DetectThreats()
    {
        Collider2D[] preds = Physics2D.OverlapCircleAll(transform.position, fleeRadius, predatorLayer);
        if (preds.Length > 0)
        {
            // Obtener el más cercano
            float minDist = float.MaxValue;
            Transform closest = null;
            foreach (var p in preds)
            {
                float d = Vector2.Distance(transform.position, p.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    closest = p.transform;
                }
            }
            if (closest != currentPredator)
            {
                currentPredator = closest;
                // Reemplazar estado de huida con nuevo objetivo
                availableStates[typeof(FleeState)] = new FleeState(this, currentPredator, fleeRadius, maxFleeSpeed);
                TransitionToState(typeof(FleeState));
            }
        }
        else if (currentPredator != null)
        {
            currentPredator = null;
        }
    }

    protected override void DetermineNextState()
    {
        if (currentPredator != null)
        {
            TransitionToState(typeof(FleeState));
        }
        else if (currentState is SchoolState && currentState.ShouldTransition())
        {
            TransitionToState(typeof(WanderState));
        }
        else if (currentState is FleeState && currentState.ShouldTransition())
        {
            TransitionToState(typeof(WanderState));
        }
    }
    
    // Método para verificar externamente si el pez está en estado de cardumen
    public bool IsInSchoolState()
    {
        return currentState is SchoolState;
    }
    
    // Método público para obtener el estado de cardumen si otro pez necesita consultarlo
    public SchoolState GetSchoolState()
    {
        return schoolState;
    }
    
    // Método para modificar dinámicamente parámetros del cardumen
    public void SetSchoolCohesionWeight(float weight)
    {
        schoolSettings.cohesionWeight = weight;
    }
    
    public void SetSchoolAlignmentWeight(float weight)
    {
        schoolSettings.alignmentWeight = weight;
    }
    
    public void SetSchoolSeparationWeight(float weight)
    {
        schoolSettings.separationWeight = weight;
    }
    
    // Este método se puede llamar desde eventos de Unity (p.ej. OnTriggerEnter2D)
    public void AlertNearbyFish(Vector2 dangerPosition, float alertRadius = 5f)
    {
        // Encontrar peces cercanos
        Collider2D[] nearbyFish = Physics2D.OverlapCircleAll(
            transform.position, 
            alertRadius, 
            schoolSettings.fishLayerMask
        );
        
        // Alertar a todos los peces cercanos sobre el peligro
        foreach (var fish in nearbyFish)
        {
            if (fish.gameObject != gameObject) // No alertarse a sí mismo
            {
                SchoolingFish otherFish = fish.GetComponent<SchoolingFish>();
                if (otherFish != null)
                {
                    // Crear un nuevo estado de huida para el pez
                    Transform dangerTransform = null;
                    
                    // Si hay un GameObject en la posición de peligro, usarlo
                    Collider2D[] dangerObjects = Physics2D.OverlapCircleAll(dangerPosition, 0.1f);
                    if (dangerObjects.Length > 0)
                    {
                        dangerTransform = dangerObjects[0].transform;
                    }
                    
                    // Alertar al otro pez con un nuevo estado de huida
                    otherFish.availableStates[typeof(FleeState)] = new FleeState(otherFish, dangerTransform, otherFish.fleeRadius, otherFish.maxFleeSpeed);
                    otherFish.TransitionToState(typeof(FleeState));
                    
                    // Nota: Aquí necesitarás implementar SetDangerPosition en tu FleeState
                    // Comentamos esta línea hasta que implementes el método
                    // ((FleeState)otherFish.availableStates[typeof(FleeState)]).SetDangerPosition(dangerPosition);
                }
            }
        }
    }
    
    // Visualización para debugging
    private void OnDrawGizmosSelected()
    {
        // Mostrar radio de detección de cardumen
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, schoolDetectionRadius);
        
        // Mostrar radio de detección de amenazas (heredado de ComplexFishMovement)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRadius);
        
        // Mostrar radio de interacción de cardumen
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, schoolSettings.detectionRadius);
        
        // Mostrar radio de separación
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Amarillo semi-transparente
        Gizmos.DrawSphere(transform.position, schoolSettings.separationRadius);
    }
}