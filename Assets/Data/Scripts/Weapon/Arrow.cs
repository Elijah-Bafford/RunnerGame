using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour {

    [SerializeField, Range(1f, 100f)] private float speed = 10f;
    [SerializeField, Range(1f, 25f)] private float lifetime = 10f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject pLight;

    private bool checkTrigger = true;


    private void Start() {
        Vector3 toPlayer = Player.Instance.transform.position - transform.position;

        if (toPlayer.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(toPlayer.normalized);

        // Now shoot the arrow along its forward direction
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other) {
        if (!checkTrigger) return;

        if (other.name == "AttackBox") {
            rb.linearVelocity = speed * Player.Instance.transform.forward;
            rb.useGravity = true;
            Destroy(gameObject, 1f);
            return;
        }

        switch (other.tag) {
            case "Wall":
            case "Tunnel":
            case "Ground":
            case "SlopedGround":
                rb.linearVelocity = Vector3.zero;
                checkTrigger = false;
                if (pLight != null) pLight.SetActive(false);
                break;
            case "Player":
                Player.Instance.SetDead(true);
                Destroy(gameObject); break;
            default: return;
        }

    }
}
