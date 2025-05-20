using UnityEngine;

public class TurretZoneTrigger : MonoBehaviour
{
    private GameObject player;
    private bool playerInZone = false;
    private bool isControlling = false;

    private TurretController turret;
    private Transform sitPosition;

    private Transform gunTransform;

    void Start()
    {
        turret = GetComponentInParent<TurretController>();
        if (turret == null)
            Debug.LogError("Không tìm thấy TurretController trong parent!");

        sitPosition = turret.transform.Find("SitPosition");
        if (sitPosition == null)
            Debug.LogError("Không tìm thấy SitPosition trong turret!");

        gunTransform = turret.transform.Find("BasePivot/HeadPivot/Gun");
        if (gunTransform == null)
            Debug.LogError("Không tìm thấy Gun trong turret!");
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerInZone = true;
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
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.SetPositionAndRotation(sitPosition.position, sitPosition.rotation);
            cc.enabled = true;
        }

        player.transform.SetParent(gunTransform); // gắn vào gun thay vì turret

        var fpc = player.GetComponent<FirstPersonController>();
        if (fpc != null)
            fpc.enabled = false;

        turret.EnableControl();
        isControlling = true;
    }

    void ExitTurret()
    {
        if (player == null || turret == null) return;

        player.transform.SetParent(null); // Gỡ khỏi turret

        FirstPersonController fpc = player.GetComponent<FirstPersonController>();
        if (fpc != null)
            fpc.enabled = true;

        turret.DisableControl();
        isControlling = false;
    }
}
