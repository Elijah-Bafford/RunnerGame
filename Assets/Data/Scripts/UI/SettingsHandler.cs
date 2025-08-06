using UnityEngine;

[DefaultExecutionOrder(-3)]
public class SettingsHandler : MonoBehaviour {
    public static SettingsHandler Instance { get; private set; }



    private string filePath;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
