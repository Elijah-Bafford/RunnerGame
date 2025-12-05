using System;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-3)]
public class RecordHandler : MonoBehaviour {
    public static RecordHandler Instance { get; private set; }

    [SerializeField] private LevelRecord[] records;
    [SerializeField] private int[] numItemsOnLevel;

    public static event Action<int, LevelRecord, bool, bool> OnRecordUpdated;

    private string filePath;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        filePath = Path.Combine(Application.persistentDataPath, "records.json");

        if (File.Exists(filePath)) {
            LoadRecords();

            if (records.Length < SceneHandler.numLevels) {
                LevelRecord[] temp = new LevelRecord[SceneHandler.numLevels];
                for (int record_i = 0; record_i < temp.Length; record_i++) {
                    if (record_i < records.Length && records[record_i] != null) {
                        temp[record_i] = records[record_i];
                    } else {
                        bool unlocked = (temp[record_i - 1].fastestTime > 0 && record_i > 1) || record_i == 2;
                        temp[record_i] = new LevelRecord(record_i, 0f, 0f, 0, unlocked);
                    }
                }
                records = temp;
            }

        } else {
            records = new LevelRecord[SceneHandler.numLevels];
            for (int i = 0; i < records.Length; i++) {
                bool unlocked = i == 2;
                records[i] = new LevelRecord(i, 0f, 0f, 0, unlocked);
            }
            SaveRecords();
        }
        BootstrapProcess.ProcessFinished(gameObject);
    }

    /// <summary>Increment the record's number of items collected by 'value'.</summary>
    /// <param name="level">The current level (SceneHandler.currentLevel)</param>
    /// <param name="value">The value to add to the number of items collected. (Default: 1)</param>
    public void UpdateItemsCollected(int level, int value = 1) {
        var rec = records[level];
        rec.itemsCollected += value;
        SaveRecords();
    }

    public string ItemsCollectedToString(int level) {
        if (level > records.Length) return "ERROR!";
        return records[level].itemsCollected + "/" + numItemsOnLevel[level];
    }

    public void UpdateRecord(int level, float newTime, float newMomentum) {
        // grab the existing record
        var rec = records[level];

        // determine if we've beaten the time record (or if it's the first run)
        bool isTimeRecord = rec.fastestTime == 0f || newTime < rec.fastestTime;
        float bestTime = isTimeRecord ? newTime : rec.fastestTime;

        // determine if we've beaten the momentum record
        bool isMomentumRecord = newMomentum > rec.highestMomentum;
        float bestMomentum = isMomentumRecord ? newMomentum : rec.highestMomentum;

        // update in place, preserve/unlock state
        rec.fastestTime = bestTime;
        rec.highestMomentum = bestMomentum;
        rec.unlocked = true;

        // unlock next level if it exists
        if (level + 1 < records.Length)
            records[level + 1].unlocked = true;

        SaveRecords();

        // notify listeners
        OnRecordUpdated?.Invoke(level, rec, isTimeRecord, isMomentumRecord);
    }

    public LevelRecord GetRecord(int level) {
        if (level >= 0 && level < records.Length) {
            return records[level];
        }
        return null;
    }

    public void SaveRecords() {
        RecordData data = new RecordData { records = records };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Records saved to: " + filePath);
    }

    public void LoadRecords() {
        if (!File.Exists(filePath)) {
            Debug.LogWarning("No records file found, creating new records array.");
            records = new LevelRecord[SceneHandler.numLevels];
            return;
        }
        string json = File.ReadAllText(filePath);
        RecordData data = JsonUtility.FromJson<RecordData>(json);
        if (data != null && data.records != null) {
            records = data.records;
            Debug.Log("Records loaded from: " + filePath);
        } else {
            Debug.LogWarning("Failed to load records, creating new ones.");
            records = new LevelRecord[SceneHandler.numLevels];
            for (int i = 0; i < records.Length; i++) {
                records[i] = new LevelRecord(i, 0f, 0f, 0, false);
            }
            if (records.Length > 2) records[2].unlocked = true;
            SaveRecords();
        }
    }
}

[System.Serializable]
public class LevelRecord {
    public int level;
    public float fastestTime;
    public float highestMomentum;
    public int itemsCollected;
    public bool unlocked;

    public LevelRecord(int level, float fastestTime, float highestMomentum, int itemsCollected, bool unlocked) {
        this.level = level;
        this.fastestTime = fastestTime;
        this.highestMomentum = highestMomentum;
        this.itemsCollected = itemsCollected;
        this.unlocked = unlocked;
    }
}

public class RecordData {
    public LevelRecord[] records;
}