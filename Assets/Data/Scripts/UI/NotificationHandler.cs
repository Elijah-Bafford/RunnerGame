using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationHandler : MonoBehaviour {

    [Header("Notification Settings")]
    [Tooltip("Message to display")]
    [SerializeField] private string message;
    [Tooltip("How long to display the notification after leaving collider.")]
    [SerializeField] private float displayTime;

    [SerializeField] private GameObject notificationBox;
    private TextMeshProUGUI write;

    private void Awake() {
        write = notificationBox.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnTriggerEnter(Collider collision) {
        if (collision.CompareTag("Player")) {
            write.text = message;
            notificationBox.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider collision) {
        if (collision.CompareTag("Player")) {
            StartCoroutine(OnExit());
        }
    }

    private IEnumerator OnExit() {
        yield return new WaitForSeconds(displayTime); 
        notificationBox.SetActive(false);
        Destroy(gameObject);
    }
}
