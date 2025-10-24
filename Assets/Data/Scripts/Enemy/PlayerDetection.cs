using UnityEngine;

public class PlayerDetection : MonoBehaviour {

    private EnemyBase _enemyKnight;
    private Player _player;
    private bool _allowDetectionRotation = false;

    private void Awake() {
        _player = Player.Instance;
        _enemyKnight = GetComponentInParent<EnemyBase>();
    }

    private void FixedUpdate() {
        if (!_allowDetectionRotation) return;
        Vector3 playerDirection = _player.transform.position - transform.position;
        Quaternion targetRot = Quaternion.LookRotation(playerDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 180f * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        _allowDetectionRotation = true;
        _enemyKnight.SetPlayerInViewDistance(true);
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;
        _allowDetectionRotation = false;
        _enemyKnight.SetPlayerInViewDistance(false);
    }
}
