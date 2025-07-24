using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Player player;

    private Enemy target = null;
    private bool hasAttacked = false;
    private bool canHit = false;

    private void OnEnable() {
        hasAttacked = false;
        canHit = false;
    }

    private void FixedUpdate() {
        bool isN = target == null;
        print("Can hit: " + canHit.ToString());
        print("hasAttacked: " + hasAttacked.ToString());
        print("target is null: " + isN.ToString());
        if (!canHit || !hasAttacked || target == null) return;
        bool isNN = target.IsDead();
        Debug.Log("Target is dead: " + isNN.ToString());
        if (target.IsDead()) return;

        Vector3 hitDirection = transform.forward + Vector3.up * 0.5f;
        hitDirection.Normalize();
        player.ChangeSpeedStat(18f);
        target.Hit(hitDirection);

        target = null;
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
