using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ButtonAnimationController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Animator animator;

    [Header("Objects to disable on click")]
    public List<GameObject> objectsToDisable;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool("Hover", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool("Hover", false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        animator.SetTrigger("Click");

        // Disable each object in the list
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
