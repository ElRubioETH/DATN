using UnityEngine;

public class TurretZoneTrigger : MonoBehaviour
{
    private GameObject player;
    private bool playerInZone = false;
    private bool isControlling = false;

    private TurretController turret;
    private Transform sitPosition;
    private Transform gunTransform;

    private Camera playerCamera;
    [SerializeField] private Camera turretCamera;

    [SerializeField] private UICameraSwitcher uiCameraSwitcher; // THÊM BIẾN NÀY

    void Start()
    {
        // Tìm turret qua tag (phải gắn tag "Turret" trong Inspector)
        turret = GameObject.FindWithTag("Turret")?.GetComponent<TurretController>();
        if (turret == null)
        {
            Debug.LogError("Không tìm thấy TurretController qua tag 'Turret'!");
            return;
        }

        // Tìm SitPosition trong scene (phải đặt tên đúng "SitPosition")
        GameObject sitObj = GameObject.Find("SitPosition");
        if (sitObj == null)
        {
            Debug.LogError("Không tìm thấy GameObject SitPosition trong scene!");
            return;
        }
        sitPosition = sitObj.transform;

        // Tìm Gun để gắn player vào
        gunTransform = turret.transform.Find("BasePivot/HeadPivot/Gun");
        if (gunTransform == null)
        {
            Debug.LogError("Không tìm thấy gunTransform trong Turret!");
        }

        // Đảm bảo TurretCamera đang tắt lúc đầu
        if (turretCamera != null)
            turretCamera.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerInZone = true;

            playerCamera = player.GetComponentInChildren<Camera>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
            playerInZone = false;
        }
    }

    void Update()
    {
        if (!playerInZone || player == null || turret == null) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isControlling)
                EnterTurret();
            else
                ExitTurret();
        }
    }

    void EnterTurret()
    {
        if (player == null || sitPosition == null) return;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.SetPositionAndRotation(sitPosition.position, sitPosition.rotation);
            cc.enabled = true;
        }

        player.transform.SetParent(sitPosition);

        var fpc = player.GetComponent<FirstPersonController>();
        if (fpc != null)
            fpc.enabled = false;

        if (playerCamera != null) playerCamera.enabled = false;
        if (turretCamera != null) turretCamera.enabled = true;

        // Gán camera UI cho turret
        if (uiCameraSwitcher != null)
            uiCameraSwitcher.SetCanvasCamera(turretCamera);

        turret.EnableControl();
        isControlling = true;
    }

    void ExitTurret()
    {
        if (player == null) return;

        player.transform.SetParent(null);

        var fpc = player.GetComponent<FirstPersonController>();
        if (fpc != null)
            fpc.enabled = true;

        if (playerCamera != null) playerCamera.enabled = true;
        if (turretCamera != null) turretCamera.enabled = false;

        // Gán lại camera UI về player
        if (uiCameraSwitcher != null)
            uiCameraSwitcher.SetCanvasCamera(playerCamera);

        turret.DisableControl();
        isControlling = false;
    }
}
