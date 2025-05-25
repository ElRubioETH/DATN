using UnityEngine;

public class PlatformFollower : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playertag = "Player";
    [SerializeField] private Transform platform;

    private Transform player;
    private CharacterController playerCC;

    private bool isOnPlatform = false;
    private Vector3 lastPlatformPos;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playertag))
        {
            player = other.transform;
            playerCC = player.GetComponent<CharacterController>();
            isOnPlatform = true;

            lastPlatformPos = platform.position; // Ghi nhớ vị trí platform lúc player bước lên
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playertag))
        {
            isOnPlatform = false;
            player = null;
            playerCC = null;
        }
    }

    void LateUpdate()
    {
        if (!isOnPlatform || player == null || playerCC == null) return;

        // Tính platform di chuyển bao nhiêu trong frame này
        Vector3 platformDelta = platform.position - lastPlatformPos;

        // Thêm chuyển động platform vào player
        playerCC.Move(platformDelta);

        // Update lại vị trí platform để tính cho frame sau
        lastPlatformPos = platform.position;
    }
}
