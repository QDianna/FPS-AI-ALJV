using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, Vector3 attackerPos);
    float health { get; set; }
}