using UnityEngine;

public class AllowHitEvent : MonoBehaviour {
    [SerializeField] private Enemy enemy;

    public void AllowHit() { enemy.AllowHit(true); }
    public void DisallowHit() { enemy.AllowHit(false); }

    public void InAttack() { enemy.SetIsInAttackAnimation(true); }
    public void OutAttack() { enemy.SetIsInAttackAnimation(false); }
}