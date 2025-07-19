using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIInput : MonoBehaviour {

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
                Time.timeScale = 1;
                SceneManager.LoadScene("MainMenu");
                break;
            case GameState.Playing:
                Time.timeScale = 1;
                break;
            case GameState.Paused:
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
