using UnityEngine;

[CreateAssetMenu(fileName = "FirearmData", menuName = "Scriptable Objects/FirearmData")]
public class FirearmData : ItemData
{
    [Header("Firearm Data")]
    public GameObject firearmPrefab;
    public GameObject firearmPickupPrefab;
    
    public float damage;
    public float fireRate;  // cooldown between shots
}
