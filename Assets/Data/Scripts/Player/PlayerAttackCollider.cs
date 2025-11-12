using UnityEngine;

public class PlayerAttackCollider : MonoBehaviour {

    private Player player;
    private Enemy target = null;
    private CapsuleCollider attackBox;

    public void InitPlayerAttack(Player player) {
        this.player = player;
        attackBox = GetComponent<CapsuleCollider>();
        SetAttackBoxEnabled(false);
    }

    public void SetAttackBoxEnabled(bool enabled) {
        attackBox.enabled = enabled;
    }

    private void OnTriggerEnter(Collider other) {
        print("Player Hit: " + other.name);

        if (other.CompareTag("Wall")) {
            AudioHandler.Instance.PlaySound(SoundType.SwordImpactWall);
        }

        if (!other.CompareTag("Enemy")) return;

        target = other.GetComponent<Enemy>();
        if (target == null) {
            target = other.GetComponentInParent<Enemy>();
            if (target == null) return;
        }

        if (target.IsDead()) return;

        player.ChangeSpeedStat(18f);
        target.Hurt(player.AttackDamage);

    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Enemy")) {
            target = null;
        }
    }
}
