using UnityEngine;

public class DescenderPlataforma : MonoBehaviour
{
    [SerializeField] private float tiempoDesactivacion = 0.3f;

    private PlatformEffector2D effector;

    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
    }

    void Update()
    {
        // Si el jugador está sobre la plataforma y presiona S
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(DesactivarPlataforma());
        }
    }

    System.Collections.IEnumerator DesactivarPlataforma()
    {
        effector.rotationalOffset = 180; // Permitir caer
        yield return new WaitForSeconds(tiempoDesactivacion);
        effector.rotationalOffset = 0; // Restaurar colisión
    }
}