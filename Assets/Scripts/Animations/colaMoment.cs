using UnityEngine;

public class ColaMovimiento : MonoBehaviour
{
    public float maxAmplitude = 0.25f;     // Amplitud máxima hacia arriba
    public float minAmplitude = 0.1f;      // Amplitud mínima
    public float mainSpeed = 14.6f;        // Velocidad base del movimiento principal
    public float amplitudeSpeed = 0.5f;    // Velocidad con que cambia la amplitud
    private Vector3 startPosition;
    public float reboteIntensidad = 0.1f;  // Intensidad del rebote al golpear
    
    // Nueva variable para controlar el límite inferior
    public float limiteInferior = -0.32f;  // Cuánto puede bajar la cola por debajo de la posición inicial
    
    // Variables para controlar la variación aleatoria de velocidad
    public float velocityVariation = 2.0f;  // Variación máxima de velocidad
    public float velocityChangeSpeed = 0.5f; // Qué tan rápido cambia la velocidad
    
    private float currentMainSpeed;        // Velocidad actual
    private float targetMainSpeed;         // Velocidad objetivo a la que se va a llegar gradualmente
    private float velocityChangeTimer;     // Temporizador para el cambio de velocidad

    void Start()
    {
        startPosition = transform.position; // Guarda la posición inicial
        currentMainSpeed = mainSpeed;       // Inicializa la velocidad actual
        targetMainSpeed = mainSpeed;        // Inicializa la velocidad objetivo
        velocityChangeTimer = 0f;           // Inicializa el temporizador
    }

    void Update()
    {
        // Actualiza el temporizador
        velocityChangeTimer -= Time.deltaTime;
        
        // Si el temporizador llega a cero, elige una nueva velocidad objetivo
        if (velocityChangeTimer <= 0)
        {
            // Elige una nueva velocidad objetivo dentro de un rango razonable
            targetMainSpeed = mainSpeed + Random.Range(-velocityVariation, velocityVariation);
            
            // Reajusta el temporizador con un tiempo aleatorio entre 1 y 3 segundos
            velocityChangeTimer = Random.Range(1.0f, 3.0f);
        }
        
        // Interpola suavemente hacia la velocidad objetivo
        currentMainSpeed = Mathf.Lerp(currentMainSpeed, targetMainSpeed, Time.deltaTime * velocityChangeSpeed);
        
        // Calcula una amplitud que oscila entre minAmplitude y maxAmplitude
        float currentAmplitude = minAmplitude + (maxAmplitude - minAmplitude) * 
                                (0.5f + 0.5f * Mathf.Sin(Time.time * amplitudeSpeed));
        
        // Movimiento senoidal básico usando la velocidad actual
        float seno = Mathf.Sin(Time.time * currentMainSpeed);
        
        float movement;
        
        if (seno >= 0) {
            // En la fase positiva, la cola se mueve hacia arriba con la amplitud normal
            movement = seno * currentAmplitude;
        } else {
            // En la fase negativa, permitimos movimiento hacia abajo hasta el límite inferior
            // Escalamos el valor negativo del seno para que llegue hasta limiteInferior
            movement = seno * Mathf.Abs(limiteInferior) * reboteIntensidad;
        }
        
        // Aplicamos el movimiento en el eje Y
        transform.position = startPosition + new Vector3(0, movement, 0);
    }
}
