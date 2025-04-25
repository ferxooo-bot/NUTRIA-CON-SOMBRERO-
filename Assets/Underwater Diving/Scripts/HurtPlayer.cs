using UnityEngine;

public class HurtPlayer : MonoBehaviour 
{
    // Elimina la búsqueda en Start() y hazlo directamente en OnTriggerEnter2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Usa CompareTag en lugar de .tag (más eficiente)
        {
            FatherMoment1 player = other.GetComponent<FatherMoment1>(); // Obtén el componente del jugador colisionado
            if (player != null)
            {
                player.Hurt(); // Llama al método hurt() solo si el jugador existe
            }
            else
            {
                Debug.LogWarning("El jugador no tiene el componente FatherMoment1.");
            }
        }
    }
}