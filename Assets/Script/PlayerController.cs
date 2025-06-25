using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private GameObject crosshairDot;
    [SerializeField] private Sprite defaultCrosshair;
    [SerializeField] private Sprite handCrosshair;

    private Camera cam;
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -20f;

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 100f;
    public float lookXLimit = 90f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Animator anim;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        cam = playerCamera.GetComponent<Camera>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleInteractionCheck();
    }
    void HandleInteractionCheck()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
        {
            // Nếu vật thể đủ gần và có tag tương tác
            if (hit.collider.CompareTag("Interactable"))
            {
                crosshairDot.GetComponent<UnityEngine.UI.Image>().sprite = handCrosshair;
                return;
            }
        }

        // Nếu không trúng gì thì để lại crosshair mặc định
        crosshairDot.GetComponent<UnityEngine.UI.Image>().sprite = defaultCrosshair;
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -lookXLimit, lookXLimit);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // Ground Check
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckDistance, groundMask);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        bool isMoving = move != Vector3.zero;

        // Running check
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        move = move.normalized * currentSpeed;

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        // Final move
        Vector3 finalVelocity = move;
        finalVelocity.y = velocity.y;
        controller.Move(finalVelocity * Time.deltaTime);

        // 🔥 Animator Update
        if (anim != null)
        {
            float animationSpeed = isMoving ? (isRunning ? 1f : 0.5f) : 0f;
            anim.SetFloat("Speed", animationSpeed);
            anim.SetBool("isRunning", isRunning);
        }
    }
}
