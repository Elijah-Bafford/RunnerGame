using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable {

    [Header("Hit multipliers")]
    [SerializeField] private float hitForceMultiplier = 3f;
    [SerializeField] private float torqueMultiplier = 6f;
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 360f;
    [Tooltip("How close the enemy gets to the player.")]
    [SerializeField] private float stopDistance = 1.25f;
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
    private bool isDead = false;

    // Attacking
    private bool inDamageDealingFrames = false;
    private bool inRangeForAttack = false;
    private bool isInAttackAnimation = false;

    private bool isGrounded = false;
    private bool disableStateChange = false;

    public enum State { Idle, Move, Attack, Dead }
    private State currentState;
    private State lastState;

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

        // If the state hasn't changed AND the current state isn't move THEN return
        if (lastState == currentState && currentState != State.Move) return;

        bool isMoveState = currentState == State.Move;
        bool animIsMove = anim.GetBool("Move");

        if (!isMoveState && animIsMove) anim.SetBool("Move", false);

        lastState = currentState;

        switch (currentState) {
            case State.Idle:
                break;
            case State.Move:
                if (!animIsMove) anim.SetBool("Move", true);
                MoveTowardPlayer();
                break;
            case State.Attack:
                AttackPlayer();
                break;
            case State.Dead:
                anim.SetBool("Dead", true);
                isDead = true;
                StartCoroutine(DisableAfter(3f));
                disableStateChange = true;
                break;
        }
    }

    private void AttackPlayer() {
        if (!inRangeForAttack) {
            currentState = State.Move;
            return;
        }

        if (!isInAttackAnimation) anim.SetTrigger("Attack");
        if (inDamageDealingFrames) {
            player.Die();
        }
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
    }

    private void MoveTowardPlayer() {
        if (isDead || currentState == State.Attack) return;
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;

        float sqrDist = dir.sqrMagnitude;
        dir = dir.normalized;

        if (sqrDist < 0.001f) return;

        FacePlayer(dir);
        if (inRangeForAttack) currentState = State.Attack;
        if (sqrDist > stopDistance * stopDistance && isGrounded) Move(dir);
        else rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    private void Move(Vector3 direction) {
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
    /// In specific Attack animation frames, this is true so that the enemy weapon can actually only deal damage in those frames.
    /// </summary>
    /// <param name="allow"></param>
    internal void AllowHit(bool allow) { inDamageDealingFrames = allow; }

    /// <summary>
    /// Called in EnemyTrigger. When the player is within the attack range of the enemy, start attacking.
    /// </summary>
    /// <param name="attacking"></param>
    internal void SetInRangeForAttack(bool attacking) { inRangeForAttack = attacking; }

    /// <summary>
    /// Called in AllowHitEvent. This remains true from the beginning of an attack to the end of the AttackWait animation. This prevents continous attacks
    /// </summary>
    /// <param name="isInAttack"></param>
    internal void SetIsInAttackAnimation(bool isInAttack) { isInAttackAnimation = isInAttack; }
}