using UnityEngine;

public class HurtPlayer : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private Vector2 knockbackDirection = new Vector2(5f, 2f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            FatherMoment1 player = other.GetComponent<FatherMoment1>();
            if(player != null)
            {
                // Calcular dirección del knockback basada en la posición
                float direction = Mathf.Sign(other.transform.position.x - transform.position.x);
                Vector2 finalKnockback = new Vector2(
                    knockbackDirection.x * direction,
                    knockbackDirection.y
                ) * knockbackForce;

                player.TakeDamage(damage, finalKnockback);
            }
        }
    }
}