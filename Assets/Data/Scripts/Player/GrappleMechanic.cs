using System;
using UnityEngine;
using UnityEngine.UI;

public class GrappleMechanic : MonoBehaviour {
    [Header("Lock-on/Grapple Mechanics")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform lockOnReticle;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float detectRange = 20f;
    [SerializeField] private float grappleRange = 10f;
    [SerializeField] private float grappleSpeed = 30f;

    private Enemy lockOnTarget = null;
    private Enemy lastLockTarget = null;
    private RawImage reticle;
    private Vector3 lockOnOffset = new Vector3(0, 0.2f, 0);
    private Vector3 grappleDirection;
    private Vector3 grappleTarget;
    private float grappleArrivalDistance = 1.0f;
    private bool inGrappleRange = false;
    private bool isGrappling = false;

    private void Awake() { reticle = lockOnReticle.GetComponent<RawImage>(); }

    public void Grapple(bool isGrounded, Vector3 position) {
        if (isGrounded || !inGrappleRange || lastLockTarget == null) return;

        // Set up the grapple state
        isGrappling = true;
        grappleTarget = lastLockTarget.transform.position + lockOnOffset;
        grappleDirection = (grappleTarget - transform.position).normalized;
    }

    public void UpdateLockOnReticle(bool isGrounded, Transform cameraTransform) {
        if (isGrounded) {
            lockOnTarget = null;
            lastLockTarget = null;
            lockOnReticle.gameObject.SetActive(false);
            return;
        }

        // Find all colliders in a sphere in front of the camera
        Vector3 origin = cameraTransform.position;
        Vector3 direction = cameraTransform.forward;
        Collider[] hits = Physics.OverlapSphere(origin + direction * (detectRange * 0.5f), detectRange * 0.5f, enemyLayer);

        Enemy bestTarget = null;
        float bestDot = -1f; // Closest to 1 is most centered
        float bestDistance = float.MaxValue;

        foreach (var hit in hits) {
            if (!hit.CompareTag("Enemy")) continue;
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy == null || enemy.IsDead()) continue;

            Vector3 toEnemy = (enemy.transform.position - origin).normalized;
            float dot = Vector3.Dot(direction, toEnemy);
            float distance = Vector3.Distance(origin, enemy.transform.position);

            // Don't allow grapple through walls
            RaycastHit rayHit;
            if (Physics.Raycast(origin, toEnemy.normalized, out rayHit, distance, enemyLayer | wallLayer))
                if (!rayHit.collider.GetComponentInParent<Enemy>()) continue;

            // Only consider those within a certain angle of the camera center (e.g., 60 degrees)
            if (dot > 0.5f && distance <= detectRange) {
                // Pick the most centered, break ties by closer distance
                if (dot > bestDot || (Mathf.Approximately(dot, bestDot) && distance < bestDistance)) {
                    bestTarget = enemy;
                    bestDot = dot;
                    bestDistance = distance;
                }
            }
        }

        if (bestTarget != null) {
            lockOnTarget = bestTarget;
            lastLockTarget = bestTarget;
        } else if (lastLockTarget != null) {
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

    public bool IsGrappling() { return isGrappling; }
    public void SetIsGrappling(bool isGrappling) { this.isGrappling = isGrappling; }
    public Vector3 GetGrappleTarget() { return grappleTarget; }
    public Vector3 GetGrappleDirection() { return grappleDirection; }
    public float GetGrappleSpeed() { return grappleSpeed; }
    public float GetGrappleArrivalDistance() { return grappleArrivalDistance; }
}