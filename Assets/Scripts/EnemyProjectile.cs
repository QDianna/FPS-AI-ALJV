using Unity.VisualScripting;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody projectileRigidbody;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileDamage;
    [SerializeField] private ParticleSystem explosionParticles;
    
    private bool hasHit;
    
    void Start()
    {
        if (!projectileRigidbody)
            projectileRigidbody = GetComponent<Rigidbody>();

        if (!explosionParticles)
            explosionParticles = GetComponentInChildren<ParticleSystem>();
        
        explosionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        Destroy(gameObject, 10f);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        if (!other.TryGetComponent<PlayerController>(out var player))
        {
            return;
        }
        Debug.Log("Hit player");

        hasHit = true;
        
        projectileRigidbody.linearVelocity = Vector3.zero;
        projectileRigidbody.isKinematic = true;

        explosionParticles.Play();
        
        player.TakeDamage(projectileDamage, transform.forward);

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<SphereCollider>().enabled = false;
        
        Destroy(gameObject, 1f);
    }
    
    public void Fire(Transform firePoint, float damage)
    {   
        projectileRigidbody.linearVelocity = firePoint.forward * projectileSpeed;
        projectileDamage = damage;
    }
    
}
