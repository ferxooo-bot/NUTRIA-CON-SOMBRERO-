using UnityEngine;

public class PickableItem : MonoBehaviour
{
    public enum ItemType
    {
        Pescado,
        BotellaPlastico,
        BotellaVidrio,
        Lata
    }

    public ItemType tipo;

    public Sprite spriteDesaparicion;
    public AudioClip sonidoRecogida;
    public float tiempoDesaparicion = 0.5f;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool recogido = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (recogido) return;
        if (other.CompareTag("Player"))
        {
            recogido = true;

            // Cambiar sprite
            if (spriteDesaparicion != null)
                spriteRenderer.sprite = spriteDesaparicion;

            // Reproducir sonido
            if (sonidoRecogida != null)
                audioSource.PlayOneShot(sonidoRecogida);

            // Contabilizar según el tipo
            switch (tipo)
            {
                case ItemType.Pescado:
                    ContadorObjetos.Instance.AumentarPescado();
                    break;
                case ItemType.BotellaPlastico:
                    ContadorObjetos.Instance.AumentarBotellaPlastico();
                    break;
                case ItemType.BotellaVidrio:
                    ContadorObjetos.Instance.AumentarBotellaVidrio();
                    break;
                case ItemType.Lata:
                    ContadorObjetos.Instance.AumentarLata();
                    break;
            }

            // Destruir objeto después del tiempo definido
            Destroy(gameObject, tiempoDesaparicion);
        }
    }
}
