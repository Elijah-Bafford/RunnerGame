using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-20)]
public class BootstrapProcess : MonoBehaviour {

    [SerializeField] private int processes = 0;

    private static int currentProcess = 0;

    private bool allowCheck = true;

    private void Update() {
        if (!allowCheck) return;
        if (currentProcess >= processes) {
            allowCheck = false;
            Debug.Log("All bootstrap processes finished. Loading Main Menu.");
            SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        }
    }

    public static void ProcessFinished(GameObject obj = default) {
        currentProcess++;
        Debug.Log("Process ID: " + currentProcess + " Loaded.  |  Object: " + obj.name, obj);
    }
}
