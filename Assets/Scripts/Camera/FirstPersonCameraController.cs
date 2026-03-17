using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform fpsCameraTarget;
    [SerializeField] private Transform playerRoot;
    
    private float lookSensitivity = 10f;
    private float minPitch = -30f;
    private float maxPitch = 60f;
    // private float minYaw = -90f;
    // private float maxYaw = 90f;
    
    private float yaw;    // rotația player-ului pe Y (stânga/dreapta)
    private float pitch;  // rotația camerei pe X (sus/jos)
    private Vector2 lookInput;   // vector2 de la mouse / stick
    
    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void Look(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
        // Debug.Log("FPS callback for Look: " + lookInput);
    }
    
    void Update()
    {
        if (lookInput.sqrMagnitude < 0.0001f)
            return;

        yaw   += lookInput.x * lookSensitivity * Time.deltaTime;
        pitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        
        // yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        
        // tracking target = pitch local
        fpsCameraTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        
        // player se aliniază instant pe yaw
        playerRoot.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
