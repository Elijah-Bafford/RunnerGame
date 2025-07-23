using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour {

    [SerializeField] TextMeshProUGUI UITimer;

    private float currentTime;
    private bool isRunning = true;

    private void Update() {
        if (isRunning) {
            currentTime += Time.deltaTime;

            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            int milliseconds = Mathf.FloorToInt((currentTime * 1000f) % 1000f / 10f);

            UITimer.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
        }
    }

    public void ResetTimer() {
        isRunning = false;
        currentTime = 0f;
        UITimer.text = "00:00:00";
    }

    /// <summary>
    /// Run the timer or stop the timer.
    /// </summary>
    /// <param name="isRunning"></param>
    public void RunTimer(bool isRunning) {
        this.isRunning = isRunning;
    }

}
