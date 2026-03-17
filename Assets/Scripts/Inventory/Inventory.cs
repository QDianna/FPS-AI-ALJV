using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

// Inventory - decides if firearm exists
// WeaponsController - decides is firearm is instantiated 

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    
    public List<InventorySlot> inventoryItems = new();
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    InventorySlot FindSlot(ItemData item)
    {
        return inventoryItems.Find(slot => slot.item == item);
    }
    
    public bool HasItem(ItemData item, int amount = 1)
    {
        InventorySlot slot = FindSlot(item);
        return slot != null && slot.quantity >= amount;
    }

    public InventorySlot GetItemOnSlot(WeaponSlot weaponSlot)
    {
        var inventorySlot = inventoryItems.Find(slot => slot.item.slot == weaponSlot);
        return inventorySlot == null ? null : inventorySlot;
    }

    public bool HasItemOnSlot(WeaponSlot weaponSlot)
    {
        return inventoryItems.Exists(slot => slot.item.slot == weaponSlot);
    }
    
    public bool AddItem(ItemData item, int amount = 1)
    {
        // search if item is already in inventory
        InventorySlot slot = FindSlot(item);

        if (slot != null && item.maxStack <= 1)
            return false;

        if (slot != null)
            slot.quantity += amount;
        else
            inventoryItems.Add(new InventorySlot(item, amount));
        
        return true;
    }
    
    public void RemoveItem(ItemData item, int amount = 1)
    {
        InventorySlot slot = FindSlot(item);

        if (slot == null)
            return;

        slot.quantity -= amount;

        if (slot.quantity <= 0)
        {
            inventoryItems.Remove(slot);
        }
    }

}
