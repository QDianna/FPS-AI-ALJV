using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleporter : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("Teleporter not working without Game Manager");
            return;
        }

        if (other.GetComponentInParent<PlayerController>() != null)
        {
            Debug.Log("Player teleporting");
            if (SceneManager.GetActiveScene().buildIndex == 3)
                GameManager.Instance.TeleportPlayer(4);
            else if (SceneManager.GetActiveScene().buildIndex == 4)
                GameManager.Instance.TeleportPlayer(3);
        }
        

    }
}
