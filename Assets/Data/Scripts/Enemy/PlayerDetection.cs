using UnityEngine;

public class PlayerDetection : MonoBehaviour {

    private Enemy _enemy;
    private Player _player;
    private bool _allowDetectionRotation = false;
    private Quaternion originalRot;

    private void Awake() {
        _player = Player.Instance;
        _enemy = GetComponentInParent<Enemy>();
        GameStateHandler.OnLevelRestart += ResetDetection;
        originalRot = transform.rotation;
    }

    private void FixedUpdate() {
        if (!_allowDetectionRotation) {
            transform.rotation = originalRot;
            return;
        }
        Vector3 playerDirection = _player.transform.position - transform.position;
        playerDirection.y = 0f;
        Quaternion targetRot = Quaternion.LookRotation(playerDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 180f * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        _allowDetectionRotation = true;
        _enemy.SetPlayerInViewDistance(true);
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;
        ResetDetection();
        _enemy.SetPlayerInViewDistance(false);
    }

    protected virtual void OnDestroy() => GameStateHandler.OnLevelRestart -= ResetDetection;

    private void ResetDetection() {
        _allowDetectionRotation = false;
    }
}
