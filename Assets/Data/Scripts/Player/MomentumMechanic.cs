using System.Collections;
using UnityEngine;

public class MomentumMechanic : MonoBehaviour {
    [Header("Momentum Settings")]
    [Tooltip("How Fast the player loses Focus.")]
    [SerializeField] private float focusLossMult = 2f;
    [Tooltip("Multiply fixedDeltaTime by this value.")]
    [SerializeField] private float timeScale = 1f;

    private float speedBuffMultiplier = 1f;
    /// <summary>Displayed Momentum</summary>
    private float rawMomentum = 1.0f;
    /// <summary>Directional based momentum multiplier</summary>
    private float basis = 1.0f;
    /// <summary>Highest (displayed) momentum.</summary>
    private float highestMomentum = 0f;

    private bool wasGroundedLastFrame = false;

    private bool isSliding = false;
    private bool isGrounded = false;
    private bool isGrappling = false;
    private bool justGrappled = false;
    private bool justWallRan = false;
    private bool isWallRunning = false;
    private bool isWallJumping = false;

    private float onSlopeAngle = 0f;

    private bool pendingRunOnce = true;

    public void SetDefaultValues() {
        speedBuffMultiplier = 1f;
        rawMomentum = 1.0f;
        basis = 1.0f;
        highestMomentum = 0f;
        wasGroundedLastFrame = false;
        isSliding = false;
        isGrounded = false;
        isGrappling = false;
        justGrappled = false;
        justWallRan = false;
        isWallRunning = false;
        isWallJumping = false;
        pendingRunOnce = true;
    }

    public void UpdateMomentum(float focus, Player.Direction currentDir) {
        if (pendingRunOnce) {
            pendingRunOnce = false;
            StatusUI.Instance.UpdateFocusBar(focus, instant: true);
        }
        // Always drain speed stat, this value is clamped.
        Player.Instance.ChangeFocus(-focusLossMult * Time.fixedDeltaTime);

        bool hasFocus = focus > 0f;

        basis = GetBasis(currentDir, hasFocus);
        UpdateStates();

        float momentum = 0.0f;

        if (hasFocus) {
            // Player is moving and not moving backwards
            if (Player.Instance.currentDir != Player.Direction.None && Player.Instance.currentDir != Player.Direction.Backward) {

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
                        print("Slide on landing to maintain momentum!");
                        momentum = 0f;
                    } else {
                        justGrappled = false;
                        justWallRan = false;
                    }
                }
            }

        }
        // Player has no focus
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
        StatusUI.Instance.UpdateFocusBar(focus);
        StatusUI.Instance.UpdateMomentumUI(rawMomentum);
        UpdateHighestSpeed();

        StatusUI.Instance.UpdateCrosshair(rawMomentum);
    }
    public void BuffSpeed(float time, float multiplier) {
        float mult = multiplier;
        if (mult <= 1f) mult = 1.5f;

        if (speedBuffMultiplier > 1) StopCoroutine("DisplayBuff");
        
        StartCoroutine(DisplayBuff(time, mult));
    }

    private IEnumerator DisplayBuff(float time, float multiplier) {
        speedBuffMultiplier = multiplier;
        StatusUI.Instance.SetMomentumUpOverlayActive(true, multiplier);
        yield return new WaitForSeconds(time);
        StatusUI.Instance.SetMomentumUpOverlayActive(false, multiplier);
        speedBuffMultiplier = 1f;
    }

    private float GetBasis(Player.Direction currentDir, bool hasFocus) {
        float currBasis = 1.0f;
        switch (currentDir) {
            case Player.Direction.Forward: currBasis = hasFocus ? 1.2f : 1.0f; break;
            case Player.Direction.Left:
            case Player.Direction.Right: currBasis = hasFocus ? 1.0f : 0.8f; break;
            case Player.Direction.Backward: currBasis = hasFocus ? 0.8f : 0.6f; break;
            default: currBasis = 1.0f; break;
        }
        if (Player.Instance.IsSliding) currBasis = hasFocus ? Average(currBasis, 1.5f) : Average(currBasis, 0.25f);
        if (Player.Instance.IsWallRunning) currBasis = Average(currBasis, 1.75f);
        if (Player.Instance.IsWallJumping) currBasis = Average(currBasis, 1.5f);
        if (Player.Instance.IsGrappling) currBasis = Average(currBasis, 2.0f);

        float timeMult = hasFocus ? 15 : 5;
        
        return Mathf.Lerp(basis, currBasis, Time.fixedDeltaTime * timeMult);

    }

    private float Average(float a, float b) => (a + b) / 2;
    

    private void UpdateStates() {
        isGrappling = Player.Instance.IsGrappling;
        if (isGrappling) justGrappled = true;
        isWallRunning = Player.Instance.IsWallRunning;
        if (isWallRunning) justWallRan = true;
        wasGroundedLastFrame = isGrounded;
        isSliding = Player.Instance.IsSliding;
        isGrounded = Player.Instance.IsGrounded;
        onSlopeAngle = Player.Instance.OnSlopeAngle;
        isWallJumping = Player.Instance.IsWallJumping;
    }

    public void EditSpeedMult(float speed, bool condition = true) {
        if (condition) rawMomentum += speed * Time.fixedDeltaTime * timeScale;
        if (rawMomentum < 1.0f) { rawMomentum = 1.0f; }
    }

    private void UpdateHighestSpeed() { if (rawMomentum > highestMomentum) highestMomentum = rawMomentum; }
    /// <summary>Get the actual momentum multiplier.</summary>
    /// <returns>Speed Mult * Speed Basis.</returns>
    public float GetTrueMomentum() => rawMomentum * basis;
    /// <summary>Get the raw momentum multiplier (displayed).</summary>
    /// <returns>Speed Mult</returns>
    public float GetRawMomentum() => rawMomentum;
    public float GetHighestSpeed() => highestMomentum; 
}