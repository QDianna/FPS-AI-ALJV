using UnityEngine;

[System.Serializable]
public class GameData
{
    public float health = 100;
    public int kills;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
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
    
    public void TeleportPlayer(int toSceneId)
    {
        SaveData.SaveGame();
        SceneLoader.Instance.LoadSceneWithFade(toSceneId);
        SaveData.LoadGame();
    }
}
