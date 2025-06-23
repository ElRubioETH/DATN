using UnityEngine;
using UnityEngine.Playables;

// Dùng PlayableDirector.stopped để gọi StartDialogue
public class TimelineEndTrigger : MonoBehaviour
{
    public PlayableDirector timeline;
    public DialogueManager dialogueManager;

    void Start()
    {
        timeline.stopped += OnTimelineEnd;
    }

    void OnTimelineEnd(PlayableDirector pd)
    {
        dialogueManager.StartDialogue();
    }
}
