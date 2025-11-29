using System;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-3)]
public class RecordHandler : MonoBehaviour {
    public static RecordHandler Instance { get; private set; }

    [SerializeField] private LevelRecord[] records;

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
                for (int i = 0; i < temp.Length; i++) {
                    if (i < records.Length && records[i] != null) {
                        temp[i] = records[i];
                    } else {
                        bool unlocked = (temp[i - 1].fastestTime > 0 && i > 1) || i == 1;
                        temp[i] = new LevelRecord(i, 0f, 0f, unlocked);
                    }
                }
                records = temp;
            }

        } else {
            records = new LevelRecord[SceneHandler.numLevels];
            for (int i = 0; i < records.Length; i++) {
                records[i] = new LevelRecord(i, 0f, 0f, false);
            }
            if (records.Length > 1) records[1].unlocked = true;
            SaveRecords();
        }
        BootstrapProcess.ProcessFinished(gameObject);
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
                records[i] = new LevelRecord(i, 0f, 0f, false);
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
    public bool unlocked;

    public LevelRecord(int level, float fastestTime, float highestMomentum, bool unlocked) {
        this.level = level;
        this.fastestTime = fastestTime;
        this.highestMomentum = highestMomentum;
        this.unlocked = unlocked;
    }

    public override string ToString() {
        return
            $"Level: {level}\n" +
            $"Fastest Time: {fastestTime}\n" +
            $"Highest Momentum: {highestMomentum}";
    }
}

public class RecordData {
    public LevelRecord[] records;
}