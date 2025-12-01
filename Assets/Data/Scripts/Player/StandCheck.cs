using UnityEngine;

public class StandCheck : MonoBehaviour {


    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Tunnel")) return;
        Player.Instance.CanStand = false;
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Tunnel")) return;
        Player.Instance.CanStand = true;
    }
}
