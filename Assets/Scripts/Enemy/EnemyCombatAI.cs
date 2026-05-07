using System.Collections.Generic;
using UnityEngine;

public enum AttackActionType
{
    StrafeLeftShoot,
    StrafeRightShoot,
    PushForwardShoot,
    BackOffShoot,
    MaintainDistanceShoot,
    HoldPositionShoot
}

enum DistanceState { Close, Medium, Far }
enum HealthState { Low, Medium, High }

struct RLState
{
    public DistanceState distance;
    public HealthState health;
    public HealthState enemyHealth;
    public bool tookDamage;

    public override int GetHashCode()
    {
        return  (tookDamage ? 1 : 0) + 
                (int)distance * 10 +
                (int)health * 100 +
                (int)enemyHealth * 1000;
    }
}


public class EnemyCombatAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform aimTarget;
    [SerializeField] private FirearmController firearm;
    [SerializeField] private FirearmAimer firearmAimer;
    [SerializeField] private EnemyBehaviour behaviour;
    [SerializeField] private EnemyMovementAI movement;
    [SerializeField] private EnemyHealth healthSystem;

    [Header("Timing parameters")]
    [SerializeField] private float decisionInterval = 0.5f;
    [SerializeField] private float fireCooldown = 0.4f;

    [Header("RL parameters")]
    [SerializeField] private float learningRate = 0.3f;
    [SerializeField] private float discount = 0.7f;
    [SerializeField] private float epsilon = 0.4f;

    [Header("Debug")]
    [SerializeField] private bool debugCombat = true;

    [Header("Timers")]
    public float tookDamageTimer;
    public float gaveDamageTimer;
    private float decisionTimer;
    private float fireTimer;

    // flags
    private bool isInCombat;
    private bool isShooting;
    
    // RL variables
    private Dictionary<int, float[]> qTable = new();
    private AttackActionType[] actions;

    private RLState prevState;
    private int prevActionIndex;
    private bool hasPrev;

    
    void Start()
    {
        actions = (AttackActionType[])System.Enum.GetValues(typeof(AttackActionType));
    }

    void Update()
    {
        UpdateTimers();

        if (!isInCombat)
            return;

        MaintainAim();
        HandleShooting();
    }

    // ----------------------------------- COMBAT ----------------------------------- //

    public void EnterCombat()
    {
        isInCombat = true;
        decisionTimer = 0f;
        hasPrev = false;
    }

    public void ExitCombat()
    {
        isInCombat = false;
        StopShooting();
        hasPrev = false;
    }

    public void TickCombat()
    {
        if (!isInCombat)
            return;

        decisionTimer -= Time.deltaTime;
        if (decisionTimer > 0f)
            return;

        decisionTimer = decisionInterval;

        RLState state = BuildRLState();

        AttackActionType action = DecideAction(state);
        int actionIndex = System.Array.IndexOf(actions, action);

        float reward = 0f;

        if (hasPrev)
        {
            reward = CalculateReward(state, actionIndex);
            UpdateQ(prevState, prevActionIndex, state, reward);
        }

        if (debugCombat)
        {
            Debug.Log($"[RL] {state.distance} | HP:{state.health} | EnemyHP:{state.enemyHealth} | TDmg:{state.tookDamage}");
            Debug.Log($"[RL] Action: {action} | Reward: {reward:F2}");
        }

        prevState = state;
        prevActionIndex = actionIndex;
        hasPrev = true;

        ExecuteAction(action);

        epsilon = Mathf.Max(0.05f, epsilon * 0.999f);
    }

    // ----------------------------------- RL ----------------------------------- //

    RLState BuildRLState()
    {
        return new RLState
        {
            distance = GetDistanceState(Vector3.Distance(transform.position, aimTarget.position)),
            health = GetHealthState(healthSystem.health),
            enemyHealth = GetHealthState(PlayerController.Instance.currentHealth),  // hardcoded enemy = player
            tookDamage = tookDamageTimer > 0
        };
    }

    AttackActionType DecideAction(RLState state)
    {
        int key = state.GetHashCode();

        if (!qTable.ContainsKey(key))
        {
            qTable[key] = new float[actions.Length];

            for (int i = 0; i < actions.Length; i++)
                qTable[key][i] = Random.Range(0f, 0.1f);
        }

        // explore - get random action
        if (Random.value < epsilon)
            return actions[Random.Range(0, actions.Length)];

        // exploit - get best action
        float[] q = qTable[key];

        int best = 0;
        for (int i = 1; i < q.Length; i++)
            if (q[i] > q[best])
                best = i;

        return actions[best];
    }
    
    void UpdateQ(RLState prevState, int actionIndex, RLState newState, float reward)
    {
        int prevKey = prevState.GetHashCode();
        int newKey = newState.GetHashCode();

        if (!qTable.ContainsKey(newKey))
            qTable[newKey] = new float[actions.Length];

        float[] prevQ = qTable[prevKey];
        float[] nextQ = qTable[newKey];

        float maxNext = Mathf.Max(nextQ);

        prevQ[actionIndex] += learningRate *
                              (reward + discount * maxNext - prevQ[actionIndex]);
    }
    
   // ------------------------------ REWARD ------------------------------ //

    float CalculateReward(RLState state, int actionIndex)
    {
        // TODO - valid move dir
        
        float reward = 0f;
        
        float dist = Vector3.Distance(transform.position, aimTarget.position);

        var action = actions[actionIndex];

        // DAMAGE
        if (gaveDamageTimer > 0)
            reward += 1f;

        if (state.tookDamage)
            reward -= 1.2f;

        // DISTANCE LOGIC
        if (dist < 3f)
        {
            reward -= 0.5f;
            if (action == AttackActionType.BackOffShoot)
                reward += 0.6f;
        }
        else if (dist > 10f)
        {
            reward -= 0.3f;
            if (action == AttackActionType.PushForwardShoot)
                reward += 0.4f;
        }
        else
        {
            reward += 0.2f;
        }

        // HEALTH LOGIC
        bool winning = state.health > state.enemyHealth;

        if (winning)
        {
            if (action == AttackActionType.PushForwardShoot)
                reward += 0.3f;

            if (action == AttackActionType.HoldPositionShoot)
                reward += 0.2f;
        }
        else
        {
            if (action == AttackActionType.BackOffShoot)
                reward += 0.4f;

            if (action == AttackActionType.StrafeLeftShoot ||
                action == AttackActionType.StrafeRightShoot)
                reward += 0.3f;
        }

        // DAMAGE REACTION
        if (state.tookDamage)
        {
            if (action == AttackActionType.StrafeLeftShoot ||
                action == AttackActionType.StrafeRightShoot)
                reward += 0.4f;

            if (action == AttackActionType.HoldPositionShoot)
                reward -= 0.3f;
        }

        // SAFE STATE
        if (!state.tookDamage && dist >= 5f && dist <= 8f)
        {
            if (action == AttackActionType.MaintainDistanceShoot)
                reward += 0.3f;
        }

        return reward;
    }

    // ----------------------------------- EXECUTION ----------------------------------- //

    void ExecuteAction(AttackActionType action)
    {
        StopShooting();

        switch (action)
        {
            case AttackActionType.StrafeLeftShoot:
                movement.StrafeLeft();
                StartShooting();
                break;

            case AttackActionType.StrafeRightShoot:
                movement.StrafeRight();
                StartShooting();
                break;

            case AttackActionType.PushForwardShoot:
                movement.PushForward();
                StartShooting();
                break;

            case AttackActionType.BackOffShoot:
                movement.BackOff();
                StartShooting();
                break;

            case AttackActionType.MaintainDistanceShoot:
                movement.MaintainDistance(5f, 8f);
                StartShooting();
                break;

            case AttackActionType.HoldPositionShoot:
                movement.HoldPosition();
                StartShooting();
                break;
        }
    }

    // ----------------------------------- SHOOTING ----------------------------------- //

    void HandleShooting()
    {
        if (!isShooting) return;
        
        Vector3 shootDir = firearmAimer.weaponPivot.forward;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireCooldown)
        {
            firearm.Fire(firearmAimer.weaponPivot.position, shootDir);
            fireTimer = 0f;
        }
    }

    void StartShooting() => isShooting = true;

    void StopShooting()
    {
        isShooting = false;
        fireTimer = 0f;
    }

    void MaintainAim()
    {
        if (!aimTarget) return;

        Vector3 targetPoint = GetBestAimPoint();
        
        Vector3 aimDir = (targetPoint - firearmAimer.weaponPivot.position).normalized;
        firearmAimer.SetAimDirection(aimDir);
    }
    
    Vector3 GetBestAimPoint()
    {
        Vector3 origin = transform.position + transform.up * 1.25f;

        Vector3[] targets =
        {
            aimTarget.position + Vector3.up * 0.7f, // head
            aimTarget.position,                     // body
            aimTarget.position - Vector3.up * 0.7f  // legs
        };

        foreach (var t in targets)
        {
            Vector3 dir = (t - origin).normalized;
            float dist = Vector3.Distance(origin, t);

            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
            {
                if (hit.transform.root == aimTarget)
                    return t;
            }
        }

        return aimTarget.position; // fallback
    }

    // ----------------------------------- EVENTS ----------------------------------- //

    public void RegisterTookDamage()
    {
        tookDamageTimer = 1.5f;
        behaviour.OnHit(aimTarget.position);
    }

    public void RegisterGaveDamage()
    {
        gaveDamageTimer = 1.5f;
    }

    // ----------------------------------- UTILS ----------------------------------- //

    public bool CanSeePlayer()
    {
        return IsInFOV() && HasLineOfSight();
    }
    
    bool IsInFOV()
    {
        Vector3 origin = transform.position + transform.up * 1.25f;
        Vector3 toTarget = (aimTarget.position - origin).normalized;

        float angle = Vector3.Angle(transform.forward, toTarget);

        float fov = 120f;

        return angle <= fov * 0.5f;
    }
    
    public bool HasLineOfSight()
    {
        Vector3 origin = transform.position + transform.up * 1.25f;
        Vector3 dir = (aimTarget.position - origin).normalized;
        float dist = Vector3.Distance(origin, aimTarget.position);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
            return hit.transform.root == aimTarget;

        return false;
    }
    
    void UpdateTimers()
    {
        gaveDamageTimer -= Time.deltaTime;
        tookDamageTimer -= Time.deltaTime;
    }

    DistanceState GetDistanceState(float d)
    {
        if (d <= 3f) return DistanceState.Close;
        if (d <= 6f) return DistanceState.Medium;
        return DistanceState.Far;
    }

    HealthState GetHealthState(float hp)
    {
        if (hp < 100f) return HealthState.Low;
        if (hp < 400f) return HealthState.Medium;
        return HealthState.High;
    }
    
    // ----------------------------------- DEBUG ----------------------------------- //
    
    void OnDrawGizmos()
    {
        if (!debugCombat || aimTarget == null)
            return;

        Vector3 origin = transform.position + transform.up * 1.25f;

        // ---------------- FOV ----------------
        float fov = 120f;
        float halfFov = fov / 2f;

        Vector3 forward = transform.forward;

        Quaternion leftRot = Quaternion.Euler(0, -halfFov, 0);
        Quaternion rightRot = Quaternion.Euler(0, halfFov, 0);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, leftDir * 20f);
        Gizmos.DrawRay(origin, rightDir * 20f);

        // ---------------- LOS ----------------
        Vector3 targetPos = aimTarget.position;
        Vector3 losDir = (targetPos - origin).normalized;
        float dist = Vector3.Distance(origin, targetPos);

        bool hasLOS = HasLineOfSight();

        Gizmos.color = hasLOS ? Color.green : Color.red;
        Gizmos.DrawRay(origin, losDir * dist);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(targetPos, 0.2f);

        /*
        // ---------------- AIM DEBUG ----------------
        if (firearmAimer == null || firearmAimer.weaponPivot == null)
            return;

        Vector3 weaponOrigin = firearmAimer.weaponPivot.position;

        // target point (head/body/legs logic)
        Vector3 bestTarget = GetBestAimPoint();

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(bestTarget, 0.15f);

        // calculated aim direction
        Vector3 aimDir = (bestTarget - weaponOrigin).normalized;

        Gizmos.color = Color.darkGreen;
        Gizmos.DrawRay(weaponOrigin, aimDir * 10f);
        */
    }
}