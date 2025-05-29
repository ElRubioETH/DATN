using UnityEngine;
using System.Collections.Generic;

public class LeverManager : MonoBehaviour
{
    private List<LeverController> allLevers;

    private void Awake()
    {
        // Sử dụng API mới, không sắp xếp thứ tự
        allLevers = new List<LeverController>(FindObjectsByType<LeverController>(FindObjectsSortMode.None));
    }

    public void TurnOffAllExcept(LeverController activeLever)
    {
        foreach (var lever in allLevers)
        {
            if (lever != activeLever && lever.isOn && !lever.brake)
            {
                lever.ToggleLever();
            }
        }
    }
}
