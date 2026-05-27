using UnityEngine;

public class PlayerAimer : FirearmAimer
{
    [Header("Player Aim")]
    [SerializeField] private float rotationSpeed = 360f;
    
    void LateUpdate()
    {
        Quaternion targetRotation =
            Quaternion.LookRotation(targetDirection);

        aimRoot.rotation =
            Quaternion.RotateTowards(
                aimRoot.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }
}