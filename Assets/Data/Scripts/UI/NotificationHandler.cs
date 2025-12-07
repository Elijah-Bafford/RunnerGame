using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationHandler : MonoBehaviour {

    [SerializeField] private GameObject notificationBox;
    [SerializeField] private TextMeshProUGUI notificationMessage;

    private Coroutine notificationEndDelay;

    public static bool DisableTutorials { get; set; } = false;

    public static NotificationHandler Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        notificationBox.SetActive(false);
    }

    public void ShowNotification(string message, float timeScale = 1f) {
        print("Show Notification: " + message);
        // A Notification is already shown
        if (notificationBox.activeSelf) StopNotification();

        notificationBox.SetActive(true);

        notificationMessage.text = message;

        if (timeScale == 0) GameStateHandler.Instance.SetGameState(GameStateHandler.GameState.Notification);
        if (timeScale > 0) Time.timeScale = timeScale;
    }

    private void StopNotification() {
        // Disable overlays
        if (notificationBox.activeSelf) notificationBox.SetActive(false);
        // If a delayed end coroutine is running, stop it and set the var to null
        if (notificationEndDelay != null) {
            StopCoroutine(notificationEndDelay);
            notificationEndDelay = null;
        }
        // Reset time scale
        Time.timeScale = 1.0f;
    }

    public void StartNotificationEndDelay(float time = 0) {
        Debug.Log("Start Notification End Delay: param: " + time);
        if (time == 0) {
            StopNotification();
            return;
        }
        
        if (notificationEndDelay != null) {
            Debug.LogWarning("NOTIFICATION END DELAY COROUTINE IS RUNNING!");
            StopNotification();
            return;
        }
        Debug.Log("Starting coroutine: NotificationEndDelay for time: " + time + " seconds.");
        notificationEndDelay = StartCoroutine(NotificationEndDelay(time));
    }

    private IEnumerator NotificationEndDelay(float time) {
        yield return new WaitForSeconds(time);
        StopNotification();
    }

    public void E_StopNotification() => StopNotification();
}
