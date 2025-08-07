using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour {
    public static SettingsHandler Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Toggle showTutorialsToggle;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider volumeSlider;

    [SerializeField] private GameSettings currentSettings = new GameSettings();
    private string filePath;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        filePath = Path.Combine(Application.persistentDataPath, "settings.json");
    }

    private void Start() {
        LoadSettings();

    }

    private void LoadSettings() {
        if (File.Exists(filePath)) {
            string json = File.ReadAllText(filePath);
            currentSettings = JsonUtility.FromJson<GameSettings>(json);
        }

        // Apply loaded settings to UI
        showTutorialsToggle.isOn = currentSettings.showTutorials;
        volumeSlider.value = currentSettings.volume;

        ApplySettings();
    }

    private void ApplySettings() {
        NotificationHandler.disableTutorials = !currentSettings.showTutorials;
        AudioHandler.Instance.SetVolume(currentSettings.volume);

        fullscreenToggle.isOn = currentSettings.isFullscreen;
        Screen.fullScreen = currentSettings.isFullscreen;
    }
    private void SaveSettings() {
        // Save to json
        currentSettings.showTutorials = showTutorialsToggle.isOn;
        currentSettings.volume = volumeSlider.value;
        currentSettings.isFullscreen = fullscreenToggle.isOn;

        string json = JsonUtility.ToJson(currentSettings, true);
        File.WriteAllText(filePath, json);
    }

    private void UISound(bool allowOverlap = true) { AudioHandler.Instance.PlaySound(SoundType.UISelect, allowOverlap); }

    // Settings events

    public void OnShowTutorialsChanged(bool isOn) { UISound(); NotificationHandler.disableTutorials = !isOn; }
    public void OnFullscreenChanged(bool isOn) { UISound(); Screen.fullScreen = isOn; }
    public void OnVolumeChanged() { UISound(allowOverlap: false); AudioHandler.Instance.SetVolume(volumeSlider.value); }
    public void OnSettingsMenuClosed() { SaveSettings(); }
}

[System.Serializable]
public class GameSettings {
    public bool showTutorials = true;
    public float volume = 1.0f;
    public bool isFullscreen = true;
}