using UnityEngine;

public class WallRunMechanic : MonoBehaviour {

    [Header("Wall Check and Wall climb Mechanics")]
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.2f;
    [SerializeField] private float wallGravity = 1.5f;
    [SerializeField] private float wallGravityMax = 4f;
    [SerializeField] private float wallGravityIncrease = 0.2f; // How fast gravity ramps up
    [SerializeField] private Vector3 wallJumpForce;

    public Vector3 jumpVelocity { get; set; } = Vector3.zero;
    public bool isWallRunning { get; private set; } = false;
    public bool isWallJumping { get; private set; } = false;

    private Rigidbody rb;
    private Player player;

    private float movingDivisor = 1.0f;
    private float currentWallGravity;
    private bool jumpKeyReleased = false;
    private bool allowWallRun = false;
    private bool isOnWallLeft = false;
    private bool isOnWallRight = false;
    private bool wasOnWallLastFrame = false;

    public void OnLevelRestart() {
        movingDivisor = 1.0f;
        currentWallGravity = wallGravity;
        jumpVelocity = Vector3.zero;
        jumpKeyReleased = false;
        allowWallRun = false;
        isOnWallLeft = false;
        isOnWallRight = false;
        isWallRunning = false;
        isWallJumping = false;
        wasOnWallLastFrame = false;
    }

    public void InitWallRunMechanic(Player player, Rigidbody rb) {
        this.player = player;
        this.rb = rb;
        currentWallGravity = wallGravity;
    }

    /// <summary>
    /// Called in Player.UpdatePhysics() (FixedUpdate)
    /// </summary>
    /// <param name="isGrounded"></param>
    public void UpdatePhysics(bool isGrounded) {
        wasOnWallLastFrame = IsOnWall();
        isOnWallLeft = !isGrounded && Physics.CheckSphere(wallCheckLeft.position, wallCheckRadius, wallLayer);
        isOnWallRight = !isGrounded && Physics.CheckSphere(wallCheckRight.position, wallCheckRadius, wallLayer);
    }

    /// <summary>
    /// Called in fixed update in the player class.
    /// When wall running, slowly increase gravity over time. 
    /// </summary>
    public void UpdateWallRun(Player.Direction currentDir) {
        // If NOT on a wall OR is grounded THEN return
        allowWallRun = jumpKeyReleased || rb.linearVelocity.y < 0f;
        if (!IsOnWall() || player.isGrounded) {
            jumpKeyReleased = false;
            currentWallGravity = wallGravity;
            isWallRunning = false;
            return;
        }
        if (allowWallRun) {
            movingDivisor = 1f;
            if (currentDir != Player.Direction.None) {
                movingDivisor = 3f;
            }
            currentWallGravity = Mathf.Min(currentWallGravity + wallGravityIncrease * Time.fixedDeltaTime, wallGravityMax);
            var velocity = rb.linearVelocity;
            velocity.y = -currentWallGravity / movingDivisor; // force downwards at the current wall gravity
            rb.linearVelocity = velocity;
            isWallRunning = true;
        }
    }

    public void UpdateWallJumpVelocity() {
        if (player.IsGrappling) { isWallJumping = false; return; }
        /*
         * If the player is grounded
         * or was NOT on a wall last frame and is this frame
         * isWallJumping = false
         */

        if (player.isGrounded || (!wasOnWallLastFrame && IsOnWall())) {
            isWallJumping = false;
        }

        float x = Mathf.Lerp(jumpVelocity.x, 0, Time.deltaTime);
        float y = Mathf.Lerp(jumpVelocity.y, -3, Time.deltaTime * 1.4f);
        float z = Mathf.Lerp(jumpVelocity.z, 0, Time.deltaTime * 2f);

        if (y < -2f) isWallJumping = false;

        jumpVelocity = new Vector3(x, y, z);
    }

    /// <summary>
    /// Called on jump context if the player is on a wall, instead of the normal player jump.
    /// Returns true if the player should do a wall jump, i.e. isOnWall = true
    /// </summary>
    /// <param name="keyReleased"></param>
    /// <returns></returns>
    public bool Jump(bool keyReleased, Transform cameraTransform) {
        if (!IsOnWall() || player.isGrounded) return false;
        if (keyReleased) return true;   // Disallow double execution and jump cut logic

        isWallJumping = true;
        // Get normalized camera forward, flattened on the XZ plane
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 awayFromWall = Vector3.zero;
        if (isOnWallLeft)
            awayFromWall = cameraTransform.right;
        else if (isOnWallRight)
            awayFromWall = -cameraTransform.right;

        // Get jump force from Player
        float jumpForce = player.GetJumpForce();


        jumpVelocity = (forward * jumpForce * wallJumpForce.x) + (Vector3.up * jumpForce * wallJumpForce.y) + (awayFromWall * jumpForce * wallJumpForce.z);


        // Apply the new velocity (Moved to player update)
        //rb.linearVelocity += jumpVelocity;

        // Reset wall-running state
        currentWallGravity = wallGravity;

        isWallRunning = false;
        return true;
    }

    /// <summary>
    /// Get if the player is on a wall.
    /// Use "direction" to get a specific direction, case doesn't matter.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public bool IsOnWall(string direction = "") {
        string dir = direction.ToLower();
        if (dir == "left") return isOnWallLeft;
        else if (dir == "right") return isOnWallRight;
        return isOnWallLeft || isOnWallRight;
    }

    //private void OnDrawGizmos() {
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(wallCheckLeft.position, wallCheckRadius);
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawSphere(wallCheckRight.position, wallCheckRadius);
    //}

    public void JumpKeyReleased(bool jumpKeyReleased) => this.jumpKeyReleased = jumpKeyReleased;
    public float InAirSpeedMultiplier() => isWallJumping ? 0.5f : 1f; 
}
