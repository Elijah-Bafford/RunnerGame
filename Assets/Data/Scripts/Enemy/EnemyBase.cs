using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class EnemyBase : MonoBehaviour {

    public static bool StopCombat = false;

    #region Fields
    [Header("Basic Config")]
    [SerializeField] protected float _movementSpeed = 4f;
    [SerializeField] protected float _turnSpeed = 1f;
    [Tooltip("How close the enemy gets to the player before stopping")]
    [SerializeField] protected float _stopDistance = 2f;
    [Tooltip("The time in seconds after an attack before the enemy can do anything")]
    [SerializeField] protected float _attackCooldownTime = 1.0f;
    [Tooltip("The position of the enemy's head for player detection")]
    [SerializeField] protected Transform _headPosition;
    [Header("Debug")]
    [SerializeField] protected bool _showDebugMessages = true;

    protected Coroutine _attackCoolDown = null;

    public enum State { Idle, Move, Attack, Hit, Dead }
    protected State _currentState = State.Idle;
    protected State _lastState = State.Idle;

    protected bool _playerInViewDistance = false;
    protected bool _playerInSight = false;
    protected bool _inRangeForAttack = false;
    protected bool _isHit = false;
    protected bool _isDead = false;

    protected Rigidbody rb;
    protected Player player;
    protected EnemyAnimator enemyAnimator;

    #endregion

    protected virtual void Awake() {
        rb = GetComponent<Rigidbody>();
        player = Player.Instance;
        enemyAnimator = new EnemyAnimator(this);
    }

    protected virtual void FixedUpdate() {
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
    protected virtual bool RunPlayerDetection() {
        if (!_playerInViewDistance) return false;

        Vector3 origin = _headPosition.position;
        Vector3 direction = (player.transform.position - origin).normalized;

        float distanceToPlayer = Vector3.Distance(origin, player.transform.position);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distanceToPlayer, ~0)) {
            if (hit.collider.CompareTag("Player")) {
                PInfo("Player Detected");
                return true;
            }
            PInfo("Player Not Detected");
        }
        return false;
    }

    #endregion

    #region State Machine

    /// <summary>
    /// Set the current State (enum) based on various bools.
    /// </summary>
    protected virtual void DecideState() {
        if (_currentState == State.Dead) return;
        if (StopCombat) return;

        if (_currentState != State.Move) enemyAnimator.ResetTurnAnimation();
        _currentState =
            _isDead ? State.Dead :
            _isHit ? State.Hit :
            _inRangeForAttack ? State.Attack :
            _playerInSight ? State.Move : State.Idle;

        DecideAction(_currentState);
    }

    /// <summary>
    /// Decide Enemy's next action based on the State (enum) arg.
    /// </summary>
    /// <param name="s"></param>
    protected virtual void DecideAction(State s) {
        _currentState = s;
        switch (s) {
            case State.Idle: ActionIdle(); break;
            case State.Move: ActionMove(); break;
            case State.Attack: ActionAttack(); break;
            case State.Hit: ActionHit(); break;
            case State.Dead: ActionDead(); break;
        }
    }
    #endregion

    #region Action States

    /// <summary>
    /// Handle the event where the enemy idles
    /// (Single cycle action)
    /// </summary>
    /// <returns>
    /// False on early exit, true on run
    /// </returns>
    protected virtual bool ActionIdle() {
        if (_lastState == State.Idle) return false;
        PInfo(_currentState);
        return true;
    }

    /// <summary>
    /// Handle the event where the enemy moves
    /// (Continuous cycle action)
    /// </summary>
    protected virtual void ActionMove() {
        SingleUpdateActionMove();
    }

    /// <summary>
    /// Handle the event where the enemy starts moving
    /// (Single cycle action)
    /// </summary>
    /// <returns>
    /// False on early exit, true on run
    /// </returns>
    protected virtual bool SingleUpdateActionMove() {
        if (_lastState == State.Move) return false;
        PInfo(_currentState);
        return true;
    }

    /// <summary>
    /// Handle the event where the enemy attacks
    /// (Single cycle action)
    /// </summary>
    /// <returns>
    /// False on early exit, true on run
    /// (Check for attack cooldown coroutine to return)
    /// </returns>
    protected virtual bool ActionAttack() {
        if (_attackCoolDown != null) return false;
        PInfo(_currentState);
        enemyAnimator.TriggerAttack();
        _attackCoolDown = StartCoroutine(AttackCooldown());
        return true;
    }

    /// <summary>
    /// Handle the event where the enemy is hit
    /// (Single cycle action)
    /// </summary>
    /// <returns>
    /// False on early exit, true on run
    /// </returns>
    protected virtual bool ActionHit() {
        if (_lastState == State.Hit) return false;
        StopAllCoroutines();
        PInfo(_currentState);
        return true;
    }

    /// <summary>
    /// Handle the event where the enemy is killed
    /// (Single cycle action)
    /// </summary>
    /// <returns>
    /// False on early exit, true on run
    /// </returns>
    protected virtual bool ActionDead() {
        if (_lastState == State.Dead) return false;
        PInfo(_currentState);
        gameObject.SetActive(false);
        return true;
    }
    #endregion

    protected virtual IEnumerator AttackCooldown() {
        yield return new WaitForSeconds(_attackCooldownTime);
        _inRangeForAttack = false;
        _attackCoolDown = null;
    }

    /// <summary>
    /// Call to hit the enemy
    /// </summary>
    public virtual void Hit() => _isHit = true;

    /// <summary>
    /// Call to kill the enemy
    /// </summary>
    public virtual void Kill() => _isDead = true;

    protected void PInfo(object message, int severity = 0) {
        if (!_showDebugMessages && severity != 0) return;
        switch (severity) {
            case 0: Debug.Log(message); break;
            case 1: Debug.LogWarning(message); break;
            case 2: Debug.LogError(message); break;
        }
    }

    #region Getters / Setters

    /// <summary>
    /// Call in PlayerDetection script on trigger enter / exit
    /// </summary>
    /// <param name="playerInRange"></param>
    public void SetPlayerInViewDistance(bool playerInRange) => _playerInViewDistance = playerInRange;
    public bool IsDead() => _isDead;
    public Animator GetAnimator() => GetComponentInChildren<Animator>();
    public Rigidbody GetRigidbody() => rb;
    public float GetMovementSpeed() => _movementSpeed;
    public EnemyAnimator GetEnemyAnimator() => enemyAnimator;

    #endregion
}
