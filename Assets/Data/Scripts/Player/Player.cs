using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float speedStat = 0;
    [SerializeField] private float speedLossMult = 2f;
    [SerializeField] private float jumpForce = 7f;
    [Header("Refs")]
    [SerializeField] private PlayingInput playerInput;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CinemachineCamera fstPersonCamera;
    [Header("UI Refs")]
    [SerializeField] private TextMeshProUGUI speedMultDisplay;
    [SerializeField] private Slider speedBar;
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float wallCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    private Rigidbody rb;
    private Vector2 moveVector = Vector2.zero;
    private Vector2 lastMoveVector = Vector2.zero;

    private bool isSliding = false;
    private bool isGrounded = false;
    private bool wasGroundedLastFrame = false;
    private bool isWallRunning = false;
    private bool isOnSlope = false;
    private bool isAttacking = false;

    private float leanAmount = 0.0f;

    private float speedMult = 1f;

    public enum Act { Attack, Slide, Jump, Move }
    private enum Direction { Forward, Backward, Left, Right, Other, None }

    private Direction currentDir;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentDir = Direction.None;
    }

    private void FixedUpdate() {
        UpdatePhysics();
        UpdateDirection();
        UpdateSpeedMult();
        UpdateCamera();
        Move();
        wasGroundedLastFrame = isGrounded;
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

    private void UpdateCamera() {
        if (currentDir == Direction.Right) leanAmount = -4f;
        else if (currentDir == Direction.Left) leanAmount = 4f;
        else leanAmount = 0.0f;
        fstPersonCamera.Lens.Dutch = Mathf.Lerp(fstPersonCamera.Lens.Dutch, leanAmount, Time.deltaTime * 3);
    }

    private void UpdatePhysics() {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        isWallRunning = !isGrounded && Physics.CheckSphere(wallCheck.position, wallCheckRadius, wallLayer);
    }

    private void UpdateSpeedMult() {

        // This function still needs a lot of tweaking. For instance the player simply running cannot really gain speed, and speed mults aren't high enough
        bool hasSpeedStat = speedStat > 0f;
        float basis = 1f;
        float targetMult = 1f;
        // Always drain speed stat, this value is clamped.
        ChangeSpeedStat(-speedLossMult * Time.deltaTime);

        // Decided the basic movement speed
        switch (currentDir) {
            case Direction.Forward: basis = hasSpeedStat ? 1.2f : 1.0f; break;
            case Direction.Backward: basis = hasSpeedStat ? 0.8f : 0.6f; break;
            case Direction.Left:
            case Direction.Right: basis = hasSpeedStat ? 1.0f : 0.8f; break;
            default: break;
        }

        // Slope bonus
        if (isOnSlope) targetMult += 0.04f;

        // Sliding bonus
        if (hasSpeedStat && isGrounded && isSliding) {
            targetMult += 0.06f;
            if (!wasGroundedLastFrame) targetMult += 0.10f; // Landed while sliding
        }

        // If just landed (grounded this frame, but NOT last frame) and NOT sliding, reset to 1× basis
        if (hasSpeedStat && isGrounded && !wasGroundedLastFrame && !isSliding) {
            targetMult = 1f; // No sliding, lose speed
        }

        // Clamp to 0.5 if sliding with no speed stat
        if (!hasSpeedStat && isSliding) targetMult = Mathf.Min(targetMult, 0.5f);

        targetMult *= basis;

        // Only update speedMult when grounded or just landed (so landing penalty applies)
        
        if (isGrounded || (!wasGroundedLastFrame && isGrounded)) {
            speedMult = Mathf.Lerp(speedMult, targetMult, Time.deltaTime * 2f);
        }

        speedMultDisplay.text = "Speed Mult: " + speedMult;
        speedBar.value = Mathf.Lerp(speedBar.value, speedStat, Time.deltaTime);
    }

    private void Move() {
        float speed = moveSpeed * speedMult;

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

    private void Attack() {
        if (isAttacking) return;
        isAttacking = true;
        anim.SetTrigger("Attack");
    }

    private void Slide() {
        isSliding = !isSliding;
        anim.SetBool("Slide", isSliding);
        speedLossMult = isSliding ? speedLossMult + 1.5f : speedLossMult - 1.5f;
    }

    private void Jump(bool keyReleased) {
        if (!keyReleased && (isGrounded || isWallRunning)) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * speedMult, jumpForce, rb.linearVelocity.z);

        } else if (keyReleased && rb.linearVelocity.y > 0f) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
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
        }
    }

    /// <summary>
    /// Animation Event Handler - allow for an attack, the player can't attack if they are already attacking.
    /// </summary>
    public void AllowAttack() {
        isAttacking = false;
    }

    /// <summary>
    /// Add/subtract to/from currentSpeed (speed AV)
    /// </summary>
    /// <param name="speed"></param>
    public void ChangeSpeedStat(float speed) {
        speedStat = Mathf.Clamp(speedStat += speed, 0f, 100f);
    }

    /// <summary>
    /// Add/subtract to/from speed multiplier
    /// </summary>
    /// <param name="speed"></param>
    public void ChangeSpeedMult(float speed, float limit = 1.0f) {
        speedMult += speed;
        if (speedMult < limit) speedMult = limit;
    }

    public void SetOnSlope(bool onSlope) {
        isOnSlope = onSlope;
    }
    /*
    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(wallCheck.position, wallCheckRadius);
    }
    */
}
