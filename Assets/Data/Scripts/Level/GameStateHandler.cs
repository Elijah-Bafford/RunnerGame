using UnityEngine;
using UnityEngine.InputSystem;

public class GameStateHandler : MonoBehaviour {

    [SerializeField] GameObject pauseOverlay;
    [SerializeField] GameObject deathOverlay;
    [SerializeField] GameObject levelCompleteOverlay;
    [SerializeField] GameTimer gameTimer;
    [SerializeField] SceneHandler sceneHandler;
    [SerializeField] PlayerInput playerInput;

    public enum GameState { MainMenu, Playing, Paused, LevelRestart, Death, LevelComplete, NextLevel }

    private GameState state;
    private GameState lastState;

    private void Awake() {
        state = GameState.Playing;
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
                sceneHandler.LoadLevel(0);
                break;
            case GameState.Playing:
                ShowPauseOverlay(false);
                break;
            case GameState.Paused:
                ShowPauseOverlay(true);
                break;
            case GameState.LevelRestart:
                ShowPauseOverlay(false);
                ShowDeathOverlay(false);
                sceneHandler.LoadLevel(SceneHandler.currentLevel);
                state = GameState.Playing;
                break;
            case GameState.Death:
                ShowPauseOverlay(false);
                ShowDeathOverlay(true);
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
                sceneHandler.LoadLevel(SceneHandler.currentLevel + 1);
                break;
        }
    }
    /// <summary>
    /// Set in menu mode. Switches control scheme, shows/hides cursor, and pauses the game timer.
    /// </summary>
    /// <param name="inMenuMode"></param>
    private void ToggleMenuMode(bool inMenuMode) {
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

    public void ButtonUnpause() { state = GameState.Playing; }
    public void ButtonQuit() { state = GameState.MainMenu; }
    public void ButtonRestart() { state = GameState.LevelRestart; }
    public void ButtonContinue() { state = GameState.NextLevel; }
    public void TriggerLevelComplete() { state = GameState.LevelComplete; }
}
