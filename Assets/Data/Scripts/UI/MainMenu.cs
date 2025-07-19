using UnityEngine;

public class MainMenu : MonoBehaviour {

    [SerializeField] private SceneHandler sceneHandler;
    [SerializeField] private int level = 1;

    public void StartGame() { sceneHandler.LoadLevel(level); }
    public void QuitGame() { Application.Quit(); }

    public void QuitInEditor() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}