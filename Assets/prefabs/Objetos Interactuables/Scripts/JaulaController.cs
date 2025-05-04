using UnityEngine;

public class JaulaController : MonoBehaviour
{
    [Header("Cage Data")]
    public string jaulaId;
    public bool needKey = true;
    public int numberKeysNeeded = 1;
    
    [Header("Visual Settings")]
    public Sprite closedSprite;
    public Sprite openedSprite;
    public GameObject jaulaDerecha; // Referencia a "Jaula Derecha"
    
    [Header("Interaction Settings")]
    [SerializeField] private float rangoDeteccion = 2f;
    private GameObject jugador;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openJaulaSound;
    [SerializeField] private AudioClip needKeySound;
    
    // Sistema de guardado
    private SaveSystem saveSystem;
    private int currentLevelId;
    private bool hasBeenOpen;
    
    private void Start()
    {
        // Comprobar o agregar AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.5f;
            }
        }
        
        // Inicializar referencias
        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador == null)
            {
                Debug.LogError("No se encontró objeto con tag 'Player'");
            }
        }
        
        // Inicializar sistema de guardado
        saveSystem = SaveSystem.Instance;
        if (saveSystem == null)
        {
            Debug.LogError("No se encontró SaveSystem.Instance");
            return;
        }
        
        LevelData levelData = saveSystem.GetCurrentLevelData();
        currentLevelId = levelData.levelId;
        
        // Verificar si la jaula ya está abierta
        hasBeenOpen = SaveSystem.Instance.IsChestOpened(jaulaId, currentLevelId);
        
        // Si la jaula ya estaba abierta, actualizamos su estado visual
        if (hasBeenOpen)
        {
            SetJaulaOpenState();
        }
        else if (GetComponent<SpriteRenderer>() != null && closedSprite != null)
        {
            // Asegurar que el sprite inicial es el de la jaula cerrada
            GetComponent<SpriteRenderer>().sprite = closedSprite;
        }
    }
    
    private void Update()
    {
        // Si la jaula ya está abierta, no necesitamos hacer nada más
        if (hasBeenOpen) return;
        
        // Verificar distancia al jugador
        if (jugador != null)
        {
            float distancia = Vector2.Distance(transform.position, jugador.transform.position);
            
            if (distancia < rangoDeteccion && Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Se presionó E cerca de la jaula");
                TryOpenJaula();
            }
        }
    }
    
    public void TryOpenJaula()
    {
        // Si la jaula ya está abierta, no hacer nada
        if (hasBeenOpen) return;
        
        // Verificar si necesita llaves
        if (needKey)
        {
            if (HasEnoughKeys())
            {
                ConsumeKeys();
                OpenJaula();
            }
            else
            {
                PlaySound(needKeySound);
                Debug.Log("No tienes suficientes llaves para abrir esta jaula");
            }
            return;
        }
        
        // Si no tiene requisitos especiales, abrir directamente
        OpenJaula();
    }
    
    private bool HasEnoughKeys()
    {
        // Verificar si hay un InventoryPlayer en la escena
        InventoryPlayer inventory = FindObjectOfType<InventoryPlayer>();
        if (inventory == null) return false;
        
        // Intentar quitar las llaves temporalmente para verificar si hay suficientes
        bool hasKeys = inventory.RemoveItem("keys", numberKeysNeeded);
        
        // Si se pudieron quitar, devolverlas al inventario
        if (hasKeys)
        {
            inventory.AddItem("keys", numberKeysNeeded);
        }
        
        return hasKeys;
    }
    
    private void ConsumeKeys()
    {
        InventoryPlayer inventory = FindObjectOfType<InventoryPlayer>();
        if (inventory != null)
        {
            inventory.RemoveItem("keys", numberKeysNeeded);
        }
    }
    
    private void SetJaulaOpenState()
    {
        if (openedSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
        
        hasBeenOpen = true;
        
        // Eliminar la jaula derecha si existe
        if (jaulaDerecha != null)
        {
            Destroy(jaulaDerecha);
            Debug.Log("Jaula derecha eliminada");
        }
    }
    
    public void OpenJaula()
    {
        // Reproducir sonido de apertura
        PlaySound(openJaulaSound);
        
        // Marcar la jaula como abierta en el sistema de guardado
        SaveSystem.Instance.OpenChest(jaulaId, currentLevelId);
        hasBeenOpen = true;
        
        // Actualizar el estado visual
        SetJaulaOpenState();
        
        Debug.Log("Jaula abierta y guardada en Save System");
    }
    
    // Método auxiliar para reproducir sonidos
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}