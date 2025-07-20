using UnityEngine;
using UnityEngine.InputSystem;

public class UIInput : MonoBehaviour {

    [SerializeField] GameObject pauseOverlay;
    [SerializeField] GameTimer gameTimer;
    [SerializeField] SceneHandler sceneHandler;
    [SerializeField] PlayerInput playerInput;

    public enum GameState { MainMenu, Playing, Paused, LevelRestart }

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
                sceneHandler.InstantLoad(SceneHandler.currentLevel);
                state = GameState.Playing;
                break;
        }
    }

    private void ShowPauseOverlay(bool paused) {
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        pauseOverlay.SetActive(paused);
        gameTimer.RunTimer(!paused);

        string input = paused ? "UI" : "Player";
        int newTime = paused ? 0 : 1;

        playerInput.SwitchCurrentActionMap(input);
        Time.timeScale = newTime;
    }

    public GameState GetGameState() { return state; }

    public void OnPauseKeyPressed(InputAction.CallbackContext context) {
        if (context.performed) {
            if (state == GameState.Playing) state = GameState.Paused;
            else state = GameState.Playing;
        }
    }

    public void ButtonUnpause() { state = GameState.Playing; }
    public void ButtonQuit() { state = GameState.MainMenu; }
    public void ButtonRestart() { state = GameState.LevelRestart; }
}
