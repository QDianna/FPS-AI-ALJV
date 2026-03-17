using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [Header("Item Data")]
    public string itemName;
    public Texture2D itemIcon;
    public WeaponSlot slot;
    
    public int maxStack;
    public int price;
}
