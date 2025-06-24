using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public DialogueManager dialogueManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TriggerZone"))
        {
            dialogueManager.StartDialogue();
        }
    }
}
