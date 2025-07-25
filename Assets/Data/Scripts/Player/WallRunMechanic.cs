using System;
using UnityEngine;

public class WallRunMechanic : MonoBehaviour {

    [Header("Wall Check and Wall climb Mechanics")]
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.2f;
    [SerializeField] private bool showGizmos = false;

    private bool isOnWallLeft = false;
    private bool isOnWallRight = false;

    internal void UpdatePhysics(bool isGrounded) {
        isOnWallLeft = !isGrounded && Physics.CheckSphere(wallCheckLeft.position, wallCheckRadius, wallLayer);
        isOnWallRight = !isGrounded && Physics.CheckSphere(wallCheckRight.position, wallCheckRadius, wallLayer);
    }

    /// <summary>
    /// Called on jump context if the player is on a wall, instead of the normal player jump.
    /// </summary>
    /// <param name="keyReleased"></param>
    /// <exception cref="NotImplementedException"></exception>
    internal void Jump(bool keyReleased) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get if the player is on a wall.
    /// Use "direction" to get a specific direction, case doesn't matter.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    internal bool IsOnWall(string direction = "") {
        string dir = direction.ToLower();
        if (dir == "left") return isOnWallLeft;
        else if (dir == "right") return isOnWallRight;
        return isOnWallLeft || isOnWallRight;
    }

    private void OnDrawGizmos() {
        if (!showGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(wallCheckLeft.position, wallCheckRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(wallCheckRight.position, wallCheckRadius);
    }
}
