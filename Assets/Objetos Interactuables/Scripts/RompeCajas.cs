using UnityEngine;

public class RompeCajas : MonoBehaviour
{
    private BreakingBox cajaCercana = null;

    private void Update()
    {
        if (cajaCercana != null && Input.GetKeyDown(KeyCode.E))
        {
            cajaCercana.Romper();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BreakingBox caja = collision.GetComponent<BreakingBox>();
        if (caja != null)
        {
            cajaCercana = caja;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        BreakingBox caja = collision.GetComponent<BreakingBox>();
        if (caja != null && caja == cajaCercana)
        {
            cajaCercana = null;
        }
    }
}
