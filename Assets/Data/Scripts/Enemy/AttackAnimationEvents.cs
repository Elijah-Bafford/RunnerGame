using UnityEngine;

public class AttackAnimationEvents : MonoBehaviour {
    [SerializeField] private Enemy enemy;

    private void Start() {
        enemy = GetComponentInParent<Enemy>();
    }

    public void WeaponCanHit() { enemy.WeaponCanHit(true); }
    public void WeaponCantHit() { enemy.WeaponCanHit(false); }
}