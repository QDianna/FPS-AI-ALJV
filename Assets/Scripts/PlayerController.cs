using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamageable
{
    public static PlayerController Instance { get; private set; }
    
    [Header("References")]
    public PlayerWeaponsController weaponsController;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FirearmAimer firearmAimer;
    [SerializeField] private Transform weaponHolder;
    
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask weaponPickupMask;
    
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float runSpeed = 20f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpForce = 5f;
    
    [Header("Stats")]
    public float maxHealth = 100;
    [HideInInspector] public float currentHealth;
    public float maxArmor = 100;
    public float currentArmor = 0;
    public int armorPrice = 12;

    public int kills;
    public int money = 10;
    public int moneyPerKill = 7;
    public float currentSpeed;
    
    public float pickupRange = 5f;
    public bool canOpenShop;
    
    private Vector2 moveInput;
    private Vector3 velocity;
    
    private bool isFiring;
    
    // --------------------------- UNITY METHODS --------------------------- //
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        characterController = GetComponent<CharacterController>();
        weaponsController = GetComponent<PlayerWeaponsController>();
        firearmAimer = GetComponent<FirearmAimer>();
    }
    
    void Start()
    {
        if (GameManager.Instance && SaveData.gameData != null)
        {
            currentHealth = SaveData.gameData.health;
            kills = SaveData.gameData.kills;
        }
                
        if (!cameraTransform && Camera.main)
            cameraTransform = Camera.main.transform;
        
        currentHealth = maxHealth;
        
        SetCanOpenShop(false);
    }
    
    void Update()
    {
        HandleMovement();
        
        HandleFire(); 
        
        // find pick-up-able weapons with raycast
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, weaponPickupMask)) 
            GameUI.Instance.ShowNotification("Press E to pick up the weapon");
    }
    
    void LateUpdate()
    {
        firearmAimer.SetAimDirection(cameraTransform.forward);
    }
    
    // ---------------------- INPUT SYSTEM CALLBACKS ---------------------- //
    
    public void Move(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    
    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && characterController.isGrounded)
        {
            velocity.y = jumpForce;
        }
    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        if (GameUI.Instance != null && GameUI.Instance.IsShopOpen)
            return;
        
        if (ctx.performed)
            isFiring = true;

        if (ctx.canceled)
            isFiring = false;
    }
    
    private void HandleFire()
    {
        if (!isFiring)
            return;

        if (weaponsController.activeFirearm)
        {
            weaponsController.activeFirearm.Fire(
                cameraTransform.position, 
                cameraTransform.forward
            );
        }
        else if (weaponsController.activeKnife.isActiveAndEnabled)
        {
            weaponsController.activeKnife.Attack();
        }
    }

    
    public void ThrowGrenade(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        
        if (GameUI.Instance != null && GameUI.Instance.IsShopOpen)
            return;
        
        if (!weaponsController.activeUtility)
            Debug.Log("No active grenade!");
        else
            weaponsController.ThrowActiveUtility();
    }
    
    public void WeaponKnife(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            weaponsController.SwitchWeaponSlot(WeaponSlot.Knife);
    }
    
    public void WeaponPrimary(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            weaponsController.SwitchWeaponSlot(WeaponSlot.Primary);
    }

    public void WeaponSecondary(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            weaponsController.SwitchWeaponSlot(WeaponSlot.Secondary);
    }
    
    public void WeaponUtility(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            weaponsController.SwitchWeaponSlot(WeaponSlot.Utility);
    }
    
    public void DropWeapon(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        
        if (!weaponsController.activeFirearm && !weaponsController.activeUtility) return;

        weaponsController.DropWeaponFromSlot(weaponsController.activeSlot);
    }
    
    public void PickUp(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, pickupRange, weaponPickupMask))
            return;
        
        WeaponPickup weaponPickup = hit.collider.GetComponent<WeaponPickup>();
        if (weaponPickup && weaponPickup.weaponData)
        {
            WeaponSlot slot = weaponPickup.weaponData.slot;
            if (slot == WeaponSlot.Primary || slot == WeaponSlot.Secondary)
            {
                // pickup a firearm
                FirearmData data = (FirearmData)weaponPickup.weaponData;
                
                bool destroyPickup = weaponsController.PickupFirearm(data);
                if (destroyPickup)
                    Destroy(weaponPickup.gameObject);
                
                return;
            }

            if (slot == WeaponSlot.Utility)
            {
                // pickup an utility
                UtilityData data = (UtilityData)weaponPickup.weaponData;
                bool destroyPickup = weaponsController.PickupUtility(data);
                if (destroyPickup)
                    Destroy(weaponPickup.gameObject);
                
                return;
            }
            
            Debug.Log("Unknown weapon detected for pickup");
        }
    }
    
    public void OpenShop(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        
        if (!canOpenShop) return;

        GameUI.Instance.OpenShopInterface();
    }
    
    // -------------------------- PLAYER MOVEMENT -------------------------- //
    
    private void HandleMovement()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right   = cameraTransform.right;
        
        forward.y = 0f;
        right.y   = 0f;

        forward.Normalize();
        right.Normalize();
        
        if (moveInput.magnitude < 0.1f)
            currentSpeed = 0f;
        else
            currentSpeed = Keyboard.current.leftShiftKey.isPressed ? runSpeed : walkSpeed;
        
        var moveDirection = forward * moveInput.y + right * moveInput.x;
        var movement = moveDirection * currentSpeed;

        // jumps
        if (characterController.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        
        Vector3 finalMove = (movement + velocity) * Time.deltaTime;
        characterController.Move(finalMove);
    }
    
    // --------------------------- PLAYER STATS --------------------------- //
    
    public void TakeDamage(float amount, Vector3 attackerPos)
    {
        /*float armorTank = currentArmor - amount;
        if (currentArmor > 0)
            currentArmor -= currentArmor > armorTank ? armorTank : currentArmor;
        float remainingDamage = currentArmor > armorTank ? 0 : (armorTank - currentArmor); */
            
        
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            GameUI.Instance.UpdateHealthUI();
            // Debug.Log("Player died");
            return;
        }

        GameUI.Instance.UpdateHealthUI();
        // GameUI.Instance.UpdateArmorUI();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        GameUI.Instance.UpdateHealthUI();
    }
    
    public void AddArmor(float amount)
    {
        currentArmor = Mathf.Min(maxArmor, currentArmor + amount);
        GameUI.Instance.UpdateArmorUI();
    }
    
    public void RemoveArmor(float amount)
    {
        currentArmor = currentArmor  - amount;
        GameUI.Instance.UpdateArmorUI();
    }
    
    // -------------------- PLAYER - SHOP INTERACTIONS -------------------- //
    
    public void SetCanOpenShop(bool value)
    {
        canOpenShop = value;

        if (!value)
        {
            GameUI.Instance.CloseShopInterface();
        }
    }

    public void TryBuyItem(ItemData item)
    {
        if (money < item.price)
        {
            GameUI.Instance.ShowNotification("Not enough money");
            return;
        }
        
        bool success = Inventory.Instance.AddItem(item);

        if (!success)
        {
            GameUI.Instance.ShowNotification("Can't have more than x1 " + item.itemName);
            return;
        }

        money -= item.price;
        
        GameUI.Instance.UpdateMoneyUI();
        GameUI.Instance.ShowNotification("Bought " + item.itemName + " for $" + item.price);
        
        // todo add effect/action taken based on item bought
    }
    
    public void TryBuyUtility()
    {
        int price = 5; // todo info get from UI
        if (money < price)
        {
            GameUI.Instance.ShowNotification("Not enough money");
            return;
        }

        money -= price;
        // todo add certain type of utility to inventory

        GameUI.Instance.ShowNotification("Grenade bought");
        // GameUI.Instance.UpdateMoneyUI();
        // GameUI.Instance.UpdateBulletsUI();
    }

    public void TryBuyAmmo()
    {
        int price = 3; // todo info get from UI
        if (money < price)
        {
            GameUI.Instance.ShowNotification("Not enough money");
            return;
        }

        money -= price;
        // activeFirearm.bullets += 100;

        GameUI.Instance.ShowNotification("Ammo bought");
        GameUI.Instance.UpdateMoneyUI();
        GameUI.Instance.UpdateBulletsUI();
    }
    public void TryBuyArmor()
    {
        if (money < armorPrice)
        {
            GameUI.Instance.ShowNotification("Not enough money");
            return;
        }

        money -= armorPrice;
        AddArmor(50);

        GameUI.Instance.ShowNotification("Armor bought");
        GameUI.Instance.UpdateMoneyUI();
        GameUI.Instance.UpdateArmorUI();
    }
}
