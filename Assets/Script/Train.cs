using UnityEngine;
using UnityEngine.Splines;

public class TrainController : MonoBehaviour
{
    [Header("References")]
    public SplineContainer splineContainer;
    public Transform train; // đầu tàu

    [Header("Movement Settings")]
    public float maxSpeed = 0.2f;
    public float acceleration = 0.05f;
    public float deceleration = 0.05f;

    [Header("Startup Settings")]
    [SerializeField] private bool startOnAwake = false;

    private float currentSpeed = 0f;
    private float t = 0f;
    private int direction = 1;
    private bool isMoving = false;
    private bool isBraking = false;

    void Start()
    {
        // Gắn vị trí đầu tiên cho đầu tàu
        MoveTrain(train, t);

        // Bắt đầu tự động nếu được bật
        if (startOnAwake)
        {
            StartTrain();
        }
    }

    void Update()
    {
        if (!isMoving) return;

        // Tăng tốc hoặc phanh
        if (!isBraking)
        {
            currentSpeed += acceleration * Time.deltaTime * direction;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            if (Mathf.Approximately(currentSpeed, 0f))
                isMoving = false;
        }

        // Di chuyển đầu tàu dọc theo spline
        t += currentSpeed * Time.deltaTime;
        t = Mathf.Repeat(t, 1f);

        MoveTrain(train, t);
    }

    private void MoveTrain(Transform target, float tValue)
    {
        Vector3 position = splineContainer.EvaluatePosition(tValue);
        Vector3 tangent = splineContainer.EvaluateTangent(tValue);

        if (position != Vector3.positiveInfinity && tangent != Vector3.zero)
        {
            target.position = position;
            Quaternion rotation = Quaternion.LookRotation(tangent, Vector3.up) * Quaternion.Euler(0, 90f, 0);
            target.rotation = rotation;
        }
    }

    public void StartTrain()
    {
        isMoving = true;
        isBraking = false;
        direction = 1;
    }

    public void BrakeTrain()
    {
        isBraking = true;
    }

    public void ReverseTrain()
    {
        if (!isMoving && Mathf.Approximately(currentSpeed, 0f))
        {
            direction *= -1;
            isMoving = true;
            isBraking = false;
        }
    }
}
