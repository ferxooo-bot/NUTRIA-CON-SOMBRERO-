
using UnityEngine;

using System.Collections;


public class MovementSwin : MonoBehaviour
{   
    public StateHandler stateHandler;
    [SerializeField] private Transform headPivot;
    
    [Header("Configuración de Movimiento en Agua")]
    [SerializeField] private float baseWaterVelocity = 14f;  
    private float waterVelocity;

    [Header("Salud y Daño")]
    public int maxHealth = 3;
    public int currentHealth;
    public float invulnerabilityTime = 1.5f;
    public float blinkInterval = 0.15f;
    private bool isInvulnerable = false;
    private bool isDead = false;

    [Header("Sistema de Corazones (UI)")]
    [SerializeField] private SpriteRenderer[] heartFulls = new SpriteRenderer[3];
    [SerializeField] private SpriteRenderer[] heartEmptys = new SpriteRenderer[3];
    [SerializeField] private float flashIntensity = 3f;

    [Header("Configuración de Muerte y Reaparición")]
    [SerializeField] public Transform respawnPoint;
    [SerializeField] private float respawnDelay = 1.0f;

    // Variables de agua
    [SerializeField] private float waterDensity = 1.0f;
    [SerializeField] private float waterDrag = 0.9f;
    [SerializeField] private float waterBuoyancy = -1f;
    [SerializeField] private float rotationSpeed = 10f;
    private float playerMass;

    private bool isInWater = false;
    private Rigidbody2D rb;
    private Vector2 direction;
    private Controls controls;
    private bool isRunning; 
    public bool lookRight = true;  
    private SpriteRenderer playerSprite;

    public GameObject BonesHeadBody;  
    public Animator animatorBonesHeadBody; 

    void Awake()
    {       
        animatorBonesHeadBody = transform.GetChild(1).GetComponent<Animator>(); 
        rb = GetComponent<Rigidbody2D>();
        controls = new Controls();
        waterVelocity = baseWaterVelocity;
        playerMass = rb.mass;
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        ResetHearts();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Movement.Run.performed += _ => isRunning = true;  
        controls.Movement.Run.canceled += _ => isRunning = false;  
        rb.gravityScale = 0;
        isInWater = true;
        animatorBonesHeadBody.SetBool("isInWater", isInWater); 
    }

    private void OnDisable() 
    {
        controls.Disable(); 
        controls.Movement.Run.performed -= _ => isRunning = true;  
        controls.Movement.Run.canceled -= _ => isRunning = false;
        rb.gravityScale = 1;
        isInWater = false;
        animatorBonesHeadBody.SetBool("isInWater", isInWater); 
    }

    void Update()
    {
        if(isInWater && !isDead)
        {
            direction = controls.Movement.Move.ReadValue<Vector2>();
            HandleRotation();
            HandleSpriteFlip();
        }

        animatorBonesHeadBody.SetBool("isInWater", isInWater); 
        animatorBonesHeadBody.SetFloat("Speed", direction.magnitude);
    }

    void FixedUpdate()
    {   
        if (isDead) return;
        
        waterVelocity = isRunning ? baseWaterVelocity * 2f : baseWaterVelocity;

        if (isInWater)
        {
            MoveInWater();
        }
    }

    // Sistema de daño y salud
    public void TakeDamage(int damageAmount, Vector2 knockback)
    {
        if(isDead || isInvulnerable) return;

        currentHealth -= damageAmount;
        UpdateHeartsUI();
    
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);
    
        if(currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        rb.bodyType = RigidbodyType2D.Kinematic; 
        rb.linearVelocity = Vector2.zero;
        LeanTween.alpha(gameObject, 0f, 0.5f)
            .setEase(LeanTweenType.easeInOutQuad);
            
        controls.Disable();
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        
        LeanTween.alpha(gameObject, 1f, 0.3f);
        rb.bodyType = RigidbodyType2D.Dynamic;
        transform.position = respawnPoint.position;
        currentHealth = maxHealth;
        ResetHearts();
        controls.Enable();
        isDead = false;
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float timer = 0f;
        bool visible = true;

        while(timer < invulnerabilityTime)
        {
           
            visible = !visible;
            timer += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }
        
        isInvulnerable = false;
    }

    // Sistema de corazones
    private void UpdateHeartsUI()
    {
        for(int i = 0; i < heartFulls.Length; i++)
        {
            if(heartFulls[i] != null) heartFulls[i].enabled = i < currentHealth;
            if(heartEmptys[i] != null) heartEmptys[i].enabled = i >= currentHealth;
        }
    }

    private void ResetHearts()
    {
        for (int i = 0; i < heartFulls.Length; i++)
        {
            if(heartFulls[i] != null)
            {
                heartFulls[i].enabled = (i < currentHealth);
                heartFulls[i].color = Color.white;
            }
            if(heartEmptys[i] != null)
                heartEmptys[i].enabled = (i >= currentHealth);
        }
    }

    // Resto de métodos de movimiento y rotación (sin cambios)
    private void HandleRotation()
    {
        if(direction.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, Time.deltaTime * rotationSpeed);
        }
    }

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

    private void MoveInWater()
    {
        Vector2 movement = direction * waterVelocity;
        rb.AddForce(movement, ForceMode2D.Force);
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, waterVelocity);
    }
}