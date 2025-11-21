using UnityEngine;
using UnityEngine.UI;

public class GrappleMechanic : MonoBehaviour {
    [Header("Lock-on/Grapple Mechanics")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform lockOnReticle;
    [SerializeField] private LayerMask obstructionMask;   // enemy | walls | ground
    [SerializeField] private LayerMask enemyLayer;        // ONLY the enemy hitbox colliders
    [SerializeField] private float detectRange = 20f;
    [SerializeField] private float grappleRange = 10f;
    [SerializeField] private float grappleSpeed = 30f;

    [Tooltip("Half-angle of the lock-on cone in degrees.")]
    [SerializeField] private float fovDegrees = 60f;

    private readonly Collider[] _enemyBuf = new Collider[16];

    private Enemy currentTarget = null;

    private Player player;
    private RawImage reticle;
    private Vector3 lockOnOffset = new Vector3(0, 0.1f, 0);
    private Vector3 grappleDirection;
    private Vector3 grappleTarget;
    private float grappleArrivalDistance = 1.0f;
    private bool inGrappleRange = false;
    private bool isGrappling = false;

    private bool hasSpeedStat = false;

    public void InitGrappleMechanic(Player player) {
        this.player = player;
        reticle = lockOnReticle.GetComponent<RawImage>();
    }

    public bool UpdateGrapple(bool hasSpeed) {
        hasSpeedStat = hasSpeed;
        if (!IsGrappling()) return false;

        Vector3 toTarget = GetGrappleTarget() - transform.position;
        float dist = toTarget.magnitude;

        player.SetLinearVelocity(GetGrappleDirection() * GetGrappleSpeed());

        if (dist < GetGrappleArrivalDistance()) {
            SetIsGrappling(false);
            player.Attack();
        }
        return true;
    }

    public bool Grapple(bool isGrounded, Vector3 position) {
        if (isGrounded || !hasSpeedStat || !inGrappleRange || currentTarget == null) return false;

        isGrappling = true;
        grappleTarget = currentTarget.transform.position + lockOnOffset;
        grappleDirection = (grappleTarget - transform.position).normalized;
        if (currentTarget != null) currentTarget.AllowClipping(true);
        return true;
    }

    public void UpdateLockOnReticle(bool isGrounded, Transform cameraTransform) {
        if (HandleGroundedState(isGrounded))
            return;

        Vector3 origin = cameraTransform.position;
        Vector3 forward = cameraTransform.forward;
        float cosThreshold = Mathf.Cos(fovDegrees * Mathf.Deg2Rad);

        Enemy best = FindBestTarget(origin, forward, cosThreshold);

        if (best != null) {
            currentTarget = best;
        } else {
            ValidateExistingTarget(origin, forward, cosThreshold);
        }

        UpdateReticleUI();
    }

    // -----------------------------
    // --- private helpers below ---
    // -----------------------------

    private bool HandleGroundedState(bool isGrounded) {
        if (!isGrounded) return false;
        currentTarget = null;
        lockOnReticle.gameObject.SetActive(false);
        return true;
    }

    private Enemy FindBestTarget(Vector3 origin, Vector3 forward, float cosThreshold) {
        float sphereRadius = detectRange * 0.5f;
        Vector3 sphereCenter = origin + forward * sphereRadius;
        int count = Physics.OverlapSphereNonAlloc(
            sphereCenter, sphereRadius, _enemyBuf, enemyLayer, QueryTriggerInteraction.Ignore);

        Enemy best = null;
        float bestDot = -1f;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < count; i++) {
            Collider col = _enemyBuf[i];
            if (!col) continue;

            Enemy enemy = col.GetComponent<Enemy>();
            if (!enemy || enemy.IsDead()) continue;

            Vector3 toEnemy = col.transform.position - origin;
            float distance = toEnemy.magnitude;
            if (distance > detectRange || distance <= 0.0001f) continue;

            Vector3 dir = toEnemy / distance;
            float dot = Vector3.Dot(forward, dir);
            if (dot < cosThreshold) continue;

            if (!HasLineOfSight(origin, dir, distance, col)) continue;

            if (dot > bestDot || (Mathf.Approximately(dot, bestDot) && distance < bestDistance)) {
                best = enemy;
                bestDot = dot;
                bestDistance = distance;
            }
        }

        return best;
    }

    private void ValidateExistingTarget(Vector3 origin, Vector3 forward, float cosThreshold) {
        if (currentTarget == null) return;

        Collider targetCol = currentTarget.GetComponent<Collider>();
        if (!targetCol) {
            currentTarget = null;
            return;
        }

        Vector3 toTarget = targetCol.transform.position - origin;
        float distance = toTarget.magnitude;

        if (distance > detectRange || distance <= 0.0001f) {
            currentTarget = null;
            return;
        }

        Vector3 dir = toTarget / distance;
        float dot = Vector3.Dot(forward, dir);
        bool visible = dot >= cosThreshold && HasLineOfSight(origin, dir, distance, targetCol);
        if (!visible) currentTarget = null;
    }

    private void UpdateReticleUI() {
        if (currentTarget == null) {
            lockOnReticle.gameObject.SetActive(false);
            return;
        }

        Vector3 targetWorld = currentTarget.transform.position + lockOnOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetWorld);

        if (screenPos.z <= 0f) {
            lockOnReticle.gameObject.SetActive(false);
            return;
        }

        lockOnReticle.gameObject.SetActive(true);
        lockOnReticle.position = screenPos;

        inGrappleRange = Vector3.Distance(transform.position, currentTarget.transform.position) <= grappleRange;
        reticle.color = (inGrappleRange && hasSpeedStat) ? Color.red : Color.white;
    }

    private bool HasLineOfSight(Vector3 origin, Vector3 dir, float distance, Collider expectedHitbox) {
        if (Physics.Raycast(origin, dir, out RaycastHit hit, distance, obstructionMask, QueryTriggerInteraction.Ignore)) {
            return hit.collider == expectedHitbox;
        }
        return true;
    }

    public bool IsGrappling() => isGrappling;
    public void SetIsGrappling(bool val) => isGrappling = val;
    public Vector3 GetGrappleTarget() => grappleTarget;
    public Vector3 GetGrappleDirection() => grappleDirection;
    public float GetGrappleSpeed() => grappleSpeed;
    public float GetGrappleArrivalDistance() => grappleArrivalDistance;
    public bool HasTarget() => currentTarget != null;
}
