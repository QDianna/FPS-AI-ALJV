using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CameraMode
{
    FirstPerson,
    ThirdPerson
}

public class CameraModeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FirstPersonCameraController firstPersonController;
    [SerializeField] private ThirdPersonCameraController thirdPersonController;
    [SerializeField] private CinemachineCamera firstPersonCamera;
    [SerializeField] private CinemachineCamera thirdPersonCamera;

    public CameraMode CurrentMode { get; private set; }

    private void Start()
    {
        SetMode(CameraMode.FirstPerson);
    }

    public void ToggleCameraMode(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        SetMode(CurrentMode == CameraMode.FirstPerson
            ? CameraMode.ThirdPerson
            : CameraMode.FirstPerson);
    }

    private void SetMode(CameraMode mode)
    {
        if (!firstPersonCamera || !thirdPersonCamera)
            return;
        CurrentMode = mode;

        firstPersonController.enabled = mode == CameraMode.FirstPerson;
        thirdPersonController.enabled = mode == CameraMode.ThirdPerson;
        
        firstPersonCamera.Priority = mode == CameraMode.FirstPerson ? 10 : 0;
        thirdPersonCamera.Priority = mode == CameraMode.ThirdPerson ? 10 : 0;
    }
}
