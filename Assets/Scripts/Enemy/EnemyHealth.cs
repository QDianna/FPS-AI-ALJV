using System;
using System.Collections;
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
    
    public event Action<float, Vector3> OnDamageTaken;
    
    void Start()
    {
        health = maxHealth;
        
        UpdateUI();
    }
    
    
    public void TakeDamage(float amount, Vector3 attackerPos)
    {
        health = Mathf.Max(0f, health - amount);
        
        if (health <= 0f)
        {
            health = maxHealth;
            OnDeath();
            return;
        }
        
        OnDamageTaken?.Invoke(amount, attackerPos);
        
        UpdateUI();
    }

    public float health { get; set; }

    private void UpdateUI()
    {
        if (healthBar)
            healthBar.value = health / maxHealth;

        if (tmp != null && tmp.gameObject.activeInHierarchy)
            tmp.text = health + " HP";
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

        Debug.Log("Enemy dead, resetting episode...");
        GameManager.Instance.ResetEpisode();
    }
    
    
    public void ResetHealth()
    {
        health = maxHealth;
        UpdateUI();
    }
}

