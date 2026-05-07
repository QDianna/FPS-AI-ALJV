using UnityEngine;

public class FirearmAimer : MonoBehaviour
{
    [Header("References")]
    public Transform weaponPivot;

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private bool instant = false;

    private bool hasTarget;
    private Vector3 targetDirection;
    
    [SerializeField] private bool usePitchOnly = false;

    public void SetAimDirection(Vector3 worldDirection)
    {
        if (worldDirection.sqrMagnitude < 0.001f)
        {
            hasTarget = false;
            return;
        }

        hasTarget = true;
        targetDirection = worldDirection.normalized;
    }

    void LateUpdate()
    {
        if (!hasTarget || weaponPivot == null)
            return;

        if (usePitchOnly)
        {
            // ENEMY MODE (pitch only)

            Quaternion worldRot = Quaternion.LookRotation(targetDirection);

            Quaternion localRot = Quaternion.Inverse(weaponPivot.parent.rotation) * worldRot;

            float pitch = localRot.eulerAngles.x;

            if (pitch > 180f) pitch -= 360f;

            Quaternion targetRot = Quaternion.Euler(pitch, 0f, 0f);

            weaponPivot.localRotation = instant
                ? targetRot
                : Quaternion.RotateTowards(
                    weaponPivot.localRotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
        }
        
        else
        {
            // PLAYER MODE (full aim)

            Quaternion targetRot = Quaternion.LookRotation(targetDirection);

            weaponPivot.rotation = instant
                ? targetRot
                : Quaternion.RotateTowards(
                    weaponPivot.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
        }
    }
}