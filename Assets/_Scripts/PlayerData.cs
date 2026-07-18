using System.IO;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }
    public GameData Data;

    private string _saveFilePath;

    [System.Serializable]
    public class GameData
    {
        // Basic Stats
        public int MaxHealth = 50;
        public float MoveSpeedMultiplier = 1.0f;
        public float JumpForceMultiplier = 1.0f;
        public int ExtraJumps = 0;
        public bool CanRangeAttack = false;

        public int RangeAttackRate = 1; // 1 bullet / second
        public int RangeAttackDamageMultiplier = 1;
        public float RangeAttackSpeedMultiplier = 1.0f;

        // Abilities
        public bool CanDash = false;
        public float DashForceMultiplier = 1.0f;
        public float DashDuration = 0.2f;

        // Others
        public float MasterVolume = 0.2f;
        public float MusicVolume = 0.2f;
        public float SoundFXVolume = 0.2f;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _saveFilePath = Path.Combine(Application.persistentDataPath, "SilentHeels.ridt");
        Load();
    }

    public void Load()
    {
        if (File.Exists(_saveFilePath))
        {
            string encrypted = File.ReadAllText(_saveFilePath);
            string json = SaveCrypto.Decrypt(encrypted);
            Data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("Game Loaded Successfully from: " + _saveFilePath);
        }
        else
        {
            // First time playing
            Data = new GameData();
            Debug.Log("No save file found");
        }
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(Data, true);
        string encrypted = SaveCrypto.Encrypt(json);
        File.WriteAllText(_saveFilePath, encrypted);
        Debug.Log("Game Saved Successfully to: " + _saveFilePath);
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}
