using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameStateHandler : MonoBehaviour {

    [Header("Pause Overlay Refs")]
    [SerializeField] GameObject pauseOverlay;
    [SerializeField] TextMeshProUGUI timeNum;
    [SerializeField] TextMeshProUGUI speedNum;

    [Header("General Refs")]
    [SerializeField] GameTimer gameTimer;
    [SerializeField] PlayerInput playerInput;

    [Header("Other Overlay Refs")]
    [SerializeField] GameObject deathOverlay;
    [SerializeField] GameObject levelCompleteOverlay;

    [SerializeField] private List<OverlaySet> overlayDefinition;
    private Dictionary<GameState, OverlaySet> overlaySets = new();

    [System.Serializable]
    public struct OverlaySet {
        public GameState gameState;
        public GameObject overlay;
        public GameObject firstSelected;
    }

    private string fastestTime;
    private string highestMomentum;

    public static event Action OnLevelRestart;

    private static bool gameOver = false;
    public static void GameOver() => gameOver = true;

    public enum GameState { MainMenu, Playing, Paused, LevelRestart, Death, LevelComplete, NextLevel }

    private GameState state = GameState.Playing;
    private GameState lastState;



    public static EventSystem SceneEventSystem { get; private set; }
    public static GameStateHandler Instance { get; private set; }

    private void Awake() {
        Instance = this;
        SceneEventSystem = FindFirstObjectByType<EventSystem>();

        foreach (var set in overlayDefinition) {
            if (!overlaySets.ContainsKey(set.gameState))
                overlaySets.Add(set.gameState, set);
        }

        foreach (var kvp in overlaySets) {
            if (kvp.Value.overlay != null)
                kvp.Value.overlay.SetActive(false);
        }
    }

    private void Start() {
        SceneHandler.OnLevelLoad += OnLevelLoad;
        UpdateLevelStats(SceneHandler.currentLevel);
    }

    private void Update() {
        if (gameOver) {
            gameOver = false;
            SetGameState(GameState.Death);
        }
        if (state != lastState) {
            lastState = state;
            UpdateGameState();
        }
    }

    private void OnDestroy() => SceneHandler.OnLevelLoad -= OnLevelLoad;

    private void OnLevelLoad(int level) => UpdateLevelStats(level);

    private void UpdateLevelStats(int level) {
        LevelRecord thisRecord = RecordHandler.Instance.GetRecord(level);

        if (thisRecord != null) {
            if (thisRecord.fastestTime == 0) {
                fastestTime = "None";
                highestMomentum = "None";
                return;
            }
            fastestTime = GameTimer.GetTimeAsString(false, thisRecord.fastestTime);
            highestMomentum = thisRecord.highestMomentum.ToString();
        }
    }


    private void UpdateGameState() {
        switch (state) {
            case GameState.MainMenu:
                // No in-game overlay here, just go to main menu scene
                ToggleMenuMode(true);
                playerInput.SwitchCurrentActionMap("UI");
                Time.timeScale = 1f;
                SceneHandler.Instance.LoadLevel(1); // Main menu
                gameTimer.ResetTimer();
                break;

            case GameState.Playing:
                // No overlay visible, game running
                ShowOverlayForState(GameState.Playing, false);
                break;

            case GameState.Paused:
                // Show pause overlay & stats, freeze gameplay
                ShowOverlayForState(GameState.Paused, true);
                if (timeNum != null) timeNum.text = fastestTime;
                if (speedNum != null) speedNum.text = highestMomentum;
                break;

            case GameState.LevelRestart:
                OnLevelRestart?.Invoke();
                gameTimer.ResetTimer();
                ShowOverlayForState(GameState.Playing, false);
                state = GameState.Playing;
                break;

            case GameState.Death:
                gameTimer.ResetTimer();
                ShowOverlayForState(GameState.Death, true);
                break;

            case GameState.LevelComplete:
                ShowOverlayForState(GameState.LevelComplete, true);
                break;

            case GameState.NextLevel:
                ShowOverlayForState(GameState.Playing, false);
                SceneHandler.Instance.LoadLevel(SceneHandler.currentLevel + 1);
                gameTimer.ResetTimer();
                break;
        }
    }

    /// <summary>
    /// Generic overlay handler:
    /// - toggles menu mode (pause, cursor, audio, input map)
    /// - disables all overlays
    /// - enables the overlay for the given state (if defined)
    /// - sets first-selected button in EventSystem
    /// </summary>
    private void ShowOverlayForState(GameState newState, bool inMenuMode) {
        ToggleMenuMode(inMenuMode);

        // Disable all overlays
        foreach (var kvp in overlaySets) {
            if (kvp.Value.overlay != null)
                kvp.Value.overlay.SetActive(false);
        }

        // Enable the overlay for this state (if it has one)
        if (!(overlaySets.TryGetValue(newState, out var set) && set.overlay != null)) return;
        set.overlay.SetActive(true);

        if (SceneEventSystem != null && set.firstSelected != null)
            SceneEventSystem.SetSelectedGameObject(set.firstSelected);
    }


    /// <summary>
    /// Set in menu mode. Switches control scheme, shows/hides cursor, and pauses the game timer.
    /// </summary>
    /// <param name="inMenuMode"></param>
    private void ToggleMenuMode(bool inMenuMode) {
        AudioHandler.Instance.SetPauseAll(inMenuMode);
        Cursor.lockState = inMenuMode ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inMenuMode;

        string input = inMenuMode ? "UI" : "Player";
        float newTime = inMenuMode ? 0 : 1;

        if (newTime == 1 && NotificationHandler.NotificationTimeScale != 1) {
            newTime = NotificationHandler.NotificationTimeScale;
        }

        playerInput.SwitchCurrentActionMap(input);
        Time.timeScale = newTime;
        gameTimer.RunTimer(!inMenuMode);
    }

    public void SetGameState(GameState gameState) => state = gameState;

    /*=====================================================================================
     *                                  Input events
     =====================================================================================*/
    public void OnPauseKeyPressed(InputAction.CallbackContext context) {
        if (context.performed) {
            if (state == GameState.Death || state == GameState.LevelComplete) return;
            if (state == GameState.Playing) state = GameState.Paused;
            else state = GameState.Playing;
        }
    }

    private void UISound() { AudioHandler.Instance.PlaySound(SoundType.UISelect); }

    public void ButtonUnpause() { UISound(); state = GameState.Playing; }
    public void ButtonQuit() { UISound(); state = GameState.MainMenu; }
    public void ButtonRestart() { UISound(); state = GameState.LevelRestart; }
    public void ButtonContinue() { UISound(); state = GameState.NextLevel; }
    public void TriggerLevelComplete() { state = GameState.LevelComplete; }
}
