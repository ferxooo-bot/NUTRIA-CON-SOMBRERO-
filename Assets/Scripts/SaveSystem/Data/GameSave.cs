using System;
using UnityEngine;


// Estructura para cada partida guardada
[Serializable]
public class GameSave
{
    public string saveName;
    public string saveDate;
    public PlayerData playerData;
    public float playTime;
    public string gameVersion;
}
