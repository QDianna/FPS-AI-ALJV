using UnityEngine;
using System.Collections;

public class EnemyMovementAI : MonoBehaviour
{
    [SerializeField] private float movement = 3f;
    [SerializeField] private float obstacleCheckDistance = 1f;

    private Transform player;

    private Coroutine currentMoveRoutine;
    private bool isMoving;

    void Start()
    {
        player = PlayerController.Instance.transform;
    }

    // ================= CORE =================

    public bool CanMove(Vector3 dir, float checkDistance)
    {
        Vector3 origin = transform.position + Vector3.up * 1f;
        return !Physics.Raycast(origin, dir, checkDistance);
    }

    private Vector3 GetRight()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        return Vector3.Cross(Vector3.up, toPlayer);
    }

    // ================= PUBLIC ACTIONS =================

    public void StrafeLeft(float duration)
    {
        StartMoveRoutine(-GetRight(), duration);
    }

    public void StrafeRight(float duration)
    {
        StartMoveRoutine(GetRight(), duration);
    }

    public void BackOff(float duration)
    {
        Vector3 dir = (transform.position - player.position).normalized;
        StartMoveRoutine(dir, duration);
    }

    public void StopMovement()
    {
        if (currentMoveRoutine != null)
            StopCoroutine(currentMoveRoutine);

        isMoving = false;
    }

    // ================= INTERNAL =================

    private void StartMoveRoutine(Vector3 dir, float duration)
    {
        // oprește orice mișcare anterioară
        if (currentMoveRoutine != null)
            StopCoroutine(currentMoveRoutine);

        currentMoveRoutine = StartCoroutine(MoveRoutine(dir, duration));
    }

    private IEnumerator MoveRoutine(Vector3 dir, float duration)
    {
        isMoving = true;

        float timer = 0f;

        // normalize doar o dată
        dir.y = 0f;
        dir.Normalize();

        while (timer < duration)
        {
            if (CanMove(dir, obstacleCheckDistance))
            {
                transform.Translate(dir * movement * Time.deltaTime, Space.World);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isMoving = false;
    }

    // ================= DEBUG / INFO =================

    public bool IsMoving()
    {
        return isMoving;
    }
}