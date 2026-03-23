using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class JetpackController : MonoBehaviour
{
    [Header("Jetpack")]
    public float linearForce = 20f;
    public float torqueForce = 5f;

    [Header("Center of Mass")]
    public Vector3 centerOfMass = new Vector3(0f, 1f, 0f);

    [Header("Effects")]
    public ParticleSystem waterParticles;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Camera")]
    public CameraController cameraController;

    [Header("Meter Reference")]
    public JetpackMeter jetpackMeter;

    private Rigidbody rb;
    private PlayerMovement playerMovement;
    private bool isJetpacking = false;
    private InputAction jetpackAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        rb.centerOfMass = centerOfMass;

        var playerInput = GetComponent<PlayerInput>();
        jetpackAction = playerInput.actions["Jetpack"];
    }

    void Update()
    {
        if (jetpackAction.WasPressedThisFrame())
        {
            if (!isJetpacking)
                EnterJetpackMode();
            else
                FireBurst();
        }

        if (!cameraController.IsThirdPerson() && isJetpacking)
            ExitJetpackMode();
    }

    void FireBurst()
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 cursorPos = Mouse.current.position.ReadValue();
        Vector2 offset = cursorPos - screenCenter;

        float scale = Screen.width * 0.15f / jetpackMeter.outerBrownRadius;
        float innerScreenRadius = jetpackMeter.innerBrownRadius * scale;
        float outerScreenRadius = jetpackMeter.outerBrownRadius * scale;

        float distance = offset.magnitude;

        if (distance <= innerScreenRadius)
        {
            rb.AddForce(-transform.forward * linearForce, ForceMode.Impulse);
        }
        else
        {
            Vector2 direction = offset.normalized;
            float clampedDistance = Mathf.Min(distance, outerScreenRadius);
            float intensity = (clampedDistance - innerScreenRadius) / (outerScreenRadius - innerScreenRadius);
            intensity = Mathf.Clamp01(intensity);

            float yTorque = direction.x * torqueForce * intensity;
            float xTorque = -direction.y * torqueForce * intensity;

            rb.AddTorque(new Vector3(xTorque, yTorque, 0f), ForceMode.Impulse);
        }

        if (waterParticles != null)
        {
            waterParticles.Stop();
            waterParticles.Play();
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

        if (waterParticles != null)
            waterParticles.Play();

        if (cameraController != null)
            cameraController.SetJetpackMode();
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