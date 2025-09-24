using UnityEngine;

public class GroundCollider : MonoBehaviour {

    private Player player;

    private Transform LevelMain; // Not required, but keeps organized

    private void Start() { 
        player = Player.Instance;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("SlopedGround")) {
            player.SetOnSlope(true);
        }

        if (other.gameObject.CompareTag("GroundAuto")) {
            PlatformAuto auto = other.GetComponent<PlatformAuto>();
            if (auto == null) return;
            LevelMain = player.transform.parent;
            player.transform.SetParent(auto.transform, true);
            player.SetConveyorVelocity(Vector3.zero);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("SlopedGround")) {
            player.SetOnSlope(false);
        }

        if (other.gameObject.CompareTag("GroundAuto")) {
            PlatformAuto auto = other.GetComponent<PlatformAuto>();
            if (auto != null) player.transform.SetParent(LevelMain, true);
            player.SetConveyorVelocity(auto.CurrentVelocity);
        }
    }
}
