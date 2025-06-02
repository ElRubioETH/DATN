using UnityEngine;
using UnityEngine.Splines;

public class TrainController : MonoBehaviour
{
    [Header("References")]
    public SplineContainer splineContainer;
    public Transform train;       // đầu tàu
    public Transform rearCar;     // toa tàu phía sau

    [Header("Movement Settings")]
    public float maxSpeed = 0.2f;
    public float acceleration = 0.05f;
    public float deceleration = 0.05f;
    public float rearCarDistance = 2f; // khoảng cách lùi sau (tính bằng spline %)

    [Header("Audio")]
    public AudioSource startOrReverseSource; // loop sound with increasing pitch
    public AudioSource brakeSource;          // loop sound
    public AudioSource runningSource;        // loop background sound

    private float currentSpeed = 0f;
    private float t = 0f;
    private int direction = 1;
    private bool isMoving = false;
    private bool isBraking = false;

    void Start()
    {
        // Đảm bảo brakeSource được loop
        brakeSource.loop = true;

        MoveTrain(train, t);

        float rearT = Mathf.Repeat(t - (rearCarDistance / splineContainer.CalculateLength()), 1f);
        MoveTrain(rearCar, rearT);
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
            {
                isMoving = false;

                startOrReverseSource.Stop();
                runningSource.Stop();
                brakeSource.Stop(); // Dừng âm thanh phanh khi tàu dừng
            }
        }

        // Di chuyển dọc theo spline
        t += currentSpeed * Time.deltaTime;
        t = Mathf.Repeat(t, 1f);

        MoveTrain(train, t);

        float rearT = Mathf.Repeat(t - (rearCarDistance / splineContainer.CalculateLength()), 1f);
        MoveTrain(rearCar, rearT);

        // Tăng dần pitch âm start theo tốc độ
        if (startOrReverseSource.isPlaying)
        {
            float speedRatio = Mathf.Abs(currentSpeed / maxSpeed);
            startOrReverseSource.pitch = Mathf.Lerp(0.3f, 1f, speedRatio);
        }
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

        // Tắt brake sound nếu đang phát
        if (brakeSource.isPlaying)
            brakeSource.Stop();

        // Bắt đầu âm thanh khởi động nếu chưa chạy
        if (!startOrReverseSource.isPlaying)
        {
            startOrReverseSource.pitch = 0.3f;
            startOrReverseSource.Play();
        }

        if (!runningSource.isPlaying)
            runningSource.Play();
    }

    public void BrakeTrain()
    {
        isBraking = true;

        // Bắt đầu âm brake loop
        if (!brakeSource.isPlaying)
        {
            brakeSource.loop = true;
            brakeSource.Play();
        }

        // Tắt âm start khi phanh
        if (startOrReverseSource.isPlaying)
            startOrReverseSource.Stop();
    }

    public void ReverseTrain()
    {
        if (!isMoving && Mathf.Approximately(currentSpeed, 0f))
        {
            direction *= -1;
            isMoving = true;
            isBraking = false;

            // Tắt brake sound nếu đang phát
            if (brakeSource.isPlaying)
                brakeSource.Stop();

            // Bắt đầu âm thanh khởi động nếu chưa chạy
            if (!startOrReverseSource.isPlaying)
            {
                startOrReverseSource.pitch = 0.3f;
                startOrReverseSource.Play();
            }

            if (!runningSource.isPlaying)
                runningSource.Play();
        }
    }
}
