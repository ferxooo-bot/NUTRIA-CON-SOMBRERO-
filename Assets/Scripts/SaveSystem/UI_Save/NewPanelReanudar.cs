using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewPanelReanudar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI saveNameText;
    [SerializeField] private TextMeshProUGUI saveDateText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] public Button deleteButton;
    [SerializeField] public Button EnterButton;
    
    public void Initialize(string saveName, string saveDate, string playTime)
    {
        if (saveNameText != null)
            saveNameText.text = saveName;
            
        if (saveDateText != null)
            saveDateText.text = saveDate;
            
        if (playTimeText != null)
            playTimeText.text = playTime;
    }
}