using UnityEngine;

public enum AttackActionType
{
    None,
    ShootStanding,
    StrafeLeftShoot,
    StrafeRightShoot,
    StrafeLeft,
    StrafeRight,
    BackOff
}
    
public struct CombatState
{
    public float distanceToPlayer;
    public float health;
    public bool playerVisible;
    public bool gaveDamage; // hit enemy recently
    public bool tookDamage; // was hit by enemy recently
}

public class EnemyCombatAI : MonoBehaviour, ICombatActions
{
    [Header("References")]
    [SerializeField] private FirearmController firearm;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private WeaponAlign weaponAlign;

    private float fireTimer;

    public float gaveDamageTimer;
    public float tookDamageTimer;

    private bool isShooting;
    private Vector3 currentTarget;

    void Start()
    {
        if (!PlayerController.Instance)
            Debug.Log("Enemy Controller: player instance not found");
        else
            aimTarget = PlayerController.Instance.transform;
    }

    void Update()
    {
        gaveDamageTimer -= Time.deltaTime;
        tookDamageTimer -= Time.deltaTime;

        HandleShooting();
    }

    // ================= CORE =================

    private void Aim(Vector3 dir)
    {
        weaponAlign.SetAimDirection(dir);
    }

    private void HandleShooting()
    {
        if (!isShooting)
            return;

        Vector3 dir = (aimTarget.position - firearm.transform.position).normalized;

        Aim(dir);

        fireTimer += Time.deltaTime;

        if (fireTimer >= firearm.data.fireRate)
        {
            firearm.Fire(transform.position, dir);
            fireTimer = 0f;
        }
    }

    // ================= PUBLIC API =================

    public void StartShooting(Vector3 target)
    {
        currentTarget = target;
        isShooting = true;
    }

    public void StopShooting()
    {
        isShooting = false;
        fireTimer = 0f;
    }

    public void RegisterTookDamage()
    {
        tookDamageTimer = 2f;
    }

    public void RegisterGaveDamage()
    {
        gaveDamageTimer = 2f;
    }

    public bool HasLineOfSight()
    {
        Vector3 origin = transform.position;
        Vector3 direction = (aimTarget.position - origin).normalized;
        float distance = Vector3.Distance(origin, aimTarget.position);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            return hit.transform == aimTarget;
        }

        return false;
    }

    // ================= LEGACY (optional) =================
    // păstrate doar pentru debug / compatibilitate

    public void ShootStanding(Vector3 target)
    {
        StartShooting(target);
    }

    public void StrafeLeftShoot(Vector3 target)
    {
        StartShooting(target);
    }

    public void StrafeRightShoot(Vector3 target)
    {
        StartShooting(target);
    }

    public void StrafeLeft() { }
    public void StrafeRight() { }
    public void BackOff() { }

    // ================= DEBUG =================

    [ContextMenu("Start Shooting")]
    private void Debug_StartShooting()
    {
        StartShooting(aimTarget.position);
    }

    [ContextMenu("Stop Shooting")]
    private void Debug_StopShooting()
    {
        StopShooting();
    }
}

