using System.Collections;
using UnityEngine;

public class EnemyOld : EnemyBase {
    
    protected override void FixedUpdate() {
        base.FixedUpdate();
        RunCurrentState();
    }

    private void RunCurrentState() {
        if (disableStateChange) return;
        if (stopCombat) currentState = State.Idle;

        if (currentState != State.Dead) {
            attackAnimPlaying = anim.GetCurrentAnimatorStateInfo(0).IsName("Attack");
            if (attackAnimPlaying) {
                if (weaponCanHit && inRangeForAttack && !isDead) player.Die();
                return;
            } else {
                if (inRangeForAttack) currentState = State.Attack;
            }
        }

        bool animIsMove = anim.GetBool("Move");

        if (currentState != State.Move && animIsMove) anim.SetBool("Move", false);

        switch (currentState) {
            case State.Idle:
                break;
            case State.Move:
                if (!animIsMove) anim.SetBool("Move", true);
                if (inRangeForAttack) SetState(State.Attack);
                MoveTowardPlayer();
                break;
            case State.Attack:
                AttackPlayer();
                break;
            case State.Dead:
                break;
        }
    }

    private void AttackPlayer() {
        if (!attackAnimPlaying) anim.SetTrigger("Attack");
        else return;
    }

    public override void Hit(Vector3 hitForce) {
        base.Hit(hitForce);

        anim.ResetTrigger("Attack");
        anim.SetBool("Move", false);
        anim.SetBool("Dead", true);
        StartCoroutine(DisableAfter(3f));
    }

    protected override void Move(Vector3 direction) {
        if (inRangeForAttack || currentState == State.Attack || anim.GetBool("Attack")) return;
        base.Move(direction);
    }

    private void MoveTowardPlayer() {
        if (isDead || currentState == State.Attack) return;
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;

        float sqrDist = dir.sqrMagnitude;
        dir = dir.normalized;

        if (sqrDist < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

        if (inRangeForAttack) {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            currentState = State.Attack;
        } else if (isGrounded) {
            Move(dir);
        }
    }
}