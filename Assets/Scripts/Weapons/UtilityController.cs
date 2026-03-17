using UnityEngine;

public class UtilityController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform throwPoint;
    
    public UtilityData data;
    
    public float throwForce = 10f;
    
    public bool isEquipped;     // for animator
    private float nextThrowTimer;
    
    public void Start()
    {
    }
    public void Throw()
    {
        if (Time.time < nextThrowTimer)
            return;
        
        Inventory.Instance.HasItem(data, 1);
        
        nextThrowTimer = Time.time + data.throwCooldown;
        Debug.Log("Firing grenade...");
        
        Inventory.Instance.RemoveItem(data, 1);
        
        GameUI.Instance.UpdateBulletsUI();
        
        // todo instantiate utility and throw it and implement effect on enemies
        
        /*
        GameObject grenade = Instantiate(
            data.utilityPrefab,
            throwPoint.position,
            throwPoint.rotation
        );
        
        CharacterController cc = PlayerController.Instance.GetComponent<CharacterController>();
        Vector3 playerVelocity = new Vector3(
            cc.velocity.x,
            0f,
            cc.velocity.z
        );
        
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.linearVelocity =
            Camera.main.transform.forward * throwForce +
            playerVelocity;
        */
    }
}
