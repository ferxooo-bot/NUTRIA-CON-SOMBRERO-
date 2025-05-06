using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ObstacleAvoidance
{
    private AquaticCreatureMovement movement;
    
    // Parámetros configurables
    public float detectionDistance = 1f;
    public float detectionAngle = 60f;
    public int rayCount = 7;
    public LayerMask obstacleLayer;
    public float avoidanceWeight = 0.7f;
    
    // Nuevos parámetros para comportamiento más natural
    private float creatureSeed;  // Semilla única para cada criatura
    private float lastObstacleTime = -10f;  // Último momento de detección de obstáculo
    private Vector2 lastAvoidanceDirection = Vector2.zero;  // Para mantener consistencia
    private int evasionStrategy = 0;  // Estrategia actual de evasión (0=opuesta, 1=bordear)
    private bool preferClockwise;  // Preferencia para bordear obstáculos
    
    // Constructor
    public ObstacleAvoidance(AquaticCreatureMovement movement, LayerMask obstacleLayer, float detectionDistance, float detectionAngle, int rayCount, float avoidanceWeight)
    {
        this.movement = movement;
        this.obstacleLayer = obstacleLayer;
        this.detectionDistance = detectionDistance;  // Corregido
        this.detectionAngle = detectionAngle; 
        this.rayCount = rayCount; 
        this.avoidanceWeight = avoidanceWeight;
        
        // Inicialización de parámetros de comportamiento natural
        this.creatureSeed = UnityEngine.Random.Range(0f, 1000f);
        this.preferClockwise = UnityEngine.Random.value > 0.5f;  // 50% de probabilidad
        SelectEvasionStrategy();
    }
    
    // Selecciona una estrategia de evasión
    private void SelectEvasionStrategy()
    {
        // 65% probabilidad de estrategia opuesta, 35% de bordear
        evasionStrategy = UnityEngine.Random.value < 0.65f ? 0 : 1;
    }
    
    // Detecta si hay obstáculos adelante
    public bool IsObstacleAhead()
    {
        bool obstacleDetected = false;
        
        for (int i = 0; i < rayCount; i++)
        {
            // Distribuir los rayos en un arco frente a la criatura
            float angle = -detectionAngle + (2 * detectionAngle / (rayCount - 1)) * i;
            Vector2 rayDirection = RotateVector(movement.CurrentDirection, angle);
            
            // Dibujar el rayo para depuración
            Debug.DrawRay(movement.transform.position, rayDirection * detectionDistance, Color.yellow);
            
            RaycastHit2D hit = Physics2D.Raycast(movement.transform.position, rayDirection, detectionDistance, obstacleLayer);
            if (hit.collider != null)
            {
                obstacleDetected = true;
                break;
            }
        }
        
        // Si acabamos de detectar un obstáculo después de un tiempo sin obstáculos
        if (obstacleDetected && Time.time > lastObstacleTime + 2.0f)
        {
            // Posibilidad de cambiar de estrategia
            if (UnityEngine.Random.value < 0.3f)
            {
                SelectEvasionStrategy();
            }
            lastObstacleTime = Time.time;
        }
        
        return obstacleDetected;
    }
    
    // Calcula la mejor dirección para evitar obstáculos
    public Vector2 GetAvoidanceDirection()
    {
        Vector2 obstaclePosition = Vector2.zero;
        Vector2 avoidanceDirection = Vector2.zero;
        float closestDistance = float.MaxValue;
        int hitCount = 0;
        
        // Recopilamos información sobre los obstáculos
        for (int i = 0; i < rayCount; i++)
        {
            float angle = -detectionAngle + (2 * detectionAngle / (rayCount - 1)) * i;
            Vector2 rayDirection = RotateVector(movement.CurrentDirection, angle);
            
            RaycastHit2D hit = Physics2D.Raycast(movement.transform.position, rayDirection, detectionDistance, obstacleLayer);
            if (hit.collider != null)
            {
                // Registrar posición del obstáculo más cercano
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    obstaclePosition = hit.point;
                }
                
                // La fuerza de evasión es inversamente proporcional a la distancia
                float weight = 1.0f - (hit.distance / detectionDistance);
                
                // Dirección opuesta al punto de impacto
                avoidanceDirection -= (hit.point - (Vector2)movement.transform.position).normalized * weight;
                hitCount++;
                
                // Dibujar el rayo de colisión para depuración
                Debug.DrawRay(movement.transform.position, rayDirection * hit.distance, Color.red);
            }
        }
        
        // Si no detectamos colisiones, retornar la dirección actual
        if (hitCount == 0) return movement.CurrentDirection;
        
        // Normalizar la dirección de evasión "opuesta"
        Vector2 oppositeDirection = avoidanceDirection.normalized;
        
        // Si la estrategia es bordear el obstáculo
        if (evasionStrategy == 1)
        {
            // Calcular vector desde la posición actual hasta el obstáculo
            Vector2 toObstacle = obstaclePosition - (Vector2)movement.transform.position;
            
            // Calcular un vector perpendicular al vector hacia el obstáculo
            Vector2 tangentDirection;
            if (preferClockwise)
            {
                tangentDirection = new Vector2(-toObstacle.y, toObstacle.x).normalized;
            }
            else
            {
                tangentDirection = new Vector2(toObstacle.y, -toObstacle.x).normalized;
            }
            
            // Mezclar con un componente de alejamiento para evitar colisión
            float distanceFactor = Mathf.Clamp01(closestDistance / detectionDistance);
            Vector2 borderingDirection = Vector2.Lerp(-toObstacle.normalized, tangentDirection, distanceFactor * 0.7f);
            
            // Si ya teníamos una dirección de evasión, hacer transición suave
            if (lastAvoidanceDirection.sqrMagnitude > 0.1f)
            {
                borderingDirection = Vector2.Lerp(lastAvoidanceDirection, borderingDirection, 0.3f).normalized;
            }
            
            lastAvoidanceDirection = borderingDirection;
            return borderingDirection.normalized;
        }
        
        // Estrategia de dirección opuesta (default)
        // Si ya teníamos una dirección de evasión, hacer transición suave
        if (lastAvoidanceDirection.sqrMagnitude > 0.1f)
        {
            oppositeDirection = Vector2.Lerp(lastAvoidanceDirection, oppositeDirection, 0.3f).normalized;
        }
        
        lastAvoidanceDirection = oppositeDirection;
        return oppositeDirection;
    }
    
    // Aplica la evasión a una dirección deseada
    public Vector2 ApplyAvoidance(Vector2 desiredDirection)
    {
        if (!IsObstacleAhead())
        {
            // Reiniciar última dirección de evasión si llevamos tiempo sin obstáculos
            if (Time.time > lastObstacleTime + 1.0f)
            {
                lastAvoidanceDirection = Vector2.zero;
            }
            return desiredDirection; // No hay obstáculos, continuar con la dirección deseada
        }
        
        Vector2 avoidanceDirection = GetAvoidanceDirection();
        
        // Varía el peso de evasión según cuán cerca esté del obstáculo
        float dynamicWeight = avoidanceWeight;
        
        // Si el pez está siguiendo una trayectoria de evasión coherente, reduce gradualmente el peso
        if (Vector2.Dot(desiredDirection, avoidanceDirection) > 0.7f)
        {
            dynamicWeight *= 0.7f;
        }
        
        // Mezclar la dirección deseada con la dirección de evasión
        Vector2 resultDirection = Vector2.Lerp(desiredDirection, avoidanceDirection, dynamicWeight);
        
        // Añadir pequeña variación basada en la semilla de la criatura
        float noiseValue = Mathf.PerlinNoise(Time.time * 0.5f, creatureSeed);
        float variationAngle = (noiseValue - 0.5f) * 15f;  // ±7.5 grados de variación
        
        return RotateVector(resultDirection.normalized, variationAngle);
    }
    
    // Método auxiliar para rotar vectores
    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
            v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad)
        );
    }
}