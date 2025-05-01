using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int levelId;
    public string levelName;
    

    
    private void Awake()
    {


        // Registrar este nivel como el actual cuando se carga
        SaveSystem.Instance.SetCurrentLevel(levelId, levelName);
    }
}