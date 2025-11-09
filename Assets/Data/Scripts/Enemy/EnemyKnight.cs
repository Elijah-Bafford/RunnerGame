
using UnityEngine;

public class EnemyKnight : Enemy {

    protected override void ActionMove() {
        base.ActionMove();
        Vector3 playerDirection = player.transform.position - transform.position;
        playerDirection.y = 0f;

        float sqrDirMag = playerDirection.sqrMagnitude;
        playerDirection = playerDirection.normalized;

        // update rotation
        Vector3 localPos = transform.InverseTransformPoint(player.transform.position);
        float angleToPlayer = Mathf.Atan2(localPos.x, localPos.z) * Mathf.Rad2Deg;
        if (enemyAnimator.ShouldProcessTurn(angleToPlayer)) {
            Quaternion targetRot = Quaternion.LookRotation(playerDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);
        }

        if (sqrDirMag <= _stopDistance) {
            _inRangeForAttack = Mathf.Abs(angleToPlayer) < 10f;
            return;
        }

        Vector3 vel = rb.linearVelocity;
        Vector3 targetXZ = playerDirection * _movementSpeed;
        Vector3 currentXZ = new Vector3(vel.x, 0f, vel.z);

        Vector3 newXZ = Vector3.MoveTowards(currentXZ, targetXZ, _movementSpeed * 10 * Time.fixedDeltaTime);

        // Preserve vertical velocity
        rb.linearVelocity = new Vector3(newXZ.x, vel.y, newXZ.z);
    }

    protected override bool ActionAttack() {
        if (!base.ActionAttack()) return false;
        enemyAnimator.TriggerMeleeAttack();
        return true;
    }
}