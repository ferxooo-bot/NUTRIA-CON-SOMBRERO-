using UnityEngine;

public class Llave : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerInventory inventario = collision.GetComponent<PlayerInventory>();
            if (inventario != null)
            {
                inventario.RecogerLlave();
                Destroy(gameObject);
            }
        }
    }
}
