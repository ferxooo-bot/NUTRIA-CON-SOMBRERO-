using UnityEngine;

public class MovimientoAnimal : MonoBehaviour
{
    public float velocidad = 2f;
    private bool puedeMoverse = false;

    public void ActivarMovimiento()
    {
        puedeMoverse = true;
    }

    void Update()
    {
        if (puedeMoverse)
        {
            transform.Translate(Vector2.right * velocidad * Time.deltaTime);
        }
    }
}
