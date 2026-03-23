using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Cameras")]
    public Camera povCamera;
    public GameObject jetpackCamera;

    [Header("Jetpack Meter")]
    public GameObject jetpackMeter;

    private bool isThirdPerson = false;
    private InputAction shiftAction;

    void Awake()
    {
        var playerInput = GetComponent<PlayerInput>();
        shiftAction = playerInput.actions["Sprint"];

        if (jetpackMeter != null)
            jetpackMeter.SetActive(false);
    }

    void Update()
    {
        if (shiftAction.WasPressedThisFrame() && isThirdPerson)
            SetWalkMode();
    }

    public void SetWalkMode()
    {
        isThirdPerson = false;
        povCamera.gameObject.SetActive(true);
        jetpackCamera.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (jetpackMeter != null)
            jetpackMeter.SetActive(false);
    }

    public void SetJetpackMode()
    {
        isThirdPerson = true;
        povCamera.gameObject.SetActive(false);
        jetpackCamera.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (jetpackMeter != null)
            jetpackMeter.SetActive(true);
    }

    public bool IsThirdPerson() => isThirdPerson;
}