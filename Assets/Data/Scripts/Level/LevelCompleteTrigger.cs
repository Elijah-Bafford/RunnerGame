using UnityEngine;

public class LevelCompleteTrigger : MonoBehaviour {

    [SerializeField] GameStateHandler gameStateHandler;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            gameStateHandler.TriggerLevelComplete();
        }
    }

}
