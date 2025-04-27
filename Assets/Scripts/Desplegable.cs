using UnityEngine;

public class Desplegable : MonoBehaviour
{
    [Header("Sprite")]
    public SpriteRenderer spriteRenderer;  // Cambiamos de Image a SpriteRenderer
    public Sprite spriteOriginal;
    public Sprite spriteNuevo;

    [Header("Objeto a mover")]
    public Transform objetoSpriteRenderer;   // El GameObject del sprite
    public Transform objetoAMover;
    public Transform objetoAMover2;
    public Transform objetoAMover3;
    public float distanciaMovimiento;

    [Header("Botón Y")]
    public GameObject botonY;

    private bool estadoActivo = false;
    private Vector3 posicionInicialSprite;    // Posición inicial del SpriteRenderer
    private Vector3 posicionInicialObjeto;    // Posición inicial del otro objeto

    private Vector3 posicionInicial;
    private Vector3 posicionInicial2;
    private Vector3 posicionInicial3;

    private void Start()
    {
        posicionInicialSprite = objetoSpriteRenderer.position;
        posicionInicialObjeto = objetoAMover.position;

        posicionInicial = objetoAMover.position;
        posicionInicial2 = objetoAMover2.position;
        posicionInicial3 = objetoAMover3.position;
        botonY.SetActive(false);
    }

    public void AlPresionarBotonX()
    {
        if (!estadoActivo)
        {
            spriteRenderer.sprite = spriteNuevo;        
            objetoSpriteRenderer.position = posicionInicialSprite + Vector3.down * (distanciaMovimiento*0.5f);
            objetoAMover.position = posicionInicial + Vector3.down * distanciaMovimiento;
            objetoAMover2.position = posicionInicial2 + Vector3.down * distanciaMovimiento;
            objetoAMover3.position = posicionInicial3 + Vector3.down * distanciaMovimiento;
            botonY.SetActive(true);
            estadoActivo = true;
        }
        else
        {
            spriteRenderer.sprite = spriteOriginal;
            objetoSpriteRenderer.position = posicionInicialSprite;
            objetoAMover.position = posicionInicial;
            objetoAMover2.position = posicionInicial2;
            objetoAMover3.position = posicionInicial3;
            botonY.SetActive(false);
            estadoActivo = false;
        }
    }
}
