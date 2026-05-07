using UnityEngine;

public class FirearmController : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject tracerPrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private LayerMask hitMask;
    
    public FirearmData data;
    
    private float nextFireTimer;
    public float bullets;
    
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip shootSound;
    
    public bool isEquipped;     // for animator
    
    public void Start()
    {
        if (!animator)
            animator = GetComponent<Animator>();
        
        animator.SetBool("isEquipped", isEquipped);

        bullets = data.bullets;
    }
    
    // called from input callback
    public void Fire(Vector3 origin, Vector3 direction)
    {
        if (Time.time < nextFireTimer) return;
        if (bullets <= 0f) return;

        nextFireTimer = Time.time + data.fireRate;
        bullets--;

        animator.SetTrigger("shoot");

        RaycastHit hit;
        
        bool hasHit = Physics.Raycast(origin, direction, out hit, data.fireRange, hitMask);
        Vector3 endPoint = hasHit ? hit.point : origin + direction * data.fireRange;

        // Debug.Log(hasHit ? $"Hit: {hit.collider.name}" : "Miss");
        
        // damage
        if (hasHit && hit.collider.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(data.damage, transform.forward);
            GetComponentInParent<EnemyCombatAI>()?.RegisterGaveDamage();

        }

        // tracer
        if (tracerPrefab)
        {
            Vector3 tracerDirection = (endPoint - firePoint.position).normalized;
            Vector3 tracerEndPoint = firePoint.position + tracerDirection * Vector3.Distance(firePoint.position, endPoint);
            
            var tracer = Instantiate(tracerPrefab, firePoint.position, firePoint.rotation);
            tracer.GetComponent<ProjectileVisual>().Init(tracerEndPoint);
        }

        // VFX + SFX
        if (muzzleFlash) muzzleFlash.Play();
        if (audioSource && shootSound) audioSource.PlayOneShot(shootSound);
    }

    void LateUpdate()
    {
        if (!isEquipped)
            return;
        
        // animate inventory weapons
        animator.SetBool("isEquipped", isEquipped);
    }
    
}
