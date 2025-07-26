using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class NotificationHandler : MonoBehaviour {

    [Header("Notification Settings")]
    [Tooltip("Message to display on entering the collider.")]
    [SerializeField] private string message;
    [Tooltip("How long to display the notification after leaving collider.")]
    [SerializeField] private float displayTime;
    [Tooltip("Slow time on entering the collider.")]
    [SerializeField] private bool slowTime;
    [Tooltip("Parent UI object for the TMP")]
    [SerializeField] private GameObject notificationBox;

    private Coroutine onExit = null;
    private TextMeshProUGUI messageTMP;

    private static GameObject lastNode;

    private void Start() {
        messageTMP = notificationBox.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnTriggerEnter(Collider collision) {
        if (collision.CompareTag("Player")) {
            if (lastNode != null) {
                if (!lastNode.IsDestroyed()) Destroy(lastNode);
            }
            messageTMP.text = message;
            notificationBox.SetActive(true);

            if (slowTime) Time.timeScale = 0.5f;
        }
    }

    private void OnTriggerExit(Collider collision) {
        if (collision.CompareTag("Player")) {
            lastNode = gameObject;
            if (onExit != null) {
                StopCoroutine(onExit);
                onExit = null;
            }
            if (slowTime) Time.timeScale = 1.0f;
            onExit = StartCoroutine(OnExit());
        }
    }
    private IEnumerator OnExit() {
        yield return new WaitForSeconds(displayTime);
        notificationBox.SetActive(false);
        Destroy(gameObject);
    }
}
