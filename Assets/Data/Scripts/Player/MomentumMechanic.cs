using System;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MomentumMechanic : MonoBehaviour {
    [Header("UI Refs")]
    [SerializeField] private TextMeshProUGUI speedMultDisplay;
    [Header("Momentum Settings")]
    [Tooltip("How Fast the player loses SpeedStat")]
    [SerializeField] private float speedLossMult = 2f;
    [Tooltip("Multiply deltaTime by this value.")]
    [SerializeField] private float timeScale = 1f;
    private Player player;

    private float speedMult = 1.0f;
    private float speedBasis = 1.0f;

    //private bool wasSlidingLastFrame = false;
    private bool wasGroundedLastFrame = false;

    private bool isSliding = false;
    private bool isGrounded = false;
    private bool isGrappling = false;
    private bool justGrappled = false;
    private bool justWallRan = false;
    private bool isOnSlope = false;
    private bool isWallRunning;


    /// <summary>
    /// Call when the player script starts.
    /// </summary>
    /// <param name="speedStat"></param>
    internal void SetPlayerRef(Player player) {
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
    internal void UpdateMomentum(float speedStat, Player.Direction currentDir) {
        // Always drain speed stat, this value is clamped.
        player.ChangeSpeedStat(-speedLossMult * Time.fixedDeltaTime);

        bool hasSpeedStat = speedStat > 0f;

        SetBasis(currentDir, hasSpeedStat);
        UpdateStates();

        float momentum = 0.0f;



        if (hasSpeedStat) {

            if (isSliding && isGrounded) {
                momentum += 0.05f;
            }

            if (isOnSlope || isGrappling) {
                momentum += 0.05f;
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

        EditSpeedMult(momentum);
        UpdateUI(speedStat);
    }

    private void SetBasis(Player.Direction currentDir, bool hasSpeedStat) {
        switch (currentDir) {
            case Player.Direction.Forward: speedBasis = hasSpeedStat ? 1.2f : 1.0f; break;
            case Player.Direction.Left:
            case Player.Direction.Right: speedBasis = hasSpeedStat ? 1.0f : 0.8f; break;
            case Player.Direction.Backward: speedBasis = hasSpeedStat ? 0.8f : 0.6f; break;
            default: speedBasis = 1.0f; break;
        }
        if (player.IsSliding()) speedBasis = hasSpeedStat ? 1.4f : 0.5f;
        if (player.IsWallRunning()) speedBasis = 1.5f;
    }

    private void UpdateUI(float speedStat) {
        speedMultDisplay.text = "Momentum: " + GetSpeedMult().ToString("F3");
        player.UpdateSpeedBar(speedStat);
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
    }

    public void EditSpeedMult(float speed, bool condition = true) {
        if (condition) speedMult += speed * Time.fixedDeltaTime * timeScale;
        if (speedMult < 1.0f) { speedMult = 1.0f; }
    }

    public float GetSpeedMult() { return speedMult * speedBasis; }
}