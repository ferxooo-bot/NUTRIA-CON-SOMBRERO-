using UnityEngine;

public class AnimalMovimiento : MonoBehaviour
{
    public float velocidad = 2f;
    private Vector2 direccion;
    private float tiempoCambio = 2f;
    private float tiempoSiguienteCambio;

    void Start()
    {
        CambiarDireccion();
    }

    void Update()
    {
        transform.Translate(direccion * velocidad * Time.deltaTime);

        if (Time.time >= tiempoSiguienteCambio)
        {
            CambiarDireccion();
        }
    }

    void CambiarDireccion()
    {
        // Dirección aleatoria entre (-1, 0, 1) para x e y
        float dirX = Random.Range(-1, 2); // -1, 0 o 1
        float dirY = Random.Range(-1, 2); // -1, 0 o 1

        direccion = new Vector2(dirX, dirY).normalized;

        tiempoSiguienteCambio = Time.time + tiempoCambio;
    }
}
