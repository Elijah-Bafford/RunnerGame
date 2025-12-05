using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Navigation")]
    [SerializeField] private FirstSelectLink[] pageFirstSelectConfig;
    private Dictionary<MenuType, GameObject> pageFirstSelectDict;
    private EventSystem eventSystem;

    public enum MenuType { TitleMenu, LevelSelect, Credits, Settings }
    private MenuType currentMenu = MenuType.TitleMenu;
    private GameObject[] menus = new GameObject[Enum.GetValues(typeof(MenuType)).Length];

    [System.Serializable]
    public struct FirstSelectLink {
        public MenuType menuType;
        public GameObject selectable;
    }

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
        ShowMenu(currentMenu, switchSelectable: false);
    }

    private void OnEnable() {
        eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null) Debug.LogWarning("Event System is null", this);
    }

    private void Start() {
        RecordHandler.OnRecordUpdated += HandleRecordUpdated;
        SceneHandler.OnLevelLoad += OnLevelLoad;
        RefreshLevelButtons();
        CreateNavigationButtons();
    }

    #region Level Selectable Buttons

    private void HandleRecordUpdated(int level, LevelRecord record, bool isTimeRecord, bool isMomentumRecord) {
        RefreshLevelButtons();
        CreateNavigationButtons();
    }

    private void RefreshLevelButtons() {
        // Clear existing buttons
        foreach (Transform child in scrollContent) {
            Destroy(child.gameObject);
        }

        // Recreate all buttons
        for (int i = 0; i < SceneHandler.numLevels; i++) {
            if (i == 0 || i == 1) continue; // Bootstrap and main menu

            GameObject btnObj = Instantiate(levelButtonPrefab, scrollContent);
            LevelButtonUI buttonUI = btnObj.GetComponent<LevelButtonUI>();

            if (i == 2) {
                pageFirstSelectConfig[3].selectable = btnObj;

                buttonUI.levelText.text = $"Tutorial";
            } else {
                buttonUI.levelText.text = $"Level {i - 2}";
            }

            LevelRecord record = RecordHandler.Instance.GetRecord(i);

            buttonUI.momentumNum.text = record.highestMomentum == 0 && record != null ? "None" : "x" + record.highestMomentum.ToString("F3");
            buttonUI.timeNum.text = record.fastestTime == 0 && record != null ? "None" : GameTimer.GetTimeAsString(false, record.fastestTime);
            buttonUI.itemsCollected.text = RecordHandler.Instance.ItemsCollectedToString(i);
            int levelIndex = i;
            if (!record.unlocked) {
                buttonUI.button.interactable = false;
            } else {
                buttonUI.button.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
            }
        }
    }

    #endregion

    private void CreateNavigationButtons() {
        pageFirstSelectDict = new Dictionary<MenuType, GameObject>();
        for (int i = 0; i < pageFirstSelectConfig.Length; i++)
            pageFirstSelectDict.Add(pageFirstSelectConfig[i].menuType, pageFirstSelectConfig[i].selectable);
    }

    private void OnLevelLoad(int level) {
        if (level == 1) {
            MainMenuUI.SetActive(true);

            RecordHandler.Instance.LoadRecords();

            CreateNavigationButtons();
            ShowMenu(MenuType.TitleMenu);
        } else {
            AudioHandler.Instance.StopAll();
        }
    }

    private void UISound() => AudioHandler.Instance.PlaySound(SoundType.UISelect);
    

    private void OnLevelButtonClicked(int levelIndex) {
        UISound();
        SceneHandler.Instance.LoadLevel(levelIndex);
        MainMenuUI.SetActive(false);
    }

    private void ShowMenu(MenuType menu, bool switchSelectable = true) {
        menus[(int)currentMenu].SetActive(false);
        menus[(int)menu].SetActive(true);
        currentMenu = menu;
        if (switchSelectable) {
            if (pageFirstSelectDict.TryGetValue(menu, out GameObject go))
                if (eventSystem != null)
                    eventSystem.SetSelectedGameObject(go);
        }
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