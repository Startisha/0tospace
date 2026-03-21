#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine;

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

    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded;
    private bool isFloating;
    private float leftGroundY;
    private bool wasGrounded;

#if ENABLE_INPUT_SYSTEM
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
#else
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
#endif

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Track when character leaves the ground
        if (wasGrounded && !isGrounded)
            leftGroundY = transform.position.y;

        wasGrounded = isGrounded;

        // Only show falling animation after dropping fallThreshold units
        bool isFalling = !isGrounded && rb.linearVelocity.y < -0.1f && 
                         (transform.position.y < leftGroundY - fallThreshold);

#if ENABLE_INPUT_SYSTEM
        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            leftGroundY = transform.position.y;
        }
#endif

        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsFloating", isFloating);
            animator.SetFloat("YVelocity", isFalling ? rb.linearVelocity.y : 0f);
        }
    }

    void FixedUpdate()
    {
#if ENABLE_INPUT_SYSTEM
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isSprinting = sprintAction.ReadValue<float>() > 0.5f;
#else
        Vector2 moveInput = Vector2.zero;
        bool isSprinting = false;
#endif

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        if (moveDirection.magnitude > 0.1f)
        {
            rb.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rb.linearVelocity.y, moveDirection.z * currentSpeed);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
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