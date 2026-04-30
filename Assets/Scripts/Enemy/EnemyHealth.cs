using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI tmp;
    
    public float health = 100f;
    private float maxHealth = 100f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (tmp)
            tmp.text = "HP: " + health;
    }
    
    public void TakeDamage(float amount)
    {
        health -= amount;
        
        GetComponentInParent<EnemyCombatAI>()?.RegisterTookDamage();
        
        if (healthBar)
            healthBar.value = health / maxHealth;
        
        if (tmp != null && tmp.gameObject.activeInHierarchy)
            tmp.text = "HP: " + health;

        if (health <= 0f)
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
    }
    
    private void Die()
    {
        Destroy(gameObject);
    }
}

