using UnityEngine;

public class ShopController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerController.Instance.SetCanOpenShop(true);
        GameUI.Instance.ShowNotification("Press E to open shop");
    }
    
    private void OnTriggerExit(Collider other)
    {
        PlayerController.Instance.SetCanOpenShop(false);
        
        if (other.GetComponent<PlayerController>())
            GameUI.Instance.CloseShopInterface();
    }
}
