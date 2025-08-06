using UnityEngine;

public class DeathPlane : MonoBehaviour {

    [Header("The height at which the player dies from falling off the map")]
    [SerializeField] private float yPosition;
    private Transform playerTransform;
    private Player player;
    private bool isDead = false;

    private void Start() {
        isDead = false;
        player = Player.player;
        playerTransform = player.GetTransform();
    }

    private void FixedUpdate() {
        if (isDead) return;
        if (playerTransform.position.y <= yPosition) {
            player.Die();
        }
    }
}
