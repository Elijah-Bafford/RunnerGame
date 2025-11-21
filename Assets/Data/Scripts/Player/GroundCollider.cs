using UnityEngine;

public class GroundCollider : MonoBehaviour {

    private Player player;

    private Transform LevelMain; // Not required, but keeps organized

    private void Start() { 
        player = Player.Instance;
    }

    private void OnTriggerEnter(Collider other) {

        if(other.CompareTag("DeathPlane")) {
            player.SetDead(died: true, force: true);
            return;
        }

        if (other.CompareTag("SlopedGround")) player.isOnSlope = true;

        if (other.CompareTag("GroundAuto")) {
            PlatformAuto auto = other.GetComponent<PlatformAuto>();
            if (auto == null) return;
            LevelMain = player.transform.parent;
            player.transform.SetParent(auto.transform, worldPositionStays: true);
            player.SetConveyorVelocity(Vector3.zero);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("SlopedGround")) player.isOnSlope = false;

        if (other.gameObject.CompareTag("GroundAuto")) {
            PlatformAuto auto = other.GetComponent<PlatformAuto>();
            if (auto != null) player.transform.SetParent(LevelMain, true);
            player.SetConveyorVelocity(auto.CurrentVelocity);
        }
    }
}
