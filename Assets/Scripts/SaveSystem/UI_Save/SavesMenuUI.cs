using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SavesMenuUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelReanudarPrefab;
    [SerializeField] private Transform savesContainer;
    [SerializeField] private TextMeshProUGUI titleText;
     private string gameSceneName = "";
    
    private void Awake()
    {
        // Opcional: establecer título u otros elementos de UI
    }
    
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
        
        if (saves.Count == 0)
        {
            // Mostrar mensaje cuando no hay partidas guardadas
            GameObject messageObj = new GameObject("NoSavesMessage");
            messageObj.transform.SetParent(savesContainer, false);
            TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = "No hay partidas guardadas.";
            messageText.fontSize = 24;
            messageText.alignment = TextAlignmentOptions.Center;
            
            RectTransform rectTransform = messageText.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(20, 20);
            rectTransform.offsetMax = new Vector2(-20, -20);
            return;
        }
        
        // Ordenar partidas por fecha (más recientes primero)
        saves.Sort((a, b) => System.DateTime.Parse(b.saveDate)
            .CompareTo(System.DateTime.Parse(a.saveDate)));
        
        // Crear un panel para cada partida
        foreach (GameSave save in saves)
        {
            // Instanciar un panel para cada partida
            GameObject panelInstance = Instantiate(panelReanudarPrefab, savesContainer);
            
            // Buscar el componente SavePanel (que debería contener el método Initialize)
            NewPanelReanudar savePanel = panelInstance.GetComponent<NewPanelReanudar>();
            if (savePanel != null)
            {
                // Formatear tiempo de juego
                int hours = (int)(save.playTime / 3600);
                int minutes = (int)((save.playTime % 3600) / 60);
                string playTimeStr = string.Format("{0:D2}:{1:D2}", hours, minutes);
                
                // Inicializar el panel con los datos de la partida
                savePanel.Initialize(save.saveName, save.saveDate, playTimeStr);
            }
            
            // Configurar el botón de reanudar
            Button resumeButton = panelInstance.transform.Find("ResumeButton").GetComponent<Button>();
            if (resumeButton != null)
            {
                string saveNameCopy = save.saveName;
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(() => {
                    SaveSystem.Instance.LoadSave(saveNameCopy);
                    LoadGameScene(save);
                });
            }
            
            // Configurar el botón de eliminar
            Button deleteButton = panelInstance.transform.Find("DeleteButton").GetComponent<Button>();
            if (deleteButton != null)
            {
                string saveNameForDelete = save.saveName;
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(() => {
                    SaveSystem.Instance.DeleteSave(saveNameForDelete);
                    RefreshSavesList();
                });
            }
        }
    }
    
    private void LoadGameScene(GameSave save)
    {
        int levelId = save.playerData.currentLevelId;
        string sceneName = GetSceneById(levelId);
        SceneManager.LoadScene(sceneName);
    }

    private string GetSceneById(int levelId)
    {
        switch (levelId)
        {
            case 1:
                return "Lv.1";
            case 2:
                return "Lv.2";
            case 3:
                return "Lv.3";
            // Añade todos los niveles que tengas
            default:
                // Si el ID no coincide con ningún nivel conocido, carga la escena principal
                Debug.LogWarning($"ID de nivel desconocido: {levelId}. Cargando escena predeterminada.");
                return gameSceneName;
        }
    }
}