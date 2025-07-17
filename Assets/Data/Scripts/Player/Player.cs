using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float speedMult = 1f;
    [SerializeField] private float currentSpeed = 0;
    [SerializeField] private float jumpForce = 7f;
    [Header("Refs")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator anim;
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private Vector2 moveVector = Vector2.zero;

    private bool isSliding = false;
    private bool isGrounded = false;
    private bool isAttacking = false;

    public enum Act { Attack, Slide, Jump, Move }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        UpdateSpeedMult();
        Move();
    }

    private void UpdateSpeedMult() {
        if (currentSpeed > 0) {
            speedMult = 1.1f;
            if (isSliding && isGrounded) {
                speedMult = 1.6f;
            }
        } else {
            speedMult = 0.8f;
            if (isSliding && isGrounded) {
                speedMult = 0.65f;
            }
        }
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
