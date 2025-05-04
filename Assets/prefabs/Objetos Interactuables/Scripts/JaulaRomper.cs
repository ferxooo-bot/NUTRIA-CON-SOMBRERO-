using UnityEngine;
using System.Collections;

public class JaulaRomper : MonoBehaviour
{
    [Header("Sprites de la jaula")]
    public Sprite spriteBase;
    public Sprite spriteTransicion;
    public Sprite spriteRota;

    [Header("Efectos")]
    public GameObject particulasRupturaPrefab;
    public AudioClip sonidoRuptura;
    public float tiempoAntesDeDestruir = 2.0f;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool yaRota = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (spriteRenderer != null)
            spriteRenderer.sprite = spriteBase;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (yaRota) return;

        if (collision.collider.CompareTag("Ground"))
        {
            StartCoroutine(RomperJaula());
        }
    }

    private IEnumerator RomperJaula()
    {
        yaRota = true;

        // Imagen de transición
        if (spriteRenderer != null && spriteTransicion != null)
            spriteRenderer.sprite = spriteTransicion;

        // Pequeña pausa visual
        yield return new WaitForSeconds(0.3f);

        // Imagen rota final
        if (spriteRenderer != null && spriteRota != null)
            spriteRenderer.sprite = spriteRota;

        // Partículas
        if (particulasRupturaPrefab != null)
            Instantiate(particulasRupturaPrefab, transform.position, Quaternion.identity);

        // Sonido
        if (sonidoRuptura != null && audioSource != null)
            audioSource.PlayOneShot(sonidoRuptura);

        // Destruir después de cierto tiempo
        Destroy(gameObject, tiempoAntesDeDestruir);
    }
}
