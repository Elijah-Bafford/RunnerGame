using UnityEngine;

public class GroundCollider : MonoBehaviour {

    private Player player;

    private void Start() { player = GetComponentInParent<Player>(); }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("SlopedGround")) {
            player.SetOnSlope(true);
        }
        if (other.gameObject.CompareTag("GroundAuto")) {
            PlatformAuto auto = other.GetComponent<PlatformAuto>();
            if (auto == null) return;
            player.SetConveyorPlatform(auto);
            player.SetOnConveyor(true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("SlopedGround")) {
            player.SetOnSlope(false);
        }
        if (other.gameObject.CompareTag("GroundAuto")) {
            PlatformAuto auto = other.GetComponent<PlatformAuto>();
            if (auto == null) return;
            player.SetConveyorPlatform(null);
            player.SetOnConveyor(false);
        }
    }
}
