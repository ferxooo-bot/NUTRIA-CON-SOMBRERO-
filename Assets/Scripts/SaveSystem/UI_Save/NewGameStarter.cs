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
        string playerName = playerNameInput.text;
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Partida " + (SaveSystem.Instance.GetAllSaves().Count + 1);
        }

        // Crear nueva partida con el nombre proporcionado
        SaveSystem.Instance.CreateNewSave(playerName);
        
        // Cargar la escena del juego
        SceneManager.LoadScene("Lv.1");
    }
}