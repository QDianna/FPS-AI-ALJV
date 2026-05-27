using UnityEngine;
using UnityEngine.AI;

public enum State
{
    Patrol,     // no LOS + no info
    Attack,     // has LOS => RL combat
    Search,     // lost LOS => investigate last known position
    HitReact,   // got hit without LOS => investigate hit direction
    Retreat
}

public class EnemyBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private Transform retreatPoint;

    public EnemyMovementAI movement;
    public EnemyHealth healthSystem;
    public EnemyCombatAI combatSystem;
    
    [Header("Parameters")]
    [SerializeField] private float waypointTolerance = 0.5f;
    [SerializeField] private float searchDuration = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool debugBehaviour = true;
   
    private float hitReactTimer;
    private float hitReactDuration = 3f;
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
       
        healthSystem.OnDamageTaken += HandleDamageTaken;
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
        if (CanSeePlayer())
        {
            SetState(State.Attack);
            combatSystem.TickCombat();
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
            Debug.Log($"************** [ENEMY BEHAVIOUR] {currentState} -> {newState} ************** ");

        OnStateExit(currentState);
        currentState = newState;
        OnStateEnter(currentState);
    }

    void OnStateEnter(State state)
    {
        switch (state)
        {
            case State.Attack:
                combatSystem.EnterCombat();

                break;

            case State.Search:

                searchTimer = 0f;
                
                combatSystem.ExitCombat();

                break;

            case State.HitReact:
                
                combatSystem.ExitCombat();

                break;

            default:

                combatSystem.ExitCombat();

                break;
        }
    }

    void OnStateExit(State state)
    {
        if (state == State.HitReact)
            hitReactTimer = 0;
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

        movement.SetLookDirection(hitDirection);
    }
    
    void PatrolUpdate()
    {
        if (wayPoints.Length == 0) return;

        if (movement.HasReachedDestination(waypointTolerance))
        {
            waypointIndex = (waypointIndex + 1) % wayPoints.Length;
            movement.GoTo(wayPoints[waypointIndex].position);
        }
    }

    void SearchUpdate()
    {
        movement.GoTo(lastKnownPlayerPos);

        if (movement.HasReachedDestination(waypointTolerance))
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchDuration)
            {
                searchTimer = 0f;
                lastTimeSeen = -999f;
            }
        }
    }

    void RetreatUpdate()
    {
        movement.GoTo(retreatPoint.position);
    }

    // ----------------------------------------- CONDITIONS ----------------------------------------- //

    bool CanSeePlayer()
        => combatSystem.CanSeePlayer();
    
    void HandleDamageTaken(float amount, Vector3 attackerPos)
    {
        OnHit(attackerPos);
    }

    public void OnHit(Vector3 attackerPos)
    {
        if (currentState == State.Attack)
            return;

        if (CanSeePlayer())
            return;

        hitDirection =
            attackerPos - transform.position;

        hitDirection.y = 0f;

        if (hitDirection.sqrMagnitude < 0.001f)
            return;

        hitDirection.Normalize();

        hitReactTimer = hitReactDuration;

        Debug.Log(
            $"[HIT REACT] Looking toward hit direction"
        );
    }
    
    // ----------------------------------------- EVENTS ----------------------------------------- //

    public void ResetAI()
    {
        currentState = State.Patrol;

        lastKnownPlayerPos = Vector3.zero;
        lastTimeSeen = -999f;

        searchTimer = 0f;

        hitReactTimer = 0f;
        hitDirection = Vector3.zero;

        waypointIndex = 0;

        combatSystem.ResetCombat();
    }
    
}


