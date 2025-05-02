using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public string itemId;
    public string itemType; // "keys", "food", "coins" o custom
    public int amount = 1;
    private SaveSystem saveSystem;
    public LevelData currentLevel;


    private void Start()
    {

    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

        }
    }
}