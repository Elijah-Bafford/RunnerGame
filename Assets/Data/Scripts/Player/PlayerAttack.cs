using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    private Player player;
    private Enemy target = null;
    private bool hasAttacked = false;
    private bool canHit = false;

    private void OnEnable() {
        hasAttacked = false;
        canHit = false;
    }

    private void Start() {
        player = GetComponentInParent<Player>();
    }

    private void FixedUpdate() {
        if (!canHit || !hasAttacked || target == null) return;
        if (target.IsDead()) return;

        hit();
    }

    private void hit() {
        Vector3 hitDirection = transform.forward + Vector3.up * 0.5f;
        hitDirection.Normalize();
        player.ChangeSpeedStat(18f);
        print("Target: " + target.name);
        target.Hit(hitDirection);

        target = null;
    }

    internal void ForceHit(Enemy target) {
        this.target = target;
        hit();
    }

    internal void ToggleAttackCollider(bool toggle) {
        gameObject.GetComponent<CapsuleCollider>().enabled = toggle;
    }

    public void HasAttacked(bool hasAttacked) { this.hasAttacked = hasAttacked; }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy")) {
            target = other.GetComponentInParent<Enemy>();
            if (target != null) canHit = true;
        }
        if (other.CompareTag("Wall")) {
            AudioHandler.Instance.PlaySound(SoundType.SwordImpactWall);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Enemy")) {
            target = null;
            canHit = false;
        }
    }
}
