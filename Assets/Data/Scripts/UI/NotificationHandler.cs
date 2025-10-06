using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationHandler : MonoBehaviour {

    [Header("Notification Settings")]
    [Tooltip("Message to display on entering the collider.")]
    [SerializeField] private string message;
    [Tooltip("How long to display the notification after leaving collider.")]
    [SerializeField] private float displayTime;
    [Tooltip("Slow time on entering the collider.")]
    [SerializeField] private bool slowTime;
    [Tooltip("This notification is a tutorial?")]
    [SerializeField] private bool isTutorial = true;
    [Tooltip("Parent UI object for the TMP")]
    [SerializeField] private GameObject notificationBox;

    public static bool disableTutorials = false;

    private Coroutine onExit = null;
    private TextMeshProUGUI messageTMP;

    private static GameObject lastNode;
    private bool playerHasEntered = false;
    private bool playerHasExited = false;

    /// <summary>
    /// The value that timeScale is set to due to Notifications.
    /// </summary>
    public static float timeSlowValue = 1f;

    private void Start() {
        messageTMP = notificationBox.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnTriggerEnter(Collider collision) {
        if (playerHasEntered) return;
        if (collision.CompareTag("Player")) {
            playerHasEntered = true;
            if (lastNode != null && !lastNode) Destroy(lastNode);

            if (disableTutorials && isTutorial) return;
            messageTMP.text = message;
            notificationBox.SetActive(true);

            if (slowTime) {
                Time.timeScale = 0.5f;
                timeSlowValue = Time.timeScale;
            }
        }
    }

    private void OnTriggerExit(Collider collision) {
        if (playerHasExited) return;
        if (collision.CompareTag("Player")) {
            playerHasExited = true;
            lastNode = gameObject;
            if (onExit != null) {
                StopCoroutine(onExit);
                onExit = null;
            }
            if (!(disableTutorials && isTutorial)) {
                if (slowTime) {
                    Time.timeScale = 1.0f;
                    timeSlowValue = Time.timeScale;
                }
            }
            onExit = StartCoroutine(OnExit());
        }
    }
    private IEnumerator OnExit() {
        yield return new WaitForSeconds(displayTime);
        notificationBox.SetActive(false);
        Destroy(gameObject);
    }
}
