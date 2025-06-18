using TMPro;
using UnityEngine;

public class NPCQuest : MonoBehaviour
{
    // Biến tham chiếu
    public GameObject player; // Đối tượng người chơi
    public float interactionDistance = 3f; // Khoảng cách để tương tác
    public TextMeshProUGUI dialogueText; // Text UI để hiển thị hội thoại
    public GameObject dialoguePanel; // Panel chứa hộp thoại
    public bool isPlayerInRange; // Kiểm tra người chơi có trong khoảng cách không
    public bool isQuestActive = false; // Trạng thái nhiệm vụ
    public int requiredItems = 5; // Số vật phẩm cần thu thập (quả táo)
    public int collectedItems = 0; // Số vật phẩm đã thu thập
    public string questItemName = "Apple"; // Tên vật phẩm cần thu thập

    // Tham chiếu đến InventorySystem và QuestDisplayUI
    private InventorySystem inventorySystem;
    private QuestDisplayUI questUI;

    void Start()
    {
        dialoguePanel.SetActive(false); // Ẩn hộp thoại khi bắt đầu
        // Tìm InventorySystem và QuestDisplayUI trên người chơi
        inventorySystem = player.GetComponent<InventorySystem>();
        questUI = player.GetComponent<QuestDisplayUI>();
    }

    void Update()
    {
        // Kiểm tra khoảng cách giữa NPC và người chơi
        float distance = Vector3.Distance(player.transform.position, transform.position);
        isPlayerInRange = distance <= interactionDistance;

        // Nếu người chơi trong khoảng cách
        if (isPlayerInRange)
        {
            // Nhấn phím E để tương tác (bật UI và xử lý nhiệm vụ)
            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
            // Nhấn phím F để tắt UI
            else if (Input.GetKeyDown(KeyCode.F) && dialoguePanel.activeSelf)
            {
                dialoguePanel.SetActive(false);
            }
        }
        // Cập nhật tiến độ liên tục
        UpdateQuestProgress();
    }

    void Interact()
    {
        // Hiển thị UI và xử lý logic nhiệm vụ
        dialoguePanel.SetActive(true);

        if (!isQuestActive)
        {
            // Giao nhiệm vụ
            dialogueText.text = $"Xin chào! Hãy giúp tôi thu thập {requiredItems} quả {questItemName}. Bạn có muốn nhận nhiệm vụ này không? (Nhấn E để tiếp tục, F để từ chối)";
            isQuestActive = true;
        }
        else
        {
            // Kiểm tra tiến độ nhiệm vụ
            if (collectedItems >= requiredItems)
            {
                dialogueText.text = $"Cảm ơn bạn đã thu thập đủ {requiredItems} quả {questItemName}! Nhiệm vụ hoàn thành! (Nhấn E để nhận thưởng, F để ẩn)";
            }
            else
            {
                dialogueText.text = $"Bạn đã thu thập {collectedItems}/{requiredItems} quả {questItemName}. Hãy tiếp tục! (Nhấn F để ẩn)";
            }
        }
    }

    // Cập nhật tiến độ nhiệm vụ dựa trên inventory
    void UpdateQuestProgress()
    {
        if (inventorySystem != null)
        {
            collectedItems = inventorySystem.GetItemCount(questItemName);
        }
        else
        {
            collectedItems = 0;
        }
    }

    // Hoàn thành nhiệm vụ và cấp thưởng
    void CompleteQuest()
    {
        if (inventorySystem != null && collectedItems >= requiredItems)
        {
            // Giảm số lượng vật phẩm trong inventory
            inventorySystem.ReduceItemCount(questItemName, requiredItems);

            // Đặt lại trạng thái
            isQuestActive = false;
            collectedItems = 0;
            dialoguePanel.SetActive(false);
        }
    }

    // Thu thập vật phẩm (kích hoạt từ InventorySystem hoặc ItemWorld)
    public void CollectItem()
    {
        if (isQuestActive && inventorySystem != null)
        {
            collectedItems = inventorySystem.GetItemCount(questItemName);
        }
    }

    // Xử lý khi nhấn E trong trạng thái hoàn thành
    void OnInteractComplete()
    {
        if (isQuestActive && collectedItems >= requiredItems)
        {
            CompleteQuest();
        }
    }

    // Hiển thị vùng tương tác
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }

    // Ẩn hộp thoại khi người chơi rời khỏi vùng
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            dialoguePanel.SetActive(false);
            isPlayerInRange = false;
        }
    }

    // Cập nhật logic tương tác khi nhấn phím
    void OnGUI()
    {
        if (isPlayerInRange && dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            OnInteractComplete();
        }
    }
}