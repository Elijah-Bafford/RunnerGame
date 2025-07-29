using System.IO;
using UnityEngine;

public class RecordHandler : MonoBehaviour {
    public static RecordHandler Instance { get; private set; }

    [SerializeField] private LevelRecord[] records;

    private string filePath;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() {
        if (Instance == null) {
            var go = new GameObject("Record Handler");
            go.AddComponent<RecordHandler>();
            DontDestroyOnLoad(go);
        }
    }

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
        } else {
            records = new LevelRecord[SceneHandler.numLevels];
            for (int i = 0; i < records.Length; i++) {
                records[i] = new LevelRecord(i, 0f, 0f);
            }
        }
    }

    public void CreateRecord(int level, float fastestTime, float highestMomentum) {
        float ft = Mathf.Min(records[level].fastestTime, fastestTime);
        float hm = Mathf.Max(records[level].highestMomentum, highestMomentum);
        records[level] = new LevelRecord(level, ft, hm);
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
        }
    }
}

[System.Serializable]
public class LevelRecord {
    public int level;
    public float fastestTime;
    public float highestMomentum;

    public LevelRecord(int level, float fastestTime, float highestMomentum) {
        this.level = level;
        this.fastestTime = fastestTime;
        this.highestMomentum = highestMomentum;
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