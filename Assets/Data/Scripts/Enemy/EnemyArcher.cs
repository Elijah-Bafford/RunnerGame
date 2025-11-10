using System.Collections;
using UnityEngine;

public class EnemyArcher : Enemy {

    [SerializeField] private Animator _bowAnimator;
    [Header("Arrow References")]
    [Tooltip("The arrow that is always attached to the enemy. This is used as a reference when creating a new arrow.")]
    [SerializeField] private GameObject _constrainedArrow;
    [SerializeField] private GameObject _arrowPrefab;
    [Header("Aiming")]
    [SerializeField] private Transform _aimingBaseline;
    [SerializeField] private float _aimLerpSpeed = 7f;
    [SerializeField] private float _aimFOV = 45;
    [Tooltip("The amount of time that the enemy has to be locked on to fire")]
    [SerializeField] private float _holdDuration = 1.2f;

    private float _holdTimer = 0f;

    private float _currentAimAngle = 0f;

    private bool _playerIsTooClose = false;
    private bool _playerTargeted = false;

    private bool _allowStateChange = true;

    protected override void FixedUpdate() {
        base.FixedUpdate();
    }

    protected override void DecideState() {
        if (_currentState == State.Dead) return;
        if (StopCombat) return;

        if (_currentState != State.Move) enemyAnimator.SetTurnAnimation(0);
        _currentState =
            _isDead ? State.Dead :
            _isStunned ? State.Stunned :
            //_playerIsTooClose ? State.Move :
            _playerTargeted ? State.Attack :
            _playerInSight ? State.Target : State.Idle;

        if (_allowStateChange || _isDead || _isStunned) DecideAction(_currentState);
    }

    protected override void ActionTarget() {
        base.ActionTarget();
        if (_playerIsTooClose || _playerTargeted) {
            SetBowDrawn_Enemy(false);
            return;
        }
        SetBowDrawn_Enemy(true);

        Vector3 playerDir = (player.transform.position - _aimingBaseline.transform.position);
        Vector3 toPlayer_n = playerDir.normalized;
        playerDir.y = 0f;

        Vector3 localPlayerPos = _aimingBaseline.InverseTransformPoint(player.transform.position);

        float horizontalDeg2Player = Mathf.Atan2(localPlayerPos.x, localPlayerPos.z) * Mathf.Rad2Deg;

        // Horizontal (Rotate towards player)
        horizontalDeg2Player = Mathf.Round(horizontalDeg2Player) / 20f;
        enemyAnimator.SetTurnAnimation(horizontalDeg2Player); // Clamped

        Quaternion targetRot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);

        print("Sqr Mag: " + playerDir.sqrMagnitude + "<= " + _stopDistance + " ~ " + (playerDir.sqrMagnitude <= _stopDistance));

        if (playerDir.sqrMagnitude <= _stopDistance) {
            
            //_playerIsTooClose = true;
            //_currentAimAngle = 0f;
            //SetBowDrawn(false);
            //return;
        }

        // Vertical (Aiming)
        float dot = Vector3.Dot(_aimingBaseline.transform.forward, toPlayer_n);
        float verticalDeg2Player = 0f;
        if (dot >= Mathf.Cos(_aimFOV * Mathf.Deg2Rad)) { // Within FOV
            verticalDeg2Player = Mathf.Atan2(localPlayerPos.y, localPlayerPos.z) * Mathf.Rad2Deg;
        }
        _currentAimAngle = Mathf.Lerp(_currentAimAngle, verticalDeg2Player, _aimLerpSpeed * Time.fixedDeltaTime);
        enemyAnimator.SetAimAngle(_currentAimAngle); // Clamped between -35 - 35

        bool inRange_v = Mathf.Abs(_currentAimAngle) < 5f;
        bool inRange_h = Mathf.Abs(horizontalDeg2Player) < 1f;
        print("|Aim Angle |: " + Mathf.Abs(_currentAimAngle) + "< 2f" + " ~ " + inRange_v);
        print("|Turn Angle|: " + Mathf.Abs(horizontalDeg2Player) + "< 1f" + " ~ " + inRange_h);
        if (inRange_v && inRange_h) {
            _holdTimer += Time.fixedDeltaTime;
            if (_holdTimer >= _holdDuration) {
                _playerTargeted = true;
                _holdTimer = 0;
            }
        } else {
            if (_holdTimer > 0f) _holdTimer -= Time.fixedDeltaTime;
            if (_holdTimer < 0f) _holdTimer = 0f;
        }
    }

    /// <summary>
    /// Player is in sight
    /// </summary>
    protected override void ActionMove() {
        base.ActionMove();
        _playerIsTooClose = false;
        //Vector3 playerDirection = player.transform.position - transform.position;
        //playerDirection.y = 0f;

        //float sqrDirMag = playerDirection.sqrMagnitude;
        //playerDirection = playerDirection.normalized;

        //Vector3 localPos = transform.InverseTransformPoint(player.transform.position);
        //float angleToPlayer = Mathf.Atan2(localPos.x, localPos.z) * Mathf.Rad2Deg;

        //angleToPlayer = Mathf.Round(angleToPlayer) / 20f;

        //enemyAnimator.SetTurnAnimation(angleToPlayer); // Clamped

        //Quaternion targetRot = Quaternion.LookRotation(playerDirection);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);
        //_inRangeForAttack = Mathf.Abs(angleToPlayer) < 5f;

        //if (sqrDirMag <= _stopDistance) {

        //    return;
        //}
    }

    /// <summary>
    /// Player is in range for attack
    /// </summary>
    /// <returns></returns>
    protected override bool ActionAttack() {
        if (!base.ActionAttack()) return false; // Check for attack cooldown
        ReleaseBow_Enemy();
        return true;
    }

    protected override IEnumerator AttackCooldown() {
        _playerTargeted = false;
        _allowStateChange = false;
        yield return new WaitForSeconds(_attackCooldownTime);
        _allowStateChange = true;
        _attackCoolDown = null;
    }

    #region Humanoid Animation Events

    private void SetBowDrawn_Enemy(bool bowIsDrawn) => enemyAnimator.SetBowDrawn(bowIsDrawn);
    private void ReleaseBow_Enemy() => enemyAnimator.TriggerRangeAttack();
    public void DisableBowDrawn() => SetBowDrawn_Enemy(false);
    #endregion

    #region Bow Animation Events
    public Animator GetBowAnimator() => _bowAnimator;


    // DO NOT CALL THESE IN SCRIPT!!
    public void SetBowDrawstring(bool stringPulled) => _bowAnimator.SetBool("Draw", stringPulled);

    public void BowStringFired() {
        _bowAnimator.SetTrigger("Loose");
        _bowAnimator.SetBool("Draw", false);
    }

    public void SetConstrainedArrow(bool active) {
        _constrainedArrow.SetActive(active);
    }

    #endregion

    /// <summary>
    /// Called when the arrow should be released from the bow
    /// </summary>
    public Arrow CreateLoosedArrow() {
        Vector3 pos = _constrainedArrow.transform.position;
        Quaternion rot = _constrainedArrow.transform.rotation * _arrowPrefab.transform.rotation;
        GameObject a = Instantiate(_arrowPrefab, pos, rot);
        a.SetActive(true);
        return GetComponent<Arrow>();
    }
}
