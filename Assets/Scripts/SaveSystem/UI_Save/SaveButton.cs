using TMPro;
using UnityEngine;

public class SaveButton : MonoBehaviour
{
    public TextMeshProUGUI saveNameText;
    public TextMeshProUGUI saveDateText;
    public TextMeshProUGUI playTimeText;

    public void Initialize(string saveName, string saveDate, string playTime = "")
    {
        saveNameText.text = saveName;
        saveDateText.text = saveDate;
        
        if (!string.IsNullOrEmpty(playTime))
        {
            playTimeText.text = "Tiempo: " + playTime;
        }
        else
        {
            playTimeText.gameObject.SetActive(false);
        }
    }
}