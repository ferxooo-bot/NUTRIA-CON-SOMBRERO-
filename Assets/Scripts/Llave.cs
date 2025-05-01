using UnityEngine;

public class Llave : MonoBehaviour
{
    public string nombreLlave = "Llave Dorada";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        FatherMoment1 jugador = collision.GetComponent<FatherMoment1>();
        if (jugador != null)
        {
            jugador.AgarrarObjeto(nombreLlave);
            Destroy(gameObject); // Destruye la llave después de recogerla
        }
    }
}