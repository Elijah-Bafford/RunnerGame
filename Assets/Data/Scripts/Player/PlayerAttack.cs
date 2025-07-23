using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Player player;
    

    public void EnableWeapon() {
        gameObject.SetActive(true);
        print("weapon enabled");
    }

    public void DisableWeapon() {
        gameObject.SetActive(false);
        print("weapon disabled");
    }
    
    private void OnTriggerEnter(Collider collision) {
        print(collision.name);
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0) {
            IDamageable target = collision.GetComponent<IDamageable>();
            if (target != null) {
                Vector3 hitDirection = transform.forward + Vector3.up * 0.5f;
                hitDirection.Normalize();
                player.ChangeSpeedStat(15f);
                target.Hit(hitDirection);
            }
        }
    }
}
