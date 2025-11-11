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
        // Rotate the enemy's transform to face the player
        RotateTowardPlayerInAim(out Vector3 playerDir);

        // Check if the player is too close to the archer
        if (playerDir.sqrMagnitude <= _radiusOfSatisfaction) {
            //SetBowDrawn_Enemy(false);
            //_playerIsTooClose = true;
            //_currentAimAngle = 0f;
            //return;
        }
        
        /* 
         * Get the local position of the player relative to the aiming baseline.
         * The aiming baseline is a transform attached as a direct child of the enemy archer, it can only be rotated by the
         * enemies main transform. (i.e. this.transform). It's forward vector acts as a flat line that leads directly forward from
         * where the arrow (when released) will fly.
        */
        Vector3 localPlayerPos = _aimingBaseline.InverseTransformPoint(player.TargetingPos);
        Vector3 playerAimDir = (player.TargetingPos - _aimingBaseline.position).normalized;

        // Set turn animation based off of horizontal angle to the player
        HorizontallyAlign(localPlayerPos, out float horizontalDeg2Player);
        // If the player is within _aimFOV (dot) then get the ACTUAL vertical angle to the player (based off of aim baseline as 0 degrees) 
        VerticallyAlign(localPlayerPos, playerAimDir, out float verticalDeg2Player);

        // The player is within 1 degree of the enemy's forward
        bool inRange_h = Mathf.Abs(horizontalDeg2Player) < 1f;

        if (!enemyAnimator.IsBowDrawn()) return;

        // Enemy must stay lined up with the player for the hold duration
        if (inRange_h) {
            _holdTimer += Time.fixedDeltaTime;
            if (_holdTimer >= _holdDuration) {
                _playerTargeted = true;
                _holdTimer = 0;
            }
        } else {
            if (_holdTimer > 0) _holdTimer -= Time.fixedDeltaTime;
            if (_holdTimer < 0) _holdTimer = 0;
        }
    }

    private void RotateTowardPlayerInAim(out Vector3 playerDirection) {
        // Direction from the aiming baseline to the player, flattened to XZ
        Vector3 playerDir = player.TargetingPos - _aimingBaseline.position;
        playerDir.y = 0f;   
        if (playerDir.sqrMagnitude > 0.0001f)
            playerDir.Normalize();

        // Current horizontal forward of the aiming baseline
        Vector3 baselineForward = _aimingBaseline.forward;
        baselineForward.y = 0f;
        if (baselineForward.sqrMagnitude > 0.0001f)
            baselineForward.Normalize();

        // How much we need to rotate so that baselineForward points to playerDir
        Quaternion delta = Quaternion.FromToRotation(baselineForward, playerDir);

        // We only care about yaw (Y axis) — kill any pitch/roll just in case
        Vector3 deltaEuler = delta.eulerAngles;
        delta = Quaternion.Euler(0f, deltaEuler.y, 0f);

        // Apply this delta to the enemy root
        Quaternion targetRot = delta * transform.rotation;

        // Smoothly rotate towards that
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);

        playerDirection = playerDir;
    }



    private void HorizontallyAlign(Vector3 localPlayerPosition, out float horizontalDeg2Player) {
        horizontalDeg2Player = Mathf.Atan2(localPlayerPosition.x, localPlayerPosition.z) * Mathf.Rad2Deg;
        horizontalDeg2Player = Mathf.Round(horizontalDeg2Player) / 20f;
        enemyAnimator.SetTurnAnimation(horizontalDeg2Player); // Clamped
    }

    private void VerticallyAlign(Vector3 localPlayerPosition, Vector3 playerAimDirection, out float verticalDeg2Player) {
        float dot = Vector3.Dot(_aimingBaseline.transform.forward, playerAimDirection);

        verticalDeg2Player = 0f;

        if (dot >= Mathf.Cos(_aimFOV * Mathf.Deg2Rad)) { // Within FOV
            verticalDeg2Player = Mathf.Atan2(localPlayerPosition.y, localPlayerPosition.z) * Mathf.Rad2Deg;
        }
        _currentAimAngle = Mathf.Lerp(_currentAimAngle, verticalDeg2Player, _aimLerpSpeed * Time.fixedDeltaTime);
        enemyAnimator.SetAimAngle(_currentAimAngle); // Clamped between -35 - 35
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
