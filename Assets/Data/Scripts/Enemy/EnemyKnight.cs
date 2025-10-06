using UnityEngine;

public class EnemyKnight : MonoBehaviour {

    public static bool StopCombat = false;

    #region Fields
    [Header("Basic Config")]
    [SerializeField] private float _movementSpeed = 4f;
    [SerializeField] private float _turnSpeed = 1f;
    [Tooltip("How close the enemy gets to the player before stopping")]
    [SerializeField] private float _stopDistance = 2f;

    public enum State { Idle, Move, Attack, Dead }
    private State _currentState = State.Idle;
    private State _lastState = State.Idle;

    private bool _playerInViewDistance = false;
    private bool _playerInSight = false;
    private bool _inRangeForAttack = false;
    private bool _isDead = false;

    private Rigidbody rb;
    private Player player;
    private EnemyAnimator enemyAnimator;

    #endregion

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        player = Player.Instance;
        enemyAnimator = new EnemyAnimator(this);
    }

    private void FixedUpdate() {
        _lastState = _currentState;
        _playerInSight = RunPlayerDetection();
        DecideState();
        enemyAnimator.UpdateAnimations();
    }

    #region Player Detection

    /// <summary>
    /// Check for the player.
    /// If the player is in view distance, then cast a ray towards the player,
    /// if the ray hits (only) the player, then the player is in sight.
    /// </summary>
    private bool RunPlayerDetection() {
        if (!_playerInViewDistance) return false;

        Vector3 origin = transform.position;
        Vector3 direction = (player.transform.position - origin).normalized;

        float distanceToPlayer = Vector3.Distance(origin, player.transform.position);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distanceToPlayer, ~0)) {
            if (!hit.collider.CompareTag("Player")) return false;
            if (hit.collider.CompareTag("Wall")) return false;
            if (hit.collider.CompareTag("Ground")) return false;
            if (hit.collider.CompareTag("SlopedGround")) return false;
            return true;
        }
        return false;
    }
    #endregion

    #region State Machine

    /// <summary>
    /// Set the current State (enum) based on various bools.
    /// </summary>
    private void DecideState() {
        if (_currentState == State.Dead) return;

        if (_isDead)
            DecideAction(State.Dead);
        else if (_inRangeForAttack)
            DecideAction(State.Attack);
        else if (_playerInSight)
            DecideAction(State.Move);
        else
            DecideAction(State.Idle);

        print(_currentState);
    }

    /// <summary>
    /// Decide Enemy's next action based on the State (enum) arg.
    /// </summary>
    /// <param name="s"></param>
    private void DecideAction(State s) {
        _currentState = s;
        switch (s) {
            case State.Idle: ActionIdle(); break;
            case State.Move: ActionMove(); break;
            case State.Attack: ActionAttack(); break;
            case State.Dead: ActionDead(); break;
        }
    }
    #endregion

    #region Action States

    /// <summary>
    /// Single time update on state change to "Idle"
    /// </summary>
    private void ActionIdle() {
        if (_lastState == State.Idle) return;
        // nothing happens here
    }

    /// <summary>
    /// Continuous update in FixedUpdate while the state is "Move"
    /// </summary>
    private void ActionMove() {
        SingleUpdateActionMove();
        Vector3 playerDirection = player.transform.position - transform.position;
        playerDirection.y = 0f;

        float sqrDirMag = playerDirection.sqrMagnitude;
        playerDirection = playerDirection.normalized;

        // update rotation
        Vector3 localPos = transform.InverseTransformPoint(player.transform.position);
        if (enemyAnimator.ShouldProcessTurn(Mathf.Atan2(localPos.x, localPos.z) * Mathf.Rad2Deg)) {
            Quaternion targetRot = Quaternion.LookRotation(playerDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);
        }

        if (sqrDirMag <= _stopDistance) {
            DecideAction(State.Idle); // temp until implement attack
            //_inRangeForAttack = true;
            return;
        }
        //_inRangeForAttack = false;

        Vector3 vel = rb.linearVelocity;
        Vector3 targetXZ = playerDirection * _movementSpeed;
        Vector3 currentXZ = new Vector3(vel.x, 0f, vel.z);

        Vector3 newXZ = Vector3.MoveTowards(currentXZ, targetXZ, _movementSpeed * 10 * Time.fixedDeltaTime);

        // Preserve vertical velocity
        rb.linearVelocity = new Vector3(newXZ.x, vel.y, newXZ.z);
    }

    private void SingleUpdateActionMove() {
        if (_lastState == State.Move) return;
    }

    /// <summary>
    /// Single time update on state change to "Attack"
    /// </summary>
    private void ActionAttack() {
        if (_lastState == State.Attack) return;
    }

    /// <summary>
    /// Single time update on state change to "Dead"
    /// </summary>
    private void ActionDead() {
        if (_lastState == State.Dead) return;
    }
    #endregion

    #region Setters

    /// <summary>
    /// Call in PlayerDetection script on trigger enter / exit
    /// </summary>
    /// <param name="playerInRange"></param>
    public void SetPlayerInViewDistance(bool playerInRange) {
        _playerInViewDistance = playerInRange;
    }
    public Animator GetAnimator() { return GetComponentInChildren<Animator>(); }
    public Rigidbody GetRigidbody() { return rb; }
    public float GetMovementSpeed() { return _movementSpeed; }

    #endregion
}