using UnityEngine;

enum State
{
    Patrol,
    Chase,
    Attack,
    Search,
    Retreat
}

public class EnemyBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] points;
    [SerializeField] private Transform retreatPoint;

    [SerializeField] private EnemyMovementAI movement;
    [SerializeField] private EnemyHealth healthSystem;
    [SerializeField] private EnemyCombatAI combatSystem;

    [Header("Parameters")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float waypointTolerance = 0.5f;

    private Vector3 lastKnownPlayerPos;
    private bool hadLOS;
    private float searchTimer;
    
    [SerializeField] private float searchDuration = 3f;
    
    private State currentState;
    private State lastState;  // DEBUG
    private int index;

    void Awake()
    {
        healthSystem = GetComponent<EnemyHealth>();
        combatSystem = GetComponent<EnemyCombatAI>();
    }

    void Update()
    {
        DecideState();

        // debug
        if (currentState != lastState)
        {
            Debug.Log("State: " + currentState);
            lastState = currentState;
        }
        //
        
        ExecuteState();
    }

    // ------------------------------ STATE ------------------------------ //

    void DecideState()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        bool hasLOS = combatSystem.HasLineOfSight();

        // dacă vede playerul → update last position
        if (hasLOS)
        {
            lastKnownPlayerPos = player.position;
            hadLOS = true;
        }
        
        if (healthSystem.health <= 20f)
        {
            currentState = State.Retreat;
            return;
        }

        if (hasLOS && dist > attackRange)
        {
            currentState = State.Chase;
            return;
        }

        if (hasLOS && dist <= attackRange)
        {
            currentState = State.Attack;
            return;
        }

        if (!hasLOS && hadLOS)
        {
            currentState = State.Search;
            return;
        }

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
            
            case State.Search:
                StopCombat();
                Search();
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

    // ------------------------------ PATROL ------------------------------ //

    private void Patrol()
    {
        if (points.Length == 0)
            return;

        if (movement.HasReachedDestination(waypointTolerance))
        {
            index = (index + 1) % points.Length;
            movement.GoTo(points[index].position);
        }
    }

    // ------------------------------ CHASE ------------------------------ //

    private void ChasePlayer()
    {
        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 targetPos = player.position + dir * 3f;

        movement.GoTo(targetPos);
    }

    // ------------------------------ RETREAT ------------------------------ //

    private void Retreat()
    {
        movement.GoTo(retreatPoint.position);
    }

    // ------------------------------ ATTACK ------------------------------ //

    private void Attack()
    {
        combatSystem.TickCombat();
    }
    
    // ------------------------------ SEARCH ------------------------------ //
    private void Search()
    {
        movement.GoTo(lastKnownPlayerPos);

        if (movement.HasReachedDestination(waypointTolerance))
        {
            // transform.Rotate(0f, 120f * Time.deltaTime, 0f);

            searchTimer += Time.deltaTime;

            if (searchTimer >= searchDuration)
            {
                hadLOS = false;
                searchTimer = 0f;
            }
        }
        else
        {
            searchTimer = 0f;
        }
    }

    private void StopCombat()
    {
        combatSystem.StopAllCombat();
    }
}

