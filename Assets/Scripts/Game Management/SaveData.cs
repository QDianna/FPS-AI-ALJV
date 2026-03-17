using System.IO;
using UnityEngine;

public static class SaveData
{
    public static GameData gameData;

    // C:\Users\elena\AppData\LocalLow\DefaultCompany\IPJV
    private static string dataFilePath = Path.Combine(Application.persistentDataPath, "SaveGameData.json");
    
    public static void SaveGame()
    {
        if (!PlayerController.Instance || !GameManager.Instance)
        {
            Debug.LogWarning("SaveData not working without player or game manager");
            return;
        }
        gameData = new GameData();
        gameData.health = PlayerController.Instance.currentHealth;
        gameData.kills = PlayerController.Instance.kills;
        
        // This creates a new StreamWriter to write to a specific file path
        using (StreamWriter writer = new StreamWriter(dataFilePath))
        {
            // This will convert our Data object into a string of JSON
            string dataToWrite = JsonUtility.ToJson(gameData);
 
            // This is where we actually write to the file
            writer.Write(dataToWrite);
        }
    }
    
    
    public static void LoadGame()
    {
        if (!File.Exists(dataFilePath))
            return;

        string dataToLoad = File.ReadAllText(dataFilePath);
        gameData = JsonUtility.FromJson<GameData>(dataToLoad);

        PlayerController.Instance.currentHealth = gameData.health;
        PlayerController.Instance.kills = gameData.kills;
    }
}
