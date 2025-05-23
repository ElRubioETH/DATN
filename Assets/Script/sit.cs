using UnityEngine;

public class TrainMovementTracker : MonoBehaviour
{
    public Vector3 Delta { get; private set; }
    private Vector3 lastPos;

    void Start() => lastPos = transform.position;

    void LateUpdate()
    {
        Delta = transform.position - lastPos;
        lastPos = transform.position;
    }
}
