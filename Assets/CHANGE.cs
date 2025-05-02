using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioEscena2D : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string nombreEscena; // Nombre exacto de la escena en Build Settings
    [SerializeField] private float delay = 0.3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Invoke("CargarEscena", delay);
        }
    }

    private void CargarEscena()
    {
        if (!string.IsNullOrEmpty(nombreEscena))
        {
            SceneManager.LoadScene(nombreEscena);
        }
        else
        {
            Debug.LogError("Nombre de escena no asignado");
        }
    }
}