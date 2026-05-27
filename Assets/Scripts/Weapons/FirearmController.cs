using System;
using UnityEngine;

public class FirearmController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;

    [Header("Weapon")]
    [SerializeField] public FirearmData data;

    [SerializeField] private LayerMask hitMask;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem muzzleFlash;

    [SerializeField] private Animator animator;

    [SerializeField] private GameObject tracerPrefab;

    [SerializeField] private AudioClip shootSound;

    [SerializeField] private AudioSource audioSource;

    [Header("State")]
    [SerializeField] public bool isEquipped;

    private float nextFireTimer;

    public event Action<float> OnDamageDealt;
    public event Action OnShotsFired; 

    void Start()
    {
        if (!animator)
            animator = GetComponent<Animator>();

        animator.SetBool("isEquipped", isEquipped);
    }

    public bool CanFire()
    {
        return Time.time >= nextFireTimer;
    }

    public bool Fire(Vector3 direction)
    {
        if (!CanFire())
            return false;

        nextFireTimer =
            Time.time + data.fireRate;

        direction.Normalize();

        animator.SetTrigger("shoot");

        OnShotsFired?.Invoke();
        
        RaycastHit hit;

        bool hasHitSomething =
            Physics.Raycast(
                firePoint.position,
                direction,
                out hit,
                30f,
                hitMask
            );

        Vector3 endPoint =
            hasHitSomething
                ? hit.point
                : firePoint.position + direction * 30f;

        bool hasHit = false;
        if (hasHitSomething)
        {
            if (hit.collider.TryGetComponent<IDamageable>(
                    out var damageable))
            {
                damageable.TakeDamage(
                    data.damage,
                    direction
                );

                OnDamageDealt?.Invoke(data.damage);

                hasHit = true;
            }
        }

        SpawnTracer(direction, endPoint);
        PlayEffects();
        
        return hasHit;
    }

    private void SpawnTracer(
        Vector3 direction,
        Vector3 endPoint)
    {
        if (!tracerPrefab)
            return;

        var tracer =
            Instantiate(
                tracerPrefab,
                firePoint.position,
                Quaternion.LookRotation(direction)
            );

        tracer
            .GetComponent<ProjectileVisual>()
            .Init(endPoint);
    }

    private void PlayEffects()
    {
        if (muzzleFlash)
            muzzleFlash.Play();

        if (audioSource && shootSound)
            audioSource.PlayOneShot(shootSound);
    }

    void LateUpdate()
    {
        if (!isEquipped)
            return;

        animator.SetBool("isEquipped", isEquipped);
    }
}