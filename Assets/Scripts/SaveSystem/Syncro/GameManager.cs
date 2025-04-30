using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    public static GameManager Instance { get; private set; }
    
    // Referencia al sistema de guardado
    private SaveSystem saveSystem;
    
    private void Awake() 
    {
        // Implementación del patrón Singleton
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Obtener referencia al sistema de guardado
            saveSystem = SaveSystem.Instance;
        } else {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy() 
    {
        // Desuscribirse del evento cuando se destruye el objeto
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        // Cuando una nueva escena se carga, espera un frame para asegurar que todos los objetos estén inicializados
        StartCoroutine(SyncSceneObjectsAfterLoad());
    }
    
    private IEnumerator SyncSceneObjectsAfterLoad()
    {
        // Esperar un frame para asegurar que todos los objetos estén inicializados
        yield return null;
        
        // Verificar si hay una partida guardada cargada
        GameSave currentSave = saveSystem.GetCurrentSave();
        if (currentSave != null) {
            // Buscar todos los sincronizadores de jugador en la escena y activarlos
            SyncAllObjects();
        }
    }
    
    // Este método coordina la sincronización de todos los sincronizadores en la escena
    public void SyncAllObjects() //SINCRONICEN CON LA BASE DE DATOS!!!!!!!!!!!!
    {
        // Jugador
        PlayerDataSynchronizer playerSync = GetComponent<PlayerDataSynchronizer>();
        playerSync.SyncPlayerWithSaveData();
        
        Debug.Log("Todos los objetos del jugador en la escena han sido sincronizados con los datos guardados");
    }
    
    // Este método coordina el guardado de datos de todos los sincronizadores
    public void SaveAllObjectsData() //GUARDEN TODO LO NECESARIO
    {
        // Jugador
        PlayerDataSynchronizer playerSync = FindObjectOfType<PlayerDataSynchronizer>();

        
        playerSync.UpdateSaveWithPlayerData();
                
        Debug.Log("Todos los datos del jugador han sido guardados");
    }
    
    
    
    private int GetCurrentLevelId()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        switch (currentSceneName)
        {
            case "Lv.1":
                return 1;
            case "Lv.2":
                return 2;
            case "Lv.3":
                return 3;
            default:
                return 0;
        }
    }
    
    private string GetSceneById(int levelId)
    {
        switch (levelId)
        {
            case 1:
                return "Lv.1";
            case 2:
                return "Lv.2";
            case 3:
                return "Lv.3";
            
            default:
                Debug.LogWarning($"ID de nivel desconocido: {levelId}. Cargando escena predeterminada.");
                return "Pantalla_Inicial";
        }
    }
}