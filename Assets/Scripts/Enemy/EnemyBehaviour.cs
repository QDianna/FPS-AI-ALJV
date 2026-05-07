using UnityEngine;

public enum State
{
    Patrol,
    Chase,
    Search,
    Attack,
    HitReact,
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
    [SerializeField] private float searchDuration = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool debugBehaviour = true;
    
    [Header("Hit React")]
    [SerializeField] private float hitReactDuration = 1f;
    private float hitReactTimer;
    private Vector3 hitDirection;

    // search variables
    private Vector3 lastKnownPlayerPos;
    private float lastTimeSeen = -999f;
    private float searchTimer;

    // patrol variables
    private int waypointIndex;
    private State currentState;

    
    void Awake()
    {
        movement = GetComponent<EnemyMovementAI>();
        healthSystem = GetComponent<EnemyHealth>();
        combatSystem = GetComponent<EnemyCombatAI>();
    }

    void Update()
    {
        UpdatePerception();
        EvaluateTree();
    }
    
    // ----------------------------------------- BEHAVIOUR TREE ----------------------------------------- //

    void EvaluateTree()
    {
        if (TryRetreat()) return;
        if (TryAttack()) return;
        if (TryChase()) return;
        if (TrySearch()) return;
        if (TryHitReact()) return;

        TryPatrol();
    }

    // ----------------------------------------- TREE NODES ----------------------------------------- //

    bool TryRetreat()
    {
        if (healthSystem.health <= 10f)
        {
            SetState(State.Retreat);
            RetreatUpdate();
            return true;
        }
        return false;
    }

    bool TryAttack()
    {
        if (CanSeePlayer() && InAttackRange())
        {
            SetState(State.Attack);
            combatSystem.TickCombat();
            return true;
        }
        return false;
    }

    bool TryChase()
    {
        if (CanSeePlayer())
        {
            SetState(State.Chase);
            ChaseUpdate();
            return true;
        }
        return false;
    }
    
    bool TrySearch()
    {
        bool recentlySeen = Time.time - lastTimeSeen < searchDuration;

        if (recentlySeen)
        {
            SetState(State.Search);
            SearchUpdate();
            return true;
        }
        return false;
    }
    
    bool TryHitReact()
    {
        if (hitReactTimer > 0f && !CanSeePlayer())
        {
            SetState(State.HitReact);
            HitReactUpdate();
            return true;
        }
        return false;
    }

    bool TryPatrol()
    {
        SetState(State.Patrol);
        PatrolUpdate();
        return true;
    }

    // ----------------------------------------- STATE TRANSITIONS ----------------------------------------- //

    void SetState(State newState)
    {
        if (currentState == newState)
            return;

        if (debugBehaviour)
            Debug.Log($"[BT] {currentState} -> {newState}");

        OnStateExit(currentState);
        currentState = newState;
        OnStateEnter(currentState);
    }

    void OnStateEnter(State state)
    {
        /*
        if (debugBehaviour)
            Debug.Log($"[STATE ENTER] {state}");
        */

        switch (state)
        {
            case State.Attack:
                movement.SetMode_NavMeshManual();
                movement.SetLookTarget(player);
                combatSystem.EnterCombat();
                break;

            case State.HitReact:
                movement.SetMode_NavMeshManual();
                movement.Stop();
                movement.SetLookDirection(hitDirection);
                combatSystem.ExitCombat();
                break;
            
            case State.Search:
                searchTimer = 0f;
                movement.SetMode_NavMeshFollow();
                movement.SetLookTarget(player);
                break;
            
            default:
                movement.SetMode_NavMeshFollow();
                movement.SetRotationAuto();
                combatSystem.ExitCombat();
                break;
        }
    }

    void OnStateExit(State state)
    {
        if (state == State.HitReact)
            hitReactTimer = 0;
        /*
         if (debugBehaviour)
            Debug.Log($"[STATE EXIT] {state}");
         */
    }
    
    void UpdatePerception()
    {
        if (CanSeePlayer())
        {
            lastKnownPlayerPos = player.position;
            lastTimeSeen = Time.time;
        }
    }

    // ----------------------------------------- NODES GAME LOGIC ----------------------------------------- //

    void HitReactUpdate()
    {
        hitReactTimer -= Time.deltaTime;

        // menține orientarea (important dacă rotația e lentă)
        movement.SetLookDirection(hitDirection);
    }
    
    void PatrolUpdate()
    {
        if (points.Length == 0) return;

        if (movement.HasReachedDestination(waypointTolerance))
        {
            waypointIndex = (waypointIndex + 1) % points.Length;
            movement.GoTo(points[waypointIndex].position);
        }
    }

    void ChaseUpdate()
    {
        float desiredDistance = 3f;
        
        Vector3 toEnemy = (transform.position - player.position).normalized;
        Vector3 desiredPos = player.position + toEnemy * desiredDistance;

        if (movement.GetClosestNavMeshPoint(desiredPos, out Vector3 validPos))
            movement.GoTo(validPos);
    }

    void RetreatUpdate()
    {
        movement.GoTo(retreatPoint.position);
    }

    void SearchUpdate()
    {
        if (!movement.HasReachedDestination(waypointTolerance))
        {
            movement.GoTo(lastKnownPlayerPos);
            return;
        }

        searchTimer += Time.deltaTime;

        if (searchTimer >= searchDuration)
        {
            searchTimer = 0f;
            lastTimeSeen = -999f;
        }
    }

    // ----------------------------------------- CONDITIONS ----------------------------------------- //
    
    bool InAttackRange() =>
        Vector3.Distance(transform.position, player.position) <= attackRange;

    bool CanSeePlayer()
        => combatSystem.CanSeePlayer();

    // ----------------------------------------- EVENTS ----------------------------------------- //

    public void OnHit(Vector3 source)
    {
        hitDirection = (source - transform.position);
        hitDirection.y = 0f;

        if (hitDirection.sqrMagnitude > 0.001f)
            hitDirection.Normalize();

        hitReactTimer = hitReactDuration;

        /*
        if (debugBehaviour)
            Debug.Log("[BT] HIT REACTION TRIGGERED");
        */
    }
}

