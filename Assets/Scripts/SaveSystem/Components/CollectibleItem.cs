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
        
        // Verificar si el ítem ya ha sido recogido
        if (SaveSystem.Instance.IsItemCollected(itemId, currentLevelId))
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Añadir el ítem al inventario
            SaveSystem.Instance.AddItem(itemType, amount);
            
            // Marcar el ítem como recogido
            SaveSystem.Instance.CollectItem(itemId, currentLevelId);
            
            // Desactivar el objeto
            gameObject.SetActive(false);
            
            // Aquí podrías añadir efectos visuales o sonoros de recolección
        }
    }
}