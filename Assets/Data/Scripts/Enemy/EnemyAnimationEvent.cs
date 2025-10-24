using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour {

    private AttackDetection attackDetection;
    private EnemyBase enemy;

    private void Start() {
        attackDetection = GetComponentInChildren<AttackDetection>();
        enemy = GetComponentInParent<EnemyBase>();
    }

    public void TriggerAttackBox() => attackDetection.TriggerAttackBox();
    public void DisableEnemyIsHit() => enemy.Stun(false);
    public void TriggerDeath() => enemy.Kill();
    
}
