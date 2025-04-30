using UnityEngine;

public class PlayerSaveController : MonoBehaviour
{
    // Solo muestra un ejemplo simplificado para la integración con el sistema de guardado
    public int maxHealth = 100;
    public int currentHealth = 100;
    
    private void Start()
    {
        // Suscribirse al evento de carga de partida
        SaveSystem.Instance.OnGameLoaded += OnGameLoaded;
        
        // Cargar datos iniciales si hay una partida activa
        GameSave currentSave = SaveSystem.Instance.GetCurrentSave();
        if (currentSave != null)
        {
            LoadPlayerData(currentSave.playerData);
        }
    }
    
    private void OnDestroy()
    {
        // Desuscribirse del evento
        SaveSystem.Instance.OnGameLoaded -= OnGameLoaded;
    }
    
    private void OnGameLoaded(PlayerData playerData)
    {
        LoadPlayerData(playerData);
    }
    
    private void LoadPlayerData(PlayerData playerData)
    {
        currentHealth = playerData.health;
        
        // Si hay posición de respawn guardada, usarla
        if (playerData.currentLevelId == FindObjectOfType<LevelManager>().levelId)
        {
            transform.position = playerData.lastRespawnPosition;
        }
        
        // Aquí cargarías más datos según tus necesidades
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Actualizar salud en el sistema de guardado
        if (SaveSystem.Instance.GetCurrentSave() != null)
        {
            SaveSystem.Instance.GetCurrentSave().playerData.health = currentHealth;
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        // Actualizar salud en el sistema de guardado
        if (SaveSystem.Instance.GetCurrentSave() != null)
        {
            SaveSystem.Instance.GetCurrentSave().playerData.health = currentHealth;
        }
    }
    
    private void Die()
    {
        // Implementa la lógica de muerte del jugador
        // Por ejemplo, reaparecer en el último checkpoint
        
        if (SaveSystem.Instance.GetCurrentSave() != null)
        {
            // Restaurar salud
            currentHealth = maxHealth;
            SaveSystem.Instance.GetCurrentSave().playerData.health = maxHealth;
            
            // Teletransportar al último punto de respawn
            transform.position = SaveSystem.Instance.GetCurrentSave().playerData.lastRespawnPosition;
        }
    }
    
    // Método para guardar manualmente el juego (podría asignarse a un botón)
    public void SaveGame()
    {
        // Actualizar datos del jugador antes de guardar
        if (SaveSystem.Instance.GetCurrentSave() != null)
        {
            SaveSystem.Instance.GetCurrentSave().playerData.health = currentHealth;
            SaveSystem.Instance.SaveGame();
        }
    }
}