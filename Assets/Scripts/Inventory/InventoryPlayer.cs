using UnityEngine;
using System;

public class InventoryPlayer : MonoBehaviour
{
    // Singleton instance
    public static InventoryPlayer Instance { get; private set; }

    // Evento que se dispara cuando cambia el inventario
    public event Action OnInventoryChanged;

    private int keys; 
    private int food; 
    private int coins; 
    private SaveSystem saveSystem;
    public bool mostrarDebug = true;

    private void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre escenas
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveSystem = SaveSystem.Instance;
        LoadInventoryFromSaveSystem();
    }

    // Método auxiliar para notificar cambios en el inventario
    private void NotifyInventoryChanged()
    {
        // Invocar el evento solo si hay suscriptores
        OnInventoryChanged?.Invoke();
    }

    public void AddItem(string item, int amount)
    {
        if(amount < 0)
        {
            return; 
        }
        switch (item.ToLower())
        {
            case "keys":
                keys += amount;
                if (mostrarDebug) Debug.Log($"Se han agregado {amount} llaves al inventario.");
                break;
            case "food":
                food += amount;
                if (mostrarDebug) Debug.Log($"Se han agregado {amount} pociones al inventario.");
                break;
            case "coins":
                coins += amount;
                if (mostrarDebug) Debug.Log($"Se han agregado {amount} monedas al inventario.");
                break;
        }

        saveSystem.AddItem(item, amount);
        
        // Notificar a la UI que el inventario ha cambiado
        NotifyInventoryChanged();
    }

    public bool RemoveItem(string item, int amount)
    {
        if (amount <= 0)
        {
            if (mostrarDebug) Debug.Log("Cantidad inválida. Debe ser mayor que cero.");
            return false;
        }

        bool resultado = false;

        switch (item.ToLower())
        {
            case "keys":
                if (keys >= amount)
                {
                    keys -= amount;
                    if (mostrarDebug) Debug.Log($"Se han removido {amount} llaves del inventario.");
                    resultado = true;
                }
                else
                {
                    if (mostrarDebug) Debug.Log($"No hay suficientes llaves en el inventario ({keys}/{amount}).");
                }
                break;

            case "food":
                if (food >= amount)
                {
                    food -= amount;
                    if (mostrarDebug) Debug.Log($"Se han removido {amount} alimentos del inventario.");
                    resultado = true;
                }
                else
                {
                    if (mostrarDebug) Debug.Log($"No hay suficientes alimentos en el inventario ({food}/{amount}).");
                }
                break;

            case "coins":
                if (coins >= amount)
                {
                    coins -= amount;
                    if (mostrarDebug) Debug.Log($"Se han removido {amount} monedas del inventario.");
                    resultado = true;
                }
                else
                {
                    if (mostrarDebug) Debug.Log($"No hay suficientes monedas en el inventario ({coins}/{amount}).");
                }
                break;

            default:
                if (mostrarDebug) Debug.Log($"Ítem desconocido: {item}");
                break;
        }

        // Si se removió correctamente, actualizar en el sistema de guardado
        if (resultado && saveSystem != null)
        {
            saveSystem.RemoveItem(item, amount);
            
            // Notificar a la UI que el inventario ha cambiado
            NotifyInventoryChanged();
        }
        else if (!resultado && mostrarDebug)
        {
            Debug.Log($"No se pudo remover {amount} de {item}. Inventario insuficiente.");
        }
        
        return resultado;
    }

    public void LoadInventoryFromSaveSystem() 
    {
        if (saveSystem != null)
        {
            // Obtener los valores actuales del sistema de guardado
            keys = saveSystem.GetItemCount("keys");
            food = saveSystem.GetItemCount("food");
            coins = saveSystem.GetItemCount("coins");
            
            if (mostrarDebug)
            {
                Debug.Log($"Inventario cargado: {keys} llaves, {food} alimentos, {coins} monedas");
            }
            
            // Notificar a la UI que el inventario ha sido cargado
            NotifyInventoryChanged();
        }
        else
        {
            keys = 0; 
            food = 0;
            coins = 0; 
            Debug.LogError("SaveSystem no está disponible. Todo inicializado en = 0.");
            
            // Notificar a la UI incluso con valores por defecto
            NotifyInventoryChanged();
        }
    }
    
    // Métodos para obtener cantidades (útiles para UI y referencias externas)
    public int GetKeyCount()
    {
        return keys;
    }
    
    public int GetFoodCount()
    {
        return food;
    }
    
    public int GetCoinCount()
    {
        return coins;
    }
}