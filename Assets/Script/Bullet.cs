using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 20f;
    public float lifetime = 2f;

    void Start()
    {
        Destroy(gameObject, lifetime); // Auto-delete after 2 secs
    }

    void OnTriggerEnter(Collider other)
    {
        EnemyAIWithFOV enemy = other.GetComponent<EnemyAIWithFOV>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

}
