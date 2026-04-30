using UnityEngine;

public enum AttackActionType
{
    ShootStanding,
    StrafeLeftShoot,
    StrafeRightShoot,
    PushForwardShoot,
    BackOffShoot,
    
    MaintainDistance,
    DodgeLeft,
    DodgeRight,
    HoldPosition
}
    
public struct CombatState
{
    public float distanceToPlayer;
    public bool gaveDamage; // hit enemy recently
    public bool tookDamage; // was hit by enemy recently
    public float health;
    public bool playerVisible;
}

public class EnemyCombatAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform aimTarget;
    [SerializeField] private FirearmController firearm;
    [SerializeField] private FirearmAimer firearmAimer;
    [SerializeField] private EnemyMovementAI movement;
    [SerializeField] private EnemyHealth healthSystem;

    [Header("Timing")]
    [SerializeField] private float decisionInterval = 0.5f;
    [SerializeField] private float actionDuration = 0.4f;

    private float decisionTimer;
    private float fireTimer;

    private bool isShooting;

    public float gaveDamageTimer;
    public float tookDamageTimer;
    
    [SerializeField] private bool useDebug = false;
    
    void Start()
    {
        aimTarget = PlayerController.Instance.transform;
    }

    void Update()
    {
        gaveDamageTimer -= Time.deltaTime;
        tookDamageTimer -= Time.deltaTime;
        
        HandleShooting();
        
        if (useDebug)
            DebugInput();
    }

    // ------------------------------ MAIN ------------------------------ //

    public void TickCombat()
    {
        if (useDebug)
            return;
        
        decisionTimer -= Time.deltaTime;

        if (decisionTimer > 0f)
            return;

        decisionTimer = decisionInterval;

        CombatState state = BuildState();
        AttackActionType action = DecideAction(state);

        ExecuteAction(action);
    }

    public void StopAllCombat()
    {
        StopShooting();
    }

    // ------------------------------ DECISION ------------------------------ //

    private CombatState BuildState()
    {
        float dist = Vector3.Distance(transform.position, aimTarget.position);

        return new CombatState
        {
            distanceToPlayer = dist,
            health = healthSystem.health,
            playerVisible = HasLineOfSight(),
            gaveDamage = gaveDamageTimer > 0,
            tookDamage = tookDamageTimer > 0
        };
    }

    private AttackActionType DecideAction(CombatState state)
    {
        if (state.distanceToPlayer < 5f)
            return AttackActionType.BackOffShoot;

        if (state.tookDamage)
        {
            tookDamageTimer = 0f;

            if (movement.CanMove(movement.GetRight(), 1f))
                return AttackActionType.StrafeRightShoot;

            if (movement.CanMove(-movement.GetRight(), 1f))
                return AttackActionType.StrafeLeftShoot;

            return AttackActionType.BackOffShoot;
        }

        if (state.gaveDamage)
        {
            gaveDamageTimer = 0f;
            return AttackActionType.ShootStanding;
        }

        return AttackActionType.ShootStanding;
    }

    // ------------------------------ EXECUTION ------------------------------ //

    private void ExecuteAction(AttackActionType action)
    {
        switch (action)
        {
            case AttackActionType.ShootStanding:
                movement.HoldPosition();
                StartShooting();
                break;

            case AttackActionType.StrafeLeftShoot:
                movement.StrafeLeft(actionDuration);
                StartShooting();
                break;

            case AttackActionType.StrafeRightShoot:
                movement.StrafeRight(actionDuration);
                StartShooting();
                break;

            case AttackActionType.PushForwardShoot:
                movement.PushForward(actionDuration);
                StartShooting();
                break;

            case AttackActionType.BackOffShoot:
                movement.BackOff(actionDuration);
                StartShooting();
                break;

            case AttackActionType.MaintainDistance:
                movement.MaintainDistance(5f, 8f, actionDuration);
                StartShooting();
                break;

            case AttackActionType.DodgeLeft:
                movement.DodgeLeft(actionDuration);
                StopShooting(); // dodge = fără shoot
                break;

            case AttackActionType.DodgeRight:
                movement.DodgeRight(actionDuration);
                StopShooting();
                break;

            case AttackActionType.HoldPosition:
                movement.HoldPosition();
                StopShooting();
                break;
        }
    }

    //  ------------------------------SHOOT ------------------------------ //

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

    private void StartShooting()
    {
        isShooting = true;
    }

    private void StopShooting()
    {
        isShooting = false;
        fireTimer = 0f;
        // firearmAimer.SetAimDirection(transform.forward);
    }

    private void Aim(Vector3 dir)
    {
        // aim weapon
        firearmAimer.SetAimDirection(dir);

        // aim body
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
    }

    // ------------------------------ UTIL ------------------------------ //

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
        Vector3 dir = (aimTarget.position - origin).normalized;
        float dist = Vector3.Distance(origin, aimTarget.position);

        return Physics.Raycast(origin, dir, out RaycastHit hit, dist)
               && hit.transform == aimTarget;
    }
    
    private void ExecuteDebug(AttackActionType action)
    {
        Debug.Log($"Action: {action}");
        ExecuteAction(action);
    }

    private void DebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ExecuteDebug(AttackActionType.ShootStanding);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            ExecuteDebug(AttackActionType.StrafeLeftShoot);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            ExecuteDebug(AttackActionType.StrafeRightShoot);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            ExecuteDebug(AttackActionType.PushForwardShoot);

        if (Input.GetKeyDown(KeyCode.Alpha5))
            ExecuteDebug(AttackActionType.BackOffShoot);

        if (Input.GetKeyDown(KeyCode.Alpha6))
            ExecuteDebug(AttackActionType.MaintainDistance);

        if (Input.GetKeyDown(KeyCode.Alpha7))
            ExecuteDebug(AttackActionType.DodgeLeft);

        if (Input.GetKeyDown(KeyCode.Alpha8))
            ExecuteDebug(AttackActionType.DodgeRight);

        if (Input.GetKeyDown(KeyCode.Alpha9))
            ExecuteDebug(AttackActionType.HoldPosition);
    }
}

