using UnityEngine;

public interface ICombatActions
{
    
    void ShootStanding(Vector3 target);
    void StrafeLeftShoot(Vector3 target);
    void StrafeRightShoot(Vector3 target);
    void StrafeLeft();
    void StrafeRight();
    void BackOff();

}