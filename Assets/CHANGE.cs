using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioEscenaTrigger : MonoBehaviour
{
    [SerializeField] private string nombreEscena = "NombreEscena"; // Asigna en Inspector
    [SerializeField] private float delay = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OBJETO QUE ENTRA: " + other.name + " - TAG: " + other.tag); // Verifica en Consola
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡TRIGGER ACTIVADO!");
            Invoke("CambiarEscena", delay);
        }
    }

    private void CambiarEscena()
    {
        if (string.IsNullOrEmpty(nombreEscena))
        {
            Debug.LogError("Asigna el nombre de la escena en el Inspector");
            return;
        }
        
        Debug.Log("Cargando escena: " + nombreEscena);
        SceneManager.LoadScene(nombreEscena);
    }

    // Dibuja el collider en el Editor para ver posición real
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (GetComponent<BoxCollider>())
        {
            Gizmos.DrawWireCube(transform.position + GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
        }
    }
}