using UnityEngine;
using UnityEngine.AI;

enum State
{
    Patrol,
    Chase,
    Attack,
    Retreat
}

public class EnemyBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] points;
    [SerializeField] private Transform retreatPoint;

    private NavMeshAgent agent;
    private EnemyHealth healthSystem;
    private EnemyMovementAI movementSystem;
    private EnemyCombatAI combatSystem;

    [Header("Parameters")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float waypointTolerance = 0.1f;

    [Header("Decision")]
    [SerializeField] private float decisionInterval = 0.5f;
    [SerializeField] private float actionDuration = 0.4f;

    private float decisionTimer;

    private State currentState;
    private int index;
    private bool isChasing;

    private AttackActionType currentAction = AttackActionType.None;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        healthSystem = GetComponent<EnemyHealth>();
        movementSystem = GetComponent<EnemyMovementAI>();
        combatSystem = GetComponent<EnemyCombatAI>();
    }

    void Update()
    {
        DecideState();
        ExecuteState();
    }

    // ================= STATE =================

    void DecideState()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (healthSystem.health <= 20f)
            currentState = State.Retreat;

        else if (combatSystem.HasLineOfSight() && dist > attackRange)
            currentState = State.Chase;

        else if (combatSystem.HasLineOfSight() && dist <= attackRange)
            currentState = State.Attack;

        else
            currentState = State.Patrol;
    }

    void ExecuteState()
    {
        switch (currentState)
        {
            case State.Patrol:
                StopCombat();
                Patrol();
                break;

            case State.Chase:
                StopCombat();
                ChasePlayer();
                break;

            case State.Attack:
                Attack();
                break;

            case State.Retreat:
                StopCombat();
                Retreat();
                break;
        }
    }

    // ================= PATROL =================

    private void Patrol()
    {
        agent.isStopped = false;

        if (points.Length == 0)
            return;

        if (isChasing)
        {
            isChasing = false;
            agent.SetDestination(points[index].position);
        }

        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            index = (index + 1) % points.Length;
            agent.SetDestination(points[index].position);
        }
    }

    // ================= CHASE =================

    private void ChasePlayer()
    {
        isChasing = true;
        agent.isStopped = false;

        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 targetPos = player.position + dir * 3f;

        agent.SetDestination(targetPos);
    }

    // ================= RETREAT =================

    private void Retreat()
    {
        agent.isStopped = false;
        agent.SetDestination(retreatPoint.position);
    }

    // ================= ATTACK =================

    private void Attack()
    {
        agent.isStopped = true;

        decisionTimer -= Time.deltaTime;

        // nu lua decizie nouă dacă:
        // - încă e în acțiune
        // - nu a trecut intervalul
        if (movementSystem.IsMoving() || decisionTimer > 0f)
            return;

        decisionTimer = decisionInterval;

        CombatState state = BuildCombatState();
        AttackActionType action = DecideAction(state);

        if (currentAction != action)
            Debug.Log("** Attack Action ** " + action);

        currentAction = action;

        ExecuteAction(action);
    }

    // ================= DECISION =================

    private CombatState BuildCombatState()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        return new CombatState
        {
            distanceToPlayer = dist,
            health = healthSystem.health,
            playerVisible = combatSystem.HasLineOfSight(),
            gaveDamage = combatSystem.gaveDamageTimer > 0,
            tookDamage = combatSystem.tookDamageTimer > 0,
        };
    }

    private AttackActionType DecideAction(CombatState state)
    {
        // prea aproape
        if (state.distanceToPlayer < 4f)
            return AttackActionType.BackOff;

        // reacție la damage primit
        if (state.tookDamage)
        {
            combatSystem.tookDamageTimer = 0f;

            return (Random.value > 0.5f)
                ? AttackActionType.StrafeRightShoot
                : AttackActionType.StrafeLeftShoot;
        }

        // dacă lovește constant → stă și trage
        if (state.gaveDamage)
        {
            combatSystem.gaveDamageTimer = 0f;
            return AttackActionType.ShootStanding;
        }

        // default
        return AttackActionType.ShootStanding;
    }

    // ================= ACTION EXECUTION =================

    private void ExecuteAction(AttackActionType action)
    {
         // StopCombat(); // reset înainte de noua acțiune

        switch (action)
        {
            case AttackActionType.ShootStanding:
                combatSystem.StartShooting(player.position);
                break;

            case AttackActionType.StrafeLeftShoot:
                movementSystem.StrafeLeft(actionDuration);
                combatSystem.StartShooting(player.position);
                break;

            case AttackActionType.StrafeRightShoot:
                movementSystem.StrafeRight(actionDuration);
                combatSystem.StartShooting(player.position);
                break;

            case AttackActionType.StrafeLeft:
                movementSystem.StrafeLeft(actionDuration);
                break;

            case AttackActionType.StrafeRight:
                movementSystem.StrafeRight(actionDuration);
                break;

            case AttackActionType.BackOff:
                movementSystem.BackOff(actionDuration);
                break;
        }
    }

    private void StopCombat()
    {
        combatSystem.StopShooting();
    }

    // ================= DEBUG =================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}