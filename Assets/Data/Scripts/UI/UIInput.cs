using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIInput : MonoBehaviour {

    [SerializeField] GameObject pauseOverlay;
    [SerializeField] GameTimer gameTimer;

    public enum GameState { MainMenu, Playing, Paused }

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
                gameTimer.ResetTimer();
                // Does nothing yet
                Time.timeScale = 1;
                break;
            case GameState.Playing:
                gameTimer.StartTimer();
                pauseOverlay.SetActive(false);
                Time.timeScale = 1;
                break;
            case GameState.Paused:
                gameTimer.StopTimer();
                pauseOverlay.SetActive(true);
                Time.timeScale = 0;
                break;
        }
    }

    public GameState GetGameState() { return state; }

    public void OnPauseKeyPressed(InputAction.CallbackContext context) {
        if (context.performed) {
            if (state == GameState.Playing) state = GameState.Paused;
            else state = GameState.Playing;
        }
    }
}
