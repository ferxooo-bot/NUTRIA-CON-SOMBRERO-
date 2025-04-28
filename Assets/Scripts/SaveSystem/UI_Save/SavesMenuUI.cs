using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SavesMenuUI : MonoBehaviour
{
    public GameObject saveButtonPrefab;
    public Transform savesContainer;
    
    // Removed input field for new save name since we're focusing on resuming saves

    private void OnEnable()
    {
        RefreshSavesList();
    }

    public void RefreshSavesList()
    {
        // Clear current list
        foreach (Transform child in savesContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all saved games
        List<GameSave> saves = SaveSystem.Instance.GetAllSaves();

        if (saves.Count == 0)
        {
            // Create message when no saves exist
            GameObject messageObj = new GameObject("NoSavesMessage");
            messageObj.transform.SetParent(savesContainer, false);
            TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = "No hay partidas guardadas.";
            messageText.fontSize = 24;
            messageText.alignment = TextAlignmentOptions.Center;
            return;
        }

        // Create button for each save
        foreach (GameSave save in saves)
        {
            GameObject buttonObj = Instantiate(saveButtonPrefab, savesContainer);
            SaveButton saveButton = buttonObj.GetComponent<SaveButton>();
            
            // Format play time as hours:minutes
            int hours = (int)(save.playTime / 3600);
            int minutes = (int)((save.playTime % 3600) / 60);
            string playTimeStr = string.Format("{0:D2}:{1:D2}", hours, minutes);
            
            saveButton.Initialize(save.saveName, save.saveDate, playTimeStr);
            
            // Configure events
            Button button = buttonObj.GetComponent<Button>();
            string saveName = save.saveName; // Local variable for capture
            
            button.onClick.AddListener(() => {
                SaveSystem.Instance.LoadSave(saveName);
                ResumeGame();
            });
            
            // Delete button
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

    // Renamed method to better reflect its purpose
    private void ResumeGame()
    {
        // Load the game scene
        SceneManager.LoadScene("GameScene");
    }
    
    // If you still need the ability to create a new game, but don't want it front and center,
    // you can keep this method and call it from a less prominent "New Game" button
    public void CreateNewGame()
    {
        string saveName = "Partida " + (SaveSystem.Instance.GetAllSaves().Count + 1);
        SaveSystem.Instance.CreateNewSave(saveName);
        ResumeGame();
    }
}