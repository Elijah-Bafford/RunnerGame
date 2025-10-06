using UnityEngine;

public class AttackAnimationEvents : MonoBehaviour {
    [SerializeField] private EnemyOld enemy;

    private void Start() {
        enemy = GetComponentInParent<EnemyOld>();
    }

    public void WeaponCanHit() { enemy.WeaponCanHit(true); }
    public void WeaponCantHit() { enemy.WeaponCanHit(false); }
}