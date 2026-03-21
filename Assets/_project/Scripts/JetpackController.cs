#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine;
using System.Collections;

public class JetpackController : MonoBehaviour
{
    [Header("Jetpack")]
    public float recoilForce = 10f;
    public ForceMode forceMode = ForceMode.Force;
    public Transform muzzle;
    public ParticleSystem waterParticles;

    [Header("Mouse Aim")]
    public float mouseSensitivity = 2f;

    [Header("Center of Mass")]
    public Vector3 centerOfMass = new Vector3(0f, 1f, 0f);

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private PlayerMovement playerMovement;
    private bool isJetpacking = false;
    private float yaw = 0f;
    private float pitch = 0f;

#if ENABLE_INPUT_SYSTEM
    private InputAction jetpackAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        rb.centerOfMass = centerOfMass;

        var playerInput = GetComponent<PlayerInput>();
        jetpackAction = playerInput.actions["Jetpack"];
    }
#else
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        rb.centerOfMass = centerOfMass;
    }
#endif

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        bool holdingJetpack = jetpackAction.IsPressed();
#else
        bool holdingJetpack = false;
#endif

        if (holdingJetpack && !isJetpacking)
            EnterJetpackMode();
        else if (!holdingJetpack && isJetpacking)
            ExitJetpackMode();

        if (isJetpacking && muzzle != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            yaw += mouseDelta.x * mouseSensitivity * 0.05f;
            pitch -= mouseDelta.y * mouseSensitivity * 0.05f;
            pitch = Mathf.Clamp(pitch, -80f, 80f);

            muzzle.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    void FixedUpdate()
    {
        if (!isJetpacking) return;

        rb.WakeUp();

        if (muzzle != null)
        {
            Vector3 recoilDirection = -muzzle.forward;
            rb.AddForce(recoilDirection * recoilForce, forceMode);
        }
    }

    void EnterJetpackMode()
    {
        isJetpacking = true;

        playerMovement.enabled = false;

        Physics.gravity = Vector3.zero;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        rb.constraints = RigidbodyConstraints.None;

        yaw = transform.eulerAngles.y;
        pitch = 0f;

        if (muzzle != null)
            muzzle.rotation = Quaternion.Euler(pitch, yaw, 0f);

        if (waterParticles != null)
            waterParticles.Play();
    }

    void ExitJetpackMode()
    {
        isJetpacking = false;

        playerMovement.enabled = true;

        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (waterParticles != null)
            waterParticles.Stop();

        StartCoroutine(ResetOnLand());
    }

    IEnumerator ResetOnLand()
    {
        while (!Physics.CheckSphere(groundCheck.position, groundDistance, groundMask))
        {
            yield return null;
        }

        Physics.gravity = new Vector3(0, -9.81f, 0);
        rb.linearDamping = 0f;
        rb.linearVelocity = Vector3.zero;
    }
}