using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Start()
    {
        if (slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 0;
        }
    }

    public void IncreaseSlider()
    {
        if (slider == null) return;

        slider.value = Mathf.Min(slider.value + 10, slider.maxValue);
    }
}
