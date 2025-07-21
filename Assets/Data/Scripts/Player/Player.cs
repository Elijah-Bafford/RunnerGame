using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float currentSpeed = 0;
    [SerializeField] private float jumpForce = 7f;
    [Header("Refs")]
    [SerializeField] private PlayingInput playerInput;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator anim;
    [SerializeField] private CinemachineCamera fstPersonCamera;
    [SerializeField] private Slider speedBar;
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private Vector2 moveVector = Vector2.zero;
    private Vector2 lastMoveVector = Vector2.zero;

    private bool isSliding = false;
    private bool isGrounded = false;
    private bool isAttacking = false;

    private float leanAmount = 0.0f;
    private float speedLossMult = 1f;
    private float speedMult = 1f;

    public enum Act { Attack, Slide, Jump, Move }
    private enum Direction { Forward, Backward, Left, Right, None }

    private Direction currentDir;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentDir = Direction.None;
    }

    private void FixedUpdate() {
        UpdateDirection();
        UpdateCamera();
        UpdateSpeedMult();
        Move();
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
        } else { currentDir = Direction.None; }
    }

    private void UpdateCamera() {
        if (currentDir == Direction.Right) leanAmount = -4f;
        else if (currentDir == Direction.Left) leanAmount = 4f;
        else leanAmount = 0.0f;
        fstPersonCamera.Lens.Dutch = Mathf.Lerp(fstPersonCamera.Lens.Dutch, leanAmount, Time.deltaTime * 3);
    }

    private void UpdateSpeedMult() {

        
        if (currentDir == Direction.Right || currentDir == Direction.Left) {
            speedMult *= 0.8f;
        } else if (currentDir == Direction.Backward) {
            speedMult *= 0.6f;
        }

        if (currentSpeed > 0) {
            currentSpeed -= Time.deltaTime * speedLossMult;
            speedMult = 1.1f;
            if (isSliding && isGrounded) {
                speedMult = 1.6f;
            }
        } else {
            currentSpeed = 0.0f;
            speedMult = 0.8f;
            if (isSliding && isGrounded) {
                speedMult = 0.65f;
            }
        }

        speedBar.value = Mathf.Lerp(speedBar.value, currentSpeed, Time.deltaTime * 4);
    }

    private void Move() {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        float speed = moveSpeed;
        speed *= speedMult;

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
        // Logic for attacking
    }

    private void Slide() {
        isSliding = !isSliding;
        anim.SetBool("Slide", isSliding);
        speedLossMult = isSliding ? speedLossMult + 1.5f : speedLossMult;
    }

    private void Jump(bool keyReleased) {
        if (!keyReleased && isGrounded) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
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

    public void ChangeSpeed(float speed) {
        this.currentSpeed = Mathf.Clamp(currentSpeed += speed, 0f, 100f);
    }
}
