using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyMovementAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float obstacleCheckDistance = 1f;

    private Transform player;
    private NavMeshAgent agent;

    private Coroutine currentMoveRoutine;
    private bool isMoving;

    private enum MovementMode
    {
        NavMesh,
        Manual
    }

    private MovementMode currentMode;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        player = PlayerController.Instance.transform;
    }

    // ------------------------------ NAVMESH ------------------------------ //

    public void GoTo(Vector3 position)
    {
        SwitchToNavMesh();

        agent.isStopped = false;
        agent.SetDestination(position);
    }

    public void StopNavMesh()
    {
        if (agent != null)
            agent.isStopped = true;
    }

    // ------------------------------ COMBAT MOVEMENT ------------------------------ //

    public void StrafeLeft(float duration)
    {
        StartManualMove(-GetRight(), duration);
    }

    public void StrafeRight(float duration)
    {
        StartManualMove(GetRight(), duration);
    }

    public void BackOff(float duration)
    {
        Vector3 dir = (transform.position - player.position).normalized;
        StartManualMove(dir, duration);
    }
    
    public void PushForward(float duration)
    {
        Vector3 dir = (player.position - transform.position).normalized;
        StartManualMove(dir, duration);
    }

    public void MaintainDistance(float desiredMin, float desiredMax, float duration)
    {
        Vector3 toPlayer = (player.position - transform.position);
        float dist = toPlayer.magnitude;

        Vector3 dir = Vector3.zero;

        if (dist < desiredMin)
            dir = -toPlayer.normalized; // prea aproape → înapoi
        else if (dist > desiredMax)
            dir = toPlayer.normalized;  // prea departe → înainte
        else
            return; // deja în range bun → nu mișcă

        StartManualMove(dir, duration);
    }
    
    public void DodgeLeft(float duration)
    {
        StartManualMove(-GetRight(), duration * 0.3f); // mai scurt decât strafe
    }

    public void DodgeRight(float duration)
    {
        StartManualMove(GetRight(), duration * 0.3f);
    }
    
    public void HoldPosition()
    {
        StopAllMovement();
    }
    
    // ------------------------------ MODE SWITCH ------------------------------ //

    private void SwitchToManual()
    {
        currentMode = MovementMode.Manual;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void SwitchToNavMesh()
    {
        currentMode = MovementMode.NavMesh;

        StopManualMovement();
    }

    // ------------------------------ CORE ------------------------------ //

    public bool CanMove(Vector3 dir, float checkDistance)
    {
        Vector3 origin = transform.position + Vector3.up * 1f;
        return !Physics.Raycast(origin, dir, checkDistance);
    }

    public Vector3 GetRight()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        return Vector3.Cross(Vector3.up, toPlayer);
    }
    
    public void FaceDirection(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
    }

    // ------------------------------ MANUAL ------------------------------ //

    private void StartManualMove(Vector3 dir, float duration)
    {
        SwitchToManual();

        if (currentMoveRoutine != null)
            StopCoroutine(currentMoveRoutine);

        currentMoveRoutine = StartCoroutine(MoveRoutine(dir, duration));
    }

    private IEnumerator MoveRoutine(Vector3 dir, float duration)
    {
        isMoving = true;

        float timer = 0f;

        dir.y = 0f;
        dir.Normalize();

        while (timer < duration)
        {
            if (CanMove(dir, obstacleCheckDistance))
            {
                transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);
                // FaceDirection(dir);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isMoving = false;
    }

    public void StopManualMovement()
    {
        if (currentMoveRoutine != null)
        {
            StopCoroutine(currentMoveRoutine);
            currentMoveRoutine = null;
        }

        isMoving = false;
    }

    // ------------------------------ GLOBAL STOP ------------------------------ //

    public void StopAllMovement()
    {
        StopManualMovement();
        StopNavMesh();
    }

    // ------------------------------ INFO ------------------------------ //

    public bool IsMoving()
    {
        return isMoving;
    }
    
    public bool HasReachedDestination(float tolerance = 0.5f)
    {
        if (agent == null || !agent.hasPath)
            return true;

        return agent.remainingDistance <= tolerance;
    }
}

