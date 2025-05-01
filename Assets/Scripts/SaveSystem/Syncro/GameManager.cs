using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    public static GameManager Instance { get; private set; }

    // Referencia al sistema de guardado
    private SaveSystem saveSystem;

    private void Awake() 
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            saveSystem = SaveSystem.Instance;
        } else {
            Destroy(gameObject);
        }
    }

    private void OnDestroy() 
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        StartCoroutine(SyncSceneObjectsAfterLoad());
    }

    private IEnumerator SyncSceneObjectsAfterLoad()
    {
        yield return null;

        GameSave currentSave = saveSystem.GetCurrentSave();
        if (currentSave != null)
        {
            SyncAllObjects();
        }
    }

    public void SyncAllObjects()
    {
        StartCoroutine(SafeSyncAllObjects());
    }

    private IEnumerator SafeSyncAllObjects()
    {
        GameObject playerObj = null;
        PlayerDataSynchronizer playerSync = null;

        while (playerSync == null)
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerSync = playerObj.GetComponent<PlayerDataSynchronizer>();

            yield return null;
        }

        playerSync.SyncPlayerWithSaveData();
        Debug.Log("Todos los objetos del jugador en la escena han sido sincronizados con los datos guardados");
    }

    public void SaveAllObjectsData()
    {
        StartCoroutine(SafeSaveAllObjects());
    }

    private IEnumerator SafeSaveAllObjects()
    {
        GameObject playerObj = null;
        PlayerDataSynchronizer playerSync = null;

        while (playerSync == null)
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerSync = playerObj.GetComponent<PlayerDataSynchronizer>();

            yield return null;
        }

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
