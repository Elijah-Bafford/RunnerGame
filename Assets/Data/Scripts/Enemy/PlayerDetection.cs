using UnityEngine;

public class PlayerDetection : MonoBehaviour {

    //private EnemyBase _enemyBase; TODO: Reorganize enemy inheritance 
    private EnemyKnight enemyKnight;

    private void Awake() {
        //_enemyBase = GetComponentInParent<EnemyBase>();
        enemyKnight = GetComponentInParent<EnemyKnight>();
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        print("CompareTag(\"Player\") == true | Name: " + other.name);
        enemyKnight.SetPlayerInViewDistance(true);
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;
        enemyKnight.SetPlayerInViewDistance(false);
        print("OUT OF VIEW DISTANCE");
    }
}
