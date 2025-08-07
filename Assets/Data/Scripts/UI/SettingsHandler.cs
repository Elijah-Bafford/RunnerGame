using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-3)]
public class SettingsHandler : MonoBehaviour {
    public static SettingsHandler Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Toggle showTutorialsToggle;

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
        // Load settings on start
    }
    private void SaveSettings() {
        // Save to json
    }

    private void UISound() { AudioHandler.Instance.PlaySound(SoundType.UISelect); }

    // Settings events

    public void OnShowTutorialsChanged(bool isOn) { UISound(); NotificationHandler.disableTutorials = !isOn; }
    public void OnSettingsMenuClosed() { SaveSettings(); }
}