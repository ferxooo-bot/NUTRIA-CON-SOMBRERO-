using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema simplificado para gestionar la carga de cinemáticas y el retorno al nivel del juego.
/// Compatible con el CinematicController existente que maneja las diapositivas y transiciones.
/// </summary>
public class CinematicSystem : MonoBehaviour
{
    #region Singleton
    // Instancia única accesible desde cualquier parte
    public static CinematicSystem Instance { get; private set; }
    
    // Clave para guardar el nivel al que regresar
    private const string RETURN_LEVEL_KEY = "LevelToReturnAfterCinematic";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    /// <summary>
    /// Carga una escena de cinemática y guarda el nivel actual para volver a él cuando termine.
    /// </summary>
    /// <param name="cinematicSceneName">Nombre de la escena de cinemática a cargar</param>
    public void PlayCinematic(string cinematicSceneName)
    {
        // Guardar el nombre de la escena actual para volver después
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(RETURN_LEVEL_KEY, currentScene);
        PlayerPrefs.Save();

        // Cargar la escena de cinemática
        SceneManager.LoadScene(cinematicSceneName);
    }

    /// <summary>
    /// Este método debe llamarse desde la escena de cinemática 
    /// cuando se carga por primera vez, para configurar el nivel de retorno.
    /// </summary>
    public void SetupCinematicScene()
    {
        // Buscar el CinematicController en la escena
        CinematicController cinematicController = FindObjectOfType<CinematicController>();
        
        if (cinematicController != null)
        {
            // Obtener el nivel al que se debe volver
            string returnLevel = PlayerPrefs.GetString(RETURN_LEVEL_KEY, cinematicController.defaultNextSceneName);
            
            // Configurar el nivel de retorno en el controlador
            // Usando reflexión para acceder al campo nextSceneName (ya que es privado)
            System.Reflection.FieldInfo field = typeof(CinematicController).GetField("nextSceneName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(cinematicController, returnLevel);
            }
            else
            {
                // Alternativa: tratar de usar un método público si existe
                // O modificar el CinematicController para exponer una propiedad pública
                Debug.LogWarning("No se pudo acceder al campo nextSceneName. Asegúrate de que el CinematicController sea compatible.");
            }
            
            // Suscribirse al evento de finalización
            cinematicController.OnCinematicCompleted += OnCinematicFinished;
        }
        else
        {
            Debug.LogError("No se encontró un CinematicController en la escena.");
        }
    }
    
    /// <summary>
    /// Método llamado cuando termina la cinemática.
    /// </summary>
    private void OnCinematicFinished()
    {
        // No es necesario hacer nada especial aquí porque
        // el CinematicController ya se encarga de cargar la siguiente escena
        Debug.Log("Cinemática completada, volviendo al nivel: " + PlayerPrefs.GetString(RETURN_LEVEL_KEY));
        
        // Desuscribirse del evento para evitar referencias persistentes
        CinematicController cinematicController = FindObjectOfType<CinematicController>();
        if (cinematicController != null)
        {
            cinematicController.OnCinematicCompleted -= OnCinematicFinished;
        }
    }
}