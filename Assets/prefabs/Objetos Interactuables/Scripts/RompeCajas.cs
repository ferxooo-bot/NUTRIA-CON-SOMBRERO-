using UnityEngine;

public class RompedorDeCajas : MonoBehaviour
{
    private CajaRompiendose cajaCercana = null;

    private void Update()
    {
        if (cajaCercana != null && Input.GetKeyDown(KeyCode.E))
        {
            cajaCercana.Romper();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CajaRompiendose caja = collision.GetComponent<CajaRompiendose>();
        if (caja != null)
        {
            cajaCercana = caja;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        CajaRompiendose caja = collision.GetComponent<CajaRompiendose>();
        if (caja != null && caja == cajaCercana)
        {
            cajaCercana = null;
        }
    }
}
