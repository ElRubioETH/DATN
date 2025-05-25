using UnityEngine;

public class TurretShooting : MonoBehaviour
{
    public Transform firePoint1;
    public Transform firePoint2;
    public GameObject bulletPrefab;
    public float fireForce = 500f;
    public float fireRate = 0.2f;
    public TurretController controller;
    public GameObject muzzleEffectPrefab;
    public Transform effectPoint1;
    public Transform effectPoint2;

    public AudioClip shootSound; // Âm thanh bắn
    private AudioSource audioSource;

    private float fireCooldown = 0f;
    private bool useFirstFirePoint = true;

    void Start()
    {
        controller = GetComponent<TurretController>();

        // Tự động thêm AudioSource nếu chưa có
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (controller == null || !controller.enabled || !controller.IsControlled()) return;

        fireCooldown -= Time.deltaTime;

        if (Input.GetMouseButton(0) && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = fireRate;
        }
    }

    void Shoot()
    {
        Transform currentFirePoint = useFirstFirePoint ? firePoint1 : firePoint2;
        Transform currentEffectPoint = useFirstFirePoint ? effectPoint1 : effectPoint2;

        // Bắn đạn
        GameObject bullet = Instantiate(bulletPrefab, currentFirePoint.position, currentFirePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(currentFirePoint.forward * fireForce, ForceMode.Impulse);

        // Hiệu ứng bắn
        if (muzzleEffectPrefab != null)
        {
            GameObject muzzleInstance = Instantiate(muzzleEffectPrefab, currentEffectPoint.position, currentEffectPoint.rotation);
            Destroy(muzzleInstance, 0.1f);
        }

        // Phát âm thanh bắn
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        useFirstFirePoint = !useFirstFirePoint;
    }
}
