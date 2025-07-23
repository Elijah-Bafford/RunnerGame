using System;
using TMPro;
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
    [SerializeField] private PlayingInput playerInput;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CinemachineCamera fstPersonCamera;

    [Header("UI Refs")]
    [SerializeField] private TextMeshProUGUI speedMultDisplay;
    [SerializeField] private Slider speedBar;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Wall Check and Wall climb Mechanics")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.2f;

    [Header("Lock-on/Grapple Mechanics")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform lockOnReticle;
    [SerializeField] private float detectRange = 20f;
    [SerializeField] private float grappleRange = 10f;
    [SerializeField] private float lockOnRadius = 0.5f;
    [SerializeField] private float grappleSpeed = 30f;
    private Enemy lockOnTarget = null;
    private Enemy lastLockTarget = null;
    private RawImage reticle;
    private Vector3 lockOnOffset = new Vector3(0, 0.2f, 0);
    private bool inGrappleRange = false;
    private bool isGrappling = false;
    private Vector3 grappleDirection;
    private Vector3 grappleTarget;
    private float grappleArrivalDistance = 1.0f;

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

    public enum Act { Attack, Slide, Jump, Move, Grapple }
    private enum Direction { Forward, Backward, Left, Right, Other, None }

    private Direction currentDir;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        reticle = lockOnReticle.GetComponent<RawImage>();
        rb.freezeRotation = true;
        currentDir = Direction.None;
    }

    private void FixedUpdate() {
        if (UpdateGrapple()) return;
        UpdatePhysics();
        UpdateDirection();
        UpdateSpeedMult();
        UpdateCamera();
        Move();
    }

    private bool UpdateGrapple() {
        if (isGrappling) {
            speedMult += 0.1f * Time.deltaTime;
            Vector3 toTarget = grappleTarget - transform.position;
            float dist = toTarget.magnitude;

            // Move directly toward the enemy
            rb.linearVelocity = grappleDirection * grappleSpeed;

            // Stop when close enough
            if (dist < grappleArrivalDistance) {
                rb.linearVelocity = Vector3.zero;
                isGrappling = false;
                Attack();
            }
            return true;
        }
        return false;
    }

    private void LateUpdate() {
        UpdateLockOnReticle();
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

    private void UpdateLockOnReticle() {
        // Always hide when grounded
        if (isGrounded) {
            lockOnTarget = null;
            lastLockTarget = null;
            lockOnReticle.gameObject.SetActive(false);
            return;
        }
        // See if there is a target
        lockOnTarget = null;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.SphereCast(ray, lockOnRadius, out RaycastHit hit, detectRange)) {
            lockOnTarget = hit.collider.GetComponentInParent<Enemy>();
        }

        // Decide the target to show the reticle on
        if (lockOnTarget != null) {
            lastLockTarget = lockOnTarget;   // keep it
        } else if (lastLockTarget != null) {
            // Keep showing the old one only while still on screen & inside detectRange
            bool stillVisible = Vector3.Distance(transform.position, lastLockTarget.transform.position) <= detectRange;
            if (!stillVisible) lastLockTarget = null;
        }

        // Update UI
        if (lastLockTarget == null) {
            lockOnReticle.gameObject.SetActive(false);
            return;
        }

        Vector3 targetWorld = lastLockTarget.transform.position + lockOnOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetWorld);

        if (screenPos.z <= 0f) {
            lockOnReticle.gameObject.SetActive(false);
            return;
        }

        lockOnReticle.gameObject.SetActive(true);
        lockOnReticle.position = screenPos;

        inGrappleRange = Vector3.Distance(transform.position, lastLockTarget.transform.position) <= grappleRange;

        reticle.color = inGrappleRange ? Color.red : Color.white;
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
        if (isOnSlope) targetMult += 0.2f;

        // Sliding bonus
        if (hasSpeedStat && isGrounded && isSliding) {
            targetMult += 0.12f;
            if (!wasGroundedLastFrame) targetMult += 0.15f; // Landed while sliding
        }

        // If just landed (grounded this frame, but NOT last frame) and NOT sliding, reset to 1× basis
        if (hasSpeedStat && isGrounded && !wasGroundedLastFrame && !isSliding) {
            targetMult = 1.2f; // No sliding, lose speed
        }

        // Clamp to 0.5 if sliding with no speed stat
        if (!hasSpeedStat && isSliding) targetMult = Mathf.Min(targetMult, 0.7f);

        targetMult *= basis;

        // Only update speedMult when grounded or just landed (so landing penalty applies)

        if (isGrounded || (!wasGroundedLastFrame && isGrounded)) {
            speedMult = Mathf.Lerp(speedMult, targetMult, Time.deltaTime);
        } else {
            speedMult += 0.05f * Time.deltaTime;
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

    private void Grapple() {
        if (isGrounded || !inGrappleRange || lastLockTarget == null) return;

        // Set up the grapple state
        isGrappling = true;
        grappleTarget = lastLockTarget.transform.position + lockOnOffset;
        grappleDirection = (grappleTarget - transform.position).normalized;

        rb.linearVelocity = Vector3.zero;
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
