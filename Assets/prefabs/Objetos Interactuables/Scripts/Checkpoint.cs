using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Del 1 al 4")]
    public int numeroCheckpoint = 1;
    public Sprite spriteActivado;

    private SpriteRenderer spriteRenderer;
    private bool playerCerca = false;
    private bool yaActivado = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Si el jugador está cerca, aún no hemos activado este checkpoint
        // y pulsa la tecla E → lo activamos
        if (playerCerca && !yaActivado && Input.GetKeyDown(KeyCode.E))
        {
            ActivarCheckpoint();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detectamos si el que entra es el jugador
        if (other.CompareTag("Player"))
        {
            playerCerca = true;
            // Aquí podrías mostrar un UI: "Pulsa E para activar"
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCerca = false;
            // Ocultar UI si lo mostrabas
        }
    }

    private void ActivarCheckpoint()
    {
        yaActivado = true;

        // Cambiar el sprite
        if (spriteActivado != null)
            spriteRenderer.sprite = spriteActivado;

        // Guardar datos en el ControladorCheckpoint
        ControladorCheckpoint.Instance.GuardarCheckpoint(numeroCheckpoint);
    }
}
