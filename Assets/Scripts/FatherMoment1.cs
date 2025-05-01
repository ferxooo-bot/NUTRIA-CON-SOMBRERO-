using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using System.Collections;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class FatherMoment1 : MonoBehaviour
{
   [Header("Input")]
    public Controls controls; // Se crea la variable para controlar el Input Action llamado Controls
    public Vector2 direction; // Almacena la dirección del movimiento
    public bool puedeMoverse = true;

    [Header("Componentes")]
    public Rigidbody2D rb2D;
    private BoxCollider2D boxCollider;
    public GameObject bones;  // GameObject para la animación de caminar (huesos)
    public GameObject sprite; // GameObject para la animación de correr y saltar (sprite)
    public Animator animatorSprite;
    public Animator animatorBones;
    private SpriteRenderer playerSprite;

    [Header("Movimiento")]
    public float movementVelocityWalk = 5f;
    public float movementVelocityRun = 7f;
    public float jumpForce = 4f;
    public bool isRunning;
    public bool isJumping;
    public bool rushing; // Variable adicional relacionada con el movimiento rápido

    [Header("Detección de Suelo")]
    public LayerMask whatIsGround;
    public bool inGround;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public float groundCheckWidth = 0.8f;
    private bool wasGrounded;

    [Header("Salud y Daño")]
    public int maxHealth = 3;
    private int currentHealth;
    public float invulnerabilityTime = 1.5f;
    public float blinkInterval = 0.15f;
    private bool isInvulnerable = false;

    [Header("Sistema de Corazones (UI)")]
    [SerializeField] private SpriteRenderer[] heartFulls = new SpriteRenderer[3];  // Corazones llenos
    [SerializeField] private SpriteRenderer[] heartEmptys = new SpriteRenderer[3]; // Corazones vacíos
    [SerializeField] private float flashIntensity = 3f; // Intensidad del brillo al parpadear

    [Header("Configuración del Collider")]
    [SerializeField] private Vector2 normalColliderSize = new Vector2(0.27f, 0.82f);
    [SerializeField] private Vector2 normalColliderSizeOffSet = new Vector2(0.27f, 0.82f);
    [SerializeField] private Vector2 runningColliderSize = new Vector2(0.92f, 0.20f);
    [SerializeField] private Vector2 runningColliderOffset = new Vector2(0f, 0.5f);
    [SerializeField] private Vector2 jumpingColliderSize = new Vector2(0.42f, 0.38f);
    [SerializeField] private Vector2 jumpingColliderOffset = new Vector2(0f, 0.6f);

    [Header("Configuración de Muerte y Reaparición")]
    [SerializeField] private Transform respawnPoint; // Arrastra el punto de reinicio
    [SerializeField] private float respawnDelay = 1.0f; // Tiempo antes de reiniciar
    private bool isDead = false;

    [Header("Estados y Animación")]
    public StateHandler stateHandler;
    public bool lookRight = true;

    private void Awake() 
    {
        ResetHearts();
        stateHandler = GetComponent<StateHandler>(); 
        controls = new();  
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        animatorSprite = transform.GetChild(0).GetComponent<Animator>();       
        animatorBones = transform.GetChild(1).GetComponent<Animator>(); 
        currentHealth = maxHealth;
        
        
       Debug.Log($"Fulls asignados: {heartFulls != null}, Emptys asignados: {heartEmptys != null}");
    Debug.Log($"Tamaño Fulls: {heartFulls?.Length}, Tamaño Emptys: {heartEmptys?.Length}");
    
    }
    private void Start()
    {
        Physics2D.autoSyncTransforms = true; 
        Physics2D.defaultContactOffset = 0.01f;
        
        

        boxCollider = GetComponent<BoxCollider2D>();
        // Si no se ha asignado groundCheck en el inspector, crear uno
        if (groundCheck == null)
        {
            GameObject checkObject = new GameObject("GroundCheck");
            checkObject.transform.parent = this.transform;
            checkObject.transform.localPosition = new Vector3(0, -0.5f, 0); // Ajusta esta posición según tu personaje
            groundCheck = checkObject.transform;
        }
        
        ResetHearts();
    }
    private void OnEnable()
    {
        controls.Enable(); 
        controls.Movement.Jump.started += _ => Jump(); 
        controls.Movement.Run.performed += _ => isRunning = true;  // Shift presionado
        controls.Movement.Run.canceled += _ => isRunning = false;  // Shift soltado
        transform.rotation = Quaternion.identity;
    }
    private void OnDisable()
    {
        controls.Disable(); 
        controls.Movement.Jump.started -= _ => Jump(); 
        controls.Movement.Run.performed -= _ => isRunning = true;  
        controls.Movement.Run.canceled -= _ => isRunning = false;
    }

    // ----------- update ------------
    private void Update(){
        
        if(isJumping){
            boxCollider.size = jumpingColliderSize;
            boxCollider.offset = jumpingColliderOffset;
            Physics2D.SyncTransforms();
        }
        else if(isRunning && (direction.x != 0)){
            boxCollider.size = runningColliderSize;
            boxCollider.offset = runningColliderOffset;
            Physics2D.SyncTransforms();
        } else if(direction.x != 0){
            boxCollider.size = normalColliderSize;
            boxCollider.offset = normalColliderSizeOffSet;
            Physics2D.SyncTransforms();
        }

        
        // si otro script modifica esa varibale no va a poder  moverse 
        if (puedeMoverse)
        {
            direction = controls.Movement.Move.ReadValue<Vector2>(); 
        }
        else
        {
            direction = Vector2.zero;
        }
            
        
        
        AdjustRotation(direction.x);
        animatorSprite.SetFloat("Horizontal", Mathf.Abs(direction.x)); 
        animatorBones.SetFloat("Horizontal", Mathf.Abs(direction.x)); 

         if (isRunning || !inGround || direction == Vector2.zero)
        {
            stateHandler.ShowSprite();

        }
        else
        {
            stateHandler.ShowBones();
            
        }
        

    }


    // ----------- fixedUpdate ------------
    private void FixedUpdate()
    {
        float speed = isRunning ? movementVelocityRun : movementVelocityWalk;
        
        if (puedeMoverse)
        {
            rb2D.linearVelocity = new Vector2(direction.x * speed, rb2D.linearVelocityY);
        }
        else
        {
            rb2D.linearVelocity = Vector2.zero;
        }
         
        
        if (speed > 15f) speed = 15f; // Límite máximo
        
        
        // Método mejorado para detectar el suelo
        CheckGrounded();
        
        animatorSprite.SetBool("inGround", inGround); 
        animatorBones.SetBool("inGround", inGround); 

        animatorSprite.SetBool("isRunning", isRunning); 
        animatorBones.SetBool("isRunning", isRunning); 

    }



    // --------------- Spin Sprite ------------------
    private void AdjustRotation(float directionX){
        if(directionX > 0 && !lookRight){
            Spin(); 
        }else if(directionX < 0 && lookRight){
            Spin(); 
        }
    }
    private void Spin(){
        lookRight = !lookRight;
        Vector3 escala = transform.localScale; 
        escala.x *= -1;
        transform.localScale = escala;
        
    }

    
    // --------------- JUMP ------------------
    private void Jump(){
        if(inGround){
            rb2D.AddForce(new Vector2(0,jumpForce), ForceMode2D.Impulse);     
            isJumping = true;
        }        
    }

// -----------------------------

    // Método mejorado para detectar el suelo
    void CheckGrounded()
    {
        // Usar tres puntos para la detección: izquierda, centro y derecha
        Vector2 center = groundCheck.position;
        Vector2 left = new Vector2(center.x - groundCheckWidth/2, center.y);
        Vector2 right = new Vector2(center.x + groundCheckWidth/2, center.y);
        
        bool centerGrounded = Physics2D.OverlapCircle(center, groundCheckRadius, whatIsGround);
        bool leftGrounded = Physics2D.OverlapCircle(left, groundCheckRadius, whatIsGround);
        bool rightGrounded = Physics2D.OverlapCircle(right, groundCheckRadius, whatIsGround);
        
        // Si cualquiera de los tres puntos detecta suelo, entonces está en el suelo
        bool currentlyGrounded = centerGrounded || leftGrounded || rightGrounded;
        
        // Actualizamos el estado previo y actual
        wasGrounded = inGround;
        inGround = currentlyGrounded;
        
        // También podemos detectar el momento de aterrizaje si es necesario
        // (cuando pasamos de estar en el aire a tocar suelo)
        if (!wasGrounded && inGround)
        {
            isJumping = false;
            // Aquí podríamos agregar lógica para el aterrizaje si es necesario
            // Por ejemplo, reproducir un sonido o una animación específica
        }
    }

    // Visualización de depuración
    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            // Dibujar el punto central
            Gizmos.color = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            
            // Dibujar puntos laterales
            Vector2 center = groundCheck.position;
            Vector2 left = new Vector2(center.x - groundCheckWidth/2, center.y);
            Vector2 right = new Vector2(center.x + groundCheckWidth/2, center.y);
            
            Gizmos.color = Physics2D.OverlapCircle(left, groundCheckRadius, whatIsGround) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(left, groundCheckRadius);
            
            Gizmos.color = Physics2D.OverlapCircle(right, groundCheckRadius, whatIsGround) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(right, groundCheckRadius);
        }
    }
    
private void ApplyKnockback()
{
   if (rb2D == null) // Verifica que el Rigidbody2D esté asignado
    {
        Debug.LogError("Rigidbody2D no está asignado en FatherMoment1");
        return;
    }

    float knockbackForce = 20f; // Aumenta la fuerza si es necesario
    Vector2 knockbackDirection = new Vector2(-Mathf.Sign(transform.localScale.x), 0.3f); // Dirección opuesta al mirar

    rb2D.linearVelocity = Vector2.zero; // Detén el movimiento actual
    rb2D.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

    Debug.Log("Knockback aplicado. Fuerza: " + knockbackDirection * knockbackForce);
}
private IEnumerator FlashAndDisableHeart(int heartIndex)
{
    if (heartIndex < 0 || heartIndex >= heartFulls.Length || 
       heartFulls[heartIndex] == null || heartEmptys[heartIndex] == null)
    {
        Debug.LogError("Índice o referencia inválida");
        yield break;
    }

    SpriteRenderer fullHeart = heartFulls[heartIndex];
    SpriteRenderer emptyHeart = heartEmptys[heartIndex];
    isInvulnerable = true;

    // Efecto de parpadeo con brillo
    for (int i = 0; i < 3; i++)
    {
        fullHeart.color = Color.red * flashIntensity; // Brillo rojo
        yield return new WaitForSeconds(0.1f);
        fullHeart.color = Color.white; // Normal
        yield return new WaitForSeconds(0.1f);
    }

    // Transición final
    fullHeart.enabled = false; // Oculta el lleno (sin desactivar GameObject)
    emptyHeart.enabled = true; // Muestra el vacío
    isInvulnerable = false;
}
    private void ResetHearts()
{
     for (int i = 0; i < heartFulls.Length; i++)
    {
        if (heartFulls[i] != null)
        {
            heartFulls[i].enabled = (i < currentHealth);
            heartFulls[i].color = Color.white; // Restablece color
        }
        if (heartEmptys[i] != null)
            heartEmptys[i].enabled = (i >= currentHealth);
    }
    
}
private void Die()
{
   if (isDead) return;
    isDead = true;

    // 1. Detener movimiento/física
    rb2D.bodyType = RigidbodyType2D.Kinematic; 
    rb2D.linearVelocity = Vector2.zero;

    // 2. Efecto de desvanecimiento (Alpha a 0 en 0.5 segundos)
    LeanTween.alpha(gameObject, 0f, 0.5f)
        .setEase(LeanTweenType.easeInOutQuad)
        .setOnComplete(() => {
            // Opcional: acciones adicionales al completar el fade
        });

    // 3. Desactivar controles
    controls.Disable();

    StartCoroutine(RespawnAfterDelay());
    
}
private IEnumerator RespawnAfterDelay()
{
   yield return new WaitForSeconds(respawnDelay);

    // 1. Reactivar visibilidad (Alpha a 1 en 0.3 segundos)
    LeanTween.alpha(gameObject, 1f, 0.3f);

    // 2. Restaurar física
    rb2D.bodyType = RigidbodyType2D.Dynamic;
    
    // 3. Resetear posición y estado
    transform.position = respawnPoint.position;
    currentHealth = maxHealth;
    ResetHearts();
    controls.Enable();
    isDead = false;
}
public void TakeDamage(int damageAmount, Vector2 knockback)
{
    if(isDead || isInvulnerable) return;

    currentHealth -= damageAmount;
    UpdateHeartsUI();
    
    // Aplicar knockback
    rb2D.linearVelocity = Vector2.zero;
    rb2D.AddForce(knockback, ForceMode2D.Impulse);
    
    // Manejar muerte o invulnerabilidad
    if(currentHealth <= 0)
    {
        Die();
    }
    else
    {
        StartCoroutine(InvulnerabilityRoutine());
    }
}
public void InstantKill()
{
    if(isDead) return;
    
    currentHealth = 0;
    UpdateHeartsUI();
    Die();
}


private IEnumerator InvulnerabilityRoutine()
{
    isInvulnerable = true;
    float timer = 0f;
    bool visible = true;

    while(timer < invulnerabilityTime)
    {
        playerSprite.enabled = visible;
        visible = !visible;
        timer += blinkInterval;
        yield return new WaitForSeconds(blinkInterval);
    }
    
    playerSprite.enabled = true;
    isInvulnerable = false;
}

private void UpdateHeartsUI()
{
    for(int i = 0; i < heartFulls.Length; i++)
    {
        heartFulls[i].enabled = i < currentHealth;
        heartEmptys[i].enabled = i >= currentHealth;
    }
}

    // --- Curacion, aun sin utilizar ---
    /*public void Heal()
    {
        if (currentHealth >= maxHealth) return;
        currentHealth++;
        heartFulls[currentHealth - 1].SetActive(true); // Reactiva el corazón
    }*/

}