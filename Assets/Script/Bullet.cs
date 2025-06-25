using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 20f;
    public float lifetime = 2f;
    public float speed = 20f;
    public GameObject impactEffect;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
        Destroy(gameObject, lifetime); // Auto-delete after 2 secs
    }

    void OnTriggerEnter(Collider other)
    {
        // Don't destroy bullet if it hits a trigger or the player
        if (other.isTrigger || other.CompareTag("Player")) return;

        SmoothEnemyAI enemy = other.GetComponent<SmoothEnemyAI>();
        if (enemy != null)
        {
            // Pass both damage amount and the bullet's position as damage source
            enemy.TakeDamage(damage, transform.position);
        }

        // Instantiate impact effect if available
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}