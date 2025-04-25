using UnityEngine;
using UnityEngine.InputSystem;

public class MovementSwin : MonoBehaviour
{   
    public StateHandler stateHandler;
    [SerializeField] private Transform headPivot; //pivote

    [Header("Configuración de Movimiento en Agua")]
    [SerializeField] private float baseWaterVelocity = 14f;  
    private float waterVelocity;

    // Declaramos variables locales para simular las propiedades del agua
    [SerializeField] private float waterDensity = 1.0f;              // Densidad del agua
    [SerializeField] private float waterDrag = 0.9f;                 // Resistencia del agua
    [SerializeField] private float waterBuoyancy = -1f;             // Fuerza de flotabilidad
    [SerializeField] private float rotationSpeed = 10f;             // Velocidad de rotación (ajústala en el inspector)
    private float playerMass;  // Masa del jugador

    private bool isInWater = false;
    private Rigidbody2D rb;
    private Vector2 direction;
    private Controls controls;
    private bool isRunning; 
    public bool lookRight = true;  

    public GameObject BonesHeadBody;  
    public Animator animatorBonesHeadBody; 

    void Awake()
    {       
        animatorBonesHeadBody = transform.GetChild(1).GetComponent<Animator>(); 
        rb = GetComponent<Rigidbody2D>();
        controls = new Controls();
        waterVelocity = baseWaterVelocity; // Inicializar con el valor base
        playerMass = rb.mass; 
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Movement.Run.performed += _ => isRunning = true;  
        controls.Movement.Run.canceled += _ => isRunning = false;  
        rb.gravityScale = 0; // Desactivar
        isInWater = true;
        animatorBonesHeadBody.SetBool("isInWater", isInWater); 
    }

    private void OnDisable() 
    {
        controls.Disable(); 
        controls.Movement.Run.performed -= _ => isRunning = true;  
        controls.Movement.Run.canceled -= _ => isRunning = false;
        rb.gravityScale = 1; //activar
        isInWater = false;
        animatorBonesHeadBody.SetBool("isInWater", isInWater); 
    }

    void Update()
    {
        if(isInWater)
        {
            direction = controls.Movement.Move.ReadValue<Vector2>();

            // Actualizamos la rotación del personaje según la dirección
            HandleRotation();
            
            // Actualizamos el flip del sprite según la dirección horizontal
            HandleSpriteFlip();
        }

        animatorBonesHeadBody.SetBool("isInWater", isInWater); 
        animatorBonesHeadBody.SetFloat("Speed", direction.magnitude);
    }

    void FixedUpdate()
    {   
        if (isRunning)
        {
            waterVelocity = baseWaterVelocity * 2f; // Ajustar velocidad si está corriendo
        }
        else
        {
            waterVelocity = baseWaterVelocity; // Restablecer velocidad al valor base
        }

        if (isInWater)
        {
            MoveInWater();
        }
    }

    // Método actualizado para rotación directa
    private void HandleRotation()
    {
        if(direction.magnitude > 0.1f) // Si hay suficiente movimiento
        {
            // Calculamos el ángulo basado en la dirección
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            
            // Rotamos directamente hacia la dirección de movimiento
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            
            // Aplicamos la rotación con la velocidad configurada
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            // Si no hay movimiento, volvemos a la rotación normal gradualmente
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, Time.deltaTime * rotationSpeed);
        }
    }

    // Método simplificado para el flip del sprite
    private void HandleSpriteFlip()
    {
        if (direction.x > 0.1f && !lookRight)
        {
            lookRight = true;
            FlipSprite(true);
        }
        else if (direction.x < -0.1f && lookRight)
        {
            lookRight = false;
            FlipSprite(false);
        }
    }

    private void FlipSprite(bool faceRight)
    {
        Vector3 newScale = BonesHeadBody.transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * (faceRight ? 1 : -1);
        BonesHeadBody.transform.localScale = newScale;
    }

    void MoveInWater()
    {
        float maxWaterSpeed = waterVelocity;

        Vector2 currentVelocity = rb.linearVelocity;  // Velocidad actual
        
        // Aplicamos resistencia del agua (desaceleración gradual)
        currentVelocity *= waterDrag;
        
        // Calculamos fuerza de impulso basada en los controles (direction)
        Vector2 moveForce = direction * waterVelocity;
        
        // Simulamos flotabilidad (empuje hacia arriba)
        Vector2 buoyancyForce = new Vector2(0, waterBuoyancy * playerMass * waterDensity);
        
        // Combinamos todas las fuerzas
        Vector2 totalForce = moveForce + buoyancyForce;
        
        // Aplicamos la fuerza a la velocidad actual
        currentVelocity += totalForce * Time.deltaTime;
        
        // Limitamos la velocidad máxima
        currentVelocity = Vector2.ClampMagnitude(currentVelocity, maxWaterSpeed);
        
        // Aplicamos la velocidad resultante
        rb.linearVelocity = currentVelocity;
    }
}