using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class ComplexFishMovement : AquaticCreatureMovement
{
    [Header("Detección de Obstáculos")]
    [SerializeField] private float detectionDistance = 3f;
    [SerializeField] private float detectionAngle = 75f;
    [SerializeField] private int rayCount = 9;
    [SerializeField] private float avoidanceWeight = 1f;
    [SerializeField] private LayerMask complexObstacleLayer;

    // Sobrescribimos las propiedades para cambiar el comportamiento
    protected override float ObstacleDetectionDistance => detectionDistance;
    protected override float ObstacleDetectionAngle => detectionAngle;
    protected override int ObstacleRayCount => rayCount;
    protected override float ObstacleAvoidanceWeight => avoidanceWeight;
    protected override LayerMask ObstacleLayer => complexObstacleLayer;
    

    [Header("Wander State Configuration")]
    [SerializeField] private float baseFrequency = 0.3f;
    [SerializeField] private float detailFrequency = 1.2f;
    [SerializeField] private float verticalBias = 0.2f;
    [Space]
    [SerializeField] private float minDirectionChangeTime = 2f;
    [SerializeField] private float maxDirectionChangeTime = 5f;
    [Space]
    [SerializeField] private float speedVariation = 0.3f;
    [SerializeField] private float speedChangeInterval = 1.5f;
    [Space]
    [Tooltip("Controla qué tan aleatorio es el movimiento (0.2=predecible, 1.0=normal, 3.0=muy exploratorio)")]
    [Range(0.2f, 3.0f)]
    [SerializeField] private float explorationFactor = 1.0f;
    
    [Header("Comportamiento de Huida")]
    public float fleeRadius = 5f;
    public float maxFleeSpeed = 4f;
    public LayerMask predatorLayer;

    private Transform currentThreat;
    private WanderState wanderState;

    protected override void Start()
    {
        base.Start();
    }

    protected override void InitializeStates()
    {
        // Crear instancia del WanderState con el factor de exploración
        wanderState = new WanderState(
            movement: this,
            baseFreq: baseFrequency,
            detailFreq: detailFrequency,
            verticalBias: verticalBias,
            minDirChange: minDirectionChangeTime,
            maxDirChange: maxDirectionChangeTime,
            speedVar: speedVariation,
            speedChangeInterval: speedChangeInterval,
            explorationFactor: explorationFactor
        );
        
        // Definir los estados disponibles
        availableStates = new Dictionary<System.Type, FishMovementState>
        {
            { typeof(WanderState), wanderState },
            { typeof(FleeState), new FleeState(this, null, fleeRadius, maxFleeSpeed) }
        };
    }

    protected override System.Type GetInitialStateType()
    {
        return typeof(WanderState);
    }

    protected override void Update()
    {
        // Detectar posibles amenazas antes de la lógica de estados
        DetectThreats();
        
        // Verificar si el factor de exploración ha cambiado (para poder ajustarlo en tiempo real)
        if (wanderState != null && !Mathf.Approximately(wanderState.CurrentExplorationFactor, explorationFactor))
        {
            wanderState.SetExplorationFactor(explorationFactor);
        }
        
        base.Update();
    }

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
            if (closest != currentThreat)
            {
                currentThreat = closest;
                // Reemplazar estado de huida con nuevo objetivo
                availableStates[typeof(FleeState)] = new FleeState(this, currentThreat, fleeRadius, maxFleeSpeed);
                TransitionToState(typeof(FleeState));
            }
        }
        else if (currentThreat != null)
        {
            currentThreat = null;
        }
    }

    protected override void DetermineNextState()
    {
        if (currentThreat != null)
        {
            TransitionToState(typeof(FleeState));
        }
        else
        {
            TransitionToState(typeof(WanderState));
        }
    }
    
    // Método público para cambiar dinámicamente el factor de exploración
    public void SetExplorationFactor(float factor)
    {
        explorationFactor = Mathf.Clamp(factor, 0.2f, 3.0f);
        if (wanderState != null)
        {
            wanderState.SetExplorationFactor(explorationFactor);
        }
    }
}