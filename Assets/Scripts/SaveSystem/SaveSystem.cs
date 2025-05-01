using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    private static SaveSystem _instance;
    public static SaveSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SaveSystem");
                _instance = go.AddComponent<SaveSystem>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private const string SAVES_DIRECTORY = "GameSaves";
    private GameSave currentSave;
    private float sessionStartTime;
    public string gameVersion = "1.0"; 
    
    // Eventos
    public event Action<PlayerData> OnGameLoaded;
    public event Action<PlayerData> OnGameSaved;

    private void Awake()
    {
        Debug.Log("persistentDataPath: " + Application.persistentDataPath);
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        
        string savesPath = Path.Combine(Application.persistentDataPath, SAVES_DIRECTORY);
        if (!Directory.Exists(savesPath))
        {
            Directory.CreateDirectory(savesPath);
        }
        
        sessionStartTime = Time.time;
    }

    

    public List<GameSave> GetAllSaves()
    {
        List<GameSave> saves = new List<GameSave>();
        
        string savesPath = Path.Combine(Application.persistentDataPath, SAVES_DIRECTORY);
        if (Directory.Exists(savesPath))
        {
            string[] saveFiles = Directory.GetFiles(savesPath, "*.json");
            
            foreach (string file in saveFiles)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    GameSave save = JsonUtility.FromJson<GameSave>(json);
                    saves.Add(save);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error al cargar el archivo de guardado: {file}. Error: {e.Message}");
                }
            }
        }
        
        return saves;
    }

    public GameSave GetCurrentSave()
    {
        return currentSave;
    }

    public void CreateNewSave(string saveName, string playerName)
    {
        PlayerData newPlayerData = new PlayerData
        {
            playerName = playerName,
            health = 3,
            level = 1,//PRUEBAS
            food =23,
            coins =100,
            currentLevelId = 1
        };
        
        GameSave newSave = new GameSave(1)
        {
            saveName = saveName,
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            playerData = newPlayerData,
            playTime = 0,
            gameVersion = gameVersion
        };
        
        currentSave = newSave;
        SaveGame();
        
        if (OnGameLoaded != null)
            OnGameLoaded(newPlayerData);
    }

    public void LoadSave(string saveName)
    {
        string savePath = Path.Combine(Application.persistentDataPath, SAVES_DIRECTORY, saveName + ".json");
        
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                GameSave save = JsonUtility.FromJson<GameSave>(json);
                currentSave = save;
                sessionStartTime = Time.time - save.playTime;
                
                if (OnGameLoaded != null)
                    OnGameLoaded(save.playerData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error al cargar la partida {saveName}: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"No se encontró el archivo de guardado {saveName}");
        }
    }

    public void DeleteSave(string saveName)
    {
        string savePath = Path.Combine(Application.persistentDataPath, SAVES_DIRECTORY, saveName + ".json");
        
        if (File.Exists(savePath))
        {
            try
            {
                File.Delete(savePath);
                
                // Si era la partida actual, limpiarla
                if (currentSave != null && currentSave.saveName == saveName)
                {
                    currentSave = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error al eliminar la partida {saveName}: {e.Message}");
            }
        }
    }

    private void UpdatePlayTime()
    {
        if (currentSave != null)
        {
            currentSave.playTime = Time.time - sessionStartTime;
        }
    }

    public void SaveGame()
    {
        if (currentSave == null) return;
        
        try
        {
            currentSave.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            UpdatePlayTime();
            
            string savePath = Path.Combine(Application.persistentDataPath, SAVES_DIRECTORY, currentSave.saveName + ".json");
            string json = JsonUtility.ToJson(currentSave, true); // true para formato legible
            File.WriteAllText(savePath, json);
            
            if (OnGameSaved != null)
                OnGameSaved(currentSave.playerData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al guardar la partida: {e.Message}");
        }
    }

    // Métodos para cambiar nivel
    public void SetCurrentLevel(int levelId, string levelName = "")
    {
        if (currentSave == null) return;
        
        currentSave.playerData.currentLevelId = levelId;
        
        // Asegurarse de que existe el nivel en la lista
        LevelData levelData = currentSave.GetLevelData(levelId);
        if (!string.IsNullOrEmpty(levelName) && string.IsNullOrEmpty(levelData.levelName))
        {
            levelData.levelName = levelName;
        }
        
        SaveGame();
    }

    public LevelData GetCurrentLevelData()
    {
        if (currentSave == null) return null;
        return currentSave.GetLevelData(currentSave.playerData.currentLevelId);
    }

    // Métodos específicos para modificar datos del jugador
    public void AddItem(string itemType, int amount)
    {
        if (currentSave == null) return;
        
        switch (itemType.ToLower())
        {
            case "keys":
                currentSave.playerData.keys += amount;
                break;
            case "food":
                currentSave.playerData.food += amount;
                break;
            case "coins":
                currentSave.playerData.coins += amount;
                break;
            default:
                // Para otros tipos de items, agregar al inventario
                var inventory = currentSave.playerData.GetInventory();
                if (!inventory.ContainsKey(itemType))
                {
                    inventory[itemType] = "0";
                }
                
                if (int.TryParse(inventory[itemType], out int currentAmount))
                {
                    inventory[itemType] = (currentAmount + amount).ToString();
                }
                else
                {
                    inventory[itemType] = amount.ToString();
                }
                
                currentSave.playerData.UpdateInventory(inventory);
                break;
        }
        
        SaveGame();
    }

    public int GetItemCount(string itemType)
    {
        if (currentSave == null) return 0;
        
        switch (itemType.ToLower())
        {
            case "keys":
                return currentSave.playerData.keys;
            case "food":
                return currentSave.playerData.food;
            case "coins":
                return currentSave.playerData.coins;
            default:
                var inventory = currentSave.playerData.GetInventory();
                if (inventory.ContainsKey(itemType) && int.TryParse(inventory[itemType], out int amount))
                {
                    return amount;
                }
                return 0;
        }
    }


    public void SetRespawnPoint(string lastRespawn, int levelId)
    {
        if (currentSave == null) return;
        
        currentSave.playerData.lastRespawn = lastRespawn;
        currentSave.playerData.currentLevelId = levelId;
        SaveGame();
    }

    // Métodos para objetos específicos de niveles
    public void OpenChest(string chestId, int levelId)
    {
        if (currentSave == null) return;
        
        LevelData levelData = currentSave.GetLevelData(levelId);

        if (!levelData.openedChests.Contains(chestId))
        {
            levelData.openedChests.Add(chestId);
            SaveGame();
        }
    }


    public void OpenDoor(string doorId, int levelId)
    {
        if (currentSave == null) return;
        
        LevelData levelData = currentSave.GetLevelData(levelId);
        if (!levelData.openedDoors.Contains(doorId))
        {
            levelData.openedDoors.Add(doorId);
            SaveGame();
        }
    }


    public void CompleteLevel(int levelId)
    {
        if (currentSave == null) return;
        
        LevelData levelData = currentSave.GetLevelData(levelId);
        levelData.isCompleted = true;
        SaveGame();
    }

    // Métodos para guardar datos adicionales en un nivel
    public void SetLevelProperty(int levelId, string key, string value)
    {
        if (currentSave == null) return;
        
        LevelData levelData = currentSave.GetLevelData(levelId);
        if (levelData.additionalData == null)
        {
            levelData.additionalData = new Dictionary<string, string>();
        }
        
        levelData.additionalData[key] = value;
        SaveGame();
    }
    
    public string GetLevelProperty(int levelId, string key, string defaultValue = "")
    {
        if (currentSave == null) return defaultValue;
        
        LevelData levelData = currentSave.GetLevelData(levelId);
        if (levelData.additionalData != null && levelData.additionalData.ContainsKey(key))
        {
            return levelData.additionalData[key];
        }
        
        return defaultValue;
    }
    
    // Métodos genéricos para guardar datos adicionales del jugador
    public void SetPlayerProperty(string key, string value)
    {
        if (currentSave == null) return;
        currentSave.playerData.SetProperty(key, value);
        SaveGame();
    }
    
    public string GetPlayerProperty(string key, string defaultValue = "")
    {
        if (currentSave == null) return defaultValue;
        return currentSave.playerData.GetProperty(key, defaultValue);
    }
    
    public void SetPlayerIntProperty(string key, int value)
    {
        if (currentSave == null) return;
        currentSave.playerData.SetIntProperty(key, value);
        SaveGame();
    }
    
    public int GetPlayerIntProperty(string key, int defaultValue = 0)
    {
        if (currentSave == null) return defaultValue;
        return currentSave.playerData.GetIntProperty(key, defaultValue);
    }
    
    public void SetPlayerBoolProperty(string key, bool value)
    {
        if (currentSave == null) return;
        currentSave.playerData.SetBoolProperty(key, value);
        SaveGame();
    }
    
    public bool GetPlayerBoolProperty(string key, bool defaultValue = false)
    {
        if (currentSave == null) return defaultValue;
        return currentSave.playerData.GetBoolProperty(key, defaultValue);
    }









    public bool IsChestOpened(string chestId, int levelId)
    {
        if (currentSave == null) return false;
        
        LevelData levelData = currentSave.GetLevelData(levelId);
        return levelData.openedChests.Contains(chestId);
    }

















    public bool IsDoorOpened(string doorId, int levelId)
    {
        if (currentSave == null) return false;
        
        LevelData levelData = currentSave.GetLevelData(levelId);
        return levelData.openedDoors.Contains(doorId);
    }





    public string GetSceneById(int levelId)
    {
        switch (levelId)
        {
            case 1:
                return "Lv.1";
            case 2:
                return "Lv.2";
            case 3:
                return "Lv.3";
            // Añade todos los niveles que tengas
            default:
                // Si el ID no coincide con ningún nivel conocido, carga la escena principal
                Debug.LogWarning($"ID de nivel desconocido: {levelId}. Cargando escena predeterminada.");
                return "";
        }
    }
}