using UnityEngine;

/// <summary>
/// EnemyBase Helper, instantiate with EnemyBase.
/// </summary>
public class EnemyAnimator {
    // Enemy component Refs
    private Rigidbody rb;
    private Animator anim;
    private Animator bowAnim;
    private EnemyBase enemy;
    private float movementSpeed;

    // Run blend
    private float velocity = 0f;
    private float lastVelocity = 0f;

    // Turn blend
    private bool rotationDeadzoneLock = false;
    private float lockAngle = 0f;
    private float turnBlendSpeed = 5f;
    private float turnParam = 0f;

    // Enemy Specific traits
    private bool isArcher = false;

    public EnemyAnimator(EnemyBase enemyBase) {
        enemy = enemyBase;
        anim = enemyBase.GetAnimator();
        if (enemyBase is EnemyArcher ea) bowAnim = ea.GetBowAnimator();
        isArcher = bowAnim != null;
        rb = enemyBase.GetRigidbody();
        movementSpeed = enemyBase.GetMovementSpeed();
    }

    public void UpdateAnimations() => UpdateRunAnimation();
    public void TriggerHit() => anim.SetTrigger("Hit");
    public void TriggerMeleeAttack() { if (!isArcher) anim.SetTrigger("Attack"); }
    public void TriggerRangeAttack() { if (isArcher) bowAnim.SetTrigger("Draw"); }

    private void UpdateRunAnimation() {
        anim.SetFloat("Health", enemy.GetHealth());
        velocity = Mathf.Clamp01(rb.linearVelocity.magnitude / movementSpeed);
        velocity = Mathf.Round(velocity * 100f) / 100f;

        if ((Mathf.Abs(velocity - lastVelocity) >= 0.01f) || velocity < 0.01f) {
            if (velocity < 0.01f) velocity = 0f;
            lastVelocity = velocity;
            anim.SetFloat("Velocity", velocity);
        }
    }

    /// <summary> Calculate if the enemy should turn based on velocity, and angle to the player. </summary>
    /// <param name="angleToPlayer"></param>
    /// <returns> True if the enemy should turn, false if the enemy shouldn't turn. </returns>
    public bool ShouldProcessTurn(float angleToPlayer) {
        float angleToPlayer_u = Mathf.Abs(angleToPlayer);
        bool inDeadZone = angleToPlayer_u < 10f;
        bool passedDeadZone = angleToPlayer_u > 20f;

        if (velocity < 0.5f) {
            // release lock when close enough
            if (angleToPlayer_u < 5f) rotationDeadzoneLock = false;

            // enter lock when we cross the outer threshold
            if (!rotationDeadzoneLock && passedDeadZone) {
                lockAngle = (angleToPlayer > 0) ? 1f : -1f;
                rotationDeadzoneLock = true;
            }

            if (!rotationDeadzoneLock) {
                // smoothly return to 0 when unlocked
                turnParam = Mathf.MoveTowards(turnParam, 0f, turnBlendSpeed * Time.fixedDeltaTime);
                anim.SetFloat("Turn", turnParam);
                lockAngle = 0f;
                return false;
            }

            // Smoothly approach -1 or +1 while locked
            turnParam = Mathf.MoveTowards(turnParam, lockAngle, turnBlendSpeed * Time.fixedDeltaTime);
            anim.SetFloat("Turn", turnParam);
        } else {
            // moving fast: unlock and smooth back to 0
            rotationDeadzoneLock = false;
            lockAngle = 0f;
            turnParam = Mathf.MoveTowards(turnParam, 0f, turnBlendSpeed * Time.fixedDeltaTime);
            anim.SetFloat("Turn", turnParam);
        }

        return true;
    }

    public void ResetTurnAnimation() {
        turnParam = 0f;
        anim.SetFloat("Turn", 0f);
    }
}