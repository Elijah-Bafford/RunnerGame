using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour {

    [SerializeField] TextMeshProUGUI UITimer;

    private float currentTime;
    private bool isRunning = true;

    private void Update() {
        if (isRunning) {
            currentTime += Time.deltaTime;

            UITimer.text = GetTimeAsString();
        }
    }

    /// <summary>
    /// Returns the time in "minutes", "seconds", or "milliseconds"
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public int GetTimeAs(string unit) {
        return unit switch {
            "minutes" => Mathf.FloorToInt(currentTime / 60),
            "seconds" => Mathf.FloorToInt(currentTime % 60),
            "milliseconds" => Mathf.FloorToInt((currentTime * 1000f) % 1000f / 10f),
            _ => throw new System.Exception("GetTimeAs in GameTimer called with invalid unit")
        };
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

    public float GetTime() {
        return currentTime;
    }

    public string GetTimeAsString() {
        int minutes = GetTimeAs("minutes");
        int seconds = GetTimeAs("seconds");
        int milliseconds = GetTimeAs("milliseconds");

        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }
}
