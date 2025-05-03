using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    [Header("Chest Data")]
    public string chestId;
    public List<GameObject> chestContents;
    public bool hasPassword; // Si tiene contraseña, abre un canvas para corroborarla
    public bool needKey;
    public int numberKeysNeeded;
    
    [Header("Animation")]
    public Animator chestAnimator; // Referencia al Animator del cofre
    public string openTriggerName = "Open"; // Nombre del trigger para la animación de apertura
    public SpriteRenderer chestRenderer; // Referencia al SpriteRenderer del cofre
    public Sprite closedSprite; // Sprite para el cofre cerrado
    public Sprite openedSprite; // Sprite para el cofre abierto

    [Header("Interaction Settings")]
    [SerializeField] private float rangoDeteccion = 2f;
    private GameObject jugador;

    [Header("UI References")]
    [SerializeField] private GameObject passwordCanvas;
    [SerializeField] private GameObject missingKeyMessage;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openChest;
    [SerializeField] private AudioClip givesItems;
    [SerializeField] private AudioClip passwordFailSound;
    [SerializeField] private AudioClip passwordSuccessSound;
    [SerializeField] private AudioClip needKeySound;

    // Sistema de guardado
    private SaveSystem saveSystem;
    private int currentLevelId;
    private bool hasBeenOpen;

    [Header("Password Settings")]
    public string correctPassword = "1234"; // Contraseña correcta para este cofre
    private LockScript lockScript; // Referencia al script de contraseña
    
    private void Start()
    {
        // Comprobar o agregar AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                // Configurar el AudioSource
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.5f; // Medio 2D, medio 3D para sonido posicional
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

        // Verificar referencia del canvas
        if (passwordCanvas == null)
        {
            Debug.LogError("Password Canvas no asignado en " + gameObject.name);
        }
        else
        {
            // Asegurar que los canvas están desactivados al inicio
            passwordCanvas.SetActive(false);
            Debug.Log("Canvas de contraseña desactivado al inicio");
            
            // Buscar el LockScript en el canvas de contraseña
            lockScript = passwordCanvas.GetComponentInChildren<LockScript>();
            if (lockScript != null)
            {
                // Establecer la conexión bidireccional
                lockScript.SetConnectedChest(this);
                lockScript.correctCode = correctPassword;
                Debug.Log("LockScript conectado al cofre correctamente");
            }
            else
            {
                Debug.LogWarning("No se encontró LockScript en el canvas de contraseña");
            }
        }

        if (missingKeyMessage != null)
        {
            missingKeyMessage.SetActive(false);
        }
        
        // Verificar componentes de animación
        if (chestAnimator == null)
        {
            chestAnimator = GetComponent<Animator>();
        }
        
        if (chestRenderer == null)
        {
            chestRenderer = GetComponent<SpriteRenderer>();
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

        // Verificar si el cofre ya está abierto
        hasBeenOpen = SaveSystem.Instance.IsChestOpened(chestId, currentLevelId);

        // Si el cofre ya estaba abierto, actualizamos su estado visual
        if (hasBeenOpen)
        {
            SetChestOpenState();
            
        }
        else if (chestRenderer != null && closedSprite != null)
        {
            // Asegurar que el sprite inicial es el del cofre cerrado
            chestRenderer.sprite = closedSprite;
        }
    }

    private void Update()
    {
        // Si el cofre ya está abierto, no necesitamos hacer nada más
        if (hasBeenOpen) return;

        // Verificar distancia al jugador
        if (jugador != null)
        {
            float distancia = Vector2.Distance(transform.position, jugador.transform.position);
            
            if (distancia < rangoDeteccion)
            {
                // El jugador está cerca, verificar interacción
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("Se presionó E cerca del cofre");
                    TryOpenChest();
                }
                
                // Si presiona Escape, cerrar UI
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    OcultarUI();
                }
            }
            else
            {
                // El jugador está lejos, ocultar cualquier UI
                OcultarUI();
            }
        }
    }

    private void OcultarUI()
    {
        // Ocultar todos los elementos de UI relacionados con el cofre
        if (passwordCanvas != null && passwordCanvas.activeSelf)
        {
            passwordCanvas.SetActive(false);
        }

        if (missingKeyMessage != null && missingKeyMessage.activeSelf)
        {
            missingKeyMessage.SetActive(false);
        }
    }

    public void TryOpenChest()
    {
        // Si el cofre ya está abierto, no hacer nada
        if (hasBeenOpen) return;
        
        // Verificar si necesita contraseña
        if (hasPassword)
        {
            MostrarPasswordCanvas();
            Debug.Log("Mostrando canvas de contraseña");
            return;
        }
        
        // Verificar si necesita llaves
        if (needKey)
        {
            if (HasEnoughKeys())
            {
                ConsumeKeys();
                OpenChest();
            }
            else
            {
                ShowMissingKeyMessage();
                PlaySound(needKeySound);
            }
            return;
        }
        
        // Si no tiene requisitos especiales, abrir directamente
        OpenChest();
    }
    
    private void MostrarPasswordCanvas()
    {
        if (passwordCanvas != null)
        {
            passwordCanvas.SetActive(true);
            
            // Verificar que se activó correctamente
            if (!passwordCanvas.activeSelf)
            {
                Debug.LogError("¡No se pudo activar el canvas de contraseña!");
            }
        }
        else
        {
            Debug.LogError("Password canvas not assigned in ChestController!");
        }
    }

    public void ClosePasswordCanvas()
    {
        if (passwordCanvas != null)
        {
            passwordCanvas.SetActive(false);
        }
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
    
    public void CheckPassword(string enteredPassword)
    {
        // Este método es llamado desde el LockScript
        Debug.Log("Verificando contraseña: " + enteredPassword);
        
        // Si la contraseña es correcta, abrir el cofre
        if (enteredPassword == correctPassword)
        {
            Debug.Log("Contraseña correcta, abriendo cofre...");
            PlaySound(passwordSuccessSound);
            ClosePasswordCanvas();
            OpenChest();
        }
        else
        {
            // Reproducir sonido de contraseña incorrecta
            PlaySound(passwordFailSound);
        }
        // No es necesario hacer nada más si la contraseña es incorrecta, ya que
        // el LockScript ya maneja la limpieza del input
    }
    
    // Método para manejar intentos fallidos (opcional)
    public void OnPasswordFailed()
    {
        Debug.Log("Intento de contraseña fallido");
        PlaySound(passwordFailSound);
        // Aquí puedes añadir efectos como sonidos o animaciones
        // También podrías implementar un sistema de intentos limitados
    }
    
    private void ShowMissingKeyMessage()
    {
        if (missingKeyMessage != null)
        {
            missingKeyMessage.SetActive(true);
            // Ocultar después de unos segundos
            Invoke("HideMissingKeyMessage", 2.0f);
        }
    }
    
    private void HideMissingKeyMessage()
    {
        if (missingKeyMessage != null)
        {
            missingKeyMessage.SetActive(false);
        }
    }
    
    private void SetChestOpenState()
    {
        if (openedSprite != null)
        {
            GameObject openedChestVisual = new GameObject("OpenedChestVisual");
            openedChestVisual.transform.SetParent(transform);
            openedChestVisual.transform.localPosition = Vector2.zero;

            SpriteRenderer newRenderer = openedChestVisual.AddComponent<SpriteRenderer>();
            newRenderer.sprite = openedSprite;
            newRenderer.sortingLayerName = "afecta"; // Asignar sorting layer deseado
            newRenderer.sortingOrder = chestRenderer.sortingOrder + 1; // Asegurar que esté por encima

            // Ajustar el tamaño del sprite para que coincida con el collider del padre
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider != null && openedSprite != null)
            {
                Vector2 spriteSize = openedSprite.bounds.size;
                float scaleX = collider.size.x / spriteSize.x;
                float scaleY = collider.size.y / spriteSize.y;
                openedChestVisual.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            // Ocultar el renderer original
            if (chestRenderer != null)
            {
                chestRenderer.enabled = false;
            }

            Debug.Log("Cofre " + chestId + " abierto usando un nuevo GameObject");
        }

        hasBeenOpen = true;

        Transform avisoTransform = transform.Find("UI board Small Set");
    
        if (avisoTransform != null)
        {
            // Destruir el objeto hijo "UI board Small Set"
            Destroy(avisoTransform.gameObject);
            Debug.Log("Objeto 'UI board Small Set' destruido.");
        }
        else
        {
            Debug.LogWarning("No se encontró el objeto hijo 'UI board Small Set'.");
        }
    }

    public void OpenChest()
    {
        // Reproducir sonido de apertura
        PlaySound(openChest);
        
        // Marcar el cofre como abierto en el sistema de guardado
        SaveSystem.Instance.OpenChest(chestId, currentLevelId);
        hasBeenOpen = true;
        
        // Reproducir la animación de apertura
        PlayOpenAnimation();
        Transform avisoTransform = transform.Find("UI board Small Set");
    
        if (avisoTransform != null)
        {
            // Destruir el objeto hijo "UI board Small Set"
            Destroy(avisoTransform.gameObject);
            Debug.Log("Objeto 'UI board Small Set' destruido.");
        }
        else
        {
            Debug.LogWarning("No se encontró el objeto hijo 'UI board Small Set'.");
        }
        
        // Ocultar cualquier UI
        OcultarUI();
        chestRenderer.sprite = null;
        chestRenderer.sprite = openedSprite; 
        
        // Generar contenidos
        if (chestContents != null && chestContents.Count > 0)
        {
            // Reproducir sonido de items después de un pequeño retraso
            Invoke("SpawnChestContents", 0.3f);
        }

        Debug.Log("Cofre sprite Abierto y guardado Save System");
    }
    
    private void PlayOpenAnimation()
    {
        // Si tenemos un animator, activar el trigger de apertura
        if (chestAnimator != null && !string.IsNullOrEmpty(openTriggerName))
        {
            chestAnimator.SetTrigger(openTriggerName);
            // La animación debería terminar estableciendo el sprite de cofre abierto
            // o cambiando a un estado "abierto" que use ese sprite
            Debug.Log("Reproduciendo animación de apertura del cofre");
            // Desactivar el animator para evitar que siga ejecutando transiciones
    
        }
        else
        {
            // Si no hay animator, simplemente establecer el sprite final directamente
            SetChestOpenState();
        }
        if (chestAnimator != null)
        {
            chestAnimator.enabled = false;
        }
    }
    
    // Este método puede ser llamado desde la animación como un Animation Event al final de la animación
    // para garantizar que el cofre quede en el estado visual correcto
    public void OnOpenAnimationComplete()
    {
        SetChestOpenState();
    }
    
    private void SpawnChestContents() {
        // Reproducir sonido de obtener items
        PlaySound(givesItems);
        
        foreach (GameObject item in chestContents) {
            // Crear posición aleatoria en un área por encima del cofre
            float randomX = Random.Range(-1.0f, 1.0f);
            float randomY = Random.Range(1.0f, 2.5f);
            Vector2 offset = new Vector2(randomX, randomY);
            
            // Instanciar el item con la posición desplazada
            Instantiate(item, transform.position + (Vector3)offset, Quaternion.identity);
        }
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