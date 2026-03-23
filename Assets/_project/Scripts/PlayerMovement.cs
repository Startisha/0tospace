using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Fall Settings")]
    public float fallThreshold = 1f;

    [Header("Mouse Look")]
    public Transform povCamera;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded;
    private bool isFloating;
    private float leftGroundY;
    private bool wasGrounded;
    private float currentPitch = 0f;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        var playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (wasGrounded && !isGrounded)
            leftGroundY = transform.position.y;

        wasGrounded = isGrounded;

        bool isFalling = !isGrounded && rb.linearVelocity.y < -0.1f &&
                         (transform.position.y < leftGroundY - fallThreshold);

        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            leftGroundY = transform.position.y;
        }

        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsFloating", isFloating);
            animator.SetFloat("YVelocity", isFalling ? rb.linearVelocity.y : 0f);
        }

        MouseLook();
    }

    void MouseLook()
    {
        if (povCamera == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Rotate character left/right
        transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity * Time.deltaTime);

        // Rotate camera up/down
        currentPitch -= mouseDelta.y * mouseSensitivity * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, -maxLookAngle, maxLookAngle);
        povCamera.localEulerAngles = new Vector3(currentPitch, 0f, 0f);
    }

    void FixedUpdate()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isSprinting = sprintAction.ReadValue<float>() > 0.5f;

        Vector3 camForward = povCamera != null ? povCamera.forward : transform.forward;
        Vector3 camRight = povCamera != null ? povCamera.right : transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        if (moveDirection.magnitude > 0.1f)
        {
            rb.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rb.linearVelocity.y, moveDirection.z * currentSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        if (animator != null)
        {
            float speed = isSprinting && moveDirection.magnitude > 0.1f ? 1f :
                          moveDirection.magnitude > 0.1f ? 0.3f : 0f;
            animator.SetFloat("Speed", speed);
        }
    }

    public void SetFloating(bool floating)
    {
        isFloating = floating;
    }
}