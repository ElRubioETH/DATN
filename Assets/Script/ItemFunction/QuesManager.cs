using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public GameObject questPanel;
    public string questItemName = "Apple";
    public int requiredAmount = 10;
    private int collectedAmount = 0;

    public Inventory inventory;

    public void ActivateQuest()
    {
        collectedAmount = 0;
        questPanel.SetActive(true);
    }

    public void TryCompleteQuest()
    {
        int count = inventory.GetItemCount(questItemName);
        if (count >= requiredAmount)
        {
            inventory.RemoveItem(questItemName, requiredAmount);
            Debug.Log("Nhiệm vụ hoàn thành! 🎉");
            questPanel.SetActive(false);
        }
        else
        {
            Debug.Log("Chưa đủ táo để trả nhiệm vụ.");
        }
    }

    public void OnItemCollected(string itemName)
    {
        if (itemName == questItemName)
        {
            collectedAmount++;
            Debug.Log("Đã nhặt " + collectedAmount + "/" + requiredAmount + " " + questItemName);
        }
    }
}
