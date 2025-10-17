using System;
using Unity.Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Player : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float speedStat = 0;
    [SerializeField] private float speedLossMult = 1f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private bool isInvincible = false;

    [Header("General Refs")]
    [SerializeField] private Transform cameraTransform;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private Animator anim;
    private CinemachineCamera fstPersonCamera;

    private GrappleMechanic grappleMech;
    private WallRunMechanic wallRunMech;
    private MomentumMechanic momentumMech;
    private PlayerAttack playerAttack;

    private Rigidbody rb;
    private Vector2 moveVector = Vector2.zero;
    private Vector2 lastMoveVector = Vector2.zero;

    private Vector3 conveyorVelocity = Vector3.zero;

    private bool isSliding = false;
    private bool isGrounded = false;
    private bool isOnSlope = false;

    private bool isInAttack = false;
    private bool slideAudioPlaying = false;

    private float leanAmount = 0.0f;

    public enum Act { Attack, Slide, Jump, Move, Grapple }
    public enum Direction { Forward, Backward, Left, Right, Other, None }

    private Direction currentDir;

    public static Player Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this)
            Debug.LogError("Duplicate Player Object");
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    private void Start() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        anim = GetComponentInChildren<Animator>();
        fstPersonCamera = GetComponentInChildren<CinemachineCamera>();
        momentumMech = GetComponent<MomentumMechanic>();
        grappleMech = GetComponent<GrappleMechanic>();
        wallRunMech = GetComponent<WallRunMechanic>();
        playerAttack = GetComponentInChildren<PlayerAttack>();
        wallRunMech.InitWallRunMechanic(this, rb);
        momentumMech.InitMomentumMechanic(this);
        grappleMech.InitGrappleMechanic(this);
        playerAttack.InitPlayerAttack(this);

        GetComponentInChildren<AnimationEventHandler>().InitAnimationEventHandler(this, playerAttack);
    }

    private void OnEnable() {
        if (playerAttack) playerAttack.HasAttacked(false);
        isInAttack = false;
        currentDir = Direction.None;
    }


    private void FixedUpdate() {
        playerAttack.UpdatePlayerAttack();
        momentumMech.UpdateMomentum(speedStat, currentDir);
        if (grappleMech.UpdateGrapple(speedStat > 0)) return;    // The player is grappling, don't update the rest.
        UpdatePhysics();
        wallRunMech.UpdateWallRun(currentDir);
        UpdateDirection();
        Move();
        UpdateLean();
        CheckPlaySlideAudio(isSliding && isGrounded);
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
        wallRunMech.UpdatePhysics(isGrounded);
    }

    private void Move() {
        float speed = moveSpeed * momentumMech.GetTrueSpeedMult();

        if (currentDir != Direction.None) {
            if (!isSliding && (isGrounded || IsWallRunning())) {
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
        wallRunMech.UpdateWallJumpVelocity();
        if (wallRunMech.IsWallJumping()) {
            targetVelocity *= wallRunMech.InAirSpeedMultiplier();
            rb.linearVelocity = targetVelocity + wallRunMech.GetJumpVelocity();
        } else {
            wallRunMech.SetJumpVelocity(Vector3.zero);
            rb.linearVelocity = targetVelocity;
        }
    }

    public void Attack() {
        if (isInAttack) return;
        AudioHandler.Instance.PlaySound(SoundType.SwordImpact);
        playerAttack.HasAttacked(true);
        isInAttack = true;
        anim.SetTrigger("Attack");
    }

    private void Slide() {
        isSliding = !isSliding;
        anim.SetBool("Slide", isSliding);
        speedLossMult = isSliding ? speedLossMult + 1.5f : speedLossMult - 1.5f;
    }

    private void CheckPlaySlideAudio(bool shouldPlaySlideAudio) {
        if (shouldPlaySlideAudio && !slideAudioPlaying) {
            slideAudioPlaying = true;
            if (currentDir != Direction.None)
                AudioHandler.Instance.SetPlaySoundLoop(SoundType.Slide, true);
        } else if (!shouldPlaySlideAudio && slideAudioPlaying) {
            slideAudioPlaying = false;
            AudioHandler.Instance.SetPlaySoundLoop(SoundType.Slide, false);
        }
    }

    private void Jump(bool keyReleased) {
        wallRunMech.JumpKeyReleased(keyReleased);
        if (!keyReleased && (isGrounded || (IsWallRunning() && speedStat > 0))) {
            AudioHandler.Instance.PlaySound(SoundType.Jump);
        }

        if (wallRunMech.IsOnWall()) {
            TryAction(() => wallRunMech.Jump(keyReleased, cameraTransform), -2.5f, wallRunMech.IsOnWall() && !keyReleased);
            return;
        }

        if (!keyReleased && isGrounded) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce * Mathf.Sqrt(momentumMech.GetTrueSpeedMult()), rb.linearVelocity.z);
        } else if (keyReleased && rb.linearVelocity.y > 0f) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }

    }

    private void Grapple() {
        TryAction(() => grappleMech.Grapple(isGrounded, transform.position), cost: -5f, failedCondition: (!isGrounded && !grappleMech.HasTarget()));
    }

    private void TryAction(Func<bool> action, float cost, bool failedCondition) {
        if (speedStat > 0f) {
            if (action()) {
                ChangeSpeedStat(cost);
            }
        } else if (failedCondition) {
            // TODO: call MomentumUI.ActionFailed
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
    /// Called at the last frame of the death animation.
    /// </summary>
    public void Died() {
        print("I don't think this is ever called");
        anim.SetBool("Died", false);
    }

    /// <summary>
    /// Called at the first frame the player is hit.
    /// </summary>
    public void Die() {
        if (isInvincible) return;
        anim.SetBool("Died", true);
    }

    /// <summary>
    /// Add/subtract to/from currentSpeed (speed AV)
    /// </summary>
    /// <param name="speed"></param>
    public void ChangeSpeedStat(float speed) {
        speedStat = Mathf.Clamp(speedStat += speed, 0f, 100f);
    }

    public void Buff(int time, float multiplier) {
        momentumMech.BuffSpeed(time, multiplier);
    }

    public void SetLinearVelocity(Vector3 target) { rb.linearVelocity = target; }
    public void SetConveyorVelocity(Vector3 velocity) { conveyorVelocity = velocity; }
    public void SetOnSlope(bool onSlope) { isOnSlope = onSlope; }
    public bool IsOnSlope() { return isOnSlope; }
    public bool IsWallJumping() { return wallRunMech.IsWallJumping(); }
    public bool IsSliding() { return isSliding; }
    public bool IsGrounded() { return isGrounded; }
    public bool IsGrappling() { return grappleMech.IsGrappling(); }
    public bool IsWallRunning() { return wallRunMech.IsWallRunning(); }
    public float GetJumpForce() { return jumpForce; }
    public void ResetIsInAttack() {
        isInAttack = false;
        ToggleAttackCollider(false);
    }
    public void ForceHitEnemy(EnemyKnight enemy) { playerAttack.ForceHit(enemy); }
    public void ToggleAttackCollider(bool toggle) { playerAttack.ToggleAttackCollider(toggle); }

    public Transform GetTransform() { return transform; }

}