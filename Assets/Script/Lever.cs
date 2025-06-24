using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    public Animator anim;
    public bool isOn = false;

    public UnityEvent onLeverOn;
    public UnityEvent onLeverOff;

    public void SetLeverState(bool state)
    {
        if (isOn == state) return;

        isOn = state;

        if (anim != null)
            anim.SetTrigger(state ? "On" : "Off");

        if (state)
            onLeverOn?.Invoke();
        else
            onLeverOff?.Invoke();
    }
}
