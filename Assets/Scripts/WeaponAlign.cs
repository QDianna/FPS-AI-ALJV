using UnityEngine;

public class WeaponAlign : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponPivot; // pivotul tău

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private bool instant = false;

    private bool hasTarget;
    private Vector3 targetDirection;

    // chemat din exterior (player sau enemy)
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
        if (!hasTarget)
            return;

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