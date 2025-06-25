using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class SmoothEnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask playerMask, obstacleMask;
    public float chaseSpeed = 4f;
    public float patrolSpeed = 2f;
    public float acceleration = 4f;
    public float deceleration = 6f;
    public float memoryDuration = 2f;
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;
    private int patrolIndex = 0;
    private float waitTime = 1f;
    private float waitCounter = 0f;
    private bool waiting = false;

    private Transform player;
    private CharacterController controller;
    private Animator animator;
    private AudioSource audioSource;

    private Vector3 velocity = Vector3.zero;
    private float speed = 0f;
    private float lastSeenTime;
    private Vector3 lastKnownPlayerPos;

    private enum State { Patrol, Chase, Return }
    private State currentState = State.Patrol;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Transform fallback = new GameObject("PatrolStart").transform;
            fallback.position = transform.position;
            patrolPoints = new Transform[] { fallback };
        }
    }

    void Update()
    {
        if (currentState == State.Patrol) Patrol();
        else if (currentState == State.Chase) Chase();
        else if (currentState == State.Return) ReturnToPatrol();

        UpdateAnimator();
    }
    public void TakeDamage(float amount, Vector3 sourcePosition)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Optional: quay mặt về hướng bị bắn
            Vector3 dir = sourcePosition - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-dir);
            }
        }
    }

    void Die()
    {
        isDead = true;
        if (animator != null) animator.SetTrigger("Die");
        if (controller != null) controller.enabled = false;
        enabled = false;

        Destroy(gameObject, 3f); // auto xoá sau 3s
    }
    void Patrol()
    {
        if (CanSeePlayer())
        {
            TriggerDetect();
            currentState = State.Chase;
            return;
        }

        Vector3 target = patrolPoints[patrolIndex].position;
        MoveTowards(target, patrolSpeed);

        if (!waiting && Vector3.Distance(transform.position, target) < 0.5f)
        {
            waiting = true;
            waitCounter = waitTime;
        }

        if (waiting)
        {
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0f)
            {
                waiting = false;
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            }
        }
    }

    void Chase()
    {
        if (CanSeePlayer())
        {
            lastSeenTime = Time.time;
            lastKnownPlayerPos = player.position;
        }

        if (Time.time - lastSeenTime > memoryDuration)
        {
            currentState = State.Return;
            return;
        }

        MoveTowards(lastKnownPlayerPos, chaseSpeed);
    }

    void ReturnToPatrol()
    {
        Vector3 target = patrolPoints[patrolIndex].position;
        MoveTowards(target, patrolSpeed);

        if (Vector3.Distance(transform.position, target) < 0.5f)
        {
            currentState = State.Patrol;
        }
    }

    void MoveTowards(Vector3 target, float targetSpeed)
    {
        Vector3 dir = (target - transform.position).normalized;
        float speedChange = (speed < targetSpeed) ? acceleration : deceleration;
        speed = Mathf.MoveTowards(speed, targetSpeed, speedChange * Time.deltaTime);

        velocity = dir * speed;
        velocity.y = -9.8f;

        controller.Move(velocity * Time.deltaTime);

        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position);
        float distToPlayer = dirToPlayer.magnitude;

        if (distToPlayer > viewRadius) return false;

        Vector3 dir = dirToPlayer.normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle / 2f) return false;

        if (Physics.Raycast(transform.position + Vector3.up, dir, distToPlayer, obstacleMask))
            return false;

        return true;
    }

    void TriggerDetect()
    {
        if (animator) animator.SetTrigger("DetectPlr");
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", new Vector3(velocity.x, 0, velocity.z).magnitude);
        animator.SetBool("IsChasing", currentState == State.Chase);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + left * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * viewRadius);
    }
}
