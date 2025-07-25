using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float speedStat = 0;
    [SerializeField] private float speedLossMult = 1f;
    [SerializeField] private float jumpForce = 7f;

    [Header("General Refs")]
    [SerializeField] private Animator anim;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CinemachineCamera fstPersonCamera;
    [SerializeField] private Slider speedBar;
    private Animator speedBarAnimator;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Wall Check and Wall climb Mechanics")]
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.2f;

    private GrappleMechanic grappleMech;
    private MomentumMechanic momentumMech;
    private PlayerAttack playerAttack;

    private Rigidbody rb;
    private Vector2 moveVector = Vector2.zero;
    private Vector2 lastMoveVector = Vector2.zero;

    private bool isSliding = false;
    private bool isGrounded = false;
    private bool isOnWallLeft = false;
    private bool isOnWallRight = false;
    private bool isOnSlope = false;

    // Attacking variables
    private bool isInAttack = false;

    private float leanAmount = 0.0f;

    public enum Act { Attack, Slide, Jump, Move, Grapple }
    public enum Direction { Forward, Backward, Left, Right, Other, None }

    private Direction currentDir;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        speedBarAnimator = speedBar.GetComponent<Animator>();
        grappleMech = GetComponent<GrappleMechanic>();
        momentumMech = GetComponent<MomentumMechanic>();
        playerAttack = GetComponentInChildren<PlayerAttack>();
        momentumMech.SetPlayerRef(this);
    }

    private void OnEnable() {
        if (playerAttack) playerAttack.HasAttacked(false);
        isInAttack = false;
        rb.freezeRotation = true;
        currentDir = Direction.None;
    }


    private void FixedUpdate() {
        momentumMech.UpdateMomentum(speedStat, currentDir);
        if (grappleMech.UpdateGrapple(speedStat > 0)) return;    // The player is grappling, don't update the rest.
        UpdatePhysics();
        UpdateDirection();
        Move();
        UpdateLean();
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
        if (!isOnWallLeft && !isOnWallRight) {
            if (currentDir == Direction.Right) leanAmount = -4f;
            else if (currentDir == Direction.Left) leanAmount = 4f;
            else leanAmount = 0.0f;
        } else {
            if (isOnWallRight) leanAmount = 6f;
            else leanAmount = -6f;
        }
        fstPersonCamera.Lens.Dutch = Mathf.Lerp(fstPersonCamera.Lens.Dutch, leanAmount, Time.deltaTime * 3);
    }

    private void UpdatePhysics() {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        isOnWallLeft = !isGrounded && Physics.CheckSphere(wallCheckLeft.position, wallCheckRadius, wallLayer);
        isOnWallRight = !isGrounded && Physics.CheckSphere(wallCheckRight.position, wallCheckRadius, wallLayer);
    }

    private void OnDrawGizmos() {
        /*
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(wallCheckLeft.position, wallCheckRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(wallCheckRight.position, wallCheckRadius);
        */
    }

    private void Move() {
        float speed = moveSpeed * momentumMech.GetSpeedMult();

        // Camera-relative movement
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 moveDir = camForward * moveVector.y + camRight * moveVector.x;
        moveDir.Normalize();

        Vector3 targetVelocity = moveDir * speed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    public void Attack() {
        if (isInAttack) return;
        playerAttack.HasAttacked(true);
        isInAttack = true;
        anim.SetTrigger("Attack");
    }

    private void Slide() {
        isSliding = !isSliding;
        anim.SetBool("Slide", isSliding);
        speedLossMult = isSliding ? speedLossMult + 1.5f : speedLossMult - 1.5f;
    }

    private void Jump(bool keyReleased) {
        if (!keyReleased && (isGrounded || (isOnWallLeft || isOnWallRight))) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

        } else if (keyReleased && rb.linearVelocity.y > 0f) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }
    }

    private void Grapple() {
        if (!TryAction(-5)) return;
        grappleMech.Grapple(isGrounded, transform.position);
        SetLinearVelocity(Vector3.zero);
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
    public void Died() { anim.SetBool("Died", false); }

    /// <summary>
    /// Called at the first frame the player is hit.
    /// </summary>
    public void Die() { anim.SetBool("Died", true); }

    /// <summary>
    /// Add/subtract to/from currentSpeed (speed AV)
    /// </summary>
    /// <param name="speed"></param>
    public void ChangeSpeedStat(float speed) {
        speedStat = Mathf.Clamp(speedStat += speed, 0f, 100f);
    }

    /// <summary>
    /// Decide if the player can perform an action that costs SpeedStat.
    /// Return true if can perform action, return false and flash SpeedBar otherwise.
    /// </summary>
    /// <param name="cost"></param>
    /// <returns></returns>
    public bool TryAction(float cost) {
        if (speedStat > 0f) {
            ChangeSpeedStat(cost);
        } else {
            speedBarAnimator.SetTrigger("Flash");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Update the speed bar. Lerp the value.
    /// </summary>
    /// <param name="value"></param>
    public void UpdateSpeedBar(float value) { speedBar.value = Mathf.Lerp(speedBar.value, value, Time.deltaTime * 4); }
    public void SetLinearVelocity(Vector3 target) { rb.linearVelocity = target; }
    public void SetOnSlope(bool onSlope) { isOnSlope = onSlope; }
    public void ResetIsInAttack() { isInAttack = false; }
    public bool IsOnSlope() { return isOnSlope; }
    public bool IsSliding() { return isSliding; }
    public bool IsGrounded() { return isGrounded; }
    public bool IsGrappling() { return grappleMech.IsGrappling(); }
}
