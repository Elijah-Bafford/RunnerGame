using System;
using UnityEngine;

/// <summary>
/// EnemyBase Helper, instantiate with EnemyBase.
/// </summary>
public class EnemyAnimator {
    // Enemy component Refs
    private Rigidbody rb;
    private Animator anim;
    private Animator bowAnim;
    private Enemy enemy;
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

    public EnemyAnimator(Enemy enemyBase) {
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

    /// <summary>
    /// Loose the bow, firing an arrow. (if the bow is drawn) (Human animator)
    /// </summary>
    public void TriggerRangeAttack() { if (isArcher && IsBowDrawn()) anim.SetTrigger("LooseArrow"); }

    public bool IsBowDrawn() => anim.GetBool("Draw");

    private void UpdateRunAnimation() {
        anim.SetFloat("Health", enemy.GetCurrentHealth());
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

    /// <summary>
    /// Set the angle for the Archer Enemy to look at (if the bow is drawn) (Human animator)
    /// Angle clamped [-35 - 35]
    /// </summary>
    /// <param name="angle"></param>
    public void SetAimAngle(float angle) {
        float c_angle = Mathf.Clamp(angle, -35f, 35f);
        anim.SetFloat("Aim", c_angle);
    }

    /// <summary>
    /// Set the turn amount in the direction 'value'
    /// Angle clamped [-1 - 1]
    /// </summary>
    /// <param name="value">The amount the enemy should appear to turn.</param>
    public void SetTurnAnimation(float value) {
        float clamp = Mathf.Clamp(value, -1f, 1f);
        turnParam = clamp;
        anim.SetFloat("Turn", clamp);
    }

    /// <summary>
    /// Pull the bow back (Human animator)
    /// </summary>
    /// <param name="bowIsDrawn">Set whether the character should draw the bow</param>
    public void SetBowDrawn(bool bowIsDrawn) => anim.SetBool("Draw", bowIsDrawn);
}