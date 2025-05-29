using UnityEngine;
using UnityEngine.Events;

public class LeverController : MonoBehaviour
{
    public GameObject interactionPanel;
    public Animator leverAnimator;
    public UnityEvent onLeverPulled;

    [HideInInspector] public bool isOn = false;
    private bool isPlayerNear = false;

    [Header("Brake Mode (prevent turning off once On)")]
    public bool brake = false;

    private LeverManager leverManager;

    private void Start()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(false);

        // API mới
        leverManager = FindFirstObjectByType<LeverManager>();
        if (leverManager == null)
        {
            Debug.LogError("Không tìm thấy LeverManager trong scene!");
        }
    }

    private void Update()
    {
        if (isPlayerNear && interactionPanel != null)
        {
            Transform cam = Camera.main.transform;
            interactionPanel.transform.LookAt(cam);
            interactionPanel.transform.Rotate(0, 180f, 0);
        }

        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            TryToggleLever();
        }
    }

    public void TryToggleLever()
    {
        if (brake && isOn)
        {
            Debug.Log("Lever đang ON và brake được bật => không thể tắt.");
            return;
        }

        if (!isOn)
        {
            leverManager?.TurnOffAllExcept(this);
        }

        ToggleLever();
    }

    public void ToggleLever()
    {
        isOn = !isOn;

        if (leverAnimator != null)
        {
            leverAnimator.SetTrigger(isOn ? "On" : "Off");
        }

        onLeverPulled?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            ShowUI(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            ShowUI(false);
        }
    }

    // ✅ Thêm hàm ShowUI để tương tác với UI panel
    public void ShowUI(bool show)
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(show);
    }
}
