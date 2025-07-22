using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Player player;
    private CapsuleCollider weaponCollider;
    
    private void Awake() {
        weaponCollider = GetComponent<CapsuleCollider>();
    }

    public void EnableWeapon() {
        weaponCollider.enabled = true;
    }

    public void DisableWeapon() {
        weaponCollider.enabled = false;
    }
    
    private void OnTriggerEnter(Collider collision) {
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0) {
            IDamageable target = collision.GetComponent<IDamageable>();
            if (target != null) {
                Vector3 hitDirection = transform.forward + Vector3.up * 0.5f;
                hitDirection.Normalize();
                player.ChangeSpeedStat(15f);
                target.Hit(hitDirection);
                DisableWeapon();
            }
        }
    }
}
