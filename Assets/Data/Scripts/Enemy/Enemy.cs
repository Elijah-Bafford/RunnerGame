using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable {

    [Header("Hit multipliers")]
    [SerializeField] private float hitForceMultiplier = 3f;
    [SerializeField] private float torqueMultiplier = 6f;
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 720f;
    [Header("Refs")]
    [SerializeField] Player player;
    [SerializeField] private Animator anim;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    // Stop all combat
    public static bool stopCombat = false;

    private Transform playerTransform;
    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    // Attacking
    private bool weaponCanHit = false;
    private bool inRangeForAttack = false;
    private bool attackAnimPlaying = false;

    private bool isGrounded = false;
    private bool disableStateChange = false;
    private bool isDead = false;

    public enum State { Idle, Move, Attack, Dead }
    private State currentState;

    private void Awake() {
        playerTransform = player.GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        startPosition = rb.position;
        startRotation = rb.rotation;
    }

    private void FixedUpdate() {
        UpdatePhysics();
        RunCurrentState();
    }

    private void UpdatePhysics() {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnEnable() {
        currentState = State.Idle;

        stopCombat = false;
        isDead = false;

        // Reset physics state
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset position and rotation
        rb.position = startPosition;
        rb.rotation = startRotation;

        rb.freezeRotation = true;
        disableStateChange = false;
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

    public void WeaponCanHit(bool canHit) {
        weaponCanHit = canHit;
    }

    public void Hit(Vector3 hitForce) {
        if (isDead) return;
        isDead = true;

        rb.freezeRotation = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(hitForce * hitForceMultiplier, ForceMode.Impulse);

        Vector3 randomTorque = new Vector3(
           Random.Range(-0.5f, 0.5f),
           Random.Range(-0.5f, 0.5f),
           Random.Range(-0.5f, 0.5f)
       ).normalized * torqueMultiplier;

        rb.AddTorque(randomTorque, ForceMode.Impulse);

        SetState(State.Dead);
        disableStateChange = true;
        anim.ResetTrigger("Attack");
        anim.SetBool("Move", false);
        anim.SetBool("Dead", true);
        StartCoroutine(DisableAfter(3f));
    }

    private void MoveTowardPlayer() {
        if (isDead || currentState == State.Attack) return;
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;

        float sqrDist = dir.sqrMagnitude;
        dir = dir.normalized;

        if (sqrDist < 0.001f) return;

        FacePlayer(dir);

        if (inRangeForAttack) {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            currentState = State.Attack;
        } else if (isGrounded) {
            Move(dir);
        }
    }

    private void Move(Vector3 direction) {
        if (inRangeForAttack || currentState == State.Attack || anim.GetBool("Attack")) return;
        Vector3 targetVelocity = direction * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    private void FacePlayer(Vector3 direction) {
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Disables the enemy after {delay}
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator DisableAfter(float delay) {
        yield return new WaitForSeconds(delay);

        // Reset velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        gameObject.SetActive(false);
    }

    internal bool IsDead() { return isDead; }

    internal void SetState(State state) { currentState = state; }

    /// <summary>
    /// Called in EnemyTrigger. When the player is within the attack range of the enemy, start attacking.
    /// </summary>
    /// <param name="attacking"></param>
    internal void SetInRangeForAttack(bool attacking) { inRangeForAttack = attacking; }
}