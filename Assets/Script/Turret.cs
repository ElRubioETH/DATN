using UnityEngine;

public class TurretController : MonoBehaviour
{
    public Transform basePivot; // Chân xoay ngang (yaw)
    public Transform headPivot; // Đầu xoay dọc (pitch)

    public float yawSpeed = 100f;
    public float pitchSpeed = 80f;
    public float minPitch = -30f;
    public float maxPitch = 45f;

    private bool isControlled = false;
    private float currentPitch = 0f;

    public void EnableControl()
    {
        isControlled = true;
        Cursor.lockState = CursorLockMode.Locked; // Khóa chuột vào giữa màn hình
        Cursor.visible = false;                       // Giữ chuột hiển thị
    }

    public void DisableControl()
    {
        isControlled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!isControlled) return;

        float mouseX = Input.GetAxis("Mouse X") * yawSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * pitchSpeed * Time.deltaTime;

        // Xoay ngang
        basePivot.Rotate(0f, mouseX, 0f);

        // Xoay dọc với giới hạn
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        headPivot.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }
}
