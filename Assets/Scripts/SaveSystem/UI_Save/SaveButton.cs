using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI saveNameText;
    [SerializeField] private TextMeshProUGUI saveDateText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] public Button deleteButton;
    
    // Opcionalmente puedes tener un botón principal para cargar la partida
    [SerializeField] public Button loadButton;

    public void Initialize(string saveName, string saveDate, string playTime)
    {
        if (saveNameText != null)
            saveNameText.text = saveName;
            
        if (saveDateText != null)
            saveDateText.text = saveDate;
            
        if (playTimeText != null)
            playTimeText.text = playTime;
    }
    
    // Puedes agregar funciones auxiliares según sea necesario
    public void SetDeleteButtonActive(bool active)
    {
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(active);
    }
}