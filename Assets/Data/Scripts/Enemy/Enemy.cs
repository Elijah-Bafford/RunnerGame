using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable {

    [Header("Hit multipliers")]
    [SerializeField] private float hitForceMultiplier = 3f;
    [SerializeField] private float torqueMultiplier = 6f;
    [SerializeField] private Transform player;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float turnSpeed = 180f;


    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    public enum State { Idle, Move, Attack, Dead }
    private State currentState;
    private State lastState;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        startPosition = rb.position;
        startRotation = rb.rotation;
    }

    private void Update() {
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
        if (currentState == lastState && currentState != State.Move) return;
        lastState = currentState;
        switch (currentState) {
            case State.Idle:
                break;
            case State.Move:
                MoveTowardPlayer();
                break;
            case State.Attack:
                break;
            case State.Dead:
                StartCoroutine(DisableAfter(3f));
                break;
        }
        print(currentState);
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

    private void MoveTowardPlayer() {
        if (currentState == State.Dead) return;
        // 1) get direction to player…
        Vector3 dir = player.position - transform.position;
        // 2) kill vertical difference so we only turn on Y:
        dir.y = 0f;

        // 3) make sure there's something to look at
        if (dir.sqrMagnitude < 0.001f) return;

        // 4) build target rotation
        Quaternion targetRot = Quaternion.LookRotation(dir);

        // 5) smoothly rotate toward it
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            turnSpeed * Time.deltaTime
        );
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
