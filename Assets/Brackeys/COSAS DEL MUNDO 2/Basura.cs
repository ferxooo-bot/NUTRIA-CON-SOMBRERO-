using UnityEngine;

public class Basura : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Basura recogida!");

            PlayerInventory inventario = collision.GetComponent<PlayerInventory>();
            if (inventario != null)
            {
                inventario.RecogerBasura(); // ← Aquí ya está correcto
                Destroy(gameObject); // Destruye la basura recolectada
            }
        }
    }
}
