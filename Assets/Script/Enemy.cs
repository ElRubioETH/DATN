using UnityEngine;
using UnityEngine.AI;

public class EnemySightChase : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private NavMeshAgent agent;

    [Header("Vision Settings")]
    public float sightRange = 15f;
    public float fieldOfView = 120f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
            Debug.LogError("❌ Player chưa được gán trong Inspector!");

        if (agent == null)
            Debug.LogError("❌ Không tìm thấy NavMeshAgent!");
    }

    private void Update()
    {
        if (player == null || agent == null) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        Debug.Log($"📏 Khoảng cách tới Player: {distanceToPlayer:F2}");
        Debug.Log($"📐 Góc tới Player: {angleToPlayer:F2}");

        if (distanceToPlayer < sightRange && angleToPlayer < fieldOfView / 2)
        {
            Debug.DrawRay(transform.position, directionToPlayer * sightRange, Color.red);

            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, sightRange))
            {
                Debug.Log($"🔍 Raycast trúng: {hit.transform.name}");

                if (hit.transform.CompareTag("Player"))
                {
                    Debug.Log("🎯 Player trong tầm nhìn! Đuổi theo!!");
                    agent.SetDestination(player.position);
                }
                else
                {
                    Debug.Log("🚫 Có vật cản giữa Enemy và Player: " + hit.transform.name);
                }
            }
            else
            {
                Debug.Log("❌ Raycast không trúng gì cả!");
            }
        }
    }
}
