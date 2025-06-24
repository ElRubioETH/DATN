using UnityEngine;

public class QuestTriggerZone : MonoBehaviour
{
    public DialogueManager dialogueManager;
    private bool isInZone = false;


    void Update()
    {
        if (isInZone && Input.GetKeyDown(KeyCode.F))
        {
            dialogueManager.StartDialogue();
            isInZone = false; // Ngăn spam F liên tục (tuỳ bạn, có thể bỏ)
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("QuestTriggerZone"))
        {
            isInZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("QuestTriggerZone"))
        {
            isInZone = false;
        }
    }
}
