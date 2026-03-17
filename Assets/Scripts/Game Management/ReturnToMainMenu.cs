using UnityEngine;
using UnityEngine.InputSystem;

public class ReturnToMainMenu : MonoBehaviour
{
    private PlayerControlls playerControlls;

    private void Awake()
    {
        playerControlls = new PlayerControlls();
        playerControlls.MainMenu.ReturnToMainMenu.performed += OnReturn;
    }

    private void OnEnable()  => playerControlls.MainMenu.Enable();
    private void OnDisable() => playerControlls.MainMenu.Disable();

    private void OnDestroy()
    {
        if (playerControlls != null)
        {
            playerControlls.MainMenu.ReturnToMainMenu.performed -= OnReturn;
            playerControlls.Dispose();
        }
    }

    private void OnReturn(InputAction.CallbackContext _)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        SceneLoader.Instance.LoadMainMenuWithFade();
    }
}