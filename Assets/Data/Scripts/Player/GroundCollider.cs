using UnityEngine;

public class GroundCollider : MonoBehaviour {

    private Player player;

    private Transform LevelMain; // Not required, but keeps organized

    private void Start() { player = GetComponentInParent<Player>(); }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("SlopedGround")) {
            player.SetOnSlope(true);
        }

        if (other.gameObject.CompareTag("GroundAuto")) {
            PlatformAuto auto = other.GetComponent<PlatformAuto>();
            if (auto == null) return;
            LevelMain = player.GetTransform().parent;
            player.GetTransform().SetParent(auto.transform, true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("SlopedGround")) {
            player.SetOnSlope(false);
        }

        if (other.gameObject.CompareTag("GroundAuto")) {
            PlatformAuto auto = other.GetComponent<PlatformAuto>();
            if (auto != null) player.GetTransform().SetParent(LevelMain, true);
        }
    }
}
