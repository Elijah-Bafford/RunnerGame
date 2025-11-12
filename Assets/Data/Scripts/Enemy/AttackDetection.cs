using UnityEngine;

public class AttackDetection : MonoBehaviour {

    [SerializeField] private CapsuleCollider attackBox;
    [SerializeField] private Enemy enemy;

    public void SetAttackBoxEnabled(bool enabled) => attackBox.enabled = enabled;

    private void OnTriggerStay(Collider other) {
        if (enemy.IsStunned()) return;
        if (other.name == "AttackBox") return;
        if (!other.CompareTag("Player")) return;
        Player.Instance.SetDead();
    }
}
