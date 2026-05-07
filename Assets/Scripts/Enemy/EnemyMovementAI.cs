using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementAI : MonoBehaviour
{
    public enum MovementMode
    {
        NavMeshFollow,
        NavMeshManual
    }

    public enum RotationMode
    {
        Auto,
        LookAt
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sampleRadius = 1f;
    [SerializeField] private float rotationSpeed = 20f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMovement = false;

    private Transform player;
    private Transform lookTarget;
    private Vector3 lookDirection;
    
    private bool useDirection;

    private NavMeshAgent agent;

    private MovementMode movementMode;
    private RotationMode rotationMode;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        player = PlayerController.Instance.transform;
        agent.speed = moveSpeed;
    }

    void Update()
    {
        HandleRotation();
    }

    // ----------------------------------------- MODE CONTROL ----------------------------------------- //

    public void SetMode_NavMeshFollow()
    {
        movementMode = MovementMode.NavMeshFollow;
        agent.isStopped = false;
        agent.updateRotation = true;
        
        if (debugMovement)
            Debug.Log("[MOVE] Mode = NavMeshFollow");
    }

    public void SetMode_NavMeshManual()
    {
        movementMode = MovementMode.NavMeshManual;
        agent.isStopped = false;
        agent.updateRotation = false;
        
        if (debugMovement)
            Debug.Log("[MOVE] Mode = NavMeshManual");
    }

    public void SetRotationAuto()
    {
        rotationMode = RotationMode.Auto;
        agent.updateRotation = true;

        if (debugMovement)
            Debug.Log("[MOVE] Rotation = Auto");
    }

    public void SetLookTarget(Transform target)
    {
        rotationMode = RotationMode.LookAt;
        lookTarget = target;
        useDirection = false;
        agent.updateRotation = false;
    }
    
    public void SetLookDirection(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.001f)
            return;

        rotationMode = RotationMode.LookAt;
        lookDirection = dir.normalized;
        useDirection = true;
        agent.updateRotation = false;
    }

    // ----------------------------------------- CORE NAV ----------------------------------------- //

    public void Stop()
    {
        agent.isStopped = true;
    }

    public void GoTo(Vector3 position)
    {
        if (TryGetValidNavMeshPosition(position, out Vector3 validPos))
        {
            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.SetDestination(validPos);

            if (debugMovement)
                Debug.Log($"[MOVE] GoTo -> {validPos}");
        }
        else
        {
            if (debugMovement)
                Debug.Log("[MOVE] Invalid destination");
        }
    }

    public bool GetClosestNavMeshPoint(Vector3 target, out Vector3 result)
    {
        return TryGetValidNavMeshPosition(target, out result);
    }

    // ----------------------------------------- COMBAT MOVEMENT ----------------------------------------- //

    public void StrafeLeft(float distance = 2f)
    {
        Vector3 dir = -GetRight();
        MoveInDirection(dir, distance, moveSpeed);
    }

    public void StrafeRight(float distance = 2f)
    {
        Vector3 dir = GetRight();
        MoveInDirection(dir, distance, moveSpeed);
    }

    public void BackOff(float distance = 2f)
    {
        Vector3 dir = (transform.position - player.position).normalized;
        MoveInDirection(dir, distance, moveSpeed);
    }

    public void PushForward(float distance = 2f)
    {
        Vector3 dir = (player.position - transform.position).normalized;
        MoveInDirection(dir, distance, moveSpeed);
    }
    
    public void MaintainDistance(float min, float max)
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < min)
            BackOff(1f);
        else if (dist > max)
            PushForward(1f);
        else
            Stop();
    }

    public void HoldPosition()
    {
        Stop();
    }

    private void MoveInDirection(Vector3 dir, float distance, float speed)
    {
        Vector3 target = transform.position + dir * distance;

        if (TryGetValidNavMeshPosition(target, out Vector3 validPos))
        {
            agent.isStopped = false;
            agent.speed = speed;
            agent.SetDestination(validPos);

            if (debugMovement)
                Debug.Log($"[MOVE] Directional move -> {validPos}");
        }
    }

    // ----------------------------------------- ROTATION ----------------------------------------- //

    private void HandleRotation()
    {
        if (rotationMode != RotationMode.LookAt)
            return;

        Vector3 dir;

        if (useDirection)
        {
            dir = lookDirection;
        }
        else if (lookTarget != null)
        {
            dir = lookTarget.position - transform.position;
        }
        else
        {
            return;
        }

        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotationSpeed
        );
    }

    // ----------------------------------------- HELPERS ----------------------------------------- //

    public Vector3 GetRight()
    {
        Vector3 toPlayer = (player.position - transform.position);
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.01f)
            return transform.right;

        return Vector3.Cross(Vector3.up, toPlayer.normalized);
    }

    public bool HasReachedDestination(float tolerance = 0.5f)
    {
        if (!agent.hasPath)
            return true;

        return agent.remainingDistance <= tolerance;
    }

    private bool TryGetValidNavMeshPosition(Vector3 target, out Vector3 result)
    {
        Vector3 flatTarget = target;
        flatTarget.y = transform.position.y;
        
        if (NavMesh.SamplePosition(flatTarget, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = transform.position;
        return false;
    }
}

