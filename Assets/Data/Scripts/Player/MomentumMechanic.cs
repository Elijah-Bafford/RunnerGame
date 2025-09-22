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
    private bool isOnSlope = false;
    private bool isWallRunning = false;
    private bool isWallJumping = false;

    internal void InitMomentumMechanic(Player player) {
        this.player = player;
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

            if (isSliding && isGrounded) {
                momentum += 0.04f;
            }

            if (isOnSlope || isGrappling) {
                momentum += 0.06f;
            }

            if (isWallJumping || isWallRunning) {
                momentum += 0.02f;
            }

            // Maintain momentum if sliding when landing a jump, otherwise lose momentum
            if (isGrounded && !wasGroundedLastFrame) {
                if (!isSliding && !isGrappling && !isWallRunning) {
                    if (!justGrappled && !justWallRan) {
                        momentum -= 0.3f;
                    } else {
                        justGrappled = false;
                        justWallRan = false;
                    }
                }
            }

        } else if (!hasSpeedStat || currentDir == Player.Direction.None) momentum = -0.07f;


        momentum *= speedBuffMultiplier;


        EditSpeedMult(momentum);
        MomentumUI.GetSelf().UpdateSpeedBar(speedStat);
        UpdateHighestSpeed();
    }

    public void BuffSpeed(int time, float multiplier = 2.0f) {
        if (multiplier <= 1f) {
            multiplier = 1.1f;
            Debug.LogWarning("Cannot apply speed buff with a multiplier of less than or equal to 1. Argument changed to 1.1f");
        }
        if (speedBuffMultiplier > 1) {
            StopCoroutine("DisplayBuff");
        }
        speedBuffMultiplier = multiplier;
        StartCoroutine(DisplayBuff(time));
    }

    private IEnumerator DisplayBuff(float time) {
        MomentumUI.GetSelf().ToggleBuffOverlay(true);
        yield return new WaitForSeconds(time);
        MomentumUI.GetSelf().ToggleBuffOverlay(false);
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
        if (player.IsSliding()) currBasis = hasSpeedStat ? 1.4f : 0.5f;
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
        isSliding = player.IsSliding();
        isGrounded = player.IsGrounded();
        isOnSlope = player.IsOnSlope();
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