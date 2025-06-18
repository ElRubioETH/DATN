using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonTextColorChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text buttonText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.cyan;

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = normalColor;
    }
}
