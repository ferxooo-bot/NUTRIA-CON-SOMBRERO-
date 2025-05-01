using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public string itemId;
    public string itemType; // "keys", "food", "coins" o custom
    public int amount = 1;
    
    private int currentLevelId;
    
    private void Start()
    {
        // Obtener el ID del nivel actual
        currentLevelId = FindObjectOfType<LevelManager>().levelId;

    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            
            // Desactivar el objeto
            gameObject.SetActive(false);
            
            // Aquí podrías añadir efectos visuales o sonoros de recolección
        }
    }
}