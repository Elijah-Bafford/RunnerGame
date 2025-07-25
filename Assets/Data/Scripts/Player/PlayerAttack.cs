using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Player player;

    private Enemy target = null;
    private bool hasAttacked = false;
    private bool canHit = false;
    private bool bypass = false;

    private void OnEnable() {
        hasAttacked = false;
        canHit = false;
    }

    private void FixedUpdate() {
        if (target == null) bypass = false;
        if (!bypass) {
            bypass = false;
            if (!canHit || !hasAttacked || target == null) return;
            if (target.IsDead()) return;
        }

        Vector3 hitDirection = transform.forward + Vector3.up * 0.5f;
        hitDirection.Normalize();
        player.ChangeSpeedStat(18f);
        target.Hit(hitDirection);

        target = null;
    }

    internal void ForceHit(Enemy target) {
        this.target = target;
        bypass = true;
    }

    public void HasAttacked(bool hasAttacked) { this.hasAttacked = hasAttacked; }

    private void OnTriggerEnter(Collider other) {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0) {
            target = other.GetComponentInParent<Enemy>();
            if (target != null) canHit = true;

        }
    }

    private void OnTriggerExit(Collider other) {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0) {
            target = null;
            canHit = false;
        }
    }
}
