using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
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
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
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
