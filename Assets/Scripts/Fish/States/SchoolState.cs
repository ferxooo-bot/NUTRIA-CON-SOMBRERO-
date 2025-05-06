using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Estado que implementa el comportamiento de cardumen/banco de peces
public class SchoolState : FishMovementState
{
    [System.Serializable]
    public class SchoolSettings
    {
        public float detectionRadius = 5f;          // Radio para detectar otros peces
        public float cohesionWeight = 0.5f;         // Fuerza para ir hacia el centro del grupo
        public float alignmentWeight = 0.3f;        // Fuerza para alinearse con la dirección del grupo
        public float separationWeight = 0.8f;       // Fuerza para evitar chocar con otros peces
        public float separationRadius = 1.5f;       // Radio mínimo entre peces (para separación)
        public float randomMovementWeight = 0.1f;   // Pequeña cantidad de movimiento aleatorio
        public float stateTransitionChance = 0.01f; // Probabilidad de salir del cardumen
        public float maxTimeInSchool = 30f;         // Tiempo máximo en estado de cardumen
        public LayerMask fishLayerMask;             // Capa para detectar otros peces
    }

    private SchoolSettings settings;
    private float stateTimer;
    private System.Random random;

    // Constructor
    public SchoolState(AquaticCreatureMovement movement, SchoolSettings settings) : base(movement)
    {
        this.settings = settings;
        random = new System.Random();
    }

    public override void Enter()
    {
        stateTimer = 0f;
        // Opcional: notificar a otros peces cercanos para unirse al cardumen
    }

    public override Vector2 UpdateMove()
    {
        stateTimer += Time.deltaTime;
        
        // Encontrar peces cercanos
        Collider2D[] nearbyFish = Physics2D.OverlapCircleAll(
            movement.transform.position, 
            settings.detectionRadius, 
            settings.fishLayerMask
        );

        List<AquaticCreatureMovement> schoolMembers = new List<AquaticCreatureMovement>();
        
        foreach (var fish in nearbyFish)
        {
            if (fish.gameObject != movement.gameObject) // Evitar incluirse a sí mismo
            {
                var fishMovement = fish.GetComponent<AquaticCreatureMovement>();
                if (fishMovement != null)
                {
                    schoolMembers.Add(fishMovement);
                }
            }
        }

        // Si no hay otros peces cerca, mantener dirección actual con ligera variación
        if (schoolMembers.Count == 0)
        {
            return AddRandomMovementVariation(movement.CurrentDirection);
        }

        // Calcular los tres comportamientos básicos de cardumen
        Vector2 cohesionForce = CalculateCohesionForce(schoolMembers);
        Vector2 alignmentForce = CalculateAlignmentForce(schoolMembers);
        Vector2 separationForce = CalculateSeparationForce(schoolMembers);
        
        // Combinar las fuerzas con sus pesos
        Vector2 totalForce = cohesionForce * settings.cohesionWeight +
                             alignmentForce * settings.alignmentWeight +
                             separationForce * settings.separationWeight;
        
        // Añadir un poco de movimiento aleatorio
        totalForce += AddRandomMovementVariation(Vector2.zero);
        
        // Normalizar para obtener solo la dirección
        if (totalForce.sqrMagnitude > 0.01f)
        {
            return totalForce.normalized;
        }
        
        // Si no hay fuerza calculada, mantener dirección actual
        return movement.CurrentDirection;
    }

    // Fuerza para moverse hacia el centro del grupo
    private Vector2 CalculateCohesionForce(List<AquaticCreatureMovement> schoolMembers)
    {
        if (schoolMembers.Count == 0) return Vector2.zero;
        
        Vector2 centerPosition = Vector2.zero;
        foreach (var fish in schoolMembers)
        {
            centerPosition += (Vector2)fish.transform.position;
        }
        
        centerPosition /= schoolMembers.Count;
        
        // Vector dirección hacia el centro del cardumen
        return (centerPosition - (Vector2)movement.transform.position).normalized;
    }

    // Fuerza para alinearse con la dirección promedio del grupo
    private Vector2 CalculateAlignmentForce(List<AquaticCreatureMovement> schoolMembers)
    {
        if (schoolMembers.Count == 0) return Vector2.zero;
        
        Vector2 averageDirection = Vector2.zero;
        foreach (var fish in schoolMembers)
        {
            averageDirection += fish.CurrentDirection;
        }
        
        averageDirection /= schoolMembers.Count;
        return averageDirection.normalized;
    }

    // Fuerza para mantener distancia mínima con otros peces
    private Vector2 CalculateSeparationForce(List<AquaticCreatureMovement> schoolMembers)
    {
        if (schoolMembers.Count == 0) return Vector2.zero;
        
        Vector2 separationDirection = Vector2.zero;
        int tooCloseCount = 0;
        
        foreach (var fish in schoolMembers)
        {
            float distance = Vector2.Distance(fish.transform.position, movement.transform.position);
            
            if (distance < settings.separationRadius)
            {
                // Vector que aleja de este pez, ponderado por la cercanía
                Vector2 moveAway = (Vector2)(movement.transform.position - fish.transform.position);
                float weight = 1.0f - (distance / settings.separationRadius); // Más peso cuanto más cerca
                separationDirection += moveAway.normalized * weight;
                tooCloseCount++;
            }
        }
        
        if (tooCloseCount > 0)
        {
            separationDirection /= tooCloseCount;
            return separationDirection.normalized;
        }
        
        return Vector2.zero;
    }

    // Añade pequeña variación aleatoria al movimiento para más naturalidad
    private Vector2 AddRandomMovementVariation(Vector2 baseDirection)
    {
        float randomAngle = Random.Range(-20f, 20f);
        Vector2 randomForce = movement.RotateVector(
            baseDirection.sqrMagnitude > 0.01f ? baseDirection : Vector2.up, 
            randomAngle
        );
        
        return randomForce * settings.randomMovementWeight;
    }

    public override bool ShouldTransition()
    {
        // Probabilidad aleatoria de salir del cardumen
        if (Random.value < settings.stateTransitionChance * Time.deltaTime)
        {
            return true;
        }
        
        // Limitar el tiempo máximo en cardumen
        if (stateTimer > settings.maxTimeInSchool)
        {
            return true;
        }
        
        // También podría salir si no hay peces cercanos durante cierto tiempo
        return false;
    }

    public override void Exit()
    {
        // Realizar acciones necesarias al salir del estado
        stateTimer = 0f;
    }
}