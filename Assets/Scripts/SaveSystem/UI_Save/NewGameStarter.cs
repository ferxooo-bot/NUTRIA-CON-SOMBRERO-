using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGameStarter : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public Button startButton;
    public string cinematicSceneName = "IntroductoryCinematic"; // Nombre de la escena con tu CinematicController
    
    private void Start()
    {
        // Conectar el botón al método si se ha asignado
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartNewGame);
        }
    }
    
    public void StartNewGame()
    {
        string saveName = "Partida " + (SaveSystem.Instance.GetAllSaves().Count + 1);
        string playerName = playerNameInput.text;
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "SIN NOMBRE"; 
        }
        
        // Crear nueva partida con el nombre proporcionado
        SaveSystem.Instance.CreateNewSave(saveName, playerName);
        
        // Guardar información del nivel al que debemos ir después de la cinemática
        GameSave currentSave = SaveSystem.Instance.GetCurrentSave();
        string startLevel = SaveSystem.Instance.GetSceneById(currentSave.playerData.currentLevelId);
        PlayerPrefs.SetString("LevelAfterCinematic", startLevel);
        PlayerPrefs.Save();
        
        // Cargar la escena de la cinemática
        SceneManager.LoadScene(cinematicSceneName);
    }
}