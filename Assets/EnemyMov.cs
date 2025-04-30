using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    enum EnemyState { Patrolling, Chasing }

    [Header("Configuración de Patrulla")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float reachingDistance = 0.2f;

    [Header("Configuración de Persecución")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float visionRange = 4f;
    [SerializeField] private float visionAngle = 60f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float chaseTimeout = 3f;

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    private EnemyState currentState;
    private int currentPointIndex = 0;
    private float chaseTimer;
    private Vector2 movementDirection;

    private void Start()
    {
        if (patrolPoints.Length == 0)
        {
            Debug.LogError("¡No hay puntos de patrulla asignados!");
            enabled = false;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                PatrolBehavior();
                CheckPlayerDetection();
                animator.SetBool("IsChasing", false);
                break;

            case EnemyState.Chasing:
                ChaseBehavior();
                animator.SetBool("IsChasing", true);
                break;
        }

        UpdateFacingDirection();
    }

    private void PatrolBehavior()
    {
        Vector2 targetPosition = patrolPoints[currentPointIndex].position;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        rb.linearVelocity = direction * patrolSpeed;

        if (Vector2.Distance(transform.position, targetPosition) < reachingDistance)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    private void CheckPlayerDetection()
    {
        if (PlayerInSight())
        {
            currentState = EnemyState.Chasing;
            chaseTimer = 0f;
        }
    }

    private bool PlayerInSight()
    {
        if (player == null) return false;
        if (Vector2.Distance(transform.position, player.position) > visionRange) return false;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector2.Angle(transform.right, directionToPlayer);

        if (angleToPlayer > visionAngle / 2) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer,
            visionRange,
            obstacleMask
        );

        return hit.collider == null || hit.collider.CompareTag("Player");
    }

    private void ChaseBehavior()
    {
        chaseTimer += Time.deltaTime;
        
        if (chaseTimer >= chaseTimeout)
        {
            currentState = EnemyState.Patrolling;
            return;
        }

        if (PlayerInSight())
        {
            chaseTimer = 0f;
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * chaseSpeed;
            movementDirection = direction;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void UpdateFacingDirection()
    {
        if (rb.linearVelocity.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(rb.linearVelocity.x);
            transform.localScale = scale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja la ruta de patrulla
        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] != null)
            {
                Gizmos.DrawSphere(patrolPoints[i].position, 0.15f);
                if (i > 0) Gizmos.DrawLine(patrolPoints[i - 1].position, patrolPoints[i].position);
            }
        }

        // Dibuja el cono de visión
        Gizmos.color = Color.yellow;
        Vector3 visionStart = transform.position;
        Vector3 visionRight = Quaternion.Euler(0, 0, visionAngle / 2) * transform.right * visionRange;
        Vector3 visionLeft = Quaternion.Euler(0, 0, -visionAngle / 2) * transform.right * visionRange;

        Gizmos.DrawLine(visionStart, visionStart + visionRight);
        Gizmos.DrawLine(visionStart, visionStart + visionLeft);
    }
}