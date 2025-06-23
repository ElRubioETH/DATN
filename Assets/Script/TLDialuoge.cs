using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Dialogue Data")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            ShowNextLine();
        }
    }

    public void StartDialogue()
    {
        currentLineIndex = 0;
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogueLines[currentLineIndex];
    }

    void ShowNextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
        else
        {
            dialoguePanel.SetActive(false);
            isDialogueActive = false;
        }
    }
}
