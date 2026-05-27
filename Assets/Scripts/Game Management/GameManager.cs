using System;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class GameData
{
    public float health = 100;
    public int kills;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Episode")]
    [SerializeField] public int currentEpisode = 1;

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private EnemyBehaviour enemyBehaviour;
    [SerializeField] private EnemyHealth enemyHealth;

    [Header("Spawn Points")]
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform enemySpawn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GameUI.Instance.UpdateRound();
    }

    // ----------------------------------- SCENE ----------------------------------- //

    public void TeleportPlayer(int toSceneId)
    {
        SaveData.SaveGame();
        SceneLoader.Instance.LoadSceneWithFade(toSceneId);
        SaveData.LoadGame();
    }

    // ----------------------------------- EPISODES ----------------------------------- //

    public void ResetEpisode()
    {
        currentEpisode ++;
        
        GameUI.Instance.UpdateRound();

        ResetPlayer();
        ResetEnemy();

        Debug.Log($"[EPISODE] Reset -> {currentEpisode}");
    }

    void ResetPlayer()
    {
        if (!player)
            return;

        CharacterController cc = player.characterController;

        if (cc)
            cc.enabled = false;

        player.transform.position = playerSpawn.position;
        player.transform.rotation = playerSpawn.rotation;

        if (cc)
            cc.enabled = true;

        player.ResetHealth();
    }

    void ResetEnemy()
    {
        if (!enemyBehaviour)
            return;

        NavMeshAgent agent = enemyBehaviour.movement.agent;

        if (agent != null)
            agent.enabled = false;

        enemyBehaviour.transform.position = enemySpawn.position;
        enemyBehaviour.transform.rotation = enemySpawn.rotation;

        if (agent != null)
            agent.enabled = true;

        enemyHealth.ResetHealth();
        
        enemyBehaviour.ResetAI();
    }
}

