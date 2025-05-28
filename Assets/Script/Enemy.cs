using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Chase Settings")]
    public float chaseDistance = 10f;
    public float stopDistance = 2f;
    public float returnDistance = 15f;

    [Header("References")]
    public Slider healthSlider;
    public Transform healthBarCanvas;
    public Transform player;

    private NavMeshAgent agent;
    private Vector3 initialPosition;
    private Animator anim;

    private enum State { Idle, Chasing, Returning }
    private State currentState = State.Idle;

    private bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>(); // get that spicy Animator

        currentHealth = maxHealth;
        UpdateHealthUI();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        initialPosition = transform.position;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                if (distanceToPlayer <= chaseDistance)
                    currentState = State.Chasing;
                agent.SetDestination(transform.position); // stay idle
                break;

            case State.Chasing:
                if (distanceToPlayer <= stopDistance)
                {
                    agent.SetDestination(transform.position); // Stand still
                }
                else if (distanceToPlayer >= returnDistance)
                {
                    currentState = State.Returning;
                }
                else
                {
                    agent.SetDestination(player.position);
                }
                break;

            case State.Returning:
                float distToStart = Vector3.Distance(transform.position, initialPosition);
                if (distToStart > 0.5f)
                {
                    agent.SetDestination(initialPosition);
                }
                else
                {
                    currentState = State.Idle;
                }
                break;
        }

        // Animator Speed param
        if (anim != null)
        {
            float movementSpeed = agent.velocity.magnitude;
            anim.SetFloat("Speed", movementSpeed);
        }

        // Face camera
        if (healthBarCanvas != null && Camera.main != null)
        {
            healthBarCanvas.rotation = Quaternion.LookRotation(healthBarCanvas.position - Camera.main.transform.position);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        isDead = true;
        agent.isStopped = true;

        if (anim != null)
        {
            anim.SetBool("isDead", true);
        }

        // Destroy after animation plays
        Destroy(gameObject, 3f); // or use event callback from animation
    }
}
