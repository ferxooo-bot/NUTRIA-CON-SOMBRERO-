using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;  
// Modificación de la clase base para usar el componente de evasión
public abstract class AquaticCreatureMovement : MonoBehaviour
{
    [Header("Movimiento General")]
    public float maxSpeed = 2f;
    public float acceleration = 4f;
    public float deceleration = 2f;
    public float rotationSpeed = 180f;
    
    [Header("Física Acuática")]
    public float waterResistance = 0.2f;
    public float buoyancy = 0.05f;
    
    [Header("Detección de Obstáculos")]
    public bool enableObstacleAvoidance = true;
    protected virtual float ObstacleDetectionDistance => 2.5f;
    protected virtual float ObstacleDetectionAngle => 60f;
    protected virtual int ObstacleRayCount => 7;
    protected virtual float ObstacleAvoidanceWeight => 0.7f;
    protected virtual LayerMask ObstacleLayer => default;

    


    protected Vector2 currentDirection = Vector2.right;
    protected Vector2 currentVelocity = Vector2.zero;
    protected FishMovementState currentState;
    protected Dictionary<System.Type, FishMovementState> availableStates;
    
    // Sistema de detección de obstáculos compartido
    protected ObstacleAvoidance obstacleAvoidance;

    protected virtual void Start()
    {
        currentDirection = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
        
        // Inicializar el sistema de detección de obstáculos
        // Usar las propiedades que pueden ser sobreescritas por la clase hija
        obstacleAvoidance = new ObstacleAvoidance(
            this,
            ObstacleLayer,
            ObstacleDetectionDistance,
            ObstacleDetectionAngle,
            ObstacleRayCount,
            ObstacleAvoidanceWeight
        );
        InitializeStates();
        // Establecer estado inicial
        TransitionToState(GetInitialStateType());
    }

    protected virtual void InitializeStates()
    {
        // Inicializar diccionario de estados disponibles
        availableStates = new Dictionary<System.Type, FishMovementState>();
        // Las clases hijas rellenarán esto con los estados específicos
    }

    protected virtual System.Type GetInitialStateType()
    {
        // Las clases hijas definirán cuál es el estado inicial
        return null;
    }

    protected virtual void Update()
    {
        if (currentState != null)
        {
            // Chequear si debemos cambiar de estado
            if (currentState.ShouldTransition())
            {
                DetermineNextState();
            }
            
            // Actualizar movimiento según el estado actual
            Vector2 targetDirection = currentState.UpdateMove();
            
            // Aplicar evasión de obstáculos si está habilitada
            if (enableObstacleAvoidance)
            {
                targetDirection = obstacleAvoidance.ApplyAvoidance(targetDirection);
            }
            
            ApplyMovement(targetDirection);
        }
    }

    protected virtual void DetermineNextState()
    {
        // Las clases hijas determinarán la lógica de transición entre estados
    }

    public void TransitionToState(System.Type stateType)
    {
        if (availableStates.TryGetValue(stateType, out FishMovementState newState))
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }
    }

    protected virtual void ApplyMovement(Vector2 targetDirection)
    {
        if (targetDirection.sqrMagnitude > 0.01f)
        {
            // Aceleración gradual hacia la dirección objetivo
            Vector2 accelerationVector = (targetDirection.normalized * maxSpeed - currentVelocity) * acceleration * Time.deltaTime;
            currentVelocity += accelerationVector;
        }
        else
        {
            // Desaceleración cuando no hay dirección objetivo
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.deltaTime);
        }

        // Aplicar resistencia del agua
        currentVelocity -= currentVelocity * waterResistance * Time.deltaTime;
        
        // Aplicar flotabilidad (empuje suave hacia arriba)
        currentVelocity += Vector2.up * buoyancy * Time.deltaTime;

        // Limitar velocidad máxima
        if (currentVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            currentVelocity = currentVelocity.normalized * maxSpeed;
        }

        // Solo rotar si estamos moviéndonos lo suficientemente rápido
        if (currentVelocity.sqrMagnitude > 0.1f)
        {
            // Actualizar dirección actual
            currentDirection = currentVelocity.normalized;
            
            // Calcular la rotación
            float targetAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.z;
            
            // Suavizar la rotación con interpolación esférica
            float newAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angularVelocity, 0.1f, rotationSpeed);
            
            // Aplicar rotación
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
        
        // Mover el objeto
        transform.Translate(currentVelocity * Time.deltaTime, Space.World);
        
        // Animación (si hay un animator)
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            float speedRatio = currentVelocity.magnitude / maxSpeed;
            animator.SetFloat("Speed", speedRatio);
        }
    }
    
    // Variable para el SmoothDampAngle
    private float angularVelocity;

    public Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
            v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad)
        );
    }

    public Vector2 CurrentDirection
    {
        get { return currentDirection; }
        set { currentDirection = value.normalized; }
    }
    
    public Vector2 CurrentVelocity
    {
        get { return currentVelocity; }
    }
    
    // Método público para acceder al sistema de evasión
    public ObstacleAvoidance ObstacleAvoidance
    {
        get { return obstacleAvoidance; }
    }



}
