using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneHandler : MonoBehaviour {
    
    public static int currentLevel;

    [Header("Loading UI")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressBar;

    public void LoadLevel(int level) { 
        currentLevel = level;
        StartCoroutine(LoadSceneAsync(level));
    }

    private IEnumerator LoadSceneAsync(int levelIndex) {
        loadingScreen?.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(levelIndex);

        while (operation.progress < 0.9f) {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;
        }

        yield return null;
    }
}