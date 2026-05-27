using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public NavMeshAgent agent;

    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [SerializeField] private float rotationSpeed = 120f;

    [SerializeField] private float moveTolerance = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool debugMovement = false;

    private bool hasLookRequest;
    private Vector3 requestedLookDirection;

    void Awake()
    {
        if (!agent)
            agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (!player)
            player = PlayerController.Instance.transform;

        agent.speed = moveSpeed;
        agent.updateRotation = false;
    }

    void Update()
    {
        if (hasLookRequest)
        {
            RotateToward(
                requestedLookDirection
            );

            hasLookRequest = false;

            return;
        }

        RotateTowardVelocity();
    }
    // ----------------------------------------- ROTATION ----------------------------------------- //

    void RotateToward(Vector3 direction)
    {
        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }
    
    void RotateTowardVelocity()
    {
        Vector3 velocity =
            agent.velocity;

        velocity.y = 0f;

        if (velocity.sqrMagnitude < 0.001f)
            return;

        RotateToward(
            velocity.normalized
        );
    }

    public void SetLookDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        requestedLookDirection =
            direction.normalized;

        hasLookRequest = true;
    }

    // ----------------------------------------- CORE NAV ----------------------------------------- //

    public void GoTo(Vector3 position)
    {
        if (TryGetValidNavMeshPosition(position, out Vector3 validPos))
        {
            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.SetDestination(validPos);

            if (debugMovement)
            {
                Debug.Log($"[MOVE] GoTo -> {validPos}");
            }
        }
    }

    // ----------------------------------------- COMBAT MOVEMENT ----------------------------------------- //

    public void PushForward(float distance = 2f)
    {
        MoveInDirection(
            transform.forward,
            distance,
            moveSpeed
        );
    }

    public void BackOff(float distance = 2f)
    {
        MoveInDirection(
            -transform.forward,
            distance,
            moveSpeed
        );
    }

    public void StrafeLeft(float distance = 2f)
    {
        MoveInDirection(
            -transform.right,
            distance,
            moveSpeed
        );
    }

    public void StrafeRight(float distance = 2f)
    {
        MoveInDirection(
            transform.right,
            distance,
            moveSpeed
        );
    }

    private void MoveInDirection(
        Vector3 dir,
        float distance,
        float speed)
    {
        dir.y = 0f;
        dir.Normalize();

        Vector3 target =
            transform.position + dir * distance;

        if (TryGetValidNavMeshPosition(
                target,
                out Vector3 validPos))
        {
            agent.isStopped = false;
            agent.speed = speed;
            agent.SetDestination(validPos);

            if (debugMovement)
            {
                Debug.Log($"[MOVE] -> {validPos}");
            }
        }
    }

    // ----------------------------------------- VALIDATION ----------------------------------------- //

    public bool CanPushForward(float distance = 2f)
    {
        return CanMoveInDirection(
            transform.forward,
            distance
        );
    }

    public bool CanRetreat(float distance = 2f)
    {
        return CanMoveInDirection(
            -transform.forward,
            distance
        );
    }

    public bool CanStrafeLeft(float distance = 2f)
    {
        return CanMoveInDirection(
            -transform.right,
            distance
        );
    }

    public bool CanStrafeRight(float distance = 2f)
    {
        return CanMoveInDirection(
            transform.right,
            distance
        );
    }

    private bool CanMoveInDirection(
        Vector3 dir,
        float distance)
    {
        dir.y = 0f;
        dir.Normalize();

        Vector3 target =
            transform.position + dir * distance;

        return TryGetValidNavMeshPosition(
            target,
            out _
        );
    }

    // ----------------------------------------- HELPERS ----------------------------------------- //

    public bool HasReachedDestination(
        float tolerance = 0.25f)
    {
        if (!agent.hasPath)
            return true;

        return agent.remainingDistance <= tolerance;
    }

    public bool TryGetValidNavMeshPosition(
        Vector3 desiredTarget,
        out Vector3 result)
    {
        Vector3 flatTarget = desiredTarget;

        flatTarget.y = transform.position.y;

        if (NavMesh.SamplePosition(
                flatTarget,
                out NavMeshHit hit,
                moveTolerance,
                NavMesh.AllAreas))
        {
            float error =
                Vector3.Distance(
                    flatTarget,
                    hit.position
                );

            if (error > moveTolerance)
            {
                result = transform.position;
                return false;
            }

            result = hit.position;
            return true;
        }

        result = transform.position;
        return false;
    }

}

