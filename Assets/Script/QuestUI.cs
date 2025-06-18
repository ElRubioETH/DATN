using TMPro;
using UnityEngine;

public class QuestDisplayUI : MonoBehaviour
{
    // UI Elements
    public GameObject questPanel; // Panel chứa danh sách nhiệm vụ
    public TextMeshProUGUI questText; // Hiển thị danh sách nhiệm vụ
    private bool isQuestPanelOpen = false;

    // Tham chiếu đến NPCQuest
    public GameObject Player; // Tham chiếu đến GameObject chứa NPCQuest
    private NPCQuest npcQuest;

    void Start()
    {
        questPanel.SetActive(false); // Ẩn panel khi bắt đầu
        npcQuest = Player.GetComponent<NPCQuest>();
        UpdateQuestUI();
    }

    void Update()
    {
        // Phím Q để mở/đóng Quest UI
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleQuestPanel();
        }
        // Cập nhật UI nếu panel đang mở
        if (isQuestPanelOpen)
        {
            UpdateQuestUI();
        }
    }

    // Mở/Đóng panel nhiệm vụ
    void ToggleQuestPanel()
    {
        isQuestPanelOpen = !isQuestPanelOpen;
        questPanel.SetActive(isQuestPanelOpen);
    }

    // Cập nhật giao diện nhiệm vụ
    void UpdateQuestUI()
    {
        if (questText != null)
        {
            // Xóa nội dung cũ
            questText.text = "Nhiệm vụ Hiện Có:\n";

            // Hiển thị nhiệm vụ từ NPCQuest
            if (npcQuest != null && npcQuest.isQuestActive)
            {
                string questDescription = $"Thu thập {npcQuest.requiredItems} quả {npcQuest.questItemName} " +
                                         $"(Đã thu thập: {npcQuest.collectedItems}/{npcQuest.requiredItems})";
                questText.text += $"- {questDescription}\n";
            }
            else
            {
                questText.text += "Không có nhiệm vụ nào.\n";
            }
        }
    }
}