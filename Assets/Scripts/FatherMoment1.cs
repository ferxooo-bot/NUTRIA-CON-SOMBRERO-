using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using System.Collections;
using UnityEngine.UI;

public class FatherMoment1 : MonoBehaviour
{
   public Controls controls; // se crea la variable para controlar el Input Action llamado Controls
    public bool puedeMoverse = true;
    public Vector2 direction; // almacena la dirección del movimiento
    public Rigidbody2D rb2D; 
    public float movementVelocityWalk =7f; 
    public float movementVelocityRun;
    public GameObject bones;  // Animación de caminar
    public GameObject sprite; // Animación de correr y saltar
    public Animator animatorSprite;
    public Animator animatorBones; 

   
[Header("Heart System")]
[SerializeField] private SpriteRenderer[] heartFulls = new SpriteRenderer[3];  // Corazones llenos
[SerializeField] private SpriteRenderer[] heartEmptys = new SpriteRenderer[3]; // Corazones vacíos
[SerializeField] private float flashIntensity = 3f; // Intensidad del brillo al parpadear

[Header("Configuración de Muerte")]
[SerializeField] public Transform respawnPoint; // Arrastra el punto de reinicio
[SerializeField] private float respawnDelay = 1.0f; // Tiempo antes de reiniciar
private bool isDead = false;
    //-------
    public int currentHealth;
    public float jumpForce = 4f; 
    public LayerMask whatIsGround; 
    public bool inGround; 
    public bool isRunning;

    public int maxHealth = 3;
    
    // ---------------------
    public StateHandler stateHandler;
    private bool isInvulnerable = false;
    //---------------------




    // -------------------- ANIMACIÓN HIJOS--------------

    public bool lookRight = true;
    internal bool rushing;

    private void Awake() 
    {
        ResetHearts();
        stateHandler = GetComponent<StateHandler>(); 
        controls = new();  
        
        animatorSprite = transform.GetChild(0).GetComponent<Animator>();       
        animatorBones = transform.GetChild(1).GetComponent<Animator>(); 
        currentHealth = maxHealth;
        
        
       Debug.Log($"Fulls asignados: {heartFulls != null}, Emptys asignados: {heartEmptys != null}");
    Debug.Log($"Tamaño Fulls: {heartFulls?.Length}, Tamaño Emptys: {heartEmptys?.Length}");
    
    }
    private void Start()
{
    
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
         
        

        //OverlapBox detecta coliciones en un area determinada, a un layer determinado => devuelve true/false
        //Caja para detectar suelo
        // Parámetros => posiciónDelCentroDeLaCaja Transform => ObjEmpty / Dimensiones V3 / Rotación / LayerMask  
        GroundCheck(); 
        
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
        }        
    }

// -----------------------------


    private void GroundCheck(){
        float rayLenth = 0.7f; 
        Ray ray= new Ray(transform.position, Vector2.down); 

        Debug.DrawRay(ray.origin, ray.direction * rayLenth, Color.red); 

        inGround = Physics2D.Raycast(ray.origin, ray.direction, rayLenth, whatIsGround); 

    }
    public void Hurt()
{ if (isInvulnerable || currentHealth <= 0) return;
    
    if (isInvulnerable || currentHealth <= 0) return;
    
    currentHealth--;
    int heartIndex = Mathf.Clamp(currentHealth, 0, heartFulls.Length - 1);
    StartCoroutine(FlashAndDisableHeart(heartIndex));
    


    ApplyKnockback();
     if (currentHealth <= 0) Die();

     
    
    
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
public IEnumerator RespawnAfterDelay()
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


public void SetRespawnPoint(string lastRespawn)
{
    // Buscar el Transform con el nombre 'lastRespawn'
    Transform newRespawnPoint = GameObject.Find(lastRespawn)?.transform;

    // Verificar si se encontró el respawn
    if (newRespawnPoint != null)
    {
        // Asignar el nuevo respawn al campo 'respawnPoint'
        respawnPoint = newRespawnPoint;
    }
    else
    {
        Debug.LogWarning("No se encontró el respawn con el nombre: " + lastRespawn);
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





