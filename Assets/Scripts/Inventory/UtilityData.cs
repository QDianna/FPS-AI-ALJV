using UnityEngine;

[CreateAssetMenu(fileName = "UtilityData", menuName = "Scriptable Objects/UtilityData")]
public class UtilityData : ItemData
{
    [Header("Utility Data")]
    public GameObject utilityPrefab;
    public GameObject utilityPickupPrefab;
    
    public float damage;
    public float radius;
    public float fuseTime;  // time until grenade explodes
    public float throwCooldown = 2f;
    
}
