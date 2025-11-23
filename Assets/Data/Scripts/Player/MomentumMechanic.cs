using System.Collections;
using UnityEngine;

public class MomentumMechanic : MonoBehaviour {
    [Header("Momentum Settings")]
    [Tooltip("How Fast the player loses SpeedStat")]
    [SerializeField] private float speedLossMult = 2f;
    [Tooltip("Multiply deltaTime by this value.")]
    [SerializeField] private float timeScale = 1f;
    private Player player;

    private float speedBuffMultiplier = 1f;
    private float speedMult = 1.0f;
    private float speedBasis = 1.0f;
    private float highestSpeed = 0f;

    private bool wasGroundedLastFrame = false;

    private bool isSliding = false;
    private bool isGrounded = false;
    private bool isGrappling = false;
    private bool justGrappled = false;
    private bool justWallRan = false;
    private bool isWallRunning = false;
    private bool isWallJumping = false;

    private float onSlopeAngle = 0f;

    private MomentumUI momentumUI;

    public void OnLevelRestart() {
        speedBuffMultiplier = 1f;
        speedMult = 1.0f;
        speedBasis = 1.0f;
        highestSpeed = 0f;
        wasGroundedLastFrame = false;
        isSliding = false;
        isGrounded = false;
        isGrappling = false;
        justGrappled = false;
        justWallRan = false;
        isWallRunning = false;
        isWallJumping = false;

    }

    private void Start() {
        player = Player.Instance;
        momentumUI = MomentumUI.Instance;
    }

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
    public void UpdateMomentum(float speedStat, Player.Direction currentDir) {
        // Always drain speed stat, this value is clamped.
        player.ChangeSpeedStat(-speedLossMult * Time.fixedDeltaTime);

        //if (buffTimer > 0) buffTimer--;

        bool hasSpeedStat = speedStat > 0f;

        SetBasis(currentDir, hasSpeedStat);
        UpdateStates();

        float momentum = 0.0f;

        if (hasSpeedStat) {
            // Player is moving and not moving backwards
            if (player.currentDir != Player.Direction.None && player.currentDir != Player.Direction.Backward) {

                // Player is sliding on the ground
                if (isSliding && isGrounded) momentum += 4f;

                // Player is going DOWN a slope
                if (onSlopeAngle > 0) momentum += 6f + (onSlopeAngle / 15f);

                // Player is grappling
                if (isGrappling) momentum += 16f;

                // Player is wall jumping or wall running
                if (isWallJumping || isWallRunning) momentum += 2f;
            }

            // Maintain momentum if sliding when landing a jump, otherwise lose momentum
            if (isGrounded && !wasGroundedLastFrame) {
                if (!isSliding && !isGrappling && !isWallRunning) {
                    if (!justGrappled && !justWallRan) {
                        momentum -= 100f;
                    } else {
                        justGrappled = false;
                        justWallRan = false;
                    }
                }
            }

        }
        // Player has no speed stat
        else momentum = -15f;

        // Player is not moving
        if (currentDir == Player.Direction.None) momentum -= 10f;

        // Player is going UP a slope
        if (onSlopeAngle < 0) momentum -= 6f + (onSlopeAngle / 15f);

        // Correct momentum for usage
        momentum /= 100f;

        // Apply speed buff multiplier
        momentum *= speedBuffMultiplier;

        EditSpeedMult(momentum);
        momentumUI.UpdateSpeedBar(speedStat);
        momentumUI.UpdateSpeedMult(speedMult);
        UpdateHighestSpeed();

        if (speedMult > m_temp) {
            if (m_temp2 != 1) {
                m_temp2 = 1;
                momentumUI.UpdateIncDec("/\\");
            }
        } else if (speedMult < m_temp) {
            if (m_temp2 != -1) {
                m_temp2 = -1;
                momentumUI.UpdateIncDec("\\/");
            }
        } else {
            if (m_temp2 != 0) {
                m_temp2 = 0;
                momentumUI.UpdateIncDec("--");
            }
        }
        m_temp = speedMult;
    }
    

    float m_temp = 0f;
    sbyte m_temp2 = 0;

    public void BuffSpeed(float time, float multiplier) {
        float mult = multiplier;
        if (mult <= 1f) mult = 1.5f;

        if (speedBuffMultiplier > 1) StopCoroutine("DisplayBuff");
        
        StartCoroutine(DisplayBuff(time, mult));
    }

    private IEnumerator DisplayBuff(float time, float multiplier) {
        speedBuffMultiplier = multiplier;
        MomentumUI.Instance.ToggleBuffOverlay(true, multiplier);
        yield return new WaitForSeconds(time);
        MomentumUI.Instance.ToggleBuffOverlay(false, multiplier);
        speedBuffMultiplier = 1f;
    }

    private void SetBasis(Player.Direction currentDir, bool hasSpeedStat) {
        float currBasis = 1.0f;
        switch (currentDir) {
            case Player.Direction.Forward: currBasis = hasSpeedStat ? 1.2f : 1.0f; break;
            case Player.Direction.Left:
            case Player.Direction.Right: currBasis = hasSpeedStat ? 1.0f : 0.8f; break;
            case Player.Direction.Backward: currBasis = hasSpeedStat ? 0.8f : 0.6f; break;
            default: currBasis = 1.0f; break;
        }
        if (player.isSliding) currBasis = hasSpeedStat ? 1.4f : 0.5f;
        if (player.IsWallRunning()) currBasis = 1.5f;

        float timeMult = hasSpeedStat ? 15 : 5;

        speedBasis = Mathf.Lerp(speedBasis, currBasis, Time.deltaTime * timeMult);

    }

    private void UpdateStates() {
        isGrappling = player.IsGrappling();
        if (isGrappling) justGrappled = true;
        isWallRunning = player.IsWallRunning();
        if (isWallRunning) justWallRan = true;
        wasGroundedLastFrame = isGrounded;
        isSliding = player.isSliding;
        isGrounded = player.isGrounded;
        onSlopeAngle = player.onSlopeAngle;
        isWallJumping = player.IsWallJumping();
    }

    public void EditSpeedMult(float speed, bool condition = true) {
        if (condition) speedMult += speed * Time.fixedDeltaTime * timeScale;
        if (speedMult < 1.0f) { speedMult = 1.0f; }
    }

    private void UpdateHighestSpeed() { if (speedMult > highestSpeed) highestSpeed = speedMult; }
    public float GetTrueSpeedMult() { return speedMult * speedBasis; }
    public float GetActualSpeedMult() { return speedMult; }
    public float GetHighestSpeed() { return highestSpeed; }
}