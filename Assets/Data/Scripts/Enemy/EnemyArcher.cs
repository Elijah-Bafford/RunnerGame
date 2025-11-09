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

    private float _currentAimAngle = 0f;

    private bool bowIsDrawn = false;

    protected override void FixedUpdate() {
        base.FixedUpdate();
        AimAtPlayer();
    }

    private void AimAtPlayer() {
        if (!bowIsDrawn) return;

        Vector3 toPlayer = (player.transform.position - _aimingBaseline.transform.position).normalized;
        
        float dot = Vector3.Dot(_aimingBaseline.transform.forward, toPlayer);

        float aimAngle = 0f;

        if (dot >= Mathf.Cos(_aimFOV * Mathf.Deg2Rad)) { // Within FOV
            // Actual magic
            Vector3 localPlayerPos = _aimingBaseline.InverseTransformPoint(player.transform.position);
            aimAngle = Mathf.Atan2(localPlayerPos.y, localPlayerPos.z) * Mathf.Rad2Deg;
        }

        _currentAimAngle = Mathf.Lerp(_currentAimAngle, aimAngle, _aimLerpSpeed * Time.fixedDeltaTime);

        enemyAnimator.SetAimAngle(_currentAimAngle); // Clamped between -35 - 35
    }

    /// <summary>
    /// Player is in sight
    /// </summary>
    protected override void ActionMove() {
        base.ActionMove();
        Vector3 playerDirection = player.transform.position - transform.position;
        playerDirection.y = 0f;

        float sqrDirMag = playerDirection.sqrMagnitude;
        playerDirection = playerDirection.normalized;

        // update rotation
        Vector3 localPos = transform.InverseTransformPoint(player.transform.position);
        float angleToPlayer = Mathf.Atan2(localPos.x, localPos.z) * Mathf.Rad2Deg;

        Quaternion targetRot = Quaternion.LookRotation(playerDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);
        //_inRangeForAttack = Mathf.Abs(angleToPlayer) < 5f;

        //if (sqrDirMag <= _stopDistance) {
        //    
        //    return;
        //}
    }

    /// <summary>
    /// Player is in range for attack
    /// </summary>
    /// <returns></returns>
    protected override bool ActionAttack() {
        if (!base.ActionAttack()) return false; // Check for attack cooldown
        SetBowDrawn(true);

        return true;
    }

    private void SetBowDrawn(bool bowIsDrawn) {
        this.bowIsDrawn = bowIsDrawn;
        enemyAnimator.SetBowDrawn(bowIsDrawn);
    }

    #region Animation Events

    public void TriggerBowDrawstring() => _bowAnimator.SetBool("Draw", !_bowAnimator.GetBool("Draw"));

    public void FireBow() {
        _bowAnimator.SetTrigger("Loose");
        _bowAnimator.SetBool("Draw", false);
    }

    public void CancelDraw() {
        _bowAnimator.SetBool("Draw", false);
    }

    public void SetConstrainedArrow(bool active) {
        _constrainedArrow.SetActive(active);
    }

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

    #endregion

    public Animator GetBowAnimator() => _bowAnimator;
}
