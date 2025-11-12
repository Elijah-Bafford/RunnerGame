using System;
using UnityEngine;
using UnityEngine.Animations;

public class EnemyAnimationEvent : MonoBehaviour {

    [Header("Enemy Archer References")]
    [SerializeField] private RotationConstraint spineConstraint;

    private AttackDetection attackDetection;
    private Enemy enemy;
    private EnemyArcher enemyArcher;

    private void Start() {
        attackDetection = GetComponentInChildren<AttackDetection>();
        enemy = GetComponentInParent<Enemy>();
        if (spineConstraint != null) spineConstraint.enabled = false;
        if (enemy is EnemyArcher ea) enemyArcher = ea;
    }

    public void EnableAttackBox() => attackDetection.SetAttackBoxEnabled(true);
    public void DisableAttackBox() => attackDetection.SetAttackBoxEnabled(false);
    public void DisableEnemyIsHit() => enemy.Stun(false);
    public void DestroyEnemy() => enemy.DestroyEnemy(); 

    #region Enemy Archer
    public void DisableSpineConstraint() => spineConstraint.enabled = false;
    public void EnableSpineConstraint() => spineConstraint.enabled = true;
    public void DisableConstrainedArrow() => enemyArcher.SetConstrainedArrow(false);
    public void EnableConstrainedArrow() => enemyArcher.SetConstrainedArrow(true);

    // Human Animator

    public void DisableBowDraw() { if (enemyArcher != null) enemyArcher.DisableBowDrawn(); }

    // Bow animator
    public void DisableDrawstring() { if (enemyArcher != null) enemyArcher.SetBowDrawstring(false); }
    public void EnableDrawstring() { if (enemyArcher != null) enemyArcher.SetBowDrawstring(true); }
    public void FireBowString() => enemyArcher.BowStringFired();
    public void CreateLooseArrow() => enemyArcher.CreateLoosedArrow();
    #endregion
}
