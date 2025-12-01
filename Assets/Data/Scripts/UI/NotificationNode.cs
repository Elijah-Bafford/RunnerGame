using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class NotificationNode : MonoBehaviour {

    [SerializeField] private string message;
    [SerializeField] private bool isTutorial = false;
    [SerializeField, Range(0, 1)] private float timeScale = 1.0f;
    [SerializeField] private float endDelay = 0;

    private byte triggerState = 0;

    //private void Start() => GameStateHandler.OnLevelRestart += OnLevelRestart;
    //private void OnDestroy() => GameStateHandler.OnLevelRestart -= OnLevelRestart;

    //private void OnLevelRestart() {
    //    triggerState = 0;
    //    gameObject.SetActive(true);
    //}

    private void OnEnable() {
        if (isTutorial && NotificationHandler.DisableTutorials) {
            Debug.Log("Notfication Disabled: " + gameObject.name);
            gameObject.SetActive(false);
            return;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (CheckTriggerState(0, other))
            NotificationHandler.Instance.ShowNotification(message, timeScale);
    }

    private void OnTriggerExit(Collider other) {
        if (CheckTriggerState(1, other)) {
            NotificationHandler.Instance.StartNotificationEndDelay(endDelay);
            gameObject.SetActive(false);
        }
    }

    private bool CheckTriggerState(byte state, Collider other) {
        if (triggerState > state) return false;
        if (!other.CompareTag("Player")) return false;
        triggerState++;
        return true;
    }
}
