using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using System.Collections;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Parameters")] 
    public bool IsShopOpen;
    
    [Header("Item Data for shop items")] 
    [SerializeField] private ItemData pistol00;
    [SerializeField] private ItemData pistol01;
    [SerializeField] private ItemData rifle00;
    [SerializeField] private ItemData grenade;
    
    private VisualElement root;

    private ProgressBar healthBar, armorBar;

    private Label moneyLabel, notificationLabel, bulletsLabel, killsLabel;

    private VisualElement knifeSlot, primaryFirearmSlot, secondaryFirearmSlot, utilitySlot;

    private VisualElement shopInterface;
    private VisualElement buyPistol00Slot, buyPistol01Slot, buyPistol02Slot;
    private VisualElement buyRiffle00Slot, buyRiffle01Slot, buyRiffle02Slot;
    private VisualElement buyGrenadeSlot, buyFlashSlot, buySmokeSlot;
    private VisualElement buyPistolAmmoSlot, buyRiffleAmmoSlot;
    private VisualElement buyArmorSlot;
    
    private Coroutine currentRoutine;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (uiDocument == null)
        {
            Debug.LogError("UI Document not found");
            return;
        }

        root = uiDocument.rootVisualElement;
        
        // player stats visual elements
        healthBar = root.Q<ProgressBar>("HealthBar");
        armorBar  = root.Q<ProgressBar>("ArmorBar");
        
        moneyLabel = root.Q<Label>("MoneyLabel");
        notificationLabel = root.Q<Label>("NotificationLabel");
        notificationLabel.style.display = DisplayStyle.None;
        bulletsLabel = root.Q<Label>("BulletsLabel");
        bulletsLabel.AddToClassList("normal");
        killsLabel = root.Q<Label>("KillsLabel");
        killsLabel.AddToClassList("normal");
        
        // weapons slots visual elements
        knifeSlot             = root.Q<VisualElement>("KnifeSlot");
        primaryFirearmSlot    = root.Q<VisualElement>("PrimaryFirearmSlot");
        secondaryFirearmSlot  = root.Q<VisualElement>("SecondaryFirearmSlot");
        utilitySlot           = root.Q<VisualElement>("UtilitySlot");
        
        // shop visual elements
        shopInterface = root.Q<VisualElement>("ShopInterface");
        
        buyPistol00Slot = shopInterface.Q<VisualElement>("BuyPistol00Slot");
        buyPistol01Slot = shopInterface.Q<VisualElement>("BuyPistol01Slot");
        buyPistol02Slot = shopInterface.Q<VisualElement>("BuyPistol02Slot");
        
        buyRiffle00Slot = shopInterface.Q<VisualElement>("BuyRiffle00Slot");
        buyRiffle01Slot = shopInterface.Q<VisualElement>("BuyRiffle01Slot");
        buyRiffle02Slot = shopInterface.Q<VisualElement>("BuyRiffle02Slot");
        
        buyGrenadeSlot = shopInterface.Q<VisualElement>("BuyGrenadeSlot");
        buyFlashSlot   = shopInterface.Q<VisualElement>("BuyFlashSlot");
        buySmokeSlot   = shopInterface.Q<VisualElement>("BuySmokeSlot");
        
        buyPistolAmmoSlot = shopInterface.Q<VisualElement>("BuyPistolAmmoSlot");
        buyRiffleAmmoSlot = shopInterface.Q<VisualElement>("BuyRiffleAmmoSlot");
        buyArmorSlot      = shopInterface.Q<VisualElement>("BuyArmorSlot");
    }
    
    void Start()
    {
        if (!PlayerController.Instance)
        {
            Debug.LogWarning("Player instance not found by UI Manager");
            return;
        }
        
        // display player stats into UI
        UpdateHealthUI();
        UpdateArmorUI();
        UpdateKillsUI();
        UpdateMoneyUI();
        UpdateBulletsUI();
        
        // bind shop items to item data
        BindShopItem("BuyPistol00Slot", pistol00);
        BindShopItem("BuyPistol01Slot", pistol01);
        BindShopItem("BuyRiffle00Slot", rifle00);
        BindShopItem("BuyGrenadeSlot", grenade);
    }
    
    // ------------------------------ SHOP UI ------------------------------ //
    
    public void OpenShopInterface()
    { 
        shopInterface.style.display = DisplayStyle.Flex;
        IsShopOpen = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void CloseShopInterface()
    { 
        shopInterface.style.display = DisplayStyle.None;
        IsShopOpen = false;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void BindShopItem(string uiName, ItemData item)
    {
        VisualElement ve = root.Q<VisualElement>(uiName);

        ve.style.backgroundImage = new StyleBackground(item.itemIcon);
        
        // todo display item name and price
        
        ve.RegisterCallback<ClickEvent>(_ =>
        {
            PlayerController.Instance.TryBuyItem(item);
        });
    }

    // -------------------------- PLAYER STATS UI -------------------------- //
    
    public void UpdateHealthUI()
    {
        if (healthBar != null)
            healthBar.value = PlayerController.Instance.currentHealth;
    }
    
    public void UpdateArmorUI()
    {
        if (armorBar != null)
            armorBar.value = PlayerController.Instance.currentArmor;
    }

    public void UpdateBulletsUI()
    {
        if (bulletsLabel == null)
        {
            Debug.LogWarning("No bullets label found by UI");
            return;
        }

        FirearmController activeFirearm = PlayerController.Instance.weaponsController.activeFirearm;
        UtilityData activeUtility = PlayerController.Instance.weaponsController.activeUtility;

        if (activeFirearm)
        {
            // firearm bullets = weapon's bullets
            bulletsLabel.text = "x" + PlayerController.Instance.weaponsController.activeFirearm.bullets;
        }
        else if (activeUtility)
        {
            // utility "bullets" = quantity in inventory
            InventorySlot inventorySlot = Inventory.Instance.GetItemOnSlot(WeaponSlot.Utility);
            if (inventorySlot != null)
                bulletsLabel.text = "x" + inventorySlot.quantity;
            else
                bulletsLabel.text = "x0";
        }
        else
        {
            // knife = no bullets
            bulletsLabel.text = "";
        }
    }
    
    public void UpdateKillsUI()
    {
        if (killsLabel != null)
            killsLabel.text = PlayerController.Instance.kills + " kills";
        
        StopAllCoroutines();
        StartCoroutine(FlashKills());
    }

    public void UpdateMoneyUI()
    {
        if (moneyLabel != null)
            moneyLabel.text = "$" + PlayerController.Instance.money;
        
    }
    public void ShowNotification(string notification)
    {
        notificationLabel.text = notification;
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowForSeconds(2f));
    }

    private IEnumerator ShowForSeconds(float seconds)
    {
        notificationLabel.style.display = DisplayStyle.Flex;
        yield return new WaitForSeconds(seconds);
        notificationLabel.style.display = DisplayStyle.None;
        currentRoutine = null;
    }

    public void OnNoBullets()
    {
        StopAllCoroutines();
        StartCoroutine(FlashNoBullets());
    }

    IEnumerator FlashNoBullets()
    {
        bulletsLabel.RemoveFromClassList("normal");
        bulletsLabel.AddToClassList("warning");

        yield return new WaitForSeconds(0.2f);

        bulletsLabel.RemoveFromClassList("warning");
        bulletsLabel.AddToClassList("normal");
    }

    IEnumerator FlashKills()
    {
        killsLabel.RemoveFromClassList("normal");
        killsLabel.AddToClassList("warning");

        yield return new WaitForSeconds(0.2f);

        killsLabel.RemoveFromClassList("warning");
        killsLabel.AddToClassList("normal");
    }
    
    // ------------------------- WEAPONS SLOTS UI ------------------------- //
    
    public void OnSlotSelected(WeaponSlot slot)
    {
        switch (slot)
        {
            case WeaponSlot.Knife:
                OnSlot0Selected();
                break;
            case WeaponSlot.Primary:
                OnSlot1Selected();
                break;
            case WeaponSlot.Secondary:
                OnSlot2Selected();
                break;
            case WeaponSlot.Utility:
                OnSlot3Selected();
                break;
            default: Debug.Log("Slot not found");
                break;
        }
    }

    public void OnSlotFull(WeaponSlot slot, Texture2D icon)
    {
        // TODO specific weapon texture received from PlayerController
        switch (slot)
        {
            case WeaponSlot.Primary: OnSlot1Full(icon); 
                break;
            case WeaponSlot.Secondary: OnSlot2Full(icon);
                break;
            case WeaponSlot.Utility: OnSlot3Full(icon);
                break;
            default: Debug.Log("Slot not found");
                break;
        }
    }

    public void OnSlotEmpty(WeaponSlot slot)
    {
        switch (slot)
        {
            case WeaponSlot.Primary: OnSlot1Empty();
                break;
            case WeaponSlot.Secondary: OnSlot2Empty();
                break;
            case WeaponSlot.Utility: OnSlot3Empty();
                break;
            default: Debug.Log("Slot not found");
                break;
        }
    }

    public void OnSlot0Selected()
    {
        knifeSlot.EnableInClassList("selected", true);
        knifeSlot.EnableInClassList("normal", false);
        
        primaryFirearmSlot.EnableInClassList("selected", false);
        primaryFirearmSlot.EnableInClassList("normal", true);
        
        secondaryFirearmSlot.EnableInClassList("selected", false);
        secondaryFirearmSlot.EnableInClassList("normal", true);
        
        utilitySlot.EnableInClassList("selected", false);
        utilitySlot.EnableInClassList("normal", true);
    }
    
    public void OnSlot1Selected()
    {
        primaryFirearmSlot.EnableInClassList("selected", true);
        primaryFirearmSlot.EnableInClassList("normal", false);
        
        knifeSlot.EnableInClassList("selected", false);
        knifeSlot.EnableInClassList("normal", true);
        
        secondaryFirearmSlot.EnableInClassList("selected", false);
        secondaryFirearmSlot.EnableInClassList("normal", true);
        
        utilitySlot.EnableInClassList("selected", false);
        utilitySlot.EnableInClassList("normal", true);
    }

    public void OnSlot2Selected()
    {
        secondaryFirearmSlot.EnableInClassList("selected", true);
        secondaryFirearmSlot.EnableInClassList("normal", false);
        
        knifeSlot.EnableInClassList("selected", false);
        knifeSlot.EnableInClassList("normal", true);
        
        primaryFirearmSlot.EnableInClassList("selected", false);
        primaryFirearmSlot.EnableInClassList("normal", true);
        
        utilitySlot.EnableInClassList("selected", false);
        utilitySlot.EnableInClassList("normal", true);
    }
    
    public void OnSlot3Selected()
    {
        utilitySlot.EnableInClassList("selected", true);
        utilitySlot.EnableInClassList("normal", false);
        
        knifeSlot.EnableInClassList("selected", false);
        knifeSlot.EnableInClassList("normal", true);
        
        secondaryFirearmSlot.EnableInClassList("selected", false);
        secondaryFirearmSlot.EnableInClassList("normal", true);
        
        primaryFirearmSlot.EnableInClassList("selected", false);
        primaryFirearmSlot.EnableInClassList("normal", true);
    }

    public void OnSlot1Empty()
    {
        primaryFirearmSlot.style.backgroundImage = StyleKeyword.None;
    }
    public void OnSlot2Empty()
    {
        secondaryFirearmSlot.style.backgroundImage = StyleKeyword.None;
    }
    public void OnSlot3Empty()
    {
        utilitySlot.style.backgroundImage = StyleKeyword.None;
    }

    public void OnSlot1Full(Texture2D icon)
    {
        primaryFirearmSlot.style.backgroundImage = new StyleBackground(icon);
    }

    public void OnSlot2Full(Texture2D icon)
    {
        secondaryFirearmSlot.style.backgroundImage = new StyleBackground(icon);
    }
    
    public void OnSlot3Full(Texture2D icon)
    {
        utilitySlot.style.backgroundImage = new StyleBackground(icon);
    }
    
}

