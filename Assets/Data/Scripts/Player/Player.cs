using System;
using Unity.Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(-2)]
public class Player : MonoBehaviour {

    public static Player Instance { get; private set; }

    [Header("- Focus Stat")]
    [SerializeField] private float currentFocus = 0;

    [SerializeField] private bool isInvincible = false;

    [Header("General Refs")]
    [SerializeField] private Transform _TargetingPos;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform spawnPoint;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    /// <summary>Event player's maxfocus has changed.</summary>
    public Action<float> OnMaxFocusChanged;
    /// <summary>Directions of the player. Other is a combination of movements, none is not moving.</summary>
    public enum Direction { Forward, Backward, Left, Right, Other, None }
    /// <summary>Actions available to the player</summary>
    public enum Act { Attack, Slide, Jump, Move, Grapple }

    #region Properties

    /// <summary>The current direction the player is moving in.</summary>
    public Direction currentDir { get; private set; } = Direction.None;
    public float MaxFocus => maxFocus;
    public float CurrentFocus => currentFocus;
    public Vector3 TargetingPos => _TargetingPos.position;
    public bool IsSliding { get; set; } = false;
    public bool CanStand { get; set; } = true;
    public bool IsGrounded { get; set; } = false;
    /// <summary>The angle of the slope. 0 if not on a slope (Abs value)</summary>
    public float OnSlopeAngle { get; set; } = 0f;
    /// <summary>The Y position of the player on the slope. Used to check if the player is going up or down a slope</summary>
    public float OnSlopeY { get; set; } = 0f;
    ///<summary>Whether or not the player is in the attack animation.</summary>
    public bool IsInAttack { get; set; } = false;

    #endregion

    #region Private fields

    private float attackDamage = 10f;

    private float moveSpeed = 5f;
    private float jumpForce = 4.8f;

    private float maxFocus = 100f;
    private float startFocus = 0;
    private float focusLossMult = 2f;

    private Rigidbody rb;
    private CapsuleCollider hitbox;
    private Animator anim;

    private CinemachineCamera fstPersonCamera;
    private CinemachinePanTilt cameraPanTilt;

    private GrappleMechanic grappleMech;
    private WallRunMechanic wallRunMech;
    private MomentumMechanic momentumMech;
    private PlayerAttackCollider playerAttack;

    private Vector2 moveVector = Vector2.zero;
    private Vector2 lastMoveVector = Vector2.zero;
    private Vector3 conveyorVelocity = Vector3.zero;

    ///<summary>Whether a restart is pending (execute on first FixedUpdate after true).</summary>
    private bool pendingRespawn = false;
    /// <summary>Player released slide key but couldn't stand.</summary>
    private bool pendingStand = false;

    private float leanAmount = 0.0f;

    private Vector3 groundNormal = Vector3.up;

    #endregion


    private void Awake() {
        if (Instance != null && Instance != this)
            Debug.LogError("Duplicate Player Object");
        Instance = this;

    }

    private void OnDestroy() {
        GameStateHandler.OnLevelRestart -= OnLevelRestart;
        if (Instance == this) Instance = null;
    }

    private void Start() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        hitbox = GetComponent<CapsuleCollider>();
        anim = GetComponentInChildren<Animator>();
        fstPersonCamera = GetComponentInChildren<CinemachineCamera>();
        cameraPanTilt = GetComponentInChildren<CinemachinePanTilt>();
        momentumMech = GetComponent<MomentumMechanic>();
        grappleMech = GetComponent<GrappleMechanic>();
        wallRunMech = GetComponent<WallRunMechanic>();
        playerAttack = GetComponentInChildren<PlayerAttackCollider>();
        wallRunMech.InitWallRunMechanic(this, rb);
        grappleMech.InitGrappleMechanic(this);
        playerAttack.InitPlayerAttack(this);

        GameStateHandler.OnLevelRestart += OnLevelRestart;
        OnLevelRestart();
    }

    private void OnLevelRestart() => pendingRespawn = true;

    private void Respawn() {
        AudioHandler.Instance.StopAll();

        SetPersistentStats();
        pendingRespawn = false;
        CanStand = true;
        SetDead(false, force: true);
        currentDir = Direction.None;

        leanAmount = 0;
        IsSliding = false;
        OnSlopeAngle = 0;
        OnSlopeY = 0;

        currentFocus = startFocus;

        moveVector = Vector3.zero;
        lastMoveVector = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        anim.Play("Idle", 0, 0f);

        cameraPanTilt.PanAxis.Value = 0;
        cameraPanTilt.TiltAxis.Value = 0;

        if (spawnPoint != null) {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        IsInAttack = false;
        playerAttack.SetAttackBoxEnabled(false);

        momentumMech.OnLevelRestart();
        wallRunMech.OnLevelRestart();
        grappleMech.OnLevelRestart();
    }

    private void SetPersistentStats() {
        ChangeStartFocus(PlayerData.Data.Stats.StartFocus, false);
        ChangeMaxFocus(PlayerData.Data.Stats.MaxFocus, false, false);
        focusLossMult = PlayerData.Data.Stats.FocusLossMult;

        moveSpeed = PlayerData.Data.Stats.BaseMovementSpeed;
        jumpForce = PlayerData.Data.Stats.JumpForce;

        attackDamage = PlayerData.Data.Stats.AttackDamage;
    }

    #region Update
    private void FixedUpdate() {
        if (pendingRespawn) Respawn();
        if (pendingStand) {
            pendingStand = false;
            Slide(true);
        }
        momentumMech.UpdateMomentum(CurrentFocus, currentDir);
        if (grappleMech.UpdateGrapple(CurrentFocus > 0)) return;    // The player is grappling, don't update the rest.
        UpdatePhysics();
        wallRunMech.UpdateWallRun(currentDir);
        UpdateDirection();
        Move();
        UpdateLean();
        AudioHandler.Instance.SetContPlaySoundLoop(SoundType.Slide, IsSliding && IsGrounded && currentDir != Direction.None);
    }

    private void LateUpdate() => grappleMech.UpdateLockOnReticle(IsGrounded, cameraTransform);

    private void UpdateDirection() {
        if (moveVector == lastMoveVector) return;
        lastMoveVector = moveVector;
        if (moveVector == Vector2.up) {
            currentDir = Direction.Forward;
        } else if (moveVector == Vector2.down) {
            currentDir = Direction.Backward;
        } else if (moveVector == Vector2.left) {
            currentDir = Direction.Left;
        } else if (moveVector == Vector2.right) {
            currentDir = Direction.Right;
        } else if (moveVector == Vector2.zero) {
            currentDir = Direction.None;
        } else { currentDir = Direction.Other; }
    }

    private void UpdateLean() {
        if (!wallRunMech.IsOnWall()) {
            if (currentDir == Direction.Right) leanAmount = -4f;
            else if (currentDir == Direction.Left) leanAmount = 4f;
            else leanAmount = 0.0f;
        } else {
            if (wallRunMech.IsOnWall(direction: "right")) leanAmount = 6f;
            else leanAmount = -6f;
        }
        fstPersonCamera.Lens.Dutch = Mathf.Lerp(fstPersonCamera.Lens.Dutch, leanAmount, Time.fixedDeltaTime * 3);
    }

    private void UpdatePhysics() {
        IsGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // Find the ground normal when grounded
        if (IsGrounded) {
            // Small upward offset so the ray doesn't start inside the ground
            Vector3 origin = groundCheck.position + Vector3.up * 0.1f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1f, groundLayer)) {
                groundNormal = hit.normal;
            } else {
                groundNormal = Vector3.up;
            }
        } else {
            groundNormal = Vector3.up;
        }

        wallRunMech.UpdatePhysics(IsGrounded);
    }

    #endregion

    #region Actions
    private void Move() {
        float speed = moveSpeed * momentumMech.GetTrueMomentum();

        if (currentDir != Direction.None) {
            if (!IsSliding && (IsGrounded || IsWallRunning)) {
                AudioHandler.Instance.PlaySoundRND(SoundType.Footstep);
            }
        }

        // Camera-relative movement
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 moveDir = camForward * moveVector.y + camRight * moveVector.x;
        moveDir.Normalize();

        conveyorVelocity = Vector3.Lerp(conveyorVelocity, Vector3.zero, Time.fixedDeltaTime);

        Vector3 targetVelocity = moveDir * speed + conveyorVelocity;
        targetVelocity.y = rb.linearVelocity.y;

        // Bend velocity to follow ground when grounded & not moving upward
        if (IsGrounded && !IsWallRunning && targetVelocity.y <= 0f)
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, groundNormal);

        wallRunMech.UpdateWallJumpVelocity();

        if (wallRunMech.isWallJumping) {
            targetVelocity *= wallRunMech.InAirSpeedMultiplier();
            rb.linearVelocity = targetVelocity + wallRunMech.jumpVelocity;
        } else {
            wallRunMech.jumpVelocity = Vector3.zero;
            rb.linearVelocity = targetVelocity;
        }
    }

    public void Attack() {
        if (IsInAttack) return;
        AudioHandler.Instance.PlaySound(SoundType.SwordImpact);
        anim.SetTrigger("Attack");
    }

    private void Slide(bool keyReleased) {
        if (!CanStand) {
            pendingStand = keyReleased;
            return;
        }
        pendingStand = false;
        IsSliding = !keyReleased;

        anim.SetBool("Slide", IsSliding);
        focusLossMult = IsSliding ? focusLossMult + 1.5f : focusLossMult - 1.5f;
        AdjustHitboxHeight(IsSliding ? 2 : 1);
    }

    private void AdjustHitboxHeight(float divisor) {
        float hbSizeY = 0.9f / divisor;
        if (hitbox.height == hbSizeY) return;

        float hbCentY = (hbSizeY - 1f) / 2;
        hitbox.center = new Vector3(0f, hbCentY, 0f);
        hitbox.height = hbSizeY;
    }

    private void Jump(bool keyReleased) {
        wallRunMech.JumpKeyReleased(keyReleased);
        if (!keyReleased && (IsGrounded || (IsWallRunning && CurrentFocus > 0))) {
            AudioHandler.Instance.PlaySound(SoundType.Jump);
        }

        if (wallRunMech.IsOnWall() && !keyReleased) {
            TryAction(() => wallRunMech.Jump(keyReleased, cameraTransform), cost: -2.5f, !keyReleased);
            return;
        }

        if (!keyReleased && IsGrounded)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce * Mathf.Sqrt(momentumMech.GetTrueMomentum()), rb.linearVelocity.z);
        else if (keyReleased && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }

    private void Grapple() {
        if (!grappleMech.HasTarget()) return;
        TryAction(() => grappleMech.Grapple(IsGrounded, transform.position), cost: -5f, noFocusFlashCondition: !IsGrounded);
    }
    private void TryAction(Func<bool> action, float cost, bool noFocusFlashCondition) {
        if (CurrentFocus > 0f) {
            if (action()) ChangeFocus(cost);

        } else if (noFocusFlashCondition) {
            StatusUI.Instance.FlashFocusBar();
        }
    }

    public void Perform(Act action, Vector2 actionVector = default, bool keyReleased = default) {
        switch (action) {
            case Act.Attack:
                Attack();
                break;
            case Act.Slide:
                Slide(keyReleased);
                break;
            case Act.Jump:
                Jump(keyReleased);
                break;
            case Act.Move:
                moveVector = actionVector;
                break;
            case Act.Grapple:
                Grapple();
                break;
        }
    }

    #endregion


    /// <summary>
    /// Call to set the player to dead.
    /// Called at the first frame the player is hit.
    /// </summary>
    /// <param name="died">Default=true, kill the player</param>
    /// <param name="force">overrides isInvincible if true</param>
    public void SetDead(bool died = true, bool force = false) {
        if (isInvincible && !force) return;
        anim.SetBool("Died", died);
        if (died) GameStateHandler.GameOver();
    }

    /// <summary>
    /// Add/subtract to/from currentSpeed (focus AV)
    /// </summary>
    /// <param name="focus"></param>
    public void ChangeFocus(float focus, bool showUIIncrease = false) {
        currentFocus = Mathf.Clamp(currentFocus += focus, 0f, maxFocus);
        if (showUIIncrease) StatusUI.Instance.ShowFocusIncrease(focus);
    }

    /// <summary>Change the player's max focus.</summary>
    /// <param name="value">The value to change it to/by.</param>
    /// <param name="addToMax">True: Add the 'value' to the max. False: Set the value.</param>
    /// <param name="addToCurrent">True: Increase current focus. False: Don't change.</param>
    public void ChangeMaxFocus(float value, bool addToMax = true, bool addToCurrent = true) {
        if (addToMax) maxFocus += value;
        else maxFocus = value;
        if (addToCurrent) ChangeFocus(value);
        OnMaxFocusChanged?.Invoke(maxFocus);
        PlayerData.Data.Stats.MaxFocus = maxFocus;
        PlayerData.Data.WriteStats();
    }

    public void ChangeStartFocus(float value, bool addToCurrent = true) {
        if (addToCurrent) startFocus += value;
        else startFocus = value;
        PlayerData.Data.Stats.StartFocus = startFocus;
        PlayerData.Data.WriteStats();
    }

    public void ChangeMomentumCap(float value, bool addToCurrent = true) {
        PlayerData.Data.Stats.MomentumCap = (addToCurrent) ? PlayerData.Data.Stats.MomentumCap + value : value;
        PlayerData.Data.WriteStats();
    }

    /// <summary>PowerUp the player's focus stat multiplier.</summary>
    /// <param name="time">Seconds the buff lasts.</param>
    /// <param name="multiplier">The amount to multiply the gain by.</param>
    /// <param name="speedStatBoost">The amount to Add SpeedStat by.</param>
    public void SpeedBuff(float time, float multiplier, float speedStatBoost) {
        ChangeFocus(speedStatBoost);
        momentumMech.BuffSpeed(time, multiplier);
    }
    public void SetLinearVelocity(Vector3 target) => rb.linearVelocity = target;
    public void SetConveyorVelocity(Vector3 velocity) => conveyorVelocity = velocity;
    public bool IsWallJumping => wallRunMech.isWallJumping;
    public bool IsWallRunning => wallRunMech.isWallRunning;
    public bool IsGrappling => grappleMech.IsGrappling();
    public float GetJumpForce() => jumpForce;

    public void SetAttackBoxEnabled(bool enabled) => playerAttack.SetAttackBoxEnabled(enabled);
}