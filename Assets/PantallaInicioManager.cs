using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void IrANuevoJuego()
    {
        SceneManager.LoadScene("P Nuevo Juego");
    }

    public void IrAReanudarJuego()
    {
        SceneManager.LoadScene("P Reanudar Juego");
    }

    public void IrAOpciones()
    {
        SceneManager.LoadScene("P Opciones");
    }

    public void IrATienda()
    {
        SceneManager.LoadScene("P Tienda");
    }

    public void VolverAMenuPrincipal()
    {
        SceneManager.LoadScene("Pantalla Inicial");
    }
}
