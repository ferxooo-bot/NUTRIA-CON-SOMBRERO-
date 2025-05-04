using System.Collections;
using UnityEngine;

public class PlayerDataSynchronizer : MonoBehaviour
{
    [SerializeField] private FatherMoment1 fatherMovement;

    private SaveSystem saveSystem;
    
    private void Awake()
    {
        saveSystem = SaveSystem.Instance;
        
        if (fatherMovement == null)
        {
            fatherMovement = GetComponent<FatherMoment1>();
        }

    }
    
    private void Start()
    {

        if (saveSystem.GetCurrentSave() != null)
        {
            SyncPlayerWithSaveData();
        }
    }
    
    
    // Este método solo se encarga de actualizar el estado del SaveSystem con los datos actuales del player

    //cada vez que el jugador reciba daño o toque un nuevo respawn se ejecuta esto para actualizar Bd; 
    public void UpdateSaveWithPlayerData(){
        GameSave currentSave = saveSystem.GetCurrentSave();
        if (saveSystem.GetCurrentSave() == null)
        {
            Debug.LogWarning("No hay partida actual para actualizar");
            return;
        }

        int currentLevelID = currentSave.playerData.currentLevelId; 

        currentSave.playerData.health= fatherMovement.currentHealth; 

        currentSave.playerData.lastRespawn = fatherMovement.respawnPoint.name; 

        saveSystem.SaveGame();
    }


//Inverso al anterior : cuando se inicia una escena: 

    public void SyncPlayerWithSaveData(){
        GameSave currentSave = saveSystem.GetCurrentSave();
        if (currentSave == null || currentSave.playerData == null)
        {
            Debug.LogWarning("No hay datos para sincronizar con el jugador");
            return;
        }
        
        string lastRespawn = currentSave.playerData.lastRespawn; 

        fatherMovement.SetRespawnPoint(lastRespawn); 
        StartCoroutine(fatherMovement.RespawnWithOutDelay());
    }
}