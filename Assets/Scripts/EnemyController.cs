using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI tmp;
    
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform gun;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform aimTarget;
    
    [Header("Parameters")]
    [SerializeField] private float radius = 10f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float fireRate = 3f;
    [SerializeField] private float rotationSpeed = 90f;
    
    private float health = 100f;
    private float maxHealth = 100f;

    private float fireTimer;
    
    private float minPitch = -60f;
    private float maxPitch = 60f;
    float yawTolerance = 5f;
    float pitchTolerance = 5f;

    void Start()
    {
        if (!PlayerController.Instance)
            Debug.Log("Enemy Controller: player instance not found");
        else
            aimTarget = PlayerController.Instance.transform;
        
        if (tmp)
            tmp.text = "HP: " + health;
    }
    
    void Update()
    {
        float dist = Vector3.Distance(transform.position, aimTarget.position);
        if (dist > radius)
            return;

        // ROTATE TOWER BASE ON OY
        Vector3 dirToPlayer = aimTarget.position - transform.position;
        dirToPlayer.y = 0f;

        if (dirToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetYaw = Quaternion.LookRotation(dirToPlayer);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetYaw,
                rotationSpeed * Time.deltaTime
            );
        }

        // ROTATE GUN ON OX
        Vector3 dirWorld = aimTarget.position - gun.position;

        Vector3 dirLocal = gun.parent.InverseTransformDirection(dirWorld);
        dirLocal.Normalize();

        float targetPitch = Mathf.Atan2(-dirLocal.y, dirLocal.z) * Mathf.Rad2Deg;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        Quaternion gunTargetRot = Quaternion.Euler(targetPitch, 0f, 0f);

        gun.localRotation = Quaternion.RotateTowards(
            gun.localRotation,
            gunTargetRot,
            rotationSpeed * Time.deltaTime
        );

        // FIRE TURRET GUN
        fireTimer += Time.deltaTime;

        float yawError = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dirToPlayer));
        float pitchError = Mathf.Abs(targetPitch - gun.localEulerAngles.x);

        if (fireTimer >= fireRate && yawError < yawTolerance && pitchError < pitchTolerance)
        {
            Fire();
            fireTimer = 0f;
        }
    }

    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        if (!projectile)
        {
            Debug.Log("Projectile not instantiated");
            return;
        }

        EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();
        enemyProjectile.Fire(firePoint, damage);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        
        if (healthBar)
            healthBar.value = health / maxHealth;
        
        if (tmp)
            tmp.text = "HP: " + health;

        if (health <= 0f)
        {
            PlayerController.Instance.kills ++;
            PlayerController.Instance.money += PlayerController.Instance.moneyPerKill;
            
            GameUI.Instance.UpdateKillsUI();
            GameUI.Instance.UpdateMoneyUI();
            
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
