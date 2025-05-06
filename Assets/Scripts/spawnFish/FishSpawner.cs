using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [System.Serializable]
    public class FishType
    {
        public string name;
        public GameObject prefab; // Arrastra aquí el prefab del pez en el inspector
        [Range(0f, 1f)]
        public float spawnProbability = 0.1f; // Probabilidad de aparición (de 0 a 1)
        public int maxCount = 10; // Máximo número de este tipo de pez
        [HideInInspector]
        public int currentCount = 0; // Contador actual (se actualiza automáticamente)
    }

    [Header("Peces")]
    public List<FishType> fishTypes = new List<FishType>();

    [Header("Áreas de Spawn")]
    public List<Collider2D> spawnAreas; // Áreas donde pueden aparecer peces
    public List<Collider2D> countAreas; // Áreas para contar peces (normalmente las mismas)
    
    [Header("Configuración")]
    public int maxFishTotal = 50; // Límite total de peces en la escena
    public float spawnInterval = 1.5f; // Tiempo entre intentos de spawn
    public string fishTag = "Fish"; // Tag que usarán los peces

    private int currentFishTotal = 0;
    private bool isSpawning = false;

    private void Start()
    {
        // Prepara los 10 tipos de peces si no los has definido manualmente
        if (fishTypes.Count == 0)
        {
            SetupDefaultFishTypes();
        }

        // Actualiza conteo inicial
        UpdateFishCount();

        // Inicia la rutina de generación
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Espera el intervalo
            yield return new WaitForSeconds(spawnInterval);

            // Actualiza conteo de peces en todas las áreas
            UpdateFishCount();

            // Si no superamos el máximo total y no estamos ya generando, intenta generar un pez
            if (currentFishTotal < maxFishTotal && !isSpawning)
            {
                // Verifica si hay espacio disponible para algún tipo de pez
                bool anySpaceAvailable = false;
                foreach (var fishType in fishTypes)
                {
                    if (fishType.currentCount < fishType.maxCount)
                    {
                        anySpaceAvailable = true;
                        break;
                    }
                }

                if (anySpaceAvailable)
                {
                    StartCoroutine(SpawnFish());
                }
            }
            else
            {
                // Debug.Log($"No generando: Total={currentFishTotal}/{maxFishTotal}, isSpawning={isSpawning}");
            }
        }
    }

    private void UpdateFishCount()
    {
        // Reinicia contadores completamente
        currentFishTotal = 0;
        foreach (var fishType in fishTypes)
        {
            fishType.currentCount = 0;
        }

        // Encuentra todos los peces activos con el tag
        GameObject[] allFish = GameObject.FindGameObjectsWithTag(fishTag);

        foreach (GameObject fish in allFish)
        {
            // Comprueba si el pez está en alguna de las áreas de conteo
            bool isInCountArea = false;
            
            // Si no hay áreas de conteo, contamos todos los peces
            if (countAreas.Count == 0)
            {
                isInCountArea = true;
            }
            else
            {
                foreach (Collider2D area in countAreas)
                {
                    if (area != null && fish.GetComponent<Collider2D>() != null)
                    {
                        // Si el pez tiene un collider y está dentro de un área de conteo
                        if (fish.GetComponent<Collider2D>().bounds.Intersects(area.bounds))
                        {
                            isInCountArea = true;
                            break;
                        }
                    }
                }
            }

            // Si está en un área de conteo (o no hay áreas definidas), actualiza los contadores
            if (isInCountArea)
            {
                currentFishTotal++;
                
                // Busca a qué tipo pertenece
                FishIdentifier identifier = fish.GetComponent<FishIdentifier>();
                if (identifier != null && identifier.typeIndex >= 0 && identifier.typeIndex < fishTypes.Count)
                {
                    fishTypes[identifier.typeIndex].currentCount++;
                }
            }
        }
        
        // Activar para depuración
        // Debug.Log($"Total de peces: {currentFishTotal}/{maxFishTotal}");
    }

    private IEnumerator SpawnFish()
    {
        // Marca que estamos generando
        isSpawning = true;

        // Actualiza el conteo primero para tener datos precisos
        UpdateFishCount();
        
        // Verificación final antes de generar
        if (currentFishTotal >= maxFishTotal)
        {
            isSpawning = false;
            yield break;
        }

        // Selecciona un tipo de pez basado en probabilidades
        int fishTypeIndex = SelectFishType();
        
        if (fishTypeIndex >= 0 && spawnAreas.Count > 0)
        {
            FishType selectedType = fishTypes[fishTypeIndex];
            
            // Verificación adicional para asegurar que estamos dentro del límite
            if (selectedType.currentCount < selectedType.maxCount && currentFishTotal < maxFishTotal)
            {
                // Elige un área de spawn aleatoria
                Collider2D spawnArea = spawnAreas[Random.Range(0, spawnAreas.Count)];
                
                if (spawnArea != null)
                {
                    // Genera una posición aleatoria dentro del área
                    Vector2 spawnPos = GetRandomPositionInArea(spawnArea);
                    
                    // Crea el pez
                    GameObject newFish = Instantiate(selectedType.prefab, spawnPos, Quaternion.identity);
                    
                    // Asigna el tag para identificarlo
                    newFish.tag = fishTag;
                    
                    // Añade un identificador para saber qué tipo es
                    FishIdentifier identifier = newFish.GetComponent<FishIdentifier>();
                    if (identifier == null)
                    {
                        identifier = newFish.AddComponent<FishIdentifier>();
                    }
                    identifier.typeIndex = fishTypeIndex;
                    
                    // Incrementamos manualmente los contadores para mayor precisión
                    selectedType.currentCount++;
                    currentFishTotal++;
                    
                    // Debug.Log($"Pez generado - Tipo: {selectedType.name}, Total: {currentFishTotal}/{maxFishTotal}");
                }
            }
        }
        
        // Espera un poco para evitar spawns simultáneos
        yield return new WaitForSeconds(0.1f);
        isSpawning = false;
    }

    private int SelectFishType()
    {
        // Calcula la probabilidad total disponible
        float totalProbability = 0f;
        List<int> availableTypes = new List<int>();
        
        for (int i = 0; i < fishTypes.Count; i++)
        {
            // Solo considera tipos que no han alcanzado su máximo
            if (fishTypes[i].currentCount < fishTypes[i].maxCount)
            {
                totalProbability += fishTypes[i].spawnProbability;
                availableTypes.Add(i);
            }
        }
        
        // Si no hay tipos disponibles, retorna -1
        if (availableTypes.Count == 0 || totalProbability <= 0f)
        {
            return -1;
        }
        
        // Selecciona un tipo basado en su probabilidad
        float randomValue = Random.Range(0f, totalProbability);
        float accumulator = 0f;
        
        foreach (int typeIndex in availableTypes)
        {
            accumulator += fishTypes[typeIndex].spawnProbability;
            if (randomValue <= accumulator)
            {
                return typeIndex;
            }
        }
        
        // Fallback
        return availableTypes[0];
    }

    private Vector2 GetRandomPositionInArea(Collider2D area)
    {
        // Obtiene límites del área
        Bounds bounds = area.bounds;
        
        // Genera posición aleatoria dentro del área
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        
        return new Vector2(x, y);
    }

    // Configura 10 tipos de peces predeterminados (sólo nombres, debes asignar prefabs)
    private void SetupDefaultFishTypes()
    {
        string[] names = {
            "Pez Dorado", "Pez Azul", "Tiburón", "Pez Globo", "Estrella de Mar", 
            "Ballena", "Delfín", "Pez Payaso", "Medusa", "Tortuga"
        };
        
        float[] probabilities = {
            0.20f, 0.18f, 0.05f, 0.12f, 0.15f,
            0.03f, 0.07f, 0.10f, 0.06f, 0.04f
        };
        
        // Ajustar estos valores para asegurar que la suma total sea <= maxFishTotal
        int[] maxCounts = {
            12, 10, 2, 6, 7,
            1, 3, 5, 3, 1
        };
        
        for (int i = 0; i < 10; i++)
        {
            FishType fishType = new FishType
            {
                name = names[i],
                spawnProbability = probabilities[i],
                maxCount = maxCounts[i]
            };
            
            fishTypes.Add(fishType);
        }
        
        // Verifica que la suma de maxCounts no exceda maxFishTotal
        int totalMaxCount = 0;
        foreach (var fishType in fishTypes)
        {
            totalMaxCount += fishType.maxCount;
        }
        
        Debug.Log($"Configuración de peces: Total máximo permitido: {maxFishTotal}, Suma de límites individuales: {totalMaxCount}");
        
        if (totalMaxCount < maxFishTotal)
        {
            Debug.LogWarning($"La suma de maxCount ({totalMaxCount}) es menor que maxFishTotal ({maxFishTotal}). Esto podría resultar en menos peces de los esperados.");
        }
        else if (totalMaxCount > maxFishTotal)
        {
            Debug.LogWarning($"La suma de maxCount ({totalMaxCount}) es mayor que maxFishTotal ({maxFishTotal}). Algunos tipos de peces no alcanzarán su límite máximo individual.");
            
            // Ajusta automáticamente los límites individuales para que no excedan el total
            AdjustFishLimits();
        }
    }
    
    // Método para ajustar automáticamente los límites de peces
    private void AdjustFishLimits()
    {
        int totalMaxCount = 0;
        foreach (var fishType in fishTypes)
        {
            totalMaxCount += fishType.maxCount;
        }
        
        if (totalMaxCount > maxFishTotal)
        {
            float ratio = (float)maxFishTotal / totalMaxCount;
            
            // Ajusta cada límite proporcionalmente
            for (int i = 0; i < fishTypes.Count; i++)
            {
                int newMax = Mathf.Max(1, Mathf.FloorToInt(fishTypes[i].maxCount * ratio));
                Debug.Log($"Ajustando límite de {fishTypes[i].name} de {fishTypes[i].maxCount} a {newMax}");
                fishTypes[i].maxCount = newMax;
            }
            
            // Verifica nuevamente la suma
            int newTotal = 0;
            foreach (var fishType in fishTypes)
            {
                newTotal += fishType.maxCount;
            }
            
            Debug.Log($"Después del ajuste: Total máximo permitido: {maxFishTotal}, Suma de límites individuales: {newTotal}");
        }
    }

    // Método para depuración - puedes llamarlo desde otra parte o un botón
    public void DebugFishCounts()
    {
        UpdateFishCount(); // Asegúrate de tener conteos actualizados
        
        string debugText = $"Total: {currentFishTotal}/{maxFishTotal}\n";
        foreach (var fish in fishTypes)
        {
            debugText += $"{fish.name}: {fish.currentCount}/{fish.maxCount}\n";
        }
        Debug.Log(debugText);
    }
}

// Componente simple para identificar el tipo de pez
public class FishIdentifier : MonoBehaviour
{
    public int typeIndex = -1;
}