using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 15f;
    public bool enableContinuousJump = true;
    public float continuousJumpForce = 10f;
    public float maxJumpTime = 0.3f;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    private float jumpCooldown = 0.1f;
    private float lastJumpTime;
    private bool isJumping = false;
    private float jumpTime = 0f;
    
    // Modificaciones al ground check
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public float groundCheckWidth = 0.8f;
    public LayerMask groundLayer;
    
    // Variables para control de animación
    private bool isLanding = false;
    private bool jumpInputQueued = false;
    private float landingTime = 0f;
    public float minLandingDuration = 0.2f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        // Control horizontal
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        
        // Actualizar animación y dirección
        animator.SetBool("run", Mathf.Abs(moveInput) > 0.1f);
        if (moveInput > 0.1f) spriteRenderer.flipX = false;
        else if (moveInput < -0.1f) spriteRenderer.flipX = true;
        
        // Verificar si está en el suelo con un método mejorado
        CheckGrounded();
        
        // Gestionar la secuencia de animaciones
        HandleAnimationSequence();
        
        // Gestionar los inputs de salto
        HandleJumpInput();
    }
    
    // Método mejorado para detectar el suelo
    void CheckGrounded()
    {
        // Usar tres puntos para la detección: izquierda, centro y derecha
        Vector2 center = groundCheck.position;
        Vector2 left = new Vector2(center.x - groundCheckWidth/2, center.y);
        Vector2 right = new Vector2(center.x + groundCheckWidth/2, center.y);
        
        bool centerGrounded = Physics2D.OverlapCircle(center, groundCheckRadius, groundLayer);
        bool leftGrounded = Physics2D.OverlapCircle(left, groundCheckRadius, groundLayer);
        bool rightGrounded = Physics2D.OverlapCircle(right, groundCheckRadius, groundLayer);
        
        // Si cualquiera de los tres puntos detecta suelo, entonces está en el suelo
        bool currentlyGrounded = centerGrounded || leftGrounded || rightGrounded;
        
        // Detectar el momento de aterrizaje (cuando pasamos de estar en el aire a tocar suelo)
        if (!wasGrounded && currentlyGrounded)
        {
            isLanding = true;
            landingTime = Time.time;
            animator.SetBool("jump", false);
            animator.SetBool("isLanding", true);
        }
        
        // Actualizar el estado de grounded para el próximo frame
        wasGrounded = isGrounded;
        isGrounded = currentlyGrounded;
    }
    
    // Método para manejar la secuencia de animaciones
    void HandleAnimationSequence()
    {
        // Si estamos en animación de aterrizaje, verificar si ha terminado
        if (isLanding)
        {
            // Asegurar tiempo mínimo para la animación de aterrizaje
            if (Time.time >= landingTime + minLandingDuration)
            {
                isLanding = false;
                animator.SetBool("isLanding", false);
                
                // Procesar salto en cola (si existe)
                if (jumpInputQueued)
                {
                    PerformJump();
                    jumpInputQueued = false;
                }
            }
        }
    }
    
    // Método para manejar la entrada de salto
    void HandleJumpInput()
    {
        // Verificar si se presionó la tecla de salto
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Si estamos en animación de aterrizaje, encolar el salto
            if (isLanding)
            {
                jumpInputQueued = true;
            }
            // Si podemos saltar normalmente (en suelo y no en aterrizaje)
            else if (isGrounded && Time.time > lastJumpTime + jumpCooldown)
            {
                PerformJump();
            }
        }
        
        // Manejar salto sostenido
        if (enableContinuousJump && Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpTime < maxJumpTime)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, continuousJumpForce);
                jumpTime += Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }
        
        // Al soltar la tecla de salto, finalizar el salto sostenido
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
            // También limpiar cualquier entrada encolada si soltamos antes de que se procese
            jumpInputQueued = false;
        }
    }
    
    // Método para realizar el salto
    void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        lastJumpTime = Time.time;
        animator.SetBool("jump", true);
        animator.SetBool("isLanding", false);
        isJumping = true;
        jumpTime = 0f;
    }
    
    // Visualización de depuración
    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            // Dibujar el punto central
            Gizmos.color = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            
            // Dibujar puntos laterales
            Vector2 center = groundCheck.position;
            Vector2 left = new Vector2(center.x - groundCheckWidth/2, center.y);
            Vector2 right = new Vector2(center.x + groundCheckWidth/2, center.y);
            
            Gizmos.color = Physics2D.OverlapCircle(left, groundCheckRadius, groundLayer) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(left, groundCheckRadius);
            
            Gizmos.color = Physics2D.OverlapCircle(right, groundCheckRadius, groundLayer) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(right, groundCheckRadius);
        }
    }
}