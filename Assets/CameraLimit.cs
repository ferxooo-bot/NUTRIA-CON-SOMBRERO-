using UnityEngine;

public class CameraLimit : MonoBehaviour
{
    public Transform player; // Asigna el jugador en el Inspector
    public float minY; // Altura mínima que la cámara puede alcanzar

    void Update()
    {
        if (player != null)
        {
            Vector3 newPosition = transform.position;
            newPosition.y = Mathf.Max(player.position.y, minY); // Limita la altura
            transform.position = newPosition;
        }
    }
}