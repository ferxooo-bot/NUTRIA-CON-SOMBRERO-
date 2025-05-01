using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public string chestId;
    public GameObject closedChestVisual;
    public GameObject openChestVisual;
    public List<GameObject> chestContents;
    private SaveSystem saveSystem;
    
    private int currentLevelId;
    
    private void Start()
    {
        saveSystem = SaveSystem.Instance;
        LevelData levelData = saveSystem.GetCurrentLevelData();
        currentLevelId= levelData.levelId; 

        // Verificar si el cofre ya está abierto
        bool isOpened = SaveSystem.Instance.IsChestOpened(chestId, currentLevelId);
        UpdateChestVisual(isOpened);
        
        // Si ya está abierto, desactivar los contenidos
        if (isOpened && chestContents != null)
        {
            foreach (GameObject item in chestContents)
            {
                if (item != null) item.SetActive(false);
            }
        }
    }
    
    public void OpenChest()
    {
        // Marcar el cofre como abierto
        SaveSystem.Instance.OpenChest(chestId, currentLevelId);
        UpdateChestVisual(true);
    }
    
    private void UpdateChestVisual(bool isOpened)
    {
        if (closedChestVisual != null) closedChestVisual.SetActive(!isOpened);
        if (openChestVisual != null) openChestVisual.SetActive(isOpened);
    }
}