using UnityEngine;
using TMPro;

public class LockScript : MonoBehaviour
{
    public string correctCode = "1234"; 
    public TextMeshProUGUI[] digitDisplays;
    
    // Referencia al ChestController
    private ChestController connectedChest;

    private string currentCode; 

    private void Start()
    {
        currentCode = "";
        UpdateDisplay(); // Actualizar la visualización al inicio
        
        // Intentar encontrar el cofre asociado (puede ser el padre o puede asignarse manualmente)
        if (connectedChest == null)
        {
            // Intenta encontrar en el padre
            connectedChest = GetComponentInParent<ChestController>();
            
            if (connectedChest == null)
            {
                // Si no está en el padre, busca en la misma escena (asumiendo que solo hay uno)
                connectedChest = FindObjectOfType<ChestController>();
            }
        }
    }
    
    // Método público para establecer el cofre conectado desde fuera
    public void SetConnectedChest(ChestController chest)
    {
        connectedChest = chest;
    }
    
    public void AddDigit(int digit)
    {
        if(currentCode.Length >= 4) return; 
        currentCode += digit.ToString(); 
        UpdateDisplay(); 
    }

    public void DeleteCode()
    {
        currentCode = "";
        UpdateDisplay();
    }

    public void CheckCode()
    {
        if (currentCode == correctCode)
        {
            Debug.Log("¡Código correcto!");
            
            // Notificar al cofre si está conectado
            if (connectedChest != null)
            {
                connectedChest.CheckPassword(currentCode);
            }
            else
            {
                Debug.LogWarning("No hay un cofre conectado para verificar la contraseña");
            }
        }
        else
        {
            Debug.Log("¡Código Incorrecto!");
            // Aquí puedes añadir efectos de error (sonido, animación, etc.)
            
            // Opcional: también puedes notificar al cofre sobre el intento fallido
            if (connectedChest != null)
            {
                // Si quieres implementar alguna lógica de intentos fallidos en el cofre
                connectedChest.OnPasswordFailed();
            }
        }
        
        // Limpiar el código después del intento
        DeleteCode();
    }
    
    private void UpdateDisplay()
    {
        for (int i = 0; i < digitDisplays.Length; i++)
        {
            if (i < currentCode.Length)
                digitDisplays[i].text = currentCode[i].ToString();
            else
                digitDisplays[i].text = "_";
        }
    }
    
    // Método para cerrar el panel de contraseña (puede conectarse a un botón "Cancelar")
    public void CancelInput()
    {
        DeleteCode();
        
        if (connectedChest != null)
        {
            connectedChest.ClosePasswordCanvas();
        }
    }
}