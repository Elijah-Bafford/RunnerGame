using UnityEngine;
using UnityEngine.Animations;

public class EnemyAnimationEvent : MonoBehaviour {

    [Header("Enemy Archer References")]
    [SerializeField] private RotationConstraint spineConstraint;

    private AttackDetection attackDetection;
    private EnemyBase enemy;
    private EnemyArcher enemyArcher;

    private void Start() {
        attackDetection = GetComponentInChildren<AttackDetection>();
        enemy = GetComponentInParent<EnemyBase>();
        if (spineConstraint != null) spineConstraint.enabled = false;
        if (enemy is EnemyArcher ea) enemyArcher = ea;
    }

    public void TriggerAttackBox() => attackDetection.TriggerAttackBox();
    public void DisableEnemyIsHit() => enemy.Stun(false);
    public void TriggerDeath() => enemy.Kill();

    // Enemy Archer
    public void ToggleSpineConstraint() => spineConstraint.enabled = !spineConstraint.enabled;
    public void DrawBow() => enemyArcher.TriggerBowDrawstring();
    public void CancelDraw() => enemyArcher.CancelDraw();
    public void FireBow() => enemyArcher.FireBow();
    public void ToggleConstrainedArrow() => enemyArcher.ToggleConstrainedArrow();
    public void CreateLooseArrow() => enemyArcher.CreateLoosedArrow();
}
