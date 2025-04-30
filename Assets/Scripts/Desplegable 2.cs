using UnityEngine;

public class Desplegable2 : MonoBehaviour
{
    [Header("Sprite")]
    public SpriteRenderer spriteRenderer;  // Cambiamos de Image a SpriteRenderer
    public Sprite spriteOriginal;
    public Sprite spriteNuevo;

    [Header("Objeto a mover")]
    public Transform objetoSpriteRenderer;   // El GameObject del sprite
   
    public float distanciaMovimiento;

    [Header("Botón Y")]
    public GameObject botonY;

    private bool estadoActivo = false;
    private Vector3 posicionInicialSprite;    // Posición inicial del SpriteRenderer
    private Vector3 posicionInicialObjeto;    // Posición inicial del otro objeto

    

    private void Start()
    {
        posicionInicialSprite = objetoSpriteRenderer.position;
       
        botonY.SetActive(false);
    }

    public void AlPresionarBotonX()
    {
        if (!estadoActivo)
        {
            spriteRenderer.sprite = spriteNuevo;
            objetoSpriteRenderer.position = posicionInicialSprite + Vector3.down * (distanciaMovimiento * 0.5f);
            
            botonY.SetActive(true);
            estadoActivo = true;
        }
        else
        {
            spriteRenderer.sprite = spriteOriginal;
            objetoSpriteRenderer.position = posicionInicialSprite;
           
            botonY.SetActive(false);
            estadoActivo = false;
        }
    }
}
