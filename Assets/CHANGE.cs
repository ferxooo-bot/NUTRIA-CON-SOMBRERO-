using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioEscena2D : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string nombreEscena; // Nombre exacto de la escena en Build Settings
    [SerializeField] public int nextLevelId;
    [SerializeField] private float delay = 0.3f;
    
    private string nextLevelName; 
    private GameSave currentSave; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Invoke("CargarEscena", delay);
        }
    }

    private void CargarEscena()
    {
        nextLevelName = "Nivel " + nextLevelId.ToString();
        if (!string.IsNullOrEmpty(nombreEscena))
        {
            SaveSystem.Instance.SetCurrentLevel(nextLevelId, nextLevelName); 
            SceneManager.LoadScene(nombreEscena);
        }
        else
        {
            Debug.LogError("Nombre de escena no asignado");
        }
    }
}