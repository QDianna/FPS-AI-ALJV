using UnityEngine;
using System.Collections.Generic;

public enum WeaponSlot
{
    Knife,
    Primary,
    Secondary,
    Utility
}

// Inventory - decides if firearm exists
// WeaponsController - decides is firearm is instantiated 

public class PlayerWeaponsController : MonoBehaviour
{
    [Header("References")]
    public Transform firearmGripPoint;
    public Transform utilityGripPoint;
    
    // active instances of firearms
    private Dictionary<WeaponSlot, FirearmController> instantiatedFirearms = new();
    
    // active weapon - firearm or utility
    public KnifeController activeKnife;
    public FirearmController activeFirearm;
    public UtilityData activeUtility;
    private int activeUtilityIndex = 0;
    
    public WeaponSlot activeSlot;

    void Start()
    {
        // knife always equipped on player at start
        activeKnife = firearmGripPoint.GetComponentInChildren<KnifeController>();
        
        // find firearm already equipped on player at start
        activeFirearm = firearmGripPoint.GetComponentInChildren<FirearmController>();

        if (activeFirearm)
        {
            activeFirearm.isEquipped = true;  // for firearm animator

            WeaponSlot slot = activeFirearm.data.slot;
            activeSlot = slot;
            
            Inventory.Instance.AddItem(activeFirearm.data, 1);
            
            GameUI.Instance.OnSlotSelected(activeSlot);
            GameUI.Instance.OnSlotFull(slot, activeFirearm.data.itemIcon);

            activeUtility = null;
            
            instantiatedFirearms.Add(slot, activeFirearm);
        }
        else
        {
            // Debug.Log("No firearm found on player at the start");
            
            activeSlot = WeaponSlot.Knife;
            GameUI.Instance.OnSlotSelected(activeSlot);
        }
    }
    
    
    public bool PickupFirearm(FirearmData pickupData)
    {
        WeaponSlot weaponSlot = pickupData.slot;
        
        // already have this weapon
        if (Inventory.Instance.HasItem(pickupData))
        {
            // Debug.Log("Pickup Firearm: already have this weapon");
            return false;
        }

        // have different weapon on the slot => on drop from specific slot
        if (Inventory.Instance.HasItemOnSlot(weaponSlot))
        {
            // Debug.Log("Pickup Firearm: already have a weapon on this slot, dropping...");
            DropWeaponFromSlot(weaponSlot);
        }

        EquipGun(pickupData);
        return true;
    }

    public bool PickupUtility(UtilityData pickupData)
    {
        return false;
    }

    public void DropWeaponFromSlot(WeaponSlot weaponSlot)
    {
        if (!instantiatedFirearms.TryGetValue(weaponSlot, out var instance))
        {
            // Debug.Log("Drop Weapon: no other weapon found on this slot");
            return;
        }

        // weapon that needs to drop
        FirearmData data = instance.data;

        Vector3 pickupPosition = new Vector3(transform.position.x + transform.forward.x * 3f, 
                                            0.5f, 
                                            transform.position.z + transform.forward.z * 3f);
        
        GameObject pickupFirearm = Instantiate(
            data.firearmPickupPrefab, 
            pickupPosition,
            Quaternion.identity
        );
        
        // pickupFirearm.GetComponent<WeaponPickup>().remainingBullets = instance.bullets;
        
        Inventory.Instance.RemoveItem(data);
        instantiatedFirearms.Remove(weaponSlot);
        Destroy(instance.gameObject);
        
        // Debug.Log("Drop Weapon From Slot: dropped weapon on the ground");
    }
    
    // add it to inventory and instantiated firearms
    public void EquipGun(FirearmData data)
    {
        Inventory.Instance.AddItem(data, 1);
        
        FirearmController firearm = Instantiate(
            data.firearmPrefab,
            firearmGripPoint
        ).GetComponent<FirearmController>();
        
        firearm.isEquipped = true;
        
        instantiatedFirearms.Add(data.slot, firearm);
        
        SwitchWeaponSlot(data.slot);
        GameUI.Instance.OnSlotFull(data.slot, data.itemIcon);
    }
    
    // deactivate old and activate new weapn
    public void SwitchWeaponSlot(WeaponSlot slot)
    {
        // deactivate active firearm
        if (activeFirearm)
        {
            activeFirearm.isEquipped = false; 
            activeFirearm.gameObject.SetActive(false);
            // Debug.Log("Switch Weapon Slot: last active firearm deactivated");
        }
        
        // activate from firearms instances dictionary
        if (instantiatedFirearms.TryGetValue(slot, out FirearmController firearm))
        {
            firearm.gameObject.SetActive(true);
            firearm.isEquipped = true;

            activeFirearm = firearm;
        
            GameUI.Instance.UpdateBulletsUI();
            // Debug.Log("Switch Weapon Slot: current active firearm activated");
        }
        
        // todo activate knife
        // todo activate utility
        if (slot != WeaponSlot.Knife)
        {
            activeKnife.gameObject.SetActive(false);
        }
        else
        {
            activeKnife.gameObject.SetActive(true);
        }

        if (slot == WeaponSlot.Utility)
        {
            SwitchUtility();
        }
        
        activeSlot = slot;
        GameUI.Instance.OnSlotSelected(slot);
    }
    
    public void SwitchUtility()
    {
        List<UtilityData> utilities = GetAvailableUtilities();

        if (utilities.Count == 0)
        {
            activeUtility = null;
            return;
        }

        activeUtilityIndex = (activeUtilityIndex + 1) % utilities.Count;
        activeUtility = utilities[activeUtilityIndex];

        activeSlot = WeaponSlot.Utility;
        
        GameUI.Instance.OnSlotSelected(WeaponSlot.Utility);
        GameUI.Instance.OnSlotFull(WeaponSlot.Utility, activeUtility.itemIcon);
        GameUI.Instance.UpdateBulletsUI();
    }
    
    private List<UtilityData> GetAvailableUtilities()
    {
        List<UtilityData> result = new();

        foreach (var slot in Inventory.Instance.inventoryItems)
        {
            if (slot.item.slot == WeaponSlot.Utility && slot.quantity > 0)
                result.Add((UtilityData)slot.item);
        }

        return result;
    }

    public void ThrowActiveUtility()
    {
        if (activeUtility == null)
            return;

        Instantiate(
            activeUtility.utilityPrefab,
            utilityGripPoint.position,
            utilityGripPoint.rotation
        );
        Inventory.Instance.RemoveItem(activeUtility, 1);
        bool autoNext = !Inventory.Instance.HasItem(activeUtility);
        
        // dacă s-a terminat, treci automat la următoarea
        if (autoNext)
        {
            activeUtility = null;
            GameUI.Instance.OnSlot3Empty();
            GameUI.Instance.UpdateBulletsUI();
            SwitchUtility();
        }
        
        GameUI.Instance.UpdateBulletsUI();
    }
    
}
