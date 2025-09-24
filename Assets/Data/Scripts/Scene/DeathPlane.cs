using UnityEngine;

public class DeathPlane : MonoBehaviour {

    private Transform playerTransform;
    private Player player;
    private float deathPlaneY;
    private bool isDead = false;

    private void Start() {
        isDead = false;
        deathPlaneY = transform.position.y;
        player = Player.Instance;
        playerTransform = player.transform;
    }

    private void FixedUpdate() {
        if (isDead || Time.timeScale == 0f) return;
        if (playerTransform.position.y <= deathPlaneY) {
            player.Die();
        }
    }
}
