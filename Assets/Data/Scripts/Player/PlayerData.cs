using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-3)]
public class PlayerData : MonoBehaviour {

    public static PlayerData Data { get; private set; }

    [System.Serializable]
    public class PlayerStats {
        public float StartFocus = 0f;
        public float MaxFocus = 25f;
        public float MomentumCap = 2f;
        public float BaseMovementSpeed = 5f;
        public float JumpForce = 4.8f;
        public float FocusLossMult = 2f;
        public float AttackDamage = 10f;
        public List<int> CollectedItems = new();
    }

    public PlayerStats Stats;

    private static string filePath;

    private void Awake() {
        Data = this;
        filePath = Path.Combine(Application.persistentDataPath, "playerData.json");

        // Read existing stats, or create a new file
        if (!File.Exists(filePath) || Stats == null) WriteStats();
        else ReadStats();

    }

    public bool IsCollected(int item) => Stats.CollectedItems.Contains(item);
    public void CollectItem(int item) => Stats.CollectedItems.Add(item);

    public void WriteStats() {
        string json = JsonUtility.ToJson(Stats, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"Player data saved to: {filePath}");
    }

    public void ReadStats() {
        if (!File.Exists(filePath)) {
            Debug.LogWarning("Player data file missing, creating new default stats.");
            WriteStats();
            return;
        }

        string json = File.ReadAllText(filePath);
        Stats = JsonUtility.FromJson<PlayerStats>(json);

        Debug.Log($"Player data loaded from: {filePath}");
    }
}
