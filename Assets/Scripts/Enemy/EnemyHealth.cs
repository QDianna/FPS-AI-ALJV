using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI tmp;
    
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [HideInInspector] public float health;
    
    private EnemyCombatAI combatAI;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    void Awake()
    {
        health = maxHealth;
        combatAI = GetComponentInParent<EnemyCombatAI>();
    }
    
    void Start()
    {
        UpdateUI();
    }
    
    public void TakeDamage(float amount, Vector3 attackerPos)
    {
        health = Mathf.Max(0f, health - amount);

        // notify combat AI
        combatAI?.RegisterTookDamage();

        UpdateUI();

        if (health <= 0f)
        {
            OnDeath();
        }
    }
    
    private void UpdateUI()
    {
        if (healthBar)
            healthBar.value = health / maxHealth;

        if (tmp != null && tmp.gameObject.activeInHierarchy)
            tmp.text = "HP: " + health;
    }

    private void OnDeath()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.kills++;
            PlayerController.Instance.money += PlayerController.Instance.moneyPerKill;
        }

        GameUI.Instance.UpdateKillsUI();
        GameUI.Instance.UpdateMoneyUI();

        Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}

