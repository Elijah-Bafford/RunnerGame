using System;
using TMPro;
using UnityEngine;
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

    private string fastestTime;
    private string highestMomentum;

    public static event Action OnLevelRestart;

    public enum GameState { MainMenu, Playing, Paused, LevelRestart, Death, LevelComplete, NextLevel }

    private GameState state;
    private GameState lastState;

    private void Awake() {
        state = GameState.Playing;
    }

    private void Start() {
        SceneHandler.OnLevelLoad += OnLevelLoad;
        UpdateLevelStats(SceneHandler.currentLevel);
    }

    private void OnLevelLoad(int level) {
        UpdateLevelStats(level);
    }

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

    private void Update() {
        if (state != lastState) {
            lastState = state;
            UpdateGameState();
        }
    }

    private void UpdateGameState() {
        switch (state) {
            case GameState.MainMenu:
                playerInput.SwitchCurrentActionMap("UI");
                Time.timeScale = 1;
                SceneHandler.Instance.LoadLevel(1); // Main menu
                gameTimer.ResetTimer();
                break;
            case GameState.Playing:
                ShowPauseOverlay(false);
                break;
            case GameState.Paused:
                ShowPauseOverlay(true);
                break;
            case GameState.LevelRestart:
                OnLevelRestart?.Invoke();
                state = GameState.Playing;
                gameTimer.ResetTimer();
                ShowPauseOverlay(false);
                ShowDeathOverlay(false);
                break;
            case GameState.Death:
                gameTimer.ResetTimer();
                ShowPauseOverlay(false);
                ShowDeathOverlay(true);
                gameTimer.ResetTimer();
                break;
            case GameState.LevelComplete:
                ShowPauseOverlay(false);
                ShowDeathOverlay(false);
                ShowLevelCompleteOverlay(true);
                break;
            case GameState.NextLevel:
                ShowLevelCompleteOverlay(false);
                ShowPauseOverlay(false);
                ShowDeathOverlay(false);
                SceneHandler.Instance.LoadLevel(SceneHandler.currentLevel + 1);
                gameTimer.ResetTimer();
                break;
        }
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

        if (newTime == 1 && NotificationHandler.timeSlowValue != 1) {
            newTime = NotificationHandler.timeSlowValue;
        }

        playerInput.SwitchCurrentActionMap(input);
        Time.timeScale = newTime;
        gameTimer.RunTimer(!inMenuMode);
    }

    private void ShowDeathOverlay(bool active) {
        ToggleMenuMode(active);
        deathOverlay.SetActive(active);
    }

    private void ShowPauseOverlay(bool active) {
        ToggleMenuMode(active);
        pauseOverlay.SetActive(active);
        if (active) {
            speedNum.text = highestMomentum;
            timeNum.text = fastestTime;
        }
    }

    private void ShowLevelCompleteOverlay(bool active) {
        ToggleMenuMode(active);
        levelCompleteOverlay.SetActive(active);
    }

    public GameState GetGameState() { return state; }
    public void SetGameState(GameState gameState) { state = gameState; }

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
