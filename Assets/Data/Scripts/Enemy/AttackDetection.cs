using UnityEngine;

public class AttackDetection : MonoBehaviour {

    private CapsuleCollider attackBox;

    private void Start() {
        attackBox = GetComponent<CapsuleCollider>();
    }

    public void TriggerAttackBox() {
        attackBox.enabled = !attackBox.enabled;
    }

    private void OnTriggerStay(Collider other) {
        if (!other.CompareTag("Player")) return;
        Player.Instance.Die();
    }
}
