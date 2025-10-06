using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public abstract class EnemyBase : MonoBehaviour {

    [Header("Hit multipliers")]
    [SerializeField] protected float hitForceMultiplier = 3f;
    [SerializeField] protected float torqueMultiplier = 6f;
    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float turnSpeed = 720f;
    [Header("Refs")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckRadius = 0.2f;

    protected Rigidbody rb;

    // Stop all combat
    public static bool stopCombat = false;

    public enum State { Idle, Move, Attack, Dead }
    protected State currentState;

    // Player refs
    protected Player player;
    protected Transform playerTransform;

    // Attacking
    protected bool weaponCanHit = false;
    protected bool inRangeForAttack = false;
    protected bool attackAnimPlaying = false;

    protected bool isGrounded = false;
    protected bool disableStateChange = false;
    protected bool isDead = false;

    protected virtual void Awake() {
        player = Player.Instance;
        playerTransform = player.transform;
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void FixedUpdate() {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    protected virtual void OnEnable() {
        currentState = State.Idle;

        stopCombat = false;
        isDead = false;

        // Reset physics state
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.freezeRotation = true;
        disableStateChange = false;
    }

    protected virtual void Move(Vector3 direction) {
        Vector3 targetVelocity = direction * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    public virtual void Hit(Vector3 hitForce) {
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
    }

    /// <summary>
    /// Disables the enemy after {delay}
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    protected virtual IEnumerator DisableAfter(float delay) {
        yield return new WaitForSeconds(delay);

        // Reset velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called in EnemyTrigger. When the player is within the attack range of the enemy, start attacking.
    /// </summary>
    /// <param name="attacking"></param>
    public void SetInRangeForAttack(bool attacking) { inRangeForAttack = attacking; }
    public bool IsDead() { return isDead; }
    public void WeaponCanHit(bool canHit) { weaponCanHit = canHit; }

    public void SetState(State state) { currentState = state; }

}
