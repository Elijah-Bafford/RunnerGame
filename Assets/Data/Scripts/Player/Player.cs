using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-2)]
public class Player : MonoBehaviour {

    [Header("Player Stats")]

    [SerializeField] private float attackDamage;

    [Header("- Focus Stat")]
    [SerializeField] private float maxFocus = 100f;
    [SerializeField] private float startFocus = 0;
    [SerializeField] private float currentFocus = 0;
    [SerializeField] private float focusLossMult = 2f;

    [Header("- Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;

    [SerializeField] private bool isInvincible = false;

    [Header("General Refs")]
    [SerializeField] private Transform _TargetingPos;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform spawnPoint;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    public float AttackDamage => attackDamage;
    public float MaxSpeedStat => maxFocus;
    public float CurrentSpeedStat => currentFocus;
    public Vector3 TargetingPos => _TargetingPos.position;

    private Rigidbody rb;
    private BoxCollider hitbox;
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

    public bool isSliding { get; set; } = false;
    public bool isGrounded { get; set; } = false;
    /// <summary>The angle of the slope. 0 if not on a slope (Abs value)</summary>
    public float OnSlopeAngle { get; set; } = 0f;
    /// <summary>The Y position of the player on the slope. Used to check if the player is going up or down a slope</summary>
    public float OnSlopeY { get; set; } = 0f;

    ///<summary>Whether or not the player is in the attack animation.</summary>
    public bool IsInAttack { get; set; } = false;

    ///<summary>Whether a restart is pending (execute on first FixedUpdate after true).</summary>
    private bool pendingRespawn = false;

    private float leanAmount = 0.0f;

    private Vector3 groundNormal = Vector3.up;

    /// <summary>Actions available to the player</summary>
    public enum Act { Attack, Slide, Jump, Move, Grapple }
    /// <summary>Directions of the player. Other is a combination of movements, none is not moving.</summary>
    public enum Direction { Forward, Backward, Left, Right, Other, None }
    /// <summary>The current direction the player is moving in.</summary>
    public Direction currentDir { get; private set; } = Direction.None;
    public static Player Instance { get; private set; }

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
        hitbox = GetComponent<BoxCollider>();
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
        currentFocus = startFocus;

        GameStateHandler.OnLevelRestart += OnLevelRestart;
    }

    private void OnLevelRestart() => pendingRespawn = true;

    private void Respawn() {
        pendingRespawn = false;
        SetDead(false, force: true);
        currentDir = Direction.None;

        leanAmount = 0;
        currentFocus = startFocus;

        moveVector = Vector3.zero;
        lastMoveVector = Vector3.zero;

        cameraPanTilt.PanAxis.Value = 0;
        cameraPanTilt.TiltAxis.Value = 0;

        if (spawnPoint != null) {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        IsInAttack = false;
        playerAttack.SetAttackBoxEnabled(false);

        momentumMech.SetDefaultValues();
        wallRunMech.OnLevelRestart();
    }

    private void FixedUpdate() {
        if (pendingRespawn) Respawn();

        momentumMech.UpdateMomentum(CurrentSpeedStat, currentDir);
        if (grappleMech.UpdateGrapple(CurrentSpeedStat > 0)) return;    // The player is grappling, don't update the rest.
        UpdatePhysics();
        wallRunMech.UpdateWallRun(currentDir);
        UpdateDirection();
        Move();
        UpdateLean();
        AudioHandler.Instance.SetContPlaySoundLoop(SoundType.Slide, isSliding && isGrounded && currentDir != Direction.None);
    }

    private void LateUpdate() {
        grappleMech.UpdateLockOnReticle(isGrounded, cameraTransform);
    }

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
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // Find the ground normal when grounded
        if (isGrounded) {
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

        wallRunMech.UpdatePhysics(isGrounded);
    }

    private void Move() {
        float speed = moveSpeed * momentumMech.GetTrueMomentum();

        if (currentDir != Direction.None) {
            if (!isSliding && (isGrounded || IsWallRunning)) {
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
        if (isGrounded && !IsWallRunning && targetVelocity.y <= 0f)
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

    private void Slide() {
        isSliding = !isSliding;
        anim.SetBool("Slide", isSliding);
        focusLossMult = isSliding ? focusLossMult + 1.5f : focusLossMult - 1.5f;
        AdjustHitboxHeight(isSliding ? 2 : 1);
    }

    private void AdjustHitboxHeight(float divisor) {
        float hbSizeY = 0.9f / divisor;
        if (hitbox.size.y == hbSizeY) return;

        float hbCentY = (hbSizeY - 1f) / 2;
        hitbox.center = new Vector3(0f, hbCentY, 0f);
        hitbox.size = new Vector3(0.8f, hbSizeY, 0.8f);
    }

    private void Jump(bool keyReleased) {
        wallRunMech.JumpKeyReleased(keyReleased);
        if (!keyReleased && (isGrounded || (IsWallRunning && CurrentSpeedStat > 0))) {
            AudioHandler.Instance.PlaySound(SoundType.Jump);
        }

        if (wallRunMech.IsOnWall()) {
            TryAction(() => wallRunMech.Jump(keyReleased, cameraTransform), -2.5f, !keyReleased);
            return;
        }

        if (!keyReleased && isGrounded)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce * Mathf.Sqrt(momentumMech.GetTrueMomentum()), rb.linearVelocity.z);
        else if (keyReleased && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }

    private void Grapple() {
        if (grappleMech.HasTarget())
            TryAction(() => grappleMech.Grapple(isGrounded, transform.position), cost: -5f, noFocusFlashCondition: !isGrounded);
    }
    private void TryAction(Func<bool> action, float cost, bool noFocusFlashCondition) {
        if (CurrentSpeedStat > 0f) {
            if (action()) ChangeFocus(cost);

        } else if (noFocusFlashCondition) {
            StatusUI.Instance.ActionFailed();
        }
    }

    public void Perform(Act action, Vector2 actionVector = default, bool keyReleased = default) {
        switch (action) {
            case Act.Attack:
                Attack();
                break;
            case Act.Slide:
                Slide();
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
        currentFocus = Mathf.Clamp(currentFocus += focus, 0f, MaxSpeedStat);
        if (showUIIncrease) StatusUI.Instance.ShowFocusIncrease(focus);
    }

    /// <summary>PowerUp the player's focus stat multiplier.</summary>
    /// <param name="time">Seconds the buff lasts.</param>
    /// <param name="multiplier">The amount to multiply the gain by.</param>
    /// <param name="speedStatBoost">The amount to increase SpeedStat by.</param>
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