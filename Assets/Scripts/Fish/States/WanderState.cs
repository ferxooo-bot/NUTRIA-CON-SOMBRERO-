using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WanderState : FishMovementState
{
    // Parámetros de comportamiento
    private float baseFrequency;
    private float detailFrequency;
    private float verticalBias;
    private float minDirectionChangeTime;
    private float maxDirectionChangeTime;
    private float speedVariation;
    private float speedChangeInterval;
    private float explorationFactor;  // Nuevo parámetro para controlar la aleatoriedad/exploración
    
    // Variables de estado internas
    private float baseSeed;
    private float detailSeed;
    private float timeOffset;  // Offset temporal para desincronizar peces
    private float targetSpeedMultiplier = 1f;
    private float currentSpeedMultiplier = 1f;
    private float lastSpeedChange;
    private float changeDirectionTime;
    private float lastDirectionChange;
    private Vector2 targetDirection;
    private float directionSmoothingFactor;
    private float personalityFactor;  // Factor de "personalidad" para diferenciación
    private float baseMinDirectionChangeTime;
    private float baseMaxDirectionChangeTime;
    
    // Constructor con variación de parámetros y factor de exploración
    public WanderState(
        AquaticCreatureMovement movement,
        float baseFreq = 0.3f,
        float detailFreq = 0.8f,
        float verticalBias = 0.4f,
        float minDirChange = 3.0f,
        float maxDirChange = 8.0f,
        float speedVar = 0.3f,
        float speedChangeInterval = 2.0f,
        float paramVariation = 0.4f,  // Introduce variación en los parámetros
        float explorationFactor = 1.0f  // Nuevo: factor de exploración (1.0 = normal, >1.0 = más exploratorio, <1.0 = menos exploratorio)
        
    ) : base(movement)
    {
        this.explorationFactor = Mathf.Clamp(explorationFactor, 0.2f, 3.0f);  // Limitar el rango para evitar comportamientos extremos
        
        // Aplicar variación a los parámetros para cada pez, influenciada por el factor de exploración
        this.baseFrequency = baseFreq * Random.Range(1f - paramVariation, 1f + paramVariation);
        this.detailFrequency = detailFreq * Random.Range(1f - paramVariation, 1f + paramVariation) * Mathf.Lerp(1f, 1.5f, (explorationFactor - 1f) / 2f);
        this.verticalBias = verticalBias * Random.Range(1f - paramVariation, 1f + paramVariation);
        
        // El factor de exploración afecta directamente los tiempos de cambio de dirección
        float dirChangeTimeFactor = Mathf.Lerp(1.5f, 0.6f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        this.minDirectionChangeTime = minDirChange * Random.Range(1f - paramVariation/2, 1f + paramVariation) * dirChangeTimeFactor;
        this.maxDirectionChangeTime = maxDirChange * Random.Range(1f - paramVariation/2, 1f + paramVariation) * dirChangeTimeFactor;
        
        // Mayor exploración = mayor variación de velocidad
        this.speedVariation = speedVar * Random.Range(0.8f, 1.2f) * Mathf.Lerp(0.7f, 1.5f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        this.speedChangeInterval = speedChangeInterval * Random.Range(0.7f, 1.3f) * dirChangeTimeFactor;
        
        // Semillas y tiempos realmente aleatorios
        baseSeed = Random.Range(0f, 10000f);
        detailSeed = Random.Range(0f, 10000f);
        timeOffset = Random.Range(0f, 1000f);  // Desplazamiento temporal
        
        // Personalidad única para cada pez, afectada por el factor de exploración
        personalityFactor = Random.Range(0.7f, 1.3f) * Mathf.Lerp(0.8f, 1.2f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        
        // Suavizado de dirección: menor valor = cambios más bruscos (alta exploración)
        directionSmoothingFactor = 2.5f * Random.Range(0.8f, 1.2f) * Mathf.Lerp(1.3f, 0.7f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        
        // Inicializar dirección objetivo con una dirección aleatoria
        float randomAngle = Random.Range(0f, 360f);
        targetDirection = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );


        this.baseMinDirectionChangeTime = minDirChange;
        this.baseMaxDirectionChangeTime = maxDirChange; 
    }
    
    public override void Enter()
    {
        // Iniciar en diferentes momentos del ciclo
        lastDirectionChange = Time.time - Random.Range(0f, minDirectionChangeTime);
        lastSpeedChange = Time.time - Random.Range(0f, speedChangeInterval);
        currentSpeedMultiplier = Random.Range(0.8f, 1.2f);
        targetSpeedMultiplier = Random.Range(0.8f, 1.2f);
        CalculateNewTargets();
    }
    
    private void CalculateNewTargets()
    {
        // Variación más pronunciada entre peces
        changeDirectionTime = Random.Range(minDirectionChangeTime, maxDirectionChangeTime);
        
        // Usar tiempo personalizado para cada pez
        float fishTime = Time.time + timeOffset;
        
        // El factor de exploración influye en la amplitud de la variación de velocidad
        float speedVariationAmp = speedVariation * Mathf.Lerp(0.8f, 1.5f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        float rawSpeedMultiplier = 1f + (Mathf.PerlinNoise(fishTime * 0.2f, baseSeed + 50f) * 2f - 1f) * speedVariationAmp;
        
        // Aplica factor de personalidad
        rawSpeedMultiplier *= personalityFactor;
        
        // Mayor exploración = cambios de velocidad más bruscos
        float speedTransitionFactor = Mathf.Lerp(0.3f, 0.6f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        targetSpeedMultiplier = Mathf.Lerp(targetSpeedMultiplier, rawSpeedMultiplier, speedTransitionFactor);
    }
    
    public override Vector2 UpdateMove()
    {
        // Tiempo personalizado para cada pez
        float fishTime = Time.time + timeOffset;
        
        // Comprobar si es momento de calcular una nueva dirección
        if (fishTime > lastDirectionChange + changeDirectionTime)
        {
            CalculateNewDirection(fishTime);
            lastDirectionChange = fishTime;
        }
        
        // Comprobar si es momento de cambiar la velocidad
        if (fishTime > lastSpeedChange + speedChangeInterval)
        {
            CalculateNewTargets();
            lastSpeedChange = fishTime;
        }
        
        // Alta exploración = menos suavizado, cambios más repentinos
        float exploreSmoothModifier = Mathf.Lerp(1.2f, 0.7f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        float smoothFactor = directionSmoothingFactor * exploreSmoothModifier * Mathf.Lerp(0.8f, 1.2f, 
            Mathf.PerlinNoise(fishTime * 0.05f, baseSeed + 100f));
            
        movement.CurrentDirection = Vector2.Lerp(
            movement.CurrentDirection,
            targetDirection,
            Time.deltaTime * smoothFactor
        ).normalized;
        
        // Suavizar cambios de velocidad con variación
        float speedSmoothFactor = 0.8f * personalityFactor * exploreSmoothModifier;
        currentSpeedMultiplier = Mathf.Lerp(
            currentSpeedMultiplier, 
            targetSpeedMultiplier, 
            Time.deltaTime * speedSmoothFactor
        );
        
        // Añadir pequeño efecto pulsante único para cada pez, amplificado con exploración
        float pulseAmplitude = 0.05f * personalityFactor * Mathf.Lerp(0.8f, 1.5f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        float pulseEffect = 1f + Mathf.Sin(fishTime * 0.5f + baseSeed) * pulseAmplitude;
        
        return movement.CurrentDirection * currentSpeedMultiplier * pulseEffect;
    }
    
    private void CalculateNewDirection(float fishTime)
    {
        // El factor de exploración influye en la frecuencia del ruido
        // Más exploración = más detalle en el ruido = movimientos más erráticos
        float exploreDetailMod = Mathf.Lerp(0.8f, 1.6f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        float exploreBaseMod = Mathf.Lerp(0.9f, 1.3f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        
        // Usando tiempo personalizado para ruido Perlin
        float baseNoiseX = Mathf.PerlinNoise(fishTime * baseFrequency * exploreBaseMod, baseSeed);
        float baseNoiseY = Mathf.PerlinNoise(baseSeed, fishTime * baseFrequency * exploreBaseMod);
        float detailNoiseX = Mathf.PerlinNoise(fishTime * detailFrequency * exploreDetailMod, detailSeed);
        float detailNoiseY = Mathf.PerlinNoise(detailSeed, fishTime * detailFrequency * exploreDetailMod);
        
        // Mayor exploración = más influencia del ruido de detalle
        float detailWeight = 0.3f * Mathf.Lerp(0.7f, 1.8f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        
        // Crear vector de dirección base con componentes de ruido
        // Personalizar las proporciones del ruido según el factor de personalidad
        Vector2 newDirection = new Vector2(
            (baseNoiseX - 0.5f) * 2f * personalityFactor + (detailNoiseX - 0.5f) * detailWeight,
            (baseNoiseY - 0.5f) * 2f * personalityFactor + (detailNoiseY - 0.5f) * detailWeight + 
                verticalBias * Mathf.Sin(fishTime * 0.4f * personalityFactor)
        );
        
        // Evitar cambios de dirección demasiado bruscos, pero permitir giros más amplios con más exploración
        float maxTurnAngleFactor = Mathf.Lerp(0.7f, 1.6f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        float maxTurnAngle = 60f * personalityFactor * maxTurnAngleFactor;
        float currentAngle = Mathf.Atan2(movement.CurrentDirection.y, movement.CurrentDirection.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
        
        // Calcular diferencia angular y limitar si es necesario
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
        if (Mathf.Abs(angleDifference) > maxTurnAngle)
        {
            targetAngle = currentAngle + Mathf.Sign(angleDifference) * maxTurnAngle;
            newDirection = new Vector2(
                Mathf.Cos(targetAngle * Mathf.Deg2Rad),
                Mathf.Sin(targetAngle * Mathf.Deg2Rad)
            );
        }
        
        // Normalizar y asignar como dirección objetivo
        targetDirection = newDirection.normalized;
    }
    
    public override bool ShouldTransition()
    {
        // Ajustar probabilidad de transición basada en exploración
        // Mayor exploración = mayor probabilidad de cambiar de estado
        float exploreFactor = Mathf.Lerp(0.8f, 1.5f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        float transitionChance = 0.04f * Time.deltaTime * personalityFactor * exploreFactor;
        
        // Aumentar probabilidad si llevamos mucho tiempo en este estado
        if (Time.time - lastDirectionChange > maxDirectionChangeTime * 2)
        {
            transitionChance *= 2.5f;
        }
        
        return Random.value < transitionChance;
    }
    
    public override void Exit()
    {
        // Nada que limpiar
    }
    
    // Método para ajustar el factor de exploración en tiempo de ejecución
    public void SetExplorationFactor(float newFactor)
    {
        explorationFactor = Mathf.Clamp(newFactor, 0.2f, 3.0f);
        
        // Recalcular algunos parámetros para que el cambio tenga efecto inmediato
        float dirChangeTimeFactor = Mathf.Lerp(1.5f, 0.6f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        
        minDirectionChangeTime = baseMinDirectionChangeTime * dirChangeTimeFactor;
        maxDirectionChangeTime = baseMaxDirectionChangeTime * dirChangeTimeFactor;
        
        directionSmoothingFactor = directionSmoothingFactor * Mathf.Lerp(1.3f, 0.7f, Mathf.InverseLerp(0.2f, 3.0f, explorationFactor));
        
        // Forzar un recálculo para aplicar los cambios
        CalculateNewTargets();
        CalculateNewDirection(Time.time + timeOffset);
    }
    public float CurrentExplorationFactor 
    {
        get { return explorationFactor; }
    }
}