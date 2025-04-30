using System;
using System.Collections.Generic;
using UnityEngine;

// Estructura para almacenar datos del jugador
[Serializable]
public class PlayerData
{
    public int keys;
    public int food;
    public int coins;
    public int level;
    public int health; 
    public List<int> unlockedLevels = new List<int>();
    public Vector3 lastRespawnPosition;
    public int currentLevelId;
    
    // Lista de datos de cada nivel
    public List<LevelData> levelDataList = new List<LevelData>();
    
    // Diccionario serializable para el inventario
    public SerializableDictionary inventory = new SerializableDictionary();
    
    // Diccionario serializable para datos adicionales del jugador
    public SerializableDictionary playerProperties = new SerializableDictionary();
    
    private Dictionary<string, string> _inventoryCache;
    private Dictionary<string, string> _propertiesCache;
    
    // Método para obtener datos de un nivel específico
    public LevelData GetLevelData(int levelId)
    {
        LevelData levelData = levelDataList.Find(ld => ld.levelId == levelId);
        if (levelData == null)
        {
            // Si no existe, crear uno nuevo
            levelData = new LevelData { levelId = levelId, levelName = "Nivel " + levelId };
            levelDataList.Add(levelData);
        }
        return levelData;
    }
    
    // Métodos para manejar el inventario
    public Dictionary<string, string> GetInventory()
    {
        if (_inventoryCache == null)
        {
            _inventoryCache = inventory.ToDictionary();
        }
        return _inventoryCache;
    }
    
    public void UpdateInventory(Dictionary<string, string> newInventory)
    {
        _inventoryCache = newInventory;
        inventory.FromDictionary(newInventory);
    }
    
    // Métodos para manejar propiedades adicionales del jugador
    public Dictionary<string, string> GetProperties()
    {
        if (_propertiesCache == null)
        {
            _propertiesCache = playerProperties.ToDictionary();
        }
        return _propertiesCache;
    }
    
    public void UpdateProperties(Dictionary<string, string> newProperties)
    {
        _propertiesCache = newProperties;
        playerProperties.FromDictionary(newProperties);
    }
    
    // Métodos para obtener/establecer propiedades adicionales
    public string GetProperty(string key, string defaultValue = "")
    {
        var props = GetProperties();
        if (props.ContainsKey(key))
        {
            return props[key];
        }
        return defaultValue;
    }
    
    public void SetProperty(string key, string value)
    {
        var props = GetProperties();
        props[key] = value;
        UpdateProperties(props);
    }
    
    public int GetIntProperty(string key, int defaultValue = 0)
    {
        string val = GetProperty(key);
        if (int.TryParse(val, out int result))
        {
            return result;
        }
        return defaultValue;
    }
    
    public void SetIntProperty(string key, int value)
    {
        SetProperty(key, value.ToString());
    }
    
    public float GetFloatProperty(string key, float defaultValue = 0f)
    {
        string val = GetProperty(key);
        if (float.TryParse(val, out float result))
        {
            return result;
        }
        return defaultValue;
    }
    
    public void SetFloatProperty(string key, float value)
    {
        SetProperty(key, value.ToString());
    }
    
    public bool GetBoolProperty(string key, bool defaultValue = false)
    {
        string val = GetProperty(key);
        if (bool.TryParse(val, out bool result))
        {
            return result;
        }
        return defaultValue;
    }
    
    public void SetBoolProperty(string key, bool value)
    {
        SetProperty(key, value.ToString());
    }
}