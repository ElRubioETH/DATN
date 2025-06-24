using UnityEngine;

public class TrainLeverController : MonoBehaviour
{
    public Lever startLever;
    public Lever reverseLever;
    public Lever brakeLever;

    public void ToggleStart()
    {
        if (reverseLever.isOn) return;
        bool newState = !startLever.isOn;
        startLever.SetLeverState(newState);
        if (newState) reverseLever.SetLeverState(false);
    }

    public void ToggleReverse()
    {
        if (startLever.isOn) return;
        bool newState = !reverseLever.isOn;
        reverseLever.SetLeverState(newState);
        if (newState) startLever.SetLeverState(false);
    }

    public void ToggleBrake()
    {
        bool newState = !brakeLever.isOn;
        brakeLever.SetLeverState(newState);
    }
}
