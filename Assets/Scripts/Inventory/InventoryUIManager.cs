using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    [Header("UI Referencias")]
    [SerializeField] private GameObject inventoryPanel;
    
    [Header("Contadores de Items")]
    [SerializeField] private TextMeshProUGUI keysCountText;
    [SerializeField] private TextMeshProUGUI foodCountText;
    [SerializeField] private TextMeshProUGUI coinsCountText;
    
    [Header("Configuración")]
    [SerializeField] private bool actualizarEnCadaFrame = false;
    [SerializeField] private bool mostrarUIAlIniciar = true;
    
    [SerializeField] private InventoryPlayer inventoryPlayer;
    
    private void Start()
    {
        // Esperar a que InventoryPlayer esté disponible
        inventoryPlayer = InventoryPlayer.Instance;
        
        if (inventoryPlayer == null)
        {
            Debug.LogError("No se encontró InventoryPlayer. Asegúrate de que exista en la escena.");
            return;
        }
        
        // Suscribirse al evento OnInventoryChanged
        inventoryPlayer.OnInventoryChanged += UpdateAllUI;
        
        // Mostrar/ocultar panel según configuración
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(mostrarUIAlIniciar);
        }
        
        // Actualizar UI con los valores iniciales
        UpdateAllUI();
    }
    
    private void OnDestroy()
    {
        // Desuscribirse del evento cuando se destruya este objeto
        if (inventoryPlayer != null)
        {
            inventoryPlayer.OnInventoryChanged -= UpdateAllUI;
        }
    }
    
    private void Update()
    {
        // Solo actualizar en cada frame si está configurado
        if (actualizarEnCadaFrame)
        {
            UpdateAllUI();
        }
    }
    
    // Método público para actualizar la UI desde fuera (se puede llamar después de añadir/quitar ítems)
    public void UpdateAllUI()
    {
        if (inventoryPlayer == null) return;
        
        // Actualizar todos los contadores
        if (keysCountText != null)
        {
            keysCountText.text = inventoryPlayer.GetKeyCount().ToString();
        }
        
        if (foodCountText != null)
        {
            foodCountText.text = inventoryPlayer.GetFoodCount().ToString();
        }
        
        if (coinsCountText != null)
        {
            coinsCountText.text = inventoryPlayer.GetCoinCount().ToString();
        }
    }
    
    // Método para mostrar/ocultar el panel de inventario (para botones de UI)
    public void ToggleInventoryPanel()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }
}