using UnityEngine;

public class DeathPlane : MonoBehaviour {

    [Header("The Y position of this object determines how far the player\ncan fall before dying")]
    [SerializeField] private float thisVariableIsReadOnly;
    private Transform playerTransform;
    private Player player;
    private bool isDead = false;

    private void Start() {
        isDead = false;
        thisVariableIsReadOnly = transform.position.y;
        player = Player.player;
        playerTransform = player.GetTransform();
    }

    private void FixedUpdate() {
        if (isDead) return;
        if (playerTransform.position.y <= thisVariableIsReadOnly) {
            player.Die();
        }
    }
}
