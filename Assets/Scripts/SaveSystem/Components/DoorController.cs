using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private pregunta preguntasScript;
    public string doorId;
    public bool startClosed = true;
    private bool isOpen;
    private SaveSystem saveSystem;
    public LevelData currentLevel; 

    private void Start()
    {
        preguntasScript = GetComponent<pregunta>(); 
        doorId = preguntasScript.doorId; 

        saveSystem = SaveSystem.Instance;
        if (saveSystem != null)
        {
            // Obtener datos del nivel actual
            currentLevel = saveSystem.GetCurrentLevelData();
            
            // Verificar si la puerta ya está abierta en el guardado
            if (currentLevel != null)
            {
                isOpen = currentLevel.openedDoors.Contains(doorId);
                // Si la puerta ya está abierta en la base de datos, destruirla
                if (isOpen)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                Debug.LogWarning("No se pudieron cargar datos del nivel actual para la puerta " + doorId);
                isOpen = !startClosed;
            }
        }
    }


    public void AddNewDoorOpen()
    {
        if (currentLevel != null && !string.IsNullOrEmpty(doorId))
        {
            if (!currentLevel.openedDoors.Contains(doorId))
            {
                currentLevel.openedDoors.Add(doorId);
                saveSystem.SaveGame();

                Debug.Log("Puerta " + doorId + " actualizada en la base de datos");
            }
        }

        
    }

}