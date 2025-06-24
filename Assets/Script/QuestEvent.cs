using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public GameObject questPanel;
    public DialogueManager secondDialogue;
    public GameObject triggerZone; // vùng kích hoạt nhiệm vụ

    void Start()
    {
        questPanel.SetActive(false);
        secondDialogue.gameObject.SetActive(false);
        triggerZone.SetActive(false);

        // Bắt đầu khi timeline kết thúc
        dialogueManager.onDialogueFinished.AddListener(OnFirstDialogueEnd);
    }

    public void OnFirstDialogueEnd()
    {
        questPanel.SetActive(true);      // Hiện panel nhiệm vụ
        triggerZone.SetActive(true);     // Bật vùng trigger
    }

    public void OnEnterTriggerZone()
    {
        questPanel.SetActive(false);     // Tắt nhiệm vụ
        secondDialogue.gameObject.SetActive(true);
        secondDialogue.StartDialogue();  // Mở đoạn thoại tiếp theo
    }
}
