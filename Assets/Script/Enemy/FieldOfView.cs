using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FieldOfView : MonoBehaviour
{
    public Animator animator;
    private AudioSource audioSource;
    [Header("Audio Clips")]
    [SerializeField] private AudioClip detectClip;
    [SerializeField] private AudioClip chaseClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip dieClip;
    // Add these properties to your FieldOfView class
    public float DetectionProgress => detectionProgress;
    public float DetectionTime => detectionTime;
    // FOV parameters
    public float radius;
    [Range(0, 360)]
    public float angle;

    // Detection parameters
    public float chaseDuration = 5f;
    public float chaseRadiusMultiplier = 1.5f;
    [SerializeField] private float detectionTime = 1f;
    // References
    public GameObject playerRef;
    public LayerMask targetMask;
    public LayerMask obstructionMask;
    private NavMeshAgent agent;
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    // States
    public enum EnemyState { Patrol, Detect, Chase, Return, Die }
    public EnemyState currentState = EnemyState.Patrol;

    // State variables
    private float detectionProgress = 0f;
    private float chaseTimeRemaining = 0f;
    private bool isDead = false;

    // Patrol variables
    public List<Transform> patrolPoints;
    private int currentPatrolIndex = 0;
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f;
    public float stoppingDistance = 1f;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        playerRef = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        startingPosition = transform.position;
        startingRotation = transform.rotation;

        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        agent.speed = patrolSpeed;
        agent.stoppingDistance = stoppingDistance;

        StartCoroutine(FOVRoutine());
        StartCoroutine(StateMachine());
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (!isDead)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private IEnumerator StateMachine()
    {
        while (!isDead)
        {
            switch (currentState)
            {
                case EnemyState.Patrol:
                    yield return StartCoroutine(PatrolState());
                    break;
                case EnemyState.Detect:
                    yield return StartCoroutine(DetectState());
                    break;
                case EnemyState.Chase:
                    yield return StartCoroutine(ChaseState());
                    break;
                case EnemyState.Return:
                    yield return StartCoroutine(ReturnState());
                    break;
                case EnemyState.Die:
                    yield return StartCoroutine(DieState());
                    break;
            }
            yield return null;
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    if (currentState == EnemyState.Patrol || currentState == EnemyState.Return)
                    {
                        currentState = EnemyState.Detect;
                        detectionProgress = 0f;
                    }
                }
            }
        }
    }

    private IEnumerator PatrolState()
    {
        if (animator) animator.SetBool("IsWalking", true);

        if (patrolPoints.Count == 0) yield break;

        agent.speed = patrolSpeed;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        while (currentState == EnemyState.Patrol)
        {
            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);

                // Optional: Add a wait time at each patrol point
                yield return new WaitForSeconds(1f);
            }
            if (animator) animator.SetBool("IsWalking", false);

            yield return null;
        }

    }

    private IEnumerator DetectState()
    {
        PlaySound(detectClip);
        if (animator) animator.SetTrigger("Detect");
        detectionProgress = 0f;

        while (currentState == EnemyState.Detect && detectionProgress < detectionTime)
        {
            // Keep looking at the player while detecting
            Vector3 directionToPlayer = (playerRef.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            // Check if player is still visible
            if (IsPlayerVisible())
            {
                detectionProgress += Time.deltaTime;
            }
            else
            {
                currentState = EnemyState.Patrol;
                yield break;
            }
            yield return null;
        }

        if (detectionProgress >= detectionTime)
        {
            currentState = EnemyState.Chase;
            chaseTimeRemaining = chaseDuration;
        }
    }

    private IEnumerator ChaseState()
    {
        PlaySound(chaseClip);
        if (animator) animator.SetBool("IsChasing", true);
        agent.speed = chaseSpeed;
        chaseTimeRemaining = chaseDuration;

        while (currentState == EnemyState.Chase && chaseTimeRemaining > 0)
        {
            if (IsPlayerVisible())
            {
                // Player is still visible, reset chase timer and continue chasing
                chaseTimeRemaining = chaseDuration;
                agent.SetDestination(playerRef.transform.position);
            }
            else
            {
                // Player not visible, count down chase timer
                chaseTimeRemaining -= Time.deltaTime;
            }

            yield return null;
        }
        if (animator) animator.SetBool("IsChasing", false);

        // Transition to return state when chase time is up
        currentState = EnemyState.Return;
    }

    private IEnumerator ReturnState()
    {
        StopSound(chaseClip);
        agent.SetDestination(startingPosition);

        while (currentState == EnemyState.Return)
        {
            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                // Reached starting position, return to patrol
                transform.rotation = Quaternion.Slerp(transform.rotation, startingRotation, Time.deltaTime * 5f);
                currentState = EnemyState.Patrol;
                yield break;
            }

            // Check if player becomes visible during return
            if (IsPlayerVisible())
            {
                currentState = EnemyState.Detect;
                detectionProgress = 0f;
                yield break;
            }

            yield return null;
        }
    }
    public void Attack()
    {
        if (animator) animator.SetTrigger("Attack");
        PlaySound(attackClip);

        // Nếu muốn gây sát thương cho player, thêm code tại đây
    }

    private IEnumerator DieState()
    {
        if (animator) animator.SetTrigger("Die");
        PlaySound(dieClip);
        isDead = true;
        agent.isStopped = true;

        // Add death animation or effects here
        // For example:
        // GetComponent<Animator>().SetTrigger("Die");
        // yield return new WaitForSeconds(2f);
        yield return new WaitForSeconds(2f); // chờ animation chết

        Destroy(gameObject);
        yield return null;
    }

    private bool IsPlayerVisible()
    {
        Vector3 directionToTarget = (playerRef.transform.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, playerRef.transform.position);

        return (Vector3.Angle(transform.forward, directionToTarget) < angle / 2) &&
               (distanceToTarget <= radius * (currentState == EnemyState.Chase ? chaseRadiusMultiplier : 1f)) &&
               !Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask);
    }

    public void Die()
    {
        if (!isDead)
        {
            currentState = EnemyState.Die;
        }
    }
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
    private void StopSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.Stop();
    }
}