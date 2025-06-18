using UnityEngine;

public class EnemyAIWithFOV : MonoBehaviour
{
    public enum AIState { Patrolling, Chasing, Returning }
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;
    [Header("Sight Settings")]
    [SerializeField] private float viewRadius = 8f;
    [SerializeField][Range(0, 360)] private float viewAngle = 90f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float visionCheckInterval = 0.2f;
	
    [Header("Movement Settings")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stoppingDistance = 1f;

    [Header("Chase Settings")]
    [SerializeField] private float maxChaseDistance = 12f;
    [SerializeField] private float memoryDuration = 2f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private CharacterController characterController;

    private AIState currentState = AIState.Patrolling;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float lastSeenTime;
    private float nextVisionCheckTime;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        switch (currentState)
        {
            case AIState.Patrolling:
                PatrolBehavior();
                break;
            case AIState.Chasing:
                ChaseBehavior();
                break;
            case AIState.Returning:
                ReturnBehavior();
                break;
        }
    }
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void PatrolBehavior()
    {
        if (Time.time >= nextVisionCheckTime)
        {
            nextVisionCheckTime = Time.time + visionCheckInterval;
            if (CanSeePlayer())
            {
animator.SetBool("isWalking", false);
animator.SetBool("isCrouching", false);
                currentState = AIState.Chasing;
                lastSeenTime = Time.time;
                return;
            }
        }
    }

    void ChaseBehavior()
    {
animator.SetBool("isWalking", true);
        if (player == null)
        {
            currentState = AIState.Returning;
            return;
        }

        // Update player memory
        if (Time.time >= nextVisionCheckTime)
        {
            nextVisionCheckTime = Time.time + visionCheckInterval;
            if (CanSeePlayer())
            {
                lastSeenTime = Time.time;
            }
            else if (Time.time - lastSeenTime > memoryDuration)
            {
                currentState = AIState.Returning;
                return;
            }
        }

        // Check distance limits
        float distanceFromHome = Vector3.Distance(transform.position, initialPosition);
        if (distanceFromHome > maxChaseDistance)
        {
            currentState = AIState.Returning;
            return;
        }

        // Chase logic
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        MoveTowards(directionToPlayer, chaseSpeed);
        SmoothLookAt(player.position);
    }
    void Die()
    {
        isDead = true;
        animator.SetTrigger("isDead");

        // Vô hiệu hóa AI di chuyển và phát hiện
        currentState = AIState.Patrolling; // hoặc dùng flag riêng
        enabled = false;

        // Option: Destroy sau vài giây
        Destroy(gameObject, 3f);
    }

    void ReturnBehavior()
    {


        Vector3 directionToHome = (initialPosition - transform.position).normalized;
        float distanceToHome = Vector3.Distance(transform.position, initialPosition);

        if (distanceToHome < stoppingDistance)
        {
            currentState = AIState.Patrolling;
            transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, rotationSpeed * Time.deltaTime);
            return;
            animator.SetBool("isWalking", false);
        }

        MoveTowards(directionToHome, returnSpeed);
        SmoothLookAt(initialPosition);
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;
        directionToPlayer.Normalize();

        // Distance check
        if (distanceToPlayer > viewRadius) return false;

        // Angle check (more efficient than Vector3.Angle)
        if (Vector3.Dot(transform.forward, directionToPlayer) < Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad))
            return false;
        animator.SetTrigger("isRoaring"); // hoặc "womboCombo"

        // Obstacle check
        return !Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask);
    }

    void MoveTowards(Vector3 direction, float speed)
    {
        if (characterController != null && characterController.enabled)
        {
            characterController.Move(direction * speed * Time.deltaTime);
        }
        else
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    void SmoothLookAt(Vector3 targetPosition)
    {
        Vector3 lookDirection = new Vector3(
            targetPosition.x - transform.position.x,
            0,
            targetPosition.z - transform.position.z
        );

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw view radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // Draw view angle
        Vector3 forward = transform.forward;
        Vector3 leftLimit = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
        Vector3 rightLimit = Quaternion.Euler(0, viewAngle / 2, 0) * forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftLimit * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightLimit * viewRadius);

        // Draw current state
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, currentState.ToString(), style);
    }
}