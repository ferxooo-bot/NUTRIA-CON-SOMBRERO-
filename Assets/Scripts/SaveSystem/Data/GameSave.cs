using System;
using UnityEngine;
using System.Collections.Generic;


// Estructura para cada partida guardada
[Serializable]
public class GameSave
{
    public string saveName;
    public string saveDate;
    public PlayerData playerData;
    public List<LevelData> levels = new List<LevelData>();
    public float playTime;
    public string gameVersion;

    // Constructor por defecto requerido para serialización
    public GameSave() { }
    
    // Constructor que inicializa todos los niveles
    public GameSave(int totalLevels)
    {
        // Inicializar la lista de niveles
        levels = new List<LevelData>();
        
        // Crear datos para cada nivel
        for (int i = 1; i <= totalLevels; i++)
        {
            levels.Add(new LevelData
            {
                levelId = i,
                levelName = "Nivel " + i,
                isCompleted = false
                // openedChests y openedDoors ya se inicializan vacíos por defecto
            });
        }
    }



    public LevelData GetLevelData(int levelId)
    { 
        // Buscar el nivel con el ID actual
        foreach (LevelData level in levels)
        {
            if (level.levelId == levelId)
            {
                return level;
            }
        }
        
        // Si no se encuentra, mostrar mensaje de error y devolver null
        Debug.LogWarning("No se encontró el nivel actual con ID: " + levelId);
        return null;
    } 
}