using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour {
    public static MainMenu Instance { get; private set; }

    [Header("Level Buttons")]
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private RectTransform scrollContent;

    [Header("Menus")]
    [SerializeField] private GameObject MainMenuUI;
    [SerializeField] private GameObject titleMenu;
    [SerializeField] private GameObject levelSelectMenu;
    [SerializeField] private GameObject creditsMenu;
    [SerializeField] private GameObject settingsMenu;

    private enum MenuType { TitleMenu, LevelSelect, Credits, Settings }
    private MenuType currentMenu = MenuType.TitleMenu;
    private GameObject[] menus = new GameObject[Enum.GetValues(typeof(MenuType)).Length];

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        menus[(int)MenuType.TitleMenu] = titleMenu;
        menus[(int)MenuType.LevelSelect] = levelSelectMenu;
        menus[(int)MenuType.Credits] = creditsMenu;
        menus[(int)MenuType.Settings] = settingsMenu;

        for (int i = 0; i < menus.Length; i++) {
            if (menus[i] != menus[(int)MenuType.TitleMenu] && menus[i] != null) {
                menus[i].SetActive(false);
            }
        }
        currentMenu = MenuType.TitleMenu;
        ShowMenu(currentMenu);
    }

    private void Start() {
        RecordHandler.OnRecordUpdated += HandleRecordUpdated;
        SceneHandler.OnLevelLoad += OnLevelLoad;
        RefreshLevelButtons();
    }

    private void HandleRecordUpdated(int level, LevelRecord record, bool isTimeRecord, bool isMomentumRecord) {
        RefreshLevelButtons();
    }

    private void RefreshLevelButtons() {
        // Clear existing buttons
        foreach (Transform child in scrollContent) {
            Destroy(child.gameObject);
        }

        // Recreate all buttons
        for (int i = 0; i < SceneHandler.numLevels; i++) {
            if (i == 0) continue;

            GameObject btnObj = Instantiate(levelButtonPrefab, scrollContent);
            LevelButtonUI buttonUI = btnObj.GetComponent<LevelButtonUI>();

            if (i == 1) {
                buttonUI.levelText.text = $"Tutorial";
            } else {
                buttonUI.levelText.text = $"Level {i - 1}";
            }

            LevelRecord record = RecordHandler.Instance.GetRecord(i);

            buttonUI.momentumNum.text = record.highestMomentum == 0 && record != null ? "None" : record.highestMomentum.ToString();
            buttonUI.timeNum.text = record.fastestTime == 0 && record != null ? "None" : GameTimer.GetTimeAsString(false, record.fastestTime);

            int levelIndex = i;
            if (!record.unlocked) {
                buttonUI.button.interactable = false;
            } else {
                buttonUI.button.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
            }
        }
    }


    private void OnLevelLoad(int level) {
        if (level == 0) {
            MainMenuUI.SetActive(true);
            ShowMenu(MenuType.TitleMenu);
            RecordHandler.Instance.LoadRecords();

        } else {
            AudioHandler.Instance.SetPlaySoundLoop(SoundType.Slide, false);
        }
    }

    private void UISound() {
        AudioHandler.Instance.PlaySound(SoundType.UISelect);
    }

    private void OnLevelButtonClicked(int levelIndex) {
        UISound();
        SceneHandler.Instance.LoadLevel(levelIndex);
        MainMenuUI.SetActive(false);
    }

    private void ShowMenu(MenuType menu, bool show = true) {
        menus[(int)currentMenu].SetActive(!show);
        menus[(int)menu].SetActive(show);
        currentMenu = menu;
    }

    // Button events
    public void ReturnToTitle() { UISound(); ShowMenu(MenuType.TitleMenu); }
    public void StartGame() { UISound(); ShowMenu(MenuType.LevelSelect); }
    public void Settings() { UISound(); ShowMenu(MenuType.Settings); }
    public void Credits() { UISound(); ShowMenu(MenuType.Credits); }
    public void QuitGame() { UISound(); Application.Quit(); }

    // Input Actions

    public void I_ReturnToTitle(InputAction.CallbackContext cont) {
        if (cont.performed && currentMenu != MenuType.TitleMenu) {
            ReturnToTitle();
        }
    }

    public void QuitInEditor() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}