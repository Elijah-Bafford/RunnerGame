using UnityEngine;

public class DeathPlane : MonoBehaviour {

    [Header("The Y position of this object is the lowest the player can go\nPlayer Object Ref:")]
    [SerializeField] private GameObject player;

    private Player playerScript;
    private bool isDead = false;

    private void Start() {
        isDead = false;
        playerScript = player.GetComponent<Player>();
    }

    private void FixedUpdate() {
        if (isDead) return;
        if (player.transform.position.y < gameObject.transform.position.y) {
            playerScript.Die();
        }
    }
}
