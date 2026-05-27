using UnityEngine;

public abstract class FirearmAimer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Transform aimRoot;

    protected Vector3 targetDirection;
    
    public virtual Vector3 GetAimDirection()
    {
        return aimRoot.forward;
    }
    
    public void SetAimDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        targetDirection =
            direction.normalized;
    }

    public virtual float GetAlignment()
    {
        Vector3 currentDirection =
            GetAimDirection();
        
        Vector3 desiredDirection =
            targetDirection;

        currentDirection.y = 0f;

        float angle =
            Vector3.Angle(
                currentDirection.normalized,
                desiredDirection.normalized
            );

        float normalized =
            Mathf.Clamp01(angle / 15f);

        float alignment =
            1f - normalized;

        return alignment * alignment;
    }
    
    /*
    void OnDrawGizmos()
    {
        if (aimRoot == null)
            return;

        Vector3 origin =
            aimRoot.position;

        Vector3 currentDirection =
            GetAimDirection();

        Vector3 desiredDirection =
            targetDirection;

        // current aim
        Gizmos.color = Color.green;

        Gizmos.DrawRay(
            origin,
            currentDirection.normalized * 3f
        );

        // desired target
        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            origin,
            desiredDirection.normalized * 3f
        );
    }
    */
}