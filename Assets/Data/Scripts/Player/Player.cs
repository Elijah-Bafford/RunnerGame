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

    private GrappleMechanic grappleMech;
    private PlayerAttack playerAttack;

    private Rigidbody rb;
    private Vector2 moveVector = Vector2.zero;
    private Vector2 lastMoveVector = Vector2.zero;

    private bool isSliding = false;
    private bool isGrounded = false;
    private bool wasGroundedLastFrame = false;
    private bool isWallRunning = false;
    private bool isOnSlope = false;

    // Attacking variables
    private bool isInAttack = false;

    private float leanAmount = 0.0f;

    private float speedMult = 1f;

    public enum Act { Attack, Slide, Jump, Move, Grapple }
    private enum Direction { Forward, Backward, Left, Right, Other, None }

    private Direction currentDir;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        grappleMech = GetComponent<GrappleMechanic>();
        playerAttack = GetComponentInChildren<PlayerAttack>();
        print(playerAttack.name);
    }

    private void OnEnable() {
        playerAttack.HasAttacked(false);
        isInAttack = false;
        speedBar.value = speedStat;
        rb.freezeRotation = true;
        currentDir = Direction.None;
    }


    private void FixedUpdate() {
        UpdateSpeedMult();
        if (UpdateGrapple()) return;
        UpdatePhysics();
        UpdateDirection();
        UpdateCamera();
        Move();
    }

    private bool UpdateGrapple() {
        if (grappleMech.IsGrappling()) {
            speedMult += 0.1f * Time.deltaTime;
            Vector3 toTarget = grappleMech.GetGrappleTarget() - transform.position;
            float dist = toTarget.magnitude;

            // Move directly toward the enemy
            rb.linearVelocity = grappleMech.GetGrappleDirection() * grappleMech.GetGrappleSpeed();

            // Stop when close enough
            if (dist < grappleMech.GetGrappleArrivalDistance()) {
                rb.linearVelocity = Vector3.zero;
                grappleMech.SetIsGrappling(false);
                Attack();
            }
            return true;
        }
        return false;
    }

    private void LateUpdate() {
        grappleMech.UpdateLockOnReticle(isGrounded, cameraTransform);
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
        // Always drain speed stat, this value is clamped.
        ChangeSpeedStat(-speedLossMult * Time.deltaTime);

        /* How it works:
         * 
         * Directions have a basis, forward is faster than left and right, and backwards is slower than them.
         * 
         * Basis with no speedMult: Forward: 1.0, Left/Right 0.8, Backwards 0.6, sliding 0.5
         * Basis with speedMult: Forward 1.2, Left/Right 1.0, Backward 0.8, sliding 1.4 (sliding requires speedStat so it's much higher)
         * 
         * 
         * Certain actions will require speedStat: sliding, wall running, and grappling
         * When the player changes direction, speedMult will be reduced.
         * 
         * I think having a continous "fight" between losing and gaining speedMult would make this work.
         * By trying to bring the speedMult continuously down to the basis, and stopping it from reaching 2x (although possible to momentarily bring it above that)
         * 
         * 
         * Without Speed Stat:
         * The player will begin to slowly lose their momentum, all the way down to the basis
         * 
         * With Speed Stat:
         * When Sliding, grappling, and on slopes the player will gain speedMult
         * 
         * When the player jumps, if they land while sliding they will maintain their momentum otherwise they will lose most of it
         */

        bool hasSpeedStat = speedStat > 0f;
        float basis = 1f;
        float targetMult = basis;

        switch (currentDir) {
            case Direction.Forward: basis = hasSpeedStat ? 1.2f : 1.0f; break;
            case Direction.Left:
            case Direction.Right: basis = hasSpeedStat ? 1.0f : 0.8f; break;
            case Direction.Backward: basis = hasSpeedStat ? 0.8f : 0.6f; break;
            default: basis = 1.0f; break;
        }
        if (isSliding) basis = hasSpeedStat ? 1.4f : 0.5f;

        if (hasSpeedStat) {
            if (isSliding && isGrounded) targetMult += 0.2f;
            if (isOnSlope) targetMult += 0.23f;
            if (isGrounded && !wasGroundedLastFrame && isSliding) targetMult += 0.23f;

            targetMult = Mathf.Clamp(targetMult, basis, 2.5f);
            speedMult = Mathf.Lerp(speedMult, targetMult, 8f * Time.deltaTime);
        } else {
            speedMult = Mathf.Lerp(speedMult, basis, 0.5f * Time.deltaTime);
        }

        if ((currentDir == Direction.None) || (isGrounded && !wasGroundedLastFrame && !isSliding)) {
            speedMult = basis;
        }

        speedMultDisplay.text = "Speed Mult: " + speedMult.ToString("F3");
        speedBar.value = Mathf.Lerp(speedBar.value, speedStat, Time.deltaTime * 4);
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
        if (!keyReleased && (isGrounded || isWallRunning)) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * speedMult, jumpForce, rb.linearVelocity.z);

        } else if (keyReleased && rb.linearVelocity.y > 0f) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }
    }

    private void Grapple() {
        grappleMech.Grapple(isGrounded, transform.position);
        ChangeSpeedStat(-5);
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
    public void IsInAttack() {
        isInAttack = false;
    }

    /// <summary>
    /// Called at the last frame of the death animation.
    /// </summary>
    public void Died() { anim.SetBool("Died", false); }

    public void Die() { anim.SetBool("Died", true); }

    /// <summary>
    /// Add/subtract to/from currentSpeed (speed AV)
    /// </summary>
    /// <param name="speed"></param>
    public void ChangeSpeedStat(float speed) { speedStat = Mathf.Clamp(speedStat += speed, 0f, 100f); }

    /// <summary>
    /// Add/subtract to/from speed multiplier
    /// </summary>
    /// <param name="speed"></param>
    public void ChangeSpeedMult(float speed, float limit = 1.0f) {
        speedMult += speed;
        if (speedMult < limit) speedMult = limit;
    }

    public void SetOnSlope(bool onSlope) { isOnSlope = onSlope; }

    public bool PlayerIsGrappling() { return grappleMech.IsGrappling(); }
}
