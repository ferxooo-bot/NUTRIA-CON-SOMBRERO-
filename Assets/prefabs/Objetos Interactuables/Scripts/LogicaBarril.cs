using UnityEngine;

public class LogicaBarril : MonoBehaviour
{

    private bool canToggle = false;
    private bool isHidden = false;
    private Vector3 originalPosition;
    private int originalLayer;


    private int groundLayer;
    private int afectaLayer;


    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // índices de capa
        groundLayer = LayerMask.NameToLayer("Ground");
        afectaLayer = LayerMask.NameToLayer("afecta");
        originalLayer = gameObject.layer;

        // componentes del jugador
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError("Player necesita un SpriteRenderer.");

        // verificar capas
        if (groundLayer == -1 || afectaLayer == -1)
            Debug.LogError("Asegúrate de haber creado las capas 'Ground' y 'afecta'.");
    }

    void Update()
    {

        if (canToggle && Input.GetKeyDown(KeyCode.E))
        {
            if (!isHidden)
                HidePlayer();
            else
                ShowPlayer();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // entrar al trigger del barril
        if (other.CompareTag("Barril"))
            canToggle = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // salir del trigger del barril
        if (other.CompareTag("Barril"))
            canToggle = false;
    }

    private void HidePlayer()
    {
        // Guardar capa y posicion
        originalPosition = transform.position;
        originalLayer = gameObject.layer;


        gameObject.layer = groundLayer;
        spriteRenderer.enabled = false;

        isHidden = true;
    }

    private void ShowPlayer()
    {
        // volver a la capa y posicion inicial
        transform.position = originalPosition;
        gameObject.layer = originalLayer;
        spriteRenderer.enabled = true;

        isHidden = false;
    }
}
