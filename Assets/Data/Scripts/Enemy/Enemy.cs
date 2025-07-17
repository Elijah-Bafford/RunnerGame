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
    [SerializeField] private Transform player;
    [SerializeField] private Animator anim;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool canAttack = true;

    public enum State { Idle, Move, Attack, Dead }
    private State currentState;
    private State lastState;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        startPosition = rb.position;
        startRotation = rb.rotation;
    }

    private void FixedUpdate() {
        RunCurrentState();
    }

    private void OnEnable() {
        currentState = State.Idle;

        // Reset physics state
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset position and rotation
        rb.position = startPosition;
        rb.rotation = startRotation;

        rb.freezeRotation = true;
    }

    private void RunCurrentState() {
        bool stateChanged = lastState != currentState;
        // If the state didn't change return, unless state == move
        if (!stateChanged && currentState != State.Move) return;
        lastState = currentState;
        switch (currentState) {
            case State.Idle:
                break;
            case State.Move:
                MoveTowardPlayer();
                break;
            case State.Attack:
                AttackPlayer();
                break;
            case State.Dead:
                StartCoroutine(DisableAfter(3f));
                break;
        }
    }

    public void Hit(Vector3 hitForce) {
        if (currentState == State.Dead) return;

        rb.freezeRotation = false;
        rb.AddForce(hitForce * hitForceMultiplier, ForceMode.Impulse);

        Vector3 randomTorque = new Vector3(
           Random.Range(-0.5f, 0.5f),
           Random.Range(-0.5f, 0.5f),
           Random.Range(-0.5f, 0.5f)
       ).normalized * torqueMultiplier;

        rb.AddTorque(randomTorque, ForceMode.Impulse);

        SetState(State.Dead);
    }

    private void AttackPlayer() {
        if (!canAttack) return;

    }

    private void MoveTowardPlayer() {
        if (currentState == State.Dead || currentState == State.Attack) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        float sqrDist = dir.sqrMagnitude;
        dir = dir.normalized;

        if (sqrDist < 0.001f) return;

        FacePlayer(dir);

        if (sqrDist > stopDistance * stopDistance) Move(dir);
        else rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    private void FacePlayer(Vector3 direction) {
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            turnSpeed * Time.deltaTime
        );
    }

    private void Move(Vector3 direction) {
        Vector3 targetVelocity = direction * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
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

    public void SetState(State state) {
        currentState = state;
    }
}
