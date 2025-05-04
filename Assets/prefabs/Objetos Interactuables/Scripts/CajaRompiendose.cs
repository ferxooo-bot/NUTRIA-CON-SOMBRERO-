using UnityEngine;
using System.Collections;

public class CajaRompiendose : MonoBehaviour
{
    public Sprite spriteBase;
    public Sprite spriteTransicion;
    public Sprite spriteRoto;

    public GameObject particulasRupturaPrefab;
    public AudioClip sonidoRuptura;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool yaRota = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer.sprite = spriteBase;
    }

    public void Romper()
    {
        if (yaRota) return;
        StartCoroutine(ProcesoRomper());
    }

    private IEnumerator ProcesoRomper()
    {
        yaRota = true;

        spriteRenderer.sprite = spriteTransicion;
        yield return new WaitForSeconds(0.3f);

        spriteRenderer.sprite = spriteRoto;

        if (particulasRupturaPrefab != null)
            Instantiate(particulasRupturaPrefab, transform.position, Quaternion.identity);

        if (sonidoRuptura != null && audioSource != null)
            audioSource.PlayOneShot(sonidoRuptura);
    }
}
