using UnityEngine;

public class GroundCollider : MonoBehaviour {

    private Player player;

    private Transform LevelMain; // Not required, but keeps organized

    private void Start() {
        player = Player.Instance;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("SlopedGround")) player.onSlopeLowY = player.transform.position.y;

        if (other.CompareTag("DeathPlane")) {
            player.SetDead(died: true, force: true);
            return;
        }

        if (other.CompareTag("GroundAuto")) {
            PlatformAuto auto = other.GetComponent<PlatformAuto>();
            if (auto == null) return;
            LevelMain = player.transform.parent;
            player.transform.SetParent(auto.transform, worldPositionStays: true);
            player.SetConveyorVelocity(Vector3.zero);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("SlopedGround")) player.onSlopeAngle = 0f;

        if (other.gameObject.CompareTag("GroundAuto")) {
            PlatformAuto auto = other.GetComponent<PlatformAuto>();
            if (auto != null) player.transform.SetParent(LevelMain, true);
            player.SetConveyorVelocity(auto.CurrentVelocity);
        }
    }

    private void OnTriggerStay(Collider other) {
        CheckSlope(other.gameObject);
    }

    private void CheckSlope(GameObject colliderGameObject) {
        if (!colliderGameObject.CompareTag("SlopedGround")) return;
        // If the player is lower (y) than in the last execution then set the angle to a positive value
        float angleToCalc = Mathf.Abs(colliderGameObject.transform.eulerAngles.z);
        angleToCalc *= (player.transform.position.y < player.onSlopeLowY) ? 1 : -1;
        player.onSlopeAngle = angleToCalc;

        player.onSlopeLowY = player.transform.position.y;

    }
}
