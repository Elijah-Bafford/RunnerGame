using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class SceneHandler : MonoBehaviour {
    
    public static int currentLevel;
    public static int numLevels;

    [Header("Loading UI")]
    [Tooltip("Set this index value of the scene this Handler is attached to")]
    [SerializeField] private int thisLevel;

    [Tooltip("Disable an element when loading starts. Not required.")]
    [SerializeField] private GameObject ObjToDisable;

    public static event Action<int> OnLevelLoad;


    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressBar;

    private void Awake() {
        currentLevel = thisLevel;
        numLevels = SceneManager.sceneCount + 1;
    }

    /// <summary>
    /// Normally load a level.
    /// </summary>
    /// <param name="level"></param>
    public void LoadLevel(int level) {
        currentLevel = level;
        StartCoroutine(LoadSceneAsync(level));
        print("OnLoad");
        OnLevelLoad?.Invoke(level);
    }

    /*
    /// <summary>
    /// Load a level with no loading screen.
    /// </summary>
    /// <param name="level"></param>
    public void InstantLoad(int level) {
        currentLevel = level;
        SceneManager.LoadScene(currentLevel);
    }
    */

    private IEnumerator LoadSceneAsync(int levelIndex) {
        if (ObjToDisable) ObjToDisable.SetActive(false);
        loadingScreen.SetActive(true);
        progressBar.value = 0f;

        yield return new WaitForSeconds(0.5f);      // Showing the loading sreen for debugging.
        AsyncOperation operation = SceneManager.LoadSceneAsync(levelIndex);

        while (operation.progress < 0.9f) {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            progressBar.value = progress;
            yield return null;
        }

        progressBar.value = 1f;

        operation.allowSceneActivation = true;
        yield return null;
    }
}