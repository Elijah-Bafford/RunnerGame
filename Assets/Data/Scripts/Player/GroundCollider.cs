using UnityEngine;

public class GroundCollider : MonoBehaviour {

    private Player player;

    private void Start() { player = GetComponentInParent<Player>(); }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("SlopedGround")) {
            player.SetOnSlope(true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("SlopedGround")) {
            player.SetOnSlope(false);
        }
    }
}
