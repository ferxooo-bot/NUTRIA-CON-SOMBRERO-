using UnityEngine;

public class DoorController : MonoBehaviour
{
    public string doorId;
    public Animator doorAnimator;
    public bool startClosed = true;
    
    private int currentLevelId;
    private bool isOpen;
    
    private void Start()
    {
        // Obtener el ID del nivel actual
        currentLevelId = FindObjectOfType<LevelManager>().levelId;
        
        // Verificar si la puerta ya está abierta
        isOpen = SaveSystem.Instance.IsDoorOpened(doorId, currentLevelId);
        
        // Actualizar el estado visual de la puerta
        UpdateDoorVisual();
    }
    
    public void ToggleDoor()
    {
        isOpen = !isOpen;
        
        if (isOpen)
        {
            // Guardar que la puerta ha sido abierta
            SaveSystem.Instance.OpenDoor(doorId, currentLevelId);
        }
        
        UpdateDoorVisual();
    }
    
    private void UpdateDoorVisual()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("IsOpen", isOpen);
        }
    }
}