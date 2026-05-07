using System;
using System.Collections;
using UnityEngine;

public class KnifeController : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float range = 0.5f;
    [SerializeField] private int damage = 50;
    [SerializeField] private float cooldown = 0.6f;

    private bool canAttack = true;

    private void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (!cameraTransform)
            cameraTransform = Camera.main ? Camera.main.transform : null;
    }

    public void Attack()
    {
        if (!canAttack) return;
        animator.SetTrigger("Attack");
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward,
                out RaycastHit hit, range, hitMask))
        {
            if (hit.collider.TryGetComponent<EnemyHealth>(out var enemy))
            {
                enemy.TakeDamage(damage, transform.forward);
            }
        }

        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }
}
