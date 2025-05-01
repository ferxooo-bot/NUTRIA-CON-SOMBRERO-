using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData
{
    public int levelId;
    public string levelName;
    public List<string> openedChests = new List<string>();
    public List<string> openedDoors = new List<string>();
    public bool isCompleted = false;
    
    

    
    // Campo para almacenar datos adicionales en formato string:valor
    public Dictionary<string, string> additionalData = new Dictionary<string, string>();
}
