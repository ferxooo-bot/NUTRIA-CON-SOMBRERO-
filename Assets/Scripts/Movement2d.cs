using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class Movement2d : MonoBehaviour
{
   public Controls controls; // se crea la variable para controlar el Input Action llamado Controls

    public Vector2 direction; // almacena la dirección del movimiento
    public Rigidbody2D rb2D; 
    public float movementVelocityWalk =7f; 
    public float movementVelocityRun;
    //is running 
    public bool isRunning; 

    public float jumpForce;  

    //-------
    public LayerMask whatIsGround; 
    public Transform GroundController; 
    public Vector3 BoxDimensions; 
    public bool inGround; 
    
    // -------------------- ANIMACIÓN --------------

    [Header("Animation")]
    private Animator animator; 

    public bool lookRight = true;  

    private void Awake() 
    {
        controls = new(); 
        animator = GetComponent<Animator>(); 
        
    }
    private void OnEnable()
    {
        controls.Enable(); 
        controls.Movement.Jump.started += _ => Jump(); 
        // controls.Movement.Run.performed += _ => isRunning = true;  // Shift presionado
        // controls.Movement.Run.canceled += _ => isRunning = false;  // Shift soltado
    }
    private void OnDisable()
    {
        controls.Disable(); 
        controls.Movement.Jump.started -= _ => Jump(); 
        // controls.Movement.Run.performed -= _ => isRunning = true;  
        // controls.Movement.Run.canceled -= _ => isRunning = false;
    }

    // ----------- update ------------
    private void Update(){
        direction = controls.Movement.Move.ReadValue<Vector2>(); 


        animator.SetFloat("Horizontal", Mathf.Abs(direction.x));
        // animator.SetBool("isRunning", isRunning); 
        

        AdjustRotation(direction.x);


    }


    // ----------- fixedUpdate ------------
    private void FixedUpdate()
    {

        // float speed = isRunning ? movementVelocityRun : movementVelocityWalk;
        rb2D.linearVelocity = new Vector2(direction.x * movementVelocityWalk, rb2D.linearVelocityY); 
        

        //OverlapBox detecta coliciones en un area determinada, a un layer determinado => devuelve true/false
        //Caja para detectar suelo
        // Parámetros => posiciónDelCentroDeLaCaja Transform => ObjEmpty / Dimensiones V3 / Rotación / LayerMask  
        inGround = Physics2D.OverlapBox(GroundController.position, BoxDimensions, 0f, whatIsGround); 

        animator.SetBool("inGround", inGround); 
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
    
    //Dibujar caja detectora de suelo: 
    private void OnDrawGizmos(){
        Gizmos.color = Color.white; 
        Gizmos.DrawWireCube(GroundController.position, BoxDimensions); 
    }


}
