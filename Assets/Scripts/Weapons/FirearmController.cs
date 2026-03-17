using UnityEngine;

public class FirearmController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform firePoint;
    // [SerializeField] private GameObject projectilePrefab;

    public FirearmData data;
    
    public float bullets;
    
    public bool isEquipped;     // for animator
    private bool pendingFire;   // to sync with camera direction
    private float nextFireTimer;
    
    public void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogWarning("No main camera");
        
        if (!animator)
            animator = GetComponent<Animator>();
        
        animator.SetBool("isEquipped", isEquipped);

        bullets = data.bullets;
    }
    
    // called from input callback
    public void Fire()
    {
        pendingFire = true;
    }

    // called from late update so the muzzle position is more stable
    public void FirePending()
    {
        if (Time.time < nextFireTimer) return;
        
        if (data.bullets <= 0f)
        {
            GameUI.Instance.OnNoBullets();
            return;
        }

        nextFireTimer = Time.time + data.fireRate;
        bullets--;
        GameUI.Instance.UpdateBulletsUI();
        animator.SetTrigger("shoot");

        // todo bullet effect?
        // Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }

    void LateUpdate()
    {
        if (!isEquipped || !mainCamera)
            return;
        
        // visual alignment of gun
        transform.rotation = mainCamera.transform.rotation;

        // fire the pending fire
        if (pendingFire)
        {
            FirePending();
            pendingFire = false;
        }
        
        // animate inventory weapons
        animator.SetBool("isEquipped", isEquipped);
    }
    
}
