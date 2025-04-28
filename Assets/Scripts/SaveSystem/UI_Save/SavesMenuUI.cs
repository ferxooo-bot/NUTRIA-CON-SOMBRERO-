using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SavesMenuUI : MonoBehaviour
{
    public GameObject saveButtonPrefab;
    public Transform savesContainer;
    public TMP_InputField newSaveNameInput;

    private void OnEnable()
    {
        RefreshSavesList();
    }

    public void RefreshSavesList()
    {
        // Limpiar lista actual
        foreach (Transform child in savesContainer)
        {
            Destroy(child.gameObject);
        }

        // Obtener todas las partidas guardadas
        List<GameSave> saves = SaveSystem.Instance.GetAllSaves();

        // Crear botón para cada partida
        foreach (GameSave save in saves)
        {
            GameObject buttonObj = Instantiate(saveButtonPrefab, savesContainer);
            SaveButton saveButton = buttonObj.GetComponent<SaveButton>();
            
            // Formatear tiempo de juego en horas:minutos
            int hours = (int)(save.playTime / 3600);
            int minutes = (int)((save.playTime % 3600) / 60);
            string playTimeStr = string.Format("{0:D2}:{1:D2}", hours, minutes);
            
            saveButton.Initialize(save.saveName, save.saveDate, playTimeStr);
            
            // Configurar eventos
            Button button = buttonObj.GetComponent<Button>();
            string saveName = save.saveName; // Variable local para captura
            
            button.onClick.AddListener(() => {
                SaveSystem.Instance.LoadSave(saveName);
                StartGame();
            });

            // Botón de eliminar
            Transform deleteButton = buttonObj.transform.Find("DeleteButton");
            if (deleteButton != null)
            {
                deleteButton.GetComponent<Button>().onClick.AddListener(() => {
                    SaveSystem.Instance.DeleteSave(saveName);
                    RefreshSavesList();
                });
            }
        }
    }

    public void CreateNewSave()
    {
        string saveName = newSaveNameInput.text;
        if (string.IsNullOrEmpty(saveName))
        {
            saveName = "Partida " + (SaveSystem.Instance.GetAllSaves().Count + 1);
        }

        SaveSystem.Instance.CreateNewSave(saveName);
        StartGame();
    }

    private void StartGame()
    {
        // Cargar la escena del juego
        SceneManager.LoadScene("GameScene");
    }
}