using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    private Player player;
    private Enemy target = null;
    private bool hasAttacked = false;
    private bool canHit = false;

    private CapsuleCollider weaponCollider;

    private void OnEnable() {
        hasAttacked = false;
        canHit = false;
    }

    public void InitPlayerAttack(Player player) {
        this.player = player;
        weaponCollider = GetComponent<CapsuleCollider>();
    }

    public void UpdatePlayerAttack() {
        if (!canHit || !hasAttacked || target == null) return;
        if (target.IsDead()) return;
        Attack();
    }

    private void Attack() {
        player.ChangeSpeedStat(18f);
        target.Hurt(player.attackDamage);
        target = null;
    }

    public void ForceHit(Enemy target) {
        this.target = target;
        Attack();
    }

    public void ToggleAttackCollider(bool toggle) {
        weaponCollider.enabled = toggle;
    }

    public void HasAttacked(bool hasAttacked) { this.hasAttacked = hasAttacked; }

    private void OnTriggerEnter(Collider other) {
        print(other.name);
        if (other.CompareTag("Enemy")) {
            target = other.GetComponent<Enemy>();
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
