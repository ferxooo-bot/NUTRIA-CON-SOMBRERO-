using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGameStarter : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public Button startButton;

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
        SaveSystem.Instance.CreateNewSave(saveName);
        
        GameSave currentSave = SaveSystem.Instance.GetCurrentSave();
        string startLevel = SaveSystem.Instance.GetSceneById(currentSave.playerData.currentLevelId); 
        
        SceneManager.LoadScene(startLevel);
    }
}