using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-5)]
public class SceneHandler : MonoBehaviour {

    public static SceneHandler Instance { get; private set; }

    [Header("Set the number of scenes")]
    [SerializeField] private int _numberOfScenes;
    public static int currentLevel;
    public static int numLevels;

    [Header("Loading UI")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressBar;

    public static event Action<int> OnLevelLoad;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        numLevels = _numberOfScenes;
        currentLevel = SceneManager.GetActiveScene().buildIndex;
        print("Current Level: " + currentLevel);
        
    }
    private void Start() {
        BootstrapProcess.ProcessFinished(gameObject);
    }

    /// <summary>
    /// Load a level.
    /// </summary>
    /// <param name="level"></param>
    public void LoadLevel(int level) {
        if (level >= numLevels) {
            Debug.LogWarning("Level index out of bounds.");
            return;
        }
        currentLevel = level;
        Time.timeScale = 1.0f;
        StartCoroutine(LoadSceneAsync(level));
    }

    private IEnumerator LoadSceneAsync(int levelIndex) {
        loadingScreen.SetActive(true);
        progressBar.value = 0f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(levelIndex);
        operation.allowSceneActivation = false; // Hold off activation until we're ready

        // While loading, update progress
        while (operation.progress < 0.9f) {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;
            yield return null;
        }

        progressBar.value = 1f;

        // Now allow the scene to activate
        operation.allowSceneActivation = true;

        // Wait for the scene activation to complete
        while (!operation.isDone) {
            yield return null;
        }

        OnLevelLoad?.Invoke(levelIndex);

        print("Current Level: " + currentLevel);
        loadingScreen?.SetActive(false);
    }

}