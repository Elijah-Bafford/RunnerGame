using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour {

    [SerializeField] TextMeshProUGUI UITimer;

    private float currentTime;
    private bool isRunning = true;

    private void Update() {
        if (isRunning) {
            currentTime += Time.deltaTime;

            UITimer.text = GetTimeAsString(true);
        }
    }

    /// <summary>
    /// Returns the time in "minutes", "seconds", or "milliseconds" of the float time.
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public int GetTimeAs(string unit, float time) {
        return unit switch {
            "minutes" => Mathf.FloorToInt(time / 60),
            "seconds" => Mathf.FloorToInt(time % 60),
            "milliseconds" => Mathf.FloorToInt((time * 1000f) % 1000f / 10f),
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

    public float GetCurrentTime() {
        return currentTime;
    }

    /// <summary>
    /// Return a time value as a string. If isCurrentTime is true return current time, otherwise return the value of argument time.
    /// </summary>
    /// <param name="isCurrentTime"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public string GetTimeAsString(bool isCurrentTime, float time = 0) {
        float time_v = isCurrentTime ? currentTime : time;
        int minutes = GetTimeAs("minutes", time_v);
        int seconds = GetTimeAs("seconds", time_v);
        int milliseconds = GetTimeAs("milliseconds", time_v);

        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }
}
