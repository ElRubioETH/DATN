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

    private enum State { Idle, Chasing, Returning }
    private State currentState = State.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        initialPosition = transform.position;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                if (distanceToPlayer <= chaseDistance)
                    currentState = State.Chasing;
                break;

            case State.Chasing:
                if (distanceToPlayer <= stopDistance)
                {
                    agent.SetDestination(transform.position); // Đứng yên
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

        // Luôn xoay health bar nhìn camera
        if (healthBarCanvas != null && Camera.main != null)
        {
            healthBarCanvas.rotation = Quaternion.LookRotation(healthBarCanvas.position - Camera.main.transform.position);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
        UpdateHealthUI();
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
        Destroy(gameObject); // Hoặc làm animation, disable...
    }
}
