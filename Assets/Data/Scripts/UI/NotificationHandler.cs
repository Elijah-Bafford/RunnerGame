using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationHandler : MonoBehaviour {

    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject notificationBox;
    [SerializeField] private TextMeshProUGUI notificationMessage;

    private Coroutine notificationEndDelay;

    /// <summary> The value that timeScale is set to due to Notifications.</summary>
    public static float NotificationTimeScale { get; private set; } = 1f;

    public static bool DisableTutorials { get; set; } = false;

    public static NotificationHandler Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        notificationBox.SetActive(false);
        continueButton.SetActive(false);
    }

    public void ShowNotification(string message, float timeScale = 1f) {
        // A Notification is already shown
        if (notificationBox.activeSelf) StopNotification();

        notificationBox.SetActive(true);

        notificationMessage.text = message;

        if (timeScale == 0) continueButton.SetActive(true);

        NotificationTimeScale = timeScale;
        Time.timeScale = NotificationTimeScale;
    }

    private void StopNotification() {
        // Disable overlays
        if (notificationBox.activeSelf) notificationBox.SetActive(false);
        if (continueButton.activeSelf) continueButton.SetActive(false);
        // If a delayed end coroutine is running, stop it and set the var to null
        if (notificationEndDelay != null) {
            StopCoroutine(notificationEndDelay);
            notificationEndDelay = null;
        }
        // Reset time scale
        if (NotificationTimeScale != 1.0f) {
            NotificationTimeScale = 1.0f;
            Time.timeScale = NotificationTimeScale;
        }
    }

    public void StartNotificationEndDelay(float time = 0) {
        if (time == 0) {
            StopNotification();
            return;
        }

        if (notificationEndDelay != null) {
            Debug.LogWarning("NOTIFICATION END DELAY COROUTINE IS RUNNING!");
            StopNotification();
            return;
        }

        notificationEndDelay = StartCoroutine(NotificationEndDelay(time));
    }

    private IEnumerator NotificationEndDelay(float time) {
        yield return new WaitForSeconds(time);
        StopNotification();
    }

    public void ContinueButton() => StopNotification();
    
}
