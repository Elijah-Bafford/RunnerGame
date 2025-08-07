using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour {
    public static SettingsHandler Instance { get; private set; }

    [SerializeField] private GameSettings currentSettings = new GameSettings();

    [Header("UI References")]
    [SerializeField] private Toggle showTutorialsToggle;

    [SerializeField] private Slider volumeSlider;

    [SerializeField] private TMP_Dropdown refreshRateDropdown;

    // Resolution
    [Header("Resolution and Fullscreen")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private readonly Vector2Int[] allowedResolutions = new Vector2Int[] {
        new Vector2Int(1280, 720),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),
    };

    private readonly int[] allowedRefreshRates = new int[] {
        30, 60, 120, 144, 165, 240
    };

    private int highestRefreshRate;

    private Resolution[] resolutions;
    private List<Resolution> selectedResolutionList = new();
    private List<RefreshRate> selectedRefreshRateList = new();

    private List<double> refreshRates = new();

    // Json file
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
        BuildResolutions();
        BuildRefreshRates();
        LoadSettings();
    }

    private void BuildResolutions() {
        resolutionDropdown.ClearOptions();
        selectedResolutionList.Clear();

        resolutions = Screen.resolutions;
        List<string> resStringList = new List<string>();

        foreach (var resolution in resolutions) {
            highestRefreshRate = Mathf.RoundToInt((float)resolution.refreshRateRatio.value);
            Vector2Int res = new Vector2Int(resolution.width, resolution.height);
            if (allowedResolutions.Contains(res)) {
                string resString = $"{resolution.width}x{resolution.height}";
                if (!resStringList.Contains(resString)) {
                    resStringList.Add(resString);
                    selectedResolutionList.Add(resolution);
                }
            }
        }
        resolutionDropdown.AddOptions(resStringList);
    }

    private void BuildRefreshRates() {
        refreshRateDropdown.ClearOptions();
        refreshRates.Clear();
        List<string> refreshRateList = new List<string>();

        foreach (var refreshRate in allowedRefreshRates) {
            // Only allow refresh rates that the monitor can handle
            if (refreshRate <= highestRefreshRate) {
                RefreshRate rate = new RefreshRate();
                rate.numerator = (uint)refreshRate;
                rate.denominator = 1;
                selectedRefreshRateList.Add(rate);
                refreshRateList.Add(rate.ToString());
            }

        }
        refreshRateDropdown.AddOptions(refreshRateList);
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

        // Find matching index in dropdown
        int resIndex = selectedResolutionList.FindIndex(r =>
            r.width == currentSettings.resolutionWidth &&
            r.height == currentSettings.resolutionHeight);

        int rrIndex = selectedRefreshRateList.FindIndex(r =>
        r.numerator == (uint)currentSettings.refreshRate);

        RefreshRate rate = new() { numerator = 30, denominator = 1 };
        if (rrIndex != -1) {
            rate = new() { numerator = (uint)currentSettings.refreshRate, denominator = 1 };
            refreshRateDropdown.value = rrIndex;
            refreshRateDropdown.RefreshShownValue();
        }
        
        if (resIndex != -1) {
            resolutionDropdown.value = resIndex;
            resolutionDropdown.RefreshShownValue();
            Screen.SetResolution(
                currentSettings.resolutionWidth,
                currentSettings.resolutionHeight,
                currentSettings.isFullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed,
                rate
            );
        } else {
            Debug.LogWarning("Saved resolution not found in allowed list.");
        }

    }

    /// <summary>
    /// Save to json
    /// </summary>
    private void SaveSettings() {
        currentSettings.showTutorials = showTutorialsToggle.isOn;
        currentSettings.volume = volumeSlider.value;
        currentSettings.isFullscreen = fullscreenToggle.isOn;

        Resolution selectedRes = selectedResolutionList[resolutionDropdown.value];
        currentSettings.resolutionWidth = selectedRes.width;
        currentSettings.resolutionHeight = selectedRes.height;

        RefreshRate rate = selectedRefreshRateList[refreshRateDropdown.value];
        currentSettings.refreshRate = (int)rate.numerator;

        string json = JsonUtility.ToJson(currentSettings, true);
        File.WriteAllText(filePath, json);
    }

    private void UISound(bool allowOverlap = true) { AudioHandler.Instance.PlaySound(SoundType.UISelect, allowOverlap); }

    // Settings events

    public void OnShowTutorialsChanged(bool isOn) { UISound(); NotificationHandler.disableTutorials = !isOn; }
    public void OnVolumeChanged() { UISound(allowOverlap: false); AudioHandler.Instance.SetVolume(volumeSlider.value); }
    public void OnSettingsMenuClosed() { SaveSettings(); }
    public void OnFullscreenChanged(bool isOn) {
        UISound();
        Screen.fullScreen = isOn;

    }
    public void OnResolutionChanged() {
        UISound();
        int index = resolutionDropdown.value;
        Resolution res = selectedResolutionList[index];
        Screen.SetResolution(
            res.width,
            res.height,
            Screen.fullScreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed,
            new RefreshRate { numerator = (uint)currentSettings.refreshRate, denominator = 1 }
        );

        currentSettings.resolutionWidth = res.width;
        currentSettings.resolutionHeight = res.height;
    }

    public void OnRefreshRateChanged() {
        UISound();
        int index = refreshRateDropdown.value;

        RefreshRate refreshRate = selectedRefreshRateList[index];
        Screen.SetResolution(
            currentSettings.resolutionWidth,
            currentSettings.resolutionHeight,
            Screen.fullScreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed,
            refreshRate
        );

        currentSettings.refreshRate = (int)refreshRate.numerator;
    }

}

[System.Serializable]
public class GameSettings {
    public bool showTutorials = true;
    public float volume = 1.0f;
    public bool isFullscreen = true;
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public int refreshRate = 60;
}